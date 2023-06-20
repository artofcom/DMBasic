using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE.Diagnostics;

// [2X2_BURST]
public class RandLine2x23Piece : PieceDefinition {
    public float startDelay = 0.15F;
    public float sequenceDelay = 0.05F;

    List<Board> mListBurstTarget = new List<Board>();
    public List<Board> getListBurstTarget() { return mListBurstTarget; }

	protected override void OnPieceCreate (GamePiece gp) {
        //=if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) NNSoundHelper.Play("4m");
	}

    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        return 0;
    }

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score)
	{
        // note : Always reset !, so should use list mListBurstTarget right after call this func !!!
        mListBurstTarget.Clear();

        if(linkX.Count>=1 && linkY.Count>=1)
        {
            // x축에 대해서 matchable 목록을 찾는다. - 1)
            // y축에 대해서 matchable 목록을 찾는다. - 2)
            // 1)과 2) 목록에서 공통되게 나오는 녀석이 있으면 true 아니면 false. - 3)
            //
            // linkX, linkY, 3)에서 찾은 녀석을 포함하는 match 목록까지 bust 하면 된다.
            
            List<Board> exX     = new List<Board>();
            List<Board> exY     = new List<Board>();
            // 1) 
            for(int z = 0; z < linkX.Count; ++z)
                JMFUtils.AddItemWithoutConflict(ref exX, JMFUtils.GM.GetColMatchableBoards(linkX[z], bd.ColorIndex, JMF_DIRECTION.NONE));
            // 2)
            for(int z = 0; z < linkY.Count; ++z)
                JMFUtils.AddItemWithoutConflict(ref exY, JMFUtils.GM.GetRowMatchableBoards(linkY[z], bd.ColorIndex, JMF_DIRECTION.NONE));

            // 3) 
            int countCornor     = 0;
            for(int u = 0; u < exX.Count; ++u)
            {
                if(true == exY.Contains( exX[u] ))
                {
                    ++countCornor;
                    if(false == mListBurstTarget.Contains(exX[u]))
                        mListBurstTarget.Add( exX[u] );
                    JMFUtils.AddItemWithoutConflict( ref mListBurstTarget, JMFUtils.GM.GetColMatchableBoards(exX[u], bd.ColorIndex, JMF_DIRECTION.NONE) );
                    JMFUtils.AddItemWithoutConflict( ref mListBurstTarget, JMFUtils.GM.GetRowMatchableBoards(exX[u], bd.ColorIndex, JMF_DIRECTION.NONE) );
                }
            }
            if(0 == mListBurstTarget.Count)
            {
                score = 0;
                return false;
            }


            int colorIndex      = bd.ColorIndex;
            score               = createScore;

            // 특수 셀은 생성 루틴을 타지 않는다.
            if(colorIndex >= (int)(LEItem.COLOR.STRAWBERRY-1))
            {
                score += bd.Hit(true);
                return true;
            }

            // 3x2, 총 6개가 모두 같은 색의 perfact match 인 경우, 특수 블럭을 하나 더 만들어 준다.
            if(countCornor >= 2)
            {
                StartCoroutine( _coMakeSpecialPiece(0.4f, linkX, linkY, bd, mListBurstTarget) );
            }

            // 
            StartCoroutine( _coMakeSpecialPiece(0.3f, linkX, linkY, bd, mListBurstTarget) );

            // 동시에 line bomb 만드는 조건이 되면 그것도 수행.
            if(linkX.Count >= 3)    _makeLineBoosterPiece(GM.PieceTypes[2], bd, linkX, colorIndex);
            if(linkY.Count >= 3)    _makeLineBoosterPiece(GM.PieceTypes[1], bd, linkY, colorIndex);

            //bd.Piece.triggerBombMultiBursting();
            score += bd.Hit(true);

            return true;
        }

        score = 0;
        return false;
        
        /*
        SwapMysteryPiece(ref bd, linkX, linkY);
        */
        //return true;
	}

    // line bome 만들 조건이 되면 그것도 만들어 준다.
    void _makeLineBoosterPiece(PieceDefinition pd, Board bd, List<Board> listBoard, int idxColor)
    {
        if(null==pd || null==bd || null==listBoard)
            return;
	
        // 이 매칭으로 생성되는 특수블럭은, 연쇄 효과때 바로 터지지 않는다.(그대로 놔둠.)		
        for(int z = 0; z < listBoard.Count; ++z)
            listBoard[z].SkipTarget = bd;

        StartCoroutine( _coMakeLineBoosterPiece(pd, bd, idxColor) );
    }

    IEnumerator _coMakeLineBoosterPiece(PieceDefinition pd, Board bd, int idxColor)
    {
        if(null==pd || null== bd)
            yield break;

        GM.makingSpecialPiece = true;

        yield return new WaitForSeconds( JMFUtils.GM.delayForMakingSpecialPiece );

        bd.ResetPiece(pd, idxColor); 
		// _bd.Piece.Play("create",false,0.0f,ResetMakingSpecialPieceStatus);
        bd.Piece.Play("create", false, 0.0f, null);
        ResetMakingSpecialPieceStatus();
        //

        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		effect.ChangeColor(1, bd.Piece.ColorIndex);
		effect.Play(bd.Piece.Position, bd.Piece.Scale,false, .0f, 1.0f, bd.Piece.GO.transform);
    }

    IEnumerator _coOnFinish(float delay, GameObject objActor)
    {
        yield return new WaitForSeconds(delay);

        NNPool.Abandon(objActor);
    }

    IEnumerator _coMakeSpecialPiece(float delay, List<Board> linkX, List<Board> linkY, Board bd, List<Board> listBurstTargets)
    {
        Vector3 vStartPos       = bd.Position;
        int idxColor            = bd.ColorIndex;

        yield return new WaitForSeconds(delay);

        // note : VLine-Bomb이나 HLine-Bomb을 만들므로 그녀석을 미리 찾는다.
        PieceDefinition vhDef   = null;
        for (int q = 0; q < GM.PieceTypes.Length; ++q)
        {
            if((true==GM.SwitchDirectionV && (GM.PieceTypes[q] is VerticalPiece)) || 
                (false==GM.SwitchDirectionV && (GM.PieceTypes[q] is HorizontalPiece)) )
            {
                vhDef           = GM.PieceTypes[q];
                break;
            }
        }
        Debugger.Assert(vhDef!=null, "target should not be null !!!");

        // color index가 같은 임의의 piece를 찾고 변화.
        List<Board> listTargets     = new List<Board>();
        List<Board> listAllTargets  = new List<Board>();
        foreach (Board _bd in GM.Boards)
        {
			if(_bd.PD is NormalPiece == false) continue;
            if(true == listBurstTargets.Contains(_bd))  
                continue;
            if(linkX.Contains(_bd) || linkY.Contains(_bd))
                continue;
            if(null!=_bd.Piece && _bd.Piece.LifeCover>0)
                continue;

            listAllTargets.Add( _bd );

            if (_bd.Piece.ColorIndex != idxColor)
                continue;
                
            listTargets.Add( _bd );
        }

        // 랜덤해서 하나를 고름.
        Board bdTarget      = null;
        if(listTargets.Count == 0)          // 같은 color가 없다면, 
        {
            if(listAllTargets.Count > 0)    // 다른 color라도 고른다.
                bdTarget    = listAllTargets[ Random.Range(0, listAllTargets.Count) ];
            else
                yield break;                 // 노멀한 piece가 아예 없다면, 그만 둔다.
        }
		else 
            bdTarget      = listTargets[ Random.Range(0, listTargets.Count) ];
            
        GM.makingSpecialPiece   = true;

        //Tail effectTail = NNPool.GetItem<Tail>("TailEffect");
        //LightningTail effectTail = NNPool.GetItem<LightningTail>("LightningTail");
        //float duration = effectTail.Play("play", effectPos, bdTarget.Position, _color, false);
        //float duration = effectTail.Play("move", vStartPos, bdTarget.Position, idxColor, false);

        // v = s / t
        //float duration          = 0.25f;
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = GM.playOverlayHandler.collectAnyMission.getJewelSkinSprite( idxColor );
        
        float duration = JMFUtils.tween_move(SR.transform, vStartPos, bdTarget.Position, 12);
        StartCoroutine( _coOnFinish(duration, SR.gameObject) );
        
		NNSoundHelper.Play("IFX_line_block_wind");

		bdTarget.ChangePiece(duration, bdTarget.Position, bdTarget.Piece.Scale, idxColor, () =>
        {
            bdTarget.ResetPiece( vhDef, idxColor);
			// bdTarget.Piece.Play("create",false,0.0f,ResetMakingSpecialPieceStatus);
            bdTarget.Piece.Play("create",false,0.0f, null);
			ResetMakingSpecialPieceStatus();
            //
			PanelNetHit effect = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
			effect.ChangeColor(1, idxColor);
			effect.Play(bdTarget.Position, bdTarget.Piece.Scale, false, .0f, 1.0f, bdTarget.Piece.GO.transform);
        });
    }

    public override float ShowDestroyEffect (GamePiece gp) 
	{
		BlockCrash effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		return effect.Play("play",gp.Position,gp.Scale, gp.ColorIndex);//, false);
    }

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
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.LB);
        }
		else if (other.PD is BombPiece) 
		{
            //StartCoroutine(CoPerformBombBombPower(from, to, gp, other));
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.B);
            GM.CollectSpecialJewel(SJ_COMBINE_TYPE.BB);
        }
        return startDelay;
	}

//    void AnimationNormalToSpecial(Board bd)
//	{
//		bd.Piece.Play("create",false,()=>{ResetMakingSpecialPieceStatus();});
////		ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("ConvertToSpecial", bd.Piece.GO.transform);
////        pp.transform.localPosition = Vector3.zero;
////        pp.Play();
//    }

    void ResetMakingSpecialPieceStatus() {
        GM.makingSpecialPiece = false;
    }
}
