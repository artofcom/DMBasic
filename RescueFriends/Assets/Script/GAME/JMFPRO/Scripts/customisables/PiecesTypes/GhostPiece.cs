using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE;

public class GhostPiece : PieceDefinition {
    const int CONVERT_COUNT = 4;

	public override int GetColorIndex () { return 0; }

    public override float ShowDestroyEffect (GamePiece gp) {
//        GhostEffect ge = NNPool.GetItem<GhostEffect>("GhostEffect");
//        ge.transform.position = gp.Position;
//        ge.Play();
        return 0F;
    }

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
	}

	public override bool IsCombinable ( GamePiece other) 
	{ 
		if (other.IsMatchable()) return true;
        return false; 
    }

	public override float PerformCombinationPower (Board from, Board to, GamePiece gp, GamePiece other) {
        gp.Owner.Hit();
        other.Owner.RemovePiece();
        return 2F;
    }

    public override void PerformPower (GamePiece gp) {
        StartCoroutine(PerformGhostPower(gp));
    }

    IEnumerator PerformGhostPower (GamePiece gp) {
        Board ghostBoard = gp.Owner;

        yield return new WaitForSeconds(0.5F);

        List<Board> targets = new List<Board>();
        List<Board> candidates = new List<Board>();
        List<GamePiece> bombs = new List<GamePiece>();

		foreach (Board bd in GM.Boards) {
            if (bd == ghostBoard) continue;
            if (((bd.PND is BasicPanel) == false) && (bd.Panel.IsDestroyable() == false) && 
                (bd.Panel.IsFillable() == false)) continue;
            if (bd.IsFilled && (bd.Piece.IsDestroyable() == false)) continue;
            if (bd.IsFilled && (bd.PD is GhostPiece)) continue;

            candidates.Add(bd);
		}

        if (candidates.Count > 0) {
            int count = 0;

            NNTool.ExecuteForEachRandomIndex(0, candidates.Count-1, (i) => {
                targets.Add(candidates[i]);
                count++;
                return count < CONVERT_COUNT;
            });
        }

        for (int i = 0; i < targets.Count; i++) {
            Vector3 from = ghostBoard.Position;
            Vector3 to = targets[i].Position;
            GameObject go = NNPool.GetItem("Lightning");
            go.GetComponent<Lightning>().EmitLightning(from, to, 1F);

            if (targets[i].Panel.IsDestroyable()) {
                targets[i].ResetPanel(GM.GetPanelType<BasicPanel>(), 0);
            }

            targets[i].ResetPiece(GM.GetPieceType<BombPiece>(), GM.GetRandomColorIndex());
            bombs.Add(targets[i].Piece);
        }

        yield return new WaitForSeconds(1.5F);

        yield return StartCoroutine( GM.coWaitTillStable() );

        for (int i = 0; i < bombs.Count; i++) {
            if ((bombs[i].PD != null) && (bombs[i].GO != null)) bombs[i].Destroy(false, true, false, false); 
        }
    }
}
