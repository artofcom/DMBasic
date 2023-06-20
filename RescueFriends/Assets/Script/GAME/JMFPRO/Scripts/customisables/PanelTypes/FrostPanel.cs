using UnityEngine;
using System.Collections;

public class FrostPanel : PanelDefinition 
{
	protected override void OnPanelDestroy (BoardPanel bp) 
	{
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("IFX_bubble_bust");
	}
	
    public override float ShowHitEffect (BoardPanel bp) 
	{ 
        //PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");
		//Vector3 pos             = bp.Owner.Position;
		//pos.z -= 2.0f;
        //effect.Play(pos, Vector3.one, false, .0f, 1.0f, null, "bubble_overbubbleL" );
        // differ's from effect playing time.
        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = bp.Owner.Position;
        effect.Play(pos, Vector3.one, 1, false, 0.45f);

        return 0.2f;
    }

	public override string GetImageName (int type) 
	{
		if(type > -1 && type < 6)
			return string.Format("OverbubbleL{0}",type +1);
		
		return null;
	}
	
	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return true; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }
    public override bool IsApplyShockWave() { return false; }
    public override bool isLevelMissionTarget() {   return true;    }
}
