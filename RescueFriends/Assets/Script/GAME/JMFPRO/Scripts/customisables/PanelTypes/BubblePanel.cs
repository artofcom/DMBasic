using UnityEngine;
using System.Collections;

// [BUBBLE_PANNEL]
public class BubblePanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {

       /* if(JMFUtils.GM.CurrentLevel.countRectChocho > 0)
        {
            JMFUtils.GM.countMatchRectChocho++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countRectChocho - JMFUtils.GM.countMatchRectChocho;
            if(remainCount >= 0)
                JMFUtils.GM.AnimationMissionObject(bp.Owner, remainCount, "RectChoco", "RectChoco1");
        }*/
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("IFX_bubble_bust");
	}
	
    public override float ShowHitEffect (BoardPanel bp) { 
//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("WoodCrash", bp.Owner.GM.transform);
//        pp.transform.localPosition = bp.Owner.LocalPosition;
//        pp.Play();

        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");
		Vector3 pos             = bp.Owner.Position;
		pos.z -= 2.0f;
        effect.Play(pos, Vector3.one, false, .0f, 1.0f, null, "bubble_obstacle" );
        // differ's from effect playing time.
        return 0.2f;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
	public override bool IsShufflable (BoardPanel bp) { return false; }
    public override bool IsApplyShockWave() { return false; }
    public override bool isLevelMissionTarget() {   return true;    }

	public override string GetImageName (int type) 
	{
		if(type > -1 && type < 5)
			return string.Format("obstacle_bubble_0{0}",type +1);

		return null;
	}
}
