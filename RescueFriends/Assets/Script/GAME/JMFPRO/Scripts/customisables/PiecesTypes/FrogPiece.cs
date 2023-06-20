using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrogPiece : PieceDefinition {
    public int fillingCount = 12;

	protected override void OnPieceClick (GamePiece gp) {
        if (GM.IsStable == false) return;

        Frog frog = gp.GO.GetComponent<Frog>();

        if (frog.IsReadyToJump == false) return;

        if (GM.ReadiedFrog == null) {
            GM.ReadiedFrog = gp.Owner;
        } else {
            GM.ReadiedFrog = null;
        }
    }

/*
    public bool IsReadyToJump { get { return Frog.EatingCount >= fillingCount; } }

	public abstract bool Match (List<Board> linkX, List<Board> linkY);

    public virtual void ExecutePower (Board bd) { }

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) {
        score = 0;

        UpdateMatchData(ref bd, linkX, linkY);

        if (frogBoard == null) return false;

        FrogPiece prevFP = frogBoard.PD as FrogPiece;

        if (Match(linkX, linkY)) {
            OnPanelDestroy(frogBoard.Piece);
            prevFP.ExecutePower(frogBoard);
            return true;
        }

        return false;
    }

    void UpdateMatchData (ref Board bd, List<Board> linkX, List<Board> linkY) {
        frogBoard = null;

        if (IsFrog(bd)) frogBoard = bd;

        for (int i = 0; i < linkX.Count; i++) {
            if (IsFrog(linkX[i])) {
                frogBoard = linkX[i];
                linkX[i] = bd;
                bd = frogBoard;
            }
        }

        for (int i = 0; i < linkY.Count; i++) {
            if (IsFrog(linkY[i])) {
                frogBoard = linkY[i];
                linkY[i] = bd;
                bd = frogBoard;
            }
        }
    }

    bool IsFrog (Board bd) {
        if (bd.IsFilled == false) return false;
        return bd.PD is FrogPiece;
    }

    protected void DestroyBySequencial (List<Board> boards, float interval, int ignoreCount, bool isLiner = false) {
        for (int i = 0; i < boards.Count; i++) {
            if (i < ignoreCount) continue;
            boards[i].Hit();//GM.DestroyInTime(boards[i], interval);
            if (isLiner && (boards[i].PD is RoundChocoPiece)) break;
        }
    }

    protected override void OnPanelDestroy (GamePiece gp) {
        Frog.EnableEating = true;
        CancelInvoke("DisableEating");
        Invoke("DisableEating", 0.5F);
    }

    void DisableEating () {
        if (Frog.EnableEating) {
            Frog.EnableEating = false;
        }
    }
*/
}
