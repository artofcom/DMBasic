using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using JsonFx.Json;
using System.Linq;

namespace NOVNINE
{
	public static class Alarm 
    {
		class Entry 
        {
			public int index = 0;
			public string title = "";
			public string msg = "";
			public System.DateTime date = System.DateTime.UtcNow;
		}

		public static int AlarmCount
        {
			get {
				return entrys.Count;
			}
		}

		static List<Entry> entrys = new List<Entry>();

        static Alarm() 
        {
#if UNITY_IPHONE
			UnityEngine.iOS.NotificationServices.RegisterForNotifications(
				UnityEngine.iOS.NotificationType.Alert |
				UnityEngine.iOS.NotificationType.Badge |
				UnityEngine.iOS.NotificationType.Sound);
#endif
            //Init();
        }

		public static void Init() 
        {
			if (PlayerPrefs.HasKey ("Alarm")) 
            {
				string empty = PlayerPrefs.GetString ("Alarm");
				entrys = JsonReader.Deserialize<Entry[]> (empty).ToList ();
			}
		}

        public static int Register(string title, string message, System.DateTime date, Color32 bgColor, bool enableSound = false,string bigIcon = "", bool vibrate = true, bool lights = true) 
        {
            Debugger.Assert(title.Length>0 && message.Length>0);

			entrys.Sort((a, b) => a.index.CompareTo(b.index));
			int index = 0;
			for (int i = 0; true; ++i) 
            {
				if (entrys.Count <= i || entrys[i].index != i) 
                {
					index = i;
					break;
				}
			}

			Entry addEntry = new Entry ();
			addEntry.index = index;
			addEntry.title = title;
			addEntry.msg = message;
			addEntry.date = date;
			entrys.Add (addEntry);

		#if UNITY_IPHONE
			UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification();
			notif.fireDate = date;
			if(enableSound)
                notif.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
			notif.alertAction = Locale.GetString(title);
			notif.alertBody = Locale.GetString(message);
			UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notif);

            //notification.hasAction = false;

		#endif
			
        #if UNITY_ANDROID
			if(Application.platform == RuntimePlatform.Android) 
            {
//                string ltitle = Locale.GetString(title);
//                string lmsg = Locale.GetString(message);
                TimeSpan TS = date - System.DateTime.UtcNow;

                //(int)TS.TotalSeconds * 1000
//                using (AndroidJavaClass cls = new AndroidJavaClass("com.bitmango.LocalNotification")) 
//                {
//                    using(AndroidJavaObject inst = cls.CallStatic<AndroidJavaObject>("instance")) 
//                    {
//                        bool isSuccess = inst.Call<bool>("registNotification", new object[] {lmsg, ltitle, date.ToString(), enableSound, index} );
//                        if(!isSuccess) Debug.Log("AlarmRegister Fail");
//                    }
//                }

                AndroidJavaClass cls = new AndroidJavaClass("nov9.unitynotification.UnityNotificationManager");

                if (cls != null)
                {
                    cls.CallStatic("SetNotification", index, (long)(TS.TotalSeconds * 1000), title, message, message, 
                        enableSound ? 1 : 0, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, "notify_icon_small", 
                        bgColor.r * 65536 + bgColor.g * 256 + bgColor.b, Application.identifier);
                }
            }
			#endif

			PlayerPrefs.SetString("Alarm", JsonWriter.Serialize (entrys));

			return index;
		}

		public static void UnRegister(int index) 
        {
            Debugger.Assert(index>=0);

			List<Entry> unRegisterEntrys = entrys.FindAll(x => x.index == index);
			#if UNITY_IPHONE
			UnityEngine.iOS.LocalNotification[] notifs = UnityEngine.iOS.NotificationServices.scheduledLocalNotifications;

			foreach(Entry unRegisterEntry in unRegisterEntrys) 
            {
				foreach(UnityEngine.iOS.LocalNotification notif in notifs) 
                {
					if (notif.alertAction.Equals(Locale.GetString(unRegisterEntry.title)) && notif.alertBody.Equals(Locale.GetString(unRegisterEntry.msg))) 
                        UnityEngine.iOS.NotificationServices.CancelLocalNotification(notif);
				}
			}
			#endif
			
            #if UNITY_ANDROID
			if(Application.platform == RuntimePlatform.Android) 
            {
//                int failCnt = 0;
//                using (AndroidJavaClass cls = new AndroidJavaClass("com.bitmango.LocalNotification")) {
//                    using(AndroidJavaObject inst = cls.CallStatic<AndroidJavaObject>("instance")) {
//                        foreach(Entry unRegisterEntry in unRegisterEntrys) { 
//                            bool isSuccess = inst.Call<bool>("unregistNotification", unRegisterEntry.index);
//                            if (!isSuccess) failCnt++;
//                        }
//                    }
//                }
//                if(failCnt > 0) Debug.Log("AlarmUnRegister failCnt : " + failCnt);
            
                AndroidJavaClass pluginClass = new AndroidJavaClass("nov9.unitynotification.UnityNotificationManager");
                if (pluginClass != null)
                    pluginClass.CallStatic("CancelPendingNotification", index);
            }
			#endif

			foreach (Entry unRegisterEntry in unRegisterEntrys) 
            {
				entrys.Remove(unRegisterEntry);
			}

			PlayerPrefs.SetString("Alarm", JsonWriter.Serialize (entrys));
		}

        public static void AllUnRegister() 
        {

#if UNITY_IPHONE
			UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
#elif UNITY_ANDROID
			if(Application.platform == RuntimePlatform.Android) 
            {
                AndroidJavaClass pluginClass = new AndroidJavaClass("nov9.unitynotification.UnityNotificationManager");
                pluginClass.CallStatic("ClearShowingNotifications");
//                if (pluginClass != null)
//                {
//                    foreach(Entry unRegisterEntry in entrys)
//                    { 
//                        pluginClass.CallStatic("ClearShowingNotifications", unRegisterEntry.index);
//                    }
//                }

//                using (AndroidJavaClass cls = new AndroidJavaClass("com.bitmango.LocalNotification")) 
//                {
//                    using(AndroidJavaObject inst = cls.CallStatic<AndroidJavaObject>("instance"))
//                    {
//						foreach(Entry unRegisterEntry in entrys) 
//                        {
//                            bool isSuccess = inst.Call<bool>("unregistNotification", unRegisterEntry.index);
//                            if (!isSuccess) failCnt++;
//                        }
//                    }
//                }
//                if(failCnt > 0) Debug.Log("AllUnRegister failCnt : " + failCnt);
            }
#endif

			entrys.RemoveRange (0, entrys.Count);
			PlayerPrefs.SetString("Alarm", JsonWriter.Serialize (entrys));
		}
	}
}
