using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Spine;
using JsonFx.Json;
using System.Reflection;
using NOVNINE.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldMapBuilder : MonoBehaviour 
{
#if UNITY_EDITOR
	static public bool EditorMode = false;
#endif
	

    public int LevelCount = 0;
	public List<int> TreasureList = new List<int>();
	
    List<SkeletonRenderer> PathList = new List<SkeletonRenderer>();


    public AtlasAsset _atlasAsset = null;
	
	public SkeletonDataAsset _skeletonDataAsset = null;
	public SkeletonRenderer _skeletonAnimation = null;
	
	public Dictionary<string,object> dataJson = new Dictionary<string,object>();
	public	List<AtlasRegion> atlasETCRegionList = new List<AtlasRegion>();
	public	List<AtlasRegion> atlasBGRegionList = new List<AtlasRegion>();




	string[] Symbol = new string[] { "Symbol_collect", "Symbol_fill", "Symbol_defeat", "Symbol_find", "Symbol_clear"};
	
	Bone Me;
	GameObject tm;
	
	void Awake()
	{
        if(null == _skeletonAnimation)
            return;

#if UNITY_EDITOR
		WorldMapBuilder.EditorMode = false;
#endif
		
		if(_skeletonDataAsset == null)
		{
			SetAtlasData(_atlasAsset);
			//CreateWorldMap();	
		}
		
		for(int i = 0; i < 20; ++i)
		{
			string slotName = string.Format("Level{0}", i); 
			Slot _slot = _skeletonAnimation.Skeleton.FindSlot(slotName);
			int slotIndex = _skeletonAnimation.Skeleton.FindSlotIndex(slotName);
			_slot.Attachment = _skeletonAnimation.Skeleton.GetAttachment(slotIndex, Symbol[i % Symbol.Length]);
		}
		
		Bone _root = _skeletonAnimation.Skeleton.FindBone("root");
		
		BoneData _boneData = new BoneData(1,"ME",_root.Data);
		Me = new Bone(_boneData, _skeletonAnimation.Skeleton, _root);
		
		PathConstraint _pathConstraint = _skeletonAnimation.Skeleton.FindPathConstraint("Path.T1");
		PathConstraintData _data = _pathConstraint.Data;
		PathConstraintData _newData = new PathConstraintData("ME_path");
		_newData.Order = 5;
		_newData.Target = _data.Target;
		_newData.PositionMode = _data.PositionMode;
		_newData.SpacingMode = _data.SpacingMode;
		_newData.RotateMode = _data.RotateMode;
		_newData.OffsetRotation = _data.OffsetRotation;
		_newData.RotateMix = _data.RotateMix;
		_newData.TranslateMix = _data.TranslateMix;
		_newData.Bones.Add(_boneData);
		_newData.Position = _data.Position + (_data.Spacing * 6);
		
		PathConstraint tt = new PathConstraint(_newData, _skeletonAnimation.Skeleton);
		tt.Bones.Items[0] = Me;
		tt.Update();
		
		tm = GameObject.CreatePrimitive(PrimitiveType.Plane);
		tm.transform.SetParent(_skeletonAnimation.transform.parent, false);
		Vector3 targetWorldPosition = _skeletonAnimation.transform.TransformPoint(new Vector3(Me.worldX, Me.worldY, 0f));
		targetWorldPosition.z =  -3f;
		tm.transform.position = targetWorldPosition;
	}

    void FindPathData()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            SkeletonRenderer render = transform.GetChild(i).GetComponent<SkeletonRenderer>();
            if (render != null)
            {
                render.Initialize(true);
                PathList.Add(render);        
            }
        }
    }
	
    public void PaserData()
	{
        TreasureList.Clear();
        PathList.Clear();
        FindPathData();

		LevelCount = 0;
		NNTheme.Instance.themeInfos.Clear();

        for (int i = 0; i < PathList.Count; ++i)
        {
            SkeletonData skeletonData = PathList[i].SkeletonDataAsset.GetSkeletonData(false);
            PathConstraintData _pathConstraint = skeletonData.FindPathConstraint(string.Format("Path.T{0}", i));
            Debug.Assert(_pathConstraint != null);

            string[] list = _pathConstraint.Target.Name.Split('.');
            NNTheme.Instance.themeInfos.Add(list[1]);

            for(int n = 0; n < _pathConstraint.Bones.Count; ++n)
            {
                string token = _pathConstraint.Bones.Items[n].Name.Substring(0, 5);
                if(token == "Level")
                    ++LevelCount;
                else
                    TreasureList.Add(LevelCount);
            }
        }
		
#if UNITY_EDITOR
		if(WorldMapBuilder.EditorMode) AssetDatabase.Refresh();
#endif		
	}
	
	public void SetAtlasData(AtlasAsset atlasAsset)
	{
		if(_skeletonDataAsset == null)
		{
			_skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
			_skeletonDataAsset.scale = 0.021f; 
		}

		_skeletonAnimation = GetComponent<SkeletonRenderer>();
		if(_skeletonAnimation == null)
			_skeletonAnimation = gameObject.AddComponent<SkeletonRenderer>();

		atlasETCRegionList.Clear();
		atlasBGRegionList.Clear();

		_skeletonDataAsset.Clear();

		for(int n =0; n< _skeletonDataAsset.atlasAssets.Length; ++n) 
		{
			if(_skeletonDataAsset.atlasAssets[n] != null)
				_skeletonDataAsset.atlasAssets[n].Clear();
		}

		if(atlasAsset != null)
		{
			FieldInfo regionsField = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.NonPublic);
			List<AtlasRegion> atlasRegionList = (List<AtlasRegion>)regionsField.GetValue(atlasAsset.GetAtlas());
			List<AtlasRegion> bgList = new List<AtlasRegion>();

			for(int i = 0; i < atlasRegionList.Count; ++i)
			{
				int size = atlasRegionList[i].name.Length;
				if(size > 10)
				{
					string token = atlasRegionList[i].name.Substring(0, 10);

					if(token == "background")
					{
						//					H = atlasRegionList[i].originalHeight;
						//					W = atlasRegionList[i].width;
						bgList.Add(atlasRegionList[i]);
					}
					else
						atlasETCRegionList.Add(atlasRegionList[i]);
				}
				else
					atlasETCRegionList.Add(atlasRegionList[i]);
			}
			
			while(bgList.Count > 0)
			{
				int num = int.Parse(bgList[0].name.Substring(10));
				int index = 0;
				while(true)
				{
					bool ok = true;
					
					for( int i = 0; i < bgList.Count; ++i)
					{
						int T = int.Parse(bgList[i].name.Substring(10));
						if(num > T)
						{
							num = T;
							index = i;
							ok = false;
							break;
						}
					}
					
					if(ok)
					{
						atlasBGRegionList.Add(bgList[index]);
						bgList.RemoveAt(index);
						break;
					}
				}				
			}
			
			AtlasAsset[] values = {atlasAsset};

			_skeletonDataAsset.atlasAssets = values;			
		}
	}

	public void CreateWorldMap() 
	{
		if(_atlasAsset == null)
			return;

		//MakeSkeletonData();

		_skeletonAnimation.skeletonDataAsset = _skeletonDataAsset;
		_skeletonAnimation.Initialize(true);
	}

//	void MakeSkeletonData()
//	{		
//		dataJson.Clear();
//		
//		Dictionary<string,object> _skeleton = new Dictionary<string,object>();
//
//		_skeleton.Add("hash", "tOjF277XiMHLe5DLTuJc4rEYdxs");
//		_skeleton.Add("spine","3.3.07");
//		_skeleton.Add("width", 0);
//		_skeleton.Add("height", 0);
//		dataJson.Add("skeleton",_skeleton);
//
//		List<object> _bones = new List<object>();
//		Dictionary<string,object> _bone = new Dictionary<string,object>();
//		_bone.Add("name","root");
//		_bones.Add(_bone);
//		dataJson.Add("bones",_bones);
//
//		/* order end top */ 
//		//* build *//
//
//		Dictionary<string,object> _default = new Dictionary<string,object>();
//		Dictionary<string,object> _Json1 = new Dictionary<string,object>();
//		List<object> _list = new List<object>();
//
//		for(int i = 0; i < atlasBGRegionList.Count; ++i)
//		{
//			string name = atlasBGRegionList[i].name;
//			int token = int.Parse(name.Substring(10));
//
//			Dictionary<string,string> _Json = new Dictionary<string,string>();
//			_Json.Add("name",name);
//			_Json.Add("bone","root");
//			_Json.Add("attachment",name);
//			_list.Add(_Json);
//
//			Dictionary<string,object> _Json2 = new Dictionary<string,object>();
//			Dictionary<string,object> _Json3 = new Dictionary<string,object>();	
//
//			_Json2.Add("y" ,atlasBGRegionList[i].originalHeight * token + (atlasBGRegionList[i].originalHeight * 0.5f) - 300.0f);
//			_Json2.Add("width",atlasBGRegionList[i].originalWidth);
//			_Json2.Add("height",atlasBGRegionList[i].originalHeight);
//
//			_Json3.Add(atlasBGRegionList[i].name,_Json2);
//			_Json1.Add(name,_Json3);
//		}
//
//		_default.Add("default", _Json1);
//		dataJson["skins"] = _default;
//		dataJson.Add("slots",_list);
//
//		_skeletonDataAsset.SkeletonJSONText = JsonWriter.Serialize(dataJson);
//		AttachmentLoader attachmentLoader = new AtlasAttachmentLoader(_skeletonDataAsset.GetAtlasArray());
//		
//		SkeletonData loadedSkeletonData = CustomSkeletonDataAsset.ReadSkeletonData(_skeletonDataAsset.SkeletonJSONText, attachmentLoader, _skeletonDataAsset.scale);
//		_skeletonDataAsset.InitializeWithData(loadedSkeletonData);
//	}
}
