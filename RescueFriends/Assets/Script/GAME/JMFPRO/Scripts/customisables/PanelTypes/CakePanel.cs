using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;

public enum CAKE_TYPE { A, B, C, D, MAX }

public abstract class CakePanel : PanelDefinition {
    public abstract CAKE_TYPE Type { get; }

    public override float ShowHitEffect (BoardPanel bp) { 
        if ((bp.Durability == 1) && IsCakeDestroyable(bp)) {
            NNSoundHelper.Play("IFX_hard_bust");
//            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("CakePanelCrash", bp.Owner.GM.transform);
//            pp.transform.localPosition = bp.Owner.LocalPosition;
//            pp.Play();
        } else {
//            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("SnowCrash", bp.Owner.GM.transform);
//            pp.transform.localPosition = bp.Owner.LocalPosition;
//            pp.Play();

            NNSoundHelper.Play("IFX_hard_bust");
        }

        return 0F;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return true; }
    public override bool IsShufflable (BoardPanel bp) { return false; }

	public override bool IsDestroyable (BoardPanel bp) { 
        return bp.Durability > 0;
    }

    public bool IsExplodable (BoardPanel bp) {
        if (bp.Durability != 0) return false;
        return IsCakeDestroyable(bp);
    }

    public List<Board> GetOtherCakes (BoardPanel bp) {
        List<Board> bds = new List<Board>();

        for (int i = 0; i < (int)CAKE_TYPE.MAX; i++) {
            if ((bp.PND as CakePanel).Type == (CAKE_TYPE)i) continue;
            bds.Add(GetCake(bp, (CAKE_TYPE)i));
        }

        Debugger.Assert(bds.Count == 3, "CakePanel.GetOtherCakes : Cake set is wrong.");

        return bds;
    }

    BoardPanel GetNext (BoardPanel bp) {
        int index = (int)(bp.PND as CakePanel).Type + 1;
        if (index > 3) index = 0;
        return GetCake(bp, (CAKE_TYPE)index).Panel;
    }

    bool IsCakeDestroyable (BoardPanel bp) {
        List<Board> otherCakes = GetOtherCakes(bp);

        for (int i = 0; i < otherCakes.Count; i++) {
            if (otherCakes[i].Panel.Durability > 0) return false;
        }

        return true;
    }

    Board GetRootCake (BoardPanel bp) {
        switch ((bp.PND as CakePanel).Type) {
            case CAKE_TYPE.B : 
                Debugger.Assert(bp.Owner.Left != null, "CakePanel.GetRootCake : L board is null.");
                return bp.Owner.Left;
            case CAKE_TYPE.C :
                Debugger.Assert(bp.Owner.Bottom != null, "CakePanel.GetRootCake : B board is null.");
                return bp.Owner.Bottom;
            case CAKE_TYPE.D :
                Debugger.Assert(bp.Owner.Bottom != null, "CakePanel.GetRootCake : B board is null.");
                Debugger.Assert(bp.Owner.Bottom.Left != null, "CakePanel.GetRootCake : BL board is null.");
                return bp.Owner.Bottom.Left;
            default : return bp.Owner;
        }
    }

    Board GetCake (BoardPanel bp, CAKE_TYPE targetType) {
        Board cakeA = GetRootCake(bp);

        switch (targetType) {
            case CAKE_TYPE.A : return cakeA;
            case CAKE_TYPE.B : return cakeA.Right;
            case CAKE_TYPE.C : return cakeA.Top;
            case CAKE_TYPE.D : return cakeA.Top.Right;
            default : 
                Debug.LogError("CakePanel.GetCake : Type is wrong.");
                return bp.Owner;
        }
    }
}
