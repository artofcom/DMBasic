using UnityEngine;
using System.Collections;
using Spine;

public class Potion1 : Block 
{
    public static int TotalCount { get; private set; }

    void OnEnable () {
        TotalCount++;
    }

    void OnDisable () {
        TotalCount--;
    }
	
	public override void ChangeBlockColor(int index, int type) 
	{
		//if(SA != null)
		//{
		//	Spine.Animation ani = SA.skeleton.Data.FindAnimation("normalidle");
		////	if(ani != null)
		//		SA.AnimationState.SetAnimation(1,ani, true);
		//}
	}
}
