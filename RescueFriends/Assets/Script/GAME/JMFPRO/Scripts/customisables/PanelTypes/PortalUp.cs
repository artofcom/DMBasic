using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;

public class PortalUp : PanelDefinition {
    Dictionary<Board, Board> portalDict = new Dictionary<Board, Board>();

    void Awake () {
        JMFRelay.OnGameReady += OnGameReady;
    }

    void OnGameReady () {
        portalDict.Clear();

        foreach (Board bdB in GM.Boards) {
            if ((bdB.PND is PortalUp) == false) continue;

            foreach (Board bdA in GM.Boards) {
                if ((bdA.PND is PortalUp) == false) continue;
                if (bdA.Panel.Durability != bdB.Panel.Durability) continue;
                portalDict.Add(bdB, bdA);
                break;
            }
        }
    }

    public Board GetOtherSide (Board board) {
        Debugger.Assert(portalDict.ContainsKey(board), "PortalB.GetOtherSide : Can not find portal board.");
        return portalDict[board];
    }

	public override bool IsFallable (BoardPanel bp) {
        Board otherSide = GetOtherSide(bp.Owner);

        if (bp.Owner.Bottom == null) return false;
        if (bp.Owner.Bottom.IsFillable == false) return false;
        if (otherSide.IsDropable == false) return false;

        return true;
	}
	
	public override bool IsSolid (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return true; }
	public override bool IsStealable (BoardPanel bp) { return true; }
	public override bool IsMatchable (BoardPanel bp) { return true; }
	public override bool IsSwitchable (BoardPanel bp) { return true; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return false; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return true; }
    public override bool IsShufflable (BoardPanel bp) { return true; }
}
