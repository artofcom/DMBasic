using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BuyRubyPopupHandler : BasePopupHandler 
{
    public tk2dUIItem _btnClose = null;
    NOVNINE.ShopItem            _curBuyingItem = null;

    enum BUY_RES { NONE, FAILED, SUCCESSED, OFFLINE_TEST, };
    BUY_RES _eResult            = BUY_RES.NONE;

    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);
        _refreshUI();

        StartCoroutine( "_coStateMachine" );
    }
	
    protected override void OnLeaveFinished()
	{
        StopCoroutine( "_coStateMachine" );

        base.OnLeaveFinished();
	}

    IEnumerator _coStateMachine()
    {
        do
        {
            switch(_eResult)
            {
            case BUY_RES.OFFLINE_TEST:
                _buy_with_offline();
                break;
            case BUY_RES.SUCCESSED:
                _buy_successed();
                break;
            case BUY_RES.FAILED:
                _buy_failed();
                break;
            }
            _eResult            = BUY_RES.NONE;
            yield return new WaitForSeconds(0.1f);

        }while(true);
    }

    void _buy_with_offline()
    {
        DOVirtual.DelayedCall(1.0f, () =>
        {
            Scene.ClosePopup();
            NOVNINE.Wallet.GainGameMoney(_curBuyingItem.rewards[0].count);

            ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( "coin" );
            if(null != invenItem)   data.strPicName = invenItem.spriteName;
            data.count              = _curBuyingItem.rewards[0].count;
            Scene.ShowPopup("ItemEarnPopup", data, (param) => { });
        });
    }

    void _buy_successed()
    {
        Scene.ClosePopup();

        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Ruby buy successed!";
        data2.emotion           = MessagePopupHandler.EMOTIONS.HAPPY;
        Scene.ShowPopup("MessagePopup", data2, (temp) =>
        {
            NOVNINE.Wallet.GainGameMoney(_curBuyingItem.rewards[0].count);

            ItemEarnPopupHandler.Info data = new ItemEarnPopupHandler.Info();
            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( "coin" );
            if(null != invenItem)   data.strPicName = invenItem.spriteName;
            data.count              = _curBuyingItem.rewards[0].count;
            Scene.ShowPopup("ItemEarnPopup", data, (param) => { });
        });
    }

    void _buy_failed()
    {
        Scene.ClosePopup();

        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Ruby buy failed!\n Please try again.";
        data2.emotion           = MessagePopupHandler.EMOTIONS.SAD;
        Scene.ShowPopup("MessagePopup", data2, null);
    }

	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnClose == item)
			Scene.ClosePopup();
        else
        {
            RubyShopItem        shopItem = item.transform.parent.parent.GetComponent<RubyShopItem>();
            if(null != shopItem)
            {
                Scene.ShowPopup("LoadingPopup");

                _curBuyingItem  = shopItem.getShopItem();
                if(null == IAPHandler.getInstance())
                {
                    _eResult    = BUY_RES.OFFLINE_TEST;
                    return;
                }

                IAPHandler.getInstance().purchase(_curBuyingItem.productId, 

                () =>   // onSuccess callback.
                {
                    _eResult    = BUY_RES.SUCCESSED;
                }, 

                () =>   // onFailed callback.
                {
                    _eResult    = BUY_RES.FAILED;
                });

            }
        }
	}

    void _refreshUI()
    {
        // init cell items by data.

        // 각도와 대조해서 item 맞추는 기능 필요.
        //_sprWheel.transform.localEulerAngles = Vector3.zero;
    }

}
