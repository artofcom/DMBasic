using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class BlockRainbow : Block
{
	
	/*protected override void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "destroy_block")
		{
//			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");
//			Slot _slot = SA.Skeleton.FindSlot("block_normal");
//			Slot _slot1 = SA.Skeleton.FindSlot(e.String);
//            if(null != _slot.Attachment)
//			    _slot1.Attachment = SA.Skeleton.GetAttachment(slotIndex, _slot.Attachment.Name);	
//			return;
		}
	}*/
	
	public override void ChangeBlockColor(int index, int type) 
	{		
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
