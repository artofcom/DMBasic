using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using NOVNINE;

public class QuitPopupHandler : BasePopupHandler
{
	public tk2dUIItem yesBTN;
	public tk2dUIItem noBTN;

	ButtonAnimation[] BTNAni = new ButtonAnimation[2];
	
	protected override void Awake () 
	{
		base.Awake();
	}

	protected override void Start () 
	{
		base.Start();
		if(yesBTN != null)
			BTNAni[0] = yesBTN.GetComponent<ButtonAnimation>();
		if(noBTN != null)
			BTNAni[1] = noBTN.GetComponent<ButtonAnimation>();
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
			for(int i = 0; i < BTNAni.Length; ++i)
			{
				BTNAni[i].CLICK();
			}
		}

		if(entry.Animation.Name == "hide")
		{
			gameObject.SetActive(false);
		}
	}

	protected override void OnLeave()
	{
		base.OnLeave();
	}

	public override void CLICK (tk2dUIItem item) 
	{
		base.CLICK(item);
		if(yesBTN == item )
		{
			NativeInterface.ApplicationQuit();
			return;
		}

		if(noBTN == item)
		{
			OnEscape();
			return;
		}
	}
}
