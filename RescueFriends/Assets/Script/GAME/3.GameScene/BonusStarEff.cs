using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using System;
using DG.Tweening;

public class BonusStarEff : SpineEffect 
{
	public	ParticleSystem PS = null;
	
	protected override void OnComplete (TrackEntry entry)
	{
		if(PS != null)
			PS.Stop();
			
		base.OnComplete(entry);
	}
	
	protected override void Recycle () 
	{
		if(PS != null)
			DOTween.Kill(PS.GetInstanceID());
		
		base.Recycle();
	}
	
	protected override void OnEvent (TrackEntry entry, Spine.Event e)
	{
		//if(e.Data.Name == "tailChange")
		//{
		//}
	}

    // spriteName : 하단에 적절한 sprite name 목록 참조.
	public float Play(Vector3 start, Vector3 end)
	{
        // init.
		Reset(Vector3.zero, -65.0f);
	/*	SA.enabled              = true;

        // adjust path.
        PathAttachment attSrc   = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("star_move01"), "star_move01") as PathAttachment;
		PathAttachment attRet   = JMFUtils.CreateLineForSpine(attSrc, start, end);
		Slot _slot              = SA.Skeleton.FindSlot("star_move01");
		if(null!=_slot && null!=attRet)
            _slot.Attachment    = attRet;

        string strAniName       = "star_move01";
		Spine.Animation ani     = SA.skeleton.Data.FindAnimation(strAniName);
		if(ani != null)
		{
			float d             = ani.Duration;
			TrackEntry trEntry  = SA.AnimationState.SetAnimation(0, strAniName, false);
			trEntry.Delay       = .0f;
			bRemove             = true;
            trEntry.timeScale   = 2.0f;
			return (d / trEntry.timeScale);
		}
        */
		return 0.0f;
	}
}
