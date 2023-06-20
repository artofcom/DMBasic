using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using System;
using DG.Tweening;

public class LightningTail : SpineEffect 
{
	public float Play ( string aniName, Vector3 start, Vector3 end,  int _index, bool loop, float delay = 0.0f) 
	{
		Reset(Vector3.zero, -65.0f);
	/*	SA.enabled = true;
		index = _index;
		PathAttachment attachment = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("path"), "path") as PathAttachment;
		PathAttachment _pathAttachment = JMFUtils.CreateBezierPointsForSpine(attachment,start, end);
		
		Slot _slot = SA.Skeleton.FindSlot("path");
		_slot.Attachment = _pathAttachment;	
		
		Spine.Animation ani = SA.skeleton.Data.FindAnimation(aniName);
		if(ani != null)
		{
			float d = ani.Duration;
			TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, aniName, loop);
			_trackEntry.Delay = delay;
			bRemove = !loop;
			return d;
		}
        */
		return 0.0f;
	}
}
