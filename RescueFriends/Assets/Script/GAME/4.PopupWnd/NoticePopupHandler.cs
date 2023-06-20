using UnityEngine;
using System.Collections;
using System;
using Spine.Unity;
using Spine;


public class NoticePopupHandler : BasePopupHandler
{
	public enum ERROR_TYPE { NONE = -1, ERR_0 = 0, ERR_55, ERR_60, ERR_INVITE, ERR_FBLOIN, ERR_NETWORK}
	public enum BUTTON_TYPE { NONE = 0 , OK, OK_CANCEL, OK_CANCEL_CLOSE, CLOSE, OK_CLOSE, OK_RETRY }
	const string TITLE_SORRY = "Sorry!";
	const string TITLE_INVITE = "Invite";
	const string TITLE_LOGIN = "Login";
	const string TITLE_0 = "Connection Failed";
	const string TITLE_1 = "Notice";
	const string ERROR_0 = "Please check your internet\nconnection and retry";
	const string ERROR_55 = "Entered {0} account has\nalready been registered.";
	const string ERROR_60 = "Entered {0} account has\nalready been registered.";
	const string ERROR_SORRY = "Please restart!";
	
	public tk2dUIItem OKBTN;
	public tk2dUIItem CANCELBTN;
	public tk2dUIItem closeBTN;
	public tk2dUIItem backBTN;
    public tk2dUIItem RetryBTN;
	public TextMesh Comment;
	public TextMesh Title;
    public tk2dBaseSprite TitleBG;
	
	ButtonAnimation[] BTNAni = {null,null,null, null};

    ERROR_TYPE mErrorType   = ERROR_TYPE.NONE;
    public ERROR_TYPE getErrorType() { return mErrorType; }
    BUTTON_TYPE mBtnType    = BUTTON_TYPE.NONE;
    public BUTTON_TYPE getBtnType() { return mBtnType; }

	public Action<object> OK_Callback;
	public Action<object> CANCEL_Callback;
	public Action<object> CLOSE_Callback;
    public Action<object> RETRY_Callback;
	object Param;
	
	protected override void Start() 
	{
		base.Start();
		if(OKBTN != null)
			BTNAni[0] = OKBTN.GetComponent<ButtonAnimation>();
		if(closeBTN != null)
			BTNAni[1] = closeBTN.GetComponent<ButtonAnimation>();
		if(CANCELBTN != null)
			BTNAni[2] = CANCELBTN.GetComponent<ButtonAnimation>();
        if(RetryBTN != null)
			BTNAni[3] = RetryBTN.GetComponent<ButtonAnimation>();
	}

	public void  SetErrorCode (ERROR_TYPE error,string messeage="") 
	{
        mErrorType              = error;

		switch(error)
		{
			case ERROR_TYPE.ERR_55:
				{
					Title.text = TITLE_1;
					Comment.text = string.Format(ERROR_55, messeage);
				}
			break;
			case ERROR_TYPE.ERR_60:
				{
					Title.text = TITLE_1;
					Comment.text = string.Format(ERROR_60, messeage);
				}
			break;
			case ERROR_TYPE.ERR_0:
				{
					Title.text = TITLE_0;
					Comment.text = ERROR_0;
				}
			break;
			case ERROR_TYPE.ERR_NETWORK:
				{
					Title.text = TITLE_SORRY;
					Comment.text = ERROR_SORRY;
				}
			break;
			case ERROR_TYPE.ERR_INVITE:
				{
					Title.text = TITLE_INVITE;
					Comment.text = messeage;
				}
			break;
			case ERROR_TYPE.ERR_FBLOIN:
				{
					Title.text = TITLE_LOGIN;
					Comment.text = messeage;
				} 
			break;
            default:
                Title.text = TITLE_SORRY;
                Comment.text = messeage;
                break;
		}
	}

    public void SetTitleBG(string _name)
    {
        if (TitleBG != null)
            TitleBG.SetSprite(_name);
    }

    public void  SetErrorCode (string title,string messeage) 
    {
        Title.text = title;
        Comment.text = messeage;
    }

    public void SetTextOKButton(string _text = "OK")
    {
        tk2dTextMesh tx = OKBTN.transform.Find("text").GetComponent<tk2dTextMesh>();
        if (tx != null)
        {
            tx.text = _text;    
            tx.Commit();
        }
    }

    public void SetTextCancelButton(string _text = "Cancel")
    {
        tk2dTextMesh tx = CANCELBTN.transform.Find("text").GetComponent<tk2dTextMesh>();
        if (tx != null)
        {
            tx.text = _text;    
            tx.Commit();
        }
    }
	
	public void  InitButton (BUTTON_TYPE type) 
	{
		bool[] active = { false, false, false,false, false };
		switch(type)
		{
			case BUTTON_TYPE.CLOSE:
				active[2] = true;
			break;
			case BUTTON_TYPE.OK:
				active[0] = true;
			break;
			case BUTTON_TYPE.OK_CANCEL:
				active[0] = true;
				active[1] = true;
			break;
			case BUTTON_TYPE.OK_CANCEL_CLOSE:
				active[0] = true;
				active[1] = true;
				active[2] = true;
			break;
			case BUTTON_TYPE.OK_CLOSE:
				active[0] = true;
				active[2] = true;
			break;
            case BUTTON_TYPE.OK_RETRY:
                active[0] = true;
				active[4] = true;
			break;
			default:
				active[3] = true;
			break;
		}
		
        mBtnType                = type;

		if(OKBTN != null) OKBTN.gameObject.SetActive(active[0]);
		if(CANCELBTN != null) CANCELBTN.gameObject.SetActive(active[1]);
		if(closeBTN != null) closeBTN.gameObject.SetActive(active[2]);
		if(backBTN != null) backBTN.gameObject.SetActive(active[3]);
        if(RetryBTN != null) RetryBTN.gameObject.SetActive(active[4]);
	}
	
	protected override void  OnEnter (object param) 
	{
		base.OnEnter(param);
	}

	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(entry.Animation.Name == "show")
		{
			Scene.UnlockWithMsg();
			
			for(int i = 0; i < BTNAni.Length; ++i)
			{
				if(BTNAni[i] != null)
					BTNAni[i].CLICK();
			}
			
			return;
		}

		if(entry.Animation.Name == "hide")
		{
			gameObject.SetActive(false);
		}
	}

	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(OKBTN == item )
		{
			if(OK_Callback != null)
				OK_Callback(Param);
		}
		
		if(CANCELBTN == item )
		{
			if(CANCEL_Callback != null)
				CANCEL_Callback(Param);
		}

        if(RetryBTN == item )
		{
			if(RETRY_Callback != null)
				RETRY_Callback(Param);
		}

		if(closeBTN == item || backBTN == item)
		{
			if(CLOSE_Callback != null)
				CLOSE_Callback(Param);
		}
		
		OnEscape();
		OK_Callback = null;
		CANCEL_Callback = null;
		CLOSE_Callback = null;
        RETRY_Callback = null;
        Param = null;
        mErrorType = ERROR_TYPE.NONE;
        mBtnType = BUTTON_TYPE.NONE;
	}
}
