using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [GREEN_BUBBLE]
public class GreenBubblePiece : PieceDefinition {
	public override int GetColorIndex () { return 0; }

    public static bool DestroyedThisTurn { get; private set; }

    bool spawned;
    int moveForThis = 0;
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
        moveForThis = 0;
        DestroyedThisTurn = false;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.greenBubbleSpawnCountPerMove);
    }

    void OnPlayerMove () {

        OnBoardStable();

        if (GreenBubble.TotalCount < JMFUtils.GM.CurrentLevel.maxGreenBubble) {
            moveForThis++;
        }
    }

    void OnBoardStable () {
        DestroyedThisTurn = false;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.greenBubbleSpawnCountPerMove);

        if (spawned) {
            moveForThis = 0;
            spawned = false;
        }
    }
	
	public override string GetImageName (int colorIndex) 
	{
		string fileName = null;
		switch(colorIndex)
		{
			case 0: fileName = "GreenbubbleL1"; break;
			case 1: fileName = "GreenbubbleL2"; break;
			case 2: fileName = "GreenbubbleL3"; break;
		}

		return fileName;
	}

	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        DestroyedThisTurn = true;
        
        return destroyScore;
	}

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (JMFUtils.GM.CurrentLevel.enableGreenBubble == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.greenBubbleSpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.greenBubbleSpawnColumn) {
            if (i == pt.X) {
                isChosenColumn = true;
                break;
            }
        }

        if (isChosenColumn == false) return null;

        return IsChanceOfSpawn() ? this : null;
	}

    public override bool IsChanceOfSpawn () {
        // if (DestroyedThisTurn) return false;
        if (moveForThis < JMFUtils.GM.CurrentLevel.movePerGreenBubble) return false;
        if (GreenBubble.TotalCount >= Mathf.Max(1, JMFUtils.GM.CurrentLevel.maxGreenBubble)) return false;

        if (GreenBubble.TotalCount < Mathf.Max(1, JMFUtils.GM.CurrentLevel.minGreenBubble)) {
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

    public override float ShowDestroyEffect (GamePiece gp) {
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("Break");
		Vector3 pos             = gp.Owner.Position;
		pos.z -= 2.0f;
        effect.Play("Greenbubble_break", pos, Vector3.one, 0);//, false, 0.0f);

        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY)
            NNSoundHelper.Play("IFX_hard_bust");

        return effectTime;
    }
}
