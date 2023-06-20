using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [ROUND_CHOCO]
public class CookieJellyPiece : PieceDefinition {
	public override int GetColorIndex () {return 0;}

    public static bool DestroyedThisTurn { get; private set; }
    //public static bool FactoryLevel;

	public override string GetImageName (int colorIndex) 
	{
		if(colorIndex > -1 && colorIndex < 3)
			return string.Format("RoundchocoL{0}",colorIndex +1);

		return null;
	}
	
	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        DestroyedThisTurn = true;
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY)
            NNSoundHelper.Play("IFX_soft_bust");

        return destroyScore;
	}

    protected override void OnPieceDestroyed (Board bd, int prevColor)
    {
        // misstion 처리 필요.
        if( JMFUtils.GM.CurrentLevel.countCookieJelly > 0)
        {
            JMFUtils.GM.countMatchCookieJelly++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countCookieJelly - JMFUtils.GM.countMatchCookieJelly;
            if(remainCount >= 0)
            {
				JMFUtils.GM.AnimationMissionObjectWithSprite(bd, remainCount, "CookieJelly", string.Format("cookie_jelly{0}", bd.Piece.CookieJellyType+1));
            }
        }
    }

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is CookieJellyPiece))
            return false;
             
        if (JMFUtils.GM.CurrentLevel.countCookieJelly > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.countCookieJelly - JMFUtils.GM.countMatchCookieJelly;
            return (remainCount>=0);
        }
        return false;
    }

    public override float ShowDestroyEffect (GamePiece gp) {
        // no effect here !!!
        /*
        SpineEffect effect      = NNPool.GetItem<SpineEffect>("JellyCookieEffect");
		Vector3 pos             = gp.GO.transform.position;
		pos.z -= 2.0f;
        effect.play( string.Format("cookie_level{0}_hit", gp.Durability+1), .0f);
        effect.Reset(pos);
        effect.transform.parent     = gp.GO.transform.parent;
        effect.transform.localScale = gp.GO.transform.localScale;
        
        if(0 < gp.Durability)
        {
            gp.Play("hit_blending", false);  // 0.2F;
            return 0.35f;
        }
        else*/
            return .0f;
    }
    public override bool isShowDestroyAnimation()
    {
        return false;
    }

    public override bool IsSwitchableHorizontal (GamePiece gp)
    {
        return false;
    }
	public override bool IsSwitchableVertical (GamePiece gp)
    {
        return false;
    }
}
