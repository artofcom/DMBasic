//#define LLOG_APPLESTORE

#if UNITY_STANDALONE_OSX && USE_UncleBill_MacAppStore

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NOVNINE.Store
{

public class MacAppStoreAPI : IBillingMethod
{
    private System.Action<string> _initializeCallback;
    private System.Action<string[]> _restoreTransactionCallback;
    private System.Action<string> _purchaseCallback;
    private List<string> itemIDs;

    public void Initialize(System.Action<string> callback)
    {
        _initializeCallback = callback;

        Module.Holder.CreateTemporaryGameObjectWithComponent<StoreKitManager>();

        StoreKitManager.purchaseSuccessfulEvent += purchaseSuccessful;
        StoreKitManager.purchaseCancelledEvent += purchaseCancelled;
        StoreKitManager.purchaseFailedEvent += purchaseFailed;

        StoreKitManager.restoreTransactionsFinishedEvent += restoreTransactionsFinished;
        StoreKitManager.restoreTransactionsFailedEvent += restoreTransactionsFailed;

        StoreKitManager.productListReceivedEvent += productListReceived;
        StoreKitManager.productListRequestFailedEvent += productListRequestFailed;

        if(!StoreKitBinding.canMakePayments()) {
            Platform.SafeCallback(_initializeCallback, "StoreKitBinding.canMakePayments is false");
            return;
        }

        itemIDs = new List<string>();
        foreach(ShopItem item in Context.UncleBill.shopItems) {
            itemIDs.Add(item.GetPriceID());
//            if(item.IsCashPrice()) {
//                itemIDs.Add(item.GetPriceID());
//            }
        }
        StoreKitBinding.requestProductData(itemIDs.ToArray());
    }

    public void Dispose()
    {
        StoreKitManager.purchaseSuccessfulEvent -= purchaseSuccessful;
        StoreKitManager.purchaseCancelledEvent -= purchaseCancelled;
        StoreKitManager.purchaseFailedEvent -= purchaseFailed;

        StoreKitManager.restoreTransactionsFinishedEvent -= restoreTransactionsFinished;
        StoreKitManager.restoreTransactionsFailedEvent -= restoreTransactionsFailed;

        StoreKitManager.productListReceivedEvent -= productListReceived;
        StoreKitManager.productListRequestFailedEvent -= productListRequestFailed;
        //Object.Destroy(Module.Holder.GetComponent<StoreKitManager>());
    }

    public void Purchase(string productIdentifier, bool consumable, System.Action<string> callback)
    {
        _purchaseCallback = callback;

        if(consumable) {
            StoreKitBinding.purchaseProduct(productIdentifier, 1);
        } else {
            if(CheckLocalPurchasedInfo(productIdentifier)) {
                //already purchased
                Platform.SafeCallback(_purchaseCallback, "success");
            } else {
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

    public void OpenStore(string storeParam = null, bool reviewPage = false)
    {
        Debug.LogError("MacAppStoreAPI.OpenStore Not Implemented");

    }

    public void SearchStore(string keyword)
    {
        Debug.LogError("MacAppStoreAPI.SearchStore Not Implemented");

    }

    #region Callbacks
    void purchaseSuccessful( StoreKitTransaction transaction )
    {
#if LLOG_APPLESTORE
        Debug.Log( "purchased product: "+transaction.productIdentifier.ToString() + "  " + transaction.quantity );
#endif
        Platform.SafeCallback(_purchaseCallback, "success");
        _purchaseCallback = null;
    }

    void purchaseCancelled( string error )
    {
#if LLOG_APPLESTORE
        Debug.Log( "purchase cancelled with error: "+error );
#endif
        Platform.SafeCallback(_purchaseCallback, error);
        _purchaseCallback = null;
    }

    void purchaseFailed( string error )
    {
        Debug.LogError( "AppleStoreAPI.purchaseFailed with error: "+error);
        Platform.SafeCallback(_purchaseCallback, error);
        _purchaseCallback = null;
    }

    void productListReceived( List<StoreKitProduct> productList )
    {
#if LLOG_APPLESTORE
        Debug.Log( "total productsReceived: " + productList.Count );
        // Do something more useful with the products than printing them to the console
        foreach( StoreKitProduct product in productList ) {
            Debug.Log(string.Format( "StoreKitProduct - ID: {0} Price: {1}", product.productIdentifier, product.price));
        }
#endif
        if(productList.Count == 0) {
            UncleBill.DisableCashPurchase();
        } else if(productList.Count < itemIDs.Count) {
            foreach(string id in itemIDs) {
                StoreKitProduct item = productList.Find((pd)=> {
                    return pd.productIdentifier == id;
                });
                if(item == null) {
                    UncleBill.DisableShopItemByPriceID(id);
                }
            }
        }
        Platform.SafeCallback(_initializeCallback, "success");
    }

    void productListRequestFailed( string error )
    {
        Debug.Log( "AppleStoreAPI.productListRequestFailed: "+error);
        Platform.SafeCallback(_initializeCallback, error);
    }

    void restoreTransactionsFinished()
    {
#if LLOG_APPLESTORE
        Debug.Log("AppleStoreAPI.restoreTransactionsFinished");
#endif
        List<string> items = new List<string>();
        foreach(StoreKitTransaction tr in StoreKitBinding.getAllSavedTransactions()) {
#if LLOG_APPLESTORE
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
#if LLOG_APPLESTORE
        Debug.Log("CheckLocalPurchasedInfo - " + productIdentifier);
#endif

        bool purchased = false;
        List<StoreKitTransaction> transactionList = StoreKitBinding.getAllSavedTransactions();
        for (int i=0; i<transactionList.Count; i++) {
            StoreKitTransaction transaction = transactionList[i];
            if (transaction.productIdentifier == productIdentifier) {
                purchased = true;
                break;
            }
        }

#if LLOG_APPLESTORE
        Debug.Log("End CheckLocalPurchasedInfo - " + purchased.ToString());
#endif
        return purchased;
    }
}

}

#endif

