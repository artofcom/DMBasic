using UnityEngine;
//using System.Collections;
using Spine;
using Spine.Unity;
using System;
using System.IO;

public class CustomSkeletonDataAsset : ScriptableObject 
{
	#region Inspector
	public AtlasAsset[] atlasAssets = new AtlasAsset[0];
	public float scale = 0.01f;
	
	public TextAsset skeletonJSON;
	[SpineAnimation(includeNone: false)]
	public string[] fromAnimation = new string[0];
	[SpineAnimation(includeNone: false)]
	public string[] toAnimation = new string[0];
	public float[] duration = new float[0];
	public float defaultMix;
	public RuntimeAnimatorController controller;
	
	string skeletonJSONText;

	public bool IsLoaded { get { return this.skeletonData != null; } }

	public string SkeletonJSONText
	{
		set{ skeletonJSONText = value; }
		get{ return skeletonJSONText; }
	}
	
	void Reset () {
		Clear();
	}
	#endregion

	SkeletonData skeletonData;
	AnimationStateData stateData;
	
	#region Runtime Instantiation
	/// <summary>
	/// Creates a runtime SkeletonDataAsset.</summary>
	public static CustomSkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAsset atlasAsset, bool initialize, float scale = 0.01f) {
		return CreateRuntimeInstance(skeletonDataFile, new [] {atlasAsset}, initialize, scale);
	}

	/// <summary>
	/// Creates a runtime SkeletonDataAsset.</summary>
	public static CustomSkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAsset[] atlasAssets, bool initialize, float scale = 0.01f) {
		CustomSkeletonDataAsset skeletonDataAsset = ScriptableObject.CreateInstance<CustomSkeletonDataAsset>();
		skeletonDataAsset.Clear();
		skeletonDataAsset.skeletonJSON = skeletonDataFile;
		skeletonDataAsset.atlasAssets = atlasAssets;
		skeletonDataAsset.scale = scale;

		if (initialize)
			skeletonDataAsset.GetSkeletonData(true);

		return skeletonDataAsset;
	}
	#endregion

	public void Clear () {
		skeletonData = null;
		stateData = null;
	}

	public SkeletonData GetSkeletonData (bool quiet) {
		
//		if (skeletonJSON == null) 
//        {
//			if (!quiet)
//				Debug.LogError("Skeleton JSON file not set for SkeletonData asset: " + name, this);
//			
//            Clear();
//			return null;
//		}
			
			
		if(skeletonJSON != null)
			skeletonJSONText = skeletonJSON.text;
		
        if (skeletonJSONText == null || skeletonJSONText.Length < 0)
        {
            Clear();
            return null;
        }
			
			
		if (atlasAssets == null) {
			atlasAssets = new AtlasAsset[0];
			if (!quiet)
				Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
			Clear();
			return null;
		}


		if (atlasAssets.Length == 0) {
			Clear();
			return null;
		}

		if (skeletonData != null)
			return skeletonData;

		AttachmentLoader attachmentLoader;
		float skeletonDataScale;
		Atlas[] atlasArray = this.GetAtlasArray();

		attachmentLoader = new AtlasAttachmentLoader(atlasArray);
		skeletonDataScale = scale;

		SkeletonData loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSONText, attachmentLoader, skeletonDataScale);
/*
bool isBinary = skeletonJSON.name.ToLower().Contains(".skel");
			SkeletonData loadedSkeletonData;

			try {
				if (isBinary)
					loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.bytes, attachmentLoader, skeletonDataScale);
				else
					loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.text, attachmentLoader, skeletonDataScale);

			} catch (Exception ex) {
				if (!quiet)
					Debug.LogError("Error reading skeleton JSON file for SkeletonData asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
				return null;

			}
*/
		this.InitializeWithData(loadedSkeletonData);

		return skeletonData;
	}

	public void InitializeWithData (SkeletonData sd) 
	{
		this.skeletonData = sd;
		this.stateData = new AnimationStateData(skeletonData);
		FillStateData();
	}
	
	public void FillStateData () {
			if (stateData != null) {
				stateData.defaultMix = defaultMix;

				for (int i = 0, n = fromAnimation.Length; i < n; i++) {
					if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0)
						continue;
					stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
				}
			}
		}
		
	public Atlas[] GetAtlasArray () 
	{
		var returnList = new System.Collections.Generic.List<Atlas>(atlasAssets.Length);
		for (int i = 0; i < atlasAssets.Length; i++) 
		{
			var aa = atlasAssets[i];
			if (aa == null) continue;
			var a = aa.GetAtlas();
			if (a == null) continue;
			returnList.Add(a);
		}
		return returnList.ToArray();
	}

	public static SkeletonData ReadSkeletonData (byte[] bytes, AttachmentLoader attachmentLoader, float scale)
	{
		var input = new MemoryStream(bytes);
		var binary = new SkeletonBinary(attachmentLoader){ Scale = scale };
		return binary.ReadSkeletonData(input);
	}

	public static SkeletonData ReadSkeletonData (string text, AttachmentLoader attachmentLoader, float scale) 
	{
		var input = new StringReader(text);
		var json = new SkeletonJson(attachmentLoader){Scale = scale};
		return json.ReadSkeletonData(input);
	}
	
	public AnimationStateData GetAnimationStateData () 
	{
		if (stateData != null)
			return stateData;
		GetSkeletonData(false);
		return stateData;
	}

}
