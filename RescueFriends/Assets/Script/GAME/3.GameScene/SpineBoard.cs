using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Spine;
using JsonFx.Json;
using System.Reflection;

public class SpineBoard : MonoBehaviour 
{
	enum LinkIndex 
	{
		NW = 0,
		N = 1,
		NE = 2,
		E = 3,
		SE = 4,
		S = 5,
		SW = 6,
		W = 7,
		MAX = 8,
	};
	
	public class BordLinkType 
	{
		public int type = 0;
		public List<int> list = new List<int>();
		public List<float> scaleXList = new List<float>();
		public List<float> scaleYList = new List<float>();
		public List<float> posXList = new List<float>();
		public List<float> posYList = new List<float>();
	}
	
	CustomSkeletonRenderer _skeletonAnimation = null;
	CustomSkeletonDataAsset _skeletonDataAsset = null;
	public AtlasAsset _atlasAsset = null;
	
	float WH = 0.0f;
	float wh = 0.0f;
	
	Renderer _renderer = null;
	
	Dictionary<string,object> dataJson = new Dictionary<string,object>();
	List<AtlasRegion> atlasRegionList = new List<AtlasRegion>();
	
	void Awake()
	{
		_skeletonDataAsset = ScriptableObject.CreateInstance<CustomSkeletonDataAsset>();
		
		_skeletonAnimation = gameObject.AddComponent<CustomSkeletonRenderer>();
		
		_renderer = _skeletonAnimation.GetComponent<Renderer>() as Renderer;
		//_renderer.sortingOrder = i;
		if(_atlasAsset == null)
			Debug.LogError("_atlasAsset = null");
		
		SetAtlasData(_atlasAsset);
	}
	
	public void SetAtlasData(AtlasAsset atlasAsset)
	{
		atlasRegionList.Clear();
		_atlasAsset = atlasAsset;
		
		FieldInfo regionsField = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.NonPublic);
		atlasRegionList = (List<AtlasRegion>)regionsField.GetValue(_atlasAsset.GetAtlas());
		
		WH = atlasRegionList[0].originalWidth;
		//wh = atlasRegionList[12].originalWidth;
		wh = atlasRegionList[7].originalWidth;
		
		AtlasAsset[] values = {_atlasAsset};
		_skeletonDataAsset.Clear();		

		for(int n =0; n< _skeletonDataAsset.atlasAssets.Length; ++n) 
		{
			if(_skeletonDataAsset.atlasAssets[n] != null)
				_skeletonDataAsset.atlasAssets[n].Clear();
		}

		_skeletonDataAsset.atlasAssets = values;
	}
	
	public void CreateSpineBoardFromBlocks(Point[] blks) 
	{
		if(_atlasAsset == null)
			return;
						
		MakeSkeletonData(blks);
		
		_skeletonAnimation.skeletonDataAsset = _skeletonDataAsset;
		_skeletonAnimation.Initialize(true);
	}
	
	void MakeBordLinkType( BordLinkType linkType,Point _point, int max )
	{
		//linkType.type = (_point.x + _point.y) % 2;
		linkType.type = 0;
		for(int i = 0; i < (int)LinkIndex.MAX; ++i)
		{
			linkType.list.Add(-1);
			linkType.scaleXList.Add(1.0f);
			linkType.scaleYList.Add(1.0f);
			linkType.posXList.Add(0.0f);
			linkType.posYList.Add(0.0f);
		}

		float w = WH - wh;

		//linkType.list[(int)LinkIndex.NW] = 12 + linkType.type;
		linkType.list[(int)LinkIndex.NW] = 7;
		linkType.posXList[(int)LinkIndex.NW] = _point.x * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.NW] = (_point.y + 1) * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.N] = 10 + linkType.type;
		linkType.list[(int)LinkIndex.N] = 6;
		linkType.scaleXList[(int)LinkIndex.N] = w;
		linkType.posXList[(int)LinkIndex.N] = _point.x * atlasRegionList[0].originalWidth;
		linkType.posYList[(int)LinkIndex.N] = (_point.y +1) * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.NE] = 12 + linkType.type;
		linkType.list[(int)LinkIndex.NE] = 7;
		linkType.scaleXList[(int)LinkIndex.NE] = -1.0f;
		linkType.posXList[(int)LinkIndex.NE] = (_point.x+ 1) * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.NE] = (_point.y +1) * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.E] = 8 + linkType.type;
		linkType.list[(int)LinkIndex.E] = 5;
		linkType.scaleXList[(int)LinkIndex.E] = -1.0f;
		linkType.scaleYList[(int)LinkIndex.E] = w;
		linkType.posXList[(int)LinkIndex.E] = (_point.x +1) * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.E] = _point.y * atlasRegionList[0].originalHeight;

		//linkType.list[(int)LinkIndex.SE] = 6 + linkType.type;
		linkType.list[(int)LinkIndex.SE] = 4;
		linkType.scaleXList[(int)LinkIndex.SE] = -1.0f;
		linkType.posXList[(int)LinkIndex.SE] = (_point.x + 1) * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.SE] = _point.y * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.S] = 2 + linkType.type;
		linkType.list[(int)LinkIndex.S] = 2;
		linkType.scaleXList[(int)LinkIndex.S] = w;
		linkType.posXList[(int)LinkIndex.S] = _point.x * atlasRegionList[0].originalWidth;
		linkType.posYList[(int)LinkIndex.S] = _point.y * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.SW] = 6 + linkType.type;
		linkType.list[(int)LinkIndex.SW] = 4;
		linkType.posXList[(int)LinkIndex.SW] = _point.x * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.SW] = _point.y * atlasRegionList[0].originalHeight - atlasRegionList[0].originalHeight * 0.5f;

		//linkType.list[(int)LinkIndex.W] = 8 + linkType.type;	
		linkType.list[(int)LinkIndex.W] = 5;
		linkType.scaleYList[(int)LinkIndex.W] = w;
		linkType.posXList[(int)LinkIndex.W] = _point.x * atlasRegionList[0].originalWidth - atlasRegionList[0].originalWidth * 0.5f;
		linkType.posYList[(int)LinkIndex.W] = _point.y * atlasRegionList[0].originalHeight;
	}
	
	void MakeSkeletonData(Point[] blks)
	{		
        dataJson.Clear();

		Dictionary<string,object> _skeleton = new Dictionary<string,object>();
		
		_skeleton.Add("hash", "tOjF277XiMHLe5DLTuJc4rEYdxs");
		_skeleton.Add("spine","3.3.07");
		_skeleton.Add("width", 0);
		_skeleton.Add("height",0);
		dataJson.Add("skeleton",_skeleton);
		
		List<object> _bones = new List<object>();
		Dictionary<string,object> _bone = new Dictionary<string,object>();
		_bone.Add("name","root");
		_bones.Add(_bone);
		dataJson.Add("bones",_bones);
		
		int xmax = blks.Max(b=>b.X)+1;
		int ymax = blks.Max(b=>b.Y)+1;
		int xmin = blks.Min(b=>b.X);
		int ymin = blks.Min(b=>b.Y);
		
		/* order end top */ 
		
		List<object> _list = new List<object>();
		
		Dictionary<Point, BordLinkType> _bordLinkTypeList = new Dictionary<Point, BordLinkType>();
		
		for(int x = xmin; x < xmax; ++x)
		{
			for(int y = ymin; y < ymax; ++y)
			{
				_bordLinkTypeList.Add(new Point(x,y), new BordLinkType());
			}
		}
		
		for(int i = 0; i < blks.Length; ++i)
		{
			MakeBordLinkType(_bordLinkTypeList[blks[i]], blks[i], xmax);
		}
		 
		//* build *//
		List<int> checkList = new List<int>();
		
		for(int i = 0; i < blks.Length; ++i)
		{
			List<Point> _linkList = new List<Point>();
			
			//			NW:0| N:1|	NE:2
			//			----|----|-----
			//			W:7 |----| E:3
			//			----|----|-----
			//			SW:6| S:5| SE:4
			
			_linkList.Add(new Point(blks[i].x - 1,blks[i].y + 1));
			_linkList.Add(new Point(blks[i].x, blks[i].y + 1));
			_linkList.Add(new Point(blks[i].x + 1,blks[i].y + 1));
			_linkList.Add(new Point(blks[i].x + 1,blks[i].y));
			_linkList.Add(new Point(blks[i].x + 1,blks[i].y -1));
			_linkList.Add(new Point(blks[i].x ,blks[i].y - 1));
			_linkList.Add(new Point(blks[i].x -1, blks[i].y - 1));
			_linkList.Add(new Point(blks[i].x -1,blks[i].y ));
			
			
			int t = 0;
			for(int index = 0; index < 4; ++index)
			{
				t = index * 2;
				checkList.Clear();
				checkList.Add(index + ( index -1 ));
				checkList.Add(index +1+ ( index -1 )); 
				checkList.Add( index +2+ ( index -1 ));
				
				if(index == 0)
					checkList[0] = 7;					
				
				bool b1 = (_bordLinkTypeList.ContainsKey(_linkList[checkList[0]]) && _bordLinkTypeList[_linkList[checkList[0]]].list.Count > 0);
				bool b2 = (_bordLinkTypeList.ContainsKey(_linkList[checkList[1]]) && _bordLinkTypeList[_linkList[checkList[1]]].list.Count > 0);
				bool b3 = (_bordLinkTypeList.ContainsKey(_linkList[checkList[2]]) && _bordLinkTypeList[_linkList[checkList[2]]].list.Count > 0);
				
				if(b1)
				{
					_bordLinkTypeList[blks[i]].list[((int)LinkIndex.W + t) % (int)LinkIndex.MAX] = -1;// 7
					_bordLinkTypeList[_linkList[checkList[0]]].list[((int)LinkIndex.NE+ t) % (int)LinkIndex.MAX ] = -1;// 2
					_bordLinkTypeList[_linkList[checkList[0]]].list[((int)LinkIndex.E + t) % (int)LinkIndex.MAX] = -1;// 3
				}

				if(b3)
				{
					_bordLinkTypeList[blks[i]].list[((int)LinkIndex.N + t) % (int)LinkIndex.MAX] = -1;// 1
					_bordLinkTypeList[_linkList[checkList[2]]].list[((int)LinkIndex.S+ t) % (int)LinkIndex.MAX] = -1;// 5
					_bordLinkTypeList[_linkList[checkList[2]]].list[((int)LinkIndex.SW+ t) % (int)LinkIndex.MAX] = -1;// 6
				}

				if(b2)
				{
					_bordLinkTypeList[_linkList[checkList[1]]].list[((int)LinkIndex.SE+ t) % (int)LinkIndex.MAX] = -1;// 4
					if(_bordLinkTypeList[_linkList[checkList[0]]].list.Count > 0)
						_bordLinkTypeList[_linkList[checkList[0]]].list[((int)LinkIndex.NE+ t) % (int)LinkIndex.MAX] = -1;// 2
					if(_bordLinkTypeList[_linkList[checkList[2]]].list.Count > 0)
						_bordLinkTypeList[_linkList[checkList[2]]].list[((int)LinkIndex.SW + t) % (int)LinkIndex.MAX] = -1;// 6

					if(b1 && b3)
					{
						_bordLinkTypeList[blks[i]].list[t] = -1;
						_bordLinkTypeList[blks[i]].list[((int)LinkIndex.W + t) % (int)LinkIndex.MAX] = -1;// 7
						_bordLinkTypeList[blks[i]].list[((int)LinkIndex.N + t) % (int)LinkIndex.MAX] = -1;// 1

						_bordLinkTypeList[_linkList[checkList[0]]].list[((int)LinkIndex.N + t) % (int)LinkIndex.MAX] = -1;// 1
						_bordLinkTypeList[_linkList[checkList[1]]].list[((int)LinkIndex.S + t) % (int)LinkIndex.MAX] = -1;// 5
						_bordLinkTypeList[_linkList[checkList[1]]].list[((int)LinkIndex.E + t) % (int)LinkIndex.MAX] = -1;// 3
						_bordLinkTypeList[_linkList[checkList[2]]].list[((int)LinkIndex.W + t) % (int)LinkIndex.MAX] = -1;// 7
					}
					else
					{
						//_bordLinkTypeList[blks[i]].list[t] = 4 + _bordLinkTypeList[blks[i]].type;
						_bordLinkTypeList[blks[i]].list[t] = 3;

						if(t == (int)LinkIndex.NW || t == (int)LinkIndex.SE)
							_bordLinkTypeList[blks[i]].scaleXList[t] = -1.0f;
						else if(t == (int)LinkIndex.NE || t == (int)LinkIndex.SW)
							_bordLinkTypeList[blks[i]].scaleXList[t] = 1.0f;
					}
				}
				else
				{
					if(b1 && b3)
					{
						//_bordLinkTypeList[blks[i]].list[t] = 4 + _bordLinkTypeList[blks[i]].type;
						_bordLinkTypeList[blks[i]].list[t] = 3;

						if( t == (int)LinkIndex.SE)
							_bordLinkTypeList[blks[i]].scaleXList[t] = -1.0f;
						else
							_bordLinkTypeList[blks[i]].scaleXList[t] = 1.0f;

					}
					else if(!(!b1 || !b3))
						_bordLinkTypeList[blks[i]].list[t] = -1;
				}
			}			
		}
		
		//* resize *//
		//float WW = atlasRegionList[12].originalWidth * 0.5f;
		float WW = atlasRegionList[7].originalWidth * 0.5f;
		float XX = WW * 0.5f;
		int M = 0;
		
		List<int> checkList1 = new List<int>();
		
		for(int i = 0; i < blks.Length; ++i)
		{
			for(int t = 0; t < 4; ++t)
			{
				M = (t *2 ) +1;

				if(_bordLinkTypeList[blks[i]].list[M] > 0)
				{
					bool Check1 = true;
					bool Check2 = true;
					List<Point> _linkList = new List<Point>();

					_linkList.Add(new Point(blks[i].x - 1,blks[i].y + 1));
					_linkList.Add(new Point(blks[i].x, blks[i].y + 1));
					_linkList.Add(new Point(blks[i].x + 1,blks[i].y + 1));
					_linkList.Add(new Point(blks[i].x + 1,blks[i].y));
					_linkList.Add(new Point(blks[i].x + 1,blks[i].y -1));
					_linkList.Add(new Point(blks[i].x ,blks[i].y - 1));
					_linkList.Add(new Point(blks[i].x -1, blks[i].y - 1));
					_linkList.Add(new Point(blks[i].x -1,blks[i].y ));
					
					checkList.Clear();
					checkList1.Clear();
					
					for(int _XX = 0; _XX < 3; ++_XX)
					{
						checkList.Add((6 + M  + _XX) % (int)LinkIndex.MAX);
					}
					
					for(int _XX = 2; _XX < 5; ++_XX)
					{
						checkList1.Add((6 + M  + _XX) % (int)LinkIndex.MAX);
					}
					
					for(int yy = 0; yy < 3; ++yy)
					{
						if(_bordLinkTypeList.ContainsKey(_linkList[checkList[yy]]) && _bordLinkTypeList[_linkList[checkList[yy]]].list.Count > 0 && _bordLinkTypeList[_linkList[checkList[yy]]].list[(((int)LinkIndex.NE + (M -1)) + (yy * 2))% (int)LinkIndex.MAX] > -1)
							Check1 = false;
						
						if(_bordLinkTypeList.ContainsKey(_linkList[checkList1[yy]]) && _bordLinkTypeList[_linkList[checkList1[yy]]].list.Count > 0 && _bordLinkTypeList[_linkList[checkList1[yy]]].list[(((int)LinkIndex.SE + (M -1) ) + (yy * 2))% (int)LinkIndex.MAX] > -1)
							Check2 = false;
					}
					
					if(Check1)
						Check1 = _bordLinkTypeList[blks[i]].list[((int)LinkIndex.NW + (M - 1)) % (int)LinkIndex.MAX] < 0;
					if(Check2)
						Check2 = _bordLinkTypeList[blks[i]].list[((int)LinkIndex.NW + (M + 1)) % (int)LinkIndex.MAX] < 0;

					if(M == (int)LinkIndex.N)
					{	
						if(Check1)
						{
							_bordLinkTypeList[blks[i]].posXList[M] -= XX; 
							_bordLinkTypeList[blks[i]].scaleXList[M] += WW; 
						}
						
						if(Check2)
						{
							_bordLinkTypeList[blks[i]].posXList[M] += XX; 
							_bordLinkTypeList[blks[i]].scaleXList[M] += WW; 
						}						
					}

					if(M == (int)LinkIndex.E)
					{
						if(Check1)
						{
							_bordLinkTypeList[blks[i]].posYList[M] += XX; 
							_bordLinkTypeList[blks[i]].scaleYList[M] += WW; 
						}

						if(Check2)
						{
							_bordLinkTypeList[blks[i]].posYList[M] -= XX; 
							_bordLinkTypeList[blks[i]].scaleYList[M] += WW; 
						}
					}

					if(M == (int)LinkIndex.S)
					{
						if(Check1)
						{
							_bordLinkTypeList[blks[i]].posXList[M] += XX; 
							_bordLinkTypeList[blks[i]].scaleXList[M] += WW; 
						}

						if(Check2)
						{
							_bordLinkTypeList[blks[i]].posXList[M] -= XX; 
							_bordLinkTypeList[blks[i]].scaleXList[M] += WW; 
						}
					}

					if(M == (int)LinkIndex.W)
					{
						if(Check1)
						{
							_bordLinkTypeList[blks[i]].posYList[M] -= XX; 
							_bordLinkTypeList[blks[i]].scaleYList[M] += WW; 
						}

						if(Check2)
						{
							_bordLinkTypeList[blks[i]].posYList[M] += XX; 
							_bordLinkTypeList[blks[i]].scaleYList[M] += WW; 
						}
					}
				}
			}
		}
		
		Dictionary<string,object> _default = new Dictionary<string,object>();
		Dictionary<string,object> _Json1 = new Dictionary<string,object>();
		Dictionary<Point, BordLinkType>.Enumerator it = _bordLinkTypeList.GetEnumerator();
		
		int count = 0;
		
		while(it.MoveNext())
		{
			for(int i = 0; i < it.Current.Value.list.Count; ++i)
			{
				if(it.Current.Value.list[i] != -1)
				{
					string name = atlasRegionList[it.Current.Value.list[i]].name + "_" + count.ToString();
					
					Dictionary<string,string> _Json = new Dictionary<string,string>();
					_Json.Add("name",name);
					_Json.Add("bone","root");
					_Json.Add("attachment",atlasRegionList[it.Current.Value.list[i]].name);
					_list.Add(_Json);

					Dictionary<string,object> _Json2 = new Dictionary<string,object>();
					Dictionary<string,object> _Json3 = new Dictionary<string,object>();
					_Json2.Add("scaleX",it.Current.Value.scaleXList[i]);
					_Json2.Add("scaleY",it.Current.Value.scaleYList[i]);
					_Json2.Add("x",it.Current.Value.posXList[i]);
					_Json2.Add("y",it.Current.Value.posYList[i]);
					_Json2.Add("height",atlasRegionList[it.Current.Value.list[i]].originalHeight);
					_Json2.Add("width",atlasRegionList[it.Current.Value.list[i]].originalWidth);

					_Json3.Add(atlasRegionList[it.Current.Value.list[i]].name,_Json2);	
					_Json1.Add(name,_Json3);		
				}
				++count;
			}
		}

		for(int i = 0; i < blks.Length; ++i)
		{
			//M = (blks[i].x * xmax) + blks[i].y;
			//M = M % 2;
			M = ((blks[i].x % 2) + (blks[i].y % 2)) %2;
			
			string name = atlasRegionList[M].name + "_" + count.ToString();
			Dictionary<string,string> _Json = new Dictionary<string,string>();
			_Json.Add("name",name);
			_Json.Add("bone","root");
			_Json.Add("attachment",atlasRegionList[M].name);
			_list.Add(_Json);

			Dictionary<string,object> _Json2 = new Dictionary<string,object>();
			Dictionary<string,object> _Json3 = new Dictionary<string,object>();
			_Json2.Add("x",atlasRegionList[M].originalWidth * blks[i].x);
			_Json2.Add("y",atlasRegionList[M].originalHeight * blks[i].y);
			_Json2.Add("height",atlasRegionList[M].originalWidth);
			_Json2.Add("width",atlasRegionList[M].originalHeight);

			_Json3.Add(atlasRegionList[M].name,_Json2);
			_Json1.Add(name,_Json3);
			++count;
		}
		
		_default.Add("default", _Json1);
		dataJson["skins"] = _default;
		dataJson.Add("slots",_list);
		
		_skeletonDataAsset.SkeletonJSONText = JsonWriter.Serialize(dataJson);
		AttachmentLoader attachmentLoader = new AtlasAttachmentLoader(_skeletonDataAsset.GetAtlasArray());
		
		SkeletonData loadedSkeletonData = CustomSkeletonDataAsset.ReadSkeletonData(_skeletonDataAsset.SkeletonJSONText, attachmentLoader, 0.01f);
		_skeletonDataAsset.InitializeWithData(loadedSkeletonData);
	}
}
