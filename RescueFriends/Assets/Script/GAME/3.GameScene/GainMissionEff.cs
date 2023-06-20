using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using System;
using DG.Tweening;

// 일반적인 피스, 블러커 모을때.
// gain_mission_action 
// play - 'play_L'
public class GainMissionEff : SpineEffect 
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
	public float Play(string spriteName, bool dirLeft,  Vector3 start, Vector3 end,  bool loop, float delay = 0.0f) 
	{
        // init.
		Reset(Vector3.zero, -65.0f);
	/*	SA.enabled                      = true;

        // adjust path.
        PathAttachment attSrc   = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("chocobar"), "chocobar") as PathAttachment;
		PathAttachment attRet   = JMFUtils.CreateBezierPointsForSpine(attSrc, start, end);
		Slot _slot              = SA.Skeleton.FindSlot("chocobar");
		if(null!=_slot && null!=attRet)
            _slot.Attachment    = attRet;
		
        // update texture.
        _slot                   = SA.Skeleton.FindSlot("Panel_chocobar");
        Attachment att          = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("Panel_chocobar"), spriteName );
        if(null!= att)          _slot.attachment    = att;

        string strAniName       = "play_L"; // dirLeft ? "play_L" : "play_R";
		Spine.Animation ani     = SA.skeleton.Data.FindAnimation(strAniName);
		if(ani != null)
		{
			float d             = ani.Duration;
			TrackEntry trEntry  = SA.AnimationState.SetAnimation(0, strAniName, loop);
			trEntry.Delay       = delay;
			bRemove             = !loop;

            // trigger particle.
		    if(PS != null)
		    {
                PS.Stop();
                PS.transform.position   = end;
			    Sequence seq    = DOTween.Sequence();
			    seq.AppendInterval(d);
			    seq.OnComplete(()=>{ PS.Play();});
			    seq.SetId(PS.GetInstanceID());
		    }
			return d;
		}
        */
		return 0.0f;
	}
}


/*

    proper sprite map name. - bubble_move_eff.json.txt

           "Dough": { "width": 190, "height": 190 },
			"GreenbubbleL1": { "width": 190, "height": 190 },
			"RoundchocoL1": { "width": 190, "height": 190 },
			"Strawberry": { "width": 148, "height": 148 },
			"block_blue": { "width": 148, "height": 148 },
			"block_blue_S": { "width": 148, "height": 148 },
			"block_green": { "width": 148, "height": 148 },
			"block_green_S": { "width": 148, "height": 148 },
			"block_orange": { "width": 148, "height": 148 },
			"block_orange_S": { "width": 148, "height": 148 },
			"block_purple": { "width": 148, "height": 148 },
			"block_purple_S": { "width": 148, "height": 148 },
			"block_red": { "width": 148, "height": 148 },
			"block_red_S": { "width": 148, "height": 148 },
			"block_skyblue": { "width": 148, "height": 148 },
			"block_skyblue_S": { "width": 148, "height": 148 },
			"block_violet": { "width": 148, "height": 148 },
			"block_violet_S": { "width": 148, "height": 148 },
			"block_yellow": { "width": 148, "height": 148 },
			"block_yellow_S": { "width": 148, "height": 148 },
			"obstacle_bgstone_01": { "width": 190, "height": 190 },
			"obstacle_jam": { "width": 190, "height": 190 },
			"obstacle_monster_01": { "width": 190, "height": 190 },
			"obstacle_wood_01": { "width": 190, "height": 190 }*/