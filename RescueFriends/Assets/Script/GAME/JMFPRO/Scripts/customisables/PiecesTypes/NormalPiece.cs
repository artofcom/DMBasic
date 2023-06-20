using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class NormalPiece : PieceDefinition 
{
	public override PieceDefinition GetSpawnableDefinition (Point pt) { return this; }

    //
    Board _bdCornor             = null; // for 2x2 match.
    public override void set2x2BoardInfo(Board boardCornor)
    {
        _bdCornor               = boardCornor;
    }
    //

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) 
	{
        if ((linkX.Count < 2) && (linkY.Count < 2)) 
		{
            score = 0;
            return false;
        }

        Board frogBoard = FindFrogBoard(bd, linkX, linkY);

        if (frogBoard != null) 
		{
            Frog frog = frogBoard.Piece.GO.GetComponent<Frog>();

            frog.EatingCount += linkX.Count;
            frog.EatingCount += linkY.Count;

            FeedToFrog(frogBoard, bd, linkX);
            FeedToFrog(frogBoard, bd, linkY);
        }

        //bd.Piece.triggerBombMultiBursting();
		score = bd.Hit(true);

        return true;
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
	
	public override int GetColorIndex () 
	{ 
		int colorIndex = GM.GetRandomColorIndex(); 
		return Mathf.Clamp(colorIndex, 0, 8);
	}

    // note : 실제보다 빠르게 진행되도록 특정 시간(play.unity에 저장) 간격을 반환한다.
    public override float ShowDestroyEffect (GamePiece gp)
	{
        NNSoundHelper.Play( "IFX_block_crush" );

		BlockCrash effect       = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		effect.Play("play",gp.Position,gp.Scale, gp.ColorIndex);//, false);
        //effect.Play(gp.Position);
        return 0.2f;
    }

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is NormalPiece))
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
        if(colorIndex < 0)      return 0;
        
        // CRASH TEST !!!
        //List<int> _listAA = null;
        //_listAA.Add( 10 );      // should crash !!!
        //

        // for auto event.
        JMFRelay.FireOnCollectPieceForAutoEvent(colorIndex+1, 0, gp.LocalPosition);

        // buff piece는 goal에 상관없이 실행.
        if(gp.NeedMakeBuffWhenDestroy)
            JMFUtils.GM.AnimateGainBuff(gp);

        if (JMFUtils.GM.CurrentLevel.isGetTypesGame && JMFUtils.GM.CurrentLevel.numToGet.Length>colorIndex && 
            JMFUtils.GM.CurrentLevel.numToGet[colorIndex]>0 && false==JMFUtils.GM.isAIFightMode) 
		{
            JMFUtils.GM.JewelMatchCounts[colorIndex]++;
            int remainCount = JMFUtils.GM.CurrentLevel.numToGet[colorIndex] - JMFUtils.GM.JewelMatchCounts[colorIndex];
            if (remainCount >= 0) 
			{
				JMFUtils.GM.AnimateGainJewel(gp, remainCount, colorIndex);
                JMFRelay.FireOnCollectJewel(colorIndex, remainCount);
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
        
        if(board._changeColorIndex==(int)LEItem.COLOR.RANDOM)
            board.ResetPiece(board.PD, GM.GetRandomColorIndex());
        else 
            board.ResetPiece(board.PD, board._changeColorIndex-1);

        board.AnimateColorChanger();

        AnimationNormalToSpecial(board);
    }

    void AnimationNormalToSpecial(Board bd)
	{
        Vector3 startScale = bd.Piece.GO.transform.localScale;
		Sequence seq = DOTween.Sequence();
		//seq.OnComplete(ResetMakingSpecialPieceStatus);
		seq.Append(bd.Piece.GO.transform.DOScale( startScale * 1.5F,0.2F).SetEase(Ease.Linear));
		seq.Append(bd.Piece.GO.transform.DOScale( startScale,0.2F).SetEase(Ease.Linear));
		
        seq.Play();
    }
}
