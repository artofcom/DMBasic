using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using AssetBundles;
using NOVNINE.Diagnostics;

// JUST Testor !!!
public static class _WAYNE_TESTOR
{
#if UNITY_EDITOR
	public static bool _SHOW_ALWAYS_DAILY_REWARD    = false;// false;// 일일보상 팝업이 언제나 뜬다.
//	public static int _DAILY_REWARD_DAY             = 3;    // 일일보상 팝업 target day.
	public static bool _CAN_RECIPE_ALL              = false;// 조건이 안되도 무조건 재료조합 가능하다.
	public static bool _ALWAYS_DROP_INGREDIENT      = false; // 조건이 안되도 map clear 시 재료 드랍된다.
    public static bool _UNLOCK_BOOSTER              = false;//true;
    public static int _CNT_MIXED_BOOSTER            = 0;    // 테스트 booster 수
    public static int _CNT_TRIPLE_BOOSTER           = 0;    // 테스트 booster 수
    public static int _CNT_SPECIAL_BOOSTER          = 0;    // 테스트 booster 수
    public static bool _UNLOCK_ITEM                 = false;//true;
    public static int _CNT_ITEM_HAMMER              = 0;    // 테스트 item 수
    public static int _CNT_ITEM_FIRECRACKER         = 0;    // 테스트 item 수
    public static int _CNT_ITEM_MAGICSWAP           = 0;    // 테스트 item 수
    public static int _CNT_ITEM_RAINBOWBUST         = 0;    // 테스트 item 수    
    public static int _TEST_AUTO_EVENT_ID           = 9;     // 테스트 auto event id - default 0 !!!
    public static int _TEST_AUTO_EVENT_BLK_COUNT    = 0;    // 테스트 auto event block count - default 0 !!!
    public static int _TEST_AUTO_EVENT_GOAL_LV_ID   = 18;   // 테스트 auto event 목표 level id.
#else
	// Defaults !!! Must released under these settings.
	public static bool _SHOW_ALWAYS_DAILY_REWARD    = false;
//	public static int _DAILY_REWARD_DAY             = 0;
	public static bool _CAN_RECIPE_ALL              = false;
	public static bool _ALWAYS_DROP_INGREDIENT      = false;
    public static bool _UNLOCK_BOOSTER              = false;
    public static int _CNT_MIXED_BOOSTER            = 0;    // 테스트 booster 수
    public static int _CNT_TRIPLE_BOOSTER           = 0;    // 테스트 booster 수
    public static int _CNT_SPECIAL_BOOSTER          = 0;    // 테스트 booster 수
    public static bool _UNLOCK_ITEM                 = false;
    public static int _CNT_ITEM_HAMMER              = 0;    // 테스트 item 수
    public static int _CNT_ITEM_FIRECRACKER         = 0;    // 테스트 item 수
    public static int _CNT_ITEM_MAGICSWAP           = 0;    // 테스트 item 수
    public static int _CNT_ITEM_RAINBOWBUST         = 0;    // 테스트 item 수
    public static int _TEST_AUTO_EVENT_ID           = 0;     // 테스트 auto event id - default 0 !!!
    public static int _TEST_AUTO_EVENT_BLK_COUNT    = 0;    // 테스트 auto event block count - default 0 !!!
    public static int _TEST_AUTO_EVENT_GOAL_LV_ID   = 0;    // 테스트 auto event 목표 level id.
#endif
};
//

public struct _ADS_INFO
{
    public string               strRewardName;
    public int                  count;
    public int                  cool_time_min;
};

public struct _DAILY_REWARD_INFO
{
	public string               strRewardName;
	public int                  count;
};

public struct _RESCUE_REWARD_INFO
{
	public int                  id, nMapId0, nMapId1, nMapId2;
	public string               strItemName;
	public int                  count;

    public void set(int info_id, int id0, int id1, int id2, string item, int ncount)
    {
        id                      = info_id;
        nMapId0                 = id0;
        nMapId1                 = id1;
        nMapId2                 = id2;
        strItemName             = item;
        count                   = ncount;
    }
};

public struct _WHEEL_GACHA_ITEM_INFO
{
    public string               strItemId;
    public int                  count;
};

public struct _PLAY_ON_INFO
{
	public int                  moveCount, addTime;
	public string               strBooster;
    public int                  boosterNum;
};
    
public struct _INVITE_REWARD_INFO
{
    public int RewardInviteCount;
    public int[]                  RewardNum;
    public string[]               RewardName;
};

public struct _ITEM_SHOP_DATA
{
    public static int MAX_ITEM  = 3;
    public string _strId;
    public int[] arrCount;
    public int[] arrPrice;
    public int[] arrSale;
};

public static class InfoLoader {

    static public int MAX_FOX_BUFF  = 10;

	static List<_DAILY_REWARD_INFO> sDailyRewardData = new List<_DAILY_REWARD_INFO>();
	static List<_RESCUE_REWARD_INFO> sRescueRewardData = new List<_RESCUE_REWARD_INFO>();
    static List<_WHEEL_GACHA_ITEM_INFO> sWheelGachaItemData = new List<_WHEEL_GACHA_ITEM_INFO>();
	static int _price_wheelGacha    = 0;
    static int _min_time_freeGacha  = 0;

	static List<_PLAY_ON_INFO> sDefaultPlayOnData = new List<_PLAY_ON_INFO>();
	static Dictionary<string,int> sCoinInfo = new Dictionary<string,int>();
    
    static List<_INVITE_REWARD_INFO> sInviteRewardInfoList = new List<_INVITE_REWARD_INFO>();
    static List<_ITEM_SHOP_DATA> sItemShopInfoList = new List<_ITEM_SHOP_DATA>();

    //static int _life_refill_time_min= 0;
    static _ADS_INFO sAdsInfo;

    static void InitDataForNoGameServer()
    {
        TextAsset textXML = null;
        
        textXML = Resources.Load("data/dailyRewardData", typeof(TextAsset)) as TextAsset;
        _loadDailyRewardData(textXML);
        Debug.Log("=== reading daily reward infos...size ; " + sDailyRewardData.Count);
        
        //textXML = Resources.Load("data/ItemIDTable", typeof(TextAsset)) as TextAsset;
        //_loadItemIDTable(textXML);
        //Debug.Log("=== reading item id table infos...size ; " + s);
        
        textXML = Resources.Load("data/rescueReward", typeof(TextAsset)) as TextAsset;
        _loadRescueReward(textXML);
        Debug.Log("=== reading rescue reward infos...size ; " + sRescueRewardData.Count);
        
        textXML = Resources.Load("data/GameInfo", typeof(TextAsset)) as TextAsset;
        _loadGameInfo(textXML);
        Debug.Log("=== reading game infos...size ; " + sCoinInfo.Count);

        //textXML = Resources.Load("data/rubyShopItemInfo", typeof(TextAsset)) as TextAsset;
        //if(textXML != null)
        //    TaskManager.StartCoroutine(LoadRubyShopItmeInfo(textXML.text));
        
        textXML = Resources.Load("data/wheelGacha", typeof(TextAsset)) as TextAsset;
        _loadWheelGachaItem(textXML);
        Debug.Log("=== reading wheel gacha infos...size ; " + sWheelGachaItemData.Count);
        
        textXML = Resources.Load("data/itemShopData", typeof(TextAsset)) as TextAsset;
        _loadItemShopData(textXML);
        Debug.Log("=== reading item shop data infos.. size ; " + sItemShopInfoList.Count);
    }

	// Use this for initialization
	//public static void init() 
	public static IEnumerator init()
	{
        // init Needs Only 1 time !!!
        if(sDailyRewardData.Count > 0)
            yield break;

		//Debug.Assert(Director.PatchComplete, " not Director.PatchComplete!");
		
        sDailyRewardData.Clear();

        //#if UNITY_EDITOR
        //AssetBundleManager.SimulateAssetBundleInEditor = true;
        //#else
        InitDataForNoGameServer();
        yield break;
        //#endif

     /*   if(AssetBundleManager.GetInstance() == null)
		{
            //AssetBundleManager.SetSourceAssetBundleURL(LGameData.GetInstance().GetPathURL(), LGameData.GetDataVersion());
			AssetBundleLoadManifestOperation operation = AssetBundleManager.Initialize();
			if(operation != null)
                yield return TaskManager.StartCoroutine(operation);
		}
		
		XmlDocument xmlDoc = new XmlDocument();
		
		AssetBundleLoadAssetOperation kAssetBundleLoadAssetOperation = null;
		//if(kAssetBundleLoadAssetOperation == null) yield break;
        //yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

		TextAsset textXML = null; // kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();

        ///_DAILY_REWARD_INFO
		/// 
		kAssetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync("patchdata", "dailyRewardData", typeof(TextAsset));
		if(kAssetBundleLoadAssetOperation == null) yield break;
        yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

		textXML = kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();
        _loadDailyRewardData(textXML);
		
        kAssetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync("patchdata", "ItemIDTable", typeof(TextAsset));
        if(kAssetBundleLoadAssetOperation == null) yield break;
        yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

        textXML = kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();
        _loadItemIDTable(textXML);

		kAssetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync("patchdata", "rescueReward", typeof(TextAsset));
		if(kAssetBundleLoadAssetOperation == null) yield break;
        yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

		textXML = kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();
        _loadRescueReward(textXML);
		
		kAssetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync("patchdata", "GameInfo", typeof(TextAsset));
		if(kAssetBundleLoadAssetOperation == null) yield break;
        yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

		textXML = kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();
        _loadGameInfo(textXML);
		
        kAssetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync("patchdata", "cashiteminfo", typeof(TextAsset));
        if(kAssetBundleLoadAssetOperation == null) yield break;
        yield return TaskManager.StartCoroutine(kAssetBundleLoadAssetOperation);

    //    textXML = kAssetBundleLoadAssetOperation.GetAsset<TextAsset>();
    //    if(textXML != null)
    //        yield return TaskManager.StartCoroutine(LoadRubyShopItmeInfo(textXML.text));

        //AssetBundleManager.UnloadAllAssetBundle(true);*/
    }

    static void _loadDailyRewardData(TextAsset textXML)
    {
        if(textXML == null)     return;

        XmlDocument xmlDoc      = new XmlDocument();
        xmlDoc.LoadXml(textXML.text);

        sDailyRewardData.Clear();
        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("item");
        for(int q = 0; q < listItems.Count; ++q)
        {
            _DAILY_REWARD_INFO oInfo   = new _DAILY_REWARD_INFO();

            XmlNode nodeTower   = listItems[q];
            XmlAttributeCollection attr = nodeTower.Attributes;
            for(int a = 0; a < attr.Count; ++a)
            {
                XmlAttribute attrData = attr[a];
                if(attrData.Name == "reward")       oInfo.strRewardName= attrData.InnerText;
                else if(attrData.Name == "count")   oInfo.count     = int.Parse( attrData.InnerText );
            }

            sDailyRewardData.Add( oInfo );
        }
    }

    //static void _loadItemIDTable(TextAsset textXML)
    //{
        /*if(textXML == null)     return;
        
        XmlDocument xmlDoc      = new XmlDocument();
        xmlDoc.LoadXml(textXML.text);

        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("item");
        for(int q = 0; q < listItems.Count; ++q)
        {
            XmlNode nodeTower   = listItems[q];
            XmlAttributeCollection attr = nodeTower.Attributes;
            if (attr["type"].InnerText == "2" || attr["type"].InnerText == "3")
            {
                sUsedItemUniqueNumberList.Add(int.Parse(attr["slUniqueNumber"].InnerText));
            }
        }*/
    //}

    static void _loadRescueReward(TextAsset textXML)
    {
        if(textXML == null)     return;
        
        XmlDocument xmlDoc      = new XmlDocument();
        xmlDoc.LoadXml(textXML.text);

        sRescueRewardData.Clear();
        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("item");
        for(int q = 0; q < listItems.Count; ++q)
        {
            _RESCUE_REWARD_INFO     oInfo   = new _RESCUE_REWARD_INFO();
            XmlNode nodeTower       = listItems[q];
            XmlAttributeCollection  attr = nodeTower.Attributes;
            int id                  = int.Parse(attr["id"].InnerText);
            int level0              = null==attr["level_0"] ? -1 : int.Parse(attr["level_0"].InnerText);
            int level1              = null==attr["level_1"] ? -1 : int.Parse(attr["level_1"].InnerText);
            int level2              = null==attr["level_2"] ? -1 : int.Parse(attr["level_2"].InnerText);

            oInfo.set(id, level0, level1, level2, attr["reward"].InnerText, int.Parse( attr["count"].InnerText));
            sRescueRewardData.Add( oInfo );
        }
    }

    static void _loadWheelGachaItem(TextAsset textXML)
    {
        if(textXML == null)     return;
        
        XmlDocument xmlDoc      = new XmlDocument();
        xmlDoc.LoadXml(textXML.text);

        sWheelGachaItemData.Clear();
        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("wheel_item");
        for(int q = 0; q < listItems.Count; ++q)
        {
            _WHEEL_GACHA_ITEM_INFO  oInfo   = new _WHEEL_GACHA_ITEM_INFO();
            XmlNode nodeTower       = listItems[q];
            XmlAttributeCollection  attr = nodeTower.Attributes;
            
            oInfo.count             = int.Parse( attr["count"].InnerText);
            oInfo.strItemId         = attr["id"].InnerText;

            sWheelGachaItemData.Add( oInfo );
        }

        listItems               = xmlDoc.GetElementsByTagName("price");
        _price_wheelGacha       = int.Parse( listItems[0].Attributes["rubby"].InnerText );
        _min_time_freeGacha     = int.Parse( listItems[0].Attributes["free_time_min"].InnerText );
    }

    static void _loadItemShopData(TextAsset textXML)
    {
        if(textXML == null)     return;
        
        XmlDocument xmlDoc      = new XmlDocument();
        xmlDoc.LoadXml(textXML.text);

        sItemShopInfoList.Clear();
        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("item");
        for(int q = 0; q < listItems.Count; ++q)
        {
            _ITEM_SHOP_DATA oInfo   = new _ITEM_SHOP_DATA();
            XmlNode nodeTower       = listItems[q];
            XmlAttributeCollection  attr = nodeTower.Attributes;
            
            oInfo.arrCount          = new int[_ITEM_SHOP_DATA.MAX_ITEM];
            oInfo.arrPrice          = new int[_ITEM_SHOP_DATA.MAX_ITEM];
            oInfo.arrSale           = new int[_ITEM_SHOP_DATA.MAX_ITEM];

            oInfo._strId            = attr["id"].InnerText;
            for(int zz=0; zz < _ITEM_SHOP_DATA.MAX_ITEM; ++zz)
            {
                oInfo.arrSale[zz]   = int.Parse( attr[ string.Format("sale_{0}", zz) ].InnerText);
                oInfo.arrCount[zz]  = int.Parse( attr[ string.Format("count_{0}", zz) ].InnerText);
                oInfo.arrPrice[zz]  = int.Parse( attr[ string.Format("price_{0}", zz) ].InnerText);
            }

            sItemShopInfoList.Add( oInfo );
        }
    }

    static void _loadGameInfo(TextAsset textXML)
    {
        if(textXML == null)     return;

        XmlDocument xmlDoc      = new XmlDocument();
        
        xmlDoc.LoadXml(textXML.text);

        XmlNodeList listItems   = xmlDoc.GetElementsByTagName("Root");
        if(listItems.Count > 0)
        {
            XmlNode nodeTower   = listItems[0];
            XmlAttributeCollection attr = nodeTower.Attributes;

            for(int a = 0; a < attr.Count; ++a)
            {
                XmlAttribute attrData = attr[a];
                //if(attrData.Name == "login")
                //    PlayerPrefs.SetString("strUseLogin", attrData.InnerText);
                //else if(attrData.Name == "localNotification")
               //     PlayerPrefs.SetInt("useLocalNotification", int.Parse(attrData.InnerText));
                //else if(attrData.Name == "pushNotification")
               //     PlayerPrefs.SetInt("usePushNotification", int.Parse(attrData.InnerText));
               // else if(attrData.Name == "inviteFirends")
               //     PlayerPrefs.SetInt("useInviteFirends", int.Parse(attrData.InnerText));
            }
        }
            
        sCoinInfo.Clear();
        listItems   = xmlDoc.GetElementsByTagName("coinInfo");
        if(listItems.Count > 0)
        {
            XmlNode nodeTower   = listItems[0];

            foreach(XmlAttribute itr in nodeTower.Attributes)
            {
                sCoinInfo.Add( itr.Name, int.Parse(itr.InnerText));
            }
        }

        listItems               = xmlDoc.GetElementsByTagName("basic_info");
        if(listItems.Count > 0)
        {
            XmlNode nodeBasic   = listItems[0];
            foreach(XmlAttribute itr in nodeBasic.Attributes)
            {
                //if(itr.Name.Equals("life_refill_time_min"))
                //    _life_refill_time_min = int.Parse(itr.InnerText);
            }
        }

        listItems               = xmlDoc.GetElementsByTagName("ads_info");
        if(listItems.Count > 0)
        {
            sAdsInfo            = new _ADS_INFO();
            XmlNode nodeBasic   = listItems[0];
            foreach(XmlAttribute itr in nodeBasic.Attributes)
            {
                if(itr.Name.Equals("reward_id"))
                    sAdsInfo.strRewardName  = itr.InnerText;
                else if(itr.Name.Equals("reward_count"))
                    sAdsInfo.count          = int.Parse(itr.InnerText);
                else if(itr.Name.Equals("cool_time_min"))
                    sAdsInfo.cool_time_min  = int.Parse(itr.InnerText);
            }
        }

        //listItems   = xmlDoc.GetElementsByTagName("buyInfo");
        //if(listItems.Count > 0)
        //{
        //    XmlNode nodeTower = listItems[0];

        //    foreach(XmlAttribute itr in nodeTower.Attributes)
        //    {
        //        sBuyItemCountInfo.Add( itr.Name, int.Parse(itr.InnerText));
         //   }
        //}

        listItems   = xmlDoc.GetElementsByTagName("playonInfo");
        sDefaultPlayOnData.Clear();
        for(int q = 0; q < listItems.Count; ++q)
        {
            _PLAY_ON_INFO oInfo   = new _PLAY_ON_INFO();

            XmlNode nodeTower   = listItems[q];
            XmlAttributeCollection attr = nodeTower.Attributes;

            oInfo.strBooster = attr["booster"].InnerText;
            //oInfo.item = attr["item"].InnerText;
            //oInfo.itemNum = int.Parse(attr["itemNum"].InnerText);
            oInfo.boosterNum = int.Parse(attr["boosterNum"].InnerText);
            oInfo.moveCount = int.Parse(attr["moveNum"].InnerText);
            oInfo.addTime  = int.Parse(attr["addTime"].InnerText);

            sDefaultPlayOnData.Add(oInfo);
        }

        sInviteRewardInfoList.Clear();
        listItems   = xmlDoc.GetElementsByTagName("InviteRewardInfo");
        for(int q = 0; q < listItems.Count; ++q)
        {
            _INVITE_REWARD_INFO oInfo   = new _INVITE_REWARD_INFO();
                
            oInfo.RewardNum = new int[3];
            oInfo.RewardName = new string[3];

            XmlNode nodeTower   = listItems[q];
            XmlAttributeCollection attr = nodeTower.Attributes;
            oInfo.RewardInviteCount = int.Parse(attr["RewardInviteCount"].InnerText);
            oInfo.RewardName[0] = attr["Reward0"].InnerText;
            oInfo.RewardNum[0] = int.Parse(attr["RewardNum0"].InnerText);
            oInfo.RewardName[1] = attr["Reward1"].InnerText;
            oInfo.RewardNum[1] = int.Parse(attr["RewardNum1"].InnerText);
            oInfo.RewardName[2] = attr["Reward2"].InnerText;
            oInfo.RewardNum[2] = int.Parse(attr["RewardNum2"].InnerText);
            sInviteRewardInfoList.Add(oInfo);
        }
    }
        
	public static int GetItemCost(string itmeName)
	{
		if(sCoinInfo.ContainsKey(itmeName))
		   return sCoinInfo[itmeName];
		return 0;
	}
	
    public static int GetInviteRewardNum()
    {
        return sInviteRewardInfoList.Count;
    }

    public static int GetInviteRewardInviteCount(int step)
    {
        if (step < 0 || step >= sInviteRewardInfoList.Count)
            return 0;

        return sInviteRewardInfoList[step].RewardInviteCount;
    }

    public static int GetInviteRewardNum(int step, int index)
    {
        if (step < 0 || step >= sInviteRewardInfoList.Count)
            return 0;
            
        if (index < 0 || index >= 3)
            return 0;
        
        return sInviteRewardInfoList[step].RewardNum[index];
    }

    public static string GetInviteRewardName(int step, int index)
    {
        if (step < 0 || step >= sInviteRewardInfoList.Count)
            return null;

        if (index < 0 || index >= 3)
            return null;

        return sInviteRewardInfoList[step].RewardName[index];
    }
    
    public static int GetDefaultPlayOnMoveCount(int index)
	{
        return sDefaultPlayOnData[0].moveCount;

        //if(index < sDefaultPlayOnData.Count && index > -1)
        //    return sDefaultPlayOnData[index].moveCount;
        //else if (index >= sDefaultPlayOnData.Count)
        //    return sDefaultPlayOnData[sDefaultPlayOnData.Count -1].moveCount;
        
		//return 0;
	}

    public static int GetDefaultPlayOnAddTime(int index)
	{
        return sDefaultPlayOnData[0].addTime;
	}

    public static string GetDefaultPlayOnBooster()
	{
        return sDefaultPlayOnData[0].strBooster;
	}

    public static int GetDefaultPlayOnBoosterNum(int index)
    {
        if(index < sDefaultPlayOnData.Count && index > -1)
            return sDefaultPlayOnData[index].boosterNum;
        else if (index >= sDefaultPlayOnData.Count)
            return sDefaultPlayOnData[sDefaultPlayOnData.Count -1].boosterNum;

        return 0;
    }

	public static void GetRescueRewardInfoById(int id, ref _RESCUE_REWARD_INFO outInfo)
	{
		outInfo.set(0, 0, 0, 0, "", 0);

        for(int k = 0; k < sRescueRewardData.Count; ++k)
        {
            if(id == sRescueRewardData[k].id)
            {
                outInfo         = sRescueRewardData[k];
                break;
            }
        }
	}
    public static int GetRescueRewardDataCount()
    {
        return sRescueRewardData.Count;
    }
	public static int getRescueRewardIdByLevel(int levelId)
    {
        for(int k = 0; k < sRescueRewardData.Count; ++k)
        {
            if(sRescueRewardData[k].nMapId0==levelId || sRescueRewardData[k].nMapId1==levelId || 
                sRescueRewardData[k].nMapId2==levelId )
                return sRescueRewardData[k].id;
        }
        return 0;
    }
    
    public static bool checkMissionClearedByLevel(int idLevel)
    {
        int idData              = InfoLoader.getRescueRewardIdByLevel(idLevel);
        if(idData <= 0)         return false;

        return checkMissionClearedById(idData);
    }
    public static int getMissionClearedConditionCountById(int idMission)
    {
        _RESCUE_REWARD_INFO info = new _RESCUE_REWARD_INFO();
        InfoLoader.GetRescueRewardInfoById(idMission, ref info);
        
        if(Root.Data.idxMaxClearedLevel+1 < info.nMapId0) 
            return 0;

        int temp                = 3==Root.Data.gameData.GetGradeLevelByIndex(info.nMapId0-1) ? 1 : 0;
        temp                    = 3==Root.Data.gameData.GetGradeLevelByIndex(info.nMapId1-1) ? ++temp : temp;
        temp                    = 3==Root.Data.gameData.GetGradeLevelByIndex(info.nMapId2-1) ? ++temp : temp;

        return temp;
    }
    public static bool checkMissionClearedById(int idData)
    {
        return (3 == getMissionClearedConditionCountById(idData));
    }
    public static bool isMissionRewardedById(int idMission)
    {
        if(false == checkMissionClearedById(idMission))
            return false;

        return Root.Data.gameData.isMissionRewarded(idMission);
    }
    // successed but not yet rewarded id.
    public static int getFirstMissionRewardIdToReward()
    {
        int idCurCH             = (Root.Data.TotalClearedLevelCount / (int)Data.BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER) + 1;
        for(int q = 0; q < idCurCH*2; ++q)
        {
            int id              = q + 1;
            if(InfoLoader.checkMissionClearedById(id) && false==Root.Data.gameData.isMissionRewarded(id))
                return id;
        }
        return -1;
    }

    // nDay : 1 ~ 
	public static void GetDailyRewardData(int nDay, ref _DAILY_REWARD_INFO outInfo)
	{
		outInfo.count           = 0;

		if(0 == sDailyRewardData.Count)    
			return;

        int idx                 = --nDay;
        if(idx<0 || idx>=sDailyRewardData.Count)
            idx                 = 0;
        
        outInfo                 = sDailyRewardData[idx];
	}


    public static int GetCachaWheelItemCount()
    {
        return sWheelGachaItemData.Count;
    }

    public static bool GetWheelGachaItemInfoByIndex(int idx, ref _WHEEL_GACHA_ITEM_INFO oOutInfo)
    {
        oOutInfo.count          = 0;

        if(idx<0 || idx>=sWheelGachaItemData.Count)
            return false;

        oOutInfo                = sWheelGachaItemData[ idx ];
        return true;
    }

    public static bool GetItemShopInfoById(string strId, ref _ITEM_SHOP_DATA oOutInfo)
    {
        oOutInfo._strId         = "";

        for(int z = 0; z < sItemShopInfoList.Count; ++z)
        {
            if(true == sItemShopInfoList[z]._strId.Equals( strId ))
            {
                oOutInfo        = sItemShopInfoList[z];
                return true;
            }
        }
        return false;
    }

    public static int getPriceGacha()
    {
        return _price_wheelGacha;
    }
    public static int getMinTimeFreeGacha()
    {
        return _min_time_freeGacha;
    }
    public static int getMinTime4LifeRefill()
    {
        return Data.GameData.CHARGE_MIN;    // _life_refill_time_min;
    }

    public static _ADS_INFO getAdsInfo()
    {
        return sAdsInfo;
    }

    // for firebase data management.
    public static string buildLVGradeData(int idxLv)
    {
        Data.GameData oData     = Root.Data.gameData;
        int _100                = 100;
        int idxStart            = idxLv - (idxLv%_100);
        string strBuffer        = "";
        for(int zz = 0; zz < _100; ++zz)
        {
            int idxCur          = idxStart + zz;
            if(idxCur >= oData.record.LevelResultDataList.Length)
                return strBuffer;
                
            if(zz>0)            strBuffer += ",";

            strBuffer += oData.record.LevelResultDataList[idxCur].ucGrade.ToString();
        }
        return strBuffer;
    }

    public static string buildRMissionData()
    {
        Data.GameData oData     = Root.Data.gameData;
        string strBuffer        = "";
        for(int zz = 0; zz < oData.record.RewardedMissionIds.Length; ++zz)
        {
            if(zz>0)            strBuffer += ",";
            strBuffer += oData.record.RewardedMissionIds[zz].ToString();
        }
        return strBuffer;
    }
}

