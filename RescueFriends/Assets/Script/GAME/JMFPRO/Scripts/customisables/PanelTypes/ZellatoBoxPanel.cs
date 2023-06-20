using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// [ZELLATO_BOX]
public class ZellatoBoxPanel : PanelDefinition {

    const int maxLife           = 3;
    const int targetCount       = 3;

	protected override void OnPanelDestroy (BoardPanel bp) {
        //if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("stone_cracks");
	}
	
    protected override void OnHit (BoardPanel bp, bool isSpecialAttack) {

        //DestroyedCount++;
        //NNSoundHelper.Play("IFX_soft_bust");
    }

    void ResetMakingSpecialPieceStatus() {
        GM.makingSpecialPiece = false;
    }
    public override float ShowHitEffect (BoardPanel bp) { 
        //SpineEffect crash       = NNPool.GetItem<SpineEffect>("GelatoBlockerHit");        
        //crash.play("play", .0f);
        //crash.Reset(bp.Owner.Position);
        //NNSoundHelper.Play("IFX_syrup_eff");

        // note : 이미지 처리를 위해 반드시 0으로 반환 필요 !
        return .0f;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }// false; } // ----
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
	public override bool IsShufflable (BoardPanel bp) { return false; }

	public override string GetImageName (int type) 
	{
        if (type >= 0 && type < maxLife)
            return string.Format("GelatoboxL{0}", maxLife - type);
        
		return null;
	}
}
