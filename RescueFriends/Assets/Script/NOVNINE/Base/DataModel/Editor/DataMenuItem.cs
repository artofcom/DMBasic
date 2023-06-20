using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using NOVNINE;
using NOVNINE.Diagnostics;
using JsonFx.Json;
using ProtoBuf;
using ProtoBuf.Meta;

public static class DataMenuItem
{
#if (UNITY_3_4 || UNITY_3_5)
    static string MCS ="/Applications/Unity/Unity.app/Contents/Frameworks/MonoBleedingEdge/bin/smcs";
	static string MONO ="/Applications/Unity/Unity.app/Contents/Frameworks/MonoBleedingEdge/bin/cli";
	static string UnityEnginePath = "/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll";
#elif UNITY_5_4_OR_NEWER
	static string MCS ="/Applications/Unity/Unity.app/Contents/MonoBleedingEdge/bin/mcs";
	static string MONO ="/Applications/Unity/Unity.app/Contents/MonoBleedingEdge/bin/cli";
	static string UnityEnginePath = "/Applications/Unity/Unity.app/Contents/Managed/UnityEngine.dll";
#else
    // Uniyt 4_x.
    static string MCS ="/Applications/Unity/Unity.app/Contents/Frameworks/MonoBleedingEdge/bin/mcs";
	static string MONO ="/Applications/Unity/Unity.app/Contents/Frameworks/MonoBleedingEdge/bin/cli";
	static string UnityEnginePath = "/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll";
#endif

    
    static string JsonFxPath ="../Assets/NOVNINE/ExternalLib/JsonFX/JsonFX.Json.dll";
    static string ProtoBufPath ="../Assets/NOVNINE/ExternalLib/protobuf/protobuf-net.dll";

    [MenuItem("NOVNINE/Data/Compile DLL")]
    public static void Compile()
    {
        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);
        string pwd = root + "data";

        Directory.CreateDirectory(root+"Assets/Data/model");
        Directory.CreateDirectory(root+"Assets/Data/lib");

        //if(!Directory.Exists(pwd)) {
        //    Debug.Log("Data build Path is not existing creating data template");
        bool firstCompile = !Directory.Exists(pwd);

        string zipPath = "Assets/NOVNINE/Base/DataModel/Editor/data.zip";
        ShellRunner.Run("unzip", "-o "+zipPath, root);

        File.Copy(Application.dataPath+"/NOVNINE/Base/DataModel/ContextHolder.cs", pwd+"/ContextHolder.cs", true);

        string[] src = Directory.GetFiles(pwd, "*.cs");
        foreach(string path in src) {
            string fn = Path.GetFileName(path);
            if(fn == "ContextHolder.cs") continue;
            try {
                File.Copy(path, root+"Assets/Data/model/"+fn, fn != "Data.Root.cs");
            } catch (System.Exception ex) {
                Debug.LogWarning(ex.Message);
            }
        }

        if(firstCompile) {
            ShellRunner.Run("open", "Data.Root.cs", root+"Assets/Data/model");
        }

        #if UNITY_5_6
        string[] splitBundleIdentifier = Application.identifier.Split ('.');// NativeInterface.GetBundleIdentifier ().Split ('.');
        #else
        string[] splitBundleIdentifier = { "" };// Application.bundleIdentifier.Split ('.');// NativeInterface.GetBundleIdentifier ().Split ('.');
        #endif

		string dataDLL = splitBundleIdentifier[splitBundleIdentifier.Length-1]+".dll";

        //Build Data DLL
        List<string> parms = new List<string>();
        parms.Add("-target:library");
		parms.Add("-sdk:2");
        parms.Add("-out:precompile/data/"+dataDLL);
        parms.Add("-r:"+UnityEnginePath);
        parms.Add("-r:"+JsonFxPath);
        parms.Add("-r:"+ProtoBufPath);
        parms.Add("ContextHolder.cs");
        //parms.Add("-define:USE_SOURCEDATA");

        string[] sources = Directory.GetFiles(root+"Assets/Data/model", "*.cs");
        foreach(string path in sources) {
            string fn = Path.GetFileName(path);
            try {
                File.Copy(path, pwd+"/"+fn, true);
            } catch (System.Exception ex) {
                Debug.LogWarning(ex.Message);
            }
            parms.Add(fn);
        }
        Debug.Log("Compile "+dataDLL);
        //Debug.Log(string.Join(" ", parms.ToArray()));
        string result = ShellRunner.Run(MCS, string.Join(" ", parms.ToArray()), pwd);
        Debug.Log(result);

        //Build DataBuilder.dll
        parms.Clear();
        parms.Add("precompile.exe");
        parms.Add("data/"+dataDLL);
		parms.Add("-f:/Applications/Unity/MonoDevelop.app/Contents/Frameworks/Mono.framework/Versions/Current/lib/mono/2.0");
		 
		parms.Add("-t:DataBuilder");
		parms.Add("-o:../../Assets/Data/lib/DataBuilder.dll");
        pwd += "/precompile";
        Debug.Log("Compile DataBuilder.dll");
        result = ShellRunner.Run(MONO, string.Join(" ", parms.ToArray()), pwd);
        Debug.Log(result);

        Debug.Log("Copying "+dataDLL);
        File.Copy(pwd+"/data/"+dataDLL, Application.dataPath+"/Data/lib/"+dataDLL, true);
        Debug.Log("Data Compile Done");

        if(File.Exists(Application.dataPath+"/NOVNINE/Base/DataModel/DataContainer.cs_")) 
		{
            File.Delete(Application.dataPath+"/NOVNINE/Base/DataModel/DataContainer.cs");
            File.Move(Application.dataPath+"/NOVNINE/Base/DataModel/DataContainer.cs_",
                      Application.dataPath+"/NOVNINE/Base/DataModel/DataContainer.cs");
        }

        if(File.Exists(Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataMenuItem2.cs_")) 
		{
            File.Delete(Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataMenuItem2.cs");
            File.Move(Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataMenuItem2.cs_",
                      Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataMenuItem2.cs");
        }

        if(File.Exists(Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataSelectWindow.cs_")) 
		{
            if(!Directory.Exists(Application.dataPath+"/Data/Editor")) 
                Directory.CreateDirectory(Application.dataPath+"/Data/Editor");

            File.Delete(Application.dataPath+"/Data/Editor/DataSelectWindow.cs");
            File.Copy(Application.dataPath+"/NOVNINE/Base/DataModel/Editor/DataSelectWindow.cs_",
                    Application.dataPath+"/Data/Editor/DataSelectWindow.cs");
        }

        if(!File.Exists(Application.dataPath+"/Data/Root.cs")) 
		{
            string content = "public class Root : DataContainer<Data.Root> {}";
            File.WriteAllText(Application.dataPath+"/Data/Root.cs", content);
            Debug.Log("Creating Root.cs file");
        }
		
		BuildTargetGroup[] list = new BuildTargetGroup[]{ BuildTargetGroup.Android, BuildTargetGroup.iOS };
		//if(ScriptDefine.HasDefine("USE_DLLDATACLASS", list) == false)
		//	ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, list);
		
        AssetDatabase.Refresh();
    }

    [MenuItem("NOVNINE/Data/Use/C# Data", true, 3101)]
    public static bool IsUseProtoBuf()
    {
        //return File.Exists(Application.dataPath+"/smcs.rsp");
        return false;
    }

    [MenuItem("NOVNINE/Data/Use/C# Data", false, 3101)]
    public static void SetUseJsonFX()
    {
        //File.Delete(Application.dataPath+"/smcs.rsp");
        //File.Delete(Application.dataPath+"/gmcs.rsp");
        /*
		ScriptDefine.Remove(new string[] {"USE_DLLDATACLASS"}, new BuildTargetGroup[]{BuildTargetGroup.Android,BuildTargetGroup.iOS});
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Data/Root.cs", ImportAssetOptions.ForceUpdate);
        */
    }

    [MenuItem("NOVNINE/Data/Use/DLL Data", true, 3102)]
    public static bool IsUseJsonFX()
    {
        return true;
    }

    [MenuItem("NOVNINE/Data/Use/DLL Data", false, 3102)]
    public static void SetUseProtoBuf()
    {
        //File.WriteAllText(Application.dataPath+"/smcs.rsp", "-define:USE_DLLDATACLASS");
        //File.WriteAllText(Application.dataPath+"/gmcs.rsp", "-define:USE_DLLDATACLASS");
		/*ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, new BuildTargetGroup[]{BuildTargetGroup.Android,BuildTargetGroup.iOS});
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Data/Root.cs", ImportAssetOptions.ForceUpdate);
        */
    }

    [MenuItem("NOVNINE/Data/ResetPlayerPrefs", false, 3105)]
    static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        //Root.SetPostfix("A");
        //PlayerPrefs.SetString("DataSet", "A");
    }

    public static void deleteDirs(DirectoryInfo dirs)
    {
        if (dirs == null || !dirs.Exists)
            return;
        
        DirectoryInfo[] subDir = dirs.GetDirectories();
        if(subDir!=null)
        {
            for(int i=0;i<subDir.Length;i++)
            {
                if(subDir[i]!=null)
                {
                    deleteDirs(subDir[i]);
                }
            }
            subDir=null;
        }

        FileInfo[] files=dirs.GetFiles();
        if(files!=null)
        {
            for(int i=0;i<files.Length;i++)
            {
                if(files[i]!=null)
                {
                    Debug.Log("파일 삭제:"+files[i].FullName+"__over");
                    files[i].Delete();
                    files[i]=null;
                }
            }
            files=null;
        }

        Debug.Log("폴더 삭제:"+dirs.FullName+"__over");
        dirs.Delete();
    }

   
    /* 
	[MenuItem("NOVNINE/Data/Json -> Json", true, 3112)]
    public static bool IsRefreshJson() {
		return !DataMenuItem.IsUseJsonFX();
	}
	*/	

    [MenuItem("NOVNINE/Data/Json -> Json", false, 3112)]
    public static void RefreshJson() {
        System.AppDomain app = System.AppDomain.CurrentDomain;
        Assembly[] ass = app.GetAssemblies();

        foreach(Assembly assembly in ass) {
            var types = assembly.GetTypes().Where(t => 
                t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(DataContainer<>));
            foreach(var subType in types) {
                MethodInfo loadMethod = subType.BaseType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
                object ret = loadMethod.Invoke(null, null);
                Debugger.Assert(ret != null, "Root.Data.txt Json Load Fail, maybe parsing error?");
                MethodInfo saveMethod = subType.BaseType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static);
                saveMethod.Invoke(null, new object[]{ret});
            }
        }
    }
}

