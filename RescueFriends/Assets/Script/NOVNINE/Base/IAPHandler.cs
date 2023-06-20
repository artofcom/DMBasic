using UnityEngine;
using System.Collections;
using Sdkbox;
using System;
using System.Collections.Generic;

public class IAPHandler : MonoBehaviour {

    // accessor.
    static IAPHandler mHandler = null;
    public static IAPHandler getInstance()
    {
        return mHandler;
    }
    //

    // IAP.
    public IAP _iap             { get; set; }
    [HideInInspector]
    public Product _curProduct;

    List<Product> _listProducts = new List<Product>();
    Action _cbPurchaseSuccessed = null;
    Action _cbPurchaseFailed    = null;

    void Awake()
    {
        mHandler                = this;
        DontDestroyOnLoad(transform.gameObject);

        _iap                    = FindObjectOfType<Sdkbox.IAP>();
        if(null == _iap)
        {
            Debug.Log("Failed to find IAP instance");
        }
    }

	// Use this for initialization
	//void Start () {}
	
	// Update is called once per frame
	//void Update () {}

    public Product getProduct(string id)
    {
        for(int q = 0; q < _listProducts.Count; ++q)
        {
            if(_listProducts[q].id == id)
                return _listProducts[q];
        }
        
        Product p               = default(Product);
        p.id                    = "";
        return p;
    }

    public void purchase(string id, Action callBackSuccess, Action callbackFailed)
    {
        _cbPurchaseSuccessed    = callBackSuccess;
        _cbPurchaseFailed       = callbackFailed;
        _iap.purchase( id );

        // callbackFailed();
    }

    #region => IAP callbacks.
    public void onInitialized(bool status)
    {
        Debug.Log("PurchaseHandler.onInitialized " + status);
    }
    
    public void onSuccess(Product product)
    {
        Debug.Log("PurchaseHandler.onSuccess: " + product.name);
        _curProduct             = product;

        if(null != _cbPurchaseSuccessed)
            _cbPurchaseSuccessed();

        //Firebase.Analytics.FirebaseAnalytics.EventEcommercePurchase..
        //Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventEcommercePurchase, 
        //    new Firebase.Analytics.Parameter[] {
        //        new Firebase.Analytics.Parameter("product_id", product.id),
        //        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, product.currencyCode), 
        //        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, product.priceValue)
        //    });

    }

    public void onFailure(Product product, string message)
    {
        Debug.Log("PurchaseHandler.onFailure " + message);// + " " + product.id);
        _curProduct             = product;
        
        if(null != _cbPurchaseFailed)
            _cbPurchaseFailed();

#if !UNITY_EDITOR
       Firebase.Analytics.FirebaseAnalytics.LogEvent("purchase_failed", "product_id", product.id);
#endif

    }

    public void onCanceled(Product product)
    {
        Debug.Log("PurchaseHandler.onCanceled product: ");// + product.name);
        _curProduct             = product;

        if(null != _cbPurchaseFailed)
            _cbPurchaseFailed();

#if (UNITY_IPHONE || UNITY_ANDROID)
        //UnityEngine.Analytics.Analytics.CustomEvent("IAP", new Dictionary<string, object>
        //{
        //    { "status", "canceled" }
        //});
#endif
    }








    public void onRestored(Product product)
    {
        Debug.Log("PurchaseHandler.onRestored: " + product.name);
        //globals.getInstance()._listRestoredProducts.Add(product);
    }

    public void onProductRequestSuccess(Product[] products)
    {
        Debug.Log("PurchaseHandler.onProductRequestSuccess : " + products.Length);

        //globals.getInstance().on_IAP_ProductRequestSuccess_Start();
        _listProducts.Clear();
        foreach (var p in products)
        {
            Debug.Log("Product: " + p.name + " price: " + p.price);
            //_curProduct         = p;
            _listProducts.Add( p );
     //       globals.getInstance().on_IAP_ProductRequestSuccess();
        }
       // globals.getInstance().on_IAP_ProductRequestSuccess_End();
    }

    public void onProductRequestFailure(string message)
    {
        Debug.Log("PurchaseHandler.onProductRequestFailure: " + message);
    }

    public void onRestoreComplete(string message)
    {
        Debug.Log("PurchaseHandler.onRestoreComplete: " + message);
    }
#endregion

}
