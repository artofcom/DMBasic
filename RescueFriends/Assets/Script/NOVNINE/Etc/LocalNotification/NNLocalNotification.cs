using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using NOVNINE;

#if USE_LocalNotification

public class NNLocalNotification :  MonoBehaviour
{
    public static bool IsAlarm = true;
    const int ALARM_HOUR = 12;

    public bool enableSound = true;

    string alertTitle = "Tasty Magic";
    string[] alertMessages = new string[] 
    {
        "Play Tasty Magic!",
        "Hey! Let's play together.",
        "Hey I'm Envy! Let's play together.",
        "Hey I'm Penelope! Let's play together.",
        "Hey I'm choco! Let's play together.",
        "Hey! Let's play together."
    };

	int[] notificationDays = new int[] { 3, 7, 14, 21, 28 };
	bool isRegist = false;

	void Awake()
	{
		Alarm.Init();
        UnRegist();
	}

    void OnApplicationPause(bool pause)
    {
        if (PlayerPrefs.GetInt("useLocalNotification") == 1)
        {
            if(NNLocalNotification.IsAlarm)
            {
                if (true == pause) 
                    Regist();
                else
                    UnRegist();    
            }   
        }
	}

	void OnApplicationQuit()
	{
        if (PlayerPrefs.GetInt("useLocalNotification") == 1)
        {
            if (NNLocalNotification.IsAlarm)
                Regist();   
        }
	}

	void Regist() 
    {
        if (isRegist == false) 
        {
            List<int> registerIndexs = new List<int>();

            for (int i=0; i< notificationDays.Length; i++)
            {
                registerIndexs.Add(RegisterDate(alertTitle, alertMessages[i], GetFireTickAfterDays(notificationDays[i]), enableSound));
            }
                
            if (Director.Instance != null && Root.Data != null)
            {
                if (Director.Instance.IsFullChargedStamina() == false)
                    registerIndexs.Add(RegisterDate(alertTitle, "Your Life is Full! You are ready to take on a journey! Enjoy the world with full of Tasty Magic!", GetFireTickFullStamina(), enableSound));

                registerIndexs.Add(RegisterDate(alertTitle, "Special Daily reward for you is ready. Play tasty levels right now!", GetFireTickDailyBonus(), enableSound));
            }

            string bmRegister = string.Join(",", registerIndexs.ConvertAll(x=>x.ToString()).ToArray());
			PlayerPrefs.SetString("NNLocalNotification", bmRegister);
			isRegist = true;
		}
	}

	void UnRegist()
    {
        string[] bmRegisters = PlayerPrefs.GetString("NNLocalNotification").Split(',');
		
		foreach(string bmRegister in bmRegisters) 
        {
			if (!string.IsNullOrEmpty(bmRegister))
				UnRegister(System.Convert.ToInt32(bmRegister));
			
		}
        PlayerPrefs.SetString("NNLocalNotification", "");
		isRegist = false;
	}

    static System.DateTime GetFireTickFullStamina()
    {
        int count = Data.GameData.FULL_STAMINA - NOVNINE.Wallet.GetItemCount("life");
        if (count > 0)
        {
            System.DateTime fireTime = System.DateTime.UtcNow;
            long tickGap = LGameData.GetInstance().GetCurrentServerTime().Ticks - Root.Data.gameData.GetStaminaChargeTick().Ticks;
            fireTime = fireTime.AddTicks((Data.GameData.CHARGE_TIME * count) - tickGap);
            return fireTime;    
        }
            
        return new System.DateTime();
    }

    static System.DateTime GetFireTickDailyBonus()
    {
        System.DateTime fireTime = System.DateTime.UtcNow;
        System.DateTime t = Root.Data.gameData.GetDailyRewardDate().AddDays(1);
        long ticks = ((t.Hour * 60 * 60) + (t.Minute * 60) + t.Second) * Data.GameData.TICK_DETAIL;
        ticks = t.Ticks - ticks - LGameData.GetInstance().GetCurrentServerTime().Ticks;
        fireTime = fireTime.AddTicks(ticks);
        return fireTime;
    }

	static System.DateTime GetFireTickAfterDays(int days)
    {
        System.DateTime fireTime = System.DateTime.UtcNow;
        int hourOffset = ALARM_HOUR - fireTime.Hour;
        if(hourOffset < 0)
            hourOffset += 24;
        fireTime = fireTime.AddHours(hourOffset);
        fireTime = fireTime.AddDays(days);
		return fireTime;
    }

  	public static int RegisterDate(string title, string message, System.DateTime fireTime, bool enableSound = false, int index = 0)
	{
		Debugger.Assert(title.Length>0 && message.Length>0);
		//string lmessage = Locale.GetString(message);
		//string ltitle = Locale.GetString(title);
        int returnIndex = Alarm.Register(title, message, fireTime, new Color32(0x77, 0x92, 0xa9, 255),enableSound,"notify_icon_big"); 
		return returnIndex;
	}
	
	public static void UnRegister(int index)
    {
		Alarm.UnRegister (index);
    }

	public static void AllUnRegister()
	{
		Alarm.AllUnRegister();
	}
}
#endif