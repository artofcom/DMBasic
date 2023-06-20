using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

using NOVNINE.Native;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
using NInterface = NOVNINE.Native.DummyExt;
#elif UNITY_IPHONE
using NInterface = NOVNINE.Native.iOSExt;
#elif UNITY_ANDROID
using NInterface = NOVNINE.Native.AndroidExt;
#elif UNITY_WEBPLAYER
using NInterface = NOVNINE.Native.WebPlayerExt;
#else
using NInterface = NOVNINE.Native.DummyExt;
#endif

namespace NOVNINE
{

public static class NativeInterface
{
    public static bool IsMusicPlaying()
    {
        return NInterface.IsMusicPlaying();
    }

    public static string GetCurrentLocaleID()
    {
        return NInterface.GetCurrentLocaleID();
    }

    public static string GetBundleVersion()
    {
        return NInterface.GetBundleVersion();
    }

    public static string GetBundleIdentifier()
    {
        return NInterface.GetBundleIdentifier();
    }

    public static bool CanOpenURL(string url)
    {
        return NInterface.CanOpenURL(url);
    }

	public static void OpenURL(string url)
	{
//        Platform.SetRecoverPoint("openurl");
		NInterface.OpenURL(url);
	}

    public static bool IsCloudAvailable()
    {
        return NInterface.IsCloudAvailable();
    }

    public static bool SaveUserDataToCloud()
    {
        return NInterface.SaveUserDataToCloud();
    }

    public static bool LoadUserDataFromCloud()
    {
        return NInterface.LoadUserDataFromCloud();
    }

	public static void SendFeedback(string mail, string optional = null)
	{
        string gamename = "";
		gamename = (NOVNINE.Context.NNPlatform.appName.EN).Replace(" ","");
        if(gamename == "") gamename = (NOVNINE.Context.NNPlatform.appID).Replace(" ","");
		//string ADID = NOVNINE.Profile.ADID;
        //string subject = string.Format("[USERFEED] {0} v{1} {2} {3} Support", gamename, NOVNINE.NativeInterface.GetBundleVersion(),  NOVNINE.Context.NNPlatform.storeType.ToString(), ADID);
        string accountid        = "TestClient";
        string subject = string.Format("[USERFEED] {0} v{1} {2} [{3}] Support", gamename, NOVNINE.NativeInterface.GetBundleVersion(),  NOVNINE.Context.NNPlatform.storeType.ToString(), accountid);
        if(optional != null)
            subject += ":"+optional;
		string URL = ("mailto:"+mail+"?subject="+subject).Replace(" ", "%20");
        //Platform.SetRecoverPoint("feed");
		Application.OpenURL(URL);
	}

    public static string AdvertiserIdentifier()
	{
		return NInterface.AdvertiserIdentifier ();
	}

	public static void ApplicationQuit()
	{
		NInterface.ApplicationQuit ();
	}

    public static void SystemAlert(string title, string message, string positiveButton, string negativeButton = null, System.Action<bool> callback = null)
    {
        //Platform.SetRecoverPoint("alert");
        NInterface.SystemAlert(Locale.GetString(title), Locale.GetString(message), Locale.GetString(positiveButton), Locale.GetString(negativeButton), callback);
    }
		
	public static void ShowWebPage(string url)
    {
        //Platform.SetRecoverPoint("webpage");
		NInterface.ShowWebPage(url);
	}

    public static void ChoosePhotoAlbum(System.Action<Texture2D> callback)
    {
        //Platform.SetRecoverPoint("photoalbum");
        NInterface.ChoosePhotoAlbum(callback);
    }

    public static bool HasCamera()
    {
        return NInterface.HasCamera();
    }

	public static void ShowShare(string title, string message) {
        //Platform.SetRecoverPoint("showshare");
		NInterface.ShowShare (title, message);
	}

	public static void ShowShare(string title, string message, System.Action<bool> callback) {
        //Platform.SetRecoverPoint("showshare");
		NInterface.ShowShare (title, message, callback);
	}

	public static void ShowShare(string title, string message, Texture2D texture, System.Action<bool> callback) {
		//Platform.SetRecoverPoint("showshare");
        NInterface.ShowShare(title, message, texture, callback);
	}

	public static void TakePhoto(int width, int height, string fileName, System.Action<string> callback) {
        //Platform.SetRecoverPoint("takephoto");
		NInterface.TakePhoto(width, height, fileName, callback);
	}

	public static void SelectAlbum(int width, int height, string fileName, System.Action<string> callback) {
        //Platform.SetRecoverPoint("selectalbum");
		NInterface.SelectAlbum(width, height, fileName, callback);
	}
		
#region AlertView
	
	public static void ShowAlert(string title, string message, string positiveButton, System.Action<bool> callback)
	{
         //Platform.SetRecoverPoint("showalert");
#if UNITY_EDITOR
			bool positiveClick = EditorUtility.DisplayDialog(title, message, positiveButton);
        	if(callback != null)
				callback(positiveClick);
#else
			NInterface.ShowAlert(title, message, positiveButton, callback);
#endif
    }
		
    public static void ShowAlert(string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback)
	{
        //Platform.SetRecoverPoint("showalert");
#if UNITY_EDITOR
			bool positiveClick = EditorUtility.DisplayDialog(title, message, positiveButton, negativeButton);
			if(callback != null)
				callback(positiveClick);
#else
			NInterface.ShowAlert(title, message, positiveButton, negativeButton, callback);
#endif
			
	}

#endregion
}

}
