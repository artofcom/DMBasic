using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;
//using Facebook.Unity;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif


public class WorldSceneHandler : MonoBehaviour, IGameScene
{
    public class _paramInfo
    {
        public int idxCurrent   = 0;
        public bool isFromInGame= false;
    };

    public WorldMapHandler      _worldMapHdr    = null;
    public SpriteRenderer       _sprFader       = null;

    int _idCurMission           = -1;
    Transform _trCurMissionDlg  = null;

    int _idxMapTarget           = -1;
    bool _isMissionClearedWhenEnter= false;
    bool _isSceneEffectPlaying  = false;
    public bool IsSceneEffectPlaying()
    {
        return _isSceneEffectPlaying;
    }

    void Awake()
    {
        _sprFader.gameObject.SetActive( true );
        _sprFader.color         = new Color(0, 0, 0, 255);      

#if UNITY_EDITOR
        if(true == LevelEditorSceneHandler.EditorMode)
            _sprFader.gameObject.SetActive( false );
#endif
        
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

	public void OnEnterScene (object param) 
	{
        Debugger.Assert(param is _paramInfo, "WorldSceneHandler.OnEnterScene : Param is wrong.");
        Scene.ShowOverlay("WorldOverlay");

        //Level targetLevel       = param as Level;
        //Level targetLevel       = Root.Data.GetLevelFromIndex(5);
        //int idxLevel            = targetLevel.Index;

        _paramInfo info         = param as _paramInfo;
        _worldMapHdr.refreshWorld(info.idxCurrent, info.isFromInGame, _openNextLevel);
        _idxMapTarget           = info.idxCurrent;

        if(false == info.isFromInGame)
            StartCoroutine( _coCheckDailyAttend() );
        else
            StartCoroutine( _coCheckPopups(info.idxCurrent) );

        if(false == NNSound.Instance.IsPlayingBGM())
            NNSoundHelper.PlayBGM("bgm_title"); 


         // test.
        //FireBaseHandler.update("RewardedMissionIds0", 3);
        //if(null==Root.Data.gameData.record.RewardedMissionIds || Root.Data.gameData.record.RewardedMissionIds.Length==0)
        //{
        //    Root.Data.gameData.addRewardedMission(3);
        //    Root.Data.gameData.addRewardedMission(6);
        //   Root.Data.gameData.SaveContext();
        //}
    }


    public void OnLeaveScene () 
	{
        Scene.CloseOverlay("WorldOverlay");
        //JMFRelay.OnGameOver -= OnGameOver;
    }
    
	public void OnEscape () 
	{
        // ask app quit.
        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = false;
        data2.strMessage        = "Wanna quit this game?";
        data2.emotion           = MessagePopupHandler.EMOTIONS.ON_QUIT;
        Scene.ShowPopup("MessagePopup", data2, (param) =>
        {
            if(null != param)
            {
                MessagePopupHandler.RET ret = (MessagePopupHandler.RET)param;
                if(ret == MessagePopupHandler.RET.OK)
                    Application.Quit();
            }
        });

    }    

    public void DoDataExchange()
    { }

    public void OnClickBottomMission(tk2dUIItem item)
    {
        int id_rewardable       = InfoLoader.getFirstMissionRewardIdToReward();
        int id_mission          = id_rewardable>0 ? id_rewardable : InfoLoader.getRescueRewardIdByLevel(_idxMapTarget+1);
        if (id_mission <= 0)
        {
            int idxCh           = _idxMapTarget / (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;
            int idx             = _idxMapTarget >= ((int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER/2) ? 1 : 0;
            id_mission          = idxCh*2 + idx+1;
            _idCurMission       = id_mission;
        }

        
        Scene.ShowPopup("RescuePopup", id_mission, (nMapIdParam) =>
        {
            int nMapId          = (int)nMapIdParam;
            if(nMapId > 0)
            {
                _idxMapTarget   = nMapId-1;
                _onBtnMapClick();
            }

            // don't have taget, so dont refresh ui.
            // ==> _worldMapHdr.initRescueDlg(_idCurMission, _trCurMissionDlg);
        });
    }

    public void OnRescueClicked(tk2dUIItem item)
    {
        int idx                 = -1;
        int idxCh               = -1;
        const int maxTry        = 50;
        Transform trSome        = item.transform.parent;
        for(int q = 0 ; q < maxTry; ++q)
        {
            if(trSome.name.Contains("dlgRec0") || trSome.name.Contains("dlgRec1"))
            {
                string itsName  = trSome.name;
                itsName         = itsName.Remove(0, 6);
                idx             = int.Parse( itsName );
                _trCurMissionDlg= trSome.GetChild(0);
            }
            else if(trSome.name.Contains("mapGroup"))
            {
                string strNums  = "000";        // 000 ~ 999
                string chKey    = "mapGroup";
                string strClone = "(Clone)";
                string strChName= trSome.name;  // mapGroup000(Clone)
                                                        // mapGroup000
                strChName       = strChName.Remove(chKey.Length+strNums.Length, strClone.Length);
                                                        // 000
                string strChapterIndex  = strChName.Remove(0, chKey.Length );
                idxCh           = int.Parse(strChapterIndex);
            }

            trSome              = trSome.parent;
            if(null==trSome || (idx>=0 && idxCh>=0))
                break;
        }

        int id_mission          = idxCh*2+idx+1;
        _idCurMission           = id_mission;

        Scene.ShowPopup("RescuePopup", id_mission, (nMapIdParam) =>
        {
            int nMapId          = (int)nMapIdParam;
            if(nMapId > 0)
            {
                _idxMapTarget   = nMapId - 1;
                _onBtnMapClick();
            }

            _worldMapHdr.initRescueDlg(_idCurMission, _trCurMissionDlg);
        });
    }

    IEnumerator _coCheckDailyAttend()
    {
        yield return new WaitForSeconds(1.0f);

         do{
            yield return null;
        }while(false==Context.UncleBill.IsComplete());

        // check daily reward.
        const int NUM_DAILY_REWARD = 10;
        DateTime dtRewarded     = new DateTime( Root.Data.gameData.GetDailyRewardDate() );
        DateTime dtNow          = DateTime.UtcNow;
        
        // 지난 수령과 같은 날.
        if(dtRewarded.Year==dtNow.Year && dtRewarded.DayOfYear==dtNow.DayOfYear)
        {
            // Already Received !!!
            Debug.Log("You've alreay received today's reward.");
        }
        else
        {
            //===============================================================//
            //
            // Abusing check.
            // note : 지난수령 시간이 현재 시간보다 미래라면, 아무 조치 없음.
            //        => 날짜 변경으로 미리 받기는 가능하나, 중복 받기는 절대 불가.
            if(dtNow.Ticks < dtRewarded.Ticks)
            {
                Debug.Log("You've Aboused Daily Attend Event !");
                yield break;
            }

            // 
            // note : 연속 출석이 아니면 리셋.
            DateTime dtNextDay      = dtRewarded.AddDays(1);
            int nSeqDay             = 1;
            if(dtNextDay.Year==dtNow.Year && dtNextDay.DayOfYear==dtNow.DayOfYear)
                nSeqDay             = (Root.Data.gameData.GetDailyRewardedCount()+1) % NUM_DAILY_REWARD;
         
            
            Root.Data.gameData.SetDailyRewardedCount( nSeqDay );
            Root.Data.gameData.SetDailyRewardDate(DateTime.UtcNow.Ticks);

            Root.Data.gameData.SaveContext();
  /*          FireBaseHandler.update("playerData/m_slDailyRewardedCount", nSeqDay);
            FireBaseHandler.update("playerData/m_DailyRewardDate", DateTime.UtcNow.Ticks, true, () =>
            {
                Scene.ShowPopup("DailyPopup", null, (param_daily) => 
                {
                    // 오늘 reward 수령 처리.
                    int idTodaysReward      = Root.Data.gameData.GetDailyRewardedCount();
                    _DAILY_REWARD_INFO      outInfo = new _DAILY_REWARD_INFO();
                    InfoLoader.GetDailyRewardData(idTodaysReward, ref outInfo);

                    Debug.Log("== Daily Reward Count : " + idTodaysReward);

#if !UNITY_EDITOR
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("daily_rewarded", "sDay", idTodaysReward.ToString());
#endif

                    Debug.Log("== Reward item Name : " + outInfo.strRewardName);

                    ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
                    NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( outInfo.strRewardName );
                    if(null != invenItem)   data.strPicName = invenItem.spriteName;
                    data.count              = outInfo.count;

                    Debug.Log("== Reward pic file : " + invenItem.spriteName);

                    DOVirtual.DelayedCall(0.01f, () =>
                    {
                        Scene.ShowPopup("ItemEarnPopup", data, (param) =>
                        {
                            Wallet.Gain(outInfo.strRewardName, outInfo.count);                        
                        });
                    });
                });
            });*/
        }
    }

    IEnumerator _coCheckPopups(int idxLv)
    {
        //yield return StartCoroutine( _coCheckRescueReward(idxLv) );

        yield break;

        // ...
    }

    IEnumerator _coCheckRescueReward(int idxLv)
    {
        yield break;
        /*
        // if this one is new-cleared map, then cleared one is idxLv-1.
        if(idxLv == Root.Data.idxMaxClearedLevel+1)
            --idxLv;

        bool ret                = InfoLoader.checkMissionClearedByLevel(idxLv+1);
        if(false==_isMissionClearedWhenEnter && true== ret)
        {
            _isSceneEffectPlaying= true;

            // Need Obtain event !!!
            Debug.Log("New Mission Cleared !!! => " + (idxLv+1).ToString() + " Level");

            // 보상 수령.
            _RESCUE_REWARD_INFO info2 = new _RESCUE_REWARD_INFO();
            int id              = InfoLoader.getRescueRewardIdByLevel(idxLv+1);
            InfoLoader.GetRescueRewardInfoById(id, ref info2);
            
            ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( info2.strItemName );
            if(null != invenItem)   data.strPicName = invenItem.spriteName;
            data.count              = info2.count;

            bool allOk              = false;
            DOVirtual.DelayedCall(0.3f, () =>
            {
                Scene.ShowPopup("ItemEarnPopup", data, (paramsss) =>
                {
                    Wallet.Gain(info2.strItemName, info2.count);
                    //Root.Data.gameData.addClearedMission(info2.id);
                    allOk           = true;
                });
            });
                
            while(allOk==false)
            {
                yield return null;
            };             
        }

        _isSceneEffectPlaying       = false;
        _isMissionClearedWhenEnter  = false;
        */
    }

    public void OnBtnMapClick(tk2dUIItem item)
    {
        Debug.Log(item.name);

        Transform trGroup       = item.transform.parent;
        if(null == trGroup)     return;

        trGroup                 = trGroup.parent;
        if(null == trGroup)     return;

        string strNums          = "000";        // 000 ~ 999
        string chKey            = "mapGroup";
        string strClone         = "(Clone)";
        string strChName        = trGroup.name; // mapGroup000(Clone)
                                                // mapGroup000
        strChName               = strChName.Remove(chKey.Length+strNums.Length, strClone.Length);
                                                // 000
        string strChapterIndex  = strChName.Remove(0, chKey.Length );
        int chapterIdx          = int.Parse(strChapterIndex);

        string lvKey            = "btn";
        string strBtnName       = item.name;    // btn00 ~ btn09
        int lvIndex             = int.Parse( strBtnName.Remove(0, lvKey.Length) );

        int idxTarget           = lvIndex + (int)BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER*chapterIdx;// 0 ~ 9999

        int idxTrying           = Root.Data.TotalClearedLevelCount;

        if(idxTarget > idxTrying)
            return;

        this._idxMapTarget      = idxTarget;

        if(Wallet.GetItemCount("life") <= 0)
        {
            Scene.ShowPopup("BuyItemPopup", "life", (param2) =>
            {
                if(Wallet.GetItemCount("life") > 0)
                    _onBtnMapClick();
            });
        }
        else _onBtnMapClick();
    }

    void _openNextLevel()
    {
        _onBtnMapClick();
    }

    void _onBtnMapClick()
    { 
        int idxTarget           = _idxMapTarget;
        if(idxTarget>=0 && idxTarget<Root.Data.TotalLevelCount)
        {
            PlayReadyInfo info  = new PlayReadyInfo();
            info.InGame         = false;
            info.currentLevelIndex = idxTarget;

            Scene.ShowPopup("PlayReadyPopup", info, (param) =>
            {
                Data.Level lv   = Root.Data.GetLevelFromIndex( _idxMapTarget );

                switch((READY_RESULT)param)
                {
                case READY_RESULT.PLAY:
#if !UNITY_EDITOR
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("level_enter", "sEnterLvId", (_idxMapTarget+1).ToString());
#endif

                    _isMissionClearedWhenEnter = InfoLoader.checkMissionClearedByLevel(lv.Index+1);
                    _worldMapHdr.setCurLevelIndex( lv.Index );
                    Scene.ChangeTo("PlayScene", lv);
                    break;
                default:
                case READY_RESULT.CLOSE:
                    break;
                }
            });
        }
    }
}
