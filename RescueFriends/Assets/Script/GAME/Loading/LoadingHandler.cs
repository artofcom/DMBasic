using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml;
using System;
//using Facebook.Unity;

public class LoadingHandler : MonoBehaviour
{	

    public enum E_LOADING_STEP
    {
        E_LOADING_STEP_NONE,
        E_LOADING_STEP_RES_VERSIONCHECK,
        E_LOADING_STEP_PATCH_END,
        E_LOADING_STEP_RES_AUTH_ACCOUNT_LOGIN,
        E_LOADING_STEP_RES_GAMESERVER_LIST,

    };

	public tk2dTextMesh         _progressText;
    public tk2dUIProgressBar    _ProgressBar;

	//LManager _manager;
    float _progress             = 0.0f;
    AsyncOperation _ao          = null;
    private List<System.Action> sQueue = new List<Action>();
    List<System.Action>         localQueue = new List<System.Action>();
    private List<System.Action> lateQueue = new List<Action>();
   
    E_LOADING_STEP eLoadingHandler = E_LOADING_STEP.E_LOADING_STEP_NONE;
    public E_LOADING_STEP LoadingStep
    {
        get{ return eLoadingHandler;}
        set{ eLoadingHandler = value;}
    }

    static bool sQuitApplication= false;
    public static void quitApplication()
    {
        sQuitApplication        = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Awake()
    {
        tk2dCamera cam          = Camera.main.GetComponent<tk2dCamera>();
        float aspectFitWidth    = cam.ScreenExtents.width / 11.25f;
        if(aspectFitWidth < 1)
            cam.CameraSettings.orthographicSize += cam.CameraSettings.orthographicSize - (cam.CameraSettings.orthographicSize * aspectFitWidth);
    }

	void Start()
    {
        bool Next               = true;
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

       /* -W AndroidJavaClass cls = new AndroidJavaClass("nov9.unitynotification.UnityNotificationManager");

        if (cls != null)
        {
            LCommonDefine.CURRENT_DEVICE_COUNTRY = cls.CallStatic<string>("GetISO3CountryCode");
        }

        if (PlayerPrefs.GetString("strGoogleLogin", "") != "cancel")
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
        #endif

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
            NextScene();
        }
    }

    void NextScene()
    {
        //LGameData _gameData = LGameData.GetInstance();
        //_manager = LManager.GetInstance();
        //_manager.LoadingScene = this;
        //LLoginLogic.GetInstance().StartLogin();
        StartCoroutine("Load");
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                {
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_ANDROID;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_GOOGLE;
                }
                break;

            case RuntimePlatform.IPhonePlayer:
                {
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_IOS;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_APPLE;
                }
                break;

            case RuntimePlatform.SamsungTVPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                {
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_IOS;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_APPLE;

                    #if UNITY_ANDROID
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_ANDROID;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_GOOGLE;
                    #endif

                    #if UNITY_IOS
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_IOS;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_APPLE;
                    #endif

                }
                break;
            case RuntimePlatform.WebGLPlayer:
                {
                    LCommonDefine.CURRENT_DEVICE_TYPE = (System.Byte)E_DEVICETYPE.E_DEVICETYPE_WINDOWS;
                    LCommonDefine.CURRENT_APP_MARKET = (System.Byte)E_MARKET.E_MARKET_FACEBOOK;
                }
                break;
            default:
                {
                    Debug.Log(" Application.platform = " + Application.platform);
                }
                break;
        }
        //_manager.TrackingStep = LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_START;
        TaskManager.StartCoroutine( NOVNINE.Profile.CoSendTrackingLog((int)LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_START));

        Director.SoundInit();
        Director.StartLoadingHandler = true;
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
                NextScene();
                PlayerPrefs.SetString("strGoogleLogin", "cancel");
                PlayerPrefs.Save();
            }
            break;
        }
        AndroidDialog.onDialogPopupComplete -= OnDialogPopupComplete;
    }

    public void OnCompleted(string _value)
    {
        if (_value == "TRUE")
        {
            StartCoroutine(LoadLevel());
        }
        else
        {

        }
    }

    IEnumerator LoadLevel()
    {
    //        string mainPath = OBBDownloader.GetMainOBBPath(OBBDownloader.GetExpansionFilePath());
    //
    //        while (string.IsNullOrEmpty( mainPath))
    //        {
    //            yield return new WaitForSeconds(0.5f);
    //            mainPath = OBBDownloader.GetMainOBBPath(OBBDownloader.GetExpansionFilePath());
    //        }

        string uri = "file://" + OBBDownloader.GetMainOBBPath(OBBDownloader.GetExpansionFilePath());
        WWW www = WWW.LoadFromCacheOrDownload(uri , 0);

        // Wait for download to complete
        yield return www;
        if (www.error != null)
        {
            Debug.Log("wwww error " + www.error);
        }
        else
        {
            SceneManager.LoadScene("Loading");
        }
    }

    #endif

    public void ReStart()
    {
    }
        
    IEnumerator coReStartShowNetworkPopup()
    {
        yield break;
    }

    IEnumerator coShowNetworkPopup()
    {
        yield break;
    }
    
	void OnDestroy()
	{
		Debug.Log("LoadingHandler::OnDestroy");
        //Debug.Log("LoadingHandler TIME : " + (Time.realtimeSinceStartup - LManager.TIME));
        // prevent obnormal termination.
        if (false == sQuitApplication)
        {
            Scene.ClearAll();
        //    _manager = null;
		    System.GC.Collect();
        }
	}
	
	IEnumerator Load()
	{
        // while (_manager.PatchComplete == false)
        {
        //    yield return null;
        }

        //_manager.TrackingStep = LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_SCENELOAD_START;
        TaskManager.StartCoroutine( NOVNINE.Profile.CoSendTrackingLog((int)LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_SCENELOAD_START));

        //_ao                     = SceneManager.LoadSceneAsync("Play", LoadSceneMode.Single);//.Additive);
        _ao                     = SceneManager.LoadSceneAsync("Title", LoadSceneMode.Single);//.Additive);
		_ao.allowSceneActivation= false;
        //Debug.Log("allowSceneActivation start TIME : " + (Time.realtimeSinceStartup - LManager.TIME));
        //LManager.TIME = Time.realtimeSinceStartup;
        //float _time = LManager.TIME;
		while(false==_ao.isDone)
		{
			// [0, 0.9] > [0, 1]
			_progress           = Mathf.Clamp01(_ao.progress/0.9f);
            Debug.Log(_ao.progress);
			// Loading completed

			//if(_ao.isDone)
            if(_progress >= 0.9f)// && LManager.AllowSceneActivation)
			{
                
             //   Debug.Log("allowSceneActivation TIME : " + (Time.realtimeSinceStartup - _time));
                //LManager.TIME = Time.realtimeSinceStartup;
                //_time = Time.realtimeSinceStartup;

               // while (_manager.IsValidConnection() == false)
                //{
                //    Debug.Log("_manager.IsValidConnection() : " + (Time.realtimeSinceStartup - _time));
                //    yield return null;
                //}

             //   Debug.Log("allowSceneActivation TIME : " + (Time.realtimeSinceStartup - _time));
                //LManager.TIME = Time.realtimeSinceStartup;
              //  _time = Time.realtimeSinceStartup;
                _ao.allowSceneActivation = true;

              //  _manager.TrackingStep = LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_SCENELOAD_END;
                TaskManager.StartCoroutine( NOVNINE.Profile.CoSendTrackingLog((int)LCommonDefine.TRACKING_STEP.TRACKING_LOADING_SCENE_SCENELOAD_END));
				//System.GC.Collect();
				
                Debug.Log("Loading done.");
                //SceneManager.UnloadScene("Loading");
				yield break;
			}

            Debug.Log("Loading progress: " + (_progress * 100) + "%");

           // Debug.Log("ao.isDone TIME : " + (Time.realtimeSinceStartup - _time) +":" + ao.progress + ":" + LManager.AllowSceneActivation);
           // _time = Time.realtimeSinceStartup;
			yield return null;
		}
	}


	void Update()
	{
        if(null!=_ao && null!=_ProgressBar)
        {
    		if(_progress>0)     _ProgressBar.Value = _progress;
        }
	}



    void RunOnGameLateUpdate(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (lateQueue)
        {
            lateQueue.Add(action);
        }
    }

    public void RunOnGameThread(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (sQueue)
        {
            sQueue.Add(action);
        }
    }

    void LateUpdate()
    {
        localQueue.Clear();
        lock (lateQueue)
        {
            localQueue.AddRange(lateQueue);
            lateQueue.Clear();
        }

        for (int i = 0; i < localQueue.Count; i++)
        {
            localQueue[i].Invoke();
        }       
    }
}



//
//{
//    \"msg_condition\": \"NullReferenceException: A null value was found where an object instance was required.\",
//    \"msg_stacktrace\": \"LManager+\\u003C_coNotifyDailyBonus>c__Iterator69.MoveNext ()\\nUnityEngine.SetupCoroutine.InvokeMoveNext (IEnumerator enumerator, IntPtr returnValueAddress)\\nUnityEngine.MonoBehaviour.StartCoroutine_Auto (IEnumerator routine)\\nUnityEngine.MonoBehaviour.StartCoroutine (IEnumerator routine)\\nLManager.LPuzzlePlayer_PACKET_NOTIFY_DAILYBONUS (ST_Packet& packet)\\nLManager.Network_ProcessPacket (ST_Packet& packet)\\nLManager.Update ()\\nUnityEngine.MonoBehaviour:StartCoroutine(IEnumerator)\\nLManager:LPuzzlePlayer_PACKET_NOTIFY_DAILYBONUS(ST_Packet&)\\nLManager:Network_ProcessPacket(ST_Packet&)\\nLManager:Update()\\n\",
//}