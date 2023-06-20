using UnityEngine;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using JsonFx.Json;
using System.IO;
using System.Collections;

#if UNITY_IPHONE

namespace NOVNINE.Native
{

public static class iOSExt
{
    private static System.Action<bool> _alertCallback;
	private static System.Action<bool> _showShareCallback;

    private static string _positiveButton;
    private static GCHandle _handle;

    [DllImport ("__Internal")]
    private static extern bool _IsMusicPlaying();

    [DllImport("__Internal")]
    private static extern string _GetISO3CountryCode();



#region dwialert callback types
    internal delegate void AlertButtonClick(String click);
	internal delegate void ShowShareClose(bool isComplete);
#endregion

#region interstitial callback methods
    [AOT.MonoPInvokeCallback(typeof(AlertButtonClick))]
    private static void AlertButtonClicked(String button)
    {   
		Debug.Log( "NI alert button clicked: " + button);

        if (null != _alertCallback) {
			_alertCallback(button == _positiveButton);
            _alertCallback = null;
        }   
        //IntPtrToInterstitialClient(interstitialClient).OnAdLoaded();
    } 

	[AOT.MonoPInvokeCallback(typeof(ShowShareClose))]
	private static void ShowShareClosed(bool isCompleted)
	{   
		Debug.Log( "NI show share closed - share complete : " + isCompleted);
		
		if (null != _showShareCallback) {
			_showShareCallback(isCompleted);
			_showShareCallback = null;
		}   
		//IntPtrToInterstitialClient(interstitialClient).OnAdLoaded();
	}
#endregion

    public static string GetISO3CountryCode()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            return _GetISO3CountryCode();
        } else {
            return "NULL";
        }
    }

    public static bool IsMusicPlaying()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            return _IsMusicPlaying();
        } else {
            return false;
        }
    }

	[DllImport ("__Internal")]
	private static extern string _getAdvertisingIdentifier();
	public static string AdvertiserIdentifier()
	{
		return _getAdvertisingIdentifier();
	}

	public static void ApplicationQuit()
	{
		Application.Quit ();
	}
	
    [DllImport ("__Internal")]
    private static extern string _iOSGetCurrentLocaleID();

    public static string GetCurrentLocaleID()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            return _iOSGetCurrentLocaleID();
        } else {
            return "";
        }
    }

    [DllImport ("__Internal")]
    private static extern string _iOSGetBundleVersion();

    public static string GetBundleVersion()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            return _iOSGetBundleVersion();
        } else {
            return "";
        }
    }

    [DllImport ("__Internal")]
    private static extern string _iOSGetBundleIdentifier();

    public static string GetBundleIdentifier()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            return _iOSGetBundleIdentifier();
        } else {
            return "";
        }
    }

    [DllImport ("__Internal")]
    private static extern bool _iOSCanOpenURL(string url);

    public static bool CanOpenURL(string url)
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            Debug.Log("CanOpenURL : "+_iOSCanOpenURL(url));
            return _iOSCanOpenURL(url);
        } else {
            return true;
        }
    }

	public static void OpenURL(string url)
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer) {
			Debug.Log("OpenURL : "+url);
			Application.OpenURL(url);
		}
	}

    public static bool IsCloudAvailable()
    {
        return iCloud.IsAvailable();
    }

    static bool saveUserDataToCloudInternal()
    {
        if(iCloud.IsAvailable()) {
//#if USE_UncleBill
//            Wallet.current.SaveToiCloud();
//#endif
            Type cpType = Type.GetType("ContextPool");
            if(cpType != null) {
                //FIXME. need to restore original ResourceFileSystem
                JsonFileSystem.SetFileSystem( new iCloudFileSystem() );
                MethodInfo method = cpType.GetMethod("SaveAllContext");
                method.Invoke(null, null);
                JsonFileSystem.SetFileSystem( new ResourceFileSystem() );
            }
            iCloud.SetString("lastSavedDevice", SystemInfo.deviceName);
            iCloud.SetString("lastSavedTime", DateTime.UtcNow.ToString());
            iCloud.Synchronize();
            return true;
        }
        else 
            return false;
    }

    public static bool SaveUserDataToCloud()
    {
        if(iCloud.IsAvailable()) {
            if(iCloud.HasKey("lastSavedDevice")) {
                string msg = string.Format(Locale.GetString("cloud_save_warn"), iCloud.StringForKey("lastSavedDevice"),
                                           iCloud.StringForKey("lastSavedTime"));
                Platform.SystemAlert("iCloud", msg, "ok", "cancel", (ok)=> {
                    if(ok) 
                        saveUserDataToCloudInternal();
                });
            } else {
                return saveUserDataToCloudInternal();
            }
        }
        return false;
    }

    public static bool LoadUserDataFromCloud()
    {
        if(iCloud.IsAvailable()) {
            if(iCloud.HasKey("lastSavedDevice")) {
                string msg = string.Format(Locale.GetString("cloud_load_warn"), iCloud.StringForKey("lastSavedDevice"),
                                           iCloud.StringForKey("lastSavedTime"));
                Platform.SystemAlert("iCloud", msg, "ok", "cancel", (ok)=> {
                    if(ok) {
//#if USE_UncleBill
//                        Wallet.current.LoadFromiCloud();
//#endif
                        Type cpType = Type.GetType("ContextPool");
                        if(cpType != null) {
                            //FIXME. need to restore original ResourceFileSystem
                            JsonFileSystem.SetFileSystem( new iCloudFileSystem() );
                            MethodInfo method = cpType.GetMethod("LoadAllContext");
                            method.Invoke(null, null);
                            JsonFileSystem.SetFileSystem( new ResourceFileSystem() );
                        }
                        JsonFileSystem.SetFileSystem( new ResourceFileSystem() );

                    }
                });
                return true;
            }
        }
        return false;
    }

    public static void SystemAlert(string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback)
    {
        if(negativeButton != null) {
            ShowAlert(Locale.GetString(title), Locale.GetString(message), Locale.GetString(positiveButton), Locale.GetString(negativeButton), callback);
        } else {
            ShowAlert(Locale.GetString(title), Locale.GetString(message), Locale.GetString(positiveButton), callback);
        }
    }

	public static void ShowWebPage(string url)
	{
//#if USE_Etcetera
//        EtceteraBinding.showWebPage( url, true );
//#else
        Debug.LogError("ShowWebPage need Etcetera Module Installed");
//#endif
	}

    public static void ChoosePhotoAlbum(System.Action<Texture2D> callback)
    {
//#if USE_Etcetera
//        Context.Etcetera.ChoosePhotoAlbum( callback );
//#else
        Debug.LogError("ChoosePhotoAlbum need Etcetera Module Installed");
//        Platform.SafeCallback(callback, null);
//#endif
    }

    public static bool HasCamera()
    {
        switch(UnityEngine.iOS.Device.generation) {
            case UnityEngine.iOS.DeviceGeneration.iPodTouch1Gen:
            case UnityEngine.iOS.DeviceGeneration.iPodTouch2Gen:
            case UnityEngine.iOS.DeviceGeneration.iPodTouch3Gen:
            case UnityEngine.iOS.DeviceGeneration.iPad1Gen:
                return false;

            case UnityEngine.iOS.DeviceGeneration.iPhone:
            case UnityEngine.iOS.DeviceGeneration.iPhone3G:
            case UnityEngine.iOS.DeviceGeneration.iPhone3GS:
            case UnityEngine.iOS.DeviceGeneration.iPhone4:
            case UnityEngine.iOS.DeviceGeneration.iPodTouch4Gen:
            case UnityEngine.iOS.DeviceGeneration.iPhone4S:
            case UnityEngine.iOS.DeviceGeneration.iPhone5:
            case UnityEngine.iOS.DeviceGeneration.iPodTouch5Gen:
            case UnityEngine.iOS.DeviceGeneration.iPhone5C:
            case UnityEngine.iOS.DeviceGeneration.iPhone5S:
            case UnityEngine.iOS.DeviceGeneration.iPhoneUnknown:
            case UnityEngine.iOS.DeviceGeneration.iPodTouchUnknown:
            case UnityEngine.iOS.DeviceGeneration.iPad2Gen:
            case UnityEngine.iOS.DeviceGeneration.iPad3Gen:
            case UnityEngine.iOS.DeviceGeneration.iPadMini1Gen:
            case UnityEngine.iOS.DeviceGeneration.iPad4Gen:
            case UnityEngine.iOS.DeviceGeneration.iPadUnknown:
                return true;
        }
        return false;
    }

	//showshare
	[DllImport("__Internal")]
	private static extern void _showShare( string title, string message, string imgPath, ShowShareClose callback);
	
	public static void ShowShare( string title, string message)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer ) {			
			_showShare( title, message, "", null);
		}
	}
	
	public static void ShowShare( string title, string message, System.Action<bool> callback)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer ) {
			_showShareCallback = callback;
			_showShare( title, message, "", ShowShareClosed);
		}
	}
	
	public static void ShowShare( string title, string message, Texture2D texture, System.Action<bool> callback)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer ) {
			string filename = "shot.png";
			byte []bytes = texture.EncodeToPNG();
			File.WriteAllBytes(Application.persistentDataPath + Path.DirectorySeparatorChar + filename,bytes); 
			_showShareCallback = callback;
			_showShare( title, message, filename, ShowShareClosed);
		}
	}

	public static void TakePhoto(int width, int height, string fileName, System.Action<string> callback) {
		return;
	}

	public static void SelectAlbum(int width, int height, string fileName, System.Action<string> callback) {
		return;
	}

    [DllImport("__Internal")]
	private static extern void _showAlert( string title, string message, string positive, string negative, IntPtr bufferHandle, AlertButtonClick callback);

    public static void ShowAlert( string title, string message, string positive, System.Action<bool> callback)
    {
		if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				byte[] _pixels;
				_pixels = new byte[100000];
				_handle = GCHandle.Alloc(_pixels);
            _alertCallback = callback;
            _positiveButton = positive;
			_showAlert( title, message, positive, string.Empty, (IntPtr)_handle, AlertButtonClicked);
        }
    }

	public static void ShowAlert( string title, string message, string positive, string negative, System.Action<bool> callback)
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer ) { 
				byte[] _pixels;
				_pixels = new byte[100000];
				_handle = GCHandle.Alloc(_pixels);
            _alertCallback = callback;
            _positiveButton = positive;
			_showAlert( title, message, positive, negative, (IntPtr)_handle, AlertButtonClicked);
        }   
    }

    [DllImport ("__Internal")]
    private static extern void _NSLog(string log);

	public static void NSLog( string log ) 
    {
        _NSLog(log);
    }
}

}

#endif

