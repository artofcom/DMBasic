using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;


// Note : 로비로 stage 종료 이후 체크하여 보상 자동 지급.
//
public class RescuePopupHandler : BasePopupHandler 
{
    public tk2dUIItem _btnClose = null;

    public tk2dSprite _sprItem  = null;
    public tk2dTextMesh _txtCount= null;
    public tk2dTextMesh _txtReward= null;
    public tk2dTextMesh _txtConditionCount= null;
    public tk2dUIItem   _btnAskReward = null;
    public Transform _trList0   = null;
    public Transform _trList1   = null;
    public Transform _trList2   = null;
    public Transform _trFriends = null;

    int _id                     = 0;
    int _idMap0, _idMap1, _idMap2;

    protected override void  OnEnter (object param) 
	{	
        _id                     = (int)param;
		base.OnEnter(param);
        _refreshUI();
    }

    void _receiveReward()
    {
        // dbouble check 
        bool cleared        = InfoLoader.checkMissionClearedById(_id);
        bool rewarded       = InfoLoader.isMissionRewardedById(_id);
        if (cleared && !rewarded)
        {
            _RESCUE_REWARD_INFO info2 = new _RESCUE_REWARD_INFO();
            InfoLoader.GetRescueRewardInfoById(_id, ref info2);
            
            ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( info2.strItemName );
            if(null != invenItem)   data.strPicName = invenItem.spriteName;
            data.count              = info2.count;
        
            NOVNINE.Wallet.Gain(info2.strItemName, info2.count);

            Root.Data.gameData.addRewardedMission(_id);

            string strBuffer    = InfoLoader.buildRMissionData();
            /*FireBaseHandler.update("RMission", strBuffer, true, () =>
            {
                Scene.ShowPopup("ItemEarnPopup", data, (param) =>
                {
                    _refreshUI();
                });
            });*/
        }
        else
        {
            Debug.Log("Mission Reward Button Activity Error !!!");
        }
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);

        if (_btnClose == item)
            Scene.ClosePopup(0);
        else if (_btnAskReward == item)
            _receiveReward();
        else
        {
            Transform trLevel   = item.transform.parent.parent;
            if(trLevel == _trList0)
                Scene.ClosePopup( _idMap0);
            else if(trLevel == _trList1)
                Scene.ClosePopup( _idMap1);
            else if(trLevel == _trList2)
                Scene.ClosePopup( _idMap2);
        }
	}

    void _refreshUI()
    {
        _RESCUE_REWARD_INFO info = new _RESCUE_REWARD_INFO();
        InfoLoader.GetRescueRewardInfoById(_id, ref info);

        if(0 == info.id)        return;

        bool rewarded           = InfoLoader.isMissionRewardedById(_id);
        bool missionCleared     = InfoLoader.checkMissionClearedById(_id);

        NOVNINE.InventoryItem  invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID(info.strItemName);
        if(null != invenItem)
            _sprItem.spriteName= invenItem.spriteName;
        
        _txtReward.text         = rewarded ? "Rewarded" : "Reward";
        _txtReward.Commit();

        _txtConditionCount.text = string.Format("{0}/3", InfoLoader.getMissionClearedConditionCountById(_id));
        _txtConditionCount.Commit();

        JMFUtils.interactTk2dButton(missionCleared&&!rewarded, _btnAskReward, _btnAskReward.transform.Find("bg") );

        _sprItem.color          = rewarded ? Color.grey : Color.white;

        _txtCount.text          = info.count.ToString();
        _txtCount.Commit();

        _refreshMapListUI(_trList0, info.nMapId0);
        _refreshMapListUI(_trList1, info.nMapId1);
        _refreshMapListUI(_trList2, info.nMapId2);

        _idMap0                 = info.nMapId0;
        _idMap1                 = info.nMapId1;
        _idMap2                 = info.nMapId2;

        _trFriends.Find("sprTiger").gameObject.SetActive(!rewarded);
        _trFriends.Find("sprFriends").gameObject.SetActive(rewarded);

        {
            //bool cleared        = info.nMapId0 <= Root.Data.TotalClearedLevelCount;
        }
    }


    // case 0. no data.                             - no display.
    // case 1. clear with 3 star                    - display cleared.
    // case 2. clear with 2 or 1 star               - show play button.
    // case 3. uncleared and it's not next stage.   - show play button and disabled it.
    // case 4. uncleared and it's just next stage.  - show play button.
    void _refreshMapListUI(Transform trList, int mapId)
    {
        trList.gameObject.SetActive( mapId>0 );     // case 0.
        if(false == trList.gameObject.activeSelf)
            return;

        Transform trBtn         = trList.Find("bg/btnPlay");
        if(null != trBtn)       trBtn.gameObject.SetActive( true );

        bool cleared            = mapId <= Root.Data.TotalClearedLevelCount;

        int cntStar             = Root.Data.gameData.GetGradeLevelByIndex(mapId-1);
        for(int z = 0; z < 3; ++z)
        {
            Transform trStar    = trList.Find( string.Format("bg/dlgStars/sprStar{0}", z+1));
            if(null != trStar)
                trStar.GetComponent<tk2dSprite>().color = (cleared && z+1<=cntStar) ? Color.white : Color.gray;             
        }   

        if(cleared)
        {
            if(null!=trBtn)     trBtn.gameObject.SetActive( 3!=cntStar );
        }
        else
        {
            bool playable       = mapId <= Root.Data.TotalClearedLevelCount+1;
            //if(null!=trBtn)     trBtn.GetComponent<BoxCollider>().enabled   = playable;                    
            Transform trBtnBG   = trList.Find("bg/btnPlay/bg");
            //if(null!=trBtnBG)
            //    trBtnBG.GetComponent<tk2dSlicedSprite>().color = playable ? Color.white : Color.gray;

            JMFUtils.interactTk2dButton(playable, trBtn.GetComponent<tk2dUIItem>(), trBtnBG);
        }

         Transform trLv         = trList.Find( "bg/dlgStars/txtLevel" );
         if(null != trLv)       trLv.GetComponent<tk2dTextMesh>().text  = "Level " + mapId.ToString();// .color = z+1<=cntStar ? Color.white : Color.gray;
    }

}
