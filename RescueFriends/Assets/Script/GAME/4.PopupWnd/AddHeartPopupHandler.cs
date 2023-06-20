using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AddHeartPopupHandler : BasePopupHandler 
{
    public tk2dUIItem _btnClose = null;

    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);
        _refreshUI();
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnClose == item)
			Scene.ClosePopup();
	}

    void _refreshUI()
    {
        // init cell items by data.

        // 각도와 대조해서 item 맞추는 기능 필요.
        //_sprWheel.transform.localEulerAngles = Vector3.zero;
    }

}
