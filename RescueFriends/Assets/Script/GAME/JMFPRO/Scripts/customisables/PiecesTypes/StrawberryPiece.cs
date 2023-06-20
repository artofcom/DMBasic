using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// [STRAWBERRY]
public class StrawberryPiece : PieceDefinition {

    bool spawned;
    int moveForThis = 0;
    int spawnableCount = 0;

     void OnEnable () {
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnFinishDrop += OnPlayerMove;
        // JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnFinishDrop -= OnPlayerMove;
        // JMFRelay.OnBoardStable -= OnBoardStable;
    }

    public override int GetColorIndex ()
    { 
        return (int)LEItem.COLOR.STRAWBERRY - 1;
    }

	public override string GetImageName (int colorIndex) 
	{
		return "Strawberry";
	}
	
    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        int score               = 0;
        //if (isByMatch == false) score += destroyScore;
        score += destroyScore;

        if (gp.Owner.ShadedDurability < 0)
		{
            gp.Owner.eShadeType = (int)LEItem.SHADE_TYPE.JAM;
            gp.Owner.ShadedDurability   = 0;
        }

        return score;
    }

    void OnGameReady ()
    {
        spawned                 = false;
        moveForThis             = 0;
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.strawberrySpawnCountPerMove);
    }
    void OnPlayerMove ()
    {
        OnBoardStable();

        if (Strawberry.TotalCount < JMFUtils.GM.CurrentLevel.maxStrawberryCount) {
            moveForThis++;
        }
    }
    void OnBoardStable ()
    {
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.strawberrySpawnCountPerMove);
        if (spawned)
        {
            moveForThis         = 0;
            spawned             = false;
        }
    }
    public override PieceDefinition GetSpawnableDefinition (Point pt)
    {
        if (GM.CurrentLevel.enableStrawberry == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.strawberrySpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.strawberrySpawnColumn) {
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
        if (moveForThis < GM.CurrentLevel.movePerStrawberry) return false;
        if (Strawberry.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxStrawberryCount)) return false;

        if (Strawberry.TotalCount < Mathf.Max(1, GM.CurrentLevel.minStrawberryCount))
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

    public override float ShowDestroyEffect (GamePiece gp)
	{
		//BlockCrash effect       = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
        //int idxViolet           = 7;
        //return effect.Play("play", gp.Position, gp.Scale, idxViolet, false);
        float d                 = gp.Play("hit_blending", false);
        gp.GO.transform.DOScale(Vector3.one*0.01f, d).SetEase(Ease.InExpo);
        return 0.35f;
    }
}
