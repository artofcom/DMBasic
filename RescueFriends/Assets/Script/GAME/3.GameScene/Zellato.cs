using UnityEngine;
using System.Collections;
using Spine;
//using Holoville.HOTween;
//using DG.Tweening;
//using NOVNINE.Diagnostics;

public class Zellato : Block {
    
    public static int TotalCount { get; private set; }

    void OnEnable () {
        //JMFRelay.OnPlayerMove += OnPlayerMove;
        TotalCount++;
    }

    void OnDisable () {
        //JMFRelay.OnPlayerMove -= OnPlayerMove;
        TotalCount--;
    }
	
	public override void ChangeBlockColor(int index, int type) 
	{
	/*	if(SA != null )
		{
			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");

			Slot _slot = SA.Skeleton.FindSlot("block_normal");
			_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, "Gelato");
		}*/		
	}
}
