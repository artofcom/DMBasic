using UnityEngine;
using System.Collections;

// [BUBBLE_PANNEL]
public class RectChocoPanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY)
            return;

        NNSoundHelper.Play("IFX_soft_bust");

        if(JMFUtils.GM.CurrentLevel.countRectChocho > 0)
        {
            JMFUtils.GM.countMatchRectChocho++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countRectChocho - JMFUtils.GM.countMatchRectChocho;
            if(remainCount >= 0)
				JMFUtils.GM.AnimationMissionObject(bp.Owner, remainCount, "RectChoco", GetImageName(bp.Durability + 1));
        }
	}
	
    public override float ShowHitEffect (BoardPanel bp)
    {
        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = bp.Owner.Position;
        pos.z -= 2.0f;            // note : yellow color 사용. - so index is 1.
        effect.Play(pos, Vector3.one, 1, false, 0.45f);

        if(bp.Durability > 0)
        {
            BoardPanel.TYPE side        = BoardPanel.TYPE.BACK;
            if(bp.PND.isFront) side     = BoardPanel.TYPE.FRONT;
            bp[ side ].GetComponent<Panel>().Play("hit_blending", false);  // 0.2f;
            return 0.35f;
        }
        else return .0f;
    }

	public override string GetImageName (int type) 
	{
		if(type > -1 && type < 6)
			return string.Format("obstacle_wood_0{0}",type +1);

		return null;
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
    public override bool isLevelMissionTarget() {   return true;    }
}
