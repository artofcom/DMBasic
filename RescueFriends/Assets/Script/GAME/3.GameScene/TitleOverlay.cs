using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Facebook.Unity;
using Data;
//using Firebase.Database;
//using Firebase;
//using Firebase.Unity.Editor;

public class TitleOverlay : MonoBehaviour
{
    enum LOGIN_TYPE { NONE, GUEST, FACEBOOK  };

    public SpriteRenderer       _sprFader = null;
    public SpriteRenderer       _sprLoading = null;
    public tk2dTextMesh         _txtVersion = null;
    public tk2dTextMesh         _txtState   = null;
    public BasePopupHandler     _msgPopup   = null; 
    public Transform _trBtnGroup= null;

    public Transform            _trDlgLoading= null;
    public tk2dSlicedSprite     _sprProgress= null;
    public Transform            _trFox      = null;

    const float TIME_LOADING    = 5.0f;
    LOGIN_TYPE eLoginType       = LOGIN_TYPE.NONE;
    // DatabaseReference _fbDBRef  = null;
    bool _needBreakLoop         = false;
    bool _isNewUser4FireBase    = false;
    float _fRateStartSceneLoad  = .0f;

    void Awake()
    {
        //FireBaseHandler.IsInitialized   =  false;
        //FireBaseHandler.callbackOnError = OnFireBaseError;
        MessagePopupHandler._titleOverlay= this;
        _trDlgLoading.gameObject.SetActive( false );
        _trBtnGroup.gameObject.SetActive( true );

        //Scene.ClearAll();
        //Scene.   

        _sprFader.gameObject.SetActive( true );
        _sprFader.color         = new Color(0, 0, 0, 1.0f);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            _sprFader.DOFade(0.0f, 0.5f).OnComplete( () =>
            {
                _sprFader.gameObject.SetActive(false);
            });
        });
    }

    // value 0 ~ 100
    void _setLoadingProgress(float value)
    {
        value                   = value<0 ? 0 : ( value>100 ? 100 : value);
        value *= 0.01f;

        const float from        = 100.0f;
        const float to          = 880.0f;
        float progress          = (to-from)*value;
        _sprProgress.dimensions = new Vector2(from+progress, _sprProgress.dimensions.y);

        const float xFrom       = -2.92f;
        const float xTo         = 3.95f;
        progress                = (xTo-xFrom)*value;
        _trFox.transform.position   = new Vector3(xFrom+progress, _trFox.transform.position.y, _trFox.transform.position.z);
    }

	// Use this for initialization
	void Start ()
    {
        _txtState.text          = "initiating...";
        _txtVersion.text        = string.Format("Ver {0}", NOVNINE.Profile.CurrentVersion());
        _txtVersion.Commit();
        tk2dTextMesh txtShade   = _txtVersion.transform.GetChild(0).GetComponent<tk2dTextMesh>();
        txtShade.text           = string.Format("Ver {0}", NOVNINE.Profile.CurrentVersion());
        txtShade.Commit();

        _trBtnGroup.gameObject.SetActive( !FacebookHandler.getInstance().IsProcessAutoLogin );
        if(FacebookHandler.getInstance().IsProcessAutoLogin)
        {
            _triggerSpinner();

            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                Invoke("OnFaceBookError", 1.0f);
                return;
            }
        }

        if(null!=Root.Data)     Root.Data.reset();

        Director.SoundInit();
        if(false == NNSound.Instance.IsPlayingBGM())
            NNSoundHelper.PlayBGM("bgm_title"); 
        
        FacebookHandler.getInstance().registerCallbacks(OnFaceBookLoginSuccssed, OnFaceBookError);

        StartCoroutine( InfoLoader.init() );

        StartCoroutine( _coUpdator() );
	}
	
    void _triggerSpinner()
    {
        _trBtnGroup.gameObject.SetActive( false );
        _trDlgLoading.gameObject.SetActive( true );

        _setLoadingProgress(0);

        if(!_sprLoading.gameObject.activeSelf)
        {
            //StartCoroutine( _coRunSpinner() );
        }
    }

    IEnumerator _coRunSpinner()
    {
        int rot                 = 0;
        _sprLoading.gameObject.SetActive(true);
        do
        {
            _sprLoading.transform.localEulerAngles = new Vector3(0, 0, rot);
            yield return new WaitForSeconds(0.2f);

            rot -= 30;          rot %= 360;

        }while(true);
    }

    IEnumerator _loadNextScene()
    {
        _txtState.text          = "loading..";
        AsyncOperation ao       = SceneManager.LoadSceneAsync("Play", LoadSceneMode.Single);
        ao.allowSceneActivation = false;

        float fStart            = _fRateStartSceneLoad;
        float fElTime           = .0f;
        do
        {
            yield return new WaitForSeconds(0.05f);
            fElTime += 0.05f;

            _setLoadingProgress(fStart + (100.0f-_fRateStartSceneLoad)*ao.progress/0.9f);

        }while(ao.progress<0.9f || fElTime<TIME_LOADING);

        ao.allowSceneActivation = true;
    }

//#if !UNITY_EDITOR
        //UnityEngine.Analytics.Analytics.CustomEvent("TitlePlayClicked");
//#endif
  

    public void onBtnPlay()
    {
        if(FB.IsLoggedIn)       FB.LogOut();        

        NOVNINE.GameObjectExt.FadeOutRecursively(_trBtnGroup.gameObject, 0.5f, false, () =>
        {
            _triggerSpinner();
            eLoginType          = LOGIN_TYPE.GUEST;
            PlayerPrefs.SetString("LAST_LOGIN", "GUEST");

#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent("login", Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod, "guest");
#endif
        });
    }

    public void onBtnFacebook()
    {
        NOVNINE.GameObjectExt.FadeOutRecursively(_trBtnGroup.gameObject, 0.5f, false, () =>
        {
            _triggerSpinner();
            FacebookHandler.getInstance().FB_RequestLogin();
        });
    }

    public void OnFaceBookError()
    {
        if(_msgPopup.gameObject.activeSelf)
            return;

        // quit app.
        Debug.Log("Facebook Fatal Error !!!");  
        
        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Sorry. Please check\n your network.";
        _msgPopup.gameObject.SetActive(true);
        _msgPopup.OnEnterPopup(data2);
    }

    public void OnFireBaseError()
    {
        if(_msgPopup.gameObject.activeSelf)
            return;

        // quit app.
        Debug.Log("Firebase Fatal Error !!!");  
        
        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Sorry. Please check\n your network.";
        _msgPopup.gameObject.SetActive(true);
        _msgPopup.OnEnterPopup(data2);
    }

    public void onCloseErrorPopup()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnFaceBookLoginSuccssed(string strId)
    {
        Debug.Log("OnFaceBookLoginSuccssed called !!!");

        StartCoroutine( _coLoginToFirebase(strId) );        
    }

    IEnumerator _coLoginToFirebase(string strFBId)
    {
        _setLoadingProgress(10);
        yield return new WaitForSeconds(0.1f);

        yield return null;

        _txtState.text      = "Connecting...";        

        const float MAX_WAIT_SEC= 10.0f;
        float fElTime           = .0f;
       /* while(false == FireBaseHandler.IsInitialized)
        {
            yield return new WaitForSeconds(0.3f);
            fElTime += 0.3f;
            if(fElTime >= MAX_WAIT_SEC)
            {
                _txtState.text  = "connecting TimeOut....";
                Debug.Log("FireBase Init TimeOut !!!");
                yield break;
            }
        }*/
        _setLoadingProgress(15);
        yield return new WaitForSeconds(0.1f);

        // 
        string strKey           = "FACEBOOK_" + strFBId;
        PlayerPrefs.SetString("LAST_LOGIN", strKey);
        /*
#if !UNITY_EDITOR
        Firebase.Analytics.FirebaseAnalytics.LogEvent("login", Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod, "facebook");
#endif

        // 1. 조회.
        _fbDBRef                = FirebaseDatabase.DefaultInstance.RootReference;

        // 2. 있으면 데이터 가져옴.
        bool wait               = true;
        FirebaseDatabase.DefaultInstance.GetReference("users/"+strKey+"/playerData/m_siCountBuff").GetValueAsync().ContinueWith((task) =>
        {
            if (task.IsFaulted) {
                Debug.Log("base read failed !");
            }
            else if (task.IsCompleted) {
                _isNewUser4FireBase = (task.Result==null || null==task.Result.Value);
                Debug.Log("Base data received ok. - new user : " + _isNewUser4FireBase);
            }  
            wait                = false;
        });
        while(wait)             { yield return new WaitForSeconds(0.1f); }
        _setLoadingProgress(25);
        yield return new WaitForSeconds(0.1f);

        if(false == _isNewUser4FireBase)
        {
            yield return StartCoroutine( _readDataFromFireBase(strKey) );
            Root.Data.initCurrentLevel();
        }
        // 3. 없으면 지금 데이터 DB에 write.
        else
        {
            FireBaseHandler.IsInitialized   = false;    // for load local data.
            Root.Data           = Root.Load();
            Root.Data.gameData.LoadContext();
            FireBaseHandler.IsInitialized   = true;     // do not use local data anymore.

            yield return StartCoroutine( _writeDataToFireBase(strKey) );

            Root.Data.initCurrentLevel();            
        }
       
        Root.Data.gameData.key  = strKey;
        //StartCoroutine( _loading() );
        eLoginType              = LOGIN_TYPE.FACEBOOK;
        */
    }


    IEnumerator _readDataFromFireBase(string strKey)
    {
        yield break;
        /*
        _txtState.text          = "reading data .. 0%";

        Root.Data               = new Data.Root();
        Root.Data.gameData      = new GameData();

        DatabaseReference       usersDB = _fbDBRef.Child("users").Child(strKey);
        Root.Data.gameData.record= new GameData.Record();
         
        //yield return StartCoroutine( FireBaseHandler.read<GameData.Record>(usersDB.Child("base"), (ret, successed) =>
        //{
        //    _txtState.text      = "reading data .. 10";
        //    if(successed)       Root.Data.gameData.record   = ret;
        //}) );

        Debug.Log("@@@ Try Read PlayerData...");
        yield return StartCoroutine( FireBaseHandler.read<PuzzlePlayerGameBaseData>(usersDB.Child("playerData"), (ret, successed) =>
        {
            _txtState.text      = "reading data .. 20%";
            if(successed)       Root.Data.gameData.record.playerData   = ret;
        }) );
        _setLoadingProgress(35);

        Debug.Log("@@@ Try Read WalletInventory data...");
        yield return StartCoroutine( FireBaseHandler.read<PuzzlePlayerWalletInventoryData>(usersDB.Child("WalletInventoryData"), (ret, successed) =>
        {
            _txtState.text      = "reading data .. 40%";
            if(successed)       Root.Data.gameData.record.WalletInventoryData   = ret;
        }) );
        _setLoadingProgress(45);
        Debug.Log("@@@ Try Read Level data..." + Root.Data.TotalLevelCount);

        DatabaseReference       refDB = null;
        int _100                = 100;
        int idxStart            = 0;
        string strLvKey         = "";
        do
        {
            strLvKey            = string.Format("LV{0}-{1}", idxStart+1, idxStart+_100);
            refDB               = usersDB.Child( strLvKey );
            yield return StartCoroutine( FireBaseHandler.read<string>(refDB, (ret, successed) =>
            {
                if(successed)   Root.Data.gameData.AddLevelResultDataFromFireBase(ret);
                _needBreakLoop  = !successed;
            }) );
            if(_needBreakLoop)  break;
            _txtState.text      = string.Format( "reading [stage {0}]'s data ..", idxStart+1 );
            idxStart += _100;

        }while(true);
        _txtState.text          = "reading data .. 70%";
        _setLoadingProgress(70);

        Debug.Log("@@@ Try Read RewardedMissionId data...");
        
        refDB                   = usersDB.Child("RMission");
        yield return StartCoroutine( FireBaseHandler.read<string>(refDB, (ret, successed) =>
        {
            if(successed)   Root.Data.gameData.SetRewardedMissionDataFromFireBase( ret );
        }) );
        _txtState.text      = "reading data .. 85%";
        _setLoadingProgress(80);

        int max_try         = 1000;
        Debug.Log("@@@ Try Read PurchaseInfo data...");
        for(int qq = 0; qq < max_try; ++qq)
        {
            refDB               = usersDB.Child("PCs"+qq);
            if(null==refDB)     break;
            yield return StartCoroutine( FireBaseHandler.read<Purchaseinfo>(refDB, (ret, successed) =>
            {
                if(successed)   Root.Data.gameData.AddPurchaseInfo( ret );
                _needBreakLoop  = !successed;
            }) );
            if(_needBreakLoop)  break;
        }
        _txtState.text          = "reading data .. 100%";
        _setLoadingProgress(90);
        _fRateStartSceneLoad    = 90.0f;*/
    }

    IEnumerator _writeDataToFireBase(string strKey)
    {
        yield break;
        /*
        GameData oData      = Root.Data.gameData;// new GameData();   

        Debug.Log("Writing New User data to FireBase DB ; " + strKey);
    
        DatabaseReference  usersDB = _fbDBRef.Child("users").Child(strKey);
        _txtState.text      = "writting data .. 0%";

        // Player Data.
        if(null==oData.record)  oData.record = new GameData.Record();
        FireBaseHandler.write( usersDB.Child("playerData"), oData.record.playerData );
        _setLoadingProgress(35);
        yield return new WaitForSeconds(0.2f);
        _txtState.text      = "writting data .. 40%";

        // LevelResultDataList        
        if(null != oData.record.LevelResultDataList)
        {
            int _100            = 100;
            int idxStart        = 0;
            do
            {
                if(idxStart >= oData.record.LevelResultDataList.Length)
                    break;
                
                string strLvKey = string.Format("LV{0}-{1}", idxStart+1, idxStart+_100);
                string strBuffer= InfoLoader.buildLVGradeData(idxStart);
                
                //FireBaseHandler.write( usersDB.Child("LVs"+zz), oData.record.LevelResultDataList[zz] );

                FireBaseHandler.update( strLvKey, strBuffer); 
                yield return new WaitForSeconds(0.2f);
                idxStart += _100;

            }while(true);
            
            _txtState.text      = "writting data .. 70%";
        }
        _setLoadingProgress(45);

        // RewardedMissionIds
        if(null != oData.record.RewardedMissionIds)
        {
            string strBuffer= InfoLoader.buildRMissionData();
            FireBaseHandler.update( "RMission", strBuffer);             

            yield return new WaitForSeconds(0.2f);
            _txtState.text  = "writting data .. 80%";
        }
        _setLoadingProgress(55);

        // PurchaseInfoList
        if(null != oData.record.PurchaseInfoList)
        {
            for(int zz = 0; zz < oData.record.PurchaseInfoList.Length; ++zz)
            {
                FireBaseHandler.write( usersDB.Child("PCs"+zz), oData.record.PurchaseInfoList[zz] );
            }        
            yield return new WaitForSeconds(0.2f);
            _txtState.text  = "writting data .. 90";
        }
        _setLoadingProgress(75);

        FireBaseHandler.write( usersDB.Child("WalletInventoryData"), oData.record.WalletInventoryData);
        _setLoadingProgress(85);
        _fRateStartSceneLoad= 85.0f;

        _txtState.text      = "writting data .. 100%";

        oData.key           = strKey;
        Root.Data.gameData  = oData;*/
    }

    IEnumerator _coUpdator()
    {
        int counter         = 0;
        do
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("=== updator 1..." + counter++);
           // if(false == FireBaseHandler.IsInitialized)
           //     continue;
            
            Debug.Log("=== updator 2..." + counter++);
            // _txtState.text      = "Firebase init successed....";

            switch(eLoginType)
            {
            case LOGIN_TYPE.GUEST:
                _trBtnGroup.gameObject.SetActive( false );

                _setLoadingProgress(10);
                yield return new WaitForSeconds(0.1f);

                _setLoadingProgress(20);
                yield return new WaitForSeconds(0.1f);

                if(null != Root.Data)
                    Root.Data.reset();

                _setLoadingProgress(30);
                yield return new WaitForSeconds(0.1f);

                Root.Data       = Root.Load();
                Root.Data.gameData.LoadContext();
                Root.Data.initCurrentLevel();

                _setLoadingProgress(40);
                yield return new WaitForSeconds(0.1f);

                _setLoadingProgress(50);
                yield return new WaitForSeconds(0.1f);

                _fRateStartSceneLoad = 50.0f;

                StartCoroutine( _loadNextScene() );
                Debug.Log("U Logged in as Guest Mode.");
                Debug.Log("=== updator 4..." + counter++);
                break;

            case LOGIN_TYPE.FACEBOOK:
                _trBtnGroup.gameObject.SetActive( false );
                StartCoroutine( _loadNextScene() );
                Debug.Log("U Logged in as Facebook Linked Mode.");
                Debug.Log("=== updator 5..." + counter++);
                break;

            case LOGIN_TYPE.NONE:
            default:
                Debug.Log("=== updator 3..." + counter++);
                continue;
            }

            break;

        }while(true);        
    }
}
