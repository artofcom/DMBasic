using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

[RequireComponent(typeof(ParticleSystem))]
public class BubbleParticle : NNRecycler
{
    public AtlasAsset           _atlasHolder;

    ParticleSystem _ps;

    void Awake ()
	{
        _ps                     = GetComponent<ParticleSystem>();
    }

    public void play(string strMapName)
    {
        if(null==_atlasHolder || false==_atlasHolder.IsLoaded)
            return;
        if(null == _ps)         return;

        Atlas atlas             = _atlasHolder.GetAtlas();
        if(null==atlas)         return;
        AtlasRegion region      = atlas.FindRegion(strMapName);
        if(null == region)      return;

        ParticleSystemRenderer  particleRdr     = _ps.GetComponent<ParticleSystemRenderer>();
        particleRdr.material    = (Material)region.page.rendererObject;
        particleRdr.material.mainTextureOffset  = new Vector2(region.u, region.v);
        particleRdr.material.mainTextureScale   = new Vector2(region.u2-region.u, region.v2-region.v);

        _ps.Play();
    }
    
}
