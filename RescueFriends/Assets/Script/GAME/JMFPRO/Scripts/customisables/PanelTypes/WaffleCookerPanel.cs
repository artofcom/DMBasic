using UnityEngine;
using System.Collections;

public class WaffleCookerPanel : PanelDefinition {
	protected override void OnPanelDestroy (BoardPanel bp) {

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY)
            return;

        NNSoundHelper.Play("IFX_soft_bust");
        
        if(JMFUtils.GM.CurrentLevel.countWaffleCooker > 0)
        {
            JMFUtils.GM.countMatchWaffleCooker++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countWaffleCooker - JMFUtils.GM.countMatchWaffleCooker;
            if(remainCount >= 0)
				JMFUtils.GM.AnimationMissionObjectWithSprite(bp.Owner, remainCount, "WaffleCooker", "waffle2");           
                    // temp.
                    //GetImageName(bp.Durability + 1));
        }
	}

    public override float ShowHitEffect (BoardPanel bp) {
        //BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        //Vector3 pos             = bp.Owner.Position;
        //pos.z -= 2.0f;            // note : yellow color 사용. - so index is 1.
        //return effect.Play(pos, Vector3.one, 1, false, 0.45f);

        float fTime             = .0f;

        Panel pnl               = bp[BoardPanel.TYPE.BACK];
        if(null == pnl)         return .0f;
        fTime                   = (0.1f+pnl.Play(string.Format("waffle_cooker_level{0}_hit", bp.Durability+1), false));
              
        // note : 특수상태가 될때까지 destory 하지 못하도록 상태 변경.
        if(1 == bp.Durability)
        {
            bp.setIsDestroyablePanel( false );

            // 이때 다른 모든 WaffleCokkerPanel이 durability==0 이면 모두 hit 처리.
            JMFUtils.GM.StartCoroutine( JMFUtils.GM.coCheckAllWaffleCookers(bp) );
        }

        return fTime;
    }

	public override string GetImageName(int strentgh) 
	{
        int full                = 2;
        strentgh                = Mathf.Clamp(strentgh, 0, 1);
		return string.Format("waffle{0}", full - strentgh);
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
}
