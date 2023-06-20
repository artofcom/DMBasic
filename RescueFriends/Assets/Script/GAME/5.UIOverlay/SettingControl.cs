using UnityEngine;
using System.Collections;
//using Facebook.Unity;
using System.Collections.Generic;

public class SettingControl : MonoBehaviour,IControlBase
{
	readonly string[] BGMList = {"bgm_play","bgm_play","bgm_play","bgm_play", "bgm_play"};
	
	public tk2dUIItem BGM;
	public tk2dUIItem FX;
	public tk2dUIItem Home;
	public tk2dUIItem facebookLogInBTN;
    public tk2dUIItem facebookLogOutBTN;
    public tk2dUIItem googleLogInBTN;
    public tk2dUIItem InfoBTN;
    public GameObject FacebookMigrationRewardIcon;
	public GameObject sfxMute;
	public GameObject bgmMute;
    public TextMesh ID;
    #if UNITY_ANDROID && !UNITY_EDITOR
    List<object> ColorList = new List<object>();
    #endif

    public void  OnEnter () 
	{
        FacebookMigrationRewardIcon.SetActive(false);// !LGameData.GetInstance().GetFacebookMigrationRewardReceive());
		sfxMute.SetActive(!NNSoundHelper.EnableSFX);
		bgmMute.SetActive(!NNSoundHelper.EnableBGM);
        googleLogInBTN.gameObject.SetActive(false);

        facebookLogInBTN.gameObject.SetActive(false);
        facebookLogOutBTN.gameObject.SetActive(false);
	}

    public void OnLeave()
    {
    }

	public void EventLock(bool Lock)
	{
		BGM.GetComponent<BoxCollider>().enabled = Lock;
		FX.GetComponent<BoxCollider>().enabled = Lock;
		Home.GetComponent<BoxCollider>().enabled = Lock;
        facebookLogInBTN.GetComponent<BoxCollider>().enabled = Lock;	
        facebookLogOutBTN.GetComponent<BoxCollider>().enabled = Lock;    
        googleLogInBTN.GetComponent<BoxCollider>().enabled = Lock;    
        InfoBTN.GetComponent<BoxCollider>().enabled = Lock;    
	}
	
	public void CLICK (tk2dUIItem item) 
	{
		NNSoundHelper.Play("FX_btn_on");

        if (InfoBTN == item)
        {
            Scene.ShowPopup("InfoPopup");
            return;
        }

		if(BGM == item )
		{
			NNSoundHelper.EnableBGM = !NNSoundHelper.EnableBGM;

			if(NNSoundHelper.EnableBGM)
            {
                if(Scene.CurrentSceneName()=="PlayScene")
                    NNSoundHelper.PlayBGM(BGMList[JMFUtils.GM.CurrentLevel.missionType]);
                else 
                    NNSoundHelper.PlayBGM("bgm_title");
            }

			OnToggle( bgmMute, !NNSoundHelper.EnableBGM);
			
			return;
		}

		if(FX == item)
		{
			NNSoundHelper.EnableSFX = !NNSoundHelper.EnableSFX;
			OnToggle( sfxMute , !NNSoundHelper.EnableSFX);
			return;
		}
		
        //WorldOverlayHandler handler = Scene.GetOverlayHandler<WorldOverlayHandler>();
        //handler.BackClick(handler.sideMenuController.backButton);

		if( Home == item)
		{
            Scene.ChangeTo("MainScene","Home");
		}
		else if( facebookLogOutBTN == item)
        {
            NoticePopupHandler popup = Scene.GetPopup("NoticePopup") as NoticePopupHandler;

            #if UNITY_ANDROID && !UNITY_EDITOR
            if (PlayerPrefs.GetString("strGoogleLogin", "") != "cancel")
            {
                popup.SetErrorCode("Facebook Log Out", "Are you sure\nyou want to sign out?");    
            }
            else
            #endif
            {
                popup.SetErrorCode(NoticePopupHandler.ERROR_TYPE.ERR_FBLOIN, "The game data is initialized.\nWould you like to go out?");    
            }

            popup.OK_Callback = (_param) =>
                {
                    //FBHelper.Logout();

                //    #if UNITY_ANDROID && !UNITY_EDITOR
                //    if (PlayerPrefs.GetString("strGoogleLogin", "") != "cancel")
                //    {
                ////        LLoginLogic.ChangeLoginComponent<LLoginGPGSLogic>();
                //        PlayerPrefs.SetString("strFirstLogin", "GPGS");
                //    }
                //    else
                //    #endif
                    {
                        //LLoginLogic.ChangeLoginComponent<LLoginGuestLogic>();
                        PlayerPrefs.SetString("strFirstLogin", "GUEST");
                        //PlayerPrefs.DeleteKey("strJMFAccountID");
                        //PlayerPrefs.DeleteKey("strJMFAccountPW");
                    }
                    //LGameData.GetInstance().SetAccountLoginData(null,null);
                    //Director.GoTitle("FBLogOut");
                };

            popup.InitButton(NoticePopupHandler.BUTTON_TYPE.OK_CANCEL);
            Scene.ShowPopup("NoticePopup", null);

            return;
        }
        else if( facebookLogInBTN == item)
		{
            //if (LManager.GetInstance().IsValidConnection())
            {
            //    Scene.ChangeTo("MainScene","FBLogin");
            }
            //else
            {
                NoticePopupHandler popup = Scene.GetPopup("NoticePopup") as NoticePopupHandler;
                popup.InitButton(NoticePopupHandler.BUTTON_TYPE.OK);  
                popup.SetErrorCode("Network Error!", "Please check your connectivity.");
                Scene.ShowPopup("NoticePopup", null);
            }

            return;    
		}
#if UNITY_ANDROID
        else if( googleLogInBTN == item)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                //if (PlayerPrefs.GetString("strFirstLogin", "GUEST") == "FB")
                //    FBHelper.Logout();    

               // LLoginLogic.ChangeLoginComponent<LLoginGPGSLogic>();
               // Scene.ChangeTo("MainScene","GPGSLogin"); 
                //PlayerPrefs.SetString("strFirstLogin", "GPGS");
            }
            else
            {
                NoticePopupHandler popup = Scene.GetPopup("NoticePopup") as NoticePopupHandler;
                popup.InitButton(NoticePopupHandler.BUTTON_TYPE.NONE);  
                popup.SetErrorCode("Network Error!", "Please check your connectivity.");
                Scene.ShowPopup("NoticePopup", null);
            }

            return;    
        }
#endif
	}
	
	void OnToggle( GameObject item, bool toggle)
	{
		item.SetActive(toggle);
	}
	
}
