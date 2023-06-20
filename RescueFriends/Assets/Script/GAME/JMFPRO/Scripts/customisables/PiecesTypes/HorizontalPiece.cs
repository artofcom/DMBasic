using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class HorizontalPiece : PieceDefinition {
    public float startDelay = 0.1F;
    public float sequenceDelay = 0.05F;

	protected override void OnPieceCreate (GamePiece gp) {
        if (JMFUtils.GM.State==JMF_GAMESTATE.PLAY && false==JMFUtils.GM._isRemovedByConveyor)
            NNSoundHelper.Play("IFX_line_block_create");
	}

	public override string GetImageName (int colorIndex) 
	{
		string fileName = null;
		switch(colorIndex)
		{
			case 0: fileName = "block_red_H"; break;
			case 1: fileName = "block_yellow_H"; break;
			case 2: fileName = "block_green_H"; break;
			case 3: fileName = "block_blue_H"; break;
			case 4: fileName = "block_purple_H"; break;
			case 5: fileName = "block_orange_H"; break;
			case 6: fileName = "block_skyblue_H"; break;
			case 7: fileName = "block_violet_H"; break;
		}

		return fileName;
	}
	
    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is HorizontalPiece))
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
        JMFRelay.FireOnCollectPieceForAutoEvent(0, 1 ,gp.LocalPosition);

        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) {
		if (linkY.Count<3 || bd.ColorIndex>=(int)(LEItem.COLOR.STRAWBERRY-1) )
        {
            score = 0;
            return false;
        }

        SwapFrogPiece(ref bd, linkX, linkY);

        Board frogBoard = FindFrogBoard(bd, linkX, linkY);

        if (frogBoard != null) {
            Frog frog = frogBoard.Piece.GO.GetComponent<Frog>();

            frog.EatingCount += linkX.Count;
            frog.EatingCount += linkY.Count;

            FeedToFrog(frogBoard, bd, linkX);
            FeedToFrog(frogBoard, bd, linkY);
        }
// tod check list
//        SwapMysteryPiece(ref bd, linkX, linkY);

        Board _bd = bd;
        int colorIndex = bd.ColorIndex;
        score = createScore;
        // note : 이번 게임이 이 로직은 적용되지 않도록 한다.
        //if (IsSpecialPiece(_bd)) {
        //    PerformCombinationPower(_bd, _bd, _bd.Piece, _bd.Piece);
        //} else 
        {
            GM.makingSpecialPiece = true;
 
            // 이 매칭으로 생성되는 특수블럭은, 연쇄 효과때 바로 터지지 않는다.(그대로 놔둠.)           
            for(int z = 0; z < linkY.Count; ++z)
                linkY[z].SkipTarget = _bd;

            // == old code ; score += _bd.Hit(true, () => {
            //bd.Piece.triggerBombMultiBursting();
			score += _bd.Hit(true); 
            DOVirtual.DelayedCall(JMFUtils.GM.delayForMakingSpecialPiece, () => {
                
                _bd.ResetPiece(this, colorIndex); 
				//_bd.Piece.Play("create",false,0.0f,ResetMakingSpecialPieceStatus);
                _bd.Piece.Play("create",false,0.0f,null);
                ResetMakingSpecialPieceStatus();
                //
                PanelNetHit effect = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
				effect.ChangeColor(1, _bd.Piece.ColorIndex);
				effect.Play(_bd.Piece.Position, _bd.Piece.Scale,false, .0f, 1.0f, _bd.Piece.GO.transform);
            });
        }
        return true;
	}

    public override float ShowDestroyEffect (GamePiece gp) {

        /*
         * 
        reffered at PerformHorizontalPower()

        BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play",gp.Position,gp.Scale, gp.ColorIndex, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x+50, 2.0f);
        //Sequence doSeq          = DOTween.Sequence();
        //doSeq.PrependInterval(0.01f);
        //doSeq.Append( effect.transform.DOLocalMoveX(effect.transform.localPosition.x+100, 4.0f) );

        effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play",gp.Position,gp.Scale, gp.ColorIndex, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x-500, 2.0f);
        //Sequence doSeq2          = DOTween.Sequence();
        //doSeq2.PrependInterval(0.01f);
        //doSeq2.Append( effect.transform.DOLocalMoveX(effect.transform.localPosition.x-100, 4.0f) );
        */

        return 0.2f;// effectTime;
    }

	public override void PerformPower (GamePiece gp)
    {
        Block blk               = gp.GO.GetComponent<Block>();
        if(null != blk)         blk.PerformPower();

         //StartCoroutine( _coPerformHorizontalPower(gp.Owner, null) );
	}

    /*IEnumerator _coPerformHorizontalPower(Board bdMain, Board bdFrom, System.Action<float> callback=null)
    {
        // wait till all special piece finished.
        while (GM.makingSpecialPiece)
            yield return null;

		float fDuration =        PerformHorizontalPower(bdMain, bdFrom);
        if(null!=callback)      callback(fDuration);
	}
	
	float PerformHorizontalPower (Board bdMain, Board bdFrom) 
	{
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add(bdFrom);

		//float duration = GM.ShootShockWave(bdMain.PT, 5F, 1F, 0.5F,true,0.2f, listExceptions);
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("horizontal_hit",bdMain.Position,bdMain.Piece.Scale, bdMain.Piece.ColorIndex, false,duration);

        float duration          = .0f;
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bdMain.Position, bdMain.Piece.Scale, bdMain.ColorIndex, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x+50, 2.0f);
        effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		effect.Play("play", bdMain.Position, bdMain.Piece.Scale, bdMain.ColorIndex, 3.0f);
        effect.transform.DOLocalMoveX(effect.transform.localPosition.x-500, 2.0f);

        List<Board> boardsL = bdMain.GetBoardsInDirection(JMF_DIRECTION.LEFT);
        List<Board> boardsR = bdMain.GetBoardsInDirection(JMF_DIRECTION.RIGHT);
		DestroyBySequencial(bdMain, boardsL,duration, bdMain.SkipTarget);
		DestroyBySequencial(bdMain, boardsR,duration, bdMain.SkipTarget);

        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);

        bdMain.SkipTarget           = null;

        if(false==GM.ActMassMatching || true==bdMain.Piece.mPlayWhenMassMatching)
        {
            //if(null!=bdFrom)        // combination power일때는 sound 2 회 출력.
            //    DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
            DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
        }

		return duration;
	}*/

	public override bool IsCombinable (GamePiece other) 
	{ 
		if (other.PD is HorizontalPiece) return true;
        if (other.PD is VerticalPiece) return true;
        return false; 
    }

	public override float PerformCombinationPower (Board from, Board to, GamePiece gp, GamePiece other) {
        //StartCoroutine(CoPerformCombinationPower(from, to, gp, other));

        Block blk               = to.Piece.GO.GetComponent<Block>();
        if(null != blk)         blk.PerformCrossCombinationPower(from, to, gp, other);

        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.LL);
        return startDelay;
	}

    /*IEnumerator CoPerformCombinationPower (Board from, Board to, GamePiece gp, GamePiece other) {
        int score = gp.PD.destroyScore + other.PD.destroyScore;

        // 터짐 효과 추가.
        BombHit effectB         = NNPool.GetItem<BombHit>("BombPieceCrash");// "BombBubbleHit");
		Vector3 pos             = to.Position;
		pos.z -= 2.0f; 
		effectB.Play(pos, to.Piece.Scale*0.35f, to.Piece.ColorIndex, false);
        // 

		//float duration = PerformHorizontalPower(to);
        float duration          = .0f;
        yield return StartCoroutine( _coPerformHorizontalPower(to, from, (fRet) =>
        {
            duration            = fRet;
        }) );

		BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		
		effect.Play("vertical_hit",to.Position,to.Piece.Scale, other.ColorIndex, false,duration);
		
        //from.RemovePiece(duration);
		//to.RemovePiece(duration);
        from.Hit(false, .0f);
        to.Hit(false, 0.145f);

        List<Board> boardsT = to.GetBoardsInDirection(JMF_DIRECTION.UP);
        List<Board> boardsB = to.GetBoardsInDirection(JMF_DIRECTION.DOWN);
		DestroyBySequencial(from, boardsT,duration);
		DestroyBySequencial(from, boardsB,duration);

		yield return new WaitForSeconds(duration);

        if (to.ShadedDurability >= 0) {
            to.ShadedDurability--;
            score += GM.shadedScore;
        }

        GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }
	
	void DestroyBySequencial(Board bdMain, List<Board> boards,float delay = 0.0f, Board skip=null)
	{
        for (int i = 0; i < boards.Count; i++) 
		{
            // if (boards[i].PD is StonePiece) break;
            // note : cut 하지 않는 것으로 변경.
            // bool isSpiralSnow = boards[i].PD is RoundChocoPiece;
            // if (isSpiralSnow) break;

            // note : rainbow block은 이 destory에서 제외. => 다른 방식으로 제외 로직 변경.
            //if(boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is SpecialFive) || (boards[i].PD is TMatch7Piece)))
            //    continue;
            //
            // skip(직전매칭으로 생긴 특수블럭)이 target이면 hit()에서 제외한다.
            if(null!=skip && boards[i]==skip && (skip.PD is VerticalPiece || skip.PD is HorizontalPiece || skip.PD is BombPiece))
                continue;
            if(null != bdMain && bdMain.Equals(boards[i]))
                continue;

            // 매칭 목록에 없고, 특수 블럭은 waiter로 넘긴다.
            if(GM.State==JMF_GAMESTATE.PLAY && false==GM.ActMassMatching)
            {
                if(boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is VerticalPiece) || (boards[i].PD is HorizontalPiece) || (boards[i].PD is BombPiece)))
                {
                    if(null!=bdMain && false==bdMain.ListMatchings.Contains(boards[i]) && boards[i].Piece.LifeCover<=0)
                    {                        
                        // Debug.Log(string.Format(">>> H-Delayed Piece Applyed !!! {0}, {1}", boards[i].X, boards[i].Y));

                        boards[i].Piece.ShineAndBlink(delay + (sequenceDelay*i));
                        GM.DestroyAwaiter.Add(boards[i].Piece);
                        //boards[i].Piece.triggerBombMultiBursting();
                        continue;
                    }

                    if(null!=bdMain && true==bdMain.ListMatchings.Contains(boards[i]))
                        Debug.Log("H DestroyBySequencial ) it's matching now...");

                    if (boards[i].Piece.LifeCover > 0) 
                        Debug.Log("H DestroyBySequencial ) it has Life cover !.");
                }
            }

            //if(null!=boards[i].Piece) boards[i].Piece.triggerBombMultiBursting();
            if(null!=bdMain && null!=bdMain.Piece && null!=boards[i].Panel)
                boards[i].Panel.setDamagingColor( bdMain.Piece.ColorIndex + 1 );
            boards[i].Hit(delay + (sequenceDelay*i), true);
        }		
    }*/

//    IEnumerator ConvertNormalToSpecial(Board bd, int colorIndex) {
//        yield return new WaitForSeconds(0.6F);
//        bd.ResetPiece(this, colorIndex); 
//        AnimationNormalToSpecial(bd);
//    }

//    void AnimationNormalToSpecial(Board bd) {
//        Vector3 startScale = bd.Piece.GO.transform.localScale;
//		Sequence seq = DOTween.Sequence();
//		seq.OnComplete(ResetMakingSpecialPieceStatus);
//
//		seq.Append(bd.Piece.GO.transform.DOScale( startScale * 1.5F,0.2F).SetEase(Ease.Linear));
//		seq.Append(bd.Piece.GO.transform.DOScale(startScale,0.2F).SetEase(Ease.Linear));
//        seq.Play();
//
//		
//		
////        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("ConvertToSpecial", bd.Piece.GO.transform);
////        pp.transform.localPosition = Vector3.zero;
////        pp.Play();
//    }

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
}
