using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//[System.Serializable]
//public class ThemeInfo 
//{ 
//    public string themeName;
//    public tk2dSpriteCollectionData collection;
//}

public class NNTheme : ScriptableObject 
{
    const string themeSettingsAssetName = "NNThemeSettings";
    const string themeSettingsPath = "Resources";
    const string themeSettingsAssetExtension = ".asset";

	static NNTheme instance;
	
	public List<string> themeInfos = new List<string>();

    public static NNTheme Instance
	{
        get 
		{
			if(instance == null)
			{
#if UNITY_EDITOR
				if(WorldMapBuilder.EditorMode)
				{
					instance = CreateInstance<NNTheme>();
					string properPath = Path.Combine(Application.dataPath, themeSettingsPath);

					if(!Directory.Exists(properPath))
						AssetDatabase.CreateFolder("Assets", "Resources");

					string fullPath = Path.Combine(Path.Combine("Assets", themeSettingsPath), themeSettingsAssetName + themeSettingsAssetExtension);
					AssetDatabase.CreateAsset(instance, fullPath);
				}
				else
				{
					instance = Resources.Load(themeSettingsAssetName) as NNTheme;
					Debug.Assert(instance != null);	
				}
#else					
				instance = Resources.Load(themeSettingsAssetName) as NNTheme;
				Debug.Assert( instance != null);	
#endif
			}
			else
			{
//#if UNITY_EDITOR
//				if(WorldMapBuilder.EditorMode)
//				{
//					string properPath = Path.Combine(Application.dataPath, themeSettingsPath);
//
//					if(!Directory.Exists(properPath))
//						AssetDatabase.CreateFolder("Assets", "Resources");
//
//					string fullPath = Path.Combine(Path.Combine("Assets", themeSettingsPath), themeSettingsAssetName + themeSettingsAssetExtension);
//					AssetDatabase.DeleteAsset(fullPath);
//					instance = CreateInstance<NNTheme>();
//					AssetDatabase.CreateAsset(instance, fullPath);
//					AssetDatabase.Refresh();
//				}
//#endif		
			}
				
            return instance;
        }
    }

//#if UNITY_EDITOR
//	[MenuItem("NOVNINE/Theme Settings")]
//    public static void Edit () 
//	{
//        Selection.activeObject = Instance;
//    }
//#endif

    void OnEnable () 
	{
        DontDestroyOnLoad(this);
    }
	
	public string GetThemeByIndex (int index) 
	{
		if(themeInfos != null)
		{
			if(themeInfos.Count > index && index > -1)
				return themeInfos[index];
		}
		
		return null;
	}
	
    public bool ContainsTheme (string themeName) 
	{
		if(themeInfos != null)
		{
			for(int i = 0; i < themeInfos.Count; ++i)
			{
				if(themeInfos[i] == themeName)
					return true;
			}	
		}
		return false;
    }

//    public tk2dSpriteCollectionData GetCollection (string themeName)
//	{
//        if (themeInfos != null) 
//		{
//			for(int i = 0; i < themeInfos.Length; ++i)
//			{
//				if(themeInfos[i] == null) continue;
//				if(themeInfos[i].themeName == themeName) 
//					return themeInfos[i].collection;
//			}
//        }
//
//		return null;
//    }
}
