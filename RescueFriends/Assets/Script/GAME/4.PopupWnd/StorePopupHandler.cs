using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using NOVNINE;
using NOVNINE.Store;
#if LIVE_MODE
//using Facebook.Unity;
#endif

public enum BUY_RESULT { CANCLE, BUY };
public enum BUY_ITEM_TYPE { MIXED, TRIPLE, SPECIAL, HAMMER, FIRECRACKER, MAGICSWAP, RAINBOWBUST};

public class StorePopupHandler : BasePopupHandler
{	
	public tk2dUIItem closeBTN;
    public tk2dUIItem[] ShopItemBTNList;
	ButtonAnimation BTNAni;
	
	protected override void Start () 
	{
		base.Start();
		if(closeBTN != null)
			BTNAni = closeBTN.GetComponent<ButtonAnimation>();
	}

	protected override void  OnEnter (object param) 
	{
		base.OnEnter(param);
        JMFRelay.ON_PACKET_RES_CLOSE_POPUP += OnPacketResClosePopup;
        JMFRelay.ON_PACKET_RES_CANCEL += OnPacketCancel;
        for (int i = 0; i < ShopItemBTNList.Length; ++i)
        {
            StoreItem storeItem = ShopItemBTNList[i].GetComponent<StoreItem>();

            if (storeItem != null)// && NOVNINE.Context.UncleBill.shopItems[i].PackageType == 0)
            {
                storeItem.Icon.SetSprite(NOVNINE.Context.UncleBill.shopItems[i].icon);
                storeItem.priceText.text = NOVNINE.Context.UncleBill.shopItems[i].price;
                storeItem.rewardText.text = string.Format("+ {0}",NOVNINE.Context.UncleBill.shopItems[i].rewards[0].count.ToString("n0"));
                storeItem.rewardText.Commit();
                storeItem.storeKey = NOVNINE.Context.UncleBill.shopItems[i].cashItemUniqueNumber;

                /*
                if (true)// NOVNINE.Context.UncleBill.shopItems[i].startEventDateTime <= LGameData.GetInstance().GetCurrentServerTime() && NOVNINE.Context.UncleBill.shopItems[i].endEventDateTime > LGameData.GetInstance().GetCurrentServerTime())
                {
                    storeItem.eventIcon.SetActive(true);
                    storeItem.eventTimeText.text = string.Format("{0}HR",NOVNINE.Context.UncleBill.shopItems[i].rewards[1].count);
                }
                //else
                //{
                //    storeItem.eventIcon.SetActive(false);
                //    storeItem.eventTimeText.text = "";
                //}
                storeItem.eventTimeText.Commit();
                */
            }
        }
	}

	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(entry.Animation.Name == "show")
		{
			BTNAni.CLICK();
		}

		if(entry.Animation.Name == "hide")
		{
            JMFRelay.ON_PACKET_RES_CLOSE_POPUP -= OnPacketResClosePopup;
            JMFRelay.ON_PACKET_RES_CANCEL -= OnPacketCancel;
			gameObject.SetActive(false);
		}
	}

	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(closeBTN == item)
		{
			OnEscape();
			return;
		}
		
		StoreItem storeItem = item.GetComponent<StoreItem>();
		if(storeItem != null)
		{
#if !UNITY_EDITOR
            Scene.LockWithMsg("Wait...");
#endif
            NOVNINE.ShopItem shopItem = NOVNINE.Context.UncleBill.GetShopItemByID(storeItem.storeKey);

            {
                Scene.Unlock();

                NoticePopupHandler popup = Scene.GetPopup("NoticePopup") as NoticePopupHandler;
                popup.SetErrorCode("Notice", "Test 구매 입니다.");    

                popup.OK_Callback = (_param) =>
                    {
                        bool isCoin = false;
                        for (int i = 0; i < shopItem.rewards.Count; ++i)
                        {
                            Root.Data.gameData.AddWalletItemDataByID(shopItem.rewards[i].itemId, shopItem.rewards[i].count,false);
                            if (shopItem.rewards[i].itemId == "coin")
                                isCoin = true;
                        }
                        if(isCoin)
                            NOVNINE.Wallet.FireItemChangedEvent("coin", NOVNINE.Wallet.GetItemCount("coin"));        
                    };

                popup.InitButton(NoticePopupHandler.BUTTON_TYPE.OK_CANCEL);
               // Scene.AddPopup("NoticePopup",true);
                
                return;
            }
		}
    }

    void OnPacketCancel()
    {
        if (Scene.CurrentSceneName() == "PlayScene")
            Scene.CurrentScene().GetComponent<PlaySceneHandler>().SetOnNetworkErrPopupBtnOk(false);
        
      //  bReceive = true;
        StopCoroutine("_coWaitResultPacket");
        OnEscape();
    }

    void OnPacketResClosePopup()
    {
        if (Scene.CurrentSceneName() == "PlayScene")
            Scene.CurrentScene().GetComponent<PlaySceneHandler>().SetOnNetworkErrPopupBtnOk(false);
     //   bReceive = true;
        OnEscape();
    }

    IEnumerator _coWaitResultPacket()
    {
        yield break;
        //bReceive = false;

        //while (!bReceive)
        {
         //   yield return null;
        }
    }
//    void purchaseCompleteAwaitingVerificationEvent(string purchaseData,string signature)
//    {
//
//        //IKVzGAUHNgYvBym27LHAt53eqVMd0rZkIYITVFUh8BZzRNrUGmCBVjJ1/kRkiHk/+ycJDFvBwnf46FvW35m/YBP8pbwuXFZSanDmVPe/zmv0y1/jGaaS0WSljTxr5xNV5++BxENNEIBV0L3YdRVsmpk10+7UzzVYseZGkvObPI+AkQ6qTbH3TsJR/ABhnKJ0fnzf40Ubzt9MqWv0DXuWXCP8mUp3nwnVrCCn492VRUw4f15cjpoTdGo4ClI0likptUvI5Zm7UG2Gp5ccXvcVpp66pgn6XDGkwT7wP2u/83QmF+aWNaT/QoKtp6abXQuWkvS3W06pbb6ujfGirnQc0Q==
//
//        LCommonDefine.ST_CashItem_PurchaseData cashItem_PurchaseData = new LCommonDefine.ST_CashItem_PurchaseData();
//
//        LCommonDefine.ST_GoogleStore_CashItem_PurchaseData stGoogleData = new LCommonDefine.ST_GoogleStore_CashItem_PurchaseData();
//        stGoogleData.Set(purchaseData, signature);
//        cashItem_PurchaseData.SetGoogleStore_CashItem_PurchaseData (stGoogleData);
//
//        System.Byte[] packet = LPuzzlePacket.REQ_CASHITEM_PURCHASE_RENEW_20170520( "nov9.jellomagic.item.0", "1", cashItem_PurchaseData);
//        if (packet != null)
//        {
//            LManager.GetInstance().Network_SendPacket(ref packet);
//        }
//    }
}