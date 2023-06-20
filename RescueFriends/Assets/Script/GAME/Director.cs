using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using AssetBundles;


////Android  USE_UncleBill;USE_UncleBill_GooglePlayStore;JMFP;SPINE_TK2D;USE_RemoteNotification;USE_LocalNotification;USE_DLLDATACLASS
/// iOS      JMFP;USE_UncleBill;USE_UncleBill_AppleStore;NO_GPGS;SPINE_TK2D;USE_DLLDATACLASS;USE_RemoteNotification;USE_LocalNotification;
public sealed class Director : MonoBehaviour
{
    //public GoogleAnalyticsV4 googleAnalytics;
    //public static bool IsADFreePopupChance { get; private set; }

	public static Director Instance;
	public static bool StartLoadingHandler = false;
	public static bool IsTimeUpdate = false;
    public static bool sNeedReload  = false;

#if UNITY_EDITOR
	public static Data.Level CurrentSceneLevelData = null;
#endif
	
    public static bool UseForceSeed = false;
    public static int TotalAdRewardCount = 3;
	public static bool PatchComplete = false;
	//public static bool IsUpdateStamina = false;

	// [STAMINA]
    //public const long           INTERVAL_TIME     = 12 * 60  * 60 * Data.GameData.TICK_DETAIL;
	//int _curStamina             = FULL_STAMINA;
	//long _timeNeedStamina       = 0;

    //public System.Byte DataSetType = 0;

    WaitForSeconds waitForSeconds = new WaitForSeconds(1.0f);
	
	/// static!
	new public static Coroutine StartCoroutine(IEnumerator routine)
	{
		var dispatcher = Instance;
		if (dispatcher != null)
			return (dispatcher as MonoBehaviour).StartCoroutine(routine);
		else
			return null;
	}

	public static void SoundInit()
	{
		if(NNSound.soundParent!=null || true==NNSound.Instance.IsPlayingBGM())
			return;
        
		NNSound.Instance.Reset();

        NNSoundHelper.VolumeMaster  = 0.8f;

        NNSoundHelper.SetMaxChannelForClip("IFX_block_drop", 1);//GameManager.WIDTH);
        NNSoundHelper.SetMaxChannelForClip("IFX_bonus_time_bg", 1);
        NNSoundHelper.SetMaxChannelForClip("IFX_hard_bust", 3);
        NNSoundHelper.SetMaxChannelForClip("IFX_block_crush", 3);
        NNSoundHelper.SetMaxChannelForClip("IFX_fire_and_fly", 5);
        //NNSoundHelper.Play("IFX_fire_and_fly");
	}
	
    void Awake() 
	{ 
        tk2dCamera cam = Camera.main.GetComponent<tk2dCamera>();
        float aspectFitWidth = cam.ScreenExtents.width / 11.25f;
        if(aspectFitWidth < 1)
            cam.CameraSettings.orthographicSize += cam.CameraSettings.orthographicSize - (cam.CameraSettings.orthographicSize * aspectFitWidth);
        
        DOTween.SetTweensCapacity(3000, 1000);
		Instance = this;
		
        _initQualityLevel();

		Director.SoundInit();
		
        Scene.EnableAndroidEscapeEvent = true;
        //Scene.SceneChangeTime = 0.7F;

//        DataMigration();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DontDestroyOnLoad(gameObject);
    }	    

	IEnumerator Start () 
	{
        //JMFRelay.ON_PACKET_WAIT_TIMEOUT += OnPacketWaitTimeOut;        
        //JMFRelay.ON_PACKET_STOP_RETRY += OnPacketStopRetry;

		if(Director.StartLoadingHandler == false)
			yield return null;

#if UNITY_EDITOR

        // Normally we've done these all titleOverlay.cs.....
        //
        yield return StartCoroutine(InfoLoader.init());

		if(Root.Data == null)
		{	
			Root.Data           = Root.Load();
            Debug.Assert(Root.Data!=null, "Root Data Load Failed !!!");
            //Root.Data.InitializeRoot();			
            Root.Data.initCurrentLevel();
		}
        
#endif
        Platform.Init();
		NNPool.Instance.Reset();

        Director.PatchComplete  = true;

#if UNITY_EDITOR
        if(LevelEditorSceneHandler.EditorMode && null!=Director.CurrentSceneLevelData)
        {
            Scene.ChangeTo("PlayScene", Director.CurrentSceneLevelData);
            yield break;
        }
#endif
       
        WorldSceneHandler._paramInfo info = new WorldSceneHandler._paramInfo();
        int idxNext             = Root.Data.idxMaxClearedLevel + 1;
        info.idxCurrent         = (int)BASIC_INFOS.MAX_LEVEL_ID<=idxNext ? (int)BASIC_INFOS.MAX_LEVEL_ID-1 : idxNext;
        info.isFromInGame       = false;
        Scene.ChangeTo("WorldScene", (object)info);// Root.Data.currentLevel);
	}

    public static string GetTextPath () 
	{
		//return "Assets/Data/text/"+Root.GetPostfix()+"/split";
        return "data/"+Root.GetPostfix()+"/split";
    }
	
	Vector2 GetScreenPixelDimensions()
	{
		Vector2 dimensions = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
		//Camera.main.GetComponent<tk2dCamera>().TargetResolution
		return dimensions;
	}

    public bool IsFullChargedStamina()
	{
		return NOVNINE.Wallet.GetItemCount("life") >= Data.GameData.CHARGEABLE_MAX_STAMINA;
	}

#if UNITY_EDITOR
    void Update () 
	{
        if (Input.GetKeyDown(KeyCode.Alpha7)) 
		{
			/*for (int i = 0; i < Root.Data.levels.Length; i++) 
			{
				Data.LevelResultData data = Root.Data.gameData.GetLevelResultDataByIndex(i);
				bool bAdd = (data == null);
				if(data == null)
					data = new Data.LevelResultData();

				data.bCleared = Root.Data.gameData.GetClearLevelByIndex(i);
				
				if (data.bCleared) continue;

				data.ucGrade = 3;
				data.bCleared = true;
				
				if(bAdd)
					Root.Data.gameData.AddLevelResultData(data,false);
				else
					Root.Data.gameData.SetLevelResultDataByIndex(i,data,false);
            }*/
        }
    }
#endif

    #region >> SNS & ETC

    public static void Share (string msg, RenderTextureCamera cam, System.Action<bool> callback = null)
	{
        Texture2D texture = null;

        if (cam != null)
		{
            cam.gameObject.SetActive(true);
            texture = cam.BuildTexture2D();
            cam.gameObject.SetActive(false);
        }

		string shareTitle = "Rescue Friends";

#if UNITY_IPHONE
        string url = "https://itunes.apple.com/app/id" + NOVNINE.Context.NNPlatform.storeKey;
#elif UNITY_ANDROID
        string url = "https://play.google.com/store/apps/details?id=" + Application.identifier;
#else
        string url = "";
#endif

        string message = string.Format("{0} {1}", msg, url);

        if (texture == null)
            NativeInterface.ShowShare(shareTitle, message, callback);
        else 
		{
            texture.ConvertToNonTransparent();
            NativeInterface.ShowShare(shareTitle, message, texture, callback);
        }
	}

	public static bool EnableRewardOfShare () {
		if (PlayerPrefs.HasKey("lastShare") == false) return true;

		System.DateTime rewardTime = new System.DateTime();

		if (System.DateTime.TryParse(PlayerPrefs.GetString("lastShare"), out rewardTime)) {
            return rewardTime.AddDays(1) <= System.DateTime.UtcNow;
		} else {
			return true;
		}
	}

    public static void InspectPublicField (object o) {
#if !NN_DEPLOY
        Type type = o.GetType();
        FieldInfo[] fields = o.GetType().GetFields();

        foreach (FieldInfo f in fields) 
		{
            if (f.IsStatic || f.IsPrivate) continue;
            if (f.FieldType.IsClass || f.FieldType.IsArray)
                Debugger.Assert(object.Equals(f.GetValue(o), "null") == false, type.Name + " : " + f.Name + " is null.");
        }
#endif
    }

    // Move to title scene & need re-start.
    //public static void GoTitle(string strParam)
    //{
    //    Scene.ChangeTo("MainScene", strParam);
    //}

    #endregion

    #region // to prevent Spine view stearing.
    public void showMeshNextFrame(MeshRenderer meshRdr)
    {
        if(null == meshRdr)     return;
        StartCoroutine( _coShowNextFrame(meshRdr) );
    }

    IEnumerator _coShowNextFrame(MeshRenderer skTarget)
    {
        skTarget.enabled        = false;
        yield return null;
        skTarget.enabled        = true;
    }
#endregion
    
    
    void _initQualityLevel()
    {
        // 갤 S3 - 1280 x 720, 1G   
        // 갤 A8 - 1920 x 1080
        // 갤 S7 - 2560 x 1440,
        // 갤 S8 - 2220 x 1080, 2960x1440, 4G
        // G4 - 2560 x 1440, 3G
        // iphone4 - 960 x 640
        // iphone5 - 1136 x 640 
        // iphone6 - 1334 x 750, 6 plus - 1920 x 1080
        // iphone7 - 1334 x 750, 7 plus - 1920 x 1080

        // SystemInfo.deviceModel        
        int level               = 0;
#if UNITY_IPHONE
        if(UnityEngine.iOS.Device.generation < UnityEngine.iOS.DeviceGeneration.iPhone5)
            level               = 0;
        else if(UnityEngine.iOS.Device.generation < UnityEngine.iOS.DeviceGeneration.iPhone6)
            level               = 1;
        else 
            level               = 2;

#else   // UNITY_ANDROID and all....
        
        if(SystemInfo.systemMemorySize >= 4000)
            level               = 2;    // 최신폰. 메모리 4G 이상.(혹은 대부분의 pc)
        else
        {
            if(Screen.width < 1000)
                level           = 0;
            else
                level           = 1;
        }
#endif
        // temp.
        level                   = 2;

        switch(level)
        {
        case 0:                 // 저사양. 
            Application.targetFrameRate = 35;
            QualitySettings.SetQualityLevel(0, true);
            break;
        case 2:                 // 고사양.
            Application.targetFrameRate = 60;
            QualitySettings.SetQualityLevel(1, true);
            break;
        case 1:                 // 중사양 - default.
        default:
            Application.targetFrameRate = 45;
            QualitySettings.SetQualityLevel(0, true);
            break;
        }
#if UNITY_IPHONE
        Application.targetFrameRate = 50;
#endif
        QualitySettings.vSyncCount  = 0;
        // 
    }
}
