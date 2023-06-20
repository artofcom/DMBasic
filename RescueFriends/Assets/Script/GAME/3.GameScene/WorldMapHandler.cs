using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class WorldMapHandler : tk2dScrollHandler {

    //public const int COUNT_TOTAL_CHAPTER       = 15;
    //public const int COUNT_LEVEL_PER_CHAPTER   = 10;

    public class MapTransformData
    {
        public int idxChapter   = 0;
        public GameObject       _chapterRoot = null;
        public List<Transform>  _listLv = new List<Transform>();
    };

    public WorldSceneHandler    _eventReceiver  = null;
    public Transform _trPlayer  = null;

    int _idxCurLevel            = 0;        // 현재 시도한 레벨 - 성공이든 실패든.
    bool _needToMovePlayer      = false;

    //Transform _trCurLvBtn, _trNextLvBtn;    // for moving player.
    tk2dSprite _sprFox          = null;

    List<MapTransformData>      _listMapTrData = new List<MapTransformData>();
    Dictionary<int, Transform>  _dictIdxContentRoot = new Dictionary<int, Transform>();
    bool _needRefreshDict       = true;

    // clear한 level index과 star count.
    //Dictionary<int, int>        _dictIdxLv_countStar = new Dictionary<int, int>();
    int _cntTriedLvGrade        = 0;

    // Use this for initialization
    private void Awake()
    {
        vInitPos                = new Vector2(1.8f, .0f);
    }
    override protected void Start ()
    {
        if(0 == _unusedContentItems.Count)
            base.Start();

        //_sprFox                 = _trPlayer.Find("sprAvatar").GetComponent<tk2dSprite>();
        //StartCoroutine(_coFoxUpdate());
	}

    public void setCurLevelIndex(int idxLevel)
    {
        _idxCurLevel            = idxLevel;
        _cntTriedLvGrade        = Root.Data.gameData.GetGradeLevelByIndex(idxLevel);
    }

    //void _clear()
    //{
    //    _listMapTrData.Clear();
    //}

    override protected void CustomizeListObject( Transform contentRoot, int idxItem )
    {
        // refresh content root dict.
        if(_needRefreshDict)
        {
            if(true == _dictIdxContentRoot.ContainsKey(idxItem))
                _dictIdxContentRoot.Remove(idxItem);
            _dictIdxContentRoot.Add(idxItem, contentRoot);
        }

        float fDir              = -1.0f;
		contentRoot.localPosition = new Vector3(vInitPos.x, vInitPos.y + fDir * idxItem * _fItemStride, 0);
        int idx                 = _toUp ? allItems.Count - idxItem - 1 : idxItem;
        contentRoot.Find("txtId").GetComponent<tk2dTextMesh>().text = idx.ToString();

        Transform trLink        = contentRoot.Find("bg");
        GameObject prefabCH     = Resources.Load( string.Format("worldMap/mapGroup{0:D3}", idx) ) as GameObject;
        GameObject prefabLv     = Resources.Load( "worldMap/dlgLevelPref" ) as GameObject;
        GameObject prefabRescue = Resources.Load( "worldMap/dlgRescue" ) as GameObject;

        if(null!=prefabCH && null!=trLink)
        {
            if(trLink.childCount > 0)
            {
                _removeMapTranformData(trLink.GetChild(0).gameObject);
                DestroyImmediate( trLink.GetChild(0).gameObject );   // Destroy Full Chapter.
            }
            GameObject ch       = Instantiate(prefabCH);    // And create new One.
            ch.transform.SetParent( trLink );
			ch.transform.localPosition  = Vector3.zero;

            MapTransformData mapTrData = new MapTransformData();
            mapTrData.idxChapter= idx;
            mapTrData._chapterRoot = ch;

            #region => REFRESH LEVEL ICON
            for (int q = 0; q < (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER; ++q)// Build All Level Button UIs.
            {
                Transform trMap     = ch.transform.Find("bg");
                trMap.localScale    = new Vector3(1.0f, 1.01f, 1.0f);
                //trMap.GetComponent<tk2dSprite>().color  = new Color(0.9f, 0.9f, 0.9f, 1.0f);

                Transform trBtn = ch.transform.Find( string.Format("btnGroup/btn{0:D2}", q));
                if(null != trBtn)
                {
                    tk2dUIItem  itemBtn= trBtn.GetComponent<tk2dUIItem>();
                    itemBtn.OnReleaseUIItem += _eventReceiver.OnBtnMapClick;

                    Transform   trLvs = trBtn.Find("sprBG");
                    if(0 == trLvs.childCount)
                    {
                        GameObject  dlgLevel        = Instantiate(prefabLv);
                        dlgLevel.transform.SetParent(trLvs);
						//dlgLevel.transform.parent   = trLvs;
                        dlgLevel.transform.localPosition    = new Vector3(0, 0, -2);
                    }
                    mapTrData._listLv.Add( trLvs );
                    _refreshLevel(trLvs, idx*(int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER+q, contentRoot, idxItem);
                }
            }            
            #endregion

            _addMapTransformData(mapTrData);
            
            _refreshDlgMission(idx, ch, prefabRescue );
        }
    }

    void _refreshDlgMission(int idxCh, GameObject ch, GameObject prefabRescue)
    {
        // init rescue system.
        Transform trDlgRescue   = ch.transform.Find("dlgRescue");
        if(null==trDlgRescue)   return;

        const int maxRescue     = 2;    // !!!
        for(int zz = 0; zz < maxRescue; zz++)
        {
            Transform trDlgRec  = trDlgRescue.Find( string.Format("dlgRec{0}", zz));
            if(null==trDlgRec)  break;

            bool isNewOne       = 0 == trDlgRec.childCount;
            GameObject dlgRec   = null;
            if(isNewOne && null!=prefabRescue)
            {
                dlgRec          = Instantiate(prefabRescue);
				dlgRec.transform.SetParent( trDlgRec );
                dlgRec.transform.localPosition  = new Vector3(0, 0, -2);
                dlgRec.transform.localScale     = Vector3.one * 0.85f;
            }
            else if(null != trDlgRec.GetChild(0))
                dlgRec          = trDlgRec.GetChild(0).gameObject;
            else 
                Debug.Assert(false);

            initRescueDlg(idxCh*maxRescue+zz+1, dlgRec.transform);
        }
    }

    public void initRescueDlg(int idDlg, Transform trDlgRescue)
    {
        // noti update.

        bool isRewarded         = InfoLoader.isMissionRewardedById(idDlg);
        trDlgRescue.Find("dlgAll/sprBeforeBG").gameObject.SetActive(!isRewarded);
        trDlgRescue.Find("dlgAll/sprRewardedBG").gameObject.SetActive(isRewarded);

        Transform trTemp        = trDlgRescue.Find("dlgAll/sprTryBG/txtTry");
        if(trTemp)
        {
            tk2dTextMesh txtTemp= trTemp.GetComponent<tk2dTextMesh>();
            txtTemp.text        = string.Format("{0}/3", InfoLoader.getMissionClearedConditionCountById(idDlg));
            txtTemp.Commit();
        }

        trTemp                  = trDlgRescue.Find("dlgAll/btn00/noti");
        if(trTemp)
        {
            bool rewardable     = InfoLoader.checkMissionClearedById(idDlg) && !Root.Data.gameData.isMissionRewarded(idDlg);
            trTemp.gameObject.SetActive( rewardable );
        }

        tk2dUIItem itemBtn      = trDlgRescue.Find("dlgAll/btn00").GetComponent<tk2dUIItem>();
        if(null != itemBtn)
        {
            itemBtn.OnReleaseUIItem -= _eventReceiver.OnRescueClicked;
            itemBtn.OnReleaseUIItem += _eventReceiver.OnRescueClicked;
        }
    }

    void _refreshLevel(Transform trBG, int idxLv, Transform trChapter, int idxScrollItem)
    {
        if(null == trBG)        return;

        _refreshFoxPlayer(trBG, idxLv);

        _refreshLevelIcon(trBG, idxLv);
    }

    void _refreshFoxPlayer(Transform trBG, int idxLv)
    {
        if(_idxCurLevel != idxLv)
            return;

        float fYoffset          = 1.5f;
        Vector3 vMapPos         = trBG.parent.TransformPoint(trBG.localPosition);
        vMapPos                 = _trPlayer.parent.InverseTransformPoint(vMapPos);
        _trPlayer.localPosition = new Vector3(vMapPos.x, vMapPos.y + fYoffset, _trPlayer.localPosition.z);
        _trPlayer.DOKill();
    }

    Transform _findLevelBtn(int idxLevel)
    {
        int idxCh               = idxLevel / (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;
        int idxLvLest           = idxLevel % (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;
        for(int q = 0; q < _listMapTrData.Count; ++q)
        {
            if(_listMapTrData[q].idxChapter == idxCh)
            {
                Debug.Assert(idxLvLest>=0 && idxLvLest<(int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER);

                return _listMapTrData[q]._listLv[idxLvLest];
            }
        }
        Debug.Assert(false);
        return null;
    }

    void _refreshLevelIcon(Transform trBG, int idxLv)
    {
        int idxUnclearTrying    = Root.Data.TotalClearedLevelCount;
        Transform trBase        = trBG.GetChild(0);
        tk2dTextMesh txtId      = trBase.Find("txtNum").GetComponent<tk2dTextMesh>();
        if(null != txtId)
        {
            float fY            = idxUnclearTrying>=idxLv ? 0.15f : -0.21f;
            txtId.gameObject.SetActive( true );// idxUnclearTrying>=idxLv );
            txtId.transform.localPosition   = new Vector3(txtId.transform.localPosition.x, fY, txtId.transform.localPosition.z);
            if(txtId.gameObject.activeSelf)
            {
                txtId.text      = (idxLv+1).ToString();
                txtId.maxChars  = 5;
                txtId.Commit();
            }
        }

        Transform trStars       = trBase.Find("dlgStar");
        if(null != trStars)
        { 
            trStars.gameObject.SetActive( idxUnclearTrying>idxLv );
            if(trStars.gameObject.activeSelf)
            {
                const int       max_star = 3;
                int nGrade      = Root.Data.gameData.GetGradeLevelByIndex(idxLv);
                if(nGrade<=0 || nGrade>max_star)
                    nGrade      = 1;

                for(int z = 0; z < max_star; ++z)
                {
                    int nn      = z + 1;
                    Transform   trStarSet = trStars.Find( nn.ToString() );
                    if(null == trStarSet)
                        continue;
                    trStarSet.gameObject.SetActive( nn==nGrade );
                }
            }
        }        

        // update btn.
        tk2dSprite sprBtn       = trBG.GetComponent<tk2dSprite>();
        string strSprName       = "GreyKeyHole";    // "Blue_level"; "Pink_level"

        if(idxUnclearTrying == idxLv)
            strSprName          = "Pink_level";
        else if(idxUnclearTrying > idxLv)
            strSprName          = "Blue_level";
        sprBtn.spriteName       = strSprName;

        int idData              = InfoLoader.getRescueRewardIdByLevel(idxLv+1);
        // Root.Data.gameData.
        Transform  trMission    = trBase.Find("sprClear");
        if(null != trMission)   trMission.gameObject.SetActive( idData>0 );
        if(trMission.gameObject.activeSelf)
        {
            if( InfoLoader.checkMissionClearedByLevel(idxLv+1) )
                trMission.GetComponent<tk2dSprite>().spriteName = "BoxOpenPNG";
            else 
                trMission.GetComponent<tk2dSprite>().spriteName = "BoxBubblesPNG";
        }
        
    }

    // refresh, re-scroll world chapter by current target level index.
    public void refreshWorld(int idxCurLevel, bool fromGameScene, System.Action complete)
    {
        // test.
        //idxCurLevel = 15;//16;
        //idxTryingLevel = idxCurLevel;

        if(0 == _unusedContentItems.Count)
            base.Start();

        int idxOldLevel         = _idxCurLevel;
        _idxCurLevel            = idxCurLevel;

        // Rebuild map list.
        //SetItemCount( 0 );
        //allItems.Clear();
        //_cachedContentItems.Clear();
        //_unusedContentItems.Clear();
        //base.Start();
        if(0 == allItems.Count)
            SetItemCount( (int)Data.BASIC_INFOS.COUNT_CHAPTER );
        else
        { 
            // force update => unstable ... so 하지 않음.
            //_needRefreshDict        = false;
            //foreach(int key in _dictIdxContentRoot.Keys)
            //{
            //    if(_dictIdxContentRoot[key].gameObject.activeSelf)
            //        CustomizeListObject(_dictIdxContentRoot[key], key);
            //}
            //_needRefreshDict        = true;
        }        
        
        //if(_toUp)               _scrollableArea.Value   = 1.0f;
        //_scrollableArea.Value   = 1.0f;

        int idxUnclearTrying    = Root.Data.TotalClearedLevelCount;
        _needToMovePlayer       = (fromGameScene && idxOldLevel+1==idxUnclearTrying && idxCurLevel==idxUnclearTrying);

        

        int idxCurCh            = idxCurLevel/(int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;        

        float fTotalHeight      = _scrollableArea.ContentLength + _scrollableArea.VisibleAreaLength;
        float fOneChHeight      = fTotalHeight / ((float)Data.BASIC_INFOS.COUNT_CHAPTER);
        float fOneChRate        = fOneChHeight / _scrollableArea.ContentLength;
        float fViewRate         = _scrollableArea.VisibleAreaLength / _scrollableArea.ContentLength;

        float fViewOffset       = (idxCurLevel%(int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER)>=5 ? 0.4f : -0.2f;

            //1.0f / ((float)COUNT_TOTAL_CHAPTER);

        // 0.0f - top. end of world map.
        // 1.0f - start.
        //float 1LvRate          = 1.0f / (float)Root.Data.TotalLevelCount;
        _scrollableArea.Value   = 1.0f - fOneChRate*((float)idxCurCh) - fViewRate*fViewOffset; // 1.0f - ((float)idxCurLevel) * 1LvRate;


        // force update scroll sectors.
        if(true == fromGameScene)
        {
            if(_needToMovePlayer)
                StartCoroutine( _coMovePlayer(complete) );
            else 
                StartCoroutine( _coRepositionPlayer() );
        }
        

        if(null == _sprFox)
            _sprFox             = _trPlayer.Find("sprAvatar").GetComponent<tk2dSprite>();
        //_sprFox.
        //_sprFox.GetComponent<tk2dSpriteAnimator>().Play("run");
        
        //StopCoroutine( "_coFoxUpdate");
        //StartCoroutine( "_coFoxUpdate" );
    }

    IEnumerator _coUpdateLevelIconStar(Transform trBase, int idxLv)
    {
        Transform trStars       = trBase.GetChild(0).Find("dlgStar");
        if(null == trStars)     yield break;
        
        yield return new WaitForSeconds(1.0f);

        const int MAX_GRADE     = 3;
        int nGrade              = Root.Data.gameData.GetGradeLevelByIndex(idxLv);
        // update level to higher grade !
        if(nGrade>0 && nGrade<=MAX_GRADE && _cntTriedLvGrade<nGrade)   
        {
            trStars.gameObject.SetActive( true );
            
            for(int q = 0; q < MAX_GRADE; ++q)
                trStars.Find((q+1).ToString()).gameObject.SetActive(false);

            Transform trStarSet = trStars.Find( nGrade.ToString() );
            trStarSet.gameObject.SetActive( true );
            for(int t = 0; t < trStarSet.childCount; ++t)
                trStarSet.GetChild(t).GetComponent<MeshRenderer>().enabled = false;
            
            for(int t = 0; t < trStarSet.childCount; ++t)
            {
                Transform trStar        = trStarSet.GetChild(t);
                //ParticlePlayer effect   = NNPool.GetItem<ParticlePlayer>("clearStarHit");
		        //effect.Play(trStar.transform.position);
                BlockCrash effect= NNPool.GetItem<BlockCrash>("NormalPieceCrash");
                effect.Play("play", trStar.transform.position, Vector3.one, (int)LEItem.COLOR.YELLOW-1);
                //
                NNSoundHelper.Play("FX_result_star");
                //trBase.DOShakePosition(0.2f, 0.4f, 5);

                yield return new WaitForSeconds(0.3f);

                trStar.GetComponent<MeshRenderer>().enabled = true;
                trStar.GetComponent<tk2dSprite>().spriteName= "star_1";
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator _coRepositionPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        float fYoffset          = 1.5f;
        Transform trCur         = _findLevelBtn(_idxCurLevel);

        yield return StartCoroutine( _coUpdateLevelIconStar(trCur, _idxCurLevel) );

        Vector3 vCurPos         = trCur.parent.TransformPoint(trCur.localPosition);
        vCurPos                 = _trPlayer.parent.InverseTransformPoint(vCurPos);
        _trPlayer.localPosition = new Vector3(vCurPos.x, vCurPos.y + fYoffset, _trPlayer.localPosition.z);

        int idxCh               = _idxCurLevel / (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;
        MapTransformData    data = _findMapTransformData(idxCh);
        _refreshDlgMission(idxCh, data._chapterRoot, null);
    }

    IEnumerator _coMovePlayer(System.Action complete)
    {
        yield return new WaitForSeconds(0.1f);

        float fYoffset          = 1.5f;
        Transform trFrom        = _findLevelBtn(_idxCurLevel-1);
        Transform trTo          = _findLevelBtn(_idxCurLevel);
        
        yield return StartCoroutine( _coUpdateLevelIconStar(trFrom, _idxCurLevel-1) );

        Vector3 vFromPos        = trFrom.parent.TransformPoint(trFrom.localPosition);   // to world.
        vFromPos                = _trPlayer.parent.InverseTransformPoint(vFromPos);     // to local.
        Vector3 vToPos          = trTo.parent.TransformPoint(trTo.localPosition);
        vToPos                  = _trPlayer.parent.InverseTransformPoint(vToPos);
        
        float xScale            = vToPos.x<vFromPos.x ? 1.0f : -1.0f;
        _trPlayer.localScale= new Vector3(xScale, _trPlayer.localScale.y, 1.0f);
        _trPlayer.localPosition = new Vector3(vFromPos.x, vFromPos.y + fYoffset, _trPlayer.localPosition.z);

        yield return new WaitForSeconds(0.2f);

        // hold a little sec.
        while(_eventReceiver.IsSceneEffectPlaying())
        {   yield return null;   }

        Sequence seq            = DOTween.Sequence();
        seq.AppendInterval(0.6f);
        seq.Append( _trPlayer.DOLocalMoveY(1.0f, 0.2f).SetRelative(true).SetLoops(2, LoopType.Yoyo) );
        seq.Append( _trPlayer.DOLocalMove(new Vector3(vToPos.x, vToPos.y+fYoffset, _trPlayer.localPosition.z), 0.8f) );
        seq.OnComplete( () => complete() );

        //DOVirtual.DelayedCall(1.0f, () =>
        //{
            //trFox.DOLocalMoveY(0.3f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        //});

        _needToMovePlayer   = false;

        _refreshLevelIcon(trFrom, _idxCurLevel-1);
        _refreshLevelIcon(trTo, _idxCurLevel);
        // 2 more update for mission.
        int id                  = InfoLoader.getRescueRewardIdByLevel(_idxCurLevel);
        if(id > 0)
        {
            int idxCh           = _idxCurLevel / (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;
            MapTransformData    data = _findMapTransformData(idxCh);
            _refreshDlgMission(idxCh, data._chapterRoot, null);

            /*_RESCUE_REWARD_INFO info2 = new _RESCUE_REWARD_INFO();
            InfoLoader.GetRescueRewardInfoById(id, ref info2);
            if(info2.nMapId2 == _idxCurLevel)
            {
                Transform trLv  = _findLevelBtn(info2.nMapId1-1);
                if(null!=trLv)  _refreshLevelIcon(trLv, info2.nMapId1-1);
                trLv            = _findLevelBtn(info2.nMapId0-1);
                if(null!=trLv)  _refreshLevelIcon(trLv, info2.nMapId0-1);

                int idxCleared  = _idxCurLevel - 1;
                int idxCh       = idxCleared / COUNT_LEVEL_PER_CHAPTER;
                MapTransformData data = _findMapTransformData(idxCh);
                if(null != data)
                    _refreshDlgMission(idxCh, data._chapterRoot, null);
                else 
                    Debug.Assert(false);
            }*/
        }
    }
    MapTransformData _findMapTransformData(int idxCh)
    {
        // check duplicate.
        for(int z = 0; z < _listMapTrData.Count; ++z)
        {
            if(_listMapTrData[z].idxChapter == idxCh)
            {
                return _listMapTrData[z];
            }
        }
        return null;
    }

    void _addMapTransformData(MapTransformData data)
    {   
        // check duplicate.
        for(int z = 0; z < _listMapTrData.Count; ++z)
        {
            if(_listMapTrData[z].idxChapter == data.idxChapter)
            {
                Debug.Log(string.Format("{0} Chapter Tr Data Removed.", _listMapTrData[z].idxChapter));
                _listMapTrData.RemoveAt(z);
                break;
            }
        }
        Debug.Log(string.Format("{0} Chapter Tr Data Added.",  data.idxChapter));
        _listMapTrData.Add( data );
    }

    void _removeMapTranformData(GameObject objChRoot)
    {
        for(int z = 0; z < _listMapTrData.Count; ++z)
        {
            if(_listMapTrData[z]._chapterRoot.Equals(objChRoot))
            {
                Debug.Log(string.Format("{0} Chapter Tr Data Removed.", _listMapTrData[z].idxChapter));
                _listMapTrData.RemoveAt(z);
                break;
            }
        }
    }

#region ButtonHandlers
	int numToAdd = 10;

	// Event handler for "Add more..." button
	void AddMoreItems() {
		SetItemCount(allItems.Count + Random.Range(numToAdd / 10, numToAdd));
		numToAdd *= 2;
	}
	// Event handler for "Reset" button
	void ResetItems() {
		numToAdd = 100;
		SetItemCount(3);
	}
#endregion
}
