using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
//using Spine.Unity;
//using Spine;

public class BlockBomb : Block
{
	string[] _blockColorName = {"_red", "_yellow", "_green", "_blue", "_purple", "_orange", "_skyblue", "_violet" };
	
	protected override void Awake ()
	{
		base.Awake();
    }

	/*protected override void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "change")
		{
			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");
			Slot _slot = SA.Skeleton.FindSlot("block_normal");
			Slot _slot1 = SA.Skeleton.FindSlot(e.String);
            if(null != _slot.Attachment)
			    _slot1.Attachment = SA.Skeleton.GetAttachment(slotIndex, _slot.Attachment.Name);	
			return;
		}
	}*/
	
	public override void ChangeBlockColor(int index, int type) 
	{
		base.ChangeBlockColor(index,type);
		//if(SA != null && _blockColorName.Length > index )
		//{
		//	int slotIndex = SA.Skeleton.FindSlotIndex("swirl");
		//	Slot _slot = SA.Skeleton.FindSlot("swirl");
		//	_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, string.Format("swirl{0}",_blockColorName[index]));
			
		//	Spine.Animation ani = SA.skeleton.Data.FindAnimation("swirl");
		//	if(ani != null)
		//		SA.AnimationState.SetAnimation(1, ani, true);
		//}
	}
	
	/*protected override void OnComplete (TrackEntry entry)
	{
		base.OnComplete(entry);

		if(!entry.Loop)
		{
			if(entry.Animation.Name == "blink" || entry.Animation.Name == "destroy" || entry.Animation.Name == "normal" )
			{
			}
			else
				Play("normal_idle", true);		
		}

	}*/
}
