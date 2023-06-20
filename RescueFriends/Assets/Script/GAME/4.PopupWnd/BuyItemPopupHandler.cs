using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BuyItemPopupHandler : BasePopupHandler 
{
    public tk2dUIItem _btnClose = null;
    public tk2dUIItem _btnBuyRubby = null;
    public tk2dSprite _sprItem  = null;
    public tk2dTextMesh _txtName= null;
    public tk2dTextMesh _txt5More= null;

    public tk2dTextMesh _txtCount0, _txtCount1, _txtCount2;
    public tk2dTextMesh _txtPrice0, _txtPrice1, _txtPrice2;
    public tk2dTextMesh _txtSale0, _txtSale1, _txtSale2;

    public tk2dTextMesh _txtCoin;

    string _strItemId;

    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);

        _strItemId              = param as string;

        _refreshUI();
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnClose == item)
			Scene.ClosePopup();
        else if(_btnBuyRubby == item)
            Scene.ShowPopup("BuyRubyPopup", null, (p) => _refreshUI() );
        else
        {
            _ITEM_SHOP_DATA data= new _ITEM_SHOP_DATA();
            if(false == InfoLoader.GetItemShopInfoById(_strItemId, ref data))
                return;
            
            if(_txtPrice0.transform.parent.parent.GetComponent<tk2dUIItem>().Equals(item))
                _buyItem(data.arrPrice[0], data.arrCount[0]);
            else if(_txtPrice1.transform.parent.parent.GetComponent<tk2dUIItem>().Equals(item))
                _buyItem(data.arrPrice[1], data.arrCount[1]);
            else if(_txtPrice2.transform.parent.parent.GetComponent<tk2dUIItem>().Equals(item))
                _buyItem(data.arrPrice[2], data.arrCount[2]);
        }
	}

    void _buyItem(int price, int count)
    {
        int curRubby            = NOVNINE.Wallet.GetItemCount("coin");

        if(curRubby >= price)
        {
            NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( _strItemId );
            ItemEarnPopupHandler.Info data2 = new ItemEarnPopupHandler.Info();
            data2.count         = count;
            data2.strPicName    = invenItem.spriteName;

#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent("buy_item", "item_id", _strItemId);
#endif

            Scene.ShowPopup("ItemEarnPopup", data2, (param) =>
            {
                NOVNINE.Wallet.Use("coin", -1 * price);
                NOVNINE.Wallet.Gain(_strItemId, count);

                _refreshUI();
            });
        }
        else
        {
            // Rubby buy popup needed
            MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
            data2.isOkOnly      = false;
            data2.strMessage    = "You don't have \nenough rubbys.\nWanna Buy some?";
            data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
            Scene.ShowPopup("MessagePopup", data2, (param) =>
            {
                if(null != param)
                {
                    MessagePopupHandler.RET ret = (MessagePopupHandler.RET)param;
                    if(ret == MessagePopupHandler.RET.OK)
                        Scene.ShowPopup("BuyRubyPopup", null, (p) => _refreshUI());
                }
            });
        }
    }

    void _refreshUI()
    {
        NOVNINE.InventoryItem   invenItem = NOVNINE.Context.UncleBill.GetInventoryItemByID( _strItemId );
        if(null == invenItem)   return;

        _sprItem.spriteName     = invenItem.spriteName;
        _txtName.text           = invenItem.name;
        _txtName.Commit();
        _txt5More.gameObject.SetActive( _strItemId.Equals("moreturnbooster") );

        _ITEM_SHOP_DATA data    = new _ITEM_SHOP_DATA();
        if(false == InfoLoader.GetItemShopInfoById(_strItemId, ref data))
            return;

        _txtCount0.text         = "+ " + data.arrCount[0].ToString();  _txtCount0.Commit();
        _txtCount1.text         = "+ " + data.arrCount[1].ToString();  _txtCount0.Commit();
        _txtCount2.text         = "+ " + data.arrCount[2].ToString();  _txtCount0.Commit();

        _txtPrice0.text         = data.arrPrice[0].ToString();  _txtPrice0.Commit();
        _txtPrice1.text         = data.arrPrice[1].ToString();  _txtPrice0.Commit();
        _txtPrice2.text         = data.arrPrice[2].ToString();  _txtPrice0.Commit();

        _txtSale0.transform.parent.gameObject.SetActive( data.arrSale[0] > 0 );
        if(data.arrSale[0] > 0)
        {
            _txtSale0.text      = data.arrSale[0].ToString() + "%";
            _txtSale0.Commit();
        }
        _txtSale1.transform.parent.gameObject.SetActive( data.arrSale[1] > 0 );
        if(data.arrSale[1] > 0)
        {
            _txtSale1.text      = data.arrSale[1].ToString() + "%";
            _txtSale1.Commit();
        }
        _txtSale2.transform.parent.gameObject.SetActive( data.arrSale[2] > 0 );
        if(data.arrSale[2] > 0)
        {
            _txtSale2.text      = data.arrSale[2].ToString() + "%";
            _txtSale2.Commit();
        }

        _txtCoin.text           = string.Format("{0:#,###0}", NOVNINE.Wallet.GetItemCount("coin"));
        _txtCoin.Commit();
    }

}
