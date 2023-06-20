using UnityEngine;
using System.Collections;
using NOVNINE;
using NOVNINE.Store;

public class RubyShopItem : MonoBehaviour
{
    public tk2dSprite           _sprIcon;
    public GameObject           _objTagBestDeal;
    public tk2dTextMesh         _txtPrice;

    public Transform            _trBonusRoot, _trNoBonusRoot;
    public tk2dUIItem           _btnBuy;

    ShopItem _curShopItem       = null;
    public ShopItem getShopItem()
    {
        return _curShopItem;
    }

    public void refresh(int idxItem, BuyRubyPopupHandler handler)
    {
        //_RUBY_SHOP_ITEM_INFO    item   = new _RUBY_SHOP_ITEM_INFO();
        //InfoLoader.GetRubyShopItemInfoById(ItemId, ref item);
        if(idxItem<0 || idxItem>=Context.UncleBill.shopItems.Count)
            return;

        _curShopItem            = Context.UncleBill.shopItems[idxItem];
        if(0 == _curShopItem.cashItemUniqueNumber.Length)
            return;

        _objTagBestDeal.SetActive(_curShopItem.isBestDeal);
        if(_objTagBestDeal.activeSelf)
            _objTagBestDeal.GetComponent<tk2dSprite>().spriteName = "tag_best_deal";        
        else
        {
            // do this on hard-code.
            if(2 == idxItem)
            {
                _objTagBestDeal.SetActive(true);
                _objTagBestDeal.GetComponent<tk2dSprite>().spriteName = "tag_user_choice";
            }
        }

        

        _trBonusRoot.gameObject.SetActive(_curShopItem.bonusCount>0);
        _trNoBonusRoot.gameObject.SetActive(_curShopItem.bonusCount==0);
        
        _sprIcon.spriteName     = _curShopItem.icon;

        _btnBuy.OnReleaseUIItem -= handler.CLICK;
        _btnBuy.OnReleaseUIItem += handler.CLICK;

        if(_curShopItem.bonusCount > 0)
        {
            tk2dTextMesh txtOne = _trBonusRoot.Find("txtAmount").GetComponent<tk2dTextMesh>();
            txtOne.text         = string.Format("+ {0:#,###0}", _curShopItem.rewards[0].count);
            txtOne.Commit();

            txtOne              = _trBonusRoot.Find("txtOrgAmount").GetComponent<tk2dTextMesh>();
            txtOne.text         = string.Format("{0:#,###0}+", _curShopItem.rewards[0].count-_curShopItem.bonusCount);
            txtOne.Commit();

            txtOne              = _trBonusRoot.Find("txtBonus").GetComponent<tk2dTextMesh>();
            txtOne.text         = string.Format("Bonus {0:#,###0}", _curShopItem.bonusCount);
            txtOne.Commit();

            txtOne              = _trBonusRoot.Find("sprExtraBG/txtSale").GetComponent<tk2dTextMesh>();
            txtOne.text         = string.Format("{0}%", _curShopItem.bonusPercentage);
            txtOne.Commit();
        }
        else
        {
            tk2dTextMesh txtOne = _trNoBonusRoot.Find("txtAmount").GetComponent<tk2dTextMesh>();
            txtOne.text         = string.Format("+ {0:#,###0}", _curShopItem.rewards[0].count);
            txtOne.Commit();
        }


        string strPrice         = "$" + _curShopItem.price.ToString();
        if(null != IAPHandler.getInstance())
        {
            Sdkbox.Product p    = IAPHandler.getInstance().getProduct(_curShopItem.productId);
            if(p.id.Length>0)   strPrice    = p.currencyCode + p.price;
        }

        _txtPrice.text          = strPrice;
        _txtPrice.Commit();
    }   
}
