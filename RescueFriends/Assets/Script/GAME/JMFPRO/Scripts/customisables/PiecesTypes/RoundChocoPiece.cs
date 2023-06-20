using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [ROUND_CHOCO]
public class RoundChocoPiece : PieceDefinition {
	public override int GetColorIndex () {return 0;}

    public static bool DestroyedThisTurn { get; private set; }
    //public static bool FactoryLevel;

    bool spawned;
    int moveForSpiralSnow = 0;
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

    void OnGameReady () {
        spawned = false;
        moveForSpiralSnow = 0;
        DestroyedThisTurn = false;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.spiralSnowSpawnCountPerMove);

        //List<Board> factorys    = GM.GetBoards<SnowManFactoryPanel>();
        //FactoryLevel            = factorys.Count > 0;
    }

    void OnPlayerMove () {

        OnBoardStable();

        if (SpiralSnow.TotalCount < JMFUtils.GM.CurrentLevel.maxSpiralSnowCount) {
            moveForSpiralSnow++;
        }
    }

    void OnBoardStable () {
        DestroyedThisTurn = false;
        spawnableCount = Mathf.Max(1, GM.CurrentLevel.spiralSnowSpawnCountPerMove);

        if (spawned) {
            moveForSpiralSnow = 0;
            spawned = false;
        }
    }

	public override string GetImageName (int colorIndex) 
	{
		if(colorIndex > -1 && colorIndex < 3)
			return string.Format("RoundchocoL{0}",colorIndex +1);

		return null;
	}
	
	protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        DestroyedThisTurn = true;
        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("IFX_soft_bust");

        return destroyScore;
	}

    protected override void OnPieceDestroyed (Board bd, int prevColor)
    {
        if( JMFUtils.GM.CurrentLevel.countRoundChocho > 0)
        {
            JMFUtils.GM.countMatchRoundChocho++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countRoundChocho - JMFUtils.GM.countMatchRoundChocho;
            if(remainCount >= 0)
            {
				JMFUtils.GM.AnimationMissionObject(bd, remainCount, "RoundChoco", GetImageName(bd.Piece.Durability +1));
            }
        }
    }

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is RoundChocoPiece))
            return false;

        if (JMFUtils.GM.CurrentLevel.countRoundChocho > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.countRoundChocho - JMFUtils.GM.countMatchRoundChocho;
            return (remainCount>=0);
        }
        return false;
    }

	public override PieceDefinition GetSpawnableDefinition (Point pt) {
        if (JMFUtils.GM.CurrentLevel.enableSpiralSnow == false) return null;

        bool isChosenColumn = false;

        if(null == JMFUtils.GM.CurrentLevel.roundChocoSpawnColumn)
            return null;

        foreach (int i in JMFUtils.GM.CurrentLevel.roundChocoSpawnColumn) {
            if (i == pt.X) {
                isChosenColumn = true;
                break;
            }
        }

        if (isChosenColumn == false) return null;

        return IsChanceOfSpawn() ? this : null;
	}

    public override bool IsChanceOfSpawn () {

        //if (DestroyedThisTurn && FactoryLevel) return false;
        if (moveForSpiralSnow < JMFUtils.GM.CurrentLevel.movePerSpiralSnow) return false;
        if (SpiralSnow.TotalCount >= Mathf.Max(1, JMFUtils.GM.CurrentLevel.maxSpiralSnowCount)) return false;

        if (SpiralSnow.TotalCount < Mathf.Max(1, JMFUtils.GM.CurrentLevel.minSpiralSnowCount)) {
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

        if(0 < gp.Durability)
        {
            gp.Play("hit_blending", false);  // 0.2F;
            return 0.35f;
        }
        else
            return .0f;
    }
    public override bool isShowDestroyAnimation()
    {
        return false;
    }
}
