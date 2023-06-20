using UnityEngine;
using System.Collections;
using Spine.Unity;
using Spine;

public class BlockCrash : ParticlePlayer 
{
    public Color[] minStartColors;
    public Color[] maxStartColors;

    public float Play ( string aniName, Vector3 position,Vector3 scale, int _index, float fAbandonTime=.0f) 
	{
        //Debug.Assert(maxStartColors.Length==maxStartColors.Length && maxStartColors.Length==(int)LEItem.COLOR.NORMAL_COUNT);

        position += (Vector3.forward * -10.0f);

        if((int)LEItem.COLOR.NORMAL_COUNT!=maxStartColors.Length || (int)LEItem.COLOR.NORMAL_COUNT!=minStartColors.Length)
        {
            return base.Play(position);
        }

        if(_index<0 || _index>=minStartColors.Length)
            _index              = 0;

        foreach (ParticleSystem p in particleSystems)
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor     = new ParticleSystem.MinMaxGradient(minStartColors[_index], maxStartColors[_index]);
        }
        foreach (ParticleSystem p in subParticleSystems)
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor     = new ParticleSystem.MinMaxGradient(minStartColors[_index], maxStartColors[_index]);
        }

        /*Transform trParticle    = transform.Find("Crash");
        ParticleSystem partMain = trParticle.GetComponent<ParticleSystem>();
        if(null == partMain)    return .0f;

        ParticleSystem.MainModule main = partMain.main;
        main.startColor         = new ParticleSystem.MinMaxGradient(minStartColors[_index], maxStartColors[_index]);
        trParticle              = trParticle.Find("Star");
        partMain                = trParticle.GetComponent<ParticleSystem>();
        if(null == partMain)    return .0f;

        main                    = partMain.main;
        main.startColor         = new ParticleSystem.MinMaxGradient(minStartColors[_index], maxStartColors[_index]);
        */
		return base.Play(position, fAbandonTime);
	}
}
