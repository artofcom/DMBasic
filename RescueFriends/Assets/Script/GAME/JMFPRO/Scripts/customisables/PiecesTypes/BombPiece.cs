using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;

public class BombPiece : PieceDefinition {
    public float startDelay = 0.15F;
    public float sequenceDelay = 0.05F;

	protected override void OnPieceCreate (GamePiece gp) {
        if(JMFUtils.GM.State==JMF_GAMESTATE.PLAY && false==JMFUtils.GM._isRemovedByConveyor)
            NNSoundHelper.Play("IFX_bomb_block_create");
	}

	public override string GetImageName (int colorIndex) 
	{
		string fileName = null;
		switch(colorIndex)
		{
			case 0: fileName = "block_red_B"; break;
			case 1: fileName = "block_yellow_B"; break;
			case 2: fileName = "block_green_B"; break;
			case 3: fileName = "block_blue_B"; break;
			case 4: fileName = "block_purple_B"; break;
			case 5: fileName = "block_orange_B"; break;
			case 6: fileName = "block_skyblue_B"; break;
			case 7: fileName = "block_violet_B"; break;
		}
        /*
        string fileName = null;
		switch(colorIndex)
		{
			case 0: fileName = "block_red_X"; break;
			case 1: fileName = "block_yellow_X"; break;
			case 2: fileName = "block_green_X"; break;
			case 3: fileName = "block_blue_X"; break;
			case 4: fileName = "block_purple_X"; break;
			case 5: fileName = "block_orange_X"; break;
			case 6: fileName = "block_skyblue_X"; break;
			case 7: fileName = "block_violet_X"; break;
		}
        */
		return fileName;
	}
	
    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        // 특수블럭 미션 counting.
        if (JMFUtils.GM.CurrentLevel.isGetTypesGame && JMFUtils.GM.CurrentLevel.numToGet.Length>colorIndex && 
            JMFUtils.GM.CurrentLevel.numToGet[colorIndex]>0 && false==JMFUtils.GM.isAIFightMode) 
		{
            JMFUtils.GM.JewelMatchCounts[colorIndex]++;
            int remainCount = JMFUtils.GM.CurrentLevel.numToGet[colorIndex] - JMFUtils.GM.JewelMatchCounts[colorIndex];
            if (remainCount >= 0) 
			{
                JMFRelay.FireOnCollectJewelForDisplay(colorIndex, remainCount);
            }
        }
        else
        {
            if(null != JMFUtils.GM.BlockMissionPanels[colorIndex])
            {
                if(JMFUtils.GM.BlockMissionPanels[colorIndex].Panel.Durability >= 0)
                {
                    JMFUtils.GM.AnimateGainBlockToMissionPannel(gp, JMFUtils.GM.BlockMissionPanels[colorIndex]);
                }
            }
        }


        // for auto event.
        JMFRelay.FireOnCollectPieceForAutoEvent(0, 3 ,gp.LocalPosition);

        int score = destroyScore;
        if (gp.Owner.ShadedDurability >= 0)
		{
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

//#if BOMBx2
    protected override void OnPieceDestroyed (Board bd, int prevColor) 
	{
        // note : 폭탄 2번 터짐 기능 제거. by wayne [17.06.02]
        //if (GM.DestroyAwaiter.Contains(bd.Piece) == false) 
		//{
        //    bd.ResetPiece(this, prevColor);
        //    bd.Piece.ShineAndBlink();
        //    GM.DestroyAwaiter.Add(bd.Piece);
        //}
    }
//#endif

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) {
        if( ((linkX.Count < 2) || (linkY.Count < 2)) || bd.ColorIndex>=(int)(LEItem.COLOR.STRAWBERRY-1) )
        {
            score = 0;
            return false;
        }

        /*
        SwapFrogPiece(ref bd, linkX, linkY);

        Board frogBoard = FindFrogBoard(bd, linkX, linkY);

        if (frogBoard != null) {
            Frog frog = frogBoard.Piece.GO.GetComponent<Frog>();

            frog.EatingCount += linkX.Count;
            frog.EatingCount += linkY.Count;

            FeedToFrog(frogBoard, bd, linkX);
            FeedToFrog(frogBoard, bd, linkY);
        }

        SwapMysteryPiece(ref bd, linkX, linkY);
        */

        Board _bd = bd;

        int colorIndex = bd.ColorIndex;
        score = createScore;

        //if (IsSpecialPiece(_bd)) {
        //    PerformCombinationPower(_bd, _bd, _bd.Piece, _bd.Piece);
        //} else 
        {
            GM.makingSpecialPiece = true;

            // 이 매칭으로 생성되는 특수블럭은, 연쇄 효과때 바로 터지지 않는다.(그대로 놔둠.)
            for(int z = 0; z < linkX.Count; ++z)
                linkX[z].SkipTarget = _bd;
            for(int z = 0; z < linkY.Count; ++z)
                linkY[z].SkipTarget = _bd;

            // == old code ; score += _bd.Hit(true, () => {            
            //bd.Piece.triggerBombMultiBursting();
			score += _bd.Hit(true); 
            DOVirtual.DelayedCall(JMFUtils.GM.delayForMakingSpecialPiece, () => {

				_bd.ResetPiece(this, colorIndex);
                // == _bd.Piece.Play("create",false,0.0f,ResetMakingSpecialPieceStatus);
                _bd.Piece.Play("create",false,0.0f,null);
                ResetMakingSpecialPieceStatus();
                //

                PanelNetHit effect = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
				effect.ChangeColor(1, colorIndex);
				effect.Play(_bd.Piece.Position, _bd.Piece.Scale,false, .0f, 1.0f, _bd.Piece.GO.transform);
            });
        }
        return true;
	}

    public override float ShowDestroyEffect (GamePiece gp) 
	{
		return 0.3f;// effectTime;
    }

	public override void PerformPower (GamePiece gp) {

        // StartCoroutine( _coPerformPower(gp.Owner, null, 1) );
        Block blk               = gp.GO.GetComponent<Block>();
        if(null != blk)         blk.PerformPower();

        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
    }

    /*IEnumerator _coPerformPower(Board bdMain, Board from, int radius)
    {
        // wait till all special piece finished.
        while (GM.makingSpecialPiece)
            yield return null;

		PerformBombPower(bdMain, from, radius, 0.2f);
	}*/

	public override bool IsCombinable ( GamePiece other) 
	{ 
		if (other.PD is HorizontalPiece) return true;
        if (other.PD is VerticalPiece) return true;
        if (other.PD is BombPiece) return true;
        return false; 
    }

	public override float PerformCombinationPower (Board from, Board to, GamePiece gp, GamePiece other)
    {
		if ((other.PD is HorizontalPiece) || (other.PD is VerticalPiece))
        {
            //StartCoroutine(CoPerformLineBombPower(from, to, gp, other));
            Block blk           = to.Piece.GO.GetComponent<Block>();
            if(null != blk)     blk.PerformLineBombPower(from, to, gp, other);

            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.LB);
        } else if (other.PD is BombPiece) {
            //StartCoroutine(CoPerformBombBombPower(from, to, gp, other));

            Block blk           = to.Piece.GO.GetComponent<Block>();
            if(null != blk)     blk.PerformBombBombPower(from, to, gp, other);

            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.BB);
        }
        return startDelay;
	}

   /* IEnumerator CoPerformLineBombPower (Board from, Board to, GamePiece gp, GamePiece other) {
        int score = gp.PD.destroyScore + other.PD.destroyScore;

		PerformLineBombPower(to, from);

        from.Hit(false, .0f);
        to.Hit(false, 0.125f);
        yield return new WaitForSeconds(0.4f);  // note : 시각 효과가 극대화 되는 시간까지 delay.
        //float aniDelay          = to.Piece.Play("destroy", false, .0f);
        //yield return new WaitForSeconds(aniDelay);
        //to.RemovePiece(.0f);
        //to.RemovePiece(startDelay);
        //yield return new WaitForSeconds(startDelay);

        if (to.ShadedDurability >= 0) {
            to.ShadedDurability--;
            score += GM.shadedScore;
        }

        GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }

    IEnumerator CoPerformBombBombPower (Board from, Board to, GamePiece gp, GamePiece other) {
        int score = gp.PD.destroyScore + other.PD.destroyScore;
		//PerformBombPower(to, 2, 0.4f);
        yield return StartCoroutine( _coPerformPower(to, from, 2) );

        //from.RemovePiece(startDelay);
        //to.RemovePiece(startDelay);

        from.Hit(false, .0f);
        to.Hit(false, 0.125f);
        yield return new WaitForSeconds(0.4f);  // note : 시각 효과가 극대화 되는 시간까지 delay.
        //float aniDelay          = to.Piece.Play("destroy", false, .0f);
        //yield return new WaitForSeconds(aniDelay);
        //to.RemovePiece(.0f);

        if (to.ShadedDurability >= 0)
		{
            to.ShadedDurability--;
            score += GM.shadedScore;
        }

        GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }
    
	void PerformBombPower (Board bdMain, Board bdFrom, int radius, float rate) 
	{
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add( bdFrom );
		float duration = GM.ShootShockWave(bdMain.PT, 5F, 1F, 0.5F,true,rate, listExceptions);

		BombHit effect = NNPool.GetItem<BombHit>("BombPieceCrash");
		
        float effDelay          = duration - 0.05f;
		effect.Play(bdMain.Position, bdMain.Piece.Scale * ((float)radius), bdMain.Piece.ColorIndex, false, effDelay);
		
        List<Board> boards = bdMain.GetBoardsFromArea(0, radius);
		DestroyBySequencial(bdMain, boards, duration, 0F, false, 0, bdMain.SkipTarget);

        bdMain.SkipTarget       = null;
        if (false==GM.ActMassMatching || true==bdMain.Piece.mPlayWhenMassMatching)
        {
            // bomb sound 2회 간격 두면서 play.
            DOVirtual.DelayedCall(effDelay-0.1f, () => NNSoundHelper.Play("IFX_bombblock_bust") );
            DOVirtual.DelayedCall(effDelay, () => NNSoundHelper.Play("IFX_bombblock_bust") );
        }
	}
    
	void PerformHorizontalPower (Board bd, Vector3 scale, int color,  float delay = 0.0f) 
	{
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("horizontal_hit",bd.Position,scale, color, false,delay);
		
        //float duration          = .0f;
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bd.Position, bd.Piece.Scale, color, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x+50, 2.0f);
        effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bd.Position, bd.Piece.Scale, color, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x-500, 2.0f);

        List<Board> boardsL = bd.GetBoardsInDirection(JMF_DIRECTION.LEFT);
        List<Board> boardsR = bd.GetBoardsInDirection(JMF_DIRECTION.RIGHT);
		DestroyBySequencial(bd, boardsL, delay, sequenceDelay, true, 1);
		DestroyBySequencial(bd, boardsR, delay, sequenceDelay, true, 1);
    }

	void PerformVerticalPower (Board bd, Vector3 scale, int color, float delay = 0.0f) 
	{
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("vertical_hit",bd.Position,scale, color, false,delay);

        BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bd.Position, bd.Piece.Scale, color, 3.0f);
        effect.transform.DOLocalMoveY(effect.transform.localPosition.y+50, 2.0f);
        effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bd.Position, bd.Piece.Scale, color, 3.0f);
        effect.transform.DOLocalMoveY(effect.transform.localPosition.y-500, 2.0f);

        List<Board> boardsT = bd.GetBoardsInDirection(JMF_DIRECTION.UP);
        List<Board> boardsB = bd.GetBoardsInDirection(JMF_DIRECTION.DOWN);
		DestroyBySequencial(bd, boardsT, delay, sequenceDelay, true, 1);
		DestroyBySequencial(bd, boardsB, delay, sequenceDelay, true, 1);
	}

	/*void PerformLineBombPower (Board bdMain, Board bdFrom) 
	{    
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add( bdFrom );
		float duration = GM.ShootShockWave(bdMain.PT, 7F, 1F, 0.5F,true,0.3f, listExceptions);

		BombHit effect = NNPool.GetItem<BombHit>("BombPieceCrash");
		//BombHit effect = NNPool.GetItem<BombHit>("BombBubbleHit");
		Vector3 pos = bdMain.Position;
		pos.z -= 2.0f; 
		effect.Play(pos,bdMain.Piece.Scale, bdMain.Piece.ColorIndex, false);
        // 이 sound는 채널 영향 받을 필요없다. -> 대규모로 동시에 터지지 않는다.
        DOVirtual.DelayedCall(duration, () =>
        {
            NNSoundHelper.Play("IFX_bombblock_bust");   // bomb sound .  
            NNSoundHelper.Play("IFX_lineblock_bust");   // linb sound.
        });

		List<Board> boards = bdMain.GetBoardsFromArea(0, 1);
		DestroyBySequencial(bdMain, boards, startDelay, 0F, false, 0, bdFrom);
		
		PerformHorizontalPower(bdMain,bdMain.Piece.Scale,bdMain.Piece.ColorIndex,duration);
		PerformVerticalPower(bdMain,bdMain.Piece.Scale,bdMain.Piece.ColorIndex,duration);

		if (bdMain.Top != null ) PerformHorizontalPower(bdMain.Top,bdMain.Piece.Scale,bdMain.Piece.ColorIndex, duration);
		if (bdMain.Bottom != null ) PerformHorizontalPower(bdMain.Bottom,bdMain.Piece.Scale,bdMain.Piece.ColorIndex,duration);
		if (bdMain.Left != null ) PerformVerticalPower(bdMain.Left,bdMain.Piece.Scale,bdMain.Piece.ColorIndex,duration);
		if (bdMain.Right != null ) PerformVerticalPower(bdMain.Right,bdMain.Piece.Scale,bdMain.Piece.ColorIndex,duration);		
	}
    
    void DestroyBySequencial (Board bdMain, List<Board> boards, float _startDelay, float _sequenceDelay, bool isLiner, int ignoreCount, Board skip=null)
	{
        for (int i = 0; i < boards.Count; i++) 
		{
            // if (isLiner && boards[i].PD is StonePiece) break;
            if (i < ignoreCount) continue;

            // note : rainbow block은 이 destory에서 제외. => 다른 방식으로 제외 로직 변경.
            //if(boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is SpecialFive) || (boards[i].PD is TMatch7Piece)))
            //    continue;
            //
            // skip(직전매칭으로 생긴 특수블럭)이 target이면 hit()에서 제외한다.
            if(null!=skip && boards[i]==skip && (skip.PD is VerticalPiece || skip.PD is HorizontalPiece || skip.PD is BombPiece))
                continue;
            if(null!=bdMain && bdMain.Equals(boards[i]))
                continue;

            // 특수 블럭은 waiter로 넘긴다.
            if(GM.State==JMF_GAMESTATE.PLAY && false==GM.ActMassMatching)
            {
                if (boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is VerticalPiece) || (boards[i].PD is HorizontalPiece) || (boards[i].PD is BombPiece)))
                {
                    if(null!=bdMain && false==bdMain.ListMatchings.Contains(boards[i]) && boards[i].Piece.LifeCover<=0)
                    {
                        // Debug.Log(string.Format(">>> B-Delayed Piece Applyed !!! {0}, {1}", boards[i].X, boards[i].Y));

                        boards[i].Piece.ShineAndBlink(_startDelay + (sequenceDelay*i));
                        GM.DestroyAwaiter.Add(boards[i].Piece);
                        //boards[i].Piece.triggerBombMultiBursting();
                        continue;
                    }

                    if(null!=bdMain && true==bdMain.ListMatchings.Contains(boards[i]))
                        Debug.Log("B DestroyBySequencial ) it's matching now...");

                    if (boards[i].Piece.LifeCover > 0) 
                        Debug.Log("B DestroyBySequencial ) it has Life cover !.");
                }
            }

            //if(null!=boards[i].Piece)   boards[i].Piece.triggerBombMultiBursting();
            if(null!=bdMain && null!=bdMain.Piece && null!=boards[i].Panel)
                boards[i].Panel.setDamagingColor( bdMain.Piece.ColorIndex + 1 );
            boards[i].Hit(_startDelay + (_sequenceDelay * i), true);

            // note : cut 하지 않는 것으로 변경.
            //if (isLiner && (boards[i].PD is RoundChocoPiece || boards[i].PD is GreenBubblePiece))
            //    break;
        }
    }*/

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


    // change piece's color by _changeColorIndex.
    public override bool canColorChange(Board bd)
    {
        if(null == bd)          return false;
        return (bd._changeColorIndex>=0 && null!=bd.Piece && bd.Piece.LifeCover<=0);
    }
    public override IEnumerator changePieceColorAsChanger(Board board, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);
                
        if(board._changeColorIndex==(int)LEItem.COLOR.RANDOM)
            board.ResetPiece(board.PD, GM.GetRandomColorIndex());
        else 
            board.ResetPiece(board.PD, board._changeColorIndex-1);

        board.AnimateColorChanger();

        AnimationNormalToSpecial(board);
    }

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is VerticalPiece))
            return false;

        if (JMFUtils.GM.CurrentLevel.isGetTypesGame && JMFUtils.GM.CurrentLevel.numToGet[gp.ColorIndex] > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.numToGet[gp.ColorIndex] - JMFUtils.GM.JewelMatchCounts[gp.ColorIndex];
            return (remainCount>=0);
        }

         // is there block mission pannel ?
        if(null != JMFUtils.GM.BlockMissionPanels[gp.ColorIndex])
            return (JMFUtils.GM.BlockMissionPanels[gp.ColorIndex].Panel.Durability > 0);
        return false;
    }
}
