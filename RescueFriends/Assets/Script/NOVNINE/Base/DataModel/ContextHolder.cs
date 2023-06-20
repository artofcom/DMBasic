//#if !USE_DLLDATACLASS

using UnityEngine;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;
using System.IO;
using ProtoBuf;

public interface IJsonFileSystem
{
	string LoadContext(string key);
	void SaveContext(string key, string content);
	string Read(string path);
	void Write(string path, string content);
}

public static class JsonFileSystem 
{
	static IJsonFileSystem m_iJsonFileSystem = null;
	
	public static string LoadContext(string key)
	{
		if(m_iJsonFileSystem != null)
			return m_iJsonFileSystem.LoadContext(key);	
		
		return null;
	}

	public static void SaveContext(string key, string content)
	{
		if(m_iJsonFileSystem != null)
			m_iJsonFileSystem.SaveContext(key, content);
	}

	public static void SetFileSystem(IJsonFileSystem _iJsonFileSystem)
	{
		m_iJsonFileSystem = _iJsonFileSystem;
	}
	
	public static bool HasVaileFileSystem()
	{
		return m_iJsonFileSystem != null;
	}
	
	public static string Read(string path)
	{
		if(m_iJsonFileSystem != null)
			return m_iJsonFileSystem.Read(path);	

		return null;
	}

	public static void Write(string path, string content)
	{
		if(m_iJsonFileSystem != null)
			m_iJsonFileSystem.Write(path, content);
	}
}

public static class ContextPool
{
    static List<object> _instances = new List<object>();
    public static IEnumerable list {
        get {
            return _instances;
        }
    }

    public static void Reset() {
        _instances.Clear();
    }

    public static void Add(object one) {
        _instances.Add(one);
    }

    public static void LoadAllContext() {
        Debug.Log("LoadAllContext");
        foreach(object cntx in ContextPool.list) {
            //Debug.Log((string)cntx.GetType().GetField("key").GetValue(cntx));
            MethodInfo method = cntx.GetType().GetMethod("LoadContext");
            method.Invoke(cntx, null);
        }
    }

    public static void SaveAllContext() {
        Debug.Log("SaveAllContext");
        foreach(object cntx in ContextPool.list) {
            //Debug.Log((string)cntx.GetType().GetField("key").GetValue(cntx));
            MethodInfo method = cntx.GetType().GetMethod("SaveContext");
            method.Invoke(cntx, null);
        }
    }
}

//[ProtoContract]
//[JsonContextHolder]
[JsonOptIn]
public class ContextHolder<T> where T : class, new()
{
    JsonWriterSettings writerSetting = new JsonWriterSettings();

    public ContextHolder() 
    {
        ContextPool.Add(this);
    }
	
    string GetKey() {
        return (string)this.GetType().GetField("key").GetValue(this);
    }

    private T _cntx;
    [JsonIgnore] public T record {
        set {
            _cntx = value;
        }
        get {
            if(_cntx == null)
                LoadContext();
            
            return _cntx;
        }
    }
    
    public bool LoadContext() 
    {
       // if(FireBaseHandler.IsConnected())
       //     return false;

        string key = GetKey();
        if(!string.IsNullOrEmpty(key)) 
		{
            string json = JsonFileSystem.LoadContext(key);
            if(!string.IsNullOrEmpty(json)) 
			{
//                JsonReader nestReader = new JsonReader(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(json)));
                JsonReader nestReader = new JsonReader(json);
                //nestReader.BypassContextLoad = true;
                //_cntx = (T)nestReader.Deserialize(typeof(T));

				_cntx = nestReader.Deserialize<T>();
                return true;
            } else {
                _cntx = new T();
                return false;
            }
        } 
        else
        {
            Debug.LogWarning("ContextHolder key is null for Type : "+typeof(T).FullName);
            _cntx = new T();
            return true;
        }
    }

/*
    bool IsEqual(T v1, T v2)
    {
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public|BindingFlags.DeclaredOnly|BindingFlags.Instance);
        foreach(var f in fields) {
            if (f.FieldType.IsValueType) {
                if(! f.GetValue(v1).Equals(f.GetValue(v2)) ) {
                    //Debug.Log(typeof(T).FullName+"] "+f.Name+" :: "+f.GetValue(v1)+" != " +f.GetValue(v2));
                    return false;
                }
            }
        }
        return true;
    }

    static T DEFAULT_VALUE = new T();
*/
	
    public void SaveContext() 
	{
        if(_cntx == null)                   return;
        //if(FireBaseHandler.IsConnected())   return;

		/*
        if(IsEqual(_cntx, DEFAULT_VALUE)) {
            return;
        }
        */
        string key = GetKey();
        writerSetting.PrettyPrint = false;

        StringBuilder output = new StringBuilder();		

        using (JsonWriter writer = new JsonWriter(output, writerSetting))
        {
            //writer.WriteContext = true;
            writer.Write(_cntx);

           // JsonFileSystem.SaveContext(key, System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(output.ToString())));
           JsonFileSystem.SaveContext(key, output.ToString());
        }
    }
        
//    public string ToHexString( byte[] hex)
//    {
//        if (hex == null) return null;
//        if (hex.Length == 0) return string.Empty;
//
//        var s = new StringBuilder();
//        foreach (byte b in hex) {
//            s.Append(b.ToString("x2"));
//        }
//        return s.ToString();
//    }
//
//    public byte[] ToHexBytes( string hex)
//    {
//        if (hex == null) return null;
//        if (hex.Length == 0) return new byte[0];
//
//        int l = hex.Length / 2;
//        var b = new byte[l];
//        for (int i = 0; i < l; ++i) {
//            b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
//        }
//        return b;
//    }
//
//    public bool EqualsTo( byte[] bytes, byte[] bytesToCompare)
//    {
//        if (bytes == null && bytesToCompare == null) return true; // ?
//        if (bytes == null || bytesToCompare == null) return false;
//        if (object.ReferenceEquals(bytes, bytesToCompare)) return true;
//
//        if (bytes.Length != bytesToCompare.Length) return false;
//
//        for (int i = 0; i < bytes.Length; ++i) {
//            if (bytes[i] != bytesToCompare[i]) return false;
//        }
//        return true;
//    }
}
//#endif
