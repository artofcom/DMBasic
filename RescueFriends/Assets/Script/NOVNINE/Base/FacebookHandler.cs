using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class FacebookHandler : MonoBehaviour {

    // accessor.
    static FacebookHandler mHandler = null;
    public static FacebookHandler getInstance()
    {
        return mHandler;
    }
    //

    System.Action<string>       _OnLoginSuccessed = null;
    System.Action               _OnError = null;

    bool _isProcessAutoLogin    = false;
    public bool IsProcessAutoLogin
    {
        private set { _isProcessAutoLogin = value;  }
        get         { return _isProcessAutoLogin;  }
    }

    string _strUserId           = "";
    public string UserId
    {
        private set { _strUserId = value;       }
        get         { return _strUserId;        }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        mHandler                = this;

        string strFBId          = PlayerPrefs.GetString("LAST_LOGIN", "");
        if(false==strFBId.Equals("") && strFBId.Contains("FACEBOOK"))
            _isProcessAutoLogin = true;
    }

	// Use this for initialization
	void Start ()
    {
        // Auto Login Process.
        if(_isProcessAutoLogin)
        {
            if(!FB.IsInitialized)
                FB.Init(_fb_onInitComplete);
            else 
                _fb_onInitComplete();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void registerCallbacks(System.Action<string> onSuccess, System.Action onFailed)
    {
        _OnLoginSuccessed       = onSuccess;
        _OnError                = onFailed;
    }

    public void FB_RequestLogin()
    {
        // internet check first.
        if(!FB.IsInitialized)   FB.Init(_fb_onInitComplete);
    }
    void _fb_onInitComplete()
    {
        Debug.Log("FB init Sueecssed!");
        FB.LogInWithReadPermissions( new List<string>(){"public_profile"}, _fb_onAuthCallback);
    }
    void _fb_onAuthCallback(Facebook.Unity.ILoginResult res)
    {
        bool isError            = (null!=res.Error && res.Error.Length>0);
        isError |= res.ResultDictionary==null || false==res.ResultDictionary.ContainsKey("user_id");
        if(isError)
        {
            Debug.Log("FB Login Error : " + res.Error);
            if(null != _OnError)_OnError();
            return;
        }

        Debug.Log("FB Login Sueecssed! " + res.RawResult);

        string fb_id            = (string)res.ResultDictionary["user_id"];
        //PlayerPrefs.SetString("LAST_LOGIN", "FACEBOOK_" + fb_id);
        if(null != _OnLoginSuccessed)
            _OnLoginSuccessed(fb_id);

        this.UserId             = fb_id;
    }
}
