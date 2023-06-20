/**
* @file ModuleManager.cs
* @brief
* @author Choi YongWu(amugana@bitmango.com)
* @version 1.1
* @date 2013-09-14
*/

#define SyncModuleDesc

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using NOVNINE;
using NOVNINE.Diagnostics;

public class ModuleManager : EditorWindow
{
    static string PLATFORM_REPO = "http://ota.bitmango/";
    static string PLATFORM_REPO_WRITE = "file://"+System.Environment.GetEnvironmentVariable("HOME")+"/Projects/bitmango/jenkinsRepo/";

    static string MODULE_DESC_PATH = Application.dataPath+"/ModuleDesc.txt";
    //"file://"+System.Environment.GetEnvironmentVariable("HOME")+"/Projects/bitmango/jenkinsRepo/";
    //static string PLATFORM_REPO_WRITE = System.Environment.GetEnvironmentVariable("HOME")+"/Projects/bitmango/jenkinsRepo/";

    static ModuleDescription _currentDesc;
    static ModuleDescription _latestDesc;

    public static ModuleDescription currentDesc
    {
        get {
            if(_currentDesc == null) {
                _currentDesc = ModuleDescription.Load(MODULE_DESC_PATH);
            }
            return _currentDesc;
        }
    }

    public static ModuleDescription latestDesc
    {
        get {
            if(_latestDesc == null) {
                _latestDesc = ModuleDescription.Load(PLATFORM_REPO+"ModuleDesc.txt");
#if SyncModuleDesc
				if(SyncDesc(currentDesc, _latestDesc)) {
					ModuleDescription.Save(MODULE_DESC_PATH, currentDesc);
					ResetScriptDefines();
					//GenerateModuleSource(currentDesc);
				}
#endif
            }
            return _latestDesc;
        }
    }

    static Vector2 scrollPos = Vector2.zero;
    static bool[] categoryFold;
    static Dictionary<string,bool> selection;

    static float progress;
    static bool compiling;

    static void Error(string message, string title="Error")
    {
        Debug.LogError(message);
        EditorUtility.DisplayDialog(title, message, "OK");
    }

	static bool SyncDesc(ModuleDescription from, ModuleDescription to) 
	{
		if(from == null || to == null) return false;
		bool changed = false;
		var mlist = from.GetAllModules();
        for(int i=0; i<mlist.Count; ++i) {
			var m = mlist[i];
			if(to.GetModuleByID(m.id) == null) {
				from.RemoveModule(m.id);
				mlist = from.GetAllModules();
				changed = true;
				i=0;
			}
		}
		return changed;
	}

    public static string DownloadText(string path)
    {
        WWW www = new WWW(path);
        while(!www.isDone);
        if(!string.IsNullOrEmpty(www.error)) {
            Error(www.error, "Download Fail : "+path);
            return null;
        }
        return www.text;
    }

    public static byte[] DownloadBin(string path)
    {
        WWW www = new WWW(path);
        while(!www.isDone && string.IsNullOrEmpty(www.error));
        if(!string.IsNullOrEmpty(www.error)) {
            Error(www.error, "Download Fail : "+path);
            return null;
        }
        return www.bytes;
    }

    static void SortByDependency(List<ModuleDescription.Mod> modlist)
    {
        modlist.Sort(delegate(ModuleDescription.Mod m1, ModuleDescription.Mod m2) {
            if(m1.DependsOn(m2)) return 1;
            if(m2.DependsOn(m1)) return -1;
            return 0;
        });
    }

    enum ModuleStatus {
        Installed,
        NotInstalled,
        UpdateAvailable
    }

    static ModuleStatus GetModuleStatus(string modId)
    {
        ModuleDescription.Mod cur = currentDesc.GetModuleByID(modId);
        ModuleDescription.Mod latest = latestDesc.GetModuleByID(modId);
        if(cur != null) {
            if(latest != null && cur.version == latest.version)
                return ModuleStatus.Installed;
            return ModuleStatus.UpdateAvailable;
        }
        return ModuleStatus.NotInstalled;
    }

    public static bool IsModuleInstalled(string modId) 
    {
        ModuleDescription.Mod cur = currentDesc.GetModuleByID(modId);
        return (cur != null);
    }

    static string GetModuleStatusString(string modId)
    {
        ModuleStatus status = GetModuleStatus(modId);
        switch(status) {
        case ModuleStatus.Installed:
        case ModuleStatus.NotInstalled:
        default:
            return status.ToString();
        case ModuleStatus.UpdateAvailable:
            ModuleDescription.Mod latest = latestDesc.GetModuleByID(modId);
            return status.ToString()+": "+latest.version;
        }
    }

    #region static Funcs
    static void BuildModule(string modId)
    {
        //if(PLATFORM_REPO.StartsWith("http://")) return;

        ModuleDescription.Mod cur = currentDesc.GetModuleByID(modId);
        if(cur == null) return;

        //if(cur.Version == latest.version) return;

        cur.version = cur.Version;

        if(cur.excludes != null) {
            foreach(var fn in cur.excludes)
                AssetDatabase.DeleteAsset(fn);
        }

        string outputName = PLATFORM_REPO_WRITE+cur.PackageName;
        outputName = outputName.Replace("file://", "");
        Debug.Log("BuildModule : "+modId + " to "+outputName+"\nFiles:\n"+string.Join("\n", cur.files));
        AssetDatabase.ExportPackage(cur.files, outputName, ExportPackageOptions.Recurse);

        latestDesc.UpdateModule(cur);

        ModuleDescription.Save(MODULE_DESC_PATH, currentDesc);
        ModuleDescription.Save(PLATFORM_REPO_WRITE+"ModuleDesc.txt", latestDesc);
    }

    static void UpdateModule(string modId)
    {
        Debug.Log("UpdateModule : "+modId);

        ModuleDescription.Mod cur = currentDesc.GetModuleByID(modId);
        ModuleDescription.Mod latest = latestDesc.GetModuleByID(modId);
        if(!Enumerable.SequenceEqual(cur.files, latest.files))
            UninstallModule(modId);
        InstallModule(modId);
    }

    static void InstallModule(string modId)
    {
        Debug.Log("InstallModule : "+modId);
        //EditorPrefsList.Add("pending_install", modId);
        //WaitCompile();
        InstallModuleInternal(modId);
    }

    static bool InstallModuleInternal(string modId)
    {
        Debug.Log("InstallModuleInternal : "+modId);
        ModuleDescription.Mod desc = latestDesc.GetModuleByID(modId);

        //string packName = desc.PackageName;
        //byte[] downloaded = DownloadBin(PLATFORM_REPO+packName);
        //System.IO.File.WriteAllBytes(Application.temporaryCachePath+"/"+packName, downloaded);
        //AssetDatabase.ImportPackage(Application.temporaryCachePath+"/"+packName, false);

        if(currentDesc.GetModuleByID(desc.id) == null)
            currentDesc.AddModuleAtCategory(desc.category, desc);
        else
            currentDesc.UpdateModule(desc);
        ModuleDescription.Save(MODULE_DESC_PATH, currentDesc);
        //GenerateModuleSource(latestDesc);
        ScriptDefine.Add(new string[] {"USE_"+desc.id}, desc.platforms);
        //WaitCompile();
        return true;
    }

    public static bool UninstallModule(string modId)
    {
        Debug.Log("UninstallModule : "+modId);

        ModuleDescription.Mod desc = currentDesc.GetModuleByID(modId);
        /*
        foreach(var fn in desc.files) {
            AssetDatabase.DeleteAsset(fn);
        }
        */
        ScriptDefine.Remove(new string[] {"USE_"+desc.id}, desc.platforms);
        currentDesc.RemoveModule(modId);

        ModuleDescription.Save(MODULE_DESC_PATH, currentDesc);
        //GenerateModuleSource(latestDesc);
        //WaitCompile();
        ResetSelection();
        return true;
    }
    #endregion

    static ModuleDescription BuildInstalledModuleDescriptionFromScratch(ModuleDescription full)
    {
        ModuleDescription result = new ModuleDescription();
        foreach(var entry in full.modules) {
            foreach(var mod in entry.Value) {
                bool missing = false;
                foreach(var f in mod.files) {
                    string path = Application.dataPath+f.Substring(6);
                    if(!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path)) {
                        //Debug.Log("missing : "+path);
                        missing = true;
                        break;
                    }
                }
                if(!missing) {
                    //Debug.Log("exist mod : "+mod.id);
                    var cloned = mod.Clone();
                    cloned.version = "?";
                    result.AddModuleAtCategory(cloned.category, cloned);
                }
            }
        }
        return result;
    }

    #region Module.cs Generator
    public static void GenerateModuleSource(ModuleDescription desc)
    {
        Debug.Log("Generating Context.cs : "+string.Join(", ", desc.GetAllModules().ConvertAll(x=>x.id).ToArray()));

        List<ModuleDescription.Mod> modlist = desc.GetAllModules();
        StringBuilder modBuf = new StringBuilder();

        modBuf.Append("// DO NOT EDIT THIS FILE.\n");
        modBuf.Append("using System.Collections.Generic;\n");
		modBuf.Append("namespace NOVNINE\n");
        modBuf.Append("{\n");

        modBuf.Append("public static class Context\n");
        modBuf.Append("{\n");
        foreach(var mod in modlist) {
			if(GetModuleStatus(mod.id) == ModuleStatus.NotInstalled) continue;
            if(!string.IsNullOrEmpty(mod.contextClass)) {
                modBuf.Append("#if USE_"+mod.id+"\n");
                modBuf.Append("\tpublic static "+mod.contextClass+" "+mod.id+";\n");
                modBuf.Append("#endif\n");
            }
        }
        modBuf.Append("}\n");
        modBuf.Append("}\n");
		File.WriteAllText(Application.dataPath+"/NOVNINE/Base/Module/Context.cs", modBuf.ToString(), System.Text.Encoding.UTF8);
		AssetDatabase.ImportAsset("Assets/NOVNINE/Base/Module/Context.cs");
    }
    #endregion

    #region GUI Codes

    public static void DrawHeader(float[] widthes, object[] objs)
    {
        float win = (float)Screen.width*0.9f;
        GUILayout.BeginHorizontal();
        for (int i=0; i<objs.Length; ++i) {
            object O = objs[i];
            float W = widthes[i] * win;
            if(widthes[i] > 1)
                W = widthes[i];

            if(O == null) {
                GUILayout.Label("", GUILayout.Width(W));
            } else {
                System.Type Otype = O.GetType();
                if(Otype == typeof(string))
                    GUILayout.Label(O.ToString(), GUILayout.Width(W));
                else if(Otype.IsEnum)
                    EditorGUILayout.EnumPopup(O as System.Enum, GUILayout.Width(W));
                else
                    GUILayout.Label(O.ToString(), GUILayout.Width(W));
            }
        }
        GUILayout.EndHorizontal();
    }

    void CheckDependencies(string modId, bool install)
    {
        //Debug.Log("CheckDependencies :"+desc.id+" "+install);
        var desc = latestDesc.GetModuleByID(modId);
        if(install) { //new install or update
            if(desc.dependencies == null) return;
            foreach(var dep in desc.dependencies) {
                //var depDesc = latestDesc.GetModuleByID(dep);
                ModuleStatus status = GetModuleStatus(dep);
                bool flip = false;
                switch(status) {
                case ModuleStatus.Installed:
                    if(selection[dep])
                        flip = true;
                    break;
                case ModuleStatus.NotInstalled:
                    if(!selection[dep])
                        flip = true;
                    break;
                case ModuleStatus.UpdateAvailable:
                    if(!selection[dep])
                        flip = true;
                    break;
                }
                if(flip) {
                    CheckDependencies(dep, true);
                    selection[dep] = !selection[dep];
                }
            }
        } else { // remove
            //List<string> depends = new List<string>();
            foreach(var mod in currentDesc.GetAllModules()) {
                if(mod.id == desc.id) continue;
                if(mod.dependencies == null) continue;
                if(mod.DependsOn(desc)) {
                    if(selection.ContainsKey(mod.id)) {
                        if(selection[mod.id] != mod.ExistOnDisk) {
                            //Debug.Log("check2 "+mod.id);
                            CheckDependencies(mod.id, false);
                            selection[mod.id] = !selection[mod.id];
                        }
                    }
                }
            }
        }
    }

    void DrawModules()
    {
        float[] widthes = new float[] {
            20,
            0.5f,
            0.1f,
            0.1f,
            0.2f,
        };
        DrawHeader(widthes, new object[] {
            "",
            "Name",
            "Version",
            "Platform",
            "Status"
        });

        float win = (float)Screen.width*0.9f;

        if(categoryFold == null)
            ResetSelection();

        int catIdx = 0;
        foreach(var entry in latestDesc.modules) {
            GUI.color = Color.white;
            categoryFold[catIdx] = EditorGUILayout.Foldout(categoryFold[catIdx], entry.Key);
            if(categoryFold[catIdx]) {
                foreach(var desc in entry.Value) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    ModuleStatus status = GetModuleStatus(desc.id);
                    if(status == ModuleStatus.Installed)
                        GUI.color = Color.green;
                    else if(status == ModuleStatus.NotInstalled)
                        GUI.color = Color.white;
                    else if(status == ModuleStatus.UpdateAvailable)
                        GUI.color = Color.red;
                    bool newChk = EditorGUILayout.Toggle(selection[desc.id], GUILayout.Width(10));
                    if(newChk != selection[desc.id]) {
                        bool install = false;//= !Module.Exists(desc.id);
                        bool skip = false;

                        switch(status) {
                        case ModuleStatus.Installed:
                            install = !newChk;
                            break;
                        case ModuleStatus.NotInstalled:
                            install = newChk;
                            break;
                        case ModuleStatus.UpdateAvailable:
                            if(newChk)
                                install = newChk;
                            else
                                skip = true;
                            break;
                        }
                        if(!skip) {
                            CheckDependencies(desc.id, install);
                        }
                        selection[desc.id] = newChk;
                    }
                    //GUILayout.Label(desc.id, GUILayout.Width(win*0.25f));
                    GUILayout.Label(desc.name, GUILayout.Width(win*0.5f));
                    var cur = currentDesc.GetModuleByID(desc.id);
                    if(cur != null)
                        GUILayout.Label(cur.version, GUILayout.Width(win*0.1f));
                    else
                        GUILayout.Label("---", GUILayout.Width(win*0.1f));

                    if(desc.platforms == null)
                        GUILayout.Label("All", GUILayout.Width(win*0.1f));
                    else
                        GUILayout.Label(string.Join(",",System.Array.ConvertAll(desc.platforms, p=>p.ToString())), GUILayout.Width(win*0.1f));
                    GUILayout.Label(GetModuleStatusString(desc.id), GUILayout.Width(win*0.2f));
                    GUILayout.EndHorizontal();
                }
            }
            catIdx++;
        }
        GUI.color = Color.white;

        ModuleDescription.Mod[] selList = latestDesc.GetAllModules().Where(m=> selection[m.id]).ToArray();
        List<ModuleDescription.Mod> installList = new List<ModuleDescription.Mod>();
        List<ModuleDescription.Mod> uninstallList = new List<ModuleDescription.Mod>();
        foreach(var m in selList) {
            ModuleStatus status = GetModuleStatus(m.id);

            switch(status) {
            case ModuleStatus.Installed:
                uninstallList.Add(m);
                break;
            case ModuleStatus.NotInstalled:
                installList.Add(m);
                break;
            case ModuleStatus.UpdateAvailable:
                uninstallList.Add(m);
                installList.Add(m);
                break;
            }
        }

        if(GUILayout.Button("Install "+ installList.Count +" modules")) {
            SortByDependency(installList);

            foreach(var m in installList) {
                ModuleStatus status = GetModuleStatus(m.id);
                if(status == ModuleStatus.UpdateAvailable)
                    UpdateModule(m.id);
                else
                    InstallModule(m.id);
            }
        }

        if(GUILayout.Button("Delete "+ uninstallList.Count +" modules")) {
            foreach(var m in uninstallList)
                if(!UninstallModule(m.id)) {
                    return;
                }
        }

        /*
                if(GUILayout.Button("Build "+ selList.Length +" modules")){
                    foreach(var m in selList)
                        BuildModule(m.id);
                }
        */
        if(GUILayout.Button("Reset ScriptDefineSymbols")) {
            ResetScriptDefines();
        }
        if(GUILayout.Button("Sync MoudleFiles To SVN")) {
            SyncModuleFilesToSVN();
        }
    }

	[MenuItem("NOVNINE/Reset Module Defines", false, 9002)]
    public static void ResetScriptDefines()
    {
        //ScriptDefine.Remove(latestDesc.GetAllModules().ConvertAll(m=> ("USE_"+m.id)).ToArray());
        //bool useBinaryData = ScriptDefine.HasDefine("USE_DLLDATACLASS");
        ScriptDefine.RemoveStartsWith("USE_");
        foreach(var desc in currentDesc.GetAllModules()) {
			if(GetModuleStatus(desc.id) != ModuleStatus.NotInstalled) {
				Debug.Log(desc.id);
				ScriptDefine.Add(new string[] {"USE_"+desc.id}, desc.platforms);
			}
        }
        //if(useBinaryData)
        //    ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, null);
    }

    static ModuleManager()
    {
        if(EditorPrefs.HasKey("PLATFORM_REPO")) {
            PLATFORM_REPO = EditorPrefs.GetString("PLATFORM_REPO");
        }
        //EditorPrefs.DeleteKey("pending_install");
    }

    static void ResetSelection()
    {
        categoryFold = Enumerable.Repeat<bool>(true, latestDesc.modules.Keys.Count).ToArray();
        selection = new Dictionary<string, bool>();
        foreach(var m in latestDesc.GetAllModules())
            selection.Add(m.id, false);
    }

	[MenuItem("NOVNINE/ModuleSystem", false, 9001)]
    static void ModuleManagerOpen()
    {
        _currentDesc = null;
        _latestDesc = null;
        ResetSelection();

        if(!System.IO.File.Exists(MODULE_DESC_PATH)) {
            _currentDesc = BuildInstalledModuleDescriptionFromScratch(latestDesc);
            ModuleDescription.Save(MODULE_DESC_PATH, _currentDesc);
        }
		/*
        if(!System.IO.File.Exists(Application.dataPath+"/NOVNINE/Base/Module/Context.cs")) {
            GenerateModuleSource(latestDesc);
        }
		*/
        EditorWindow.GetWindow<ModuleManager>();
    }

	/*[MenuItem("NOVNINE/Migrate from NNPlatform1.0", false, 9002)]
    static void UpdateAllScript() {
    	ShellRunner.Run("find", "Script -iname *.cs -exec sed -f NOVNINE/Base/BMPlatform/platformupdate.sed -i \"\" {} \\;", Application.dataPath);
    }
    */

    public static void BuildAllModules(string writePath)
    {
        //string backup = PLATFORM_REPO;
        if(!writePath.EndsWith("/"))
            writePath += "/";
        PLATFORM_REPO_WRITE = writePath;

        EditorUtility.DisplayProgressBar( "Build All Modules", "....", 0);

        if(!System.IO.File.Exists(MODULE_DESC_PATH)) {
            _currentDesc = BuildInstalledModuleDescriptionFromScratch(latestDesc);
            ModuleDescription.Save(MODULE_DESC_PATH, _currentDesc);
            //GenerateModuleSource(latestDesc);
        }

        string path = Path.GetDirectoryName(PLATFORM_REPO_WRITE.Replace("file://", ""));
        Directory.CreateDirectory(path);

        float pp = 0.0f;
        float pp_div = 0.7f / currentDesc.GetAllModules().Count;
        foreach(var mod in currentDesc.GetAllModules()) {
            EditorUtility.DisplayProgressBar( "Build All Modules", "Exporting "+mod.id, pp);
            BuildModule(mod.id);
            pp += pp_div;
        }

        EditorUtility.DisplayProgressBar( "Build All Modules", "Exporting BMPlatform_FULL", 0.8f);
        
        path = path+"/BMPlatform_FULL-platform-r"+".unitypackage";
        //build Full-sized package
        string[] pathToExport = {
			"Assets/NOVNINE",
            "Assets/Editor",
            "Assets/Plugins",
            "Assets/link.xml"
        };
        AssetDatabase.ExportPackage(pathToExport, path, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies );

        EditorUtility.ClearProgressBar();
        //PLATFORM_REPO = backup;
    }

    void OnGUI()
    {
        if(compiling) return;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawModules();
        EditorGUILayout.EndScrollView();
    }

    #endregion

    static void WaitCompile()
    {
        progress = 0;
        compiling = true;
        EditorPrefs.SetBool("compile", true);
    }

    static string[] ignoreFiles = {
		"NOVNINE/Editor/NNPlatformFiles.txt",
        "Plugins/Android/AndroidManifest.xml",
        "Editor/UpdateXcode.pyc",
    };
    public static bool IsIgnoreFile(string path)
    {
        return System.Array.Exists( ignoreFiles, (iFile)=> {
            return path.StartsWith(iFile);
        } );
    }

    static void SyncModuleFilesToSVN()
    {
        List<string> addFile = new List<string>();
        List<string> removeFile = new List<string>();
        foreach(var desc in latestDesc.GetAllModules()) {
            foreach(var f in desc.files) {
//                string[] svnStatus = SvnUtil.StatusAt(f);
//                foreach(string st in svnStatus) {
//                    if(string.IsNullOrEmpty(st) || st.EndsWith(".meta")) continue;
//                    if(st.StartsWith("?")) {
//                        string toAdd = st.Substring(1).Trim();
//                        if(IsIgnoreFile(toAdd)) continue;
//                        if(toAdd.EndsWith("_")) continue;
//                        //ExecuteShell("svn", "add "+toAdd+"*", Application.dataPath);
//                        addFile.Add(toAdd);
//                    } else if(st.StartsWith("svn: warning: W155010:")) { //file not found.
//                        string toAdd = Path.GetDirectoryName(f).Substring(7);
//                        if(IsIgnoreFile(toAdd)) continue;
//                        if(toAdd.EndsWith("_")) continue;
//                        addFile.Add(toAdd);
//                    } else if(st.StartsWith("!")) {
//                        string toRem = st.Substring(1).Trim();
//                        if(IsIgnoreFile(toRem)) continue;
//                        if(toRem.EndsWith("_")) continue;
//                        removeFile.Add(toRem);
//                    }
//                }
            }
        }
        addFile.RemoveAll(e => removeFile.Contains(e));
//        SvnSyncWindow.SetList(addFile.Distinct().ToArray(), removeFile.Distinct().ToArray());
    }

    void Update()
    {
        if(!EditorPrefs.GetBool("compile")) return;

        if(EditorApplication.isCompiling) {
            progress+= 0.01f;
            EditorUtility.DisplayCancelableProgressBar("Compiling", "", progress);
        } else {
            Debug.Log("Watch : done");
            compiling = false;
            EditorPrefs.SetBool("compile", false);
            EditorUtility.ClearProgressBar();
            ProcessPendingInstall();
        }
    }

    bool ProcessPendingInstall()
    {
        string[] mods = EditorPrefsList.Get("pending_install");
        if(mods != null) {
            Debug.Log("pending_install : "+string.Join(", ", mods));
            int idx = 0;
            while(string.IsNullOrEmpty(mods[idx])) idx++;
            string modInst = mods[idx];
            EditorPrefsList.Remove("pending_install", modInst);
            EditorPrefsList.Add("installed", modInst);
            bool result = InstallModuleInternal(modInst);
            return result;
        } else if(EditorPrefs.HasKey("installed")) {
            List<string> newmods = new List<string>(EditorPrefsList.Get("installed"));
            if(newmods.Count > 0) {
                Debug.Log("Importing done, now update ModuleRepo.cs");
                EditorPrefs.DeleteKey("installed");
#if USE_DataModel
                //FIXME
                if(newmods.IndexOf("DataModel") != -1) {
                    DataMenuItem.Compile();
                }
#endif
                ResetSelection();
            }
        }
        return false;
    }

    public static bool ValidateModuleDefines()
    {
        return true;
    }

    public const string MODULE_PREFAB = "Assets/Resources/ModuleContext.prefab";

	//[MenuItem ("NOVNINE/Build Context Prefab", false, 2102)]
    public static void BuildContextPrefab()
    {
        string writePath = MODULE_PREFAB;
        string assetPath = Path.GetDirectoryName(writePath);
        Object prefab = null;

        //check asset side
        if (!Directory.Exists(assetPath)) {
            Directory.CreateDirectory(assetPath);
        }

        prefab = AssetDatabase.LoadAssetAtPath(writePath, typeof(GameObject));
        if (prefab == null) {
            GameObject go = new GameObject();
            prefab = PrefabUtility.CreateEmptyPrefab(writePath);
            PrefabUtility.ReplacePrefab(go, prefab);
            GameObject.DestroyImmediate(go);
            prefab = AssetDatabase.LoadAssetAtPath(writePath, typeof(GameObject));
        }

        GameObject prefabGO = prefab as GameObject;
        foreach(var mod in ModuleManager.currentDesc.GetAllModules()) {
            if(mod.contextClass == null) continue;
			if(GetModuleStatus(mod.id) == ModuleStatus.NotInstalled) {
				var comp = prefabGO.GetComponent(mod.contextClass);
				if(comp != null) {
					Debug.Log("Removindg Module Context : "+mod.contextClass);
					Object.Destroy(comp);
				}
			} else {
				if(prefabGO.GetComponent(mod.contextClass) == null) {
					Debug.Log("Adding Module Context : "+mod.contextClass);
					UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(prefabGO, "Assets/NOVNINE/Base/Module/Editor/ModuleManager.cs (768,6)", mod.contextClass);
				}
			}
        }
        AssetDatabase.SaveAssets();

        //select to new one
        if(prefab != null) {
            Object[] selection = new Object[1];
            selection[0] = prefab;
            Selection.objects = selection;
        }
    }

    static void AssignMemberwise(object src, object target)
    {
        //Debug.Log("Assigning Fields from : "+src.GetType().FullName);
        FieldInfo[] fields = src.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach(var f in fields) {
            FieldInfo ftarget = target.GetType().GetField(f.Name);
            ftarget.SetValue(target, f.GetValue(src));
        }
    }

    static void AssignField(object src, object dst, string fieldName)
    {
        //Debug.Log("AssignField "+fieldName+" : "+src.GetType().FullName+" => "+dst.GetType().FullName);
        FieldInfo fsrc = src.GetType().GetField(fieldName);
        FieldInfo fdst = dst.GetType().GetField(fieldName);
        if(fsrc.FieldType.Equals(fdst.FieldType))
            fdst.SetValue(dst, fsrc.GetValue(src));
        else if(fsrc.FieldType.IsEnum)
            fdst.SetValue(dst, System.Enum.Parse(fdst.FieldType, fsrc.GetValue(src).ToString()));
        else if(fsrc.FieldType.IsArray) {
            var srcArr = (fsrc.GetValue(src) as System.Array);
            var dstArr = System.Array.CreateInstance(fdst.FieldType.GetElementType(), srcArr.Length);
            for(int i=0; i<srcArr.Length; ++i) {
                dstArr.SetValue(System.Activator.CreateInstance(fdst.FieldType.GetElementType()),i);
                AssignMemberwise(srcArr.GetValue(i), dstArr.GetValue(i));
            }
            fdst.SetValue(dst, dstArr);
        } else {
            AssignMemberwise(src, dst);
        }
    }

    static void AssignMembersTo(object src, object[] targets)
    {
        Debug.Log("Assigning Fields from : "+src.GetType().FullName);
        object target = System.Array.Find(targets, m=> m.GetType().Equals(src.GetType()));
        FieldInfo[] fields = src.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach(var f in fields) {
            //Debug.Log(f.Name);

            if(target != null) {
                AssignField(src, target, f.Name);
            } else {
                bool found = false;
                foreach(var c in targets) {
                    FieldInfo ftarget = c.GetType().GetField(f.Name);
                    if(ftarget != null) {
                        AssignField(src, c, f.Name);
                        found = true;
                        break;
                    }
                }
                if(!found)
                    Debug.LogWarning("Field "+f.Name+" not found on ModuleContext");
            }
        }
    }

	[MenuItem ("NOVNINE/Build Context Prefab From PlatformInfo", false, 2103)]
    public static void BuildContextPrefabFromPlatformInfo()
    {
        BuildContextPrefab();
        GameObject srcHolder = AssetDatabase.LoadAssetAtPath("Assets/Resources/platformConfig/Nov9Platform.prefab", typeof(GameObject)) as GameObject;
        if(srcHolder == null)
            return;

        GameObject targetHolder = LoadModuleHolder();
        MonoBehaviour[] targets = targetHolder.GetComponents<MonoBehaviour>();

        MonoBehaviour[] srcs = srcHolder.GetComponents<MonoBehaviour>();

        foreach(var s in srcs) {
            AssignMembersTo(s, targets);
        }
        //PropertyInfo[] properties = typeof(PlatformInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        SaveModuleHolder(targetHolder);
    }

    public static GameObject LoadModuleHolder()
    {
        GameObject mod = AssetDatabase.LoadAssetAtPath(MODULE_PREFAB, typeof(GameObject)) as GameObject;
        if(mod == null)
            Debug.LogError("Module.LoadModuleHolder fail : ModuleContext.prefab is not found");
        return mod;
    }

    public static void SaveModuleHolder(GameObject go)
    {
        EditorUtility.SetDirty(go);
        AssetDatabase.SaveAssets();
    }
}

