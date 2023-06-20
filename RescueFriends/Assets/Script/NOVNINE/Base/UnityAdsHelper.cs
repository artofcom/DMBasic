﻿using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class UnityAdsHelper : MonoBehaviour {

    private const string android_game_id    = "2847479";
    private const string ios_game_id        = "2847480";
    private const string rewarded_video_id  = "rewardedVideo";

    const bool _isTestMode      = false;

	// Use this for initialization
	void Start ()
    {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Initialize()
    {
#if UNITY_ANDROID
       // Advertisement.Initialize(android_game_id, _isTestMode);
#elif UNITY_IOS
        Advertisement.Initialize(ios_game_id, _isTestMode);
#endif
    }

    public void ShowRewardedAd()
    {       
        Debug.Log("Ads 1.");

       // if (Advertisement.IsReady(rewarded_video_id))
        {
            Debug.Log("Ads 2.");

      //      var options = new ShowOptions { resultCallback = HandleShowResult };
 
       //     Advertisement.Show(rewarded_video_id, options);
        }
    }
 
    public void gainRewardAds()
    {
        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Watching ads \n has been successed.";
        data2.emotion           = MessagePopupHandler.EMOTIONS.HAPPY;
        Scene.ShowPopup("MessagePopup", data2, (p) =>
        {
            _ADS_INFO adsInfo   = InfoLoader.getAdsInfo();
            ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
            InventoryItem       invenItem = Context.UncleBill.GetInventoryItemByID( adsInfo.strRewardName );
            if(null != invenItem)   data.strPicName = invenItem.spriteName;
            data.count          = adsInfo.count;
            Scene.ShowPopup("ItemEarnPopup", data, null);

            Root.Data.gameData.SetAdsRewardedTick( DateTime.UtcNow.Ticks  );
            Wallet.Gain(adsInfo.strRewardName, adsInfo.count);

           // FireBaseHandler.update("playerData/m_AdsRewardedTick", DateTime.UtcNow.Ticks);

#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ads_rewarded");
#endif
        });
    }
    
    /*private void HandleShowResult(ShowResult result)
    {
        Debug.Log("Ads 3.  " + result.ToString());

        switch (result)
        {
        case ShowResult.Finished:
            {
                Debug.Log("The ad was successfully shown.");
 
                // to do ...
                // 광고 시청이 완료되었을 때 처리
                gainRewardAds();
                break;
            }
        case ShowResult.Skipped:
            {
                Debug.Log("The ad was skipped before reaching the end.");
 
                // to do ...
                // 광고가 스킵되었을 때 처리
                MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
                data2.isOkOnly      = true;
                data2.strMessage    = "Watching ads \n has been failed.";
                data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
                Scene.ShowPopup("MessagePopup", data2, null);
                break;
            }
        case ShowResult.Failed:
            {
                Debug.LogError("The ad failed to be shown.");
 
                // to do ...
                // 광고 시청에 실패했을 때 처리
                MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
                data2.isOkOnly      = true;
                data2.strMessage    = "Watching ads \n has been failed.";
                data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
                Scene.ShowPopup("MessagePopup", data2, null);
 
                break;
            }
        }
    }*/


}
