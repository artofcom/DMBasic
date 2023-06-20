using UnityEngine;
using System;
#if UNITY_ANDROID

namespace NOVNINE.Native
{
	public static class AndroidExt
	{
		private static System.Action<bool> _alertCallback;
		private static System.Action<bool> _showShareCallback;
		private static System.Action<string> _takePhotoCallback;
		private static System.Action<string> _selectAlbumCallback;
		private static string _positiveButton;
		
		public static bool IsMusicPlaying()
		{
			return false;
		}
		
		public static string GetCurrentLocaleID()
		{
			if(Application.platform != RuntimePlatform.Android)
				return "KR";
			using (AndroidJavaClass cls = new AndroidJavaClass("java.util.Locale")) {
				using(AndroidJavaObject locale = cls.CallStatic<AndroidJavaObject>("getDefault")) {
					return locale.Call<string>("getCountry");
				}
			}
		}
	
        public static string AdvertiserIdentifier()
		{
			string advertisingID = "";
			
            try {
                AndroidJavaClass up = new AndroidJavaClass  ("com.unity3d.player.UnityPlayer");
                if(up == null) return advertisingID;
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
                if(currentActivity == null) return advertisingID;
                AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                if(client == null) return advertisingID;
                AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo",currentActivity);
                if(adInfo == null) return advertisingID;

                advertisingID = adInfo.Call<string> ("getId").ToString();  
                return advertisingID;
            }
            catch(Exception e) {
                Debug.LogWarning("AndroidExt.AdvertiserIdentifier fail : "+e);
                return "";
            }
		}

		public static void ApplicationQuit()
		{
            Platform.BroadcastMessage("OnApplicationQuit");
            PlayerPrefs.Save();
            TaskManager.DoSecondsAfter(()=>{
                System.Diagnostics.ProcessThreadCollection pt = System.Diagnostics.Process.GetCurrentProcess().Threads;
                foreach(System.Diagnostics.ProcessThread p in pt)  {   	
                    p.Dispose();    	
                }
                Debug.Log("Application ProcessKill");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }, 0.1f);
		}
		
		public static string GetBundleVersion()
		{
			if(Application.platform != RuntimePlatform.Android)
				return "1.0";
			
			//FIXME
			using(AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				using(AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
					using(AndroidJavaObject pm = jo.Call<AndroidJavaObject>("getPackageManager")) {
						string packageName = jo.Call<string>("getPackageName");
						using(AndroidJavaObject package = pm.Call<AndroidJavaObject>("getPackageInfo", packageName,0)) {
							return package.Get<string>("versionName");
						}
					}
				}
			}
		}
		
		public static string GetBundleIdentifier()
		{
			if(Application.platform != RuntimePlatform.Android)
				return "com.nov9.unknown";
			
			using(AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				using(AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
					return jo.Call<string>("getPackageName");
				}
			}
		}
		
		public static bool CanOpenURL(string url)
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject pm = jo.Call<AndroidJavaObject>("getPackageManager");
			AndroidJavaObject intent = null;
			try {
				intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", url);
			}catch(Exception ex) {
				Debug.Log("canOpenURL=  "+ex.Message);
			}
			if (intent == null) {
				return false;
			} else {
				return true;
			}
		}
		
		public static void OpenURL(string url)
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject pm = jo.Call<AndroidJavaObject>("getPackageManager");
			AndroidJavaObject intent = null;
			try {
				intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", url);
			}catch(Exception ex) {
				Debug.Log("OpenURL=  "+ex.Message);
			}
			if (intent != null) {
				jo.Call("startActivity", intent);
			}
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
#if USE_Etcetera
        EtceteraAndroid.showWebView(url);
#else
			Debug.LogError("ShowWebPage need Etcetera Module Installed");
#endif
		}
		
		public static void ChoosePhotoAlbum(System.Action<Texture2D> callback)
		{
#if USE_Etcetera
			Context.Etcetera.ChoosePhotoAlbum( callback );
#else
			Debug.LogError("ChoosePhotoAlbum need Etcetera Module Installed");
	        Platform.SafeCallback(callback, null);
#endif
		}
		
		
		//encapsules bool PackageManager.hasSystemFeature(string)
		//see (http://developer.android.com/reference/android/content/pm/PackageManager.html)
		public static bool HasSystemFeature(string feature) 
		{
			using(AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				using(AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
					using(AndroidJavaObject pm = jo.Call<AndroidJavaObject>("getPackageManager")) {
						bool avail = pm.Call<bool>("hasSystemFeature", new object[]{ feature } );
						Debug.Log("AndroidExt.hasSystemFeature for " +feature+" returned "+avail);
						return avail;
					}
				}
			}
		}
		
		public static bool HasCamera()
		{
			//return HasSystemFeature("android.hardware.camera") || HasSystemFeature("android.hardware.camera.front");
			using(AndroidJavaClass jc = new AndroidJavaClass("android.hardware.Camera")) {
				int numCam = jc.CallStatic<int>("getNumberOfCameras");
				Debug.Log("AndroidExt.Camera.getNumberOfCameras : " +numCam);
				return numCam > 0;
			}
		}
		
		public static void ShowAlert( string title, string message, string positiveButton, System.Action<bool> callback)
		{
			ShowAlert( title, message, positiveButton, string.Empty , callback);
		}
		
		// Shows a native alert with two buttons
		public static void ShowAlert( string title, string message, string positiveButton, string negativeButton, System.Action<bool> callback)
		{
			if (_alertCallback != null) {
				Debug.Log ("alertCallBack is not null!! 2nd call is ignored..");
				return;
			}
			
			if( Application.platform != RuntimePlatform.Android )
				return;
			
			_alertCallback = callback;
			_positiveButton = positiveButton;
			
			using (AndroidJavaObject pluginClass = new AndroidJavaObject( "com.bitmango.bitmangoext.bitmangoext")) {
				NativeInterfaceManager niManager = new NativeInterfaceManager();
				niManager.onAlertButtonClicked += alertButtonClickedEvent;
				niManager.onAlertButtonCanceled += alertButtonCanceledEvent;
				pluginClass.CallStatic ("setListener", niManager);
				pluginClass.CallStatic ("showAlert", title, message, positiveButton, negativeButton);
			}
			
		}
		
		public static void ShowShare(string title, string message, Texture2D texture, System.Action<bool> callback)
		{
			ShowShare (title, message, callback);
		}
		
		public static void ShowShare(string title, string message)
		{
			if( Application.platform != RuntimePlatform.Android )
				return;

			using( var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext"))
				pluginClass.CallStatic( "showShare", title, message);			
		}
		
		public static void ShowShare(string title, string message, System.Action<bool> callback)
		{
			if( Application.platform != RuntimePlatform.Android )
				return;		

			_showShareCallback = callback;
			using(var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext")) {
				NativeInterfaceManager niManager = new NativeInterfaceManager();
				niManager.onShareClosed += shareClosedEvent;
				pluginClass.CallStatic("setListener", niManager);
				pluginClass.CallStatic("showShare", title, message);		
			}
		}
		
		public static void TakePhoto(int width, int height, string fileName, System.Action<string> callback)
		{
			if( Application.platform != RuntimePlatform.Android )
				return;
			_takePhotoCallback = callback;
			using( var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext")) {
				NativeInterfaceManager niManager = new NativeInterfaceManager();
				niManager.onTakePhotoSucceeded += takePhotoSucceededEvent;
				niManager.onTakePhotoCanceled += takePhotoCanceledEvent;
				pluginClass.CallStatic ("setListener", niManager);
				pluginClass.CallStatic( "takePhoto", width, height, fileName);
			}
		}

        /*
        SIZE_SMALL = 0;    SIZE_MEDIUM = 1;       SIZE_TALL = 2;      SIZE_STANDARD = 3;
        ANNOTATION_NONE = 0;      ANNOTATION_BUBBLE = 1;      ANNOTATION_INLINE = 2;

        Documents : https://developers.google.com/+/web/+1button/
        */
        public static void MakePlusOneButton(string url, int annotation, int size, int x, int y)
        {
            if (Application.platform != RuntimePlatform.Android )
                return;

            using(var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext")) {
                pluginClass.CallStatic("makePlusOneButton", url, annotation, size, x, y);
            }
        }

        public static void RemovePlusOneButton()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            using ( var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext")) {
                pluginClass.CallStatic("removePlusOneButton");
            }
        }
		
		public static void SelectAlbum(int width, int height, string fileName, System.Action<string> callback)
		{
			if( Application.platform != RuntimePlatform.Android )
				return;
			_selectAlbumCallback = callback;
			using( var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext")) {
				NativeInterfaceManager niManager = new NativeInterfaceManager();
				niManager.onSelectAlbumSucceeded += selectAlbumSucceededEvent;
				niManager.onSelectAlbumCanceled += selectAlbumCanceledEvent;
				pluginClass.CallStatic ("setListener", niManager);
				pluginClass.CallStatic( "selectAlbum", width, height, fileName);
			}
		}
		
		public static void selectAlbumSucceededEvent(string filePath)
		{   
			Debug.Log( "selectAlbumSucceededEvent: " + filePath);
			
			if(null != _selectAlbumCallback) {
				_selectAlbumCallback(filePath);
				_selectAlbumCallback = null;
			}   
		}   
		
		public static void selectAlbumCanceledEvent()
		{   
			Debug.Log( "selectAlbumCanceledEvent: ");
		}   
		
		public static void takePhotoSucceededEvent(string filePath)
		{   
			Debug.Log( "takePhotoSucceededEvent: " + filePath);
			
			if(null != _takePhotoCallback) {
				_takePhotoCallback(filePath);
				_takePhotoCallback = null;
			}   
		}   
		
		public static void takePhotoCanceledEvent()
		{   
			Debug.Log( "takePhotoCanceledEvent: ");
		}   
		
		public static void alertButtonClickedEvent( string positiveButton )
		{   
			Debug.Log( "alertButtonClickedEvent: " + positiveButton );
			
			if(null != _alertCallback) {
				_alertCallback(_positiveButton == positiveButton);
				_alertCallback = null;
			}   
		}   
		
		public static void alertButtonCanceledEvent()
		{   
			Debug.Log( "alertCanceledEvent" );
			
			if(null != _alertCallback) {
				_alertCallback(false);
				_alertCallback = null;
			}   
		}  
		
		public static void shareClosedEvent(bool isCanceled)
		{
			Debug.Log("shareClosedEvent");
			
			if(null != _showShareCallback) {
				_showShareCallback(isCanceled);
				_showShareCallback = null;
			}
		}
        
        public static int GetMediaVolume()
        {   
            using ( var pluginClass = new AndroidJavaClass("com.bitmango.bitmangoext.bitmangoext"))
                return pluginClass.CallStatic<int>("GetMediaVolume");
        }   

        public static void SetMediaVolume(int volume)
        {
            using(var pluginClass = new AndroidJavaClass( "com.bitmango.bitmangoext.bitmangoext"))
                pluginClass.CallStatic("SetMediaVolume", volume);
        }
	}
	
}
#endif
