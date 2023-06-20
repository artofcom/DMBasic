//place this script in the Editor folder within Assets.
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine;
//to be used on the command line:
//$ Unity -quit -batchmode -executeMethod WebGLBuilder.build


class ProjectBuilder
{
	//	static void PerformBuildOSX ()
	//	{
	//		string[] scenes = { "Assets/Scenes/Scene1.unity" };
	//		BuildPipeline.BuildPlayer(scenes, "/Users/ormes/ownCloud/Games/DungeonHack/OSX/killthespider", BuildTarget.StandaloneOSXUniversal, BuildOptions.None );
	//	}
	//
	//	static void PerformBuildWindows64()
	//	{
	//		string[] scenes = { "Assets/Scenes/Scene1.unity" };
	//		BuildPipeline.BuildPlayer(scenes, "/Users/ormes/ownCloud/Games/DungeonHack/Windows64/killthespider.exe", BuildTarget.StandaloneWindows64, BuildOptions.None );
	//	}
	//
	//	static void PerformBuildWindows ()
	//	{
	//		string[] scenes = { "Assets/Scenes/Scene1.unity" };
	//		BuildPipeline.BuildPlayer(scenes, "/Users/ormes/ownCloud/Games/DungeonHack/Windows/killthespider.exe", BuildTarget.StandaloneWindows, BuildOptions.None );
	//	}
	//
	//	static void PerformBuildLinux ()
	//	{
	//		string[] scenes = { "Assets/Scenes/Scene1.unity" };
	//		BuildPipeline.BuildPlayer(scenes, "/Users/ormes/ownCloud/Games/DungeonHack/Linux/killthespider", BuildTarget.StandaloneLinuxUniversal, BuildOptions.None );
	//	}
	//
    #if UNITY_ANDROID
	[MenuItem ("Build/Build Android - release")]
    static void PerformBuildReleaseAndroid()
	{
        string[] scenes = FindEnabledEditorScenes();

//        using UnityEditor;
//
//        /*
//안드로이드 빌드 셋팅
//*/
//
//        public class AndroidKeystore : Editor
//        {
//            [MenuItem("메뉴명/Android/Auto Settings")]
//            static void KeystoreSettings()
//            {
//                // 회사명
//                PlayerSettings.companyName = "회사명";
//                // 게임명
//                PlayerSettings.productName = "게임명";
//
//                // 번들명
//                PlayerSettings.bundleIdentifier = "com.번들명.번들명";
//                // 버젼
//                PlayerSettings.bundleVersion = "1.0";
//                // 번들 버젼
//                PlayerSettings.Android.bundleVersionCode = 1;
//
//                // keystore경로/파일
        //                PlayerSettings.Android.keystoreName = "/Users/mac/Documents/work/Match3Project/JMF_storeKey.keystore;
//                // keystore 암호
//                PlayerSettings.Android.keystorePass = "Nov91108!";
//
//                // key Alias 명/암호
//                PlayerSettings.Android.keyaliasName = "nov9 games";
//                PlayerSettings.Android.keyaliasPass = "Nov91108!";
//            }
//        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"USE_UncleBill;USE_UncleBill_GooglePlayStore;USE_RemoteNotification;USE_LocalNotification;LIVE_MODE");

        //JMFP;USE_UncleBill;USE_UncleBill_AppleStore;NO_GPGS;SPINE_TK2D;USE_RemoteNotification;USE_LocalNotification;USE_DLLDATACLASS
        //

//    if(NOVNINE.ScriptDefine.HasDefine("DEV_TEST",list))
//            NOVNINE.ScriptDefine.Remove(new string[] {"DEV_TEST"}, list);
//        
//        if(NOVNINE.ScriptDefine.HasDefine("DEV_MODE"))
//            NOVNINE.ScriptDefine.Remove(new string[] {"DEV_MODE"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_UncleBill_GooglePlayStore") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_UncleBill_GooglePlayStore"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_RemoteNotification") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_RemoteNotification"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_LocalNotification") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_LocalNotification"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_DLLDATACLASS") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_DLLDATACLASS") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, list);

        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);

        // keystore경로/파일
        PlayerSettings.Android.keystoreName = root + "JMF_storeKey.keystore";
        // keystore 암호
        PlayerSettings.Android.keystorePass = "Nov91108!";
        // key Alias 명/암호
        PlayerSettings.Android.keyaliasName = "nov9 games";
        PlayerSettings.Android.keyaliasPass = "Nov91108!";
        UnityEngine.Debug.Log("Android Build Start!!");

        string res = "";// BuildPipeline.BuildPlayer(scenes, root +string.Format("rainbowfox_{0}.apk", System.DateTime.Now.ToString("yyyy-MM-dd-HH")), BuildTarget.Android, BuildOptions.None );
		
        if(res.Length > 0)
            UnityEngine.Debug.Log("Android Build cancel!!");
        else
            UnityEngine.Debug.Log("Android Build End!!");
	}
    [MenuItem ("Build/Build Android - release Log")]
    static void PerformBuildReleaseAndroidLog()
    {
        string[] scenes = FindEnabledEditorScenes();
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"USE_UncleBill;USE_UncleBill_GooglePlayStore;USE_RemoteNotification;USE_LocalNotification;LIVE_MODE");

        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);

        // keystore경로/파일
        PlayerSettings.Android.keystoreName = root + "JMF_storeKey.keystore";
        // keystore 암호
        PlayerSettings.Android.keystorePass = "Nov91108!";
        // key Alias 명/암호
        PlayerSettings.Android.keyaliasName = "nov9 games";
        PlayerSettings.Android.keyaliasPass = "Nov91108!";
        UnityEngine.Debug.Log("Android Build Start!!");

        string res = "";// BuildPipeline.BuildPlayer(scenes, root +string.Format("rainbowfox_L_{0}.apk", System.DateTime.Now.ToString("yyyy-MM-dd-HH")), BuildTarget.Android, BuildOptions.None );

        if(res.Length > 0)
            UnityEngine.Debug.Log("Android Build cancel!!");
        else
            UnityEngine.Debug.Log("Android Build End!!");
    }

    [MenuItem ("Build/Build Android - Dev")]
    static void PerformBuildAndroidDEV ()
    {
        string[] scenes = FindEnabledEditorScenes();

        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);

        // keystore경로/파일
        PlayerSettings.Android.keystoreName = root + "JMF_storeKey.keystore";
        // keystore 암호
        PlayerSettings.Android.keystorePass = "Nov91108!";
        // key Alias 명/암호
        PlayerSettings.Android.keyaliasName = "nov9 games";
        PlayerSettings.Android.keyaliasPass = "Nov91108!";

//        string strDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
//
//        if(strDefine != "JMFP;USE_UncleBill;USE_UncleBill_GooglePlayStore;SPINE_TK2D;USE_RemoteNotification;USE_LocalNotification;USE_DLLDATACLASS")
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"USE_UncleBill;USE_UncleBill_GooglePlayStore;USE_RemoteNotification;USE_LocalNotification");

//
//        BuildTargetGroup[] list = new BuildTargetGroup[]{ BuildTargetGroup.Android};
//
//        if(NOVNINE.ScriptDefine.HasDefine("DEV_MODE"))
//            NOVNINE.ScriptDefine.Remove(new string[] {"DEV_MODE"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_UncleBill_GooglePlayStore") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_UncleBill_GooglePlayStore"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_RemoteNotification") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_RemoteNotification"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_LocalNotification") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_LocalNotification"}, list);
//
//        if(NOVNINE.ScriptDefine.HasDefine("USE_DLLDATACLASS") == false)
//            NOVNINE.ScriptDefine.Add(new string[] {"USE_DLLDATACLASS"}, list);

        UnityEngine.Debug.Log("Android Build Start!!");
        string res = "";// BuildPipeline.BuildPlayer(scenes, root +string.Format("rainbowfox_DEV_{0}.apk", System.DateTime.Now.ToString("yyyy-MM-dd-HH")), BuildTarget.Android, BuildOptions.None );
        if(res.Length > 0)
            UnityEngine.Debug.Log("Android Build cancel!!");
        else
            UnityEngine.Debug.Log("Android Build End!!");
    }
    [MenuItem ("Build/Build Android - Dev")]
    static void PerformBuildAndroidDEVLog ()
    {
        string[] scenes = FindEnabledEditorScenes();

        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);

        // keystore경로/파일
        PlayerSettings.Android.keystoreName = root + "JMF_storeKey.keystore";
        // keystore 암호
        PlayerSettings.Android.keystorePass = "Nov91108!";
        // key Alias 명/암호
        PlayerSettings.Android.keyaliasName = "nov9 games";
        PlayerSettings.Android.keyaliasPass = "Nov91108!";

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"USE_UncleBill;USE_UncleBill_GooglePlayStore;USE_RemoteNotification;USE_LocalNotification");

        UnityEngine.Debug.Log("Android Build Start!!");
        string res = "";// BuildPipeline.BuildPlayer(scenes, root +string.Format("rainbowfox_DEV_{0}.apk", System.DateTime.Now.ToString("yyyy-MM-dd-HH")), BuildTarget.Android, BuildOptions.None );
        if(res.Length > 0)
            UnityEngine.Debug.Log("Android Build cancel!!");
        else
            UnityEngine.Debug.Log("Android Build End!!");
    }


    /*[MenuItem ("Build/Build Android - Dev No Server")]
    static void PerformBuildAndroidNoGameServerDEVLog ()
    {
        string[] scenes = FindEnabledEditorScenes();

        string root = Application.dataPath.Substring(0, Application.dataPath.Length-6);

        // keystore경로/파일
        PlayerSettings.Android.keystoreName = root + "JMF_storeKey.keystore";
        // keystore 암호
        PlayerSettings.Android.keystorePass = "Nov91108!";
        // key Alias 명/암호
        PlayerSettings.Android.keyaliasName = "nov9 games";
        PlayerSettings.Android.keyaliasPass = "Nov91108!";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"USE_UncleBill;USE_UncleBill_GooglePlayStore;USE_RemoteNotification;USE_LocalNotification");

        UnityEngine.Debug.Log("Android Build Start!!");
        string res = BuildPipeline.BuildPlayer(scenes, root +string.Format("rainbowfox_DEV_L_NO{0}.apk", System.DateTime.Now.ToString("yyyy-MM-dd-HH")), BuildTarget.Android, BuildOptions.None );
        if(res.Length > 0)
            UnityEngine.Debug.Log("Android Build cancel!!");
        else
            UnityEngine.Debug.Log("Android Build End!!");
    }
    */
    #endif

	static void PerformBuildWebGL ()
	{
		string[] scenes = { "Assets/Scene/MainScene.unity" };
		BuildPipeline.BuildPlayer(scenes, "/Users/mac/Documents/work/HOWT/Unity.HeroesTab/HeroesTab/webGL1", BuildTarget.WebGL, BuildOptions.None );
	}
	
	private static string[] FindEnabledEditorScenes()
	{
		List<string> EditorScenes = new List<string>(); 
		foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled) continue;
			EditorScenes.Add(scene.path); 
		}
		return EditorScenes.ToArray(); 
	}

}


//
//echo "Begin OSX Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/OSX/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath ~/DungeonHack/ -executeMethod MyEditorScript.PerformBuildOSX
//echo "End OSX Build"
//
//echo "Begin Windows 86 Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/Windows/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath ~/DungeonHack/ -executeMethod MyEditorScript.PerformBuildWindows
//echo "End Windows Build"
//
//echo "Begin Windows 64 Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/Windows64/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath ~/DungeonHack/ -executeMethod MyEditorScript.PerformBuildWindows64
//echo "End Windows Build"
//
//echo "Begin Android Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/Android/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath /Documents/work/Match3Project/ -executeMethod ProjectBuilder.PerformBuildAndroid
//echo "End Android Build"
//
//echo "Begin WebGL Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/WebGL/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath ~/DungeonHack/ -executeMethod PerformBuild.PerformBuildWebGL
//echo "End WebGL Build"
//
//echo "Begin Linux Build"
//rm -rf /Users/ormes/ownCloud/Games/DungeonHack/Linux/*
///Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectpath ~/DungeonHack/ -executeMethod MyEditorScript.PerformBuildLinux
//echo "End Linux Build"