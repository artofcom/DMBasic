using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// [ZELLATO_BOX]
public class BlockMissionPanel : PanelDefinition
{

	protected override void OnPanelDestroy (BoardPanel bp)
    {
        //if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("stone_cracks");

        // de-init board.
        JMFUtils.GM.BlockMissionPanels[ bp.getBoardColor()-1 ] = null;
	}
	
    protected override void OnHit (BoardPanel bp, bool isSpecialAttack)
    {
        //DestroyedCount++;
        //NNSoundHelper.Play("IFX_soft_bust");
    }

    void ResetMakingSpecialPieceStatus() {
        GM.makingSpecialPiece = false;
    }
    public override float ShowHitEffect (BoardPanel bp)
    { 
        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = bp.Owner.Position;
        effect.Play(pos, Vector3.one, 1, false, 0.45f);

        // note : 이미지 처리를 위해 반드시 0으로 반환 필요 !
        return 0.0f;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return false; } // ----
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
	public override bool IsShufflable (BoardPanel bp) { return false; }
    public override bool IsNeedNumber() { return true; }

	public override string GetImageName (int color) 
	{     
        // COLOR.COLOR
		string strHead          = "";
        switch(color)
        {
        case 1: strHead         = "R";  break;
        case 2: strHead         = "Y";  break;
        case 3: strHead         = "G";  break;
        case 4: strHead         = "B";  break;
        case 5: strHead         = "P";  break;
        case 6: strHead         = "O";  break;
        case 7: strHead         = "M";  break;
        case 8: strHead         = "PP";  break;
        default: return null;
        }
        
        strHead = "lock_" + strHead;
        return strHead;
	}
}
