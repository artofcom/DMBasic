using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using System;
using DG.Tweening;

public class Tail : SpineEffect 
{
	string[] _blockColorName = {"_red", "_yellow", "_green", "_blue", "_purple", "_orange", "_skyblue", "_violet", "" };
	
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
		if(e.Data.Name == "tailChange")
		{
			string[] list = e.String.Split(',');
			
		/*	Slot _slot = SA.Skeleton.FindSlot(list[0]);
			int slotIndex = SA.Skeleton.FindSlotIndex(_slot.Data.Name);
			_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, string.Format("{0}{1}", list[0], _blockColorName[index]));
			_slot = SA.Skeleton.FindSlot(list[1]);
			slotIndex = SA.Skeleton.FindSlotIndex(_slot.Data.Name);
			_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, string.Format("{0}{1}", list[1], _blockColorName[index]));
            */
			return;
		}		
	}

	public float Play ( string aniName, Vector3 start, Vector3 end,  int _index, bool loop, float delay = 0.0f) 
	{
        // we don't use this for now.
        this.Recycle();
        return .0f;


		Reset(Vector3.zero);
	/*	SA.enabled = true;
		index = _index;
		PathAttachment attachment = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("path"), "path") as PathAttachment;
		PathAttachment _pathAttachment = JMFUtils.CreateBezierPointsForSpine(attachment,start, end);
		
		Slot _slot = SA.Skeleton.FindSlot("path");
		_slot.Attachment = _pathAttachment;	
		
		if(PS != null)
		{
			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(delay);
			seq.OnComplete(()=>{ PS.Play();});
		}
		
		Spine.Animation ani = SA.skeleton.Data.FindAnimation(aniName);
		if(ani != null)
		{
			float d = ani.Duration;
			TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, ani, loop);
			_trackEntry.Delay = delay;
			bRemove = !loop;
			return d;
		}
        */
		return 0.0f;
	}
}
