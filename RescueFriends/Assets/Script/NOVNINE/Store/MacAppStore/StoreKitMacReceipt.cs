using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_STANDALONE_OSX
public class StoreKitMacReceipt
{
    public string productIdentifier;
    public string transactionId;
    public string originalPurchaseDate;
    public string purchaseDate;
    public string originalTransactionId;
    public int quantity;

    public static StoreKitMacReceipt receiptFromDictionary( Dictionary<string,object> dict )
    {
        var receipt = new StoreKitMacReceipt();

        if( dict.ContainsKey( "productId" ) )
            receipt.productIdentifier = dict["productId"].ToString();

        if( dict.ContainsKey( "transactionId" ) )
            receipt.transactionId = dict["transactionId"].ToString();

        if( dict.ContainsKey( "originalPurchaseDate" ) )
            receipt.originalPurchaseDate = dict["originalPurchaseDate"].ToString();

        if( dict.ContainsKey( "purchaseDate" ) )
            receipt.purchaseDate = dict["purchaseDate"].ToString();

        if( dict.ContainsKey( "originalTransactionId" ) )
            receipt.originalTransactionId = dict["originalTransactionId"].ToString();

        if( dict.ContainsKey( "quantity" ) )
            receipt.quantity = int.Parse( dict["quantity"].ToString() );

        return receipt;
    }

    public override string ToString()
    {
        return string.Format( "<StoreKitMacReceipt>\nproductId: {0}\npurchaesDate: {1}\nquantity: {2}", productIdentifier, purchaseDate, quantity );
    }
}
#endif

