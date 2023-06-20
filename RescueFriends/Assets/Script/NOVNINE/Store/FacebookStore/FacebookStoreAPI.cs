#define LLOG_FACEBOOKSTORE

#if UNITY_WEBGL

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using JsonFx.Json;

namespace NOVNINE.Store
{

public class FacebookStoreAPI : IBillingMethod
{
	private const string NOVNINE_FB_BASE = "https://www.facebook.com/Nov9games-1307135889349893/";

    private System.Action<string> _initializeCallback;
    private System.Action<string[]> _restoreTransactionCallback;
    private System.Action<string> _purchaseCallback;

    public void Initialize(System.Action<string> callback)
    {
        _initializeCallback = callback;

        TaskManager.StartCoroutine(coInitialize());
    }

    IEnumerator coInitialize() {
		while(NOVNINE.Context.Facebook == null) yield return 0;
		while(NOVNINE.Context.Facebook.Initialized == false) yield return 0;
        if(FB.IsLoggedIn == false) {
            FB.Login("email", LogCallback);
            yield return 0;
            while(FB.IsLoggedIn == false) yield return 0;
        }
        Platform.SafeCallback(_initializeCallback, "success");
    }

    void LogCallback(FBResult response) {
        Debug.Log("FacebookStoreAPI.LogCallback : "+response.Text);
    }

    public void Dispose()
    {

    }

    public void Purchase(string productIdentifier, bool consumable, System.Action<string> callback)
    {
        _purchaseCallback = callback;

        FB.Canvas.Pay(
		product: NOVNINE_FB_BASE+"fbresource/"+productIdentifier+".html",
        callback: delegate(FBResult response) {
#if LLOG_FACEBOOKSTORE
            Debug.Log("FacebookStoreAPI.Purchase item["+productIdentifier+"]  response: "+response.Text);
#endif
            Hashtable result = response.Text.hashtableFromJson();
            Debug.Log("status:"+(string)result["status"]);
            if(((string)result["status"]) == "completed")
                Platform.SafeCallback(_purchaseCallback, "success");
            else {
                //FIXME
                Platform.SafeCallback(_purchaseCallback, response.Text);
            }
        }
        );
    }

    public void ConsumePurchase(string productIdentifier)
    {
    
    }

    public class PaymentTransaction {
        public class Entry {
            public string id;
            public string name;
        }
        public class Action {
            public string type;
            public string status;
            public string currency;
            public string amount;
            public string time_created;
            public string time_updated;
        }
        public class RefundableAmount {
            public string currency;
            public string amount;
        }
        public class Item {
            public string type;
            public string product;
            public int quantity;
        }
        public class Data {
            public string id;
            public Entry user;
            public Entry application;
            public List<Action> actions;
            public RefundableAmount refundable_amount;
            public List<Item> items;
            public string country;
            public string created_time;
            public float payout_foreign_exchange_rate;
        }
        public class Paging {
            public string previous;
            public string next;
        }

        public List<Data> data;
        public Paging paging;
    }

    IEnumerator coRestoreItems() {
		WWW www = new WWW(NOVNINE_FB_BASE+"fbrtu/restore.php?fbid="+FB.UserId+"&bm_appid="+NOVNINE.Context.NNPlatform.appID);
        yield return www;
        Debugger.Assert(www.isDone);
        Debugger.Assert(string.IsNullOrEmpty(www.error));
#if LLOG_FACEBOOKSTORE
        Debug.Log("FacebookStoreAPI.coRestoreItems Result : "+www.text);
#endif
        List<string> restoredItems = new List<string>();
        var tr = JsonReader.Deserialize<PaymentTransaction>(www.text);
        foreach(var data in tr.data) {
            //skip refunded item
            bool refunded = false;
            foreach(var act in data.actions) {
                if(act.type == "refund" && act.status == "completed") {
                    refunded = true;
                    break;
                }
            }
            if(refunded) continue;

            foreach(var item in data.items) {
                string id = item.product;
                id = id.Replace(BITMANGO_FB_BASE+"fbresource/", "");
                id = id.Replace(".html", "");
                restoredItems.Add(id);
            }
        }

        Platform.SafeCallback(_restoreTransactionCallback, restoredItems.ToArray());
    }

    public void RestoreAllTransactions(System.Action<string[]> callback)
    {
        _restoreTransactionCallback = callback;
        TaskManager.StartCoroutine(coRestoreItems());
    }

    public void OpenStore(string storeParam = null)
    {
        Debug.LogError("FacebookStoreAPI.OpenStore Not Implemented");
    }

    public void SearchStore(string keyword)
    {
        Debug.LogError("FacebookStoreAPI.SearchStore Not Implemented");
    }

    /*
    #region Callbacks
    	void purchaseSuccessful( StoreKitTransaction transaction )
    	{
    #if LLOG_FACEBOOKSTORE
    		Debug.Log( "purchased product: "+transaction.productIdentifier.ToString() + "  " + transaction.quantity );
    #endif
            Platform.SafeCallback(_purchaseCallback, true);
            _purchaseCallback = null;
    	}

    	void purchaseCancelled( string error )
    	{
    #if LLOG_FACEBOOKSTORE
    		Debug.Log( "purchase cancelled with error: "+error );
    #endif
            Platform.SafeCallback(_purchaseCallback, false);
            _purchaseCallback = null;
    	}

    	void purchaseFailed( string error )
    	{
    		Debug.LogError( "AppleStoreAPI.purchaseFailed with error: "+error);
            Platform.SafeCallback(_purchaseCallback, false);
            _purchaseCallback = null;
    	}

    	void productListReceived( List<StoreKitProduct> productList )
    	{
    #if LLOG_FACEBOOKSTORE
    		Debug.Log( "total productsReceived: " + productList.Count );
    		// Do something more useful with the products than printing them to the console
    		foreach( StoreKitProduct product in productList ) {
    			Debug.Log(string.Format( "StoreKitProduct - ID: {0} Price: {1}", product.productIdentifier, product.price));
    		}
    #endif
            if(productList.Count == 0)
            {
                UncleBill.DisableCashPurchase();
            }
            else if(productList.Count < itemIDs.Count)
            {
                foreach(string id in itemIDs)
                {
                    StoreKitProduct item = productList.Find((pd)=>{ return pd.productIdentifier == id; });
                    if(item == null)
                    {
                        UncleBill.DisableShopItemByPriceID(id);
                    }
                }
            }
            Platform.SafeCallback(_initializeCallback, true);
    	}

    	void productListRequestFailed( string error )
    	{
    		Debug.Log( "AppleStoreAPI.productListRequestFailed: "+error);
            Platform.SafeCallback(_initializeCallback, false);
    	}

        void restoreTransactionsFinished()
        {
    #if LLOG_FACEBOOKSTORE
            Debug.Log("AppleStoreAPI.restoreTransactionsFinished");
    #endif
            List<string> items = new List<string>();
            foreach(StoreKitTransaction tr in StoreKitBinding.getAllSavedTransactions())
            {
    #if LLOG_FACEBOOKSTORE
                Debug.Log("  - Item : "+tr.productIdentifier);
    #endif
                items.Add(tr.productIdentifier);
            }
            UncleBill.UpdateInventory(items.ToArray());
            Platform.SafeCallback(_restoreTransactionCallback, items.ToArray());
        }

        void restoreTransactionsFailed(string error)
        {
            Debug.LogError("AppleStoreAPI.restoreTransactionsFailed :"+error);
            Platform.SafeCallback(_restoreTransactionCallback, null);
        }
    #endregion

        bool CheckLocalPurchasedInfo(string productIdentifier)
        {
    #if LLOG_FACEBOOKSTORE
            Debug.Log("CheckLocalPurchasedInfo - " + productIdentifier);
    #endif

            bool purchased = false;
            List<StoreKitTransaction> transactionList = StoreKitBinding.getAllSavedTransactions();
    	    for (int i=0; i<transactionList.Count; i++) {
    		    StoreKitTransaction transaction = transactionList[i];
        	    if (transaction.productIdentifier == productIdentifier)
                {
        	        purchased = true;
    		        break;
                }
            }

    #if LLOG_FACEBOOKSTORE
            Debug.Log("End CheckLocalPurchasedInfo - " + purchased.ToString());
    #endif
            return purchased;
        }
    	*/
}

}

#endif
