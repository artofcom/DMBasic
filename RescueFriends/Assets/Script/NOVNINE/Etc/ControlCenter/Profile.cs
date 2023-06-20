using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NOVNINE;
using NOVNINE.Diagnostics;
using JsonFx.Json;
using System.IO;

namespace NOVNINE
{
	public static class Profile
	{
	    //SystemWide, Test, Debug, For Internal Use Only
	    public const int SUPERUSER      = 0x40000000; //미정
	    public const int LOG_LOCAL      = 0x20000000; //Debugger.LogXXX 함수를 출력
	    public const int LOG_REMOTE     = 0x10000000; //Debugger.LogXXX 함수를 DART로 전송
	    public const int ENABLE_CHEAT   = 0x08000000; //미정
	    public const int AD_TEST        = 0x04000000; //DUMMY_AD일때 SystemAlert으로 전면광고 확인가능
	    public const int DUMMY_IAP      = 0x02000000; //실구매 호출없음
	    public const int DUMMY_AD       = 0x01000000; //실광고 호출없음
	    public const int ECHO_INPUT     = 0x00800000; //터치입력 ECHO
	    public const int BIG_MOUTH      = 0x00400000; //BigMouth
	
	    //UserBehaviour
	    public const int IAP_WHALE      = 0x4000; //고래유저
	    public const int IAP_CRACKER    = 0x2000; //IAP, 지갑 크랙 유저
	    public const int AD_FREQ        = 0x0F00; //AdManager 광고주기 배율, 낮을수록 자주뜸, 4bit, fval = ival/4
	
	    //FVAL
	    public const int F_ONE      = 0x4;//4bit, fval = ival/4
	    public const int F_TWO      = 0x8;//4bit, fval = ival/4
	    public const int F_THREE    = 0xC;//4bit, fval = ival/4
	    public const int F_HALF     = 0x2;//4bit, fval = ival/4
	    public const int F_QUARTER  = 0x1;//4bit, fval = ival/4
	
	    static int userLv = 0;
	
	    static Profile() {
#if UNITY_ANDROID && !UNITY_EDITOR
	        int localUserLv = GetLocalUserLv();
	        if(localUserLv != 0) {
	            SetUserLv(localUserLv);
	            userLv = localUserLv;
	        }
#endif
	        if(userLv == 0) {
	            int v = PlayerPrefs.GetInt("Profile.userLv", 0);
	            if(v>=0 && v<(int)UserLevel.Max)
	                v = MigrateUserLevel(v);
	            SetUserLv(v);
	        }
	
	        CheckCurrentVersion();
	    }
	
	    public static int UserLv {
	        get {
	            return userLv;
	        }
	    }
	
	    static float GetFloat4Bit(int iv) {
	        return iv/4.0f;
	    }
	
	    public static bool IsSuperUser {
	        get { return (UserLv & SUPERUSER) != 0; }
	    }
	
	    public static bool IsLogLocal {
	        get { return (UserLv & LOG_LOCAL) != 0; }
	    }
	
	    public static bool IsLogRemote {
	        get { return (UserLv & LOG_REMOTE) != 0; }
	    }
	
	    public static bool IsEnableCheat {
	        get { return (UserLv & ENABLE_CHEAT) != 0; }
	    }
	
	    public static bool IsAdTest {
	        get { return (UserLv & AD_TEST) != 0; }
	    }
	
	    public static bool IsDummyIAP{
	        get { return (UserLv & DUMMY_IAP) != 0; }
	    }
	
	    public static bool IsDummyAD{
	        get { return (UserLv & DUMMY_AD) != 0; }
	    }
	
	    public static bool IsEchoInput{
	        get { return (UserLv & ECHO_INPUT) != 0; }
	    }
	
	    public static bool IsBigMouth{
	        get { return (UserLv & BIG_MOUTH) != 0; }
	    }
	
	    public static bool IsIapWhale{
	        get { return (UserLv & IAP_WHALE) != 0; }
	    }
	
	    public static bool IsIapCracker{
	        get { return (UserLv & IAP_CRACKER) != 0; }
	    }
	
	    public static float AdFrequency {
	        get { return GetFloat4Bit((UserLv & AD_FREQ)>>8); }
	    }
	
		static string _UUID;
		static string _ADID;
		static DateTime installTime = DateTime.MinValue;
		static string installVersion;
	
		public static string UUID {
	        get {
	            if(_UUID == null) 
	                _UUID = NOVNINE.Encryptor.EncodeWithKey (SystemInfo.deviceUniqueIdentifier, "DiscoMonkey");
	            return _UUID;
	        }
	    }
	
		public static string ADID {
	        get {
	            	if(_ADID == null) 
					{
						_ADID = NOVNINE.Encryptor.EncodeWithKey (NativeInterface.AdvertiserIdentifier(), "DiscoMonkey");
	                	if(string.IsNullOrEmpty(_ADID)) 
						{
	                    	Debugger.LogWarning("Profile.ADID not found using UUID");
	                    	_ADID = UUID;
	                	}
	            	}
	            	return _ADID;
	        	}
	    	}
	
	    public static string InstallVersion {
	        get {
	            if(installVersion == null) {
	                if (PlayerPrefs.HasKey ("InstallVersion")) {
	                    installVersion = PlayerPrefs.GetString ("InstallVersion");
	                } else {
	                    InstallVersion = NOVNINE.NativeInterface.GetBundleVersion();
	                }
	            }
	            return installVersion;
	        }
	        set {
	            installVersion = value;
	            PlayerPrefs.SetString("InstallVersion", installVersion);
	            PlayerPrefs.Save();
	        }
	    }
	
        public static string CurrentVersion()
        {
            return NOVNINE.NativeInterface.GetBundleVersion();
        }

	    static bool isVersionChanged = false;
	    
	    public static bool IsVersionChanged 
		{
	        get { return isVersionChanged;} 
	    }
	
	    public static void CheckCurrentVersion() 
		{
	        string oldVersion = PlayerPrefs.GetString("CurrentVersion", InstallVersion);
	        string currentVersion = NOVNINE.NativeInterface.GetBundleVersion();
	        if(oldVersion != currentVersion) 
			{
	            Debug.Log("Profile.OnAppVersionChanged "+oldVersion+" => "+currentVersion);
	            Platform.BroadcastMessage("OnAppVersionChanged", new string[]{oldVersion, currentVersion});
	            isVersionChanged = true;
	        }
	        PlayerPrefs.SetString("CurrentVersion", currentVersion);
	    }
	
	    public static DateTime InstallTime {
	        get {
	            if(installTime == DateTime.MinValue)
				{
	                var strTime = PlayerPrefs.GetString ("InstallTime", "");
	                if(strTime == "") 
					{
	                    InstallTime = DateTime.UtcNow;
	                }
					else
					{
	                    strTime = WWW.UnEscapeURL(strTime);
	                    if (!DateTime.TryParseExact(strTime, "yyyy-MM-dd HH:mm:ss GMT", CultureInfo.InvariantCulture, DateTimeStyles.None, out installTime)) 
						{
	                        Debug.LogError("Profile.InstallTime parse Error : "+strTime);
	                        InstallTime = DateTime.UtcNow;
	                    }
	                }
	            }
	            return installTime;
	        }
	        set {
	            installTime = value;
	            Debugger.Log("Profile.InstallTime is set to "+installTime.ToString());
	            var strTime = InstallTimeStr;
	            PlayerPrefs.SetString ("InstallTime", strTime);
	            PlayerPrefs.Save();
	        }
	    }
	
	    public static string InstallTimeStr {
	        get {
	            return WWW.EscapeURL(InstallTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")+" GMT");
	        }
	    }
	
	    public static void SetUserLv(int lv) {
	        if(lv == (int)userLv) return;
	        Debugger.Assert(lv >= 0, "Profile.SetUserLv UserLevel can't be minus");
	        PlayerPrefs.SetInt("Profile.userLv", lv);
	        userLv = lv;
	
	        Debugger.EnableLocalLog = IsLogLocal;
	        Debugger.EnableRemoteLog = IsLogRemote;
	        Debugger.Log("Profile.SetUserLv: "+lv+"  "+ToString());
	
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
	        //GooglePlayGames.OurUtils.Logger.DebugLogEnabled = IsLogLocal;
#endif
	    }
	
#if UNITY_ANDROID && !UNITY_EDITOR
	    private static int GetLocalUserLv() {
	        int result = 0;
	
	        using(AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
	            using(AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
	                using(AndroidJavaObject pm = jo.Call<AndroidJavaObject>("getPackageManager")) {
	                    string packageName = jo.Call<string>("getPackageName");
	                    int GET_META_DATA = 128;
	                    try {
	                        using(AndroidJavaObject ai = pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, GET_META_DATA)) {
	                                AndroidJavaObject aBundle = ai.Get<AndroidJavaObject>("metaData");
	                                result = aBundle.Call<int>("getInt", "userLevel");
	                        }
	                    }catch (Exception e) {
	                        Debug.LogWarning("Platform : GetUserLevel Fail"); 
	                    }
	                    return result;
	                }
	            }
	        }
	    }
#endif
	    public new static string ToString() {
	        return "SU("+IsSuperUser+") LL("+IsLogLocal+") LR("+IsLogRemote+") EC("+IsEnableCheat+") AT("+IsAdTest+") DI("+IsDummyIAP+") DA("+IsDummyAD+") EI("+IsEchoInput+") IW("+IsIapWhale+") IC("+IsIapCracker+") FQ("+AdFrequency+")";
	    }
	
	    public enum UserLevel {Normal, BigMouth, Whale, Developer, TesterLocal, TesterRemote, Max, None = -1}
	    public static int MigrateUserLevel(int lv) {
	        UserLevel olv = (UserLevel)lv;
	        int nlv = F_ONE<<8;
	        switch(olv)
	        {
	             case UserLevel.Normal:
	                 break;
	             case UserLevel.BigMouth :
	                 nlv &= ~AD_FREQ;
	                 nlv |= 0xf<<8;// 1/3.75 less ads
	                 break;
	             case UserLevel.Whale :
	                 nlv |= IAP_WHALE;
	                 break;
	             case UserLevel.Developer :
	                 nlv |= SUPERUSER | LOG_LOCAL | ENABLE_CHEAT;
	                 nlv &= ~AD_FREQ;
	                 nlv |= F_QUARTER<<8;//x4 much ads
	                 break;
	             case UserLevel.TesterLocal :
	                 nlv |= LOG_LOCAL;
	                 break;
	             case UserLevel.TesterRemote :
	                 nlv |= LOG_REMOTE;
	                 break;
	             default :
	                 Debugger.Assert(false, "Profile.SetUserLv userLv out of range : "+lv);
	                 break;
	        }
	        Debugger.Log("Profile.MigrateUserLevel : "+olv.ToString()+" -> "+nlv);
	        return nlv;
	     }

        public static IEnumerator CoSendTrackingLog(int stepID)
        {
            yield break;
        }
	}
}
