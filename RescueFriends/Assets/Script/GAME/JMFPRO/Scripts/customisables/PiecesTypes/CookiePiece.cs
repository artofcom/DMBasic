using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [ROUND_CHOCO]
public class CookiePiece : PieceDefinition {
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
