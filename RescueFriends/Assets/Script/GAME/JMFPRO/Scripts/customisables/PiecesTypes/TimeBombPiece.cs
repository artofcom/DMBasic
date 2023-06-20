using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeBombPiece : PieceDefinition {
    bool spawned;
    int moveForTimeBomb = 0;
    int spawnableCount = 0;

    void OnEnable () {
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnFinishDrop += OnPlayerMove;
        //JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnFinishDrop -= OnPlayerMove;
        //JMFRelay.OnBoardStable -= OnBoardStable;
    }

    void OnGameReady () {
        spawned = false;
        moveForTimeBomb = 0;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.timeBombSpawnCountPerMove);
    }

    void OnPlayerMove () {

        OnBoardStable();

        if (TimeBomb.TotalCount < JMFUtils.GM.CurrentLevel.maxTimeBombCount) {
            moveForTimeBomb++;
        }
    }

    void OnBoardStable () {
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.timeBombSpawnCountPerMove);

        if (spawned) {
            moveForTimeBomb = 0;
            spawned = false;
        }
    }

	protected override void OnPieceCreate (GamePiece gp) {
        gp.FallBackTime = GM.CurrentLevel.defaultFallBackTime;
    }

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("Timebomb_destroy");
        return score;
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (GM.CurrentLevel.enableTimeBomb == false) return null;
        return IsChanceOfSpawn() ? this : null;
	}

    public override bool IsChanceOfSpawn () {
        if (moveForTimeBomb < GM.CurrentLevel.movePerTimeBomb) return false;
        if (TimeBomb.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxTimeBombCount)) return false;

        if (TimeBomb.TotalCount < Mathf.Max(1, GM.CurrentLevel.minTimeBombCount)) {
            spawnableCount--;
            spawned = true;
            return true;
        }

        if (spawnableCount > 0) {
            spawnableCount--;
            spawned = true;
            return true;
        } else {
            return false;
        }
    }
}
