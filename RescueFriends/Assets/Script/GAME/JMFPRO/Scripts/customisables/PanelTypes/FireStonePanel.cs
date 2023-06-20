using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// [ZELLATO_BOX]
public class FireStonePanel : PanelDefinition {

    readonly public static int  MOVE_TO_FIRE = 3;
    

	protected override void OnPanelDestroy (BoardPanel bp)
    {
        JMFUtils.GM.removeBoardFromLauncher(bp.Owner);
        //if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("stone_cracks");

        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = bp.Owner.Position;
        effect.Play(pos, Vector3.one, 1, false, 0.45f);
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
    public override bool IsNeedGuage() { return true; }
	public override bool isLevelMissionTarget() {   return true;    }

	public override string GetImageName (int type) 
	{
		return "volcano";
	}

    public float fire(BoardPanel bdFrom)
    {
        List<Board> candidates  = new List<Board>();
        List<Board> targetBoards= new List<Board>();

        // search target.
		foreach (Board bd in GM.Boards)
        {
            if ((bd.PND is BasicPanel) == false) continue;
            if (bd.IsFilled == false) continue;
            if (bd.Piece.IsMatchable() == false) continue;
            candidates.Add(bd);
        }

        int idxRand             = Random.Range(0, candidates.Count);

        return bdFrom.fireStone( candidates[ idxRand ] );
    }
}
