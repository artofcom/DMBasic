using UnityEditor;
using UnityEngine;
using System.Collections;

[InitializeOnLoad]
public static class GlobalDefines {
	static GlobalDefines() {
		string defines;
		foreach(BuildTargetGroup btg in System.Enum.GetValues(typeof(BuildTargetGroup))){
			if(btg == BuildTargetGroup.Unknown){ continue; } // do not process unknown target~!
			defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
			//if(!defines.Contains("JMFP")){
				// adds the JMFP global define to the project :)
			//	PlayerSettings.SetScriptingDefineSymbolsForGroup(btg,defines + ";JMFP");
			//}
		}
		
	}
}
