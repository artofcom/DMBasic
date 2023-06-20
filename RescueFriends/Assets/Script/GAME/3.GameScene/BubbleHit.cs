using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class BubbleHit : ParticlePlayer 
{	
	public float Play ( Vector3 position,Vector3 scale, int _index, bool loop, float delay = 0.0f, float timeScale = 1.0f) 
	{
        position += (Vector3.forward * -10.0f);
	    return base.Play(position);
	}
}
