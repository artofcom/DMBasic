using UnityEngine;
using System.Collections;

public class CagePanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("IFX_hard_bust");
	}

    public override float ShowHitEffect (BoardPanel bp) { 
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("Break");
		Vector3 pos             = bp.Owner.Position;
		pos.z -= 2.0f;
        effect.Play("wire_cage_break", pos, Vector3.one, 0);//, false, 0);// 0.35f);

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
    public override bool isLevelMissionTarget() {   return true;    }

	public override string GetImageName (int type) 
	{
		if(type > -1 && type < 3)
			return string.Format("wire_cage{0}",type +1);

		return null;
	}
}
