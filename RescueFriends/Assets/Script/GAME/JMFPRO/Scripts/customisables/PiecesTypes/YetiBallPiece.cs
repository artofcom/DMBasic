using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class YetiBallPiece : PieceDefinition {
	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        if (Yeti.Current != null) {
            YetiAttackBall ball = NNPool.GetItem<YetiAttackBall>("YetiAttackBall", JMFUtils.GM.transform);
            ball.transform.position = gp.Owner.Position;
            ball.Play(gp.GO.transform.localScale, gp.ColorIndex);
        }

        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
    /*    if (JMFUtils.GM.CurrentLevel.yetiBallSpawnColumn == null) return null;
        bool isChosenColumn = false;

        foreach (int i in JMFUtils.GM.CurrentLevel.yetiBallSpawnColumn) {
            if (i == pt.X) {
                isChosenColumn = true;
                break;
            }
        }

        if (isChosenColumn == false) return null;

        return IsChanceOfSpawn() ? this : null;*/
        return null;
	}

    public override bool IsChanceOfSpawn () {
        if (JMFUtils.GM.CurrentLevel.isYetiGame == false) return false;
        if (YetiBall.TotalCount >= JMFUtils.GM.CurrentLevel.maxYetiBallPieceCount) return false;

        if ((YetiBall.TotalCount < JMFUtils.GM.CurrentLevel.minYetiBallPieceCount) || 
            (NOVNINE.NNTool.Rand(0, 100) < JMFUtils.GM.CurrentLevel.yetiBallDropProbability)) {
            return true;
        } else {
            return false;
        }
    }
}
