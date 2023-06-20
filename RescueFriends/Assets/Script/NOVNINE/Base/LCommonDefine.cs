using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using JsonFx.Json;

enum E_PUBLISHER_NUMBER : byte
{
    E_PUBLISHER_NUMBER_NONE,
    E_PUBLISHER_NUMBER_TRITONESOFT,
	//E_PUBLISHER_NUMBER_NOV9,
}

enum E_DEVICETYPE : byte
{
    E_DEVICETYPE_NONE,

    E_DEVICETYPE_ANDROID,
    E_DEVICETYPE_IOS,
    E_DEVICETYPE_WINDOWS,
    E_DEVICETYPE_SAMSUNGTV,

    E_DEVICETYPE_END
};

enum E_MARKET : byte
{
    E_MARKET_NONE,

    E_MARKET_GOOGLE,
    E_MARKET_APPLE,
    E_MARKET_NAVER,
    E_MARKET_TSTORE,
    E_MARKET_SAMSUNG,
    E_MARKET_WINDOWS,
    E_MARKET_FACEBOOK,

    E_MARKET_END
};

enum E_ACCOUNT_LOGIN_TYPE : byte
{
    E_ACCOUNT_LOGIN_TYPE_NONE,

    E_ACCOUNT_LOGIN_TYPE_GUEST = 1,
    //E_ACCOUNT_LOGIN_TYPE_IDPW	= 2,
    E_ACCOUNT_LOGIN_TYPE_SNS = 3,       // DB 에서 3으로 작업했음.

    E_ACCOUNT_LOGIN_TYPE_END
};


enum E_SNS_TYPE : byte
{
    E_SNS_TYPE_NONE,

    E_SNS_TYPE_KAKAO = 1,
    E_SNS_TYPE_FACEBOOK = 2,
    E_SNS_TYPE_TWITTER = 3,
    E_SNS_TYPE_GOOGLE = 4,
    E_SNS_TYPE_NAVER = 5,
    E_SNS_TYPE_SAMSUNG = 6,
    E_SNS_TYPE_GOOGLE_PLAY_GAME_SERVICE = 7,

    E_SNS_TYPE_END,
};

enum E_PUSH_TYPE : byte
{
    E_PUSH_TYPE_NONE,

    E_PUSH_TYPE_NORMAL,
    E_PUSH_TYPE_FRIEND,
    E_PUSH_TYPE_REWARD,
    E_PUSH_TYPE_REWARD_3,
    E_PUSH_TYPE_REWARD_7,

    E_SNS_TYPE_END,
};

enum E_AUTO_EVENT_TYPE : byte
{
    E_AUTO_EVENT_NONE,
    E_AUTO_EVENT_BLOCK_COLLECT,
    E_AUTO_EVENT_BLOCK_CREATE,
    E_AUTO_EVENT_LEVEL_ARRIVAL,

    E_AUTO_EVENT_END
};


enum E_GAMETYPE : short
{
    E_GAMETYPE_NONE,

    E_GAMETYPE_HEROESWILL,
    E_GAMETYPE_HEROESWILL_TAB,

    E_GAMETYPE_CASINO,
	E_GAMETYPE_PUZZLE,
	
    E_GAMETYPE_END
};

enum E_ITEMTYPE : short
{
	E_ITEMTYPE_BOOST,
	E_ITEMTYPE_NORMAL,
	E_ITEMTYPE_END
};


enum E_POINT_SOURCE : byte
{
    E_POINT_SOURCE_NONE,

    E_POINT_SOURCE_COINSHOP =1,
    E_POINT_SOURCE_ONCE,
    E_POINT_SOURCE_SUPERSALE,
    E_POINT_SOURCE_DAILYBONUS,
    E_POINT_SOURCE_TREASUREBOX,
    E_POINT_SOURCE_CONTINUE,
    E_POINT_SOURCE_INGAMESALE,
    E_POINT_SOURCE_LEVELCLEAR,
    E_POINT_SOURCE_SYSTEM_REWARD = 10,
    E_POINT_SOURCE_FRIEND_MESSAGE,
    E_POINT_SOURCE_RECIPE,
    E_POINT_SOURCE_AUTOEVENTCOLLECT_REWARD,
    E_POINT_SOURCE_AUTOEVENTARRIVE_REWARD,
    E_POINT_SOURCE_SPECIALMAP_REWARD,
    E_POINT_SOURCE_ACHIEVEMENT_REWARD,
    E_POINT_SOURCE_TOURNAMENT_REWARD,
    E_POINT_SOURCE_TESTMAP_REWARD,
    E_POINT_SOURCE_FACEBOOK_INVITE_REWARD,

    E_POINT_SOURCE_INGAME_USE = 100,
    E_POINT_SOURCE_STAMINA_SALE,
    E_POINT_SOURCE_BOOSTER_SALE,
    E_POINT_SOURCE_LEVEL_ENTER,
    E_POINT_SOURCE_GAME_PLAY,
    E_POINT_SOURCE_OUT_OF_MOVE,

    E_POINT_SOURCE_END
};

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public static class LCommonDefine
{
    public const string GOOGLESTORKEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmHYwmFXv9Z99HtGg7qzkl5IyZwZIAMnOxXkvGX+74sOURqqV8ZEnLZmtgZkBeIJR1+j158+AU0RhBxn0RXvJL/imaU3w6C1YQaPAz0PNzXqFGEHecihPWzt4l9xkF6pBzN5/LxFW6djNo6kY/SKi6xKN2UP65FNerdmcvbiPQIEeAdVZ/C2dAEghlFAxbS06EpGijwnlwKSPK3E5re+en9wE9eZFuOn9MYqdg6zp/mTkiirHDF7UVZhw5PTQdkiuarzUXO0WER+NIL6+ZxI0fNHQ3mFLPEJslYxAeD0Ds0iyeHlntOZr1Eed6Lsl6FgWVtUX+hEOw56wIxgGo6cz+QIDAQAB";
    public const string APPLESTORKEY = "1255286554";

    //public const System.Int64 CURRENT_APP_VERSION = 17101714;
    //public const System.Int64 CURRENT_DATA_VERSION = 17101109;
	
	public const System.Int16 MAX_LPUZZLE_ITEM_NUM = 100;				// ???????? ???? 100 ?? ?????? ?? ????.
	public const System.Byte MAX_USE_ITEMKIND_NUM = 7;				// ???????? ???? 5?? ???????? ????????( ?????? 5????, ???? 5????, ???????? 5???? )

    public const System.Byte RECIVE_DAILY_GIFT_NUM = 30;
    public const System.Byte MIN_NICKNAME_BUFLEN = 2;
    public const System.Byte MAX_NICKNAME_BUFLEN = 32;

	public const System.Byte GUEST_ACCOUNT_ID_PW_BUFLEN = 10;
	public const System.Byte MAX_ACCOUNT_ENCODED_PW_BUFLEN = 64;		// Guest PW : 8 Byte bigint		Base64 Encoding 하면 15 byte 지만, 
	public const System.UInt16 MAX_SNS_UID_BUFLEN = 64;
	public const System.UInt16 MAX_ACCESSTOKEN_BUFLEN = 350;
	
	public static byte CURRENT_APP_MARKET = (byte)E_MARKET.E_MARKET_NONE;
    public static byte CURRENT_DEVICE_TYPE = (byte)E_DEVICETYPE.E_DEVICETYPE_NONE;
    public static string CURRENT_DEVICE_COUNTRY = "NULL";

	public const byte SEND_GAME_DATALOAD_MAX_FRIEND_LIST_NUM = 50;
	public const byte SEND_GAME_RESULT_MAX_GAMERESULTDATA_LIST_NUM = 50;
    public const byte SEND_DAILY_GIFT_NUM = 50;
    public const byte ASK_DAILY_GIFT_NUM = 50;
    public const byte GAME_RESULT_MAX_SIZE      = 10;
    public const byte SEND_GIFT_PACKET_NUM = 10;
    public const byte ASK_GIFT_PACKET_NUM = 10;
    public const byte DELETE_GIFT_PACKET_NUM = 10;
    public const byte RECIVE_GIFT_PACKET_NUM = 10;

    public enum TRACKING_STEP
    {
        TRACKING_NONE,
        TRACKING_SPLASH_SCENE,
        TRACKING_LOADING_SCENE_START,
        TRACKING_LOADING_SCENE_SCENELOAD_START,
        TRACKING_LOADING_SCENE_SCENELOAD_END,
        TRACKING_LOADING_SCENE_RES_VERSIONCHECK = 5,
        TRACKING_LOADING_SCENE_RES_AUTH_ACCOUNT_LOGIN,
        TRACKING_LOADING_SCENE_PATCH_START,
        TRACKING_LOADING_SCENE_PATCH_END,
        TRACKING_LOADING_SCENE_RES_GAMESERVER_LIST,
        TRACKING_LOADING_SCENE_RES_AUTH_ACCOUNT_MIGRATION_SNS = 10,
        TRACKING_LOADING_SCENRES_RES_GAME_LOGIN,
        TRACKING_LOADING_SCENRES_RES_PLAYER_CREATE,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_MAPCLEARED_BEGIN,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_MAPCLEARED_END,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_ITEM_BEGIN,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_ITEM_END,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_FRIEND_LIST_BEGIN,
        TRACKING_LOADING_SCENRES_RES_GAME_DATALOAD_FRIEND_LIST_END,
        TRACKING_LOADING_SCENE_RES_GAME_DATALOAD_END = 20,
        TRACKING_LOADING_SCENE_ENTER_GAME,
        TRACKING_LOADING_SCENE_RES_AUTH_ACCOUNT_LOGIN_FAIL,

    }

	public struct ST_GoogleStore_CashItem_PurchaseData
	{
		string	strSignedData;
		string	strSignature;
		
		public void Init()
		{
			strSignedData = null;
			strSignature = null;
		}
		
		public void Set(string pSignedData, string pSignature)
		{
			strSignedData = pSignedData;
			strSignature = pSignature;
		}
		
		public void Set(LitJson.JsonData kJsonData)
		{
            string pSignedData = kJsonData["strSignedData"].ToString();
            string pSignature = kJsonData["strSignature"].ToString();

			if (null == pSignedData || null == pSignature) return;
			
			strSignedData = pSignedData;
			strSignature = pSignature;
		}

		public string GetSignedData()
		{
			return strSignedData;
		}
		
		public string GetSignature()
		{
			return strSignature;
		}
		
        public LitJson.JsonData MakeJsonData()
		{
			LitJson.JsonData kJsonData = new LitJson.JsonData();
			kJsonData["strSignedData"] = strSignedData;
			kJsonData["strSignature"] = strSignature;
			return kJsonData;
		}
	};

	public struct ST_SamsungStore_CashItem_PurchaseData
	{
		string	strInvoiceID;
		string	strCustomID;
		string	strCountryCode;
		System.Byte     ucIsSbox;
		
		public void Init()
		{
			strInvoiceID = null;
			strCustomID = null;
			strCountryCode = null;
		}

		public void Set(string pInvoieID, string pCustomID, string pCountryCode, System.Byte isSbox)
		{
			strInvoiceID = pInvoieID;
			strCustomID = pCustomID;
			strCountryCode = pCountryCode;
			ucIsSbox = isSbox;
		}
		
		public string GetInvoiceID()
		{
			return strInvoiceID;
		}
		
		public string GetCustomID()
		{
			return strCustomID;
		}
		
		public string GetCountryCode()
		{
			return strCountryCode;
		}
		public System.Byte IsSBox()
		{
			return ucIsSbox;
		}
		
		public LitJson.JsonData MakeJsonData()
		{
			LitJson.JsonData kJsonData = new LitJson.JsonData();
			kJsonData["strInvoiceID"] = strInvoiceID;
			kJsonData["strCustomID"] = strCustomID;
			kJsonData["strCountryCode"] = strCountryCode;
			kJsonData["ucIsSbox"] = ucIsSbox;
			return kJsonData;
		}
	};

	public struct ST_NaverAppStore_CashItem_PurchaseData
	{
		string	strPaymentSeq;
		string	strPayload;

		public void Init()
		{
			strPaymentSeq = null;
			strPayload = null;
		}
		
		public void Set(string pPaymentSeq, string pPayload)
		{
			strPaymentSeq = pPaymentSeq;
			strPayload = pPayload;
		}

		public string GetPaymentSeq()
		{
			return strPaymentSeq;
		}

		public string GetPayload()
		{
			return strPayload;
		}
		
		public LitJson.JsonData MakeJsonData()
		{
			LitJson.JsonData kJsonData = new LitJson.JsonData();
			kJsonData["strPaymentSeq"] = strPaymentSeq;
			kJsonData["strPayload"] = strPayload;
			return kJsonData;
		}
	}

	public  struct ST_TStore_CashItem_PurchaseData
	{
		string strTXID;
		string strSignData;
		public void Init()
		{
			strTXID = null;
			strSignData = null;
		}
		
		public void Set(string pTxID, string pSignData)
		{
			strTXID = pTxID;
			strSignData = pSignData;
		}

		public  string GetTXID()
		{
			return strTXID;
		}
		
		public LitJson.JsonData MakeJsonData()
		{
			LitJson.JsonData kJsonData = new LitJson.JsonData();
			kJsonData["strTXID"] = strTXID;
			kJsonData["strSignData"] = strSignData; 
			return kJsonData;
		}
	};

	public struct ST_AppleStore_CashItem_PurchaseData
	{
		string	strBase64EncoededReceiptData;

		public void Init()
		{
			strBase64EncoededReceiptData = null;
		}
		
		public void Set(string pBase64EncodedReceiptData)
		{
			strBase64EncoededReceiptData = pBase64EncodedReceiptData;
		}
		
		public string GetBase64EncodedReceiptData()
		{
			return strBase64EncoededReceiptData;
		}
		
	    public LitJson.JsonData MakeJsonData()
		{
			LitJson.JsonData kJsonData = new LitJson.JsonData();
			kJsonData["strBase64EncoededReceiptData"] = strBase64EncoededReceiptData;
			return kJsonData;
		}
	}
	
	public struct ST_CashItem_PurchaseData
	{
		ST_GoogleStore_CashItem_PurchaseData	stGoogleStore_CashItem_PurchaseData;
		ST_AppleStore_CashItem_PurchaseData		stAppleStore_CashItem_PurchaseData;
		ST_NaverAppStore_CashItem_PurchaseData	stNaverAppStore_CashItem_PurchaseData;
		ST_TStore_CashItem_PurchaseData			stTstore_CashItem_PurchaseData;
		ST_SamsungStore_CashItem_PurchaseData	stSamsungStore_CashItem_PurchaseData;

		public void SetGoogleStore_CashItem_PurchaseData(ST_GoogleStore_CashItem_PurchaseData googleStore_CashItem_PurchaseData) 
		{
			stGoogleStore_CashItem_PurchaseData = googleStore_CashItem_PurchaseData;
		}
		
		public ST_GoogleStore_CashItem_PurchaseData GetGoogleStore_CashItem_PurchaseData()
		{
			return stGoogleStore_CashItem_PurchaseData;
		}
		
		public void SetAppleStore_CashItem_PurchaseData(ST_AppleStore_CashItem_PurchaseData appleStore_CashItem_PurchaseData) 
		{
			stAppleStore_CashItem_PurchaseData = appleStore_CashItem_PurchaseData;
		}
		
		public ST_AppleStore_CashItem_PurchaseData GetAppleStore_CashItem_PurchaseData() 
		{
			return stAppleStore_CashItem_PurchaseData;
		}
		
		public void SetNaverAppStore_CashItem_PurchaseData(ST_NaverAppStore_CashItem_PurchaseData naverAppStore_CashItem_PurchaseData) 
		{
			stNaverAppStore_CashItem_PurchaseData = naverAppStore_CashItem_PurchaseData; 
		}
		
		public ST_NaverAppStore_CashItem_PurchaseData GetNaverAppStore_CashItem_PurchaseData()
		{
			return stNaverAppStore_CashItem_PurchaseData;
		}
		
		public void SetTStore_CashItem_PurchaseData(ST_TStore_CashItem_PurchaseData tstore_CashItem_PurchaseData)
		{
			stTstore_CashItem_PurchaseData = tstore_CashItem_PurchaseData;
		}

		public ST_TStore_CashItem_PurchaseData GetTStore_CashItem_PurchaseData() 
		{
			return stTstore_CashItem_PurchaseData; 
		}

		public void SetSamsungStore_CashItem_PurchaseData(ST_SamsungStore_CashItem_PurchaseData samsungStore_CashItem_PurchaseData)
		{
			stSamsungStore_CashItem_PurchaseData = samsungStore_CashItem_PurchaseData;
		}

		public ST_SamsungStore_CashItem_PurchaseData GetSamsungStore_CashItem_PurchaseData()
		{
			return stSamsungStore_CashItem_PurchaseData; 
		}
	}

}