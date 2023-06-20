#if UNITY_IPHONE

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NOVNINE.Diagnostics;

namespace NOVNINE.Store
{

    public class AppleStoreAPI : IBillingMethod
    {
        private System.Action<string> _initializeCallback;
        private System.Action<string[]> _restoreTransactionCallback;
        private System.Action<string> _purchaseCallback;
        private List<string> itemIDs;
        private ShopItem purchase_item;

        public void Initialize(System.Action<string> callback)
        {
            _initializeCallback = callback;

            StoreKitManager.purchaseSuccessfulEvent += purchaseSuccessful;
            StoreKitManager.purchaseCancelledEvent += purchaseCancelled;
            StoreKitManager.purchaseFailedEvent += purchaseFailed;

            StoreKitManager.restoreTransactionsFinishedEvent += restoreTransactionsFinished;
            StoreKitManager.restoreTransactionsFailedEvent += restoreTransactionsFailed;

            StoreKitManager.productListReceivedEvent += productListReceived;
            StoreKitManager.productListRequestFailedEvent += productListRequestFailed;

            StoreKitManager.productPurchaseAwaitingConfirmationEvent += productPurchaseAwaitingConfirmationEvent;


            if(!StoreKitBinding.canMakePayments())
            {
                Platform.SafeCallback(_initializeCallback, "StoreKitBinding.canMakePayments is false");
                _initializeCallback = null;
                return;
            }
                
            itemIDs = new List<string>();
            foreach(ShopItem item in Context.UncleBill.shopItems)
            {
                //if(item.IsCashPrice())
                itemIDs.Add(item.productId);
            }

            StoreKitBinding.requestProductData(itemIDs.ToArray());
        }

        public void Dispose()
        {
            // Remove all the event handlers
            StoreKitManager.purchaseSuccessfulEvent -= purchaseSuccessful;
            StoreKitManager.purchaseCancelledEvent -= purchaseCancelled;
            StoreKitManager.purchaseFailedEvent -= purchaseFailed;

            StoreKitManager.restoreTransactionsFinishedEvent -= restoreTransactionsFinished;
            StoreKitManager.restoreTransactionsFailedEvent -= restoreTransactionsFailed;

            StoreKitManager.productListReceivedEvent -= productListReceived;
            StoreKitManager.productListRequestFailedEvent -= productListRequestFailed;
            StoreKitManager.productPurchaseAwaitingConfirmationEvent -= productPurchaseAwaitingConfirmationEvent;
        }

        public void Purchase(string productIdentifier, bool consumable, System.Action<string> callback)
        {
            Debugger.Assert(_purchaseCallback == null, "AppleStoreAPI.Purchase _purchaseCallback is not null : "+productIdentifier);
            _purchaseCallback = callback;
            purchase_item = Context.UncleBill.GetShopItemByPriceID(productIdentifier);
            Debugger.Assert(purchase_item != null, "AppleStoreAPI.Purchase purchase_item not found : "+productIdentifier);

            if(consumable)
            {
                purchase_item = Context.UncleBill.GetShopItemByPriceID(productIdentifier);
                StoreKitBinding.purchaseProduct(productIdentifier, 1);
            }
            else 
            {
                if(CheckLocalPurchasedInfo(productIdentifier)) 
                {
                    //already purchased
                    Platform.SafeCallback(_purchaseCallback, "success");
                } 
                else 
                {
                    //now what?
                    StoreKitBinding.purchaseProduct(productIdentifier, 1);
                }
            }
        }

        public void ConsumePurchase(string productIdentifier)
        {
        }

        public void RestoreAllTransactions(System.Action<string[]> callback)
        {
            _restoreTransactionCallback = callback;
            StoreKitBinding.restoreCompletedTransactions();
        }

    	[DllImport("__Internal")]
    	private static extern void _openStore(int identifier);

        public void OpenStore(string storeParam = null, bool reviewPage = false)
        {
    		string storeKey = (storeParam == null) ? NOVNINE.Context.NNPlatform.storeKey : storeParam;
            /*float osVersion = -1f;
            string versionString = SystemInfo.operatingSystem.Replace("iPhone OS ", "");
            float.TryParse(versionString.Substring(0, 1), out osVersion);
            if (osVersion >= 7) {
    			_openStore (Convert.ToInt32(storeKey));
            } else {
            }
            */

            if(reviewPage) 
                Application.OpenURL(string.Format("http://itunes.apple.com/app/id{0}?mt=8", storeKey));//Application.OpenURL("itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?onlyLatestVersion=true&pageNumber=0&sortOrdering=1&type=Purple+Software&id="+storeKey);
            else //"itms-apps://itunes.apple.com/app/id\1255286554?action=write-review&mt=8"
                Application.OpenURL(string.Format("http://itunes.apple.com/app/id{0}?mt=8", storeKey));
        }

        public void SearchStore(string keyword)
        {
            Application.OpenURL("itms-apps://phobos.apple.com/WebObjects/MZSearch.woa/wa/search?media=software&submit=media&term="+keyword);
        }

        #region Callbacks
        void purchaseSuccessful( StoreKitTransaction transaction )
        {
            Debug.Assert(purchase_item != null);
            Debugger.Assert(transaction != null);
            //Debug.Log( "purchased product: "+transaction.ToString() );
            //Debug.Log(transaction.base64EncodedTransactionReceipt);
            Debugger.Assert(transaction.quantity == 1, "AppleStoreAPI.purchaseSuccessful transaction quantity fail : "+transaction.ToString());
            if(_purchaseCallback != null && purchase_item != null)
            {
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.CONSUME_PURCHASE_SUCCEEDED); 
                _purchaseCallback = null;
                purchase_item = null;
            }
        }

        void purchaseCancelled( string error )
        {
            //Debug.Log( "purchase cancelled with error: "+error );
            Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_CANCEL); 
            _purchaseCallback = null;
        }

        void purchaseFailed( string error )
        {
            //Debug.LogError( "AppleStoreAPI.purchaseFailed with error: "+error);
            if (_purchaseCallback != null)
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_FAILED); 
            _purchaseCallback = null;
            purchase_item = null;
        }

        void productListReceived( List<StoreKitProduct> productList )
        {
            //Debug.Log( "total productsReceived: " + productList.Count );
            // Do something more useful with the products than printing them to the console
//            if(Debugger.EnableLocalLog)
//            {
//                foreach( StoreKitProduct product in productList ) 
//                {
//                    Debug.Log(string.Format( "StoreKitProduct - ID: {0} Price: {1}", product.productIdentifier, product.price));
//                }
//            }

            if(productList.Count == 0) 
            {
                UncleBill.DisableCashPurchase();
            }
            else if(productList.Count < itemIDs.Count) 
            {
                foreach(string id in itemIDs) 
                {
                    StoreKitProduct item = productList.Find((pd)=> {
                        return pd.productIdentifier == id;
                    });
                    if(item == null)
                        UncleBill.DisableShopItemByPriceID(id);
                }
            }

            UncleBillContext ubCntx = NOVNINE.Context.UncleBill;
            string currencyCode = "USD";
    		foreach( StoreKitProduct product in productList ) 
            {
    			NOVNINE.ShopItem item = ubCntx.GetShopItemByPriceID(product.productIdentifier);
                item.price = product.formattedPrice;
                
                if(item.price == null)
                    item.SetPriceCurrencyCode("USD");
                currencyCode = product.currencyCode;
                item.serverPriceNumder = float.Parse(product.price);// (JMFUtils.ConvertMoney(product.formattedPrice,product.currencySymbol));
//                Debug.Log(string.Format("product.price :{0}", product.price ));
//                Debug.Log(string.Format("product.SetServerPrice:{0}", item.serverPriceNumder));
//                Debug.Log(string.Format("product.currencyCode:{0}", product.currencyCode));
//                Debug.Log(string.Format("product.formattedPrice:{0}", product.formattedPrice));
//                Debug.Log(string.Format("product.currencySymbol:{0}", product.currencySymbol));
    		}
            JMFUtils.FindSymbolCurrency(currencyCode);
            Platform.SafeCallback(_initializeCallback, "success");
            _initializeCallback = null;
        }

        void productListRequestFailed( string error )
        {
            //Debug.Log( "AppleStoreAPI.productListRequestFailed: "+error);
            Platform.SafeCallback(_initializeCallback, error);
            _initializeCallback = null;
        }

        void productPurchaseAwaitingConfirmationEvent(StoreKitTransaction st)
        {
            //Debug.Log("AppleStoreAPI.productPurchaseAwaitingConfirmationEvent");
            Debug.Log(string.Format("st :{0}",st == null));

            if (st.transactionState == null || st.transactionState != StoreKitTransactionState.Purchased)
                return;

            if (Root.Data == null)
                return;
            
            if(_purchaseCallback != null)
                Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_SUCCEEDED); 

            LCommonDefine.ST_CashItem_PurchaseData cashItem_PurchaseData = new LCommonDefine.ST_CashItem_PurchaseData();

            LCommonDefine.ST_AppleStore_CashItem_PurchaseData stAppleData = new LCommonDefine.ST_AppleStore_CashItem_PurchaseData();
            stAppleData.Set(st.base64EncodedTransactionReceipt);
            cashItem_PurchaseData.SetAppleStore_CashItem_PurchaseData (stAppleData);

            Data.Purchaseinfo purchaseinfo = new Data.Purchaseinfo();
            purchaseinfo.purchaseData = st.base64EncodedTransactionReceipt;
            purchaseinfo.purchaseItemID = purchase_item.productId;
            purchaseinfo.purchaseItemUniqueNumber = purchase_item.cashItemUniqueNumber;
            purchaseinfo.signature = st.transactionIdentifier;
            LGameData.GetInstance().SetCurrentPurchasInfo(purchaseinfo);

            Root.Data.gameData.AddPurchaseInfo(purchaseinfo);

            System.Byte[] packet = LPuzzlePacket.REQ_CASHITEM_PURCHASE_RENEW_20170520( purchase_item.productId, purchase_item.cashItemUniqueNumber, cashItem_PurchaseData);
            if (packet != null)
            {
                LManager.GetInstance().Network_SendPacket(packet,true,(int)E_LPuzzlePlayer_PACKET.E_LPuzzlePlayer_PACKET_RES_CASHITEM_PURCHASE_RENEW_20170520);

                if (_purchaseCallback != null)
                    Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_COMPLETE_AWAITING_VERIFICATION); 
            }
            else
            {
                if (_purchaseCallback != null)
                    Platform.SafeCallback(_purchaseCallback, "Please Restart.");

                _purchaseCallback = null;
                purchase_item = null;
            }
        }

        void restoreTransactionsFinished()
        {
            //Debug.Log("AppleStoreAPI.restoreTransactionsFinished");
            List<string> items = new List<string>();
            foreach(StoreKitTransaction tr in StoreKitBinding.getAllSavedTransactions()) 
            {
                //Debug.Log("  - Item : "+tr.productIdentifier);
                items.Add(tr.productIdentifier);
            }

            UncleBill.UpdateInventory(items.ToArray());
            Platform.SafeCallback(_restoreTransactionCallback, items.ToArray());
        }

        void restoreTransactionsFailed(string error)
        {
            //Debug.LogError("AppleStoreAPI.restoreTransactionsFailed :"+error);
            Platform.SafeCallback(_restoreTransactionCallback, null);
        }
        #endregion

        bool CheckLocalPurchasedInfo(string productIdentifier)
        {
            //Debug.Log("CheckLocalPurchasedInfo - " + productIdentifier);
            bool purchased = false;
            List<StoreKitTransaction> transactionList = StoreKitBinding.getAllSavedTransactions();
            for (int i=0; i<transactionList.Count; i++) 
            {
                StoreKitTransaction transaction = transactionList[i];
                if (transaction.productIdentifier == productIdentifier)
                {
                    purchased = true;
                    break;
                }
            }

            //Debug.Log("End CheckLocalPurchasedInfo - " + purchased.ToString());
            return purchased;
        }
    }

}

#endif

