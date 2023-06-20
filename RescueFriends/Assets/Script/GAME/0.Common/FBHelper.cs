using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
//using Facebook.Unity;

public class FBUser
{
    /*
	public string ID ="";
	public string Name ="";
	public Sprite Picture = null;
	public string PictureURL= "";

	public FBUser() 
	{
		ID = "";
		Name = "";
		PictureURL = "";
	}
	
	public FBUser(LitJson.JsonData userInfo) 
	{
		Debugger.Assert(userInfo.Keys.Contains("id"), "FBUser Initialize : ID is missing");
        Debugger.Assert(userInfo.Keys.Contains("first_name"), "FBUser Initialize : first_name is missing");
		Debugger.Assert(userInfo.Keys.Contains("picture"), "FBUser Initialize : Picture is missing");

        ID = userInfo["id"].ToString();
        if(userInfo.Keys.Contains("first_name") && string.IsNullOrEmpty(userInfo["first_name"].ToString()) == false)
            Name = userInfo["first_name"].ToString();
        else
            Name = "Friend";
        
		PictureURL = userInfo["picture"]["data"]["url"].ToString();
    }

    public void RequestPicture (System.Action<bool> callback) 
	{
        if (Picture == null) 
	       TaskManager.StartCoroutine(CoRequestPicture(callback));
        else
            if (callback != null) callback(true);
    }

    IEnumerator CoRequestPicture (System.Action<bool> callback) 
	{
        WWW www = new WWW(PictureURL);

        yield return www;

        if ((www.error != null) || (www.texture == null)) 
		{
            if (callback != null) callback(false);
        }
		else 
		{
			Picture = Sprite.Create(www.texture, new Rect(0f, 0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 100);
            if (callback != null) callback(true);
        }
    }
}

public static class FBHelper 
{    
    public static List<FBUser> InvitableFriends { get; private set; }

    static bool NetworkConnected
	{
        get { return (Application.internetReachability != NetworkReachability.NotReachable); }
    }

    static readonly string pictureSizeAPI = "picture.width(40).height(40)";

    public static void Login (System.Action<bool, string> callback) 
	{
        if (NetworkConnected == false) 
		{
            if (callback != null) callback(FB.IsLoggedIn, null);
            return;
        }

        if (FB.IsInitialized) 
		{
            _Login(callback);
        }
		else 
		{
            Scene.LockWithMsg("Initializing...");

			FB.Init(() => 
			{ 
				Scene.UnlockWithMsg();
                _Login(callback); 
            });
        }
    }
	
	public static void Logout()
	{
		FB.LogOut();
        //LGameData.GetInstance().FacebookLogOut();
		//Director.StartCoroutine(FBLogout());
    }
	
//	public static IEnumerator FBLogout()
//	{
//        //Scene.LockWithMsg("Wait...");
//		while (FB.IsLoggedIn)
//		{
//			yield return null;
//		}
//		
//		//Scene.UnlockWithMsg();
//	}

//    public static void RequestProfile (System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
//        if (NetworkConnected == false) 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_REACHABLE, null);
//            return;
//        }
//
//        if (FB.IsLoggedIn) 
//		{
//            _RequestProfile(callback);
//        }
//		else 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_LOGINED, null);
//        }
//    }

//    public static void RequestPresents (System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
//        if (NetworkConnected == false) 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_REACHABLE, null);
//            return;
//        }
//
//        if (FB.IsLoggedIn) 
//		{
//            _RequestPresents(callback);
//        }
//		else 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_LOGINED, null);
//        }
//    }

//    public static void RequestFriends (System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
//        if (NetworkConnected == false)
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_REACHABLE, null);
//            return;
//        }
//
//        if (FB.IsLoggedIn) 
//		{
//            _RequestFriends(callback);
//        }
//		else 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_LOGINED, null);
//        }
//    }

	public static void RequestInvitableFriends (System.Action<bool,  string> callback) 
	{
        if (NetworkConnected == false) 
		{
            if (callback != null) callback(FB.IsLoggedIn,  null);
            return;
        }

        if (FB.IsLoggedIn)
		{
            _RequestInvitableFriends(callback);
        }
		else 
		{
            if (callback != null) callback(FB.IsLoggedIn,  null);
        }
    }

    public static void SendAppInvitations (string[] to, System.Action<bool, string> callback) 
	{
        if (NetworkConnected == false) 
		{
            if (callback != null) callback(FB.IsLoggedIn, null);
            return;
        }

        if (FB.IsLoggedIn) 
		{
            _SendAppInvitations(to, callback);

        }
		else 
		{
            if (callback != null) callback(FB.IsLoggedIn,  null);
        }
    }
	
	//공유하기
    public static void ShareWithFirends(string toID, FBUser user, System.Action<bool, string> callback)
	{		

//        Dictionary<string,string> formDic = new Dictionary<string,string>();
//        formDic.Add("message", "message");
//        formDic.Add("link", "http://play.google.com/store/apps/details?id=com.nov9.jellomagicfirends");
//        formDic.Add("place", "place");
//        formDic.Add("description", "Description");
//        formDic.Add("caption", "Caption");
//        formDic.Add("picture", user.PictureURL);
//
//        Dictionary<string,string> privacyDic = new Dictionary<string,string>();
//        privacyDic.Add("value", "EVERYTONE");
//        formDic.Add("privacy", Facebook.MiniJSON.Json.Serialize(privacyDic));
//
//        Dictionary<string,string> actionDic = new Dictionary<string,string>();
//        actionDic.Add("name", "anctionName");
//        actionDic.Add("link", "http://play.google.com/store/apps/details?id=com.nov9.jellomagicfirends");
//        List<System.Object> actionDicList = new List<System.Object>();
//        actionDicList.Add(actionDic);
//        formDic.Add("actions", Facebook.MiniJSON.Json.Serialize(actionDicList));
//
//        FB.API("me/feed", HttpMethod.POST, (result)=> {
//            if (string.IsNullOrEmpty(result.Error)) 
//            {
//                if (callback != null) callback(true,  null);
//            }
//            else
//            {
//                if (callback != null) callback(false, result.Error);
//            }
//        }, formDic);

        Debug.Log(user.ID + "?fields=picture.type(large)");
        /*
        //FB.API("me?fields=name,email,picture,friends.fields(name,id,picture)", HttpMethod.GET, GetFacbookUserData);
        FB.API(user.ID + "?fields=picture.type(large)", HttpMethod.GET, (result)=> {
            Debug.Log(result.Error);
            Debug.Log(result.RawResult);
            Debug.Log(string.IsNullOrEmpty(result.Error));

            if (string.IsNullOrEmpty(result.Error)) 
            {
                LitJson.JsonData kJsonData = LitJson.JsonMapper.ToObject(result.RawResult);

                if(kJsonData.Keys.Contains("picture"))
                {
                    Debug.Log(kJsonData["picture"]["data"]["url"].ToString());
                  
//                    Application.OpenURL("https://www.facebook.com/dialog/feed?"+ "app_id="+"1224535684288911"+ "&link="+
//                        "http://sunbug.net"+ "&picture="+"http://nov9games.iptime.org:83/ts_share.png"+ "&name="+"Name"+ "&caption="+
//                        "Caption"+ "&description="+"Description"+
//                        "&redirect_uri=http://play.google.com/store/apps/details?id=com.nov9.jellomagicfirends/");
//                    
//                    //https://www.facebook.com/dialog/feed?app_id=1224535684288911&link=http://sunbug.net&picture=https://scontent.xx.fbcdn.net/v/t1.0-1/p200x200/1234251_567761826624005_1710907041_n.jpg?oh=31b2686faa5649a112b14340e0db0d01&oe=5A29BAFF&name=MyFunnyGame&caption=MyCaptionMessage&description=MyDescriptionMessage&redirect_uri=http://play.google.com/store/apps/details?id=com.nov9.jellomagicfirends
//

                    return;
                    FB.FeedShare (
                        toID,
                        //new System.Uri("http://sunbug.net"),
                        new System.Uri("http://play.google.com/store/apps/details?id=com.nov9.jellomagicfirends"),
                        "Tasty Magic",
                        "Caption",
                        "Description",
                        //new System.Uri(kJsonData["picture"]["data"]["url"].ToString()),
                        null,
                        //new System.Uri("http://nov9games.iptime.org:83/ts_share.png"),
                        string.Empty,
                        (result1)=> {
                       
                        if (string.IsNullOrEmpty(result.Error)) 
                        {
                            if (callback != null) callback(true,  null);
                        }
                        else
                        {
                            if (callback != null) callback(false, result.Error);
                        }
                    });

                }
            }
        });*
	}


//    private IEnumerator TakeScreenshot() 
//    {
//        yield return new WaitForEndOfFrame();
//
//        var width = Screen.width;
//        var height = Screen.height;
//        width = 512;
//        height = 512;
//        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
//        // Read screen contents into the texture
//        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//        tex.Apply();
//        byte[] screenshot = tex.EncodeToPNG();
//
//        var wwwForm = new WWWForm();
//        wwwForm.AddBinaryData("image", screenshot, "Screenshot.png");
//
//        FB.API("me/feed", HttpMethod.POST,(result)=> { }, wwwForm);
//    }


//    public static void SendPresent (string[] to, System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
//        if (NetworkConnected == false) 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_REACHABLE, null);
//            return;
//        }
//
//        if (FB.IsLoggedIn) 
//		{
//            _SendPresent(to, callback);
//        }
//		else 
//		{
//            if (callback != null) callback(FB.IsLoggedIn, FB_ERROR_TYPE.NOT_LOGINED, null);
//        }
//    }

    static void _Login (System.Action<bool, string> callback) 
	{
		if(FB.IsInitialized)
		{
			FB.ActivateApp();	
			
			if(FB.IsLoggedIn)
			{
				if (callback != null) callback(FB.IsLoggedIn,  null);
			}
			else
			{
                Scene.LockWithMsg("Signing...");
				FB.LogInWithReadPermissions (new List<string>(){"public_profile", "email", "user_friends"}, (result)=> {
					Scene.UnlockWithMsg();
					if(FB.IsLoggedIn) 
					{
						if (callback != null) callback(FB.IsLoggedIn,  result.RawResult);
					}
					else 
					{
						if (string.IsNullOrEmpty(result.Error) == false) 
						if (callback != null) callback(FB.IsLoggedIn,  result.Error);
					}
				});
			}
		}
    }

//    static void _RequestFriends (System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
////        FB.API("/me/friends?fields=id,name," + pictureSizeAPI,
////            Facebook.HttpMethod.GET, 
////            (result) => {
////                if (string.IsNullOrEmpty(result.Error)) {
////                    Friends = GetUsers(result);
////                    if (callback != null) callback(true, FB_ERROR_TYPE.NONE, null);
////                } else {
////                    Debug.LogWarning(result.Error);
////                    if (callback != null) callback(false, FB_ERROR_TYPE.UNKNOWN, result.Error);
////                }
////            }
////        );
//    }

	static void _RequestInvitableFriends (System.Action<bool,  string> callback) 
	{
		FB.API("/me/invitable_friends?fields=id,name," + pictureSizeAPI, HttpMethod.GET, (result) => {
                if (string.IsNullOrEmpty(result.Error)) 
				{
                    InvitableFriends = GetUsers(result);
                    if (callback != null) callback(true,  null);
                }
				else
				{
                    if (callback != null) callback(false, result.Error);
                }
            }
        );
    }

//    static void _RequestPresents (System.Action<bool, FB_ERROR_TYPE, string> callback) {
////		FB.API ("/me/apprequests", Facebook.HttpMethod.GET, (result) => {
////            if (string.IsNullOrEmpty(result.Error)) {
////                Debug.Log(result.Text);
////                if (callback != null) callback(true, FB_ERROR_TYPE.NONE, null);
////            } else {
////                Debug.LogWarning(result.Error);
////                if (callback != null) callback(false, FB_ERROR_TYPE.UNKNOWN, result.Error);
////            }
////		});
//    }

    static public void SendAppLink( System.Action<bool,string> callback)
    {
        /*
        System.Uri _url;

        #if LIVE_MODE
        _url = new System.Uri("http://tastymagic.ttsgames.com:80/JMF_main.jpg");
        #else
        _url = new System.Uri("http://nov9games.iptime.org:83/JMF_main.jpg");
        #endif

        FB.Mobile.AppInvite(new System.Uri("https://fb.me/1441145049294639"), _url,  (result)=> {
            if (string.IsNullOrEmpty(result.Error)) 
                callback(true,  result.RawResult);
            else 
                callback(false,  result.Error);
        }); *
    }

    static void _SendAppInvitations (string[] to, System.Action<bool,  string> callback) 
	{
        //AppRequest(string message, IEnumerable<string> to = null, IEnumerable<object> filters = null, IEnumerable<string> excludeIds = null, int? maxRecipients = null, string data = "", string title = "", FacebookDelegate<IAppRequestResult> callback = null);

        //FB.AppRequest("You already play Tasty Magic", null, new List<object>() { "app_non_users" }, null, 0, string.Empty, string.Empty, this.HandleResult);
        /*
        FB.AppRequest("You already play Tasty Magic", to, null, null, null, "", "Invitations", (result)=> {
            if (string.IsNullOrEmpty(result.Error)) 
			{
				if (callback != null) callback(true,  result.RawResult);
            }
			else 
			{
                if (callback != null) callback(false,  result.Error);
            }
        });*
    }

//    static void _SendPresent (string[] to, System.Action<bool, FB_ERROR_TYPE, string> callback) 
//	{
//        string msg = "You've got present !";
//        string title = "Hello Mr. Skyrack :D";
//        FB.AppRequest(msg, to, null, null, null, "{\"type\":\"request\",\"item\":\"Bomb\"}", title, (result)=> {
//            if (string.IsNullOrEmpty(result.Error)) 
//			{
//				Debug.Log(result.RawResult);
//                if (callback != null) callback(true, FB_ERROR_TYPE.NONE, null);
//            }
//			else 
//			{
//                Debug.LogWarning(result.Error);
//                if (callback != null) callback(false, FB_ERROR_TYPE.UNKNOWN, result.Error);
//            }
//        });
//    }

	static List<FBUser> GetUsers (IGraphResult result)///(FBResult result)
	{
        List<FBUser> users = new List<FBUser>();
		LitJson.JsonData kJsonData = LitJson.JsonMapper.ToObject(result.RawResult);
		if(kJsonData != null)
		{
			for (int i = 0; i < kJsonData["friends"].Count; i++) 
			{
				users.Add(new FBUser(kJsonData["friends"][i]));
			}
		}
//        List<object> data = GetData(result);

//        if (data != null)
//		{
//            for (int i = 0; i < data.Count; i++) {
//                users.Add(new FBUser(data[i] as Dictionary<string, object>));
//            }
//        }

        return users;
    }*/
}
