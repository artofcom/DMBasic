using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class BombHit : ParticlePlayer 
{
    public Color[] startColors;

    public float Play ( Vector3 position,Vector3 scale, int _index, bool loop, float delay = 0.0f, float timeScale = 1.0f) 
	{
        position += (Vector3.forward * -10.0f);

        if((int)LEItem.COLOR.NORMAL_COUNT != startColors.Length)
        {
            return base.Play(position);
        }

        if(_index<0 || _index>=startColors.Length)
            _index              = 0;

        ParticleSystem partMain = transform.GetComponent<ParticleSystem>();
        if(null == partMain)    return .0f;

        ParticleSystem.MainModule main = partMain.main;
        main.startColor         = startColors[_index];

        return base.Play(position);
	}
	
}
