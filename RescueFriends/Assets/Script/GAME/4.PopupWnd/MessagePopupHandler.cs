using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;

public class MessagePopupHandler : BasePopupHandler 
{
    public enum RET { OK, CALCEL  };
    public enum EMOTIONS { SAD, HAPPY, CRY, ON_QUIT  }
    public class Data
    {
        public string strMessage;
        public bool isOkOnly;
        public EMOTIONS emotion;
    };

    public Transform    _trOkGroup;
    public Transform    _trOkCancelGroup;
    public tk2dTextMesh _txtMessage;
    public Transform[]  _arrTrFaces = null;

    // Temp Value for TitleScene.
    public static       TitleOverlay _titleOverlay = null;

    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);
        
        Data myData             = param as Data;
        _trOkGroup.gameObject.SetActive( myData.isOkOnly );
        _trOkCancelGroup.gameObject.SetActive( !myData.isOkOnly );

        for(int z = 0 ; z < _arrTrFaces.Length; ++z)
        {
            _arrTrFaces[z].gameObject.SetActive( (int)myData.emotion==z );
        }

        _txtMessage.text        = myData.strMessage;
        _txtMessage.Commit();
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		//if(_btnClose == item)
		//	Scene.ClosePopup();
	}

    void onClickOK(tk2dUIItem item)
    {
        Scene.ClosePopup( RET.OK );

        if(null != _titleOverlay)
            _titleOverlay.onCloseErrorPopup();
    }
    void onClickCancel(tk2dUIItem item)
    {
        Scene.ClosePopup( RET.CALCEL );
    }

}
