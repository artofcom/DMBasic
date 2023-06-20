using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;
using Spine.Unity;
using Spine;
using JsonFx.Json;
using NOVNINE.Diagnostics;

[CustomEditor(typeof(WorldMapBuilder))]
public class WorldMapBuilderEditor : Editor
{
	SerializedProperty atlasAsset;
	WorldMapBuilder targetWorldMapBuilder;
	
	void OnEnable()
	{
		atlasAsset = serializedObject.FindProperty("_atlasAsset");
		targetWorldMapBuilder = (WorldMapBuilder)target;
		WorldMapBuilder.EditorMode = true;
        targetWorldMapBuilder.PaserData();
		WorldMapBuilder.EditorMode = false;
//		targetWorldMapBuilder.SetAtlasData(targetWorldMapBuilder._atlasAsset);
//		targetWorldMapBuilder.CreateWorldMap();
	}
	
	public override void OnInspectorGUI()
	{
		// Find atlasAsset	
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(atlasAsset);
		
		AtlasAsset temp = atlasAsset.objectReferenceValue as AtlasAsset;
		
		if(targetWorldMapBuilder._atlasAsset != temp)
		{
			targetWorldMapBuilder._atlasAsset = temp;
			targetWorldMapBuilder.SetAtlasData(targetWorldMapBuilder._atlasAsset);
			targetWorldMapBuilder.CreateWorldMap();
		}
		
		GUILayout.Space(20);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Build", GUILayout.MinWidth(120)))
		{
			if(targetWorldMapBuilder._atlasAsset != null)
			{
				targetWorldMapBuilder.SetAtlasData(targetWorldMapBuilder._atlasAsset);
				targetWorldMapBuilder.CreateWorldMap();	
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField(string.Format("Level Count: {0}",targetWorldMapBuilder.LevelCount));
		//targetWorldMapBuilder.lastLevel = EditorGUILayout.IntField("Level Count", targetWorldMapBuilder.lastLevel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField(string.Format("Treasure Count: {0}",targetWorldMapBuilder.TreasureList.Count));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
//		GUILayout.BeginHorizontal();
//		GUILayout.FlexibleSpace();
//		if (GUILayout.Button("Load Level", GUILayout.MinWidth(120)))
//			LoadLevelFile();
//		
//		GUILayout.FlexibleSpace();
//		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("Export", GUILayout.MinWidth(120)))
		{
			if(targetWorldMapBuilder._atlasAsset != null)
				Export();
			else
				UnityEditor.EditorUtility.DisplayDialog( "에러", "atlasAsset = null", "Ok");
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
		GUILayout.Space(64);
	}
	
	static bool DisplayDialog(string title, string msg, string ok, string cancel = null) 
	{
#if UNITY_EDITOR
		return UnityEditor.EditorUtility.DisplayDialog( title, msg, ok, cancel);
#else
		Debug.Log(string.Format("DisplayDialog Title({0}) Msg({1}) Ok({2}) Cancel({3})", 
		title, msg, ok, cancel));
		return false;
#endif
	}
	
//	void LoadLevelFile ()
//	{
//		//Root.SetPostfix(PlayerPrefs.GetString("DataSet"));
//		//Root.Data.Initialize();
//		string fullpath = "Assets/Data/text/"+ PlayerPrefs.GetString("DataSet") +"/Data.Root.txt";
//		
//		Data.Root data = JsonReader.Deserialize<Data.Root>(System.IO.File.ReadAllText(fullpath));
//		Debugger.Assert(data != null);
//		
//		List<int> list = new List<int>();
//		
//		if(targetWorldMapBuilder._skeletonAnimation != null)
//		{	
//			SkeletonData skeletonData = targetWorldMapBuilder._skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false);
//			
//			int startIndex = skeletonData.FindSlotIndex("Level0");
//			//int endIndex = skeletonData.FindSlotIndex(string.Format("Level{0}",Root.Data.levels.Length -1));
//			int endIndex = skeletonData.FindSlotIndex(string.Format("Level{0}",targetWorldMapBuilder.lastLevel));
//			int num = 0;
//			for(int i = startIndex; i < endIndex +1; ++i)
//			{
//				string token = skeletonData.Slots.Items[i].Name.Substring(0, 5);
//				if(token != "Level")
//					list.Add(num);
//				else
//					++num;
//			}
//			
//			treasureMax = list.Count;
//			data.treasureIndex = list.ToArray();
//			//Root.Save(Root.Data);
//			string text = JsonWriter.Serialize (data);
//			System.IO.File.WriteAllText(fullpath, text);
//			Debug.Log("SaveLevelToFile : "+fullpath);
//		}
//	}
	
	void Export()
	{
		string path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "WorldMap";
		
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);	
		
		string text = JsonWriter.Serialize (targetWorldMapBuilder.dataJson);
		string fullpath = path + "/WorldMapData.json";
		System.IO.File.WriteAllText(fullpath, text);
		Debug.Log("Export : "+fullpath);
	}
	
}

