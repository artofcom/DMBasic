using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TREASURE_TYPE { POTION1, POTION2, POTION3 }

public abstract class TreasurePiece : PieceDefinition {
    static bool actived;
    static bool spawned;
    static int moveForTreasure = 0;
    static int spawnableCount = 0;

    public abstract TREASURE_TYPE Type { get; }

	public override int GetColorIndex () { return 0; }

    public abstract bool CanSpawn ();

    void OnEnable () {
        if (actived == false) {
            JMFRelay.OnGameReady += OnGameReady;
            JMFRelay.OnFinishDrop += OnPlayerMove;
           // JMFRelay.OnBoardStable += OnBoardStable;
            actived = true;
        }
    }

    void OnDisable () {
        if (actived) {
            JMFRelay.OnGameReady -= OnGameReady;
            JMFRelay.OnFinishDrop -= OnPlayerMove;
          //  JMFRelay.OnBoardStable -= OnBoardStable;
            actived = false;
        }
    }

    void OnGameReady () {
        spawned = false;
        moveForTreasure = 0;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.treasureSpawnCountPerMove);
    }

    void OnPlayerMove () {
        OnBoardStable();

        if ((Potion1.TotalCount + Potion2.TotalCount + Potion3.TotalCount) < JMFUtils.GM.CurrentLevel.maxOnScreen) {
            moveForTreasure++;
        }
    }

    // init spawn values.
    void OnBoardStable () {
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.treasureSpawnCountPerMove);

        if (moveForTreasure >= JMFUtils.GM.CurrentLevel.movePerTreasure) {
            if (spawned) {
                moveForTreasure = 0;
                spawned = false;
            }
        }
    }

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        GM.IncreaseScore(destroyScore, gp.Position, 6);
        //NNSoundHelper.Play("IFX_goal_earning");
        return destroyScore;
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (JMFUtils.GM.CurrentLevel.enableTreasure == false) return null;

        if (JMFUtils.GM.CurrentLevel.treasureSpawnColumn == null) {
            return IsChanceOfSpawn() ? this : null;
        } 

        bool isChosenColumn = false;

        foreach (int i in JMFUtils.GM.CurrentLevel.treasureSpawnColumn) {
            if (i == pt.X) {
                isChosenColumn = true;
                break;
            }
        }

        if (isChosenColumn) {
            return IsChanceOfSpawn() ? this : null;
        } else {
            return null;
        }
	}

    public override bool IsChanceOfSpawn () {
		if (JMFUtils.GM.CurrentLevel.isTreasureGame == false) return false;
        if (moveForTreasure < JMFUtils.GM.CurrentLevel.movePerTreasure) return false;
        if ((Potion1.TotalCount + Potion2.TotalCount + Potion3.TotalCount) >= Mathf.Max(1, JMFUtils.GM.CurrentLevel.maxOnScreen)) return false;
        if (CanSpawn() == false) return false;

        if (spawnableCount > 0) {
            spawnableCount--;
            spawned = true;
            return true;
        } else {
            return false;
        }
    }

    public override string getDropSndName()
    {
        return "IFX_syrup_drop";
    }
}
