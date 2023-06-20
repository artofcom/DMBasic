using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE;
using NOVNINE.Diagnostics;

// [2X2_BURST]
public class TMatch7Piece : PieceDefinition {
    public float startDelay = 0.15F;
    public float sequenceDelay = 0.05F;

    const float FIRE_DELAY  = 0.05f;

	protected override void OnPieceCreate (GamePiece gp) {
        if(JMFUtils.GM.State==JMF_GAMESTATE.PLAY && false==JMFUtils.GM._isRemovedByConveyor)
            NNSoundHelper.Play("IFX_rainbow_block_create");
	}

	public override string GetImageName (int colorIndex) 
	{
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

		return fileName;
	}
	
    protected override int OnPieceDestroy (GamePiece gp, bool isByMatch, int colorIndex) {
        int score = destroyScore;

        if (gp.Owner.ShadedDurability >= 0) {
            gp.Owner.ShadedDurability--;
            score += GM.shadedScore;
        }

        return score;
    }

	public override bool Match (ref Board bd, List<Board> linkX, List<Board> linkY, out int score) {

        // CPR update. we don't use this block any more.
        score = 0;
        return false;

        /*
        if(false == ( (linkX.Count==4 && linkY.Count==2) || (linkX.Count==2 && linkY.Count==4) ))
        {
            score = 0;
            return false;
        }

        // 특수셀일때는 생성하지 않는다.
        if(bd.ColorIndex >= (int)(LEItem.COLOR.STRAWBERRY-1))
        {
            score               = 0;
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

        //score = bd.Hit(true);
        // SwapMysteryPiece(ref bd, linkX, linkY);

        Board _bd = bd;

        int colorIndex = bd.ColorIndex;
        score = createScore;

        //if (IsSpecialPiece(_bd)) {
        //    PerformCombinationPower(_bd, _bd, _bd.Piece, _bd.Piece);
        //} else {
            GM.makingSpecialPiece = true;
        //    MakeMergeEffect(_bd, linkX, linkY);
            
            // == old code ; score += _bd.Hit(true, () => {
            //bd.Piece.triggerBombMultiBursting();
			score += _bd.Hit(true); 
            DOVirtual.DelayedCall(JMFUtils.GM.delayForMakingSpecialPiece, () => {
                _bd.ResetPiece(this, colorIndex); 
                AnimationNormalToSpecial(_bd);
            });
        //}

        // [AI_MISSION] - 레인보우를 만들면 AI 턴 유지.
        GM.isRainbowCreated     = true;

        return true;
        */
	}

    public override float ShowDestroyEffect (GamePiece gp) {

        NNSoundHelper.Play("IFX_rainbow_bust"); 
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("SpecialFiveHitCrash");
		return effect.Play("play", gp.Position, gp.Scale, gp.ColorIndex);//, false);
    }
    
    public override void PerformPower (GamePiece gp)
    {
        // Note : combine 되거나, bonus hit 되므로, 자체적으로 perfom 할 일 없음.
        // StartCoroutine( _coPerformRainbowPower(gp.Owner) );
        // GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
	}
	
    IEnumerator _coPerformRainbowPower(Board bd, System.Action<float> callback=null)
    {
        // wait till all special piece finished.
        while (GM.makingSpecialPiece)
            yield return null;

        // Main Power가 Line bomb으로 변견됨. == float fDuration         = PerformRainbow7Power(bd);
        Board bdTarget          = GM.GetExistColorBoard();
        float fDuration         = PerformRainbowHV(bd.Piece, bdTarget.Piece, bdTarget.Piece.ColorIndex, bd.ColorIndex);
        if(null!=callback)      callback(fDuration);
	}

	public override bool IsCombinable ( GamePiece other)
    { 
        // 특수 type이면 false.
        //if (other.PD is StrawberryPiece || other.PD is SugarCherryPiece || other.PD is ZellatoPiece)
        //    return false;

        // normal check.
		if (other.IsMatchable())        return true;
        if (other.PD is SpecialFive)    return true;
        if (other.PD is TMatch7Piece)   return true;

        return false; 
    }

	public override float PerformCombinationPower (Board from, Board to, GamePiece gpThis, GamePiece gpOther) 
    {		
        float delay             = .0f;
		float _delay            = delay;
		
        gpThis.setIsDestroyable( true );

        if(from.PD == gpOther.PD)   {   from.Piece = gpThis;    to.Piece = gpOther; }   // moved piece is other piece !!!
        else                        {   from.Piece = gpOther;   to.Piece = gpThis; }    // moved piece is special 5.

		if (gpOther.PD is BombPiece) 
		{
            delay               = PerformRainbowBomb(gpThis, gpOther);
            gpThis.Owner.Hit(false, 1.0f);// delay+1.0f);
        }
		else if ((gpOther.PD is SpecialFive) || (gpOther.PD is TMatch7Piece))
		{
            gpOther.setIsDestroyable( true );
            StartCoroutine(CoPerformRainbowRainbowPower(from, to, gpThis , gpOther));
        }
		else if (gpOther.PD is VerticalPiece || gpOther.PD is HorizontalPiece)
        {
            // other와 같은 색을 찾아서, other색의 라인으로 바꾸고 바꾸고 터뜨린다.
            delay               = PerformRainbowHV(gpThis, gpOther, gpOther.ColorIndex, gpOther.ColorIndex);
            gpThis.Owner.Hit(false, 1.0f);// delay+1.0f);
        }
        else if(gpOther.IsMatchable()) 
		{
            if (gpOther.PD is StrawberryPiece || gpOther.PD is SugarCherryPiece || gpOther.PD is ZellatoPiece)
            {
                PerformRainbow7Power(gpThis.Owner, gpOther.ColorIndex, gpOther.Owner);
                gpThis.Owner.Hit(false, 1.0f);
            }
            else
            {
                // other와 같은색을 찾아서, 7rainbow 색의 라인으로 바꾸고 터뜨린다. 
                delay               = PerformRainbowHV(gpThis, gpOther, gpOther.ColorIndex, gpThis.ColorIndex);
                gpThis.Owner.Hit(false, 1.0f);  // delay+1.0f);
                //delay               = PerformRainbow7Power(gpThis.Owner, gpOther.ColorIndex, gpOther.Owner);
                //gpThis.Owner.Hit(false, delay-0.2f);
            }
        }
		        
		return delay;
    }

    override public float bonusHit(Board bdSelf)
    {
        Board bdTarget          = GM.GetExistColorBoard();
        float fDelay            = PerformRainbowHV(bdSelf.Piece, bdTarget.Piece, bdTarget.Piece.ColorIndex, bdSelf.Piece.ColorIndex);
        fDelay += 1.0f;
        bdSelf.Hit(false, fDelay);
        return fDelay;
    }

    IEnumerator CoPerformRainbowRainbowPower (Board from, Board to, GamePiece gp, GamePiece other) 
	{
        int score = other.PD.destroyScore;
        from.Hit();
        to.Hit();
        //from.RemovePiece(startDelay);
        //to.RemovePiece(startDelay);
        PerformRainbowRainbowPower(other.Owner);

        yield return new WaitForSeconds(startDelay);

        if (to.ShadedDurability >= 0) {
            to.ShadedDurability--;
            score += GM.shadedScore;
        }

        GM.IncreaseScore(score, to.PT, other.ColorIndex);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.RR);
    }

    IEnumerator _coResetPieceToBomb(Board bdTarget, float fDelay, Board bdFrom)
    {
        yield return new WaitForSeconds(fDelay);

        Tail effectTail         = NNPool.GetItem<Tail>("TailEffect");
	    float duration          = effectTail.Play("move", bdFrom.Position, bdTarget.Position, bdTarget.ColorIndex, false, .0f);
        NNSoundHelper.Play("IFX_line_block_wind_raindow");

        yield return new WaitForSeconds(duration);
        bdTarget.ResetPiece(GM.GetPieceType<BombPiece>(), bdTarget.ColorIndex);
        bdTarget.Piece.Play("create", false, 0.0f, null);
                
        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		effect.ChangeColor(1, bdTarget.Piece.ColorIndex);
		effect.Play(bdTarget.Piece.Position, bdTarget.Piece.Scale,false, .0f, 1.0f, bdTarget.Piece.GO.transform);
    }

    float PerformRainbowBomb (GamePiece special5, GamePiece target)
    {
        List<GamePiece>         deathNote = new List<GamePiece>();
        deathNote.Add(target);
        
        float fDelay            = .0f;
        // fire eff & destroy piece.

        foreach (Board bd in GM.Boards) {
            if (bd.IsFilled == false)               continue;
            if ((bd.PD is NormalPiece) == false)    continue; 
            if (bd.ColorIndex != target.ColorIndex) continue;
            if(bd.IsMatchable == false)             continue;

            if(bd.PD is NormalPiece)
            {
               StartCoroutine( _coResetPieceToBomb(bd, fDelay, special5.Owner) );
               fDelay += FIRE_DELAY;
            }

            if(false == deathNote.Contains(bd.Piece))
                deathNote.Add(bd.Piece);
        }

        // destroy all.
        fDelay += 0.5f;
        StartCoroutine(CoChainDestroy(deathNote, fDelay));

        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.LR);

        return fDelay;
    }

    IEnumerator _coResetPieceToHV(Board bdTarget, float fDelay, Board bdFrom, int idxColorTo)
    {
        yield return new WaitForSeconds(fDelay);

        Tail effectTail         = NNPool.GetItem<Tail>("TailEffect");
	    float duration          = effectTail.Play("move", bdFrom.Position, bdTarget.Position, idxColorTo, false, .0f);
        NNSoundHelper.Play("IFX_line_block_wind_raindow");

        yield return new WaitForSeconds(duration);

        if(0 == NNTool.Rand(0,2))
            bdTarget.ResetPiece(GM.GetPieceType<HorizontalPiece>(), idxColorTo);
        else
            bdTarget.ResetPiece(GM.GetPieceType<VerticalPiece>(), idxColorTo);
        
        bdTarget.Piece.Play("create", false, 0.0f, null);
                
        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		effect.ChangeColor(1, bdTarget.Piece.ColorIndex);
		effect.Play(bdTarget.Piece.Position, bdTarget.Piece.Scale,false, .0f, 1.0f, bdTarget.Piece.GO.transform);
    }

    float PerformRainbowHV(GamePiece special5, GamePiece target, int idxColorFrom, int idxColorTo)
    {
        List<GamePiece>         deathNote = new List<GamePiece>();
        deathNote.Add(target);

        float fDelay            = .0f;
        // fire eff & destroy piece.
        foreach (Board bd in GM.Boards)
        {
            if (bd.IsFilled == false)               continue;            
            if (bd.ColorIndex != idxColorFrom)      continue;
            if (bd.IsMatchable == false)            continue;

            if(bd.PD is NormalPiece)
            {
                StartCoroutine( _coResetPieceToHV(bd, fDelay, special5.Owner, idxColorTo) );
                fDelay += FIRE_DELAY;
            }

            if(false == deathNote.Contains(bd.Piece))
                deathNote.Add(bd.Piece);
        }

        // destroy all.
        fDelay += 0.5f;
        StartCoroutine(CoChainDestroy(deathNote, fDelay));

        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.LR);

        return fDelay;
    }

    void PerformRainbowRainbowPower (Board bd) 
	{
        for (int i = 1; i < GameManager.WIDTH; i++) 
		{
            List<Board> boards = bd.GetBoardsFromDistance(i);

            if (boards.Count > 0) 
			{
                foreach (Board _bd in boards) 
				{
                    // 이 셀중에 rainbow cell은 destory제외 한다. - 다른 방식으로 로직 변경.
                    //if((_bd.PD is SpecialFive) || (_bd.PD is TMatch7Piece))
                    //    continue;

                    if(null != _bd.Panel)   _bd.Panel.setDamagingColor(-1);
                    _bd.Hit(sequenceDelay * (i - 1));
                }
            }
			else
			    break;
        }        
	}

    float NearRainbowPower (Board bd, int colorIndex) 
	{
		float duration = 0.0f;
		List<Board> sameList = new List<Board>();

		foreach (Board _bd in GM.Boards) 
		{
			if(bd == _bd || _bd.IsFilled == false || _bd.Piece.IsMatchable() == false || _bd.Piece.ColorIndex != colorIndex) continue;

			sameList.Add(_bd);
		}
		
		List<Board>	list = new List<Board>();
		
		if(bd != null)
		{
			int i = 3;
			while(list.Count == 0)
			{
				int h = (int)((i - 1) * 0.5f);
				
				if(bd.X - h < 0 && bd.X + h > 8 && bd.Y - h < 0 && bd.Y + h > 8)
					break;
				   
				int n = 0;
				int x = 0;
				int t = 0;
				
				while(n < i * i)
				{
					int XX = 0;
					int YY = 0;
					
					if(x == 0)
					{
						XX = bd.X - h;
						YY = bd.Y - h;
					}
					else if(x == 1)
					{
						XX = bd.X - h + 1;
						YY = bd.Y + h;
					}
					else if(x == 2)
					{
						XX = bd.X + h;
						YY = bd.Y + h;
					}
					else if(x == 3)
					{
						XX = bd.X - h;
						YY = bd.Y + h;
					}
					
					for(int y = 0; y < i - t ; ++y)
					{
						Board _bd = GM[XX,YY];
						
						if(_bd != null && _bd.IsFilled == true && _bd.Piece.IsMatchable() == true && _bd.Piece.ColorIndex == colorIndex)
							list.Add(_bd);
						
						if(x == 0)
							++YY;
						else if(x == 1)
							++XX;
						else if(x == 2)
							--YY;
						else if(x == 3)
							--XX;
					}
					
					++x;
					t = 1;
					
					if(x == 3)
						t = 2;
					
					n += i - t;
				}
				
				if(list.Count == 0)
					i += 2;
			}
			
			Tail effectTail = null;
			BlockCrash effect = null;
			
			float delay = 0.0f;
			
			for(i = 0; i < list.Count; ++i)
			{
				effectTail = NNPool.GetItem<Tail>("TailEffect");
				duration = effectTail.Play("move", bd.Position, list[i].Position, list[i].ColorIndex, false, delay);

				effect = NNPool.GetItem<BlockCrash>("SpecialFiveHitCrash");
				effect.Play("play",list[i].Position,list[i].Piece.Scale, list[i].ColorIndex);//, false, duration + delay);
				list[i].Hit(duration + delay);	
				delay += 0.2f;
			}
			
			duration += delay;
		}
		
		return duration;
	}

    IEnumerator _coResetPiece(Board bd, PieceDefinition pd, int idxColor, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);

        bd.ResetPiece(pd, idxColor); 
        AnimationNormalToSpecial(bd);
    }

    float PerformRainbow7Power (Board bdSpecial7, int colorIndex = -1, Board bdFrom = null) 
	{
       int targetColorIndex = colorIndex > -1 ? colorIndex : GM.GetExistColorIndex();

		float delay = 0.0f;
		float duration = 0.0f;
		float d = 0.0f;
		
		Tail effectTail = null;
		BlockCrash effect = null;
		int idxTailColor = 0;

        // swap으로 인해 from bd가 있다면 그녀석 부터 destroy한다.
		if(bdFrom != null)
		{
            idxTailColor        = colorIndex;
            if (bdFrom.PD is StrawberryPiece || bdFrom.PD is SugarCherryPiece || bdFrom.PD is ZellatoPiece)
                idxTailColor    = 0;

			effectTail          = NNPool.GetItem<Tail>("TailEffect");
            duration            = effectTail.Play("move", bdSpecial7.Position, bdFrom.Position, idxTailColor, false, delay);
	
			effect              = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
			d                   = effect.Play("play", bdFrom.Position, bdFrom.Piece.Scale, idxTailColor);//, false, duration + delay);
            //bdSpecial5.Hit(duration + delay); ???            
            //bdFrom.Hit(duration + delay);
            //GM.SplashHit(bdFrom, duration + delay);
            delay += 0.15f; // 0.2f;
			
			//NearRainbowPower(bd,targetColorIndex,ref duration);
		}
		
        bool normalBlk          = true;
        idxTailColor            = targetColorIndex;
        if (bdFrom.PD is StrawberryPiece || bdFrom.PD is SugarCherryPiece || bdFrom.PD is ZellatoPiece)
        {
            idxTailColor        = 0;
            normalBlk           = false;
        }

		foreach (Board _bd in GM.Boards) 
		{
			if (_bd.IsFilled == false) continue;
			if (_bd.Piece.IsMatchable() == false) continue;
			if (_bd.Piece.ColorIndex != targetColorIndex) continue;
						
			effectTail = NNPool.GetItem<Tail>("TailEffect");
			duration = effectTail.Play("move", bdSpecial7.Position, _bd.Position, idxTailColor, false, delay);
			StartCoroutine( _coPlaySound("IFX_line_block_wind_raindow", delay) );

            if(true == normalBlk)
            {
			    effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
			    d = effect.Play("play",_bd.Position,_bd.Piece.Scale, _bd.ColorIndex);//, false, duration + delay);
            }
            _bd.Hit(duration + delay);
            // GM.SplashHit(_bd, duration + delay);

			//delay += 0.15f; // 0.2f;
            delay += FIRE_DELAY;     // 빠르게 다 쏨.
		}
		
        GM.makingSpecialPiece   = true;
        DOVirtual.DelayedCall( duration + delay, () => { GM.makingSpecialPiece = false; } );

		return duration + delay + d;
	}

    IEnumerator _coPlaySound(string strSndName, float delay)
    {
        yield return new WaitForSeconds(delay);

        NNSoundHelper.Play(strSndName);
    }

    IEnumerator CoChainDestroy (List<GamePiece> deathNote, float fDelay)
    {
        Debugger.Assert(deathNote != null, "SpecialFive.CoChainDestroy : GamePiece list is null.");

        //yield return new WaitForSeconds(fDelay);

        // 대상들이 모두 특수셀로 변화될때까지 대기.
        float fElTime           = 3.0f;
        do
        {
            bool stopWait       = true;
            for (int i = 0; i < deathNote.Count; i++)
            {
                if( null!=deathNote[i].PD && (false==deathNote[i].PD is VerticalPiece) && (false==deathNote[i].PD is HorizontalPiece) && 
                    (false==deathNote[i].PD is BombPiece) && (false==deathNote[i].PD is SpecialFive) && (false==deathNote[i].PD is TMatch7Piece))
                {
                    stopWait    = false; 
                    break;
                }
            }
            if(stopWait)        break;
            yield return null;

            fElTime -= Time.deltaTime;
            if(fElTime < .0f)   break;  // doublle check by time.

        }while(true);

        // 바로 hit 하는 것으로 수정.
        // = while (GM.IsStable == false) yield return null;

        // CPR update.
        int[] soundCount        = new int[] { 1, 1 };
        // int[] soundCount        = new int[] { 3, 3 };
        GM.ActMassMatching      = true;

        for (int i = 0; i < deathNote.Count; i++) {
            GamePiece gp = deathNote[i];
            if ((gp == null) || (gp.PD == null) || (gp.GO == null)) {
                continue;
            }

            if (gp.Owner.Panel.IsDestroyablePiece()) {

                // 적절한 수 만큼의 sound만 play 되도록 manage 한다.
                int idxTarget   = -1;
                if(soundCount[0]>=0 && (gp.PD is VerticalPiece || gp.PD is HorizontalPiece))
                    idxTarget   = 0;
                else if(soundCount[1]>=0 && (gp.PD is BombPiece))
                    idxTarget   = 1;
                
                if(idxTarget>=0 && idxTarget<soundCount.Length)
                {
                    gp.mPlayWhenMassMatching    = true;
                    --soundCount[idxTarget];
                }
                //

                gp.Owner.Hit(false, true, .0f, false, true, false, null);
            } else {
                gp.Owner.Panel.Destroy(false, false, true);
            }   
        }

        yield return null;
        GM.ActMassMatching      = false;
    }

    void AnimationNormalToSpecial(Board bd) {
        Vector3 startScale = bd.Piece.GO.transform.localScale;
		Sequence seq = DOTween.Sequence();
		seq.OnComplete(ResetMakingSpecialPieceStatus);

		seq.Append(bd.Piece.GO.transform.DOScale( startScale * 1.5F,0.2F).SetEase(Ease.Linear));
		seq.Append(bd.Piece.GO.transform.DOScale(startScale, 0.2F).SetEase(Ease.Linear));
        seq.Play();

        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		effect.ChangeColor(1, bd.Piece.ColorIndex);
		effect.Play(bd.Piece.Position, bd.Piece.Scale,false, .0f, 1.0f, bd.Piece.GO.transform);
    }

    void ResetMakingSpecialPieceStatus() {
        GM.makingSpecialPiece = false;
    }

    #region // old logic code.
    IEnumerator CoPerformChangeColor (Board from, Board to, GamePiece gp, GamePiece other)
    {
        int score = gp.PD.destroyScore;

        int idxSrcColor         = (from.PD is NormalPiece) ? from.ColorIndex : to.ColorIndex;
        int idxToColor          = (from.PD is NormalPiece) ? to.ColorIndex : from.ColorIndex;

        // == PerformChageColor(to, to.ColorIndex, from.ColorIndex); // other.ColorIndex);
        PerformChageColor(to, idxSrcColor, idxToColor);
        yield return new WaitForSeconds(startDelay);
        from.RemovePiece();
        to.RemovePiece();

        if (to.ShadedDurability >= 0) {
            to.ShadedDurability--;
            score += GM.shadedScore;
        }

        GM.IncreaseScore(score, to.PT, other.ColorIndex);
        GM.CollectSpecialJewel(SJ_COMBINE_TYPE.R);
    }

    void PerformChageColor (Board bd, int srcIdxColor, int colorIndex = -1) {

        PieceDefinition normalDef = null;
        for (int q = 0; q < GM.PieceTypes.Length; ++q)
        {
            if(GM.PieceTypes[q] is NormalPiece)
            {
                normalDef       = GM.PieceTypes[q];
                break;
            }
        }
        Debugger.Assert(normalDef!=null, "target pieceDef should not null !!!");


        Vector3 fromPos = bd.Position;
        //int targetColorIndex = colorIndex > -1 ? colorIndex : GM.GetExistColorIndex();

		foreach (Board _bd in GM.Boards) {

            if (_bd.IsFilled == false)              continue;
			if (_bd.Piece.IsMatchable() == false)   continue;
            if (false==(_bd.PD is NormalPiece))     continue;
            if (_bd.Piece.ColorIndex != srcIdxColor)continue;
            
            var go = NNPool.GetItem("Lightning");
            go.GetComponent<Lightning>().EmitLightning(fromPos, _bd.Position, startDelay);
            //_bd.Hit(startDelay);

            _bd.ResetPiece(normalDef, colorIndex); 
            AnimationNormalToSpecial(_bd);
		}
    }

    #endregion
}
