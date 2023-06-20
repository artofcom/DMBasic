#if UNITY_ANDROID && USE_UncleBill_GooglePlayStore

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using NOVNINE;

namespace NOVNINE.Store
{

	public class GooglePlayStoreAPI : IBillingMethod
	{
	    private System.Action<string> _initializeCallback;
	    private System.Action<string> _purchaseCallback;
	
	    private bool initialized = false;
	    private ShopItem purchase_item;
	
	    public void Initialize(System.Action<string> callback)
	    {
	        Debugger.Assert(initialized == false);
	        _initializeCallback = callback;
	
	        // Listen to all events for illustration purposes
	    /*    GoogleIABManager.billingSupportedEvent += billingSupportedEvent;
	        GoogleIABManager.billingNotSupportedEvent += billingNotSupportedEvent;
	        GoogleIABManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
	        GoogleIABManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
	        GoogleIABManager.purchaseSucceededEvent += purchaseSucceededEvent;
	        GoogleIABManager.purchaseFailedEvent += purchaseFailedEvent;
            GoogleIABManager.purchaseCompleteAwaitingVerificationEvent += purchaseCompleteAwaitingVerificationEvent;
	        GoogleIABManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
	        GoogleIABManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
	
	        GoogleIAB.enableLogging(Debugger.EnableLocalLog);
			GoogleIAB.init(NOVNINE.Context.NNPlatform.storeKey);
            */
            Debug.Log("Google Play Store Billing system init successed.");
	    }
	
	    public void Dispose()
	    {
	        // Remove all event handlers
	        /*GoogleIABManager.billingSupportedEvent -= billingSupportedEvent;
	        GoogleIABManager.billingNotSupportedEvent -= billingNotSupportedEvent;
	        GoogleIABManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
	        GoogleIABManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
	        GoogleIABManager.purchaseSucceededEvent -= purchaseSucceededEvent;
	        GoogleIABManager.purchaseFailedEvent -= purchaseFailedEvent;
            GoogleIABManager.purchaseCompleteAwaitingVerificationEvent -= purchaseCompleteAwaitingVerificationEvent;
	        GoogleIABManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
	        GoogleIABManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
	
	        GoogleIAB.unbindService();*/
	    }
	
	    public void Purchase(string productIdentifier, bool consumable, System.Action<string> callback)
	    {
	        Debugger.Assert(_purchaseCallback == null, "GooglePlayStoreAPI.Purchase _purchaseCallback is not null : "+productIdentifier);
	        _purchaseCallback = callback;
	        purchase_item = Context.UncleBill.GetShopItemByPriceID(productIdentifier);
	        Debugger.Assert(purchase_item != null, "GooglePlayStoreAPI.Purchase purchase_item not found : "+productIdentifier);
	       // GoogleIAB.purchaseProduct(productIdentifier);
	    }

        public void ConsumePurchase(string productIdentifier)
        {
          //  GoogleIAB.consumeProduct(productIdentifier);
        }
	
	    public void RestoreAllTransactions(System.Action<string[]> callback)
	    {
	        Debugger.LogWarning("GooglePlayStoreAPI.RestoreAllTransactions is not supported");
	        TaskManager.DoNextFrame(()=> {
	            Platform.SafeCallback(callback, null);
	        });
	    }
	
	    public void OpenStore(string storeParam = null, bool reviewPage = false)
	    {
			string bundleIdentifier = (storeParam == null) ? NOVNINE.NativeInterface.GetBundleIdentifier () : storeParam;
	#if UNITY_EDITOR
			Application.OpenURL("http://play.google.com/store/apps/details?id="+bundleIdentifier);
	#elif UNITY_ANDROID
			Application.OpenURL("market://details?id="+bundleIdentifier);
	#endif
	    }
	
	    public void SearchStore(string keyword)
	    {
	#if UNITY_EDITOR
	        Application.OpenURL("http://play.google.com/store/search?c=apps&q="+keyword);
	#elif UNITY_ANDROID
	        Application.OpenURL("market://search?c=apps&q="+keyword);
	#endif
	    }
	
	    #region Callbacks
	    void billingSupportedEvent()
	    {
            Debug.Log( "---------------------1 billingSupportedEvent" );
	        
	        List<string> itemIDs = new List<string>();
	        foreach(ShopItem item in Context.UncleBill.shopItems)
			{
                Debugger.Assert(!string.IsNullOrEmpty(item.productId), "Shop Item ProductID is NULL!!" + item.cashItemUniqueNumber);
	            if(string.IsNullOrEmpty(item.productId)) continue;
//	            if(item.IsCashPrice()) 
//                {
//                    // && !item.consumable)
//	                itemIDs.Add(item.GetPriceID());
//	            }
                itemIDs.Add(item.productId);
                Debug.Log( "shop item query ready --- " + item.productId );
	        }
	      //  GoogleIAB.queryInventory(itemIDs.ToArray());
	    }
	
	    void billingNotSupportedEvent(string error)
	    {
	        Platform.SafeCallback(_initializeCallback, error);
	    }
	
	    void queryInventorySucceededEvent( List<GooglePurchase> purchase, List<GoogleSkuInfo> skuInfo )
	    {
            Debug.Log("-------------------->  queryInventorySucceededEvent");

	        if(Debugger.EnableLocalLog) 
            {
	            System.Text.StringBuilder builder = new System.Text.StringBuilder();
	            foreach(var p in purchase)
	                builder.Append("\nP:"+p.ToString());
	            foreach(var s in skuInfo)
	                builder.Append("\nSKU:"+s.ToString());
//	            Debugger.Log("queryInventorySucceededEvent : "+builder.ToString());
//                Debug.Log("queryInventorySucceededEvent : "+builder.ToString());
	        }
	        
	        foreach(GooglePurchase p in purchase) 
            {
                Debug.Log( "GooglePurchased --- " + p.productId );

	            switch(p.purchaseState) 
                {
	                case GooglePurchase.GooglePurchaseState.Purchased: 
	                {
                       // Debug.Log("GooglePlayStoreAPI.queryInventorySucceededEvent pending item : "+p.productId);
//Debugger.Log("GooglePlayStoreAPI.queryInventorySucceededEvent pending item : "+p.productId);
	                    purchase_item = Context.UncleBill.GetShopItemByPriceID(p.productId);
	                    Debugger.Assert(purchase_item != null, "GooglePlayStoreAPI.queryInventorySucceededEvent purchase_item not found : "+p.productId);
	                    if(purchase_item == null) continue;
	                    if(purchase_item.consumable) 
                        {   
                            //Debug.Log("purchase_item.consumable : "+purchase_item.consumable);
	                    //    GoogleIAB.consumeProduct(p.productId);
							//NOVNINE.Wallet.Gain(purchase_item);
	                    }
//	                    else 
//                        {
//	                        Debugger.Assert(purchase_item.rewards != null);
//	                        foreach(var reward in purchase_item.rewards) 
//                            {
//	                            if(reward.GetItem() == null) 
//								{
//									//NOVNINE.Wallet.Gain(purchase_item);
//	                                break;
//	                            }
//	                        }
//	                    }
	                    
	                    purchase_item = null;
	                }
	                break;
	
	                //case GooglePurchase.GooglePurchaseState.Canceled:
	                //case GooglePurchase.GooglePurchaseState.Refunded:
	                //    UncleBill.Refund(p.productId);
	                //break;
	            }
	        }
	
            UncleBillContext ubCntx = NOVNINE.Context.UncleBill;
	        foreach(GoogleSkuInfo s in skuInfo) 
			{
                Debug.Log( "GoogleSku Info --- " + s.productId );

				NOVNINE.ShopItem item = ubCntx.GetShopItemByPriceID(s.productId);
	            Debugger.Assert(item != null, "GooglePurchase.queryInventoryFailedEvent sku not found:"+s.productId);
	            if(item == null) continue;

                Debug.Log("s.price: " + s.price);
                Debug.Log("s.priceAmountMicros: " + s.priceAmountMicros);
                Debug.Log("s.priceCurrencyCode: " + s.priceCurrencyCode);

                string serverPrice = JMFUtils.ConvertMoney(s.price, s.priceCurrencyCode);
                if (string.IsNullOrEmpty(serverPrice) == false)
                {
                    item.serverPriceNumber = float.Parse(serverPrice);
                    item.price =  s.price;//JMFUtils.FormatCurrency(item.serverPriceNumder);
                    item.SetPriceCurrencyCode(s.priceCurrencyCode);
                }
                else
                {
                    item.serverPriceNumber = float.Parse(item.price);
                    item.price = "$" + item.price;
                    item.SetPriceCurrencyCode("USD");
                }

                item.SetPriceAmountMicros(s.priceAmountMicros);
	        }
	        initialized = true;
	        Platform.SafeCallback(_initializeCallback, "success");
	    }
	
	    void queryInventoryFailedEvent( string error )
	    {
           // Debug.Log("queryInventoryFailedEvent: " + error);

	        initialized = false;
	        Platform.SafeCallback(_initializeCallback, error);
	    }
	
	    void purchaseSucceededEvent( GooglePurchase purchase )
	    {
            Debug.Assert(purchase_item != null);
        
            if (_purchaseCallback != null)
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_SUCCEEDED); 
	    }
	
	    void purchaseFailedEvent( string error )
	    {
            if (_purchaseCallback != null)
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_FAILED); 

           // Debug.Log("purchaseFailedEvent: " + error);

	        if( error.IndexOf("Item Already Owned") != -1 || error.IndexOf("you have attempted to purchase an item that has already been purchased") != -1 ) 
            {
	            if(purchase_item != null) 
                {
                  //  if (purchase_item.consumable)
                  //      GoogleIAB.consumeProduct(purchase_item.productId);
                  ///  else
                  //      Platform.SafeCallback(_purchaseCallback, "success");
	            } 
                else 
                {
	                Platform.SafeCallback(_purchaseCallback, error);
	            }
	        }
	        else
            {
                if (_purchaseCallback == null)
                    Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_FAILED); 
	        }

            _purchaseCallback = null;
            purchase_item = null;
	    }

        void purchaseCompleteAwaitingVerificationEvent(string purchaseData,string signature)
        {
            LCommonDefine.ST_CashItem_PurchaseData cashItem_PurchaseData = new LCommonDefine.ST_CashItem_PurchaseData();

            LCommonDefine.ST_GoogleStore_CashItem_PurchaseData stGoogleData = new LCommonDefine.ST_GoogleStore_CashItem_PurchaseData();
            stGoogleData.Set(purchaseData, signature);
            cashItem_PurchaseData.SetGoogleStore_CashItem_PurchaseData (stGoogleData);

            Data.Purchaseinfo purchaseinfo = new Data.Purchaseinfo();
           // purchaseinfo.purchaseData = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(purchaseData));
           // purchaseinfo.purchaseItemID = purchase_item.productId;
           // purchaseinfo.purchaseItemUniqueNumber = purchase_item.cashItemUniqueNumber;
           // purchaseinfo.signature = signature;
            purchaseinfo.strStoreId = purchase_item.productId;
            //LGameData.GetInstance().SetCurrentPurchasInfo(purchaseinfo);

            Root.Data.gameData.AddPurchaseInfo(purchaseinfo);

            ConsumePurchase(purchase_item.productId);

            /*System.Byte[] packet = LPuzzlePacket.REQ_CASHITEM_PURCHASE_RENEW_20170520( purchase_item.productId, purchase_item.cashItemUniqueNumber, cashItem_PurchaseData);
            if (packet != null)
            {
                LManager.GetInstance().Network_SendPacket( packet,true,(int)E_LPuzzlePlayer_PACKET.E_LPuzzlePlayer_PACKET_RES_CASHITEM_PURCHASE_RENEW_20170520);
                
                if (_purchaseCallback != null)
                    Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_COMPLETE_AWAITING_VERIFICATION); 
            }
            else*/
            {
                if (_purchaseCallback != null)
                    Platform.SafeCallback(_purchaseCallback, "Please Restart.");

                _purchaseCallback = null;
                purchase_item = null;
            }
        }
	
	    void consumePurchaseSucceededEvent( GooglePurchase purchase )
	    {
            if (_purchaseCallback != null && purchase_item != null)
            {
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.CONSUME_PURCHASE_SUCCEEDED); 
                _purchaseCallback = null;
                purchase_item = null;
            }

            // remove list
	    }
	
	    void consumePurchaseFailedEvent( string error )
	    {
            if (_purchaseCallback != null)
            {
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.CONSUME_PURCHASE_FAILED); 
                _purchaseCallback = null;
                purchase_item = null;
            }
	    }
	
	    #endregion
	}

}

#endif
