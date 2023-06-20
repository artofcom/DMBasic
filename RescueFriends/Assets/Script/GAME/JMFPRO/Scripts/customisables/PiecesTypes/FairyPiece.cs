using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FairyPiece : PieceDefinition {
    bool spawned;
    int moveForFairy = 0;
    int spawnableCount = 0;

    void OnEnable () {
        JMFRelay.OnGameReady += OnGameReady;
        //JMFRelay.OnFinishDrop += OnPlayerMove;
        JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnGameReady -= OnGameReady;
        //JMFRelay.OnFinishDrop -= OnPlayerMove;
        JMFRelay.OnBoardStable -= OnBoardStable;
    }

    void OnGameReady () {
        spawned = false;
        moveForFairy = 0;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.fairyPieceSpawnCountPerMove);
    }

    void OnBoardStable () {
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.fairyPieceSpawnCountPerMove);

        if (spawned) {
            moveForFairy = 0;
            spawned = false;
        }

        //JMFUtils.GM.SetFairyAnimation(); 
    }

//    void OnPlayerMove () {
//        if (Fairy.TotalCount < GM.CurrentLevel.maxFairyPieceCount) {
//            moveForFairy++;
//        }
//    }

    public override float ShowDestroyEffect (GamePiece gp) {
		BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
        
		Vector3 pos = gp.Position;
		pos.z -= 2.0f; 
		return effect.Play("play",pos,gp.Scale, gp.ColorIndex);//, false);
    }

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        JMFUtils.GM.FairyMatchCount++;
        int remainCount = JMFUtils.GM.CurrentLevel.numberOfFairy - JMFUtils.GM.FairyMatchCount;
        remainCount = Mathf.Max(0, remainCount);
        JMFRelay.FireOnChangeRemainFairy(remainCount);
        if (JMFUtils.GM.CurrentLevel.isFairyGame) JMFUtils.GM.AnimateGainFairy(gp, remainCount);

        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;

    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (JMFUtils.GM.CurrentLevel.isFairyGame == false) return null;
        return IsChanceOfSpawn() ? this : null;
	}

    public override bool IsChanceOfSpawn () {
        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return false;
        if (JMFUtils.GM.CurrentLevel.enableFairyPiece == false) return false;
        if (moveForFairy < GM.CurrentLevel.movePerFairyPiece) return false;
//        if ((JMFUtils.GM.FairyMatchCount + Fairy.TotalCount) >= JMFUtils.GM.CurrentLevel.numberOfFairy) return false;
//        if (Fairy.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxFairyPieceCount)) return false;
//
//        if (Fairy.TotalCount < Mathf.Max(1, GM.CurrentLevel.minFairyPieceCount)) {
//            spawnableCount--;
//            spawned = true;
//            return true;
//        }

        if (spawnableCount > 0) {
            spawnableCount--;
            spawned = true;
            return true;
        } else {
            return false;
        }
    }

}
