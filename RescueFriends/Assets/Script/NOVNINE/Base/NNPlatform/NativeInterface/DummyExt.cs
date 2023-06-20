using UnityEngine;
using System;
using System.Reflection;
using JsonFx.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NOVNINE.Native
{

public static class DummyExt
{
    public static bool IsMusicPlaying()
    {
        return false;
    }

    public static string GetCurrentLocaleID()
    {
        return "KR";
    }

    public static string GetBundleVersion()
    {
#if UNITY_EDITOR
        return PlayerSettings.bundleVersion;
#else
        return "1.0";
#endif
    }

    public static string GetBundleIdentifier()
    {
#if UNITY_EDITOR
        return PlayerSettings.applicationIdentifier;
#else
        return "com.nov9.jellomagicfirends";
#endif
    }
	
	public static string AdvertiserIdentifier()
	{
		return SystemInfo.deviceUniqueIdentifier+"_ADID";
	}
	
	public static void ApplicationQuit()
	{
		Application.Quit ();
	}

    public static bool CanOpenURL(string url)
    {
        return true;
    }

	public static void OpenURL(string url)
	{
		return;
	}
    public static bool IsCloudAvailable()
    {
        return false;
    }

    public static bool SaveUserDataToCloud()
    {
#if UNITY_IPHONE
//#if USE_UncleBill
//        Wallet.current.SaveToiCloud();
//#endif

        Type cpType = Type.GetType("ContextPool");
        if(cpType != null) {
            //FIXME. need to restore original ResourceFileSystem
            JsonFileSystem.SetFileSystem( new iCloudFileSystem() );
            MethodInfo method = cpType.GetMethod("SaveAllContext");
            method.Invoke(null, null);
            //ContextPool.SaveAllContext();
            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );
        }
#endif
        return false;
    }

    public static bool LoadUserDataFromCloud()
    {
#if UNITY_IPHONE
//#if USE_UncleBill
//        Wallet.current.LoadFromiCloud();
//#endif
        Type cpType = Type.GetType("ContextPool");
        if(cpType != null) {
            //FIXME. need to restore original ResourceFileSystem
            JsonFileSystem.SetFileSystem( new iCloudFileSystem() );
            MethodInfo method = cpType.GetMethod("LoadAllContext");
            method.Invoke(null, null);
            //ContextPool.LoadAllContext();
            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );
        }
        JsonFileSystem.SetFileSystem( new ResourceFileSystem() );
#endif
        return false;
    }

    public static void SystemAlert(string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback)
    {
#if UNITY_EDITOR
        EditorUtility.DisplayDialog(Locale.GetString(title), Locale.GetString(message), Locale.GetString(positiveButton), Locale.GetString(negativeButton));
//        TaskManager.DoNextFrame(()=> {
//            Platform.SafeCallback(callback, positiveClick);
//        });
#else
        Debug.LogError("DummyExt.SystemAlert Fail : Not Impl.");
#endif
    }
		
	public static void ShowWebPage(string url)
	{
		Application.OpenURL(url);	
	}

    public static void ChoosePhotoAlbum(System.Action<Texture2D> callback)
    {
#if UNITY_EDITOR
//        string path = EditorUtility.OpenFilePanel("Choose a photo", "", "");
//        var www = new WWW("file:///" + path);
//        Platform.SafeCallback(callback, www.texture);
#else
//        Debug.LogError("ChoosePhotoAlbum is not implemented here");
//        Platform.SafeCallback(callback, null);
#endif
    }
    public static bool HasCamera()
    {
        return true;
    }

	public static void ShowShare(string title, string message) {
		return;
	}

	public static void ShowShare(string title, string message, System.Action<bool> callback) {
		return;
	}

	public static void ShowShare(string title, string message, Texture2D texture, System.Action<bool> callback) {
		return;
	}

	public static void TakePhoto(int width, int height, string fileName, System.Action<string> callback) {
		return;
	}

	public static void SelectAlbum(int width, int height, string fileName, System.Action<string> callback) {
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

