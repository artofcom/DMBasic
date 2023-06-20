using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Facebook.Unity;

public class FacebookAnalytics : MonoBehaviour 
{
    /*
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (FB.IsInitialized) 
        {
            FB.ActivateApp();
        } else {
            //Handle FB.Init
            FB.Init( () => {
                FB.ActivateApp();

                var tutParams = new Dictionary<string, object>();
                tutParams["Step"] = "1";
                tutParams["Description"] = "Test1";

                FB.LogAppEvent (
                    "Traking_Log",
                    1,
                    tutParams
                );

            });
        }
    }

//    public static void SendEventFB(string eventName,float value, Dictionary<string, object> Params)
//    {
//        if (FB.IsInitialized)
//        {
//            FB.LogAppEvent (
//                eventName,
//                value,
//                Params
//            );   
//        }
//    }
//
    // Unity will call OnApplicationPause(false) when an app is resumed
    // from the background
    void OnApplicationPause (bool pauseStatus)
    {
        // Check the pauseStatus to see if we are in the foreground
        // or background
        if (!pauseStatus) 
        {
            //app resume
            if (FB.IsInitialized) 
            {
                FB.ActivateApp();
            }
            else
            {
                //Handle FB.Init
                FB.Init( () => {
                    FB.ActivateApp();
                });
            }
        }
    }*/
}
