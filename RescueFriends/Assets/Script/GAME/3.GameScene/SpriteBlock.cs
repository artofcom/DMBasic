using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using Spine;

public class SpriteBlock : Block
{
	public Sprite[] sprLinks    = null;

    public Sprite getSpriteByName(string strName)
    {
        if(null==sprLinks)      return null;

        for(int j = 0; j < sprLinks.Length; ++j)
        {
            if(strName.Equals( sprLinks[j].name))
                return sprLinks[j];
        }
        return null;
    }
}
