using UnityEngine;
using System.Collections;
using NOVNINE;
using NOVNINE.Store;

//public interface IBankItem 
//{
//    bool IsActive { get; }
//    string ItemKey { get; }
//    int RewardCount { get; }
//    Transform TF { get; }
//    void UpdateItem ();
//}

//public enum BANK_ITEM_TYPE { LIKE, SHARE, VIDEO };
//public enum PROMOTION_TYPE { NONE, HOT, BEST };

public class StoreItem : MonoBehaviour//, IBankItem 
{
    //public bool isPromotionItem;
    public string storeKey;
    //public PROMOTION_TYPE defaultPromotionType;

    //public GameObject sale;
    //public GameObject normal;
    //public GameObject promotionIcon;
    //public GameObject bestIcon;
    //public GameObject hotIcon;
    public GameObject eventIcon;
    public tk2dBaseSprite Icon;
    //public TextMesh magnification;
    //public TextMesh prevRewardText;
    //public tk2dTextMesh newRewardText;
    public tk2dTextMesh eventTimeText;
    public tk2dTextMesh rewardText;
    public TextMesh priceText;
    //public int baseSale;

	NOVNINE.ShopItem shopInfo;
	NOVNINE.ShopItem.Package rewardInfo;
    //PromotionPlanner.Plan.Item saleInfo;
    //PromotionPlanner.Plan plan;

	NOVNINE.ShopItem ShopInfo 
    {
        get {
            if (shopInfo != null) return shopInfo;
			if (shopInfo == null) shopInfo = NOVNINE.Context.UncleBill.GetShopItemByID(storeKey);
            return shopInfo;
        }
    }

	NOVNINE.ShopItem.Package RewardInfo
    {
        get {
            if (rewardInfo != null) return rewardInfo;
            if (ShopInfo == null) return null;

            if ((ShopInfo.rewards != null) && (ShopInfo.rewards.Count > 0))
                rewardInfo = ShopInfo.rewards[0];
            else
                rewardInfo = null;
            
            return rewardInfo;
        }
    }
    
//    PromotionPlanner.Plan.Item SaleInfo
//    {
//        get { 
//            if ((plan == null) || (plan.items == null)) return null;
//
//            for (int i = 0; i < plan.items.Length; i++) 
//            {
//                if (plan.items[i].item == storeKey) 
//                {
//                    saleInfo = plan.items[i];
//                    break;
//                }
//            }
//
//            return saleInfo;
//        }
//    }

//    public bool IsActive { get { return Purchased == false; } }

    public string ItemKey 
    {
        get { 
            if (RewardInfo == null) return null;
            return RewardInfo.itemId;
        }
    }

//    public bool IsNonconsumable 
//    { 
//        get 
//        { 
//            if (ShopInfo == null) return false;
//            return ShopInfo.consumable == false; 
//        } 
//    }

//    public bool Purchased 
//    {
//        get
//        {
//            if (IsNonconsumable == false) return false;
//            return Wallet.GetItemCount(RewardInfo.itemId) > 0;
//        }
//    }

    public int RewardCount
    {
        get {
            if (RewardInfo == null) return 0;
//            if (isPromotionItem && NowSale)
//                return Mathf.CeilToInt(RewardInfo.count);//return Mathf.CeilToInt(RewardInfo.count * SaleInfo.save * 0.01F);
//            else
            return RewardInfo.count;
        }
    }

//    public bool NowSale 
//    {
//        get { return SaleInfo != null; }
//    }

//    public Transform TF { get { return transform; } }

//    public void UpdateItem () 
//    {
////        if (Purchased) 
////        {
////            gameObject.SetActive(false);
////            return;
////        }
//
//        //gameObject.SetActive(true);
//
//        //plan = PromotionPlanner.CurrentPlan;
//
//        priceText.text = string.Format("{0}", ShopInfo.price);
//        priceText.Commit();
//
//        if (ItemKey == "coin")
//            rewardText.text = string.Format("{0}", RewardCount);
//        else
//            rewardText.text = string.Format("{0}", ShopInfo.name);
//
//        rewardText.Commit();
//
////        if (isPromotionItem && NowSale) 
////        {
////            sale.SetActive(true);
////            normal.SetActive(false);
////            magnification.text = string.Format("{0}%", (int)SaleInfo.save);
////            prevRewardText.text = string.Format("{0}", RewardInfo.count);
////            newRewardText.text = string.Format("{0}", RewardCount);
////            newRewardText.Commit();
////
////            promotionIcon.gameObject.SetActive(true);
////
////            if (SaleInfo.IsValue) 
////            {
////                hotIcon.SetActive(true);
////                bestIcon.SetActive(false);
////            }
////            else if (SaleInfo.IsSuper) 
////            {
////                hotIcon.SetActive(false);
////                bestIcon.SetActive(true);
////            }
////            else 
////            {
////                promotionIcon.gameObject.SetActive(false);
////            }
////        } 
////        else
////        {
////            sale.SetActive(false);
////            normal.SetActive(true);
////            if (baseSale > 0) 
////                magnification.text = string.Format("{0}%", baseSale);
////            else
////                magnification.text = "";
////            
////            
////            if (ItemKey == "coin")
////                rewardText.text = string.Format("{0}", RewardCount);
////            else
////                rewardText.text = string.Format("{0}", ShopInfo.name);
////            
////            rewardText.Commit();
////
////            promotionIcon.gameObject.SetActive(true);
////
////            switch (defaultPromotionType) 
////            {
////                case PROMOTION_TYPE.HOT :
////                    hotIcon.SetActive(true);
////                    bestIcon.SetActive(false);
////                    break;
////                case PROMOTION_TYPE.BEST :
////                    hotIcon.SetActive(false);
////                    bestIcon.SetActive(true);
////                    break;
////                default :
////                    promotionIcon.gameObject.SetActive(false);
////                    break;
////            }
////        }
//    }
}
