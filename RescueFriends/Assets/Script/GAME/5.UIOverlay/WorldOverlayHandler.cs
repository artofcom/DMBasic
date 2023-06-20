using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Store;
using NOVNINE.Diagnostics;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class WorldOverlayHandler : MonoBehaviour, IUIOverlay {

    public tk2dTextMesh         _txtLifeCount   = null;
    public tk2dTextMesh         _txtLifeTime    = null;
    public tk2dTextMesh         _txtRubbyCount  = null;
	public tk2dTextMesh         _txtAdsCoolTime = null;

    public GameObject           _objNotiOnAds   = null;
    public GameObject           _objNotiOnMission= null;
    public GameObject           _objNotiOnSpin  = null;

    bool _isAdsWatchable        = false;

    void Awake () 
	{}
	
	void Start()
	{}
	
    public void DoDataExchange () {}
	
	public GameObject GetGameObject()
	{
		return gameObject;
	}

    // note : 최적화 보다는 가독성 위주로 간결하게 코드를 만듦.
    IEnumerator _updateTimmer()
    {
        int cntRubby                = Wallet.GetItemCount("coin");
        string strTimeText          = "";
        
        do
        {
            if(null == Root.Data)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            #region => LIFE

            // refresh life & time ui.
            int cntLifeNow          = Wallet.GetItemCount("life");
            if(cntLifeNow >= Data.GameData.CHARGEABLE_MAX_STAMINA)
                strTimeText         = "FULL";
            else
            {
                DateTime lastUsedT  = new DateTime( Root.Data.gameData.GetStaminaChargeTick() );
                long timeGap        = DateTime.UtcNow.Ticks - lastUsedT.Ticks;

                int count           = (int)(timeGap / Data.GameData.CHARGE_TIME);
                timeGap             = timeGap - (Data.GameData.CHARGE_TIME*count);// ) / Data.GameData.TICK_DETAIL;

                // 00:00
                timeGap             = Data.GameData.CHARGE_TIME - timeGap;
                timeGap             = timeGap / Data.GameData.TICK_DETAIL;
                Debug.Log( string.Format( "TimeGap : {0}...{1}", timeGap, count));
                int min             = (int)(timeGap / 60);
                int sec             = (int)timeGap - min*60;
                strTimeText         = string.Format("{0:D2}:{1:D2}", min, sec);
            }

            _txtLifeCount.text      = cntLifeNow.ToString();
            _txtLifeCount.Commit();
            _txtLifeTime.text       = strTimeText;
            _txtLifeCount.Commit();

            #endregion


            #region => ADS
            {
                _isAdsWatchable     = true;
                DateTime lastUsedT  = new DateTime( Root.Data.gameData.GetAdsRewardedTick() );
                long fillTimeT      = lastUsedT.Ticks + (long)InfoLoader.getAdsInfo().cool_time_min*60*Data.GameData.TICK_DETAIL;

                long timeGap        = 0;
                strTimeText         = "";
                if(fillTimeT >= DateTime.UtcNow.Ticks)
                {
                    _isAdsWatchable = false;
                    timeGap         = fillTimeT - DateTime.UtcNow.Ticks;// lastUsedT.Ticks;
                    timeGap         = timeGap / Data.GameData.TICK_DETAIL;
                
                    int hour        = (int)(timeGap / (60*60));
                    timeGap         = timeGap - hour*(60*60);
                    int min         = (int)(timeGap / 60);
                    int sec         = (int)timeGap - min*60;
                    if(hour > 0)    strTimeText     = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
                    else            strTimeText     = string.Format("{0:D2}:{1:D2}", min, sec);
                }
            }

            _txtAdsCoolTime.text    = strTimeText;
            _txtAdsCoolTime.Commit();

            #endregion

            if (cntRubby != Wallet.GetItemCount("coin"))
            {
                cntRubby            = Wallet.GetItemCount("coin");
                _txtRubbyCount.text = string.Format("{0:#,###0}", cntRubby);
                _txtRubbyCount.Commit();
            }

            _refreshNoti();

            yield return new WaitForSeconds(1.0f);

        }while(true);

    }

    public void OnEnterUIOveray (object param) 
	{
        MessagePopupHandler._titleOverlay= null;

     //   JMFRelay.OnGameReady += OnGameReady;
        StartCoroutine( _updateTimmer() );

        _refreshUI();
    }

    public void OnLeaveUIOveray () {
        //JMFRelay.OnGameReady -= OnGameReady;
        StopAllCoroutines();
    }


    void _refreshUI()
    {
        // update rubby.
        //this._txtRubbyCount.text    = Context.UncleBill.GetInventoryItemByID("coin").count.ToString();

        _txtRubbyCount.text     = string.Format("{0:#,###0}", Wallet.GetItemCount("coin"));
        _txtRubbyCount.Commit();
    }

    void OnClickPlay (tk2dUIItem item)
    {
        //string strPath  = "Assets/Data/text/"+Root.GetPostfix()+"/split";
        //int curLevel    = 1;
        //string fullpath = string.Format("{0}/{1}.txt", strPath, curLevel);
        //Data.Level lv   = JsonFx.Json.JsonReader.Deserialize<Data.Level>(System.IO.File.ReadAllText(fullpath));
		//Scene.ChangeTo("PlayScene", lv);
    }
    void OnClickSpinWheel(tk2dUIItem item)
    {
        Scene.ShowPopup("WheelPopup");
    }
    void OnClickOption(tk2dUIItem item)
    {
        Scene.ShowPopup("OptionPopup", null, (isExitToTitle) =>
        {
            if((bool)isExitToTitle)
            {
                Scene.ClearAll();
                DOTween.KillAll();
                SceneManager.LoadScene("Title");
            }
        });

        //Wallet.Gain("coin", 10000);
    }
    void OnClickDailyReward(tk2dUIItem item)
    {
        Scene.ShowPopup("DailyPopup");
    }
    void OnClickAddHeart(tk2dUIItem item)
    {
        //Scene.ShowPopup("AddHeartPopup");
        Scene.ShowPopup("BuyItemPopup", "life", (param) => {});
    }
    void OnClickAddRubby(tk2dUIItem item)
    {
        Scene.ShowPopup("BuyRubyPopup");
    }
   
    void OnClickShowAds(tk2dUIItem item)
    {
        //DateTime lastUsedT      = new DateTime( Root.Data.gameData.GetAdsRewardedTick() );
        //long fillTimeT          = lastUsedT.Ticks + (long)InfoLoader.getAdsInfo().cool_time_min*60*Data.GameData.TICK_DETAIL;
        
        if(false == _isAdsWatchable)    // fillTimeT >= DateTime.UtcNow.Ticks)
        {
            MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
            data2.isOkOnly      = true;
            data2.strMessage    = "Please try this later.";
            data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
            Scene.ShowPopup("MessagePopup", data2, null);
            return;
        }

#if UNITY_EDITOR
        Scene.ShowPopup("LoadingPopup");
        DOVirtual.DelayedCall(2.0f, () =>
        {
            Scene.ClosePopup();
            GetComponent<UnityAdsHelper>().gainRewardAds();
        });
#else
        GetComponent<UnityAdsHelper>().ShowRewardedAd();
#endif
    }

    // noti check.
    void _refreshNoti()
    {
        // ads btn noti.
        _objNotiOnAds.SetActive( _isAdsWatchable );

        // mission btn noti.
        _objNotiOnMission.SetActive( InfoLoader.getFirstMissionRewardIdToReward() > 0 );
        
        // free spin's noti.
        _objNotiOnSpin.SetActive( WheelPopupHandler.isFreeSpinUsable() );
    }
}
