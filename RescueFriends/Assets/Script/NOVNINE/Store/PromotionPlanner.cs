//#define LLOG_PROMOTIONPLANNER

using UnityEngine;
using System;
using System.Collections.Generic;

using NOVNINE.Diagnostics;
using JsonFx.Json;

namespace NOVNINE.Store
{

public static class PromotionPlanner
{
    public static Plan CurrentPlan {
        get {
			if(schedules == null) return null;
            foreach(var s in schedules) {
                //if(s.type == Plan.PlanType.Discount && s.IsActive)
				if(s.type == null) continue;
				if(s.type.Equals("discount") && s.IsActive)
                    return s;
            }
            return null;
        }
    }

	public static Plan CurrentGift {
		get {
			if(schedules == null) return null;
			foreach(var s in schedules) {
				if(s.type == null) continue;
                //if(s.type == Plan.PlanType.Gift && s.IsActive)
				if(s.type.Equals("gift") && s.IsActive)
					return s;
			}
			return null;
		}
	}

	public static List<Plan> CurrentEvent {
		get {
			if(schedules == null) return null;
			
			List<Plan> planList = new List<Plan>();
			foreach(var s in schedules) {
				if(s.type == null) continue;
				if(s.type.Equals("event") && s.IsActive) {
					planList.Add(s);
				}
			}
			return planList;
		}
	}

    public class Plan {
        public string name;
		public string type;
        public bool useUTC;
        public DateTime begin;
        public DateTime end;
		/*
        public enum PlanType {
            Discount,
            Gift,
        }
		*/
        public enum Repeat {
            None,
            Daily,
        }
        public enum ItemTag {
            None,
            Value,
            Pop,
            Super
        }
        public enum TargetUser {
            All,
            New,
            Existing
        }

        public enum TargetDevice {
			None,
            iPhone,
           	iPad,
			Google,
			Amazon
        }

        public Repeat repeat;
        public class Item {
            public string item;
            public float save;
            public ItemTag tag;

            public bool IsPop {
                get { return (tag == ItemTag.Pop); }
            }
            public bool IsValue {
                get { return (tag == ItemTag.Value); }
            }
            public bool IsSuper {
                get { return (tag == ItemTag.Super); }
            }

				public NOVNINE.ShopItem AsShopItem {
				get {
						return NOVNINE.Context.UncleBill.GetShopItemByID(item);
				}
			}
        }
        public Item[] items;

		public string[] regions;
		public TargetDevice[] devices;
		public string text;
		public TargetUser user;

        DateTime Now {
            get {
                if(useUTC)
                    return DateTime.UtcNow;
                else 
                    return DateTime.Now;
            }
        }

		TargetDevice CurrentDeviceType {
			get {
#if UNITY_IPHONE
				if(NNTool.IsTablet()) return TargetDevice.iPad;
				else return TargetDevice.iPhone;
#else
				if(Context.NNPlatform.storeType == PlatformContext.StoreType.GoogleStore) return TargetDevice.Google;
				else if(Context.NNPlatform.storeType == PlatformContext.StoreType.AmazonStore) return TargetDevice.Amazon;
				else return TargetDevice.None;
#endif
			}
		}

		static string countryCode;

        public bool IsActive {
            get {
                //if(type == Plan.PlanType.Gift) {
				if(type.Equals("gift")) {
					if(GiftDelivered) return false;
				}

				if(user != TargetUser.All) {
					DateTime installed = Context.NNPlatform.installed.Date;
					bool inRange = (installed >= begin && installed <= end);
					if(user == TargetUser.New && !inRange) return false;
					if(user == TargetUser.Existing && inRange) return false;
				}
				
				countryCode = countryCode == null ? NativeInterface.GetCurrentLocaleID() : countryCode;

				if(regions != null && !Array.Exists(regions, (c)=> c.Equals(countryCode))) return false;

				TargetDevice deviceId = this.CurrentDeviceType;
				if(devices != null && deviceId != TargetDevice.None && !Array.Exists(devices, (c)=> c == deviceId )) return false;

                if(begin == defaultDateTime) return true;
                DateTime now = this.Now;
                if(repeat == Repeat.None) {
                    return ((now >= begin) && (now <= end));
                } else {//if(repeat == Repeat.Daily) {
                    TimeSpan nowTimeOfDay = now.TimeOfDay;
                    return ((nowTimeOfDay >= begin.TimeOfDay) && (nowTimeOfDay <= end.TimeOfDay));
                }
            }
        }

        public bool IsTemporal {
            get {
                return (begin != defaultDateTime);
            }
        }

        public TimeSpan EndsAfter {
            get {
                if(begin == defaultDateTime) return new TimeSpan();
                DateTime now = this.Now;
                if(repeat == Repeat.None) {
                    return end.Subtract(now);
                } else {//if(repeat == Repeat.Daily) {
                    return end.TimeOfDay.Subtract(now.TimeOfDay);
                }
            }
        }

        public float GetSaveForItem(string item) {
            foreach(var i in items) {
                if(i.item == item)
                    return i.save;
            }
            return 0;
        }

		public bool GiftDelivered {
			get {
                //if(type != Plan.PlanType.Gift) return false;
				if(!type.Equals("gift")) return false;
				return PlayerPrefs.HasKey("PromotionPlanner."+name);
			}
		}

		public void DeliverGift()
        {
#if LLOG_PROMOTIONPLANNER
			Debug.Log("PromotionPlanner Gift Delivered : "+name);
#endif
			foreach(var itemDesc in items) 
				{
				NOVNINE.ShopItem sitem = NOVNINE.Context.UncleBill.GetShopItemByID(itemDesc.item);
				Debugger.Assert(sitem != null);
				if(sitem == null) continue;
				NOVNINE.Wallet.Gain(sitem);
			}
			PlayerPrefs.SetInt("PromotionPlanner."+name, 1);
		}
    }

    static Plan[] schedules;
    static DateTime defaultDateTime = new DateTime();

    public static void LoadSchedules(System.Action<bool> callback) {
#if LLOG_PROMOTIONPLANNER
		Debug.Log("PromotionPlanner.LoadSchedules");
#endif
		if(schedules == null) {
#if USE_Playtomic
        PlaytomicCloud.LoadGameVariable<Plan[]>("promotionplan", (result)=>{
            if(result!=null) {
#if LLOG_PROMOTIONPLANNER
                Debug.Log("PromotionPlanner : Received] " + result);
#endif
                schedules = result;
				Platform.SafeCallback(callback, true);
            } else {
                Platform.SafeCallback(callback, false);
            }
        });
#else
        Debug.LogError("PromotionPlanner : USE_Playtomic define is not set!!");
		Platform.SafeCallback(callback, false);
#endif
		}
		else {
			Platform.SafeCallback(callback, true);
		}
    }

}

}
