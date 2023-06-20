using UnityEngine;
using System.Collections;
using System;
using Spine.Unity;
using Spine;


public class InfoPopupHandler : BasePopupHandler
{
	public tk2dUIItem OKBTN;
	ButtonAnimation BTNAni;

	protected override void Start() 
	{
		base.Start();
		if(OKBTN != null)
			BTNAni = OKBTN.GetComponent<ButtonAnimation>();
	}

	protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(entry.Animation.Name == "show")
		{
            BTNAni.CLICK();			
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
        OnEscape();
	}
}
