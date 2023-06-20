using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using JsonFx.Json;

namespace NOVNINE
{
public static class ControlCenter
{
	public static Action<Msg[]> OnMessageArrived;

	public class Data
    {
		public string protocolVer;
		public int userLv;
		public Msg[] message;
    }
	public static Data data;

	public class Msg
	{
		public string idx;
		public string title;
		public string desc;
		public string item;
		public string amount;
		public string needAck;
	}
	
	static bool isMessageArrived = false;

	public static bool IsMessageArrived {
		get {
			return isMessageArrived;
		}
	}

	static string protocolVersion = "2.0";
	static string requestParam = "";
    static bool isDataDownLoading = false;

	public static void InitParam() {
		Dictionary<string,string> dic = new Dictionary<string, string>();

		string platform = ""; 
		if (Platform.Info.storeType == PlatformContext.StoreType.AppleStore) {
			platform = "ap";
		} else if (Platform.Info.storeType == PlatformContext.StoreType.GoogleStore) {
			platform = "go";
		} else if (Platform.Info.storeType == PlatformContext.StoreType.AmazonStore) {
			platform = "az";
		} else {
            Debugger.Assert(false, "storeType unknown : "+Platform.Info.storeType);
        }

		dic.Add ("protocolVer", string.Format("{0}", protocolVersion));
		dic.Add ("appId", string.Format("{0}", Context.NNPlatform.appID));
		dic.Add ("UUID", string.Format("{0}", Profile.UUID));
		dic.Add ("aid", string.Format("{0}", Profile.ADID));
		dic.Add ("it", string.Format("{0}", Profile.InstallTimeStr));
		dic.Add ("iv", string.Format("{0}", Profile.InstallVersion));
		dic.Add ("cv", string.Format("{0}", NativeInterface.GetBundleVersion ()));
		dic.Add ("country", string.Format("{0}", NativeInterface.GetCurrentLocaleID()));
		dic.Add ("platform", string.Format("{0}", platform));
		dic.Add ("device", string.Format("{0}", NNTool.IsTablet () ? "tablet" : "phone"));
		requestParam =  WWWNov9.GetParametar(dic);
	}

    public static void GetControlCenterInfo()
    {
        if(isDataDownLoading) {
            Debugger.Log("ControlCenter.GetControlCenterInfo isDataDownLoading");
            return;
        }
		if (Application.internetReachability == NetworkReachability.NotReachable) return;
        isDataDownLoading = true;
		InitParam ();

        var swatch = new System.Diagnostics.Stopwatch();
        swatch.Start();
//		Nov9Request.GET("http://api.cc.bitmango.com/api/controlcenter.php?data=" + requestParam, (json)=>{
//            swatch.Stop();
//            if (OnMessageArrived == null)
//                OnMessageArrived = ShowMessage;
//            if(!string.IsNullOrEmpty(json)) {
//                ParseControlCenterInfo(json);
//            } else {
//                Debugger.Log("ControlCenter.ParseControlCenterInfo - json is NullOrEmpty");
//            }
//            isDataDownLoading = false;
//        });
    }
		
	public static void ParseControlCenterInfo(string json)
	{
        Debugger.Assert(!string.IsNullOrEmpty(json), "ControlCenter.ParseControlCenterInfo - json is NullOrEmpty");
	
        try {
            data = JsonReader.Deserialize<Data>(json);
            Debugger.Assert(data != null, "ControlCenter.ParseControlCenterInfo data is null");
            int v = data.userLv;
            if( v>=0 && v<(int)Profile.UserLevel.Max )
                v = Profile.MigrateUserLevel(v);
            Profile.SetUserLv(v);
			ValidateMessage();
			isMessageArrived = true;
        } catch (JsonDeserializationException exception) {
            Debugger.LogError("ControlCenter.ParseControlCenterInfo - JsonDeserializationException - "+exception.Message);
        }
        OnMessageArrived(data.message);
	}
	
	private static void ValidateMessage() {
		List<Msg> msgList = new List<Msg>();
		foreach (var m in data.message) {
            if(string.IsNullOrEmpty(m.item) && string.IsNullOrEmpty(m.desc)) {
                Debugger.Assert(false, "ControlCenter.ValidateMessage item or desc is null");
                continue;
            }
			if(PlayerPrefs.HasKey(m.idx)) {
				Debugger.Log("ControlCenter.ValidateMessage already use message - "+ m.idx);
				continue;
			}
			msgList.Add(m);
		}
		data.message = msgList.ToArray();
	}
	
	static void ShowMessage(Msg[] message) {
        TaskManager.StartCoroutine(CoShowAlert(message));
	}

    public static IEnumerator CoShowAlert(Msg[] message) {
        foreach(var msg in message) {
            Debugger.Assert(!string.IsNullOrEmpty(msg.item) || !string.IsNullOrEmpty(msg.desc), "ControlCenter.ShowMessage item and desc is null");
            if(string.IsNullOrEmpty(msg.item) && string.IsNullOrEmpty(msg.desc)) continue;
            Debugger.Log ("ControlCenter.ShowMessage - "+msg.idx);
            string description = WWW.UnEscapeURL(msg.desc);
            if(!string.IsNullOrEmpty(msg.item)) {
                try {
                    int amount = Convert.ToInt32(msg.amount);
                    Debugger.Assert(amount > 0, "ControlCenter.ShowMessage amount is Zero");
                    if(amount == 0) continue;
                    string itemName;
                    if(msg.item == "gamemoney") {
                        itemName = "coin";
                    } else {
                        var iItem = Context.UncleBill.GetInventoryItemByID(msg.item);
                        Debugger.Assert(iItem != null, "ControlCenter.Showmessage inventory item not found " + msg.item);
                        if(iItem == null) continue;
                        itemName = iItem.name;
                    }
                    description += string.IsNullOrEmpty(msg.desc) ? "" : "\n";
                    description += itemName + " x " + amount;
                } catch(System.Exception e) {
                    Debugger.LogError("ControlCenter.ShowMessage " + e.ToString());
                    continue;
                }
            }
            bool clicked = false;
            NativeInterface.ShowAlert (WWW.UnEscapeURL(msg.title), description, "ok", (ok)=>{ if(ok) {UseMessage(msg); clicked = true;}});
            while(!clicked) {
                yield return null;
            }
        }
    }

	public static void UseMessage(Msg msg) {
        Debugger.Assert(msg != null, "ControlCenter.UseMessage msg is null");
		Debugger.Log ("ControlCenter.UseMessage - "+msg.idx);
        Debugger.Assert(!string.IsNullOrEmpty(msg.item) || !string.IsNullOrEmpty(msg.desc), "ControlCenter.UseMessage item or desc is null");
        if(string.IsNullOrEmpty(msg.item) && string.IsNullOrEmpty(msg.desc)) return;

        if(!string.IsNullOrEmpty(msg.item)) {
            try {
                int amount = Convert.ToInt32(msg.amount);
                Debugger.Assert(amount > 0, "ControlCenter.UseMessage amount is Zero");
                if(amount == 0) return;
                if(msg.item == "gamemoney") {
                    Wallet.GainGameMoney(amount);
                } else {
                    var iItem = Context.UncleBill.GetInventoryItemByID(msg.item);
                    Debugger.Assert(iItem != null, "ControlCenter.Showmessage inventory item not found " + msg.item);
                    if(iItem == null) return;
                    Wallet.Gain(msg.item, amount);
                }
                Wallet.Save();
            } catch(System.Exception e) {
                Debugger.LogError("ControlCenter.UseMessage " + e.ToString());
            }
        }

		PlayerPrefs.SetInt(msg.idx,1);
		PlayerPrefs.Save();
		
		if (msg.needAck == "1") {
            Dictionary<string,string> dic = new Dictionary<string, string>();   
            dic.Add ("protocolVer", string.Format("{0}", protocolVersion));
            dic.Add ("aid", string.Format("{0}", Profile.ADID));
            dic.Add("idx", string.Format("{0}", msg.idx));
//			Nov9Request.GET_Plain("http://api.cc.bitmango.com/api/needack.php?data=" + WWWNov9.GetParametar(dic)); 
		}
	}
}
}
