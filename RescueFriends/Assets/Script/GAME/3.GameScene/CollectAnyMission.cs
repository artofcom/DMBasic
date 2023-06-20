using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using Spine.Unity;
using NOVNINE;

// [ROUND_CHOCO]
public class CollectAnyMission : MonoBehaviour 
{
    public static int MAX_CONTENT   = 5;
    public struct _SHADE_BY_H
    {
        public List<SkeletonAnimation> _listShades;
    };

	public tk2dBaseSprite BG;
	public GameObject           _obstacle_pos;
    public Transform            _trSugarIconBundle;

    GameObject[] _objGo         = new GameObject[MAX_CONTENT];
	SpriteRenderer[] _icons     = new SpriteRenderer[MAX_CONTENT];
    tk2dTextMesh[] _txtCount    = new tk2dTextMesh[MAX_CONTENT];

	Dictionary<string, tk2dTextMesh> jewelDict = new Dictionary<string, tk2dTextMesh>();
    Dictionary<string, tk2dTextMesh> missionObjectDict = new Dictionary<string, tk2dTextMesh>();

    int _nTotalShadeCount       = 0;
    List<_SHADE_BY_H>           _listShadesByH = new List<_SHADE_BY_H>();

    List<int> _listFinished     = new List<int>();
    float _fIconOrgScale        = .0f;

	void Awake ()
	{
		for(int k = 0; k < MAX_CONTENT; ++k)
		{
			_objGo[k]           = transform.Find( string.Format("Content/{0}", k+1 )).gameObject;
			_icons[k]           = _objGo[k].transform.Find("dlgIcon/Icon").GetComponent<SpriteRenderer>();
			//_icons[k].Initialize(true);
			//_icons[k].gameObject.SetActive( false );
			_txtCount[k]        = _objGo[k].transform.Find("dlgIcon/Count").GetComponent<tk2dTextMesh>();
			_txtCount[k].gameObject.SetActive( false );
			_objGo[k].SetActive( false );
            if(.0f == _fIconOrgScale)
                _fIconOrgScale  = _icons[k].transform.localScale.x;
		}
	}
	
	public void InitClearMission()
	{        
		//BG.color                = Color.white;
        //BG.color                = new Color(BG.color.r, BG.color.g, BG.color.b, 0.3f);
	}
	
    void _deInit()
    {
        jewelDict.Clear();
        //sjDict.Clear();
        missionObjectDict.Clear();
        _listFinished.Clear();

        for(int k = 0; k < MAX_CONTENT; ++k)
        {
            _icons[k].gameObject.SetActive( true );
            //_icons[k].Initialize(true); 
            
			OnComplete(k, false);
            _txtCount[k].gameObject.SetActive( true );
			_objGo[k].SetActive( false );
        }

        // de-init.
        for(int k = 0; k < _listShadesByH.Count; ++k)
        {
            for(int g = 0; g < _listShadesByH[k]._listShades.Count; ++g)
            {
                if(null != _listShadesByH[k]._listShades[g])
                    NNPool.Abandon( _listShadesByH[k]._listShades[g].gameObject );
            }
            _listShadesByH[k]._listShades.Clear();
        }
        _listShadesByH.Clear();

        _nTotalShadeCount       = 0;
    }

    void OnEnable ()
    {
        _deInit();

        int nTarget             = 0;

        if(null!=_trSugarIconBundle)
            _trSugarIconBundle.gameObject.SetActive( false );

        #region => init collect mission objects.

        // note : sugar block이 늘 첫번째로 오도록 한다 !
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "SugarBlock", JMFUtils.GM.CurrentLevel.countSugarBlock);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "RoundChoco", JMFUtils.GM.CurrentLevel.countRoundChocho);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "JamShade", JMFUtils.GM.CurrentLevel.countJamBottom);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "RectChoco", JMFUtils.GM.CurrentLevel.countRectChocho);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "ChocoBar", JMFUtils.GM.CurrentLevel.countChocoBar);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "CurseShade", JMFUtils.GM.CurrentLevel.countCursedBottom);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "CottonCandy", JMFUtils.GM.CurrentLevel.countCottonCandy);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "SodaCan", JMFUtils.GM.CurrentLevel.countSodaCan);        
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "Zellato", JMFUtils.GM.CurrentLevel.countZellatto);
		if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "Potion1", JMFUtils.GM.CurrentLevel.countPotion1);
		if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "Potion2", JMFUtils.GM.CurrentLevel.countPotion2);
		if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "Potion3", JMFUtils.GM.CurrentLevel.countPotion3);		
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "CookieJelly", JMFUtils.GM.CurrentLevel.countCookieJelly);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "ColorBox", JMFUtils.GM.CurrentLevel.countColorBox);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "WaffleCooker", JMFUtils.GM.CurrentLevel.countWaffleCooker);
        if(nTarget<MAX_CONTENT) _updateMissionObject(ref nTarget, "MudShade", JMFUtils.GM.CurrentLevel.countMudShade);
		
        #endregion


        #region => init collect normal jewels.
		
        // note : 모든 색상에 대한 데이터가 다 있고, 0 인 녀석은 제외한다.
        //        따라서 index i 는 color index가 된다.
        for (int i = 0; i < JMFUtils.GM.CurrentLevel.numToGet.Length; i++)
        {
            if (nTarget >= MAX_CONTENT) break;
            
            if (JMFUtils.GM.CurrentLevel.numToGet[i] <= 0)
                continue;

            _objGo[ nTarget ].SetActive( true );

			jewelDict.Add(string.Format("Jewel{0}",i), _txtCount[nTarget] );
			UpdateJewel( nTarget,i, JMFUtils.GM.CurrentLevel.numToGet[i]);

            nTarget++;
        }
        #endregion

//        #region => init collect special jewels.
//        for (int i = 0; i < JMFUtils.GM.CurrentLevel.specialJewels.Length; i++)
//        {
//            if (nTarget == MAX_CONTENT) break;
//
//            if (JMFUtils.GM.CurrentLevel.specialJewels[i] <= 0)
//                continue;
//
//            _objGo[ nTarget ].SetActive( true );
//
//            sjDict.Add((SJ_COMBINE_TYPE)i, _txtCount[nTarget]);
//            UpdateSpecialJewel( i, (SJ_COMBINE_TYPE)i, JMFUtils.GM.CurrentLevel.specialJewels[i]);
//
//            nTarget++;
//        }
//        #endregion

       
        AlignCollectsAtCenter();
        JMFRelay.OnCollectJewelForDisplay += OnCollectJewelForDisplay;
        JMFRelay.OnCollectSugarJewelForDisplay += OnCollectSugarJewelForDisplay;
        //JMFRelay.OnCollectSpecialJewel += OnCollectSpecialJewel;
		
		JMFRelay.OnCollectTreasureForDisplay += OnCollectTreasureForDisplay;
        JMFRelay.OnCollectMissionObjectForDisplay += OnCollectMissionObject;
        JMFRelay.OnChangeRemainShadeForDisplay += OnChangeRemainShadeForDisplay;
    }

    void _updateMissionObject(ref int nTarget, string strType, int count)
    {
        if(count <= 0)          return;
        _objGo[nTarget].SetActive( true );
        missionObjectDict.Add(strType, _txtCount[nTarget]);

        if(strType=="CurseShade" && 0==_listShadesByH.Count)
        {
           /* Transform trLT      = _obstacle_pos.transform.FindChild("LT");
            Transform trRB      = _obstacle_pos.transform.FindChild("RB");
            //_listShades.Clear();
            _nTotalShadeCount   = count;
            // init shade count bg.
            int total               = 0;
            const int CNT_PER_LINE  = 22;
            int countLine       = 1 + (count / CNT_PER_LINE);
            for(int r = 0; r < countLine; ++r)
            {
                _SHADE_BY_H shadeByH= new _SHADE_BY_H();
                shadeByH._listShades= new List<SkeletonAnimation>();

                for(int q = 0; q < CNT_PER_LINE; ++q)
                {
                    ++total;
                    if(total >= count)  break;

                //    SkeletonAnimation skAni     = NNPool.GetItem<SkeletonAnimation>("obstacle_bgstone_misson", _obstacle_pos.transform.parent);
                //    shadeByH._listShades.Add( skAni );
                //    skAni.GetComponent<MeshRenderer>().sortingOrder = -2;
                }
                if(0 == shadeByH._listShades.Count)
                    continue;

                //_listShadesByH.Add( shadeByH );

                for(int z = 0; z < shadeByH._listShades.Count; ++z)
                {
                    NNTool.ExecuteForEachRandomIndex(0, CNT_PER_LINE-1, (idx) =>
                    {
                        SkeletonAnimation skAni = shadeByH._listShades[ z ];
                        float fX    = trRB.localPosition.x - ((float)idx) * UnityEngine.Random.Range(0.4f, 0.45f);
                        float fY    = trRB.localPosition.y + UnityEngine.Random.Range(-0.05f, 0.05f) + ((float)r)*UnityEngine.Random.Range(0.6f, 0.75f);
                        TrackEntry  tr   = skAni.AnimationState.SetAnimation(0, "play", true);
                        tr.delay    = UnityEngine.Random.Range(0.0f, 0.15f);
                        skAni.transform.localPosition = new Vector3(fX, fY, trRB.localPosition.z);
                        skAni.transform.localEulerAngles    = new Vector3(.0f, .0f, UnityEngine.Random.Range(-20, 20));                        
                        return true;
                    });
                }
            }*/
        }

        UpdateMissioinObjects( nTarget, strType, count);
        ++nTarget;
    }

    void OnDisable () {
        JMFRelay.OnCollectJewelForDisplay -= OnCollectJewelForDisplay;
        JMFRelay.OnCollectSugarJewelForDisplay -= OnCollectSugarJewelForDisplay;
        //JMFRelay.OnCollectSpecialJewel -= OnCollectSpecialJewel;
		JMFRelay.OnCollectTreasureForDisplay -= OnCollectTreasureForDisplay;
        JMFRelay.OnCollectMissionObjectForDisplay -= OnCollectMissionObject;
        JMFRelay.OnChangeRemainShadeForDisplay -= OnChangeRemainShadeForDisplay;   
        
        if(null!=_trSugarIconBundle)
            _trSugarIconBundle.gameObject.SetActive( false );

        _deInit();     
    }
	
    void OnChangeRemainShadeForDisplay(int remainCount, int eShadeType)
    {
        string strType          = "";
        switch(eShadeType)
        {
        case (int)LEItem.SHADE_TYPE.CURSE:      strType = "CurseShade"; break;
        case (int)LEItem.SHADE_TYPE.MUD_COVER:  strType = "MudShade";   break;
        case (int)LEItem.SHADE_TYPE.JAM:        strType = "JamShade";   break;
        case (int)LEItem.SHADE_TYPE.NET:
        default:                return;
        }
        OnCollectMissionObject(strType, remainCount);
    }
	
    void OnCollectMissionObject(string strType, int remainCount)
    {
        //if(strType.Equals("ChocoBar"))            
        //else

        if (strType != "CurseShade")
            NNSoundHelper.Play("IFX_goal_earning");

		// Debug.Log("strType:" + strType);
        remainCount = Mathf.Max(0, remainCount);
        if(false == missionObjectDict.ContainsKey(strType))
            return;

        tk2dTextMesh textMesh   = missionObjectDict[strType];
        int curValue            = int.Parse( textMesh.text );
        if(curValue <= remainCount)
            return;

        // check animate bg.
        if (strType=="CurseShade" && JMFUtils.GM.CurrentLevel.countCursedBottom>0)
        {
            float fRate         = ((float)remainCount) / ((float)JMFUtils.GM.CurrentLevel.countCursedBottom);
            //if(_listShades.Count > 0)
            if(_listShadesByH.Count>0 && _listShadesByH[0]._listShades.Count>0)
            {
                //SkeletonAnimation   one = _listShades[ _listShades.Count-1 ];
                int idx             =  UnityEngine.Random.Range(0, _listShadesByH[ _listShadesByH.Count-1 ]._listShades.Count);
                SkeletonAnimation   one = _listShadesByH[ _listShadesByH.Count-1 ]._listShades[idx];
                //one.transform.DOMoveY(10, 1.0f).SetRelative(true).OnComplete( () =>
                //{
                //    NNPool.Abandon( one.gameObject );
                //});
                //
                Spine.Animation animation = one.skeleton.Data.FindAnimation("hide");
                one.AnimationState.SetAnimation(0, animation, false);
                DOVirtual.DelayedCall(animation.Duration, () =>
                {
                    NNPool.Abandon( one.gameObject );
                });
                //_listShades.Remove( one );
                _listShadesByH[ _listShadesByH.Count-1 ]._listShades.Remove( one );
                if(0 == _listShadesByH[ _listShadesByH.Count-1 ]._listShades.Count)
                    _listShadesByH.RemoveAt( _listShadesByH.Count-1 );
            }
            
           // BG.color            = new Color(BG.color.r, BG.color.g, BG.color.b, 0.3f + (1.0f-fRate)*0.7f);
        }
        
        textMesh.text           = remainCount.ToString();
        textMesh.Commit();
        AnimateGain(textMesh.gameObject);

		if(remainCount <= 0)
		{
			if(JMFUtils.GM.State == JMF_GAMESTATE.PENDING)
				return;
//            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("ConvertToSpecial_02");
//            pp.transform.position = textMesh.gameObject.transform.position;
//            pp.Play();

			for(int g = 0; g < MAX_CONTENT; ++g)
			{
				if(textMesh.transform.parent.parent.name.Equals(string.Format("{0}", g + 1)))
				{
					OnComplete(g, true);
					textMesh.gameObject.SetActive(false);
					break;
				}
			}
		}
		else
		{
			for(int g = 0; g < MAX_CONTENT; ++g)
			{
				if(textMesh.transform.parent.parent.name.Equals(string.Format("{0}", g + 1)))
				{
					OnComplete(g, false, strType=="CurseShade");
					break;
				}
			}
		}
    }

    void OnCollectJewelForDisplay (int index, int remainCount) 
	{
        NNSoundHelper.Play("IFX_goal_earning");

        remainCount = Mathf.Max(0, remainCount);

		tk2dTextMesh textMesh   = jewelDict[string.Format("Jewel{0}",index)];
        int curValue            = int.Parse( textMesh.text );
        if(curValue <= remainCount)
            return;
        
        textMesh.text           =  remainCount.ToString();
        textMesh.Commit();
        AnimateGain(textMesh.gameObject);
		for(int i = 0; i < _txtCount.Length; ++i)
		{
			if(_txtCount[i] == textMesh)
			{
				index = i;
				break;
			}
		}

        if (remainCount <= 0)
		{
            if (JMFUtils.GM.State == JMF_GAMESTATE.PENDING) return;
			
			OnComplete(index, true);
            textMesh.gameObject.SetActive(false);
        }
		else
			OnComplete(index, false);
    }

    void OnCollectSugarJewelForDisplay (int index, int remainCount) 
	{
        NNSoundHelper.Play("IFX_goal_earning");

        remainCount = Mathf.Max(0, remainCount);

		tk2dTextMesh textMesh   = _trSugarIconBundle.parent.Find("Count").GetComponent<tk2dTextMesh>();//  jewelDict[string.Format("Jewel{0}",index)];
        int curValue            = int.Parse( textMesh.text );
        if(curValue <= remainCount)
            return;
        
        textMesh.text           =  remainCount.ToString();
        textMesh.Commit();
        AnimateGain(textMesh.gameObject);
		for(int i = 0; i < _txtCount.Length; ++i)
		{
			if(_txtCount[i] == textMesh)
			{
				index = i;
				break;
			}
		}

        if (remainCount <= 0)
		{
            if (JMFUtils.GM.State == JMF_GAMESTATE.PENDING) return;
			
			OnComplete(0, true);
            textMesh.gameObject.SetActive(false);
        }
		else
			OnComplete(0, false);
    }
	
	void OnCollectTreasureForDisplay (TREASURE_TYPE type, int remainCount) 
	{        
		remainCount = Mathf.Max(0, remainCount);

		int index = (int)type + 1;
		string strType = string.Format("Potion{0}", index);

        OnCollectMissionObject(strType, remainCount);
	}

//    void OnCollectSpecialJewel (SJ_COMBINE_TYPE type, int remainCount) {
//        if (sjDict.ContainsKey(type) == false) return;
//
//        remainCount = Mathf.Max(0, remainCount);
//
//        tk2dTextMesh textMesh = sjDict[type];
//        textMesh.text = remainCount.ToString();
//        textMesh.Commit();
//        AnimateGain(textMesh.gameObject);
//
//        if (remainCount <= 0) {
//            if (JMFUtils.GM.State == JMF_GAMESTATE.PENDING) return;
//            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("ConvertToSpecial_02");
//            pp.transform.position = textMesh.gameObject.transform.position;
//            pp.Play();
//
//            //tk2dSprite completeSpr= (textMesh.transform.parent.name.Equals("A") == true) ? completeIconA : completeIconB;
//            tk2dSprite completeSpr  = _iconCompletes[0];    // default.
//            for(int g = 0; g < MAX_CONTENT; ++g)
//            {
//                if (textMesh.transform.parent.name.Equals( string.Format("{0}", g+1)) )
//                {
//                    completeSpr = _iconCompletes[g];
//                    break;
//                }
//            }
//            completeSpr.gameObject.SetActive(true);
//            textMesh.gameObject.SetActive(false);
//            AnimateGain(completeSpr.gameObject);
//        }
//    }
	
	void ChangeIcon(int index, string name )
	{
        if(index<0 || index>=_icons.Length)
            return;

        // reset ani.
		//int slotIndex = _icons[index].Skeleton.FindSlotIndex("block_normal");
		//Slot _slot = _icons[index].Skeleton.FindSlot("block_normal");
		//_slot.Attachment = _icons[index].Skeleton.GetAttachment(slotIndex, name);
        _icons[index].sprite    = this.GetComponent<SpriteLibrary>().getSpriteByName( name );
	}
	
	void OnComplete(int index, bool complete, bool isShade=false)
	{
		if(complete)
        {
            NNSoundHelper.Play("IFX_goal_group_complete");

            if(false == isShade)
                StartCoroutine( _processReposition( .0f, index ));
            else
            {
                //TrackEntry entry    = _icons[index].AnimationState.SetAnimation(0, "complete", false);
                //StartCoroutine( _processReposition( entry.Delay, index ));
                StartCoroutine( _processReposition( .0f, index ));
            }
        } 
		else
		{
            //if(false == isShade)
            //{
			//    TrackEntry trackEntry = _icons[index].AnimationState.SetAnimation(0, "idle", false);
			//    _icons[index].AnimationState.AddAnimation(0, "normal", true,trackEntry.Delay);
           // }
		}
		
       

//		Slot _slot = _icons[index].Skeleton.FindSlot("check");
//		if(complete)
//		{
//			int slotIndex = _icons[index].Skeleton.FindSlotIndex("check");
//			_slot.Attachment = _icons[index].Skeleton.GetAttachment(slotIndex, "icon_check");
//		}
//		else
//			_slot.Attachment = null;
	}

    IEnumerator _processReposition(float fDelay, int idxTarget)
    {
        yield return new WaitForSeconds(fDelay);
        if(idxTarget<0 || idxTarget>=_icons.Length)
            yield break;

        // Debug.Log("Drop Index.." + idxTarget);

        // 나중에 떨어지는 로직으로 변경.
        yield return StartCoroutine( _dropToGoal(idxTarget) );
        _icons[idxTarget].gameObject.SetActive( false );
        _objGo[idxTarget].gameObject.SetActive( false );

        const float fReposTime  = 0.5f;
        AlignCollectsAtCenter(fReposTime);
        yield return new WaitForSeconds(fReposTime);
    }

    IEnumerator _dropToGoal(int idx)
    {
        if(idx<0 || idx>=_icons.Length)
            yield break;

        // reposition - z.
        _listFinished.Add( idx );
        _objGo[idx].transform.transform.position    = new Vector3(_objGo[idx].transform.transform.position.x, _objGo[idx].transform.transform.position.y, -65.0f);

        float fDuration         = 0.8f;
        Ease eEaseType          = Ease.InBack;

        _objGo[idx].transform.DOKill();
        _icons[idx].transform.DOKill();
        Transform trPosHead     = transform.Find( "goalPath/mid" );
        _objGo[idx].transform.DOMoveX(trPosHead.Find("pos1").position.x, fDuration).SetEase(eEaseType);
        _objGo[idx].transform.DOMoveY(trPosHead.Find("pos1").position.y, fDuration).SetEase(eEaseType);
        
        yield return new WaitForSeconds(fDuration);
    }
    
    public static string getJewelSkinName(int index)
    {
        string name = "";
        switch (index) 
		{
			case 0 :    name = "block_red";      break;
			case 1 :    name = "block_yellow";   break;
			case 2 :    name = "block_green";    break;
			case 3 :    name = "block_blue";     break;
			case 4 :    name = "block_purple";   break;
			case 5 :    name = "block_orange";   break;            
			case 6 :    name = "block_skyblue";  break;
			case 7 :    name = "block_violet";   break;
        }
		return name;
    }
	void UpdateJewel ( int index,int type, int count) 
	{
		string name = getJewelSkinName(type);
		
		ChangeIcon(index,name);
		_txtCount[index].text           = count.ToString();
		_txtCount[index].Commit();
		
		_icons[index].transform.parent.gameObject.SetActive(count > 0);
        _icons[index].transform.localScale  = Vector3.one * _fIconOrgScale;
    }
	
//	void UpdateSpecialJewel ( int index , SJ_COMBINE_TYPE type, int count) 
//	{
//		string name = "";
//        switch (type) 
//		{
//			case SJ_COMBINE_TYPE.L :    name = "mission_l";      break;
//			case SJ_COMBINE_TYPE.LL :   name = "mission_l_l";    break;
//			case SJ_COMBINE_TYPE.LB :   name = "mission_b_l";    break;
//			case SJ_COMBINE_TYPE.LR :   name = "mission_rb_l";   break;
//			case SJ_COMBINE_TYPE.B :    name = "mission_b";      break;
//			case SJ_COMBINE_TYPE.BB :   name = "mission_b_b";    break;
//			case SJ_COMBINE_TYPE.BR :   name = "mission_rb_b";   break;
//			case SJ_COMBINE_TYPE.R :    name = "mission_rb";     break;
//			case SJ_COMBINE_TYPE.RR :   name = "mission_rb_rb";  break;
//        }
//
//		ChangeIcon(index,name);
//		_txtCount[index].text           = count.ToString();
//		_txtCount[index].Commit();
//
//		_icons[index].transform.parent.gameObject.SetActive(count > 0);
//    }
	
    string getObjectSkinName(string strType)
    {
        string name             = "";
        if(strType.Equals("RoundChoco"))        name = "RoundchocoL1";
		else if(strType.Equals("JamShade"))     name = "obstacle_jam";        
		else if(strType.Equals("RectChoco"))    name = "obstacle_wood_01";    
		else if(strType.Equals("ChocoBar"))     name = "Panel_chocobar1x1";		
		else if(strType.Equals("CottonCandy"))  name = "obstacle_monster_01"; 
		else if(strType.Equals("SodaCan"))      name = "Sodacan";		      
		else if(strType.Equals("Zellato"))      name = "Gelato";
		else if(strType.Equals("Potion1"))      name = "potion1";             
		else if(strType.Equals("Potion2"))      name = "potion2";             
		else if(strType.Equals("Potion3"))      name = "potion3";             
        else if(strType.Equals("CurseShade"))   name = "obstacle_bgstone_goal"; 
        else if(strType.Equals("CookieJelly"))  name = "cookie_jelly2";       
        else if(strType.Equals("ColorBox"))     name = "coloboxB1";           
        else if(strType.Equals("WaffleCooker")) name = "waffle2";             
        else if(strType.Equals("MudShade"))     name = "thorn_mud1";          
        else if(strType.Equals("Strawberry"))   name = "Strawberry";          

        return name;
    }

	void UpdateMissioinObjects(int index, string strType, int count)
    {
		string name = "";
		
        _icons[index].enabled   = true;

        // [ADJUST SCALE]
        float fThisScale        = 1.0f;
		if(strType.Equals("RoundChoco"))        name = "RoundchocoL1";
		else if(strType.Equals("JamShade"))     {   name = "obstacle_jam";          fThisScale = 1.1f; }
		else if(strType.Equals("RectChoco"))    {   name = "obstacle_wood_01";      fThisScale = 1.2f; }
		else if(strType.Equals("ChocoBar"))     name = "Panel_chocobar1x1";		
		else if(strType.Equals("CottonCandy"))  {   name = "obstacle_monster_01";   fThisScale = 0.8f; }
		else if(strType.Equals("SodaCan"))      {   name = "Sodacan";		        fThisScale = 0.8f; }
		else if(strType.Equals("Zellato"))      name = "Gelato";
		else if(strType.Equals("Potion1"))      {   name = "potion1";               fThisScale = 0.9f; }
		else if(strType.Equals("Potion2"))      {   name = "potion2";               fThisScale = 0.9f; }
		else if(strType.Equals("Potion3"))      {   name = "potion3";               fThisScale = 0.9f; }
        else if(strType.Equals("CurseShade"))   {   name = "obstacle_bgstone_goal"; fThisScale = 1.1f; }
        else if(strType.Equals("CookieJelly"))  {   name = "cookie_jelly2";         fThisScale = 1.2f; }
        else if(strType.Equals("ColorBox"))     {   name = "coloboxB1";             fThisScale = 0.8f; }
        else if(strType.Equals("WaffleCooker")) {   name = "waffle2";               fThisScale = 0.8f; }
        else if(strType.Equals("MudShade"))     {   name = "thorn_mud1";            fThisScale = 0.8f; }
        else if(strType.Equals("Strawberry"))   {   name = "Strawberry";            fThisScale = 1.0f; }
        else if(strType.Equals("SugarBlock"))
        {
            //if(JMFUtils.GM.ColorIndices.Count>0 && 1==(JMFUtils.GM.ColorIndices.Count%2))
            //    name            = _getSugarBlockName(JMFUtils.GM.ColorIndices[0]);
            //else 

            // note : 이 경우는 child 들로만 그린다.
            _icons[index].enabled = false;
        }
        
        //else if(strType.Equals("countRoundChocho"))  icon.spriteName = "RoundchocoL1";

		ChangeIcon(index, name);
		_txtCount[index].text           = count.ToString();
		_txtCount[index].Commit();

		_icons[index].transform.parent.gameObject.SetActive(count > 0);
        _icons[index].transform.localScale  = Vector3.one * _fIconOrgScale * fThisScale;

        //if(count>0 && true==strType.Equals("CurseShade"))
        //    _icons[index].AnimationState.SetAnimation(0, "obstacle_bgstone_dle", true);

        if(strType.Equals("SugarBlock"))
        {
            if(null != _trSugarIconBundle)
                _trSugarIconBundle.gameObject.SetActive( true );

            int maxCount        = 5;
            
            // note : 규칙에 의해 3 ~ 5개 사이만 그리도록 한다.(2개이하 무시, 5개 초과시 5개로 계산)
            List<int> listTargets = new List<int>();
            if(3 == JMFUtils.GM.ColorIndices.Count)
            {
                listTargets.Add(0); listTargets.Add(1); listTargets.Add(4); 
            }
            else if(4 == JMFUtils.GM.ColorIndices.Count)
            {
                listTargets.Add(0); listTargets.Add(1); listTargets.Add(2); listTargets.Add(3); 
            }
            else if(5 <= JMFUtils.GM.ColorIndices.Count)
            {
                listTargets.Add(0); listTargets.Add(1); listTargets.Add(2); listTargets.Add(3); listTargets.Add(4); 
            }
            
            int inc                     = 0;
            for(int u = 0; u < maxCount; ++u)
            {
                SpriteRenderer skAni = _trSugarIconBundle.Find( "Icon"+u ).GetComponent<SpriteRenderer>();
                skAni.enabled           = true;
                if(true == listTargets.Contains(u))
                {
                    //int slotIndex     = skAni.Skeleton.FindSlotIndex("block_normal");
		            //Slot _slot        = skAni.Skeleton.FindSlot("block_normal");
		            //_slot.Attachment  = skAni.Skeleton.GetAttachment(slotIndex, getSugarBlockName(JMFUtils.GM.ColorIndices[inc]));
                    skAni.sprite        = this.GetComponent<SpriteLibrary>().getSpriteByName( getSugarBlockName(JMFUtils.GM.ColorIndices[inc]) );
                    inc++;
                }
                else
                    skAni.enabled       = false;
                    //skAni.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
    
	void AlignCollectsAtCenter(float fDuration=.0f)
    {
        List<GameObject> listActives    = new List<GameObject>();
        for (int k = 0; k < MAX_CONTENT; ++k)
        {
            if (_objGo[k].activeSelf && false==_listFinished.Contains(k))
                listActives.Add(_objGo[k]);
        }
        if(0 == listActives.Count)
            return;

        // Debug.Assert(activeCount>0 && activeCount<=MAX_CONTENT, "CollectAniMission UI Error !!!");
        
        Transform trPosHead     = transform.Find( string.Format("positions/num_{0}", listActives.Count ));
        Debug.Assert(null != trPosHead, "position object should be exist !!!");

        for(int j = 0; j < listActives.Count; ++j)
        {
            Transform trTarget  = trPosHead.Find( string.Format("pos{0}", j) );
            if(null == trTarget)continue;

            if(fDuration <= .0f)
                listActives[j].transform.position   = trTarget.position;
            else
            {
                listActives[j].transform.DOMoveX(trTarget.position.x, fDuration);
                listActives[j].transform.DOMoveY(trTarget.position.y, fDuration);
            }
            listActives[j].transform.localScale      = Vector3.one;

            //if (activeCount>=3 && j>=(activeCount-2))
            //    _objGo[j].transform.localScale  = Vector3.one;
            //else
            //    _objGo[j].transform.localScale  = Vector3.one * 0.85f;
        }
    }

	Tweener AnimateGain (GameObject go)
    {
        if(null==go || true==DOTween.IsTweening(go.transform))
            return null;

        return go.transform.DOScale(Vector3.one * 1.4f, 0.1f).
                    SetLoops(2, LoopType.Yoyo).SetId(go.GetInstanceID());
    }

    public GameObject getIcon(string strType)
	{
		if(missionObjectDict.ContainsKey(strType))
			return missionObjectDict[strType].transform.parent.gameObject;
		
		if(jewelDict.ContainsKey(strType))
			return jewelDict[strType].transform.parent.gameObject;
		
        if(strType == "SugarBlock")
            return _trSugarIconBundle.gameObject;

		return null;
	}

    // idxColor : 0 ~ 7
    public static string getSugarBlockName(int idxColor)
    {
        switch(idxColor+1)
        {
        case (int)LEItem.COLOR.RED:     return "block_red_S";
		case (int)LEItem.COLOR.YELLOW:  return "block_yellow_S";
		case (int)LEItem.COLOR.GREEN:   return "block_green_S";
		case (int)LEItem.COLOR.BLUE:    return "block_blue_S";
		case (int)LEItem.COLOR.PURPLE:  return "block_purple_S";
		case (int)LEItem.COLOR.ORANGE:  return "block_orange_S";
		case (int)LEItem.COLOR.SKYBULE: return "block_skyblue_S";
		case (int)LEItem.COLOR.VIOLET:  return "block_violet_S";
        }
        return null;
    }


    public Sprite getJewelSkinSprite(int idxColor)
    {
        string strName          = getJewelSkinName(idxColor);
        if(strName.Length==0)   return null;

        return this.GetComponent<SpriteLibrary>().getSpriteByName( strName );
    }
    public Sprite getSugarJewelSkinSprite(int idxColor)
    {
        string strName          = getSugarBlockName(idxColor);
        if(strName.Length==0)   return null;

        return this.GetComponent<SpriteLibrary>().getSpriteByName( strName );
    }
    public Sprite getObjectSkinSprite(string strType)
    {
        string strName          = getObjectSkinName(strType);
        if(strName.Length==0)
            return this.GetComponent<SpriteLibrary>().getSpriteByName( strType );

        return this.GetComponent<SpriteLibrary>().getSpriteByName( strName );
    }
}
