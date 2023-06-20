/**
* @file iCloudFileSystem.cs
* @brief
* @author amugana (amugana@bitmango.com)
* @version 1.0
* @date 2013-07-05
*/
#if UNITY_IPHONE

using UnityEngine;
using System.IO;
using JsonFx.Json;

public class iCloudFileSystem : IJsonFileSystem
{

    public iCloudFileSystem()
    {
        if(!iCloud.IsAvailable())
            Debug.LogError("iCloud is Unavailable on this device");
    }

    public string LoadContext(string key)
    {
        if(iCloud.HasKey(key))
            Debug.Log("iCloudFileSystem.LoadContext "+key+" : " +iCloud.StringForKey(key));
        return iCloud.StringForKey(key);
    }

    public void SaveContext(string key, string content)
    {
        Debug.Log("iCloudFileSystem.SaveContext "+key+" : "+content);
        iCloud.SetString(key, content);
    }

    public string Read(string path)
    {
        //TODO
        //Debug.LogWarning("iCloudFileSystem.Read not Implemented");
        //return null;
#if !UNITY_WEBPLAYER
        return File.ReadAllText(Application.dataPath+"/Data/text/"+path+".txt");//, content);
#else
        Debug.LogError("WEB Player doesn't supported");
        return null;
#endif
    }

    public void Write(string path, string content)
    {
        //TODO
        Debug.LogWarning("iCloudFileSystem.Write not Implemented");
    }
}

#endif

