using UnityEngine;
using System.Collections;

public class LockedPanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("bomb2");
	}

    public override float ShowHitEffect (BoardPanel bp) { 
//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("FX_cage", bp.Owner.GM.transform);
//        pp.transform.localPosition = bp.Owner.LocalPosition;
//        pp.Play();
        return 0F; 
    }

	public override bool IsSolid (BoardPanel bp) { return false; }
	public override bool IsFallable (BoardPanel bp) { return true; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsMatchable (BoardPanel bp) { return true; }
	public override bool IsStealable (BoardPanel bp) { return true; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
	public override bool IsShufflable (BoardPanel bp) { return false; }
    public override bool isLevelMissionTarget() {   return true;    }

	public override string GetImageName (int type) 
	{
		return "wire";
	}
}
