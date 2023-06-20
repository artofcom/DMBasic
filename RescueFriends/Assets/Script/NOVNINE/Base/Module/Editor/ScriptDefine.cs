/**
* @file ScriptDefine.cs
* @brief
* @author Choi YongWu(amugana@bitmango.com)
* @version 1.0
* @date 2013-09-17
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace NOVNINE
{

	public static class ScriptDefine
	{
	    static BuildTargetGroup[] TARGETS_ALL = new BuildTargetGroup[] {
	        BuildTargetGroup.Unknown,
	        BuildTargetGroup.Standalone,
	        //BuildTargetGroup.WebPlayer,
	        //BuildTargetGroup.Wii,
	        BuildTargetGroup.iOS,
	        BuildTargetGroup.PS3,
	        BuildTargetGroup.XBOX360,
	        BuildTargetGroup.Android,
	//        BuildTargetGroup.GLESEmu,
			BuildTargetGroup.WebGL,
			BuildTargetGroup.Tizen,
			BuildTargetGroup.PSP2,
			BuildTargetGroup.PS4,
			BuildTargetGroup.XboxOne,
			BuildTargetGroup.SamsungTV,
			BuildTargetGroup.tvOS,
				
	#if UNITY_4_2
	        BuildTargetGroup.WP8,
	        BuildTargetGroup.Metro,
	        BuildTargetGroup.BB10
	#endif
	    };
	
	    public static void Add(string[] defs, BuildTargetGroup[] targets = null )
	    {
	        if(targets == null)
	            targets = TARGETS_ALL;
	        foreach(var target in targets) {
	            string[] predefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Trim().Split(';');
	            List<string> result = predefs.Concat(defs).Distinct().ToList();
	            string resultBuf = string.Join(";", result.ToArray());
	            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,resultBuf);
	            //Debug.Log("scriptdefine.add : "+resultBuf);
	        }
	    }
				
	    public static void Remove(string[] defs, BuildTargetGroup[] targets = null )
	    {
	        if(targets == null)
	            targets = TARGETS_ALL;
	        foreach(var target in targets) 
			{
	            string[] predefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Trim().Split(';');
	            List<string> result = predefs.Except(defs).ToList();
	            string resultBuf = string.Join(";", result.ToArray());
	            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,resultBuf);
	            //Debug.Log("ScriptDefine.Remove : "+resultBuf);
	        }
	    }
	
	    public static void RemoveStartsWith(string startStr, BuildTargetGroup[] targets = null )
	    {
	        if(targets == null)
	            targets = TARGETS_ALL;
	        foreach(var target in targets) {
	            string[] predefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Trim().Split(';');
	            List<string> result = predefs.Where(x => !x.StartsWith(startStr)).ToList();
	            string resultBuf = string.Join(";", result.ToArray());
	            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,resultBuf);
	            //Debug.Log("ScriptDefine.Remove : "+resultBuf);
	        }
	    }
	
	    public static bool HasDefine(string def, BuildTargetGroup[] targets = null)
	    {
	        if(targets == null)
	            targets = TARGETS_ALL;
	        foreach(var target in targets) {
	            string[] predefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Trim().Split(';');
	            if(-1 == System.Array.IndexOf(predefs, def))
	                return false;
	        }
	        return true;
	    }
	}

}

