#if USE_RemoteNotification

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using NOVNINE;
using NOVNINE.Diagnostics;

#pragma warning disable 0414 // value is assigned but its value is never used

public class RemoteNotification : MonoBehaviour
{
    public bool Completed = false;
	public string androidGCMsendID;//drawlineclassic key
    private string deviceToken = null;


    public string GetDeviceToken()
    {
        return deviceToken;
    }

    void Awake()
    {
        Context.RemoteNotification = this;
    }

    public void PushMessage()
    {
#if UNITY_ANDROID
        if( Application.platform == RuntimePlatform.Android ) 
        {
            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using ( AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent"))
                    {
                        string strmessageData = intent.Call<string>("getStringExtra","messageData");
                        if(string.IsNullOrEmpty(strmessageData) == false )
                            OnMessage(strmessageData);
//                        string strExpire = intent.Call<string>("getStringExtra","expire");
//                        Debug.Log("strExpire:" + strExpire);
//                        if(string.IsNullOrEmpty(strExpire) == false )
//                        {
//                            LGameData lGameData = LGameData.GetInstance();
//                            System.DateTime _time = System.DateTime.ParseExact(strExpire, "yyyy-MM-dd HH:mm:ss", null);
//                            if(_time > lGameData.GetCurrentServerTime())
//                            {
//                                lGameData.SetPushData(intent.Call<string>("getStringExtra","message_id"), intent.Call<int>("getIntExtra","pushType",0),
//                                    intent.Call<int>("getIntExtra","reward0",0),intent.Call<int>("getIntExtra","reward0Num",0),
//                                    intent.Call<int>("getIntExtra","reward1",0),intent.Call<int>("getIntExtra","reward1Num",0),
//                                    intent.Call<int>("getIntExtra","reward2",0),intent.Call<int>("getIntExtra","reward2Num",0) );
//                                System.Byte[] packet = LPuzzlePacket.REQ_PUSH_REWARD();
//                                if(packet != null)
//                                    LManager.GetInstance().Network_SendPacket(ref packet);
//                            }
//                        }
                        intent.Call("removeExtra","messageData");
                    }
                }
            }
        }
#elif UNITY_IPHONE

#endif

    }

    void OnApplicationPause(bool pause)
    {
#if UNITY_ANDROID
        
        if(PlayerPrefs.GetInt("usePushNotification") == 1)
        {
            using (AndroidJavaClass _gcmClass = new AndroidJavaClass ("nov9.gcm.Util")) 
            {
                if (true == pause)
                    _gcmClass.SetStatic ("notificationsEnabled",true);
                else
                    _gcmClass.SetStatic ("notificationsEnabled",false);
            }    
        }
#endif
    }

    void OnApplicationQuit()
    {
#if UNITY_ANDROID
        if( Application.platform == RuntimePlatform.Android ) 
        {
            using (AndroidJavaClass _gcmClass = new AndroidJavaClass ("nov9.gcm.Util")) 
            {
                _gcmClass.SetStatic ("notificationsEnabled",true);
            }   
        }
#endif
    }


    void Start()
    {
        Register();
    }

    public void Register()
    {
#if UNITY_IPHONE
        //NotificationServices.ClearLocalNotifications();
        //NotificationServices.ClearRemoteNotifications();
        if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXEditor) {
            UnityEngine.iOS.NotificationServices.RegisterForNotifications(
                UnityEngine.iOS.NotificationType.Alert |
                UnityEngine.iOS.NotificationType.Badge |
                UnityEngine.iOS.NotificationType.Sound);
        }
#elif UNITY_ANDROID
        if( Application.platform == RuntimePlatform.Android ) 
        {
            using (AndroidJavaClass _gcmClass = new AndroidJavaClass ("nov9.gcm.UnityGCMRegister")) 
            {
                _gcmClass.CallStatic ("SetGameObjectName","__RemoteNotification__");
                _gcmClass.CallStatic ("register", androidGCMsendID);
            }

            using (AndroidJavaClass _utilClass = new AndroidJavaClass ("nov9.gcm.Util")) 
            {
                _utilClass.SetStatic ("notificationsEnabled",false);
            }
        }
#endif

    }

    public void UnRegister()
    {
#if UNITY_IPHONE
        UnityEngine.iOS.NotificationServices.UnregisterForRemoteNotifications();
#elif UNITY_ANDROID
        if( Application.platform == RuntimePlatform.Android )
        {
            using (AndroidJavaClass cls = new AndroidJavaClass ("nov9.gcm.UnityGCMRegister"))
            {
                cls.CallStatic ("unregister");
            }
        }
#endif
    }

    public void OnError( string errorId) 
    {
        Completed = true;
        Debug.Log( "RemoteNotification.remoteRegistrationFailed : " + errorId );
    }

    public void OnMessage(string message)
    {
        Debugger.Log( "RemoteNotification.remoteNotificationReceived :"+message);

#if UNITY_IPHONE
        //{"aps":{"alert":"message","badge":1,"sound":"default"}}

#elif UNITY_ANDROID
        
        //Debug.Log( "RemoteNotification.onMessage :"+ message);

        LitJson.JsonData kJsonData = LitJson.JsonMapper.ToObject(message);

        if (kJsonData.Keys.Contains("google.message_id") && kJsonData.Keys.Contains("expire"))
        {
            System.DateTime expireTime = System.DateTime.ParseExact(kJsonData["expire"].ToString(), "yyyy-MM-dd HH:mm:ss", null);
            LGameData lGameData = LGameData.GetInstance();
            if (expireTime > lGameData.GetCurrentServerTime())
            {
                lGameData.SetPushData(kJsonData["google.message_id"].ToString(),
                    int.Parse(kJsonData["pushType"].ToString()),
                    int.Parse(kJsonData[string.Format("reward0")].ToString()),
                    int.Parse(kJsonData[string.Format("reward1")].ToString()),
                    int.Parse(kJsonData[string.Format("reward2")].ToString()),
                    int.Parse(kJsonData[string.Format("reward0Num")].ToString()),
                    int.Parse(kJsonData[string.Format("reward1Num")].ToString()),
                    int.Parse(kJsonData[string.Format("reward2Num")].ToString())
                );

                if (LManager.GetInstance().GetConnectedServerType() == LManager.E_CONNECTED_SERVER_TYPE.E_CONNECTED_SERVER_TYPE_GAME)
                {
                    System.Byte[] packet = LPuzzlePacket.REQ_PUSH_REWARD();
                    if(packet != null)
                        LManager.GetInstance().Network_SendPacket(packet);   
                }
            }
        }
#endif
    }
        
    public void OnRegistered(string registrationId) 
    {
        Debug.Log( "RemoteNotification.onRegistered :"+registrationId);
        deviceToken = registrationId;
        Completed = true;
    }

    public void OnUnregistered(string registrationId) 
    {
        Completed = true;
        Debug.Log( "RemoteNotification.onUnregistered :"+registrationId);
    }

    public void OnDeleteMessages (string message)
    {
        Debug.Log( "RemoteNotification.onDeletedMessages :"+message);
    }
       
}

#endif //USE_RemoteNotification
