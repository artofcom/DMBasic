using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SugaredPiece : PieceDefinition 
{
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
			case 0: fileName = "block_red_S"; break;
			case 1: fileName = "block_yellow_S"; break;
			case 2: fileName = "block_green_S"; break;
			case 3: fileName = "block_blue_S"; break;
			case 4: fileName = "block_purple_S"; break;
			case 5: fileName = "block_orange_S"; break;
			case 6: fileName = "block_skyblue_S"; break;
			case 7: fileName = "block_violet_S"; break;
		}

		return fileName;
	}

    public override bool isLevelMissionTarget(GamePiece gp)
    {
        if(null==gp)            return false;
        if(false == (gp.PD is SugaredPiece))
            return false;

        if (JMFUtils.GM.CurrentLevel.countSugarBlock > 0)
        {
            int remainCount = JMFUtils.GM.CurrentLevel.countSugarBlock - JMFUtils.GM.countMatchSugarBlock;
            return (remainCount>=0);
        }
        return false;

    }

    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) 
	{
        if (JMFUtils.GM.CurrentLevel.countSugarBlock > 0) 
		{
            JMFUtils.GM.countMatchSugarBlock++;
            int remainCount     = JMFUtils.GM.CurrentLevel.countSugarBlock - JMFUtils.GM.countMatchSugarBlock;
            if (remainCount >= 0) 
			{
				JMFUtils.GM.AnimateGainSugarJewel(gp, remainCount, gp.ColorIndex);
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
}
