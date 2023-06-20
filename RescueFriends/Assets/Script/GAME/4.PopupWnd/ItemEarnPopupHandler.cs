using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;

public class ItemEarnPopupHandler : BasePopupHandler 
{
    public class Info
    {
        public string strPicName;
        public int count;
    };
    public tk2dUIItem _btnClose = null;
    public tk2dSprite _sprItem  = null;
    public tk2dTextMesh _txtCnt = null;

    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);

        Info itemInfo           = param as Info;

        _sprItem.spriteName     = itemInfo.strPicName;
        _txtCnt.text            = string.Format("x {0} Earned!", itemInfo.count);
        _txtCnt.Commit();
    }
	
	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		
		if(_btnClose == item)
			Scene.ClosePopup();
	}
}
