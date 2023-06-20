using UnityEngine;
using System.Collections;
using NOVNINE.Diagnostics;

namespace NOVNINE
{

public static partial class NNTool
{
#if UNITY_ANDROID
    private static AndroidJavaObject displayMetrics;
    private static AndroidJavaObject DisplayMetrics
    {
        get {
            if(displayMetrics == null) {
                displayMetrics = new AndroidJavaObject("android.util.DisplayMetrics");

                var unityPlayerClass =  new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                var windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager");
                var displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay");
                displayInstance.Call("getMetrics", displayMetrics);
            }
            return displayMetrics;
        }
    }
#endif


    public static bool IsTablet()
    {
#if UNITY_IPHONE
        /*
                switch(iPhone.generation) {
                    case iPhoneGeneration.iPhone:
                    case iPhoneGeneration.iPhone3G:
                    case iPhoneGeneration.iPhone3GS:
                    case iPhoneGeneration.iPodTouch1Gen:
                    case iPhoneGeneration.iPodTouch2Gen:
                    case iPhoneGeneration.iPodTouch3Gen:
                    case iPhoneGeneration.iPad1Gen:
                    case iPhoneGeneration.iPhone4:
                    case iPhoneGeneration.iPodTouch4Gen:
                    case iPhoneGeneration.iPhone4S:
                    case iPhoneGeneration.iPhone5:
                    case iPhoneGeneration.iPodTouch5Gen:
                    case iPhoneGeneration.iPhone5C:
                    case iPhoneGeneration.iPhone5S:
                    case iPhoneGeneration.iPhoneUnknown:
                    case iPhoneGeneration.iPodTouchUnknown:

                    case iPhoneGeneration.iPad2Gen:
                    case iPhoneGeneration.iPad3Gen:
                    case iPhoneGeneration.iPadMini1Gen:
                    case iPhoneGeneration.iPad4Gen:
                    case iPhoneGeneration.iPadUnknown:
                    break;
                }
        */
        return SystemInfo.deviceModel.StartsWith("iPad");
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) {
            var HeightPixels = DisplayMetrics.Get<int>("heightPixels");
            var WidthPixels = DisplayMetrics.Get<int>("widthPixels");
            var XDPI = DisplayMetrics.Get<float>("xdpi");
            var YDPI = DisplayMetrics.Get<float>("ydpi");
            Vector2 size = new Vector2(WidthPixels / XDPI, HeightPixels / YDPI);
            return size.magnitude >= 6;
        } else {
            return false;
        }
#else
        return false;
#endif
    }

    public static float GetScreenDPI()
    {
        float dpi = Screen.dpi;

        //FIXME
#if UNITY_IPHONE
        if(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone6Plus) {
            Debug.Log("NNTool.GetScreenDPI : iPhone6+ fix "+Screen.dpi+" -> "+461);
            dpi = 461;//not 480
        }
#endif
        if(dpi == 0f) {
            Debug.Log("This device is not supported. Default DPI Return.");
            dpi = 160f;
        }
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) {
            var YDPI = DisplayMetrics.Get<float>("ydpi");
            dpi = YDPI;
        }
#endif
        return dpi;
    }

    public static float ConvertMillimeterToWorldLength(Camera camera, float millimeter = 1.0f)
    {
        Debugger.Assert(camera != null, "Camera is null");
        Debugger.Assert(camera.orthographic, "Camera is not orthographic");
        if(camera == null) return -1;
        float dpi = GetScreenDPI();
        float pixelInMillimeter = dpi/25.4f;
        /*Debug.Log("Screen Size : "+Screen.width+"/"+Screen.height);
        Debug.Log("GetScreenDPI : "+dpi);
        Debug.Log("pixelInMillimeter : "+pixelInMillimeter);
        */
        return ConvertPixelToWorldLength(camera) * pixelInMillimeter * millimeter;
    }

    public static float ConvertPixelToWorldLength(Camera camera, float pixel = 1.0f)
    {
        Debugger.Assert(camera != null, "Camera is null");
        Debugger.Assert(camera.orthographic, "Camera is not orthographic");
        if(camera == null) return -1;

        tk2dCamera tk2dCam = camera.gameObject.GetComponent<tk2dCamera>();
        float pixelHeight = camera.pixelHeight;
#if UNITY_IPHONE
        if(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone6Plus) { 
            //1242x2208
            if(camera.pixelHeight > camera.pixelWidth)
                pixelHeight = 2208;
            else
                pixelHeight = 1242;
            Debug.Log("NNTool.ConvertPixelToWorldLength : iPhone6+ fix "+pixelHeight);
        }
#endif
        if(tk2dCam != null) {
            return tk2dCam.ScreenExtents.height / (pixelHeight) * pixel;
        } else {
            return (camera.orthographicSize * 2.0f) / (float)pixelHeight * pixel;
        }
    }
}

}

