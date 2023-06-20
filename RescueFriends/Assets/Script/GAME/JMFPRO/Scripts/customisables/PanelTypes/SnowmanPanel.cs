using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [COTTON_CANDY]
public class SnowmanPanel : PanelDefinition {
    public static int DestroyedCount { get; private set; }

    protected override void OnPanelCreate (BoardPanel bp) {
        JMFRelay.onChangeRemainSnowman(Snowman.TotalCount);

        BoardPanel.TYPE side        = BoardPanel.TYPE.BACK;
        if(bp.PND.isFront) side     = BoardPanel.TYPE.FRONT;
        bp[ side ].GetComponent<Panel>().Play("create", false);  // 0.2f;
    }   

    protected override void OnPanelDestroy (BoardPanel bp) {

        if(JMFUtils.GM.State != JMF_GAMESTATE.PLAY) 
            return;

        JMFRelay.onChangeRemainSnowman(Snowman.TotalCount - 1); 

        // [COTTON_CANDY]
        if(JMFUtils.GM.CurrentLevel.countCottonCandy > 0)
        {
            JMFUtils.GM.countMatchCottonCandy++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countCottonCandy - JMFUtils.GM.countMatchCottonCandy;
            if(remainCount >= 0)
				JMFUtils.GM.AnimationMissionObject(bp.Owner, remainCount, "CottonCandy", GetImageName(bp.Durability +1));
        }
        //
    }   

    protected override void OnHit (BoardPanel bp, bool isSpecialAttack) {
        DestroyedCount++;
        NNSoundHelper.Play("IFX_soft_bust");
    }   
    
    public override float ShowHitEffect (BoardPanel bp) { 
//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("SnowCrash", bp.Owner.GM.transform);
//        pp.transform.localPosition = bp.Owner.LocalPosition;
//        pp.Play();
        return 0.15F; 
    }
	
	public override string GetImageName (int type) 
	{
		if(type > -1 && type < 3)
			return string.Format("obstacle_monster_0{0}",type +1);

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

    public static void ResetDestroyedCount () {
        DestroyedCount = 0;
    }   
}
