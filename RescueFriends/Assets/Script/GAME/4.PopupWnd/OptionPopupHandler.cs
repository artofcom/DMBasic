using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;

public class OptionPopupHandler : BasePopupHandler 
{
    public tk2dUIItem _btnClose = null;
    public tk2dUIItem _btnBGM   = null;
    public tk2dUIItem _btnSound = null;
    public tk2dTextMesh _txtVer = null;
    public tk2dTextMesh _txtID  = null;
    public tk2dUIItem _btnFBLogin = null;
    public tk2dUIItem _btnFBLogout= null;

    Transform _trBGMON          = null;
    Transform _trBGMOFF         = null;
    Transform _trSoundON        = null;
    Transform _trSoundOFF       = null;

    protected override void  OnEnter (object param) 
	{
        if(null==_trBGMON)	    _trBGMON    = _btnBGM.transform.Find("bgOn");
        if(null==_trBGMOFF)     _trBGMOFF   = _btnBGM.transform.Find("bgOff");
        if(null==_trSoundON)    _trSoundON  = _btnSound.transform.Find("bgOn");
        if(null==_trSoundOFF)   _trSoundOFF = _btnSound.transform.Find("bgOff");

		base.OnEnter(param);
        _refreshUI();
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnClose == item)
			Scene.ClosePopup(false);
        else if(_btnBGM == item)
        {
            NNSoundHelper.EnableBGM = !NNSoundHelper.EnableBGM;
            _refreshUI();
        }
        else if(_btnSound == item)
        {
            NNSoundHelper.EnableSFX = !NNSoundHelper.EnableSFX;
            _refreshUI();
        }
        else if(_btnFBLogin == item)
        {
            MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
            data2.isOkOnly      = true;
            data2.strMessage    = "Now you're moving to title...";
            data2.emotion       = MessagePopupHandler.EMOTIONS.HAPPY;
            Scene.ShowPopup("MessagePopup", data2, (ret) =>
            {
                Scene.ClosePopup(true);
            });
        }
        else if(_btnFBLogout == item)
        {
            MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
            data2.isOkOnly      = false;
            data2.strMessage    = "Do you want to disconnect\nfrom facebook?";
            data2.emotion       = MessagePopupHandler.EMOTIONS.SAD;
            Scene.ShowPopup("MessagePopup", data2, (ret) =>
            {
                MessagePopupHandler.RET eRet = (MessagePopupHandler.RET)ret;
                if(eRet == MessagePopupHandler.RET.OK)
                {
                    PlayerPrefs.SetString("LAST_LOGIN", "");                    
                    Facebook.Unity.FB.LogOut();
                    Scene.ClosePopup(true);
                }
            });
        }
	}

    void _refreshUI()
    {
        _trBGMON.gameObject.SetActive( NNSoundHelper.EnableBGM );
        _trBGMOFF.gameObject.SetActive( !NNSoundHelper.EnableBGM );
        _trSoundON.gameObject.SetActive( NNSoundHelper.EnableSFX );
        _trSoundOFF.gameObject.SetActive( !NNSoundHelper.EnableSFX );

        _btnFBLogin.gameObject.SetActive( !Facebook.Unity.FB.IsLoggedIn );
        _btnFBLogout.gameObject.SetActive( Facebook.Unity.FB.IsLoggedIn );

        _txtVer.text            = string.Format("Version : {0}", NOVNINE.Profile.CurrentVersion());
        _txtVer.Commit();

        if(false==NNSound.Instance.IsPlayingBGM() && NNSoundHelper.EnableBGM)
            NNSoundHelper.PlayBGM("bgm_title"); 

        _txtID.gameObject.SetActive(Facebook.Unity.FB.IsLoggedIn);
        if(_txtID.gameObject.activeSelf)
        {
            _txtID.text         = "ID : " + FacebookHandler.getInstance().UserId;
            _txtID.Commit();

        }
    }

}
