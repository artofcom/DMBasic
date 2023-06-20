using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [SUGAR_CHERRY]
public class SugarCherryPiece : PieceDefinition {

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
        return (int)LEItem.COLOR.SUGAR_CHERRY - 1;
    }

    public override float ShowDestroyEffect (GamePiece gp) 
	{       
        BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		effect.Play("play",gp.Position,gp.Scale, 0);//, false);

        NNSoundHelper.Play( "IFX_sugar_block_match" );
		return effectTime;
    }

    #region => old code.. comment.
    /*public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) 
	{
        if ((linkX.Count < 2) && (linkY.Count < 2)) 
		{
            score               = 0;
            return false;
        }

        #region => they all must be same as this type (sugarcherry).
        for(int g = 0; g < linkX.Count; ++g)
        {
            if(linkX[g].PD is SugarCherryPiece == false)
            {
                score           = 0;
                return false;
            }
        }
        for(int g = 0; g < linkY.Count; ++g)
        {
            if(linkY[g].PD is SugarCherryPiece == false)
            {
                score           = 0;
                return false;
            }
        }
        #endregion

        #region => old code.. comment.
        /* Board frogBoard = FindFrogBoard(bd, linkX, linkY);

         if (frogBoard != null) 
         {
             Frog frog = frogBoard.Piece.GO.GetComponent<Frog>();

             frog.EatingCount += linkX.Count;
             frog.EatingCount += linkY.Count;

             FeedToFrog(frogBoard, bd, linkX);
             FeedToFrog(frogBoard, bd, linkY);
         }*/
         //#endregion
        /*
        // position & Delay check.
        GM.ShootShockWave(bd.PT, 4.5f, 1F, 0.5F, false, 0.2f);
		score                   = bd.Hit(true);

        return true;
	}*/
    #endregion

	public override string GetImageName (int colorIndex) 
	{
		return "SugarCherry";
	}
	
    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        int score               = 0;
        //if (isByMatch == false) score += destroyScore;
        score += destroyScore;

        //if (gp.Owner.ShadedDurability < 0)
		//{
        //    gp.Owner.eShadeType = (int)LEItem.SHADE_TYPE.JAM;
        //    gp.Owner.ShadedDurability   = 0;
        //}
        _resetSugar(gp);

        if (gp.Owner.ShadedDurability >= 0)
		{
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

    // 주면의 노멀셀을 슈가셀로 변경한다.
    void _resetSugar(GamePiece gp)
    {        
        Board target            = null!=gp.Owner ? gp.Owner.Left : null;
        if(null!=target && target.PD is NormalPiece && target.Piece.LifeCover<=0)
        {
            target.Piece.Reset(GM.PieceTypes[22], target.Piece.ColorIndex, target.Piece.Durability);
            _triggerSugarEffect(target.Piece);
        }
        target                  = null!=gp.Owner ? gp.Owner.Right : null;
        if(null!=target && target.PD is NormalPiece && target.Piece.LifeCover<=0)
        {
            target.Piece.Reset(GM.PieceTypes[22], target.Piece.ColorIndex, target.Piece.Durability);
            _triggerSugarEffect(target.Piece);
        }
        target                  = null!=gp.Owner ? gp.Owner.Top : null;
        if(null!=target && target.PD is NormalPiece && target.Piece.LifeCover<=0)
        {
            target.Piece.Reset(GM.PieceTypes[22], target.Piece.ColorIndex, target.Piece.Durability);
            _triggerSugarEffect(target.Piece);
        }
        target                  = null!=gp.Owner ? gp.Owner.Bottom : null;
        if(null!=target && target.PD is NormalPiece && target.Piece.LifeCover<=0)
        {
            target.Piece.Reset(GM.PieceTypes[22], target.Piece.ColorIndex, target.Piece.Durability);
            _triggerSugarEffect(target.Piece);
        }
    }

    void _triggerSugarEffect(GamePiece gp)
    {
        //SpineEffect crash       = NNPool.GetItem<SpineEffect>("SugarBlockHit");        
        //crash.play("play", .0f);
        //crash.Reset(gp.Owner.Position);

        BlockCrash effect       = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		effect.Play("play", gp.Position, gp.Scale, gp.ColorIndex);//, false);
    }

    void OnGameReady ()
    {
        spawned                 = false;
        moveForThis             = 0;
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.sugarCherrySpawnCountPerMove);
    }
    void OnPlayerMove ()
    {
        OnBoardStable();

        if (SugarCherry.TotalCount < JMFUtils.GM.CurrentLevel.maxSugarCherryCount) {
            moveForThis++;
        }
    }
    void OnBoardStable ()
    {
        spawnableCount          = Mathf.Max(1, GM.CurrentLevel.sugarCherrySpawnCountPerMove);
        if (spawned)
        {
            moveForThis         = 0;
            spawned             = false;
        }
    }
    public override PieceDefinition GetSpawnableDefinition (Point pt)
    {
        if (GM.CurrentLevel.enableSugarCherry == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.sugarCherrySpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.sugarCherrySpawnColumn) {
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
        if (moveForThis < GM.CurrentLevel.movePerSugarCherry) return false;
        if (SugarCherry.TotalCount >= Mathf.Max(1, GM.CurrentLevel.maxSugarCherryCount)) return false;

        if (SugarCherry.TotalCount < Mathf.Max(1, GM.CurrentLevel.minSugarCherryCount))
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
