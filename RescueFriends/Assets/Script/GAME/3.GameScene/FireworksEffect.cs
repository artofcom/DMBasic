using UnityEngine;
using System.Collections;
using Spine;

public class FireworksEffect : SpineEffect
{
	protected override void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "FX")
		{
			NNSoundHelper.Play(e.String);
		}
	}

    protected override void OnComplete (TrackEntry entry)
    {
      /*  if (entry.Animation.Name == "hide")
            return;
        
        if(bRemove)
        {
            SA.enabled = false;
            Recycle();
        }*/
    }

    public void Hidelay()
    {
       /* if(SA != null && SA.enabled)
        {
            TrackEntry trEntry = SA.AnimationState.GetCurrent(0);
            if(trEntry.IsComplete == false)
                OnComplete(trEntry);
            
            SA.AnimationState.SetAnimation(1, "hide", false);
        }*/   
    }
        
	public float Play ( Vector3 start, Vector3 end, bool loop, float delay = 0.0f) 
	{
		Reset(Vector3.zero, -65.0f);
	/*	SA.enabled = true;
        SA.AnimationState.ClearTracks();

		Bone bone = SA.Skeleton.FindBone("star");
		float rot = UnityEngine.Random.Range(0, 5) * 60;
		bone.Rotation = rot;
		
		//bone.UpdateWorldTransform();
		
		PathAttachment attachment = SA.Skeleton.GetAttachment(SA.Skeleton.Data.FindSlotIndex("star_move01"), "star_move01") as PathAttachment;
		PathAttachment _pathAttachment = CreateBezierPointsForSpine(attachment,start, end);

		Slot _slot = SA.Skeleton.FindSlot("star_move01");
		_slot.Attachment = _pathAttachment;	

		Spine.Animation ani = SA.skeleton.Data.FindAnimation("play");
		if(ani != null)
		{
			float d = ani.Duration;
			TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, ani, loop);
			_trackEntry.Delay = delay;
			bRemove = !loop;

            Director.Instance.showMeshNextFrame( this.GetComponent<MeshRenderer>() );

			return d;
		}
        */
		return 0.0f;
	}
	
	PathAttachment CreateBezierPointsForSpine(PathAttachment pathAttachment, Vector3 start, Vector3 end )
	{		
		PathAttachment _pathAttachment = new PathAttachment(pathAttachment.Name);
		if(pathAttachment.Vertices != null)
		{
			_pathAttachment.Vertices = new float[pathAttachment.Vertices.Length];
			for(int i = 0; i < pathAttachment.Vertices.Length; ++i)
			{
				_pathAttachment.Vertices[i] = pathAttachment.Vertices[i];
			}	
		}

		if(pathAttachment.Lengths != null)
		{
			_pathAttachment.Lengths = new float[pathAttachment.Lengths.Length];
			for(int i = 0; i < pathAttachment.Lengths.Length; ++i)
			{
				_pathAttachment.Lengths[i] = pathAttachment.Lengths[i];
			}	
		}

		if(pathAttachment.Bones != null)
		{
			_pathAttachment.Bones = new int[pathAttachment.Bones.Length];
			for(int i = 0; i < pathAttachment.Bones.Length; ++i)
			{
				_pathAttachment.Bones[i] = pathAttachment.Bones[i];
			}	
		}

		_pathAttachment.Closed = pathAttachment.Closed;
		_pathAttachment.ConstantSpeed = pathAttachment.ConstantSpeed;
		_pathAttachment.WorldVerticesLength = pathAttachment.WorldVerticesLength;

		float XX = start.x - _pathAttachment.vertices[2];
		float YY = start.y - _pathAttachment.vertices[3];

		for(int i = 0; i < 3; ++i)
		{
			_pathAttachment.vertices[(i*2)] += XX;
			_pathAttachment.vertices[(i*2) + 1] += YY;
		}

		XX = end.x - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -4];
		YY = end.y - _pathAttachment.vertices[_pathAttachment.WorldVerticesLength -3];

		for(int i = 0; i < 3; ++i)
		{
			_pathAttachment.vertices[(i*2) + 6] += XX;
			_pathAttachment.vertices[(i*2) + 7] += YY;
		}
		return _pathAttachment;		
	}
}
