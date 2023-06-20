using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using Spine.Unity;
using Spine;
using DG.Tweening;

public class SpinReadyPopupHandler : BasePopupHandler
{   
	public tk2dSprite          _txtPic = null;
	
    
	protected override void  OnEnter (object param) 
	{
		base.OnEnter(param);
        _txtPic.spriteName      = "big_text_ready";
	}

    protected override void OnEnterFinished()
    {
        StartCoroutine(_coCounter());
    }

    IEnumerator _coCounter()
    {
        int counter             = 4;
        do
        {
            yield return new WaitForSeconds(1.0f);
            --counter;

            if(counter < 0)    break;

            if(0 == counter)
                _txtPic.spriteName  = "big_text_go";
            else
                _txtPic.spriteName  = string.Format("num_{0}", counter);

        }while(true);

        Scene.ClosePopup();
    }
}
