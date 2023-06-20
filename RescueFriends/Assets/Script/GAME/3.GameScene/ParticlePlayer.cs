using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ParticlePlayer : NNRecycler {

    public ParticleSystem[] particleSystems;
    public ParticleSystem[] subParticleSystems;

    void Awake () {
        foreach (ParticleSystem p in particleSystems) {
            p.GetComponent<Renderer>().sortingLayerName = "Particle";
            //p.main.startColor
        }

        foreach (ParticleSystem sp in subParticleSystems) {
            sp.GetComponent<Renderer>().sortingLayerName = "Particle";
        }
    }

    public float Play (Vector3 position, float fForceAbandonTime=.0f)
    {
        transform.position      = position;// new Vector3(position.x, position.y, -500.0f);

        float duration = 0F;

        foreach (ParticleSystem p in particleSystems) {
            p.Play();
            duration = Mathf.Max(duration, p.main.duration);// + p.main.startLifetime);
        }

        duration                = fForceAbandonTime>0.0001f ? fForceAbandonTime : duration;
        Invoke("Recycle", duration);
        return 0.2f;// duration;
    }

    void Recycle () {
        transform.DOKill();
        NNPool.Abandon(gameObject);
    }
}

