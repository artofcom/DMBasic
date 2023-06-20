using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using Spine;

public class BlockSugared : Block
{
	public override void ChangeBlockColor(int index, int type) 
	{
		base.ChangeBlockColor(index,type);
		
		//if(SA != null)
		{
		//	Spine.Animation ani = SA.skeleton.Data.FindAnimation("shiny");
		//	if(ani != null)
		//		SA.AnimationState.SetAnimation(1, "shiny", true);
		}
	}
}
