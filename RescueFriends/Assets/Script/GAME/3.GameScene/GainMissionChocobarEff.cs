using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using System;
using DG.Tweening;

// 초코바와 물약(Potion) 만 !!!
// 
public class GainMissionChocobarEff : SpineEffect 
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
        const float fOffset     = 0.8f;
        Vector3 vMidPtrOffset   = new Vector3(-fOffset, fOffset);   // dirLeft ? new Vector3(-fOffset, fOffset) : new Vector3(fOffset, fOffset);
        Vector3 midPos          = end + vMidPtrOffset;
        PathAttachment attSrc   = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("chocobar"), "chocobar") as PathAttachment;
		PathAttachment attRet   = JMFUtils.CreateBezierPointsForSpine(attSrc, start, midPos);
		Slot _slot              = SA.Skeleton.FindSlot("chocobar");
		if(null!=_slot && null!=attRet)
            _slot.Attachment    = attRet;	
        attSrc                  = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("bubblee"), "bubblee") as PathAttachment;
		attRet                  = JMFUtils.CreateBezierPointsForSpine(attSrc, midPos, end);
		_slot                   = SA.Skeleton.FindSlot("bubblee");
        if(null!=_slot && null!=attRet)
            _slot.Attachment    = attRet;	
		
        // update texture.
        _slot                   = SA.Skeleton.FindSlot("Panel_chocobar");
        Attachment att          = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("Panel_chocobar"), spriteName );
        if(null!= att)          _slot.attachment    = att;

        string strAniName       = dirLeft ? "play_L" : "play_R";
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
		}*/
        
		return 0.0f;
	}
}


/*

    proper sprite map name. - bubble_move_eff.json.txt

           "Panel_chocobar1x1": { "width": 136, "height": 142 },
			"Panel_chocobar1x3": { "width": 139, "height": 411 },
			"Panel_chocobar2x2": { "width": 277, "height": 287 },
			"Panel_chocobar2x3": { "width": 281, "height": 419 },
			"Panel_chocobar2x4": { "width": 277, "height": 552 },
			"Panel_chocobar3x1": { "width": 419, "height": 138 },
			"Panel_chocobar3x2": { "width": 434, "height": 266 },
			"Panel_chocobar4x2": { "width": 566, "height": 273 },
			"potion_1": { "width": 191, "height": 190 },
			"potion_2": { "width": 191, "height": 190 },
			"potion_3": { "width": 191, "height": 190 }*/