#define ENABLE_OSX
//#define ENABLE_IOS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using Prime31;

#if ( UNITY_STANDALONE_OSX && ENABLE_OSX ) || ( UNITY_IPHONE && ENABLE_IOS )
public class StoreKitBinding
{
#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern bool _storeKitCanMakePayments();

    // Checks to see if the currently logged in user can make payments
    public static bool canMakePayments()
    {
        return _storeKitCanMakePayments();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern string _storeKitGetAppStoreReceiptUrl();

    // iOS 7 only. Returns the location of the App Store receipt file. If called on an older iOS version it returns null.
    public static string getAppStoreReceiptLocation()
    {
        return _storeKitGetAppStoreReceiptUrl();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitRequestProductData( string productIdentifiers );

    // Accepts comma-delimited set of product identifiers
    public static void requestProductData( string[] productIdentifiers )
    {
        _storeKitRequestProductData( string.Join( ",", productIdentifiers ) );
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitPurchaseProduct( string productIdentifier, int quantity );

    // Purchases the given product and quantity
    public static void purchaseProduct( string productIdentifier, int quantity )
    {
        _storeKitPurchaseProduct( productIdentifier, quantity );
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitFinishPendingTransactions();

    // Finishes the pending transaction
    public static void finishPendingTransactions()
    {
        _storeKitFinishPendingTransactions();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitFinishPendingTransaction( string transactionIdentifier );

    // Finishes the pending transaction
    public static void finishPendingTransaction( string transactionIdentifier )
    {
        _storeKitFinishPendingTransaction( transactionIdentifier );
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitPauseDownloads();

    // Pauses any pending downloads
    public static void pauseDownloads()
    {
        _storeKitPauseDownloads();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitResumeDownloads();

    // Resumes any pending paused downloads
    public static void resumeDownloads()
    {
        _storeKitResumeDownloads();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitCancelDownloads();

    // Cancels any pending downloads
    public static void cancelDownloads()
    {
        _storeKitCancelDownloads();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern void _storeKitRestoreCompletedTransactions();

    // Restores all previous transactions.  This is used when a user gets a new device and they need to restore their old purchases.
    // DO NOT call this on every launch.  It will prompt the user for their password. Each transaction that is restored will have the normal
    // purchaseSuccessfulEvent fire for when restoration is complete.
    public static void restoreCompletedTransactions()
    {
        _storeKitRestoreCompletedTransactions();
    }

#if UNITY_IPHONE && ENABLE_IOS
    [DllImport("__Internal")]
#elif UNITY_STANDALONE_OSX && ENABLE_OSX
    [DllImport("StoreKitPlugin")]
#endif
    private static extern string _storeKitGetAllSavedTransactions();

    // Returns a list of all the transactions that occurred on this device.  They are stored in the Document directory.
    public static List<StoreKitTransaction> getAllSavedTransactions()
    {
        if( Application.platform == RuntimePlatform.OSXPlayer || ( Application.platform == RuntimePlatform.IPhonePlayer ) ) {
            // Grab the transactions and parse them out
            var json = _storeKitGetAllSavedTransactions();
            return StoreKitTransaction.transactionsFromJson( json );
        }

        return new List<StoreKitTransaction>();
    }

    // OSX only methods
#if UNITY_STANDALONE_OSX
    [DllImport("StoreKitPlugin")]
    private static extern string _storeKitUniqueDeviceIdentifier();

    // Returns a unique identifier for the current device and application
    public static string uniqueDeviceIdentifier()
    {
        return _storeKitUniqueDeviceIdentifier();
    }

    [DllImport("StoreKitPlugin")]
    private static extern string _storeKitUniqueGlobalDeviceIdentifier();

    // Returns a unique identifier for the current device
    public static string uniqueGlobalDeviceIdentifier()
    {
        return _storeKitUniqueGlobalDeviceIdentifier();
    }

    [DllImport("StoreKitPlugin")]
    private static extern bool _storeKitValidateMacAppStoreReceipt();

    // Validates the Mac App Store receipt
    public static bool validateMacAppStoreReceipt()
    {
        return _storeKitValidateMacAppStoreReceipt();
    }

    [DllImport("StoreKitPlugin")]
    private static extern string _storeKitFetchInAppPurchases();

    // Fetches a list of all the in app purchases that are currently in the receipt
    public static List<StoreKitMacReceipt> fetchInAppPurchasesFromReceipt()
    {
        var receipts = new List<StoreKitMacReceipt>();

        var json = _storeKitFetchInAppPurchases();
        var arr = json.listFromJson();

        foreach( Dictionary<string,object> dict in arr )
            receipts.Add( StoreKitMacReceipt.receiptFromDictionary( dict ) );

        return receipts;
    }
#endif

    // iOS only methods
#if UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern void _storeKitDisplayStoreWithProductId (string productId);

    // iOS 6+ only! Displays the App Store with the given productId in app
    public static void displayStoreWithProductId( string productId )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            _storeKitDisplayStoreWithProductId( productId );
    }
#endif

}
#endif

