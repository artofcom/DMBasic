using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NOVNINE.Store
{

    public class DummyAPI : IBillingMethod
    {
        private System.Action<string> _initializeCallback;
        private System.Action<bool> _restoreTransactionCallback;
        private System.Action<string> _purchaseCallback;

        public void Initialize(System.Action<string> callback)
        {
            _initializeCallback = callback;
            TaskManager.DoSecondsAfter(()=> {
                Platform.SafeCallback(_initializeCallback, "success");
            }, 1);
        }

        public void Dispose()
        {
        }

        public void Purchase(string productIdentifier, bool consumable, System.Action<string> callback)
        {
            Debug.Log("DummyAPI.Purchase : "+productIdentifier);
            _purchaseCallback = callback;

            TaskManager.DoSecondsAfter(()=> {
                if(consumable) {
                    Scene.UnlockWithMsg();
                    Platform.SafeCallback(_purchaseCallback, NOVNINE.Store.UncleBill.PURCHASE_SUCCEEDED);
                } else {
                    Platform.SafeCallback(_purchaseCallback, "success");
                }
            }, 1);
        }

        public void ConsumePurchase(string productIdentifier)
        {
            Debug.Log("DummyAPI.ConsumePurchase : ");
        }

        public void RestoreAllTransactions(System.Action<string[]> callback)
        {
            Debug.Log("DummyAPI.RestoreAllTransactions");
    //        TaskManager.DoSecondsAfter(()=> {
    //            Platform.SafeCallback(callback, null);
    //        }, 3);
        }

        public void OpenStore(string storeParam = null, bool reviewPage = false)
        {
            Debug.Log("DummyAPI.OpenStore");
    //        NOVNINE.NativeInterface.SystemAlert("OpenStore", storeParam, "OK");
        }

        public void SearchStore(string keyword)
        {
            Debug.Log("DummyAPI.SearchStore: "+keyword);
    //        NOVNINE.NativeInterface.SystemAlert("SearchStore", keyword, "OK");
        }

    }

}

