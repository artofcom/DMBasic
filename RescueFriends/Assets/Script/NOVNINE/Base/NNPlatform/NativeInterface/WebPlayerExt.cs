#if UNITY_WEBPLAYER
using UnityEngine;
using System;
using System.Reflection;
using JsonFx.Json;
using NOVNINE.Diagnostics;

namespace NOVNINE.Native
{

public static class WebPlayerExt
{
    public static bool IsMusicPlaying()
    {
        return false;
    }

    public static string GetCurrentLocaleID()
    {
        Debugger.Assert(false, "WebPlayerExt.GetCurrentLocaleID not supported");
        return "en";
    }

    public static string GetBundleVersion()
    {
        //return "1.0";
        return NOVNINE.Context.BMPlatform.bundleVersion;
    }

    public static string GetBundleIdentifier()
    {
        //return "com.bitmango."+NOVNINE.Context.BMPlatform.appID;
        return NOVNINE.Context.BMPlatform.bundleIdentifier;
    }

	public static string AdvertiserIdentifier()
	{
		return "";
	}

	public static void ApplicationQuit()
	{
		Application.Quit ();
	}

    public static bool CanOpenURL(string url)
    {
        return true;
    }

    public static bool IsCloudAvailable()
    {
        return false;
    }

    public static bool SaveUserDataToCloud()
    {
        return false;
    }

    public static bool LoadUserDataFromCloud()
    {
        return false;
    }

    public static string escapeForJS(string input)
    {
        string output = input.Replace("\"", "\\\"");
        output = output.Replace("\'", "\\\'");
        return output;
    }

    //SEE FacebookContext for "SystemAlertCallback"
    public static System.Action<bool> systemAlertCallback;
    public static void SystemAlert(string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback)
    {
        systemAlertCallback = callback;
        title = escapeForJS(title);
        message = escapeForJS(message);
        string parms = "\'"+message+"\' , \'"+title+"\'";
        string cmd =
            "if(confirm("+parms+") == true) {"+
				"u.getUnity().SendMessage(\"__NOVNINEPlatform__\",\"SystemAlertCallback\",\"ok\"); }"+
            "else {"+
				"u.getUnity().SendMessage(\"__NOVNINEPlatform__\",\"SystemAlertCallback\",\"cancel\"); }";
        Application.ExternalEval(cmd);
        Time.timeScale = 0;
    }
		
	public static void ShowWebPage(string url)
	{
        Application.ExternalEval(
            "var windowName = 'userConsole';\n"+
            "var popUp = window.open('"+url+"', 'BitMangoWnd');\n"+
            "if (popUp == null || typeof(popUp)=='undefined') {\n"+
            "   alert('Please disable your pop-up blocker and Try Again.');\n"+
            "} else {\n"+
            "   popUp.focus();\n"+
            "}");
	}	

    public static void ChoosePhotoAlbum(System.Action<Texture2D> callback)
    {
        Debug.LogError("ChoosePhotoAlbum is not implemented here");
        Platform.SafeCallback(callback, null);
    }
    public static bool HasCamera()
    {
        return false;
    }

	public static void ShowShare(string title, string message) {
		return;
	}
	public static void TakePhoto(int width, int height, string fileName, System.Action<string> callback) {
		return;
	}
	public static void SelectAlbum(int width, int height, string fileName, System.Action<string> callback) {
		return;
	}
	public static void ShowShare(string title, string message, Texture2D texture) {
		return;
	}
    public static void ShowAlert(string title, string message, string positiveButton, System.Action<bool> callback) {
        return;
    }
    public static void ShowAlert(string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback) {
        return;
    }
}

}
#endif

