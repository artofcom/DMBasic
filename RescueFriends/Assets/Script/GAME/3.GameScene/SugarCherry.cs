using UnityEngine;
using System.Collections;
using Spine;
//using Holoville.HOTween;
//using DG.Tweening;
//using NOVNINE.Diagnostics;

public class SugarCherry : Block {
    
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
		//if(SA != null )
		//{
		//	int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");

		//	Slot _slot = SA.Skeleton.FindSlot("block_normal");
		//	_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, "SugarCherry");
		//}		
	}

    public float Play(string animationName, bool loop, int idxTrack, float delay=.0f, System.Action onComplete = null) 
	{
		/*if(SA != null)
		{
            _onComplete = onComplete;
			Spine.Animation ani = SA.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				float d = ani.Duration;
				TrackEntry _trackEntry = SA.AnimationState.SetAnimation(idxTrack, ani, loop);
                _trackEntry.Delay = delay;
                _trackEntry.timeScale   = 1.0f; // confirm time scale.

				if(loop)
					return -1;
				else
					return d;
			}
		}*/
		
		return -2;
	}
}
