/**
* @file PlatformContext.cs
* @brief
* @author Choi YongWu(amugana@bitmango.com)
* @version 1.0
* @date 2013-09-24
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
//using Facebook;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PlatformContext : MonoBehaviour
{
    //Common
    public string appID;
    public LocaleString appName;
    public string appDisplayName;
	Vector3 prePosition;
    
    //bundle settings for webplayer(facebook)
    [HideInInspector] public string bundleVersion;
    [HideInInspector] public string bundleIdentifier;

    [HideInInspector] public int revision;
    [HideInInspector] public string sharedRevision;

    public Texture2D appIcon;

    public string facebookAppID
    {
        get { return "0"; }// Facebook.Unity.Settings.FacebookSettings.AppId;}
    }

    public enum StoreType 
	{
        AppleStore, //iPhone
        GoogleStore, //Android
        OneStore, //Android Korea
        AmazonStore, //Android
        BlackBerryStore, //BB10
        MacAppStore, //StandAlone
        WP8Store, //WP8
        FacebookStore //Web
    }
    
	public StoreType storeType;
    public string storeKey;

#if UNITY_EDITOR
    public string GoogleDriveKey;
#endif

    [System.NonSerializedAttribute]
    public bool isFirstRun;

//    [System.NonSerializedAttribute]
//    public string recoverPoint;

    public System.DateTime lastPlayed;
    public System.DateTime installed;

    void Awake()
    {
		Context.NNPlatform = this;
		
        #if UNITY_ANDROID
        storeType = StoreType.GoogleStore;
        storeKey = LCommonDefine.GOOGLESTORKEY;
        #elif UNITY_IPHONE
        storeType = StoreType.AppleStore;
        storeKey = LCommonDefine.APPLESTORKEY;
        #endif

		if(!PlayerPrefs.HasKey("installed")) 
		{
			installed = System.DateTime.UtcNow;
			PlayerPrefs.SetString("installed", installed.ToString());
		}
		else 
			installed = System.DateTime.Parse(PlayerPrefs.GetString("installed"));
		
        isFirstRun=!PlayerPrefs.HasKey("lastPlayed");
        if(PlayerPrefs.HasKey("lastPlayed"))
            lastPlayed = System.DateTime.Parse(PlayerPrefs.GetString("lastPlayed"));
        else
            lastPlayed = System.DateTime.UtcNow;

        Debugger.Launch(GetDebugInfoTable());

        string adidString = "";
        string screenString = "";
        Debug.Log("Debugger.EnableLocalLog:"+Debugger.EnableLocalLog);
        if(Debugger.EnableLocalLog)
            adidString =  "\n\tADID: " + Profile.ADID;
        screenString = "\n\tScreen - Width: " + UnityEngine.Screen.width + ", Height: " + UnityEngine.Screen.height;

		Debug.Log("============================================================\n\t"+appID+" ("+NativeInterface.GetBundleIdentifier()+") Version: "+NativeInterface.GetBundleVersion()+ "[r"+revision+"/"+sharedRevision+"]"+ Profile.ADID + screenString + "\n============================================================");

		if(NOVNINE.Profile.IsEchoInput)
			StartCoroutine(coEchoInput());
    }

    private static Hashtable GetDebugInfoTable()
    {
        Hashtable debugInfo = new Hashtable() {
            #region Table Value..
            {"app_id", NativeInterface.GetBundleIdentifier()},
            {"app_ver", NativeInterface.GetBundleVersion()},

            // 현재 Platfom Info의 접근시, SocialPlatform.Init 이후로 정렬되어야 함. (after Director.Awake-> SocialPlatform.Init)
            {"app_name", Platform.Info.appName.ToString()},
            {"store_type", Platform.Info.storeType.ToString()},

            //{"device_id", NativeInterface.DeviceUniqueIdentifier()},
            {"device_model", SystemInfo.deviceModel},
            {"uuid", Profile.UUID},
            #endregion
        };
        return debugInfo;
    }

//    void OnApplicationPause(bool pause)
//    {
//        //Debugger.Log("OnApplicationPause: "+pause+" recoverPoint: "+recoverPoint);
//        if(pause) 
//		{//exit
//            PlayerPrefs.SetString("lastPlayed", System.DateTime.UtcNow.ToString());
////            if(recoverPoint == null)
////                BroadcastMessage("OnGamePause", pause, SendMessageOptions.DontRequireReceiver);
//        }
//		else
//		{//enter
////            if(recoverPoint == null)
////                BroadcastMessage("OnGamePause", pause, SendMessageOptions.DontRequireReceiver);
//            
////            recoverPoint = null;
//        }
//    }

//    void OnApplicationQuit()
//    {
//        //Debugger.Log("OnApplicationQuit : recoverPoint:"+recoverPoint);
//        PlayerPrefs.SetString("lastPlayed", System.DateTime.UtcNow.ToString());
//    }

#if UNITY_EDITOR
    private static int screenshot = 0;
    public static bool showGUI = false;
    private string MagicToken = "";
    
    void Update ()
    {

        if (Input.GetKeyUp(KeyCode.Print) || Input.GetKeyUp(KeyCode.P)) {
            if(!System.IO.Directory.Exists(Application.dataPath+"/../ScreenShot")) {
                System.IO.Directory.CreateDirectory(Application.dataPath+"/../ScreenShot");
            }

            string fileName = Application.dataPath + "/../ScreenShot/" + "screenshot_" + screenshot.ToString()+".png";
            while (System.IO.File.Exists(fileName)) {
                screenshot += 1;
                fileName = Application.dataPath + "/../ScreenShot/" + "screenshot_" + screenshot.ToString()+".png";
            }

            ScreenCapture.CaptureScreenshot(fileName);
            screenshot += 1;
            Debug.Log("ScreenShot Success!! " + fileName);
        }

		if (Input.GetKeyUp(KeyCode.F1)){  PlayerPrefs.SetInt("locale", (int)LocaleType.EN); Locale.ReloadLocale();}
		else if (Input.GetKeyUp(KeyCode.F2)){  PlayerPrefs.SetInt("locale", (int)LocaleType.SE); Locale.ReloadLocale();}
		else if (Input.GetKeyUp(KeyCode.F3)){  PlayerPrefs.SetInt("locale", (int)LocaleType.DK); Locale.ReloadLocale();}
		else if (Input.GetKeyUp(KeyCode.F4)){  PlayerPrefs.SetInt("locale", (int)LocaleType.NO); Locale.ReloadLocale();}
		else if (Input.GetKeyUp(KeyCode.F5)){  PlayerPrefs.SetInt("locale", (int)LocaleType.DE); Locale.ReloadLocale();}

        if (Input.GetKeyDown(KeyCode.Equals)) {
            if (Time.timeScale < 1f) Time.timeScale += 0.1f;
        }
        
        if (Input.GetKeyDown(KeyCode.Minus)) {
            if (Time.timeScale > 0.1f) Time.timeScale -= 0.1f;
        }
    }

    void DoFacebookConnect(int windowId)
    {
        GUI.Label (new Rect (10, 20, (Screen.width - 70), 80), "Your browser window should popup, simply log in to Facebook and copy your magic token in the text box below.");
        MagicToken = GUI.TextField (new Rect (10, 50, (Screen.width - 70), 20),  MagicToken, 1024);

        if(GUI.Button(new Rect( 10, 80, 80, 20), "Connect!")) {
            //FacebookAPI.instance.accessToken = MagicToken;
            GameObject.Find("FacebookManager").SendMessage("sessionOpened", MagicToken);
            showGUI = false;
        }

    }
#endif
	
    IEnumerator coEchoInput() {
        while(true) {
            if(Input.touchCount > 0) {
                var t = Input.touches[0];
                float px = t.position.x;
                float py = t.position.y;
                Debugger.Log("EI] "+t.phase+" ("+px+","+py+")");
            }
            yield return null;
        }
    }
}


