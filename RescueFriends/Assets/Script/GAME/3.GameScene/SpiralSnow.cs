using UnityEngine;
using System.Collections;
using Spine;

public class SpiralSnow : Block {

    public static int TotalCount { get; private set; }

	void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
	
	public override void ChangeBlockColor(int index, int type) 
	{
		///if(SA != null )
		//{
		//	int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");

		//	Slot _slot = SA.Skeleton.FindSlot("block_normal");
		//	_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, string.Format("RoundchocoL{0}",index +1));
		//}		
	}
}
