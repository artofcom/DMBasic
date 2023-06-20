using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NOVNINE
{

public static class Platform
{
    public static PlatformContext Info
    {
        get {
            return Context.NNPlatform;
        }
    }

    public static GameObject Holder
    {
        get {
            return Module.Holder;
        }
    }

    public static void Init()
    {
        Module.Init();

        //FIXME
        //move somewhere it belongs, not here
//#if UNITY_ANDROID && !UNITY_EDITOR
//        using( var pluginClass = new AndroidJavaObject( "com.bitmango.bitmangoext.bitmangoext")) {
//            bool isAsyncExist = pluginClass.CallStatic<bool>("init");
//        }
//#endif
    }

    public static void SystemAlert(string title, string message, string positiveButton, string negativeButton = null, System.Action<bool> callback = null)
    {
        NativeInterface.SystemAlert(title, message, positiveButton, negativeButton, callback);
    }

    public static void SafeCallback(System.Action func)
    {
        if(func != null)
            func();
    }

    public static void SafeCallback<T>(System.Action<T> func, T param)
    {
        if(func != null)
            func(param);
    }

    public static void BroadcastMessage(string msg, object param = null)
    {
        foreach (Transform xform in UnityEngine.Object.FindObjectsOfType<Transform>())	
        {   	
            if (xform.parent == null)	
            {   	
                xform.BroadcastMessage(msg, param, SendMessageOptions.DontRequireReceiver);	
            }   	
        }
    }

//    public static void SetRecoverPoint(string name) 
//    {
//        Context.NNPlatform.recoverPoint = name;
//    }
}

}
