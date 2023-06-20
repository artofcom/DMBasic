using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using ProtoBuf;
using ProtoBuf.Meta;
using JsonFx.Json;

using NOVNINE.Diagnostics;

public class DataContainer<T>  where T : class, new()
{
    protected static T instance = null;
    protected static string postfix = "A";

    public static T Data
    {
        get {
			
//			if (instance == null) 
//			{
//#if UNITY_EDITOR && !USE_DLLDATACLASS
//                instance = Load();
//#else 
//                instance = LoadBinary();
//#endif 
//            }
            return instance;
        }

		set {instance = value;}
    }

    public static void SetPostfix() {
        //Debug.Log("DataContainer.SetPostfix : "+pf);

        // 일단 고정 by wayne.
        string pf = "A";

        postfix = pf;
        ResourceFileSystem.subPath = pf;
    }

    public static string GetPostfix() 
	{
        return postfix;
    }

    public static void Reload() 
	{
        instance = Load();
    }

    public static T Load() {
        if(!JsonFileSystem.HasVaileFileSystem())
            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );

        ContextPoolAdapter.Reset();

        string strFullName = typeof(T).FullName;

//        float startTime = Time.realtimeSinceStartup;
        string content = null;
        try {
			content = JsonFileSystem.Read(typeof(T).FullName);
        }
        catch (System.IO.FileNotFoundException ex) {
            Debug.Log(ex.Message+" "+typeof(T).FullName+".Load() Fail, Making empty one");
            return new T();
        }

        T output = null;
        try {
            output = JsonReader.Deserialize<T>(content);
        }
        catch (JsonDeserializationException exception) {
            int line, col;
            exception.GetLineAndColumn(content, out line, out col);
            Debug.LogError("JsonDeserializationException : "+exception.Message+"  on "+typeof(T).FullName+".txt Line:"+line+"  Col:"+col);
        }

        //Debug.Log(typeof(T).FullName+" Loading done - "+(Time.realtimeSinceStartup-startTime));

        Root.SetPostfix();

        return output;
    }

	public static T LoadBinary(TextAsset txAsset)
	{
		ContextPoolAdapter.Reset();

		//float startTime = Time.realtimeSinceStartup;
		T output = null;

		Debug.LogError("DataContainer.LoadBinary not supported : USE_DLLDATACLASS is off");

		return output;
	}

    public static T LoadSubBinary(TextAsset txAsset)
    {
        T output = null;
        Debug.LogError("DataContainer.LoadBinary not supported : USE_DLLDATACLASS is off");
        return output;
    }
	
    public static T LoadBinary() 
	{
		if(!JsonFileSystem.HasVaileFileSystem())
            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );

        ContextPoolAdapter.Reset();

        //float startTime = Time.realtimeSinceStartup;
        T output = null;
        Debug.LogError("DataContainer.LoadBinary not supported : USE_DLLDATACLASS is off");

//        Debug.Log(typeof(T).FullName+"."+postfix+" Binary Loading done - "+(Time.realtimeSinceStartup-startTime));
        return output;
    }

    public static void Save(T target) {
        Debugger.Assert(target != null);
        Debug.Log(typeof(T).FullName+" Saving");
        if(!JsonFileSystem.HasVaileFileSystem())
            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );

		StringBuilder output = new StringBuilder();
		JsonWriterSettings writerSetting = new JsonWriterSettings();
		writerSetting.PrettyPrint = true;

		using (JsonWriter writer = new JsonWriter(output, writerSetting))
		{
			writer.Write(target);
			JsonFileSystem.Write(typeof(T).FullName, output.ToString());
		}
	
#if UNITY_EDITOR
        AssetDatabase.ImportAsset("Assets/Data/text/"+postfix+"/"+typeof(T).FullName+".txt");
#endif
    }

    public static void SaveBinary(T target) 
	{
        Debugger.Assert(target != null);
        string dataSet = postfix;
        Debug.Log(typeof(T).FullName+"."+dataSet+" Saving");

        //Directory.CreateDirectory(Application.dataPath+"/Data/Resources");
		Directory.CreateDirectory(Application.dataPath+"/Data/data");

        Debug.LogError("DataContainer.SaveBinary not supported : USE_DLLDATACLASS is off");
    }
}

