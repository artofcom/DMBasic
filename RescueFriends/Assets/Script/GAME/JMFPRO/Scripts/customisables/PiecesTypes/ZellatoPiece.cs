using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [ZELLATO_PIECE]
public class ZellatoPiece : PieceDefinition {

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

    public override int GetColorIndex ()
    { 
        return (int)LEItem.COLOR.ZELLATO - 1;
    }
	
	public override string GetImageName (int colorIndex) 
	{
		return "Gelato";
	}

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is ZellatoPiece))
            return false;

        if (JMFUtils.GM.CurrentLevel.countZellatto > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.countZellatto - JMFUtils.GM.countMatchZellatto;
            return (remainCount>=0);
        }
        return false;

    }

    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        int score               = 0;
        //if (isByMatch == false) score += destroyScore;
        score += destroyScore;

        if(JMFUtils.GM.CurrentLevel.countZellatto > 0)
        {
            JMFUtils.GM.countMatchZellatto++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countZellatto - JMFUtils.GM.countMatchZellatto;
            if(remainCount >= 0)
            {
				JMFUtils.GM.AnimationMissionObject(gp.Owner, remainCount, "Zellato", GetImageName(gp.Durability));
            }
        }

        if (gp.Owner.ShadedDurability >= 0)
		{
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

    void OnGameReady ()
    {
        spawned                 = false;
        moveForThis             = 0;
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.zellatoSpawnCountPerMove);
    }
    void OnPlayerMove ()
    {
        OnBoardStable();

        if (Zellato.TotalCount < JMFUtils.GM.CurrentLevel.maxZellatoCount) {
            moveForThis++;
        }
    }
    void OnBoardStable ()
    {
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.zellatoSpawnCountPerMove);
        if (spawned)
        {
            moveForThis         = 0;
            spawned             = false;
        }
    }
    public override PieceDefinition GetSpawnableDefinition (Point pt)
    {
        if (GM.CurrentLevel.enableZellato == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.zellattoSpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.zellattoSpawnColumn) {
            if (i == pt.X) {
                isChosenColumn = true;
                break;
            }
        }

        if (isChosenColumn == false) return null;

        return IsChanceOfSpawn() ? this : null;
    }

    public override bool IsChanceOfSpawn ()
    {
        if (moveForThis < GM.CurrentLevel.movePerZellato) return false;
        if (Zellato.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxZellatoCount)) return false;

        if (Zellato.TotalCount < Mathf.Max(1, GM.CurrentLevel.minZellatoCount))
        {
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
        
        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        effect.Play(gp.GO.transform.position, Vector3.one, 1, false, .0f);
        return 0.35f;
    }
}
