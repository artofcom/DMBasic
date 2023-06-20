using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SodaCanPanel : PanelDefinition {
    public static int DestroyedCount { get; private set; }

    protected override void OnPanelCreate (BoardPanel bp) {
        //JMFRelay.onChangeRemainSnowman(Snowman.TotalCount);
    }   

    protected override void OnPanelDestroy (BoardPanel bp) {
        //JMFRelay.onChangeRemainSnowman(Snowman.TotalCount - 1);         
    }   

    protected override void OnHit (BoardPanel bp, bool isSpecialAttack) {
        DestroyedCount++;
        NNSoundHelper.Play("IFX_soft_bust");


        // [SODA_CAN]
        if(JMFUtils.GM.CurrentLevel.countSodaCan > 0)
        {
            JMFUtils.GM.countMatchSodaCan++;

            // particle eff.
            //BubbleHit effect    = NNPool.GetItem<BubbleHit>("BubbleHit");
		    Vector3 pos         = bp.Owner.Position;
		    pos.z -= 100.0f;
            //effect.Play(pos, Vector3.one, 0, false, 0.0f);

            ParticlePlayer effect   = NNPool.GetItem<ParticlePlayer>("eff_soda_can_hit");
            effect.transform.parent = JMFUtils.GM.transform;
		    effect.Play(pos);
            NNSoundHelper.Play("IFX_Water");

            int remainCount     = JMFUtils.GM.CurrentLevel.countSodaCan - JMFUtils.GM.countMatchSodaCan;
            if(remainCount >= 0)
                JMFUtils.GM.AnimationMissionObject(bp.Owner, remainCount, "SodaCan", "bubble_Sodacan");
				//JMFUtils.GM.AnimationMissionObject(bp.Owner, remainCount, "SodaCan", GetImageName(bp.Durability));
        }
        //

        // note : soda can은 destroy되지 않으므로 이곳에 code를 넣어서 동기를 맞춘다.
        BoardPanel.TYPE side        = BoardPanel.TYPE.BACK;
        if(bp.PND.isFront) side     = BoardPanel.TYPE.FRONT;
        bp[ side ].GetComponent<Panel>().Play("hit_blending", false);  // 0.2f;
    }   
    
    public override float ShowHitEffect (BoardPanel bp) {

        //BoardPanel.TYPE side        = BoardPanel.TYPE.BACK;
        //if(bp.PND.isFront) side     = BoardPanel.TYPE.FRONT;
        return .0f; // bp[ side ].GetComponent<Panel>().Play("hit_blending", false);  // 0.2f;
    }   

    public override bool IsSolid (BoardPanel bp) { return true; }
    public override bool IsFallable (BoardPanel bp) { return false; }
    public override bool IsFillable (BoardPanel bp) { return false; }
    public override bool IsMatchable (BoardPanel bp) { return false; }
    public override bool IsStealable (BoardPanel bp) { return false; }
    public override bool IsSwitchable (BoardPanel bp) { return false; }
    public override bool IsDestroyable (BoardPanel bp) { return false; }// true; }
    public override bool IsSplashHitable (BoardPanel bp) { return true; }
    public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }

    public static void ResetDestroyedCount () {
        DestroyedCount = 0;
    }   
	
	public override string GetImageName (int type) 
	{
		return "Sodacan";
	}
}
