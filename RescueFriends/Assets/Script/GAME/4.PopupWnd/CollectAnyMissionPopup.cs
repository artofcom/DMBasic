using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using Spine.Unity;
using NOVNINE;

// [ROUND_CHOCO]
public class CollectAnyMissionPopup : MonoBehaviour 
{
    public static int MAX_CONTENT   = 5;
    
    public Transform            _trSugarIconBundle;

    GameObject[] _objGo         = new GameObject[MAX_CONTENT];
	SpriteRenderer[] _icons     = new SpriteRenderer[MAX_CONTENT];
    tk2dTextMesh[] _txtCount    = new tk2dTextMesh[MAX_CONTENT];

	Dictionary<string, tk2dTextMesh> jewelDict = new Dictionary<string, tk2dTextMesh>();
    Dictionary<string, tk2dTextMesh> missionObjectDict = new Dictionary<string, tk2dTextMesh>();

    float _fIconOrgScale        = .0f;

	void Awake ()
	{
		for(int k = 0; k < MAX_CONTENT; ++k)
		{
			_objGo[k]           = transform.Find( string.Format("Content/{0}", k+1 )).gameObject;
			_icons[k]           = _objGo[k].transform.Find("dlgIcon/Icon").GetComponent<SpriteRenderer>();
			//_icons[k].Initialize(true);
			//_icons[k].gameObject.SetActive( false );
			_txtCount[k]        = _objGo[k].transform.Find("Count").GetComponent<tk2dTextMesh>();
			_txtCount[k].gameObject.SetActive( false );
			_objGo[k].SetActive( false );
            if(.0f == _fIconOrgScale)
                _fIconOrgScale  = _icons[k].transform.localScale.x;
		}
	}
	
    //public void refreshUI(Data.Level levelData)
    //{
    //}

    void _deInit()
    {
        jewelDict.Clear();
        //sjDict.Clear();
        missionObjectDict.Clear();

        for(int k = 0; k < MAX_CONTENT; ++k)
        {
            _icons[k].gameObject.SetActive( true );
            //_icons[k].Initialize(true); 
            
            _txtCount[k].gameObject.SetActive( true );
			_objGo[k].SetActive( false );
        }
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

        AlignCollectsAtCenter();
    }

    void _updateMissionObject(ref int nTarget, string strType, int count)
    {
        if(count <= 0)          return;
        _objGo[nTarget].SetActive( true );
        missionObjectDict.Add(strType, _txtCount[nTarget]);

        UpdateMissioinObjects( nTarget, strType, count);
        ++nTarget;
    }

    void OnDisable () {
        
        if(null!=_trSugarIconBundle)
            _trSugarIconBundle.gameObject.SetActive( false );

        _deInit();     
    }
	
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
	
	void UpdateMissioinObjects(int index, string strType, int count)
    {
		string name = "";
		
        _icons[index].enabled = true;

        // [ADJUST SCALE]
        float fThisScale        = 1.0f;
		if(strType.Equals("RoundChoco"))        name = "RoundchocoL1";
		else if(strType.Equals("JamShade"))     {   name = "obstacle_jam";          fThisScale = 1.1f; }
		else if(strType.Equals("RectChoco"))    {   name = "obstacle_wood_01";      fThisScale = 1.2f; }
		else if(strType.Equals("ChocoBar"))     name = "Panel_chocobar1x1";		
		else if(strType.Equals("CottonCandy"))  {   name = "obstacle_monster_01";   fThisScale = 0.8f; }
		else if(strType.Equals("SodaCan"))      {   name = "Sodacan";		        fThisScale = 0.9f; }
		else if(strType.Equals("Zellato"))      name = "Gelato";
		else if(strType.Equals("Potion1"))      {   name = "potion1";               fThisScale = 0.9f; }
		else if(strType.Equals("Potion2"))      {   name = "potion2";               fThisScale = 0.9f; }
		else if(strType.Equals("Potion3"))      {   name = "potion3";               fThisScale = 0.9f; }
        else if(strType.Equals("CurseShade"))   {   name = "obstacle_bgstone_goal"; fThisScale = 1.0f; }
        else if(strType.Equals("CookieJelly"))  {   name = "cookie_jelly2";         fThisScale = 1.2f; }
        else if(strType.Equals("ColorBox"))     {   name = "coloboxB1";             fThisScale = 0.8f; }
        else if(strType.Equals("WaffleCooker")) {   name = "waffle2";               fThisScale = 0.8f; }
        else if(strType.Equals("MudShade"))     {   name = "thorn_mud1";            fThisScale = 0.8f; }
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
                SpriteRenderer skAni    = _trSugarIconBundle.Find( "Icon"+u ).GetComponent<SpriteRenderer>();
                skAni.enabled           = true;
                if(true == listTargets.Contains(u))
                {
                    //int slotIndex       = skAni.Skeleton.FindSlotIndex("block_normal");
		            //Slot _slot          = skAni.Skeleton.FindSlot("block_normal");
		            //_slot.Attachment    = skAni.Skeleton.GetAttachment(slotIndex, getSugarBlockName(JMFUtils.GM.ColorIndices[inc]));
                    skAni.sprite        = this.GetComponent<SpriteLibrary>().getSpriteByName( getSugarBlockName(JMFUtils.GM.ColorIndices[inc]) );
                    inc++;
                }
                else skAni.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
    
	void AlignCollectsAtCenter(float fDuration=.0f)
    {
        List<GameObject> listActives    = new List<GameObject>();
        for (int k = 0; k < MAX_CONTENT; ++k)
        {
            if(true == _objGo[k].activeSelf)
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
		DOTween.Complete(go.GetInstanceID());
		TweenParams parms = new TweenParams();
		parms.SetId(go.GetInstanceID());
		parms.SetEase(Ease.InOutQuad);
		parms.SetLoops(2, LoopType.Yoyo);
		return go.transform.DOScale(Vector3.one * 1.5f, 0.2f);
    }

    public GameObject getIcon(string strType)
	{
		if(missionObjectDict.ContainsKey(strType))
			return missionObjectDict[strType].transform.parent.gameObject;
		
		if(jewelDict.ContainsKey(strType))
			return jewelDict[strType].transform.parent.gameObject;
		
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
}
