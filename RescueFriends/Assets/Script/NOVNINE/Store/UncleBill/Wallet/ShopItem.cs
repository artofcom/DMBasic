using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
//using UnityEngine.SocialPlatforms.Facebook;
//using NOVNINE;

namespace NOVNINE
{
    
    public enum ItemType {
        Coin2Item, //GameMoney -> Item
        Action2Coin,//Action -> GameMoney
        Cash2Coin, //Cash -> GameMoney
        Cash2Item, //Cash -> Item
        Action2Item //Action -> Item
    }
        
    public enum PriceType {
        Coin,
        Cash,
        Action
    }

    [System.Serializable]
    public class Price
    {
        public string name;
        public string priceID;
    }

    [System.Serializable]
    public class ShopItem
    {
        public enum Category {
            Item,
            Coin
        }

        public string cashItemUniqueNumber;
        public ItemType type;
        //public int PackageType;
        //public int Percent = 0;
        public string icon;
        public bool consumable = true;

        public string name;
        public string description;

        public bool enabled = true;

        // only for display !!!
        public int bonusPercentage  = 0;
        public int bonusCount       = 0;
        public bool isBestDeal      = false;
        //


        public System.DateTime startSalesDateTime;
        public System.DateTime endSalesDateTime;

        public System.DateTime startEventDateTime;
        public System.DateTime endEventDateTime;

        //public int maxSalesCount = 0;
        //public int salesCount = 0;

        [System.Serializable]
        public class Package
        {
            public string itemId;
            public int count;

            public InventoryItem GetItem()
            {
                return Wallet.Get(itemId);
            }
            public bool IsInfinite
            {
                get {
                    return (count == int.MaxValue) || (count < 0);
                }
            }
            public bool IsCoin
            {
                get {
                    return (itemId.ToLower() == "coin");
                }
            }
        }

        public List<Package> rewards = new List<Package>();
       
        public string price = null;
        public string productId= null;
        public float serverPriceNumber = 0.0f;
        
        string priceCurrencyCode = "USD";

        string priceAmountMicros = null;

        public bool IsStartSales()
        {
            //System.DateTime t = LGameData.GetInstance().GetCurrentServerTime();
            return false;// (startSalesDateTime <= t && endSalesDateTime > t);
        }

        //price categorization
        public bool IsCashPrice()
        {
            return (type == ItemType.Cash2Coin || type == ItemType.Cash2Item);
        }

        public bool IsCoinPrice()
        {
            return (type == ItemType.Coin2Item);
        }

        public bool IsActionPrice()
        {
            return (type == ItemType.Action2Coin || type == ItemType.Action2Item);
        }

        public PriceType GetPriceType()
        {
            if(IsCoinPrice()) return PriceType.Coin;
            else if(IsCashPrice()) return PriceType.Cash;
            else return PriceType.Action;
        }

        //reward categorization
        public bool IsTangibleItem()
        {
            return (type == ItemType.Cash2Item || type == ItemType.Coin2Item);
        }

        public bool IsCoinItem()
        {
            return (type == ItemType.Action2Coin || type == ItemType.Cash2Coin);
        }

        public string GetPriceAmountMicros()
        {
            return priceAmountMicros;
        }

        public void SetPriceAmountMicros(string PriceAmountMicros)
        {
            priceAmountMicros = PriceAmountMicros;
        }

        public string GetPriceCurrencyCode()
        {
            return priceCurrencyCode;
        }

        public void SetPriceCurrencyCode(string PriceCurrencyCode)
        {
            priceCurrencyCode = PriceCurrencyCode;
        }

//        public bool IsInfinite
//        {
//            get {
//                if(rewards.Count == 0) {
//                    Debug.LogError("ShopItem \""+cashItemUniqueNumber+"\" rewards is not set");
//                    return false;
//                }
//                return (rewards[0].count == int.MaxValue) || (rewards[0].count < 0);
//            }
//        }

        public bool IsSellable
        {
            get {
                /*
                            InventoryItem iitem = Wallet.Get(rewards[0].itemId);
                            if(iitem != null)
                            {
                                if(iitem.consumable && !iitem.IsInfinite)
                                    return enabled;
                                return false;
                            }
                            return enabled; */

    //@@ issue changed consumable.
                if (false == enabled) return enabled;

                InventoryItem iitem = null;
    			iitem = Wallet.Get(rewards[0].itemId);
                if (null == iitem)  return enabled;
                if (true == iitem.IsInfinite) return false;
                if (true == iitem.consumable) return true;
                if (iitem.count <= 0) return true;
                return false;
            }
        }
    }

}
