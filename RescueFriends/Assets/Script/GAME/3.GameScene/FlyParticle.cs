using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;
using DG.Tweening;

public class FlyParticle : ParticlePlayer 
{
    public float Play(Transform trParent, Vector3 vFrom, Vector3 vTo, float flyTime) 
	{
        if (null != trParent)
            this.transform.SetParent( trParent, false );

		base.Play(vFrom);

        this.transform.DOLocalMove(vTo, flyTime);

        return flyTime;
	}
}
