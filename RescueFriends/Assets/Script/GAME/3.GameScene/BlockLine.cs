using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class BlockLine : ParticlePlayer 
{
	public float Play ( string aniName, Vector3 position,Vector3 scale, int _index, bool loop, float delay = 0.0f) 
	{
        return base.Play(position);
	}
}
