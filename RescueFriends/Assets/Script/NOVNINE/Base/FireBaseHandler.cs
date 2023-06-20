using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
// using Firebase.Database;
using System.Threading.Tasks;

class FireBaseHandler : MonoBehaviour
{
}

    /*
    // accessor.
    static FireBaseHandler mHandler = null;
    public static FireBaseHandler getInstance()
    {
        return mHandler;
    }
    //
    public static bool          IsInitialized   = false;
    //public static string        UserKey         = "";

    static float                TIME_RETRY_OFFLINE  = 10.0f;
    static float                TIME_RETRY_POPUP_OFFLINE  = 5.0f;
    static float                TIME_WAIT_FIREBASE_RESPONSE  = 30.0f;

    public static Action        callbackOnError = null;
    //static object               mRetryObject    = null;
    //static string               mRetryKey       = "";
    static Action               mCbCommitSuccessed = null;
    //static DatabaseReference    mWriteDBRef     = null;
    static bool                 _waitTillFlush  = false;
    static bool                 _forceBreak     = false;

    // # data send strategy.
    // send data를 queue에 넣고 보내기 성공하면 pop 한다.
    // 응답이 꼭 필요한 send에서 모두 보내기가 성공됐을때 까지 hold한다.
    // 그렇지 못하다면 app이 close 된다.
    public class _PACKET
    {
        public string           strKey;
        public DatabaseReference dbRef;
        public object           objData;
        public void set(string key, DatabaseReference dbRef, object data)
        {
            this.strKey         = key;
            this.dbRef          = dbRef;
            this.objData        = data;
        }
    };
    static Queue<_PACKET>       _queuePackets = new Queue<_PACKET>();
    static _PACKET              _curPacket  = null;
    static bool                 _popupWait  = false;


    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        mHandler                = this;
    }

	// Use this for initialization
	void Start ()
    {
        // Auto Login Process.
        _init();

        StartCoroutine( _coUpdate() );
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator _coUpdate()
    {
        do
        {
            if(null==_curPacket && _queuePackets.Count>0)
                yield return StartCoroutine( _send() );
            else
                yield return new WaitForSeconds(0.1f);

        }while(true);
    }


    void _init()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            var dependencyStatus    = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("FireBaseHandler::_init() => Firebase init successfully.");

            //    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://rainbow-fox.firebaseio.com/");
                IsInitialized       = true;
                //_txtState.text      = "Firebase init successed....";

                Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

#if !UNITY_EDITOR
                Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
#endif

            }
            else
            {
                Debug.LogError(System.String.Format("FireBaseHandler::_init() => Could not resolve all Firebase dependencies: {0}", dependencyStatus));

                IsInitialized       = false;

                if(null != callbackOnError)
                    callbackOnError();
            }
        });
//#endif

         /*
            Firebase.Analytics.Parameter[] LevelUpParameters = {
          new Firebase.Analytics.Parameter(
            Firebase.Analytics.FirebaseAnalytics.ParameterLevel, 5),
          new Firebase.Analytics.Parameter(
            Firebase.Analytics.FirebaseAnalytics.ParameterCharacter, "mrspoon"),
          new Firebase.Analytics.Parameter(
            "hit_accuracy", 3.14f)
        };
        */
        /*
    }

  
 
    // json data only.
    public static void write(DatabaseReference dbRef, object data, bool retryWhenFailed=false, Action cb_successed=null)
    {        
        if( isGuestMode() )
        {
            Debug.Log("write() ; OffLine(Guest) Mode.");
            if(null != cb_successed)
                cb_successed();
            return;
        }

        if(_waitTillFlush)
        {
            Debug.Assert(_waitTillFlush, "Wait Mode Triggered !!!");
            return;
        }
               
        //Debug.Assert(mRetryObject==null, "write : can't write before retrying objects...");

        _PACKET pkt             = new _PACKET();
        pkt.set(null, dbRef, data);
        _queuePackets.Enqueue( pkt );
        _forceBreak             = true;

        mCbCommitSuccessed      = cb_successed;
        _waitTillFlush          = retryWhenFailed;
    }

    public static void update(string key, object entryValue, bool retryWhenFailed=false, Action cb_successed=null)
    {
        if(isGuestMode())
        {
            Debug.Log("update() ; OffLine(Guest) Mode.");
            if(null != cb_successed)
                cb_successed();
            return;
        }

        if(_waitTillFlush)
        {
            Debug.Assert(_waitTillFlush, "Wait Mode Triggered !!!");
            return;
        }

        _PACKET pkt             = new _PACKET();
        pkt.set(key, null, entryValue);
        _queuePackets.Enqueue( pkt );
        _forceBreak             = true;

        mCbCommitSuccessed      = cb_successed;
        _waitTillFlush          = retryWhenFailed;
    }

    static IEnumerator _send()
    {
        Debug.Assert(_queuePackets.Count > 0);
        if(_queuePackets.Count==0)
            yield break;

        float fElTime           = .0f;
        // network off시.
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("write() ; NetworkReachability.NotReachable....");

            // 일반 데이터 전송 -> 기다리지 않음.
            if(!_waitTillFlush)
            {
                // 특정시간 후에 (내부적으로) 재시도.
                fElTime         = .0f;
                while(fElTime < TIME_RETRY_OFFLINE)
                {
                    yield return new WaitForSeconds(0.1f);
                    fElTime += 0.1f;
                    if(_forceBreak)
                        break;
                };
            }
            else
            {
                // 대기 데이터 전송 -> 팝업을 띄워 상황 노티.
                yield return mHandler.StartCoroutine( _showNotice() );

                if(Scene.CurrentPopupName()!="LoadingPopup")
                    Scene.ShowPopup("LoadingPopup");

                // 특정시간 후에 재시도.
                fElTime         = .0f;
                while(fElTime < TIME_RETRY_POPUP_OFFLINE)
                {
                    yield return new WaitForSeconds(0.1f);
                    fElTime += 0.1f;
                    if(_forceBreak)
                        break;
                };
            }

            _forceBreak         = false;
            yield break;
        }

        _curPacket              = _queuePackets.Peek();

        if(null!=_curPacket.strKey && _curPacket.strKey.Length>0)
        {
            Debug.Log("update() ; UpdateChildrenAsync called.....");
            Dictionary<string, object> childUpdates = new Dictionary<string, object>();
            childUpdates["users/"+ getUserKey() + "/" + _curPacket.strKey] = _curPacket.objData;
            FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync( childUpdates ).ContinueWith( (task) =>
            {
                Debug.Log(string.Format("FirebaseHandler::UpdateChildrenAsync => IsCompleted:{0} IsCanceled:{1} IsFaulted:{2}", task.IsCompleted, task.IsCanceled, task.IsFaulted));
                _onFireBaseResponse(task);                
            });
        }
        else
        {
            Debug.Log("write() ; SetRawJsonValueAsync called.....");
            string json         = JsonUtility.ToJson(_curPacket.objData );
            _curPacket.dbRef.SetRawJsonValueAsync(json).ContinueWith( (task) =>
            {
                Debug.Log(string.Format("FirebaseHandler::SetRawJsonValueAsync => IsCompleted:{0} IsCanceled:{1} IsFaulted:{2}", task.IsCompleted, task.IsCanceled, task.IsFaulted));
                _onFireBaseResponse(task);                
            });
        }

        if(_waitTillFlush)
        {
            yield return new WaitForSeconds(0.3f);
            // sending fininshed ???
            if (null==_curPacket || _forceBreak)
                yield break;

            if(Scene.CurrentPopupName()!="LoadingPopup")
                Scene.ShowPopup("LoadingPopup");
        }

        // 특정시간 후에 재시도.
        fElTime                 = .0f;
        while(fElTime < TIME_WAIT_FIREBASE_RESPONSE)
        {
            yield return new WaitForSeconds(0.1f);
            fElTime += 0.1f;
            if(_forceBreak)
                break;
        };
        _forceBreak             = false;
        yield return new WaitForSeconds(0.5f);
    }

    static void _onFireBaseResponse(Task task)
    {
        if(task.IsCompleted && false==task.IsFaulted)
            _queuePackets.Dequeue();
                
        if( //0==_queuePackets.Count && 
           Scene.CurrentPopupName()=="LoadingPopup")
            Scene.ClosePopup();

        if(task.IsCompleted && false==task.IsFaulted)
        {
            if(null != mCbCommitSuccessed)
                mCbCommitSuccessed();
        }
        if(0 == _queuePackets.Count)
            _waitTillFlush      = false;

        _curPacket              = null;
        _forceBreak             = true;
    }
  

    static IEnumerator _showNotice()
    {
        if(Scene.CurrentPopupName() == "LoadingPopup")
            Scene.ClosePopup();

        MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
        data2.isOkOnly          = true;
        data2.strMessage        = "Please check your network.\nRetry sending data.";
        data2.emotion           = MessagePopupHandler.EMOTIONS.SAD;
        Scene.ShowPopup("MessagePopup", data2, null);
        
        while( Scene.CurrentPopupName() == "MessagePopup" )
        {   yield return new WaitForSeconds(0.5f); }

        yield return new WaitForSeconds(0.1f);
    }

    public static IEnumerator read<T>(DatabaseReference dbRef, System.Action<T, bool> callbackSetBuffer)
    {
        Debug.Log("FireBaseHandler::read() ; Try Read ... ");// + default(T).GetType() );

        if(null == dbRef)
        {
            Debug.Log("FireBaseHandler::read() ; DB-Ref is NULL !!!");
            callbackSetBuffer( default(T), false );
            yield break;
        }

        bool wait               = true;
        dbRef.GetValueAsync().ContinueWith( (task) => 
        {            
            Debug.Log(string.Format("FirebaseHandler::read() => IsCompleted:{0} IsCanceled:{1} IsFaulted:{2}", task.IsCompleted, task.IsCanceled, task.IsFaulted));

            if (task.IsFaulted)     callbackSetBuffer( default(T), false );
            else if (task.IsCompleted)
            {
                if(null==task.Result || null==task.Result.Value)
                {
                    Debug.Log("complete ok, but value is null !");
                    callbackSetBuffer( default(T), false );
                }
                else
                {
                    Debug.Log("complete ok, json value is.. " + task.Result.GetRawJsonValue());
                    JsonFx.Json.JsonReader  reader = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                    Debug.Log("complete ok, stage 2...");
                    callbackSetBuffer( reader.Deserialize<T>(), true );
                }
            }
            wait                = false;
        });
        while(wait)             { yield return null; }
    }

    public static DatabaseReference getUserDB()
    {
        if(false == IsConnected())
            return null;

        return FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child( getUserKey() );
    }

    public static bool IsConnected()
    {
        return (IsInitialized && false==isGuestMode());
    }

    static string getUserKey()
    {
        return PlayerPrefs.GetString("LAST_LOGIN", "");
    }
    static bool isGuestMode()   // local mode ? offline mode ?
    {
        string strKey           = getUserKey();
        return (!FB.IsLoggedIn || strKey.Contains("GUEST"));
    }
}

 /*  public static void write(string strKey, object data, bool asJson=true)
    {
        if(false == IsConnected())
            return;

        DatabaseReference       baseRef = FirebaseDatabase.DefaultInstance.RootReference;
        write(baseRef.Child(UserKey).Child(strKey), data, asJson);
    }*/


/*
 *  

 *  _fbDBRef.Child("users").Child(strKey).Child("base").GetValueAsync().ContinueWith( (task) => 
        {
            _txtState.text      = "reading data .. 10";
            if (task.IsFaulted) {
                Debug.Log("base read failed !");
            }
            else if (task.IsCompleted) {
                JsonFx.Json.JsonReader      reader = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                Root.Data.gameData.record   = reader.Deserialize<GameData.Record>();
                Debug.Log("base data received ok.");
            }
            wait                = false;
        });
        while(wait)             { yield return new WaitForSeconds(0.1f); }

    _txtState.text          = "reading data .. 20";
        wait                    = true;
        _fbDBRef.Child("users").Child(strKey).Child("playerData").GetValueAsync().ContinueWith( (task) => 
        {
            _txtState.text      = "reading data .. 30";
            if (task.IsFaulted) {
                Debug.Log("playerdata read failed !");
            }
            else if (task.IsCompleted) {
                JsonFx.Json.JsonReader      reader      = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                Root.Data.gameData.record.playerData    = reader.Deserialize<PuzzlePlayerGameBaseData>();
                Debug.Log("playerdata data received ok.");
            }
            wait                = false;
        });
        while(wait)             { yield return new WaitForSeconds(0.1f); }

    _txtState.text          = "reading data .. 40";
        wait                    = true;
        _fbDBRef.Child("users").Child(strKey).Child("WalletInventoryData").GetValueAsync().ContinueWith( (task) => 
        {
            _txtState.text      = "reading data .. 50";
            if (task.IsFaulted) {
                Debug.Log("walletinventorydata read failed !");
            }
            else if (task.IsCompleted) {
                JsonFx.Json.JsonReader      reader      = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                Root.Data.gameData.record.WalletInventoryData = reader.Deserialize<PuzzlePlayerWalletInventoryData>();
                Debug.Log("walletinventorydata received ok.");
            }
            wait                = false;
        });
        while(wait)             { yield return new WaitForSeconds(0.1f); }


    _txtState.text          = "reading data .. 60";
        wait                    = true;
        for(int qq = 0; qq < Root.Data.ClearedMaxLevelID; ++qq)
        {
            _idxRead            = qq;
            DatabaseReference DBTarget = _fbDBRef.Child("users").Child(strKey).Child("LevelResultDataList"+qq);
            if(null == DBTarget)
            {
                wait            = false;
                break;
            }
            DBTarget.GetValueAsync().ContinueWith( (task) => 
            {
                if (task.IsFaulted) {
                    Debug.Log("levelresultdata read failed ! ; " + _idxRead);
                }
                else if (task.IsCompleted) {
                    Debug.Log("levelresultdata received ok. ; " + _idxRead);
                    JsonFx.Json.JsonReader      reader      = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                    LevelResultData temp        = reader.Deserialize<LevelResultData>();
                    Root.Data.gameData.AddLevelResultData( temp );
                }
                _idxRead    = -1;
            });

            while(_idxRead>=0) { yield return new WaitForSeconds(0.1f); }
        }


    _txtState.text          = "reading data .. 70";
        wait                    = true;
        for(int qq = 0; qq > max_try ; ++qq)
        {
            _idxRead            = qq;
            DatabaseReference DBTarget = _fbDBRef.Child("users").Child(strKey).Child("RewardedMissionIds"+qq);
            if(null == DBTarget)
            {
                wait            = false;
                break;
            }
            DBTarget.GetValueAsync().ContinueWith( (task) => 
            {
                if (task.IsFaulted) {
                    Debug.Log("RewardedMissionIds read failed ! ; " + _idxRead);
                }
                else if (task.IsCompleted) {
                    Debug.Log("RewardedMissionIds received ok. ; " + _idxRead);
                    JsonFx.Json.JsonReader  reader      = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                    int temp                = reader.Deserialize<int>();
                    Root.Data.gameData.addRewardedMission( temp );
                }
                _idxRead    = -1;
            });

            while(_idxRead>=0) { yield return new WaitForSeconds(0.1f); }
        }

    _txtState.text          = "reading data .. 80";
        wait                    = true;
        for(int qq = 0; qq > max_try ; ++qq)
        {
            _idxRead            = qq;
            DatabaseReference DBTarget = _fbDBRef.Child("users").Child(strKey).Child("PurchaseInfoList"+qq);
            if(null == DBTarget)
            {
                wait            = false;
                break;
            }
            DBTarget.GetValueAsync().ContinueWith( (task) => 
            {
                if (task.IsFaulted) {
                    Debug.Log("PurchaseInfoList read failed ! ; " + _idxRead);
                }
                else if (task.IsCompleted) {
                    Debug.Log("PurchaseInfoList received ok. ; " + _idxRead);
                    JsonFx.Json.JsonReader  reader      = new JsonFx.Json.JsonReader(task.Result.GetRawJsonValue());
                    Purchaseinfo temp       = reader.Deserialize<Purchaseinfo>();
                    Root.Data.gameData.AddPurchaseInfo( temp );
                }
                _idxRead    = -1;
            });

            while(_idxRead>=0) { yield return new WaitForSeconds(0.1f); }
        }





     // value data should use this one.
    public static void update(string key, object entryValue, bool retryWhenFailed=false, Action cb_successed=null)
    {
        if(isGuestMode())
        {
            Debug.Log("update() ; OffLine(Guest) Mode.");
            if(null != cb_successed)
                cb_successed();

            mRetryObject        = null;
            return;
        }

        mCbCommitSuccessed      = cb_successed;

        Debug.Assert(mRetryObject==null, "update : can't update before retrying objects...");

        if(retryWhenFailed)
        {
            mRetryObject        = entryValue;
            mRetryKey           = key;            
        }

        if(Application.internetReachability==NetworkReachability.NotReachable)
        {
            Debug.Log("write() ; NetworkReachability.NotReachable....");

            if (retryWhenFailed)_onCommitDataFinished(true, false);
            else                _onCommitDataFinished(true, true);

            return;
        }

        if(retryWhenFailed && Scene.CurrentPopupName()!="LoadingPopup")
            Scene.ShowPopup("LoadingPopup");

        Debug.Log("update() ; UpdateChildrenAsync called.....");
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["users/"+ getUserKey() + "/" + key] = entryValue;
        FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync( childUpdates ).ContinueWith( (task) =>
        {
            Debug.Log(string.Format("FirebaseHandler::UpdateChildrenAsync => IsCompleted:{0} IsCanceled:{1} IsFaulted:{2}", task.IsCompleted, task.IsCanceled, task.IsFaulted));
            
            _onCommitDataFinished(true, task.IsCompleted && !task.IsFaulted);
        });
    }
 * */