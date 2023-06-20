using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using Spine.Unity;
using Spine;

public class LoadingPopupHandler : BasePopupHandler
{   
	public tk2dSprite _sprLoading;
	
	protected override void  OnEnter (object param) 
	{
		base.OnEnter(param);

        StartCoroutine( _loading() );
	}

    IEnumerator _loading()
    {
        float fElTime           = .0f;
        int rot                 = 0;
        _sprLoading.gameObject.SetActive(true);
        do
        {
            _sprLoading.transform.localEulerAngles = new Vector3(0, 0, rot);
            yield return new WaitForSeconds(0.2f);

            rot -= 30;          rot %= 360;
            fElTime += 0.2f;

        }while(true);
    }


	
}
