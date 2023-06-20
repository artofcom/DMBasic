using UnityEngine;
using System.Collections;

public class MudCoverPanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp)
    {
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY)
        {
            JMFUtils.GM._countMudCoverDestroyed++;
            NNSoundHelper.Play("IFX_hard_bust");
        }
	}

    public override float ShowHitEffect (BoardPanel bp)
    { 
      /*  SpineEffect spEffect    = NNPool.GetItem<SpineEffect>("Thorn");
        Vector3 pos             = bp.Owner.Position;
		pos.z -= 2.0f;
        spEffect.play("grow-down", .0f);// pos, Vector3.one, 0, false, 0);// 0.35f);
        spEffect.transform.parent = JMFUtils.GM.transform;
        spEffect.transform.position = pos;        
        spEffect.transform.localScale = bp[BoardPanel.TYPE.FRONT].transform.localScale;
        */
        // 연출 나오는 즉시 destroy 되어야 함.
        return .0f;// 0.2f;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return true; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
	public override bool IsShufflable (BoardPanel bp) { return false; }
    public override bool IsApplyShockWave() { return false; }

	public override string GetImageName (int type) 
	{
		return "thorn_mud";
	}
}
