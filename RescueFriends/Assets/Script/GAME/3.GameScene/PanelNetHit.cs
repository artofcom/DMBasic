using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class PanelNetHit : ParticlePlayer
{
	protected string[] typeName = {"Shockwave", "bubble"};
	
	public void ChangeColor(int type, int colorIndex)
	{
		int slotIndex;
	/*	for(int i = 0; i < SA.Skeleton.Slots.Count; ++i)
		{
			Slot _slot = SA.Skeleton.FindSlot(string.Format("bubble_violet{0}", i));
			slotIndex = SA.Skeleton.FindSlotIndex(_slot.Data.Name);
			Attachment attachment = SA.Skeleton.GetAttachment(slotIndex, string.Format("{0}{1}",  typeName[type], blockColorName[colorIndex]));
			_slot.Attachment = attachment;	
		}*/
	}
	
	public float Play ( Vector3 position, Vector3 scale, bool loop, float delay = 0.0f, float timeScale = 1.0f, Transform parent=null, string strSkinName=null) 
	{
        position += (Vector3.forward * -10.0f);

        return base.Play(position);
		//Reset(position);
	/*	SA.enabled = true;

        NNSoundHelper.Play("IFX_mesh_clear");

        // update texture.
        if(null != strSkinName)
        {
            const int cntBubble = 13;
            for(int z = 0 ; z < cntBubble; ++z)
            {
                string slotName = string.Format("bubble_violet{0}", z);
                Slot slot       = SA.Skeleton.FindSlot( slotName );
                if(null==slot)  continue;
                Attachment att  = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex(slotName), strSkinName );
                if(null!= att)  slot.attachment    = att;
            }
        }

		Spine.Animation ani = SA.skeleton.Data.FindAnimation("play");
		if(ani != null)
		{
			float d = ani.Duration;
			TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, ani, loop);
			_trackEntry.Delay = delay;
			_trackEntry.TimeScale= timeScale;
			SA.transform.localScale = scale;
            if(null!=parent)    SA.transform.SetParent( parent );
			bRemove = !loop;
			return d;
		}
        */
		//return 0.0f;
	}
}
