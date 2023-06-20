/**
* @file EditorPrefsList.cs
* @brief
* @author Choi YongWu(amugana@bitmango.com)
* @version 1.0
* @date 2013-09-24
*/
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public static class EditorPrefsList
{
    public static string[] Get(string key)
    {
        string src = EditorPrefs.GetString(key);
        if(!string.IsNullOrEmpty(src)) {
            //Debug.Log("GetList : "+key+" => "+src);
            return src.Split(':');
        }
        return null;
    }

    public static void Add(string key, string entry)
    {
        string src = EditorPrefs.GetString(key);
        if(src != null && src.IndexOf(entry) != -1)
            return;
        if(!string.IsNullOrEmpty(src))
            src += ":"+entry;
        else
            src = entry;
        EditorPrefs.SetString(key, src);
        //Debug.Log("AddList : "+key+" => "+src);
    }

    public static void Remove(string key, string entry)
    {
        List<string> mods = new List<string>(EditorPrefsList.Get(key));
        mods = mods.Where(x=>x!="" && x!=entry).ToList();
        EditorPrefs.SetString(key, string.Join(":", mods.ToArray()));
        //Debug.Log("RemoveList : "+key+" => "+string.Join(":", mods.ToArray()));
    }
}

