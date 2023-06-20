using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class Splash : MonoBehaviour 
{
    bool Next = true;

    public SpriteRenderer _sprFader = null;

    void Start ()
    {        
//        string ss = null;
//        try
//        {
//            int ww = ss.Length;
//        }
//        catch (System.Exception e)
//        {
//            Debug.Log("Exception in OnApplicationFocus:" +
//                e.Message + "\n" + e.StackTrace +"  -- : ////");
//
//            var st = new System.Diagnostics.StackTrace(e, true);
//            // Get the top stack frame
//            var frame = st.GetFrame(0);
//            // Get the line number from the stack frame
//            var line = frame.GetFileLineNumber();
//            Debug.Log(line);
//
//            string stack = UnityEngine.StackTraceUtility.ExtractStringFromException(e);
//            string stack1 = UnityEngine.StackTraceUtility.ExtractStackTrace();
//            Debug.Log(stack + " ::---");
//            Debug.Log(stack1);
//        }


        #if USE_RemoteNotification && !UNITY_EDITOR
        if(NOVNINE.Context.RemoteNotification == null)
        {
            GameObject prefab = Resources.Load("RemoteNotification") as GameObject;
            if(prefab != null) 
            {
                GameObject _holder = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                _holder.name = "__RemoteNotification__";
                UnityEngine.Object.DontDestroyOnLoad(_holder);   
            }
        }
        #endif

        NOVNINE.Alarm.AllUnRegister();

        #if UNITY_EDITOR
        AssetBundles.AssetBundleManager.SimulateAssetBundleInEditor = false;
        #endif

        #if UNITY_ANDROID && !UNITY_EDITOR

        AndroidJavaClass cls = new AndroidJavaClass("nov9.unitynotification.UnityNotificationManager");

        if (cls != null)
        {
            LCommonDefine.CURRENT_DEVICE_COUNTRY = cls.CallStatic<string>("GetISO3CountryCode");
        }

        /*if (PlayerPrefs.GetString("strGoogleLogin", "") != "cancel")
        {
            Next = GooglePlayGames.OurUtils.PlatformUtils.Supported;
            if (Next == false)
            {
	            AndroidDialog.onDialogPopupComplete += OnDialogPopupComplete;
	            NativeDialog dialog = new NativeDialog("Play Game App Install?", "Please App Install!", "YES","NO");
	            dialog.SetUrlString("market://details?id=com.google.android.play.games");
	            dialog.init();
			}
        }*/
        #endif

        #if UNITY_IOS && !UNITY_EDITOR
        LCommonDefine.CURRENT_DEVICE_COUNTRY = NOVNINE.Native.iOSExt.GetISO3CountryCode();
        Debug.Log("----------------->" + LCommonDefine.CURRENT_DEVICE_COUNTRY);
        #endif

        TaskManager.StartCoroutine( NOVNINE.Profile.CoSendTrackingLog((int)LCommonDefine.TRACKING_STEP.TRACKING_SPLASH_SCENE));

        if (Next)
        {
            //            #if UNITY_ANDROID && !UNITY_EDITOR && LIVE_MODE
            //
            //            if (!OBBDownloader.RunningOnAndroid())
            //                return;
            //
            //            OBBDownloader.SetOBBMessageGameObjectName(gameObject.name);
            //
            //            string expPath = OBBDownloader.GetExpansionFilePath();
            //            if (expPath != null)
            //            {
            //                string mainPath = OBBDownloader.GetMainOBBPath(expPath);
            //                
            //                if (mainPath == null)
            //                    OBBDownloader.FetchOBB();
            //            }
            //            return;
            //
            //            #endif
            //            Debug.Log("------->");

            StartCoroutine(NextScene());
        }
	}
    
    IEnumerator NextScene()
    {
        //yield return SceneManager.LoadSceneAsync("Loading");

        
        yield return new WaitForSeconds(2.0f);

        _sprFader.DOFade(1.0f, 1.0f);
        yield return new WaitForSeconds(1.0f);

        yield return SceneManager.LoadSceneAsync("Title");
    }

    #if UNITY_ANDROID && !UNITY_EDITOR
    // Raise when click on any button of Dialog popup
    void OnDialogPopupComplete(MessageState state)
    {
        switch (state)
        {
            case MessageState.YES:
                {
                    PlayerPrefs.SetString("strGoogleLogin", "");
                    PlayerPrefs.Save();
                    Application.Quit();
                }
                break;
            case MessageState.NO:
                {
                    StartCoroutine(NextScene());
                    PlayerPrefs.SetString("strGoogleLogin", "cancel");
                    PlayerPrefs.Save();
                }
                break;
        }
        AndroidDialog.onDialogPopupComplete -= OnDialogPopupComplete;
    }
    #endif

}
