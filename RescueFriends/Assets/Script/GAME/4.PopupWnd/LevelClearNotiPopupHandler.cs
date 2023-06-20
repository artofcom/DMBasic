using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using NOVNINE;
//using NOVNINE.Diagnostics;
using DG.Tweening;

public class LevelClearNotiPopupHandler : BasePopupHandler 
{
    protected override void  OnEnter (object param) 
	{	
		base.OnEnter(param);
     
        DOVirtual.DelayedCall(0.3f, () =>
        {
            NNSoundHelper.Play("VFX_cascade");
        });

        DOVirtual.DelayedCall(1.5f, () => Scene.ClosePopup());
    }
	
}
