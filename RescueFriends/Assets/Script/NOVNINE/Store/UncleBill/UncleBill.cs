using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;

namespace NOVNINE.Store
{

    public static class UncleBill
	{
        public const string PURCHASE_SUCCEEDED = "purchaseSucceeded";    
        public const string PURCHASE_CANCEL = "purchaseCancel";    
        public const string PURCHASE_FAILED = "purchaseFailed";    
        public const string PURCHASE_COMPLETE_AWAITING_VERIFICATION = "purchaseCompleteAwaitingVerification";    
        public const string CONSUME_PURCHASE_SUCCEEDED = "consumePurchaseSucceeded";    
        public const string CONSUME_PURCHASE_FAILED = "consumePurchaseFailed";    
      
	    static IBillingMethod _active;
	    static bool _isRich = false;
	    static bool _initRequested = false;
	    static bool _initialized = false;
	
	    public static IBillingMethod Active
	    {
	        get {
	            return _active;
	        }
#if !NN_DEPLOY
	        set {
	            _active = value;
	        }
#endif
	    }
	
	    public static IBillingMethod GetStoreInterface()
	    {
	        IBillingMethod API = null;
	
#if UNITY_EDITOR
	        API = new DummyAPI();
	        return API;
#else
	        if(NOVNINE.Profile.IsDummyIAP) 
			{
	            API = new DummyAPI();
	            return API;
	        }
	
	        switch(Context.UncleBill.storeType) 
			{
	#if USE_UncleBill_AppleStore
	        case PlatformContext.StoreType.AppleStore:
	            API = new AppleStoreAPI();
	            break;
	#endif
	#if USE_UncleBill_GooglePlayStore
	        case PlatformContext.StoreType.GoogleStore:
	            API = new GooglePlayStoreAPI();
	            break;
	#endif
	#if USE_UncleBill_AmazonStore
	        case PlatformContext.StoreType.AmazonStore:
	            API = new AmazonStoreAPI();
	            break;
	#endif
	#if USE_UncleBill_MacAppStore
	        case PlatformContext.StoreType.MacAppStore:
	            API = new MacAppStoreAPI();
	            break;
	#endif
	#if USE_UncleBill_FacebookStore
	        case PlatformContext.StoreType.FacebookStore:
	            API = new FacebookStoreAPI();
	            break;
	#endif
	        default:
	            API = new DummyAPI();
	            break;
	        }
	        // Debugger.Assert(API.GetType() != typeof(DummyAPI), "UncleBill.GetStoreInterface is Dummy!");
	        Debug.Log("UncleBill.GetStoreInterface API = "+API.GetType().FullName);
	        return API;
	#endif
	    }
	
	    static UncleBill()
	    {
	        _active = GetStoreInterface();
	        if(_active == null)
	            Debug.LogError("unknown Store Type : "+Platform.Info.storeType.ToString());
#if UNITY_ANDROID
            //GoogleIAB.setAutoVerifySignatures(false);
#endif
	        _isRich = PlayerPrefs.GetString(NativeInterface.GetBundleIdentifier()+".isRich") == "yes";
	    }
	
	    public static bool IsRich
	    {
	        get {
				if (!_isRich)// && Wallet.hasNonConsumable) 
				{
	                _isRich = true;
	                PlayerPrefs.SetString(NativeInterface.GetBundleIdentifier()+".isRich", "yes");
	            }
	            return _isRich;
	        }
	
	        private set {
	            _isRich = value;
	            PlayerPrefs.SetString(NativeInterface.GetBundleIdentifier()+".isRich", _isRich?"yes":"no");
	        }
	    }
	
	    public static void PrepareToOpen(System.Action<bool> callback)
	    {
            Debug.Log("UncleBill 1.");
	        if(_initialized) 
			{
                Debug.Log("UncleBill 2.");
	            Platform.SafeCallback(callback, true);
	        }
			else if(!_initRequested) 
			{
                Debug.Log("UncleBill 3.");
	            if(Application.internetReachability != NetworkReachability.NotReachable) 
				{
                    Debug.Log("UncleBill 4.");
	                Active.Initialize((result)=> 
                    {
                        Debug.Log("UncleBill 5.");

	                    bool success = (result == "success");
	                    _initialized = success;
	                    _initRequested = false;
	                    if(!success) {
	                        Debug.LogWarning("PrepareToOpen fail : "+result);
	                    }
	                    Platform.SafeCallback(callback, success);
	                });
	                _initRequested = true;
                    Debug.Log("UncleBill 6.");
	            } else {
	                Debug.LogWarning("PrepareToOpen failed with NetworkReachability("+Application.internetReachability);
					Platform.SafeCallback(callback, false);
                    Debug.Log("UncleBill 7.");
	            }
	        } else {
	            Platform.SafeCallback(callback, false);
                Debug.Log("UncleBill 8.");
	        }
            Debug.Log("UncleBill 9.");
	    }
	
	    public static void AskForBuy(string marketID, bool consumable, System.Action<string> callback)
	    {
	        if(Application.internetReachability == NetworkReachability.NotReachable) 
			{
	            Platform.SystemAlert("error", "check_network", "ok");
				Platform.SafeCallback(callback, "No Network");
	            return;
	        }
	
	        if(!_initialized) 
			{
	            if(!_initRequested) 
				{
	                PrepareToOpen((success)=> 
                    {
	                    if(success) 
						    AskForBuy(marketID, consumable, callback);
	                    else 
						    Platform.SafeCallback(callback, "initRequested fail");
	                });
	                return;
	            }
				else 
				{
	                Debug.LogWarning("AskForBuy is initializing. try later");
					Platform.SafeCallback(callback, "AskForBuy is initializing. try later");
	                return;
	            }
	        }
	
			if(string.IsNullOrEmpty(marketID)) 
			{
	            Debug.LogError("AskForBuy item PriceID is Not Set : "+marketID);
	            return;
	        }
	        
            Active.Purchase(marketID, consumable, callback);
	    }
	
        public static void ConsumePurchase(string productIdentifier)
        {
            Active.ConsumePurchase(productIdentifier);
        }

	    public static void RestoreAllPurchases(System.Action<string[]> callback)
	    {
	        //Platform.SetRecoverPoint("unclebill");
	        Active.RestoreAllTransactions(items=> 
			{
	            Platform.SafeCallback(callback, items);
	            //PlayerPrefs.Save();
	        });
	    }
	
	    public static void UpdateInventory(string[] itemids)
	    {
	        foreach(string id in itemids)
			{
	            ShopItem sitem = Context.UncleBill.GetShopItemByPriceID(id);
	            if(null != sitem && !sitem.consumable) 
				{
	                ShopItem.Package firstReward = sitem.rewards[0];
	                InventoryItem invenItem = Wallet.Get(firstReward.itemId);
	                if( (invenItem == null || invenItem.count == 0) || (firstReward.IsInfinite && !invenItem.IsInfinite) ) 
					{
						Wallet.Gain(sitem);//, "restore");
	                }
	            } 
				else 
				{
	                Debug.Log("UncleBill.UpdateInventory Skipping Item : "+id);
	            }
	        }
	    }
	
	    public static void DisableCashPurchase()
	    {
	        foreach(ShopItem item in Context.UncleBill.shopItems)
	            if(item.IsCashPrice())
	                item.enabled = false;
	    }
	
	    public static void DisableShopItemByPriceID(string priceID)
	    {
	        ShopItem sitem = Context.UncleBill.GetShopItemByPriceID(priceID);
	        if(sitem != null)
	            sitem.enabled = false;
	    }
	
	    public static void OpenStore(string storeParam = null, bool reviewPage = false)
	    {
	        //Platform.SetRecoverPoint("unclebill");
			Active.OpenStore(storeParam, reviewPage);
	    }
	
	    public static void SearchStore(string keyword)
	    {
	        //Platform.SetRecoverPoint("unclebill");
	        Active.SearchStore(keyword);
	    }
	
	    public static System.Type API
	    {
	        get {
	            return Active.GetType();
	        }
	    }
	}
}

