using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AddTimePiece : PieceDefinition 
{
    const int ADD_TIME          = 5;

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

	// public override PieceDefinition GetSpawnableDefinition (Point pt) { return this; }
    public override float ShowDestroyEffect (GamePiece gp)
	{
		BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		effect.Play("play",gp.Position,gp.Scale, gp.ColorIndex);//, false);
        //NNSoundHelper.Play( "IFX_sugar_block_match" );
        return effectTime;
    }
	
	public override string GetImageName (int colorIndex) 
	{
		string fileName = null;
		switch(colorIndex)
		{
			case 0: fileName = "block_red"; break;
			case 1: fileName = "block_yellow"; break;
			case 2: fileName = "block_green"; break;
			case 3: fileName = "block_blue"; break;
			case 4: fileName = "block_purple"; break;
			case 5: fileName = "block_orange"; break;
			case 6: fileName = "block_skyblue"; break;
			case 7: fileName = "block_violet"; break;
		}

		return fileName;
	}

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is AddTimePiece))
            return false;

        return true;
    }

    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        // 뭔가 추가 시간 처리.
        if(JMFUtils.GM.CurrentLevel.isTimerGame)
        {
            JMFUtils.GM.AddExtraTimes( ADD_TIME );
        }
		
        int score = 0;

        //if (isByMatch == false) score += destroyScore;
        score += destroyScore;

        if (gp.Owner.ShadedDurability >= 0)
		{
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }


    // change piece's color by _changeColorIndex.
    public override bool canColorChange(Board bd)
    {
        if(null == bd)          return false;
        return (bd._changeColorIndex>=0 && null!=bd.Piece && bd.Piece.LifeCover<=0);
    }
    public override IEnumerator changePieceColorAsChanger(Board board, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);

        if (board._changeColorIndex == (int)LEItem.COLOR.RANDOM)
            board.ResetPiece(board.PD, GM.GetRandomColorIndex());
        else
            board.ResetPiece(board.PD, board._changeColorIndex - 1);

        board.AnimateColorChanger();

        AnimationNormalToSpecial(board);
    }
    void AnimationNormalToSpecial(Board bd) 
	{
        Vector3 startScale = bd.Piece.GO.transform.localScale;
		Sequence seq = DOTween.Sequence();
		seq.OnComplete(ResetMakingSpecialPieceStatus);

		seq.Append(bd.Piece.GO.transform.DOScale( startScale * 1.5F,0.2F).SetEase(Ease.Linear));
		seq.Append(bd.Piece.GO.transform.DOScale( startScale,0.2F).SetEase(Ease.Linear));		
        seq.Play();
    }
    void ResetMakingSpecialPieceStatus() {
        GM.makingSpecialPiece = false;
    }

    void OnGameReady ()
    {
        spawned                 = false;
        moveForThis             = 0;
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.addTimeBlockSpawnCountPerMove);
    }
    void OnPlayerMove ()
    {
        OnBoardStable();

        if (Zellato.TotalCount < JMFUtils.GM.CurrentLevel.maxAddTimeBlockCount) {
            moveForThis++;
        }
    }
    void OnBoardStable ()
    {
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.addTimeBlockSpawnCountPerMove);
        if (spawned)
        {
            moveForThis         = 0;
            spawned             = false;
        }
    }
    public override PieceDefinition GetSpawnableDefinition (Point pt)
    {
        if (GM.CurrentLevel.enableAddTimeBlock == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.addTimeBlockSpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.addTimeBlockSpawnColumn) {
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
        if (moveForThis < GM.CurrentLevel.movePerAddTimeBlock) return false;
        if (BlockAddTime.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxAddTimeBlockCount)) return false;

        if (BlockAddTime.TotalCount < Mathf.Max(1, GM.CurrentLevel.minAddTimeBlockCount))
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
}
