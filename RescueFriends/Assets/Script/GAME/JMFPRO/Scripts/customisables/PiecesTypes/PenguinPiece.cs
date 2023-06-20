using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PenguinPiece : PieceDefinition {
	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {

        JMFUtils.GM.PenguinMatchCount++;
        int remainCount = JMFUtils.GM.CurrentLevel.numberOfPenguin - JMFUtils.GM.PenguinMatchCount;
        remainCount = Mathf.Max(0, remainCount);
        JMFRelay.FireOnChangeRemainPenguin(remainCount);
        if (JMFUtils.GM.CurrentLevel.isPenguinGame) JMFUtils.GM.AnimateGainPenguin(gp, remainCount);

        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("penguin_destroy");
        return score;
    }
}
