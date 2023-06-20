using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE.Diagnostics;

public class ChameleonPiece : PieceDefinition {
    bool spawned;
    int moveForChameleon = 0;
    int spawnableCount = 0;

    void OnEnable () {
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnFinishDrop += OnPlayerMove;
        JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnFinishDrop -= OnPlayerMove;
        JMFRelay.OnBoardStable -= OnBoardStable;
    }

    void OnGameReady () {
        spawned = false;
        moveForChameleon = 0;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.chameleonSpawnCountPerMove);
    }

    void OnPlayerMove () {
        if (Chameleon.TotalCount < JMFUtils.GM.CurrentLevel.maxChameleonPieceCount) {
            moveForChameleon++;
        }
    }

    void OnBoardStable () {
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.chameleonSpawnCountPerMove);

        if (spawned) {
            moveForChameleon = 0;
            spawned = false;
        }
    }

	protected override void OnPieceCreate (GamePiece gp) {
        Chameleon chameleon = gp.GO.GetComponent<Chameleon>();

        do {
            chameleon.NextIndex = JMFUtils.GM.GetRandomColorIndex();
        } while (chameleon.NextIndex == gp.ColorIndex);
	}

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if (JMFUtils.GM.CurrentLevel.isGetTypesGame && JMFUtils.GM.CurrentLevel.numToGet[gp.ColorIndex] > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.numToGet[gp.ColorIndex] - JMFUtils.GM.JewelMatchCounts[gp.ColorIndex];
            return (remainCount>=0);
        }
        return false;
    }

    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        if (JMFUtils.GM.CurrentLevel.isGetTypesGame && JMFUtils.GM.CurrentLevel.numToGet[colorIndex] > 0) {
            JMFUtils.GM.JewelMatchCounts[colorIndex]++;
            int remainCount = JMFUtils.GM.CurrentLevel.numToGet[colorIndex] - JMFUtils.GM.JewelMatchCounts[colorIndex];
            if (remainCount >= 0) {
                JMFRelay.FireOnCollectJewel(colorIndex, remainCount);
                JMFUtils.GM.AnimateGainJewel(gp, remainCount, colorIndex);
            }
        } else {
            ;
        }

        int score = 0;

        if (isByMatch == false) score += destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

    public override float ShowDestroyEffect (GamePiece gp) 
	{
		BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		Vector3 pos = gp.Position;
		pos.z -= 2.0f; 
		return effect.Play("play",pos,gp.Scale, gp.ColorIndex);//, false);
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
       /* if (JMFUtils.GM.CurrentLevel.enableChameleon == false) return null;
        if (JMFUtils.GM.CurrentLevel.chameleonSpawnColumn == null) return null;

        bool isChosenColumn = false;

        foreach (int i in JMFUtils.GM.CurrentLevel.chameleonSpawnColumn) {
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
        if (moveForChameleon < JMFUtils.GM.CurrentLevel.movePerChameleon) return false;
        if (Chameleon.TotalCount >= Mathf.Max(1, JMFUtils.GM.CurrentLevel.maxChameleonPieceCount)) return false;

        if (Chameleon.TotalCount < Mathf.Max(1, GM.CurrentLevel.minChameleonPieceCount)) {
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

    public void Change (GamePiece gp, System.Action<Board> onComplete) {
        Transform ptf = gp.GO.transform;
        Chameleon chameleon = ptf.GetComponent<Chameleon>();
        Vector3 originalScale = ptf.localScale;
        int currentIndex = gp.ColorIndex;
        int nextIndex = chameleon.NextIndex;

        Sequence seq = DOTween.Sequence();
        seq.Append(ptf.DOScale(Vector3.zero, 0.3F).SetEase(Ease.InBack));
        seq.AppendCallback(() => {
            chameleon.ChangeColor(nextIndex);
            chameleon.NextIndex = currentIndex;
            gp.ColorIndex = nextIndex;
        });
        seq.Append(ptf.DOScale(originalScale, 0.3F).SetEase(Ease.OutBack));
        seq.OnComplete(() => { if (onComplete != null) onComplete(gp.Owner); });
    }
}
