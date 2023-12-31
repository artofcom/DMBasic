﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using Prime31;

#if UNITY_STANDALONE_OSX
public class StoreKitManager : AbstractManager
{
    public static bool autoConfirmTransactions = true;

    // Fired when the product list your required returns.  Automatically serializes the productString into StoreKitProduct's.
    public static event Action<List<StoreKitProduct>> productListReceivedEvent;

    // Fired when requesting product data fails
    public static event Action<string> productListRequestFailedEvent;

    // Fired when a product purchase has returned from Apple's servers and is awaiting completion. By default the plugin will finish transactions for you.
    // You can change that behaviour by setting autoConfirmTransactions to false which then requires that you call StoreKitBinding.finishPendingTransaction
    // to complete a purchase.
    public static event Action<StoreKitTransaction> productPurchaseAwaitingConfirmationEvent;

    // Fired when a product is successfully paid for.  returnValue will hold the productIdentifer and receipt of the purchased product.
    public static event Action<StoreKitTransaction> purchaseSuccessfulEvent;

    // Fired when a product purchase fails
    public static event Action<string> purchaseFailedEvent;

    // Fired when a product purchase is cancelled by the user or system
    public static event Action<string> purchaseCancelledEvent;

    // Fired when an error is encountered while adding transactions from the user's purchase history back to the queue
    public static event Action<string> restoreTransactionsFailedEvent;

    // Fired when all transactions from the user's purchase history have successfully been added back to the queue
    public static event Action restoreTransactionsFinishedEvent;

    // Fired when any SKDownload objects are updated by iOS. If using hosted content you should not be confirming the transaction until all downloads are complete.
    public static event Action<List<StoreKitDownload>> paymentQueueUpdatedDownloadsEvent;

    static StoreKitManager()
    {
        AbstractManager.initialize( typeof( StoreKitManager ) );
    }

    private static StoreKitManager _instance;

    void Start()
    {
        _instance = this;
    }

    void OnApplicationQuit()
    {
        _instance = null;
    }

    public static void staticRelay( string method, string param )
    {
        var methodInfo = _instance.GetType().GetMethod( method );
        if( methodInfo == null ) {
            Debug.LogError( "Failed to find method: " + method );
            return;
        }

        methodInfo.Invoke( _instance, new object[] { param } );
    }

    // Validates a Mac App Store receipt. The completion handler will return null if the receipt couldnt be found or validation failed.
    // If successful, a Hashtable is returned with all the receipt data
    public static IEnumerator validateMacAppStoreReceiptRemotely( bool useTestServer, Action<Dictionary<string,object>> completionHandler )
    {
        // find the actual instance of ourself
        var self = FindObjectOfType( typeof( StoreKitManager ) ) as StoreKitManager;
        if( self != null ) { // cant imagine how we could be but better to check
            yield return self.StartCoroutine( self.validateMacAppStoreReceiptInstance( useTestServer, completionHandler ) );
        }
    }

    public IEnumerator validateMacAppStoreReceiptInstance( bool useTestServer, Action<Dictionary<string,object>> completionHandler )
    {
        var pathToReceipt = System.IO.Path.Combine( Application.dataPath, "_MASReceipt/receipt" );
        if( System.IO.File.Exists( pathToReceipt ) ) {
            var bytes = System.IO.File.ReadAllBytes( pathToReceipt );
            var base64String = System.Convert.ToBase64String( bytes );

            // prep our post data
            var ht = new Hashtable();
            ht.Add( "receipt-data", base64String );
            var postData = System.Text.UTF8Encoding.UTF8.GetBytes( ht.toJson() );

            var url = useTestServer ? "https://sandbox.itunes.apple.com/verifyReceipt" : "https://buy.itunes.apple.com/verifyReceipt";
            var www = new WWW( url, postData );

            yield return www;

            if( www.error == null && www.text != null ) {
                completionHandler( www.text.dictionaryFromJson() );
            } else {
                if( www.error != null )
                    Debug.Log( "validation error: " + www.error );
                completionHandler( null );
            }
        } else {
            completionHandler( null );
        }
    }

    public void productPurchaseAwaitingConfirmation( string json )
    {
        if( productPurchaseAwaitingConfirmationEvent != null )
            productPurchaseAwaitingConfirmationEvent( StoreKitTransaction.transactionFromJson( json ) );

        if( autoConfirmTransactions )
            StoreKitBinding.finishPendingTransactions();
    }

    public void productPurchased( string json )
    {
        if( purchaseSuccessfulEvent != null )
            purchaseSuccessfulEvent( StoreKitTransaction.transactionFromJson( json ) );
    }

    public void productPurchaseFailed( string error )
    {
        if( purchaseFailedEvent != null )
            purchaseFailedEvent( error );
    }

    public void productPurchaseCancelled( string error )
    {
        if( purchaseCancelledEvent != null )
            purchaseCancelledEvent( error );
    }

    public void productsReceived( string json )
    {
        if( productListReceivedEvent != null )
            productListReceivedEvent( StoreKitProduct.productsFromJson( json ) );
    }

    public void productsRequestDidFail( string error )
    {
        if( productListRequestFailedEvent != null )
            productListRequestFailedEvent( error );
    }

    public void restoreCompletedTransactionsFailed( string error )
    {
        if( restoreTransactionsFailedEvent != null )
            restoreTransactionsFailedEvent( error );
    }

    public void restoreCompletedTransactionsFinished( string empty )
    {
        if( restoreTransactionsFinishedEvent != null )
            restoreTransactionsFinishedEvent();
    }

    public void paymentQueueUpdatedDownloads( string json )
    {
        if( paymentQueueUpdatedDownloadsEvent != null )
            paymentQueueUpdatedDownloadsEvent( StoreKitDownload.downloadsFromJson( json ) );
    }

}
#endif

