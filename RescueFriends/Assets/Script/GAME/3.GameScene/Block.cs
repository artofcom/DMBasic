using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Block : NNRecycler 
{
    //protected string[] blockColorName = {"block_red", "block_yellow", "block_green", "block_blue", "block_purple", "block_orange", "block_skyblue", "block_violet", "block_rainbow" };
    //protected string[] blockType = { "", "_H","_V", "_B", "_BB","_X","_S"};

    const float sequenceDelay   = 0.05F;
    const float startDelay      = 0.15F;     

    public GamePiece Owner { get; set; }
    public PieceTracker PT { get; private set; }
    public BoxCollider BC { get; private set; }
	public Sprite[] _sprSkins   = null;
	protected SpriteRenderer    _sprView    = null;
	
	public int ShuffIndex { get; set; }
	//public SkeletonAnimation getSA() { return SA; }

	//Spine.AnimationState.TrackEntryDelegate completeDelegate;
	//Spine.AnimationState.TrackEntryEventDelegate eventDelegate;
	
	protected System.Action _onComplete;
	
    [HideInInspector] public Vector3 _vOrgBCSize = Vector3.zero;

	protected virtual void Awake ()
	{
        PT = GetComponent<PieceTracker>();
        if (PT == null) PT = gameObject.AddComponent<PieceTracker>();

        BC = GetComponent<BoxCollider>();
        if (BC == null) BC = gameObject.AddComponent<BoxCollider>();

        // adjust Box colliders scale.
        if (_vOrgBCSize.Equals( Vector3.zero ))
            _vOrgBCSize         = BC.size;
        //

        _sprView                = GetComponent<SpriteRenderer>();
		//SA = GetComponent<SkeletonAnimation>();
		//if(SA != null)
		//{
		//	completeDelegate = new Spine.AnimationState.TrackEntryDelegate(OnComplete);
		//	eventDelegate = new Spine.AnimationState.TrackEntryEventDelegate(OnEvent);
		////	SA.AnimationState.Complete += completeDelegate;
		//	SA.AnimationState.Event += eventDelegate;
		//	SA.GetComponent<Renderer>().sortingOrder = 0;
		//}
    }

	public override void Reset () 
	{
        base.Reset();
        if (PT != null) PT.enabled = true;
        if (BC != null) BC.enabled = true;

		//if(SA != null)
		//	SA.GetComponent<Renderer>().sortingOrder = 0;
    }
	
	//protected virtual void OnComplete (TrackEntry entry)
	////{
	//	if(_onComplete != null)
	//	{
	//		_onComplete();
	//		_onComplete = null;
	//	}
	//}
	/*
	protected virtual void OnEvent (TrackEntry entry, Spine.Event e)
	{
		if(e.Data.Name == "change")
		{
			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");
			Slot _slot = SA.Skeleton.FindSlot("block_normal");
			Slot _slot1 = SA.Skeleton.FindSlot(e.String);

			_slot1.Attachment = SA.Skeleton.GetAttachment(slotIndex, _slot.Attachment.Name);	
			return;
		}
		
		if(e.Data.Name == "shuffling" && ShuffIndex > -1)
		{
            string blockName = blockColorName[ShuffIndex];

            if(Owner.PD is SugaredPiece)
                blockName += "_S";
			
			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");

			Slot _slot = SA.Skeleton.FindSlot(e.String);
			_slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, blockName);
			
			return;
		}
	}*/
	
    public virtual void resetBlockColor(int idxColor)
    {
        if(idxColor<0 || idxColor>=_sprSkins.Length)
            return;

        _sprView.sprite         = _sprSkins[ idxColor ];
    }

	public virtual void ChangeBlockColor(int index, int type) 
	{
		/*if(SA != null && blockColorName.Length > index && blockType.Length > type && index + type > -1)
		{
			string blockName = blockColorName[index] + blockType[type];
			
			int slotIndex = SA.Skeleton.FindSlotIndex("RegionList");
			
			Slot _slot = SA.Skeleton.FindSlot("block_normal");
            _slot.Attachment = SA.Skeleton.GetAttachment(slotIndex, blockName);
            // Test code 
            Bone _bone = SA.Skeleton.FindBone("root");
            if (type == 5)
            {
                _bone.ScaleX = 1.1f;
                _bone.ScaleY = 1.1f;
            }
            else
            {
                _bone.ScaleX = 1f;
                _bone.ScaleY = 1f;
            }
		}*/
	}
	
	public float Play(string animationName,bool loop, float delay = 0.0f, System.Action onComplete = null) 
	{
        if(animationName == "create")
        {
            transform.DOScale(transform.localScale.x * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }

		/*if(SA != null)
		{
			_onComplete = onComplete;
			Spine.Animation ani = SA.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				float d = ani.Duration;
				TrackEntry _trackEntry = SA.AnimationState.SetAnimation(0, ani, loop);
				_trackEntry.Delay = delay;
                _trackEntry.timeScale   = 1.0f; // confirm time scale.

				if(loop)
					return -1;
				else
					return d;
			}
		}
		*/
		return -2;
	}
	
	public float AddPlay(string animationName,bool loop, float delay = 0.0f) 
	{
		/*if(SA != null)
		{
			Spine.Animation ani = SA.skeleton.Data.FindAnimation(animationName);
			if(ani != null)
			{
				SA.AnimationState.AddAnimation(0, animationName, loop,delay);
				return ani.Duration + delay;
			}
		}*/

		return -2;
	}
	
	public override void Release () 
	{
        base.Release();
        if (PT != null) PT.enabled = false;
        if (BC != null) BC.enabled = false;
        //DOTween.Kill(gameObject.GetInstanceID()+1);
        transform.DOKill();
    }

    public virtual Bounds GetBounds () 
	{
        return JMFUtils.findObjectBounds(gameObject);
    }

    public virtual void resetColliderSize()
    {
        if(null != BC)          BC.size = _vOrgBCSize;
    }

    public void PerformPower()
    {
        if(Owner.PD is VerticalPiece)
        {
            StartCoroutine( _coPerformVerticalPower(Owner.Owner, null) );
        }
        else if(Owner.PD is HorizontalPiece)
        {
            StartCoroutine( _coPerformHorizontalPower(Owner.Owner, null) );
        }
        else if(Owner.PD is BombPiece)
        {
            StartCoroutine( _coPerformBombPower(Owner.Owner, null, 1) );
        }
    }

    #region VERTICAL_PIECE
    IEnumerator _coPerformVerticalPower(Board bdMain, Board bdFrom, System.Action<float> callback=null)
    {
        // wait till all special piece finished.
        //while (JMFUtils.GM.makingSpecialPiece)
        //    yield return null;

        float fDuration         = PerformVerticalPower(bdMain, bdFrom);
        if(null!=callback)      callback(fDuration);

        yield return null;
    }

    float PerformVerticalPower (Board bdMain, Board bdFrom) 
	{
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add(bdFrom);

		//float duration = GM.ShootShockWave(bdMain.PT, 5F, 1F, 0.5F,true,0.2f, listExceptions);
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("vertical_hit",bdMain.Position,bdMain.Piece.Scale, bdMain.Piece.ColorIndex, false,duration);

        //float fDistOffset       = 20.0f;
        float duration          = .0f;
        //BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play",bdMain.Position, Vector3.one, bdMain.ColorIndex, 3.0f);
        //effect.transform.DOLocalMoveY(effect.transform.localPosition.y+fDistOffset, 2.0f);
        //effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play",bdMain.Position, Vector3.one, bdMain.ColorIndex, 3.0f);
        //effect.transform.DOLocalMoveY(effect.transform.localPosition.y-fDistOffset, 2.0f);
        ParticlePlayer player   = NNPool.GetItem<ParticlePlayer>("eff_line_V");
        player.Play(new Vector3(bdMain.Position.x, bdMain.Position.y, -100.0f));

        List<Board> boardsT = bdMain.GetBoardsInDirection(JMF_DIRECTION.UP);
        List<Board> boardsB = bdMain.GetBoardsInDirection(JMF_DIRECTION.DOWN);
        DestroyBySequencial(bdMain, boardsT, duration, bdMain.SkipTarget);
		DestroyBySequencial(bdMain, boardsB, duration, bdMain.SkipTarget);

        JMFUtils.GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);

        if(false==JMFUtils.GM.ActMassMatching || true==bdMain.Piece.mPlayWhenMassMatching)
        {
            //if(null!=bdFrom)        // combination power일때는 sound 2 회 출력.
            //    DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
            DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
        }

        bdMain.Piece.mPlayWhenMassMatching = false;
        bdMain.SkipTarget       = null;

		return duration;
	}
    void DestroyBySequencial (Board bdMain, List<Board> boards, float delay = 0.0f, Board skip=null)
	{
        for (int i = 0; i < boards.Count; i++) 
		{
            // note : cut 하지 않는 것으로 변경.
            // bool isSpiralSnow = (boards[i].PD is RoundChocoPiece || boards[i].PD is GreenBubblePiece);
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

            // rainbow 등에 의한 massmatching 시는 delay destroy를 하지 않는다.
            if(JMFUtils.GM.State==JMF_GAMESTATE.PLAY && false==JMFUtils.GM.ActMassMatching)
            {
                // cage 등의 패널이 있으면 하지 않는다.
                if (boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is VerticalPiece) || (boards[i].PD is HorizontalPiece) || (boards[i].PD is BombPiece)))
                {
                    // 매칭에 속한 녀석은 제외한다.
                    if(null!=bdMain && false==bdMain.ListMatchings.Contains(boards[i]) && boards[i].Piece.LifeCover<=0)
                    {
                        // Debug.Log(string.Format(">>> V-Delayed Piece Applyed !!! {0}, {1}", boards[i].X, boards[i].Y));

                        boards[i].Piece.ShineAndBlink(delay + (sequenceDelay*i));
                        JMFUtils.GM.DestroyAwaiter.Add(boards[i].Piece);
                        // boards[i].Piece.triggerBombMultiBursting();
                        continue;
                    }

                    if(null!=bdMain && true==bdMain.ListMatchings.Contains(boards[i]))
                        Debug.Log("V DestroyBySequencial ) it's matching now...");

                    if (boards[i].Piece.LifeCover > 0) 
                        Debug.Log("V DestroyBySequencial ) it has Life cover !.");
                }
            }

            // if (boards[i].PD is StonePiece) break;
            //if(null!=boards[i].Piece) boards[i].Piece.triggerBombMultiBursting();
            if(null!=bdMain && null!=bdMain.Piece && null!=boards[i].Panel)
                boards[i].Panel.setDamagingColor( bdMain.Piece.ColorIndex + 1 );
            boards[i].Hit(delay + (sequenceDelay * i), true);
        }
    }

    public void PerformCrossCombinationPower (Board from, Board to, GamePiece gp, GamePiece other)
    {
        StartCoroutine(CoPerformCrossCombinationPower(from, to, gp, other));
    }
    IEnumerator CoPerformCrossCombinationPower (Board from, Board to, GamePiece gp, GamePiece other) 
	{
        int score = gp.PD.destroyScore + other.PD.destroyScore;

        // 터짐 효과 추가.
        BombHit effectB         = NNPool.GetItem<BombHit>("BombPieceCrash");// BombBubbleHit");
		Vector3 pos             = to.Position;
		pos.z -= 2.0f; 
		effectB.Play(pos, to.Piece.Scale*0.35f, to.Piece.ColorIndex, false);
        // 

        float duration          = .0f;
        yield return StartCoroutine( _coPerformVerticalPower(to, from, (fRet) =>
        {
            duration            = fRet;
        }) );

        // wait till all special piece finished.
        //while (GM.makingSpecialPiece)    yield return null;
        //float duration = PerformVerticalPower(to);

		BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		effect.Play("horizontal_hit",to.Position,to.Piece.Scale, other.ColorIndex, false,duration);

        from.Hit(false, .0f);
        to.Hit(false, 0.145f);

        List<Board> boardsL = to.GetBoardsInDirection(JMF_DIRECTION.LEFT);
        List<Board> boardsR = to.GetBoardsInDirection(JMF_DIRECTION.RIGHT);
		DestroyBySequencial(from, boardsL,duration);
		DestroyBySequencial(from, boardsR,duration);

		yield return new WaitForSeconds(duration);

        if (to.ShadedDurability >= 0) {
            to.ShadedDurability--;
            score += JMFUtils.GM.shadedScore;
        }

        JMFUtils.GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }
    #endregion

    #region HORIZE_PIECE
    IEnumerator _coPerformHorizontalPower(Board bdMain, Board bdFrom, System.Action<float> callback=null)
    {
        // wait till all special piece finished.
        //while (JMFUtils.GM.makingSpecialPiece)
        //    yield return null;

		float fDuration =        PerformHorizontalPower(bdMain, bdFrom);
        if(null!=callback)      callback(fDuration);

         yield return null;
	}
    float PerformHorizontalPower (Board bdMain, Board bdFrom) 
	{
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add(bdFrom);

		//float duration = GM.ShootShockWave(bdMain.PT, 5F, 1F, 0.5F,true,0.2f, listExceptions);
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("horizontal_hit",bdMain.Position,bdMain.Piece.Scale, bdMain.Piece.ColorIndex, false,duration);

        //float fDistOffset       = 20.0f;
        float duration          = .0f;
        //BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bdMain.Position, Vector3.one, bdMain.ColorIndex, 3.0f);
        //effect.transform.DOLocalMoveX(effect.transform.localPosition.x+fDistOffset, 2.0f);
        //effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bdMain.Position, Vector3.one, bdMain.ColorIndex, 3.0f);
        //effect.transform.DOLocalMoveX(effect.transform.localPosition.x-fDistOffset, 2.0f);
        ParticlePlayer player   = NNPool.GetItem<ParticlePlayer>("eff_line_H");
        player.Play(new Vector3(bdMain.Position.x, bdMain.Position.y, -100.0f));

        List<Board> boardsL = bdMain.GetBoardsInDirection(JMF_DIRECTION.LEFT);
        List<Board> boardsR = bdMain.GetBoardsInDirection(JMF_DIRECTION.RIGHT);
		DestroyBySequencial(bdMain, boardsL, duration, bdMain.SkipTarget);
		DestroyBySequencial(bdMain, boardsR, duration, bdMain.SkipTarget);

        JMFUtils.GM.CollectSpecialJewel(SJ_COMBINE_TYPE.L);

        bdMain.SkipTarget           = null;

        if(false==JMFUtils.GM.ActMassMatching || true==bdMain.Piece.mPlayWhenMassMatching)
        {
            //if(null!=bdFrom)        // combination power일때는 sound 2 회 출력.
            //    DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
            DOVirtual.DelayedCall(duration, () => NNSoundHelper.Play("IFX_lineblock_bust"));
        }

		return duration;
	}
    #endregion

    #region BOMB_PIECE
    IEnumerator _coPerformBombPower(Board bdMain, Board from, int radius)
    {
        // wait till all special piece finished.
        //while (JMFUtils.GM.makingSpecialPiece)
        //    yield return null;

		PerformBombPower(bdMain, from, radius, 0.2f);

        yield return null;
	}
    void PerformBombPower (Board bdMain, Board bdFrom, int radius, float rate) 
	{
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add( bdFrom );
		float duration = JMFUtils.GM.ShootShockWave(bdMain.PT, 5F, 1F, 0.5F,true,rate, listExceptions);

		BombHit effect = NNPool.GetItem<BombHit>("BombPieceCrash");
		
        float effDelay          = duration - 0.05f;
		effect.Play(bdMain.Position, bdMain.Piece.Scale * ((float)radius), bdMain.Piece.ColorIndex, false, effDelay);
		
        List<Board> boards = bdMain.GetBoardsFromArea(0, radius);
		DestroyBySequencial(bdMain, boards, duration, 0F, false, 0, bdMain.SkipTarget);

        bdMain.SkipTarget       = null;
        if (false==JMFUtils.GM.ActMassMatching || true==bdMain.Piece.mPlayWhenMassMatching)
        {
            // bomb sound 2회 간격 두면서 play.
            DOVirtual.DelayedCall(effDelay-0.1f, () => NNSoundHelper.Play("IFX_bombblock_bust") );
            DOVirtual.DelayedCall(effDelay, () => NNSoundHelper.Play("IFX_bombblock_bust") );
        }
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
            if(JMFUtils.GM.State==JMF_GAMESTATE.PLAY && false==JMFUtils.GM.ActMassMatching)
            {
                if (boards[i].IsFilled && boards[i].Panel.IsDestroyablePiece() && ((boards[i].PD is VerticalPiece) || (boards[i].PD is HorizontalPiece) || (boards[i].PD is BombPiece)))
                {
                    if(null!=bdMain && false==bdMain.ListMatchings.Contains(boards[i]) && boards[i].Piece.LifeCover<=0)
                    {
                        // Debug.Log(string.Format(">>> B-Delayed Piece Applyed !!! {0}, {1}", boards[i].X, boards[i].Y));

                        boards[i].Piece.ShineAndBlink(_startDelay + (sequenceDelay*i));
                        JMFUtils.GM.DestroyAwaiter.Add(boards[i].Piece);
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
    }

    public void PerformLineBombPower (Board from, Board to, GamePiece gp, GamePiece other)
    {
        StartCoroutine(CoPerformLineBombPower(from, to, gp, other));
    }
    public void PerformBombBombPower (Board from, Board to, GamePiece gp, GamePiece other)
    {
        StartCoroutine(CoPerformBombBombPower(from, to, gp, other));
    }
    IEnumerator CoPerformLineBombPower (Board from, Board to, GamePiece gp, GamePiece other) {
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
            score += JMFUtils.GM.shadedScore;
        }

        JMFUtils.GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }
     IEnumerator CoPerformBombBombPower (Board from, Board to, GamePiece gp, GamePiece other) {
        int score = gp.PD.destroyScore + other.PD.destroyScore;
		//PerformBombPower(to, 2, 0.4f);
        yield return StartCoroutine( _coPerformBombPower(to, from, 2) );

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
            score += JMFUtils.GM.shadedScore;
        }

        JMFUtils.GM.IncreaseScore(score, to.PT, other.ColorIndex);
    }
    void PerformLineBombPower (Board bdMain, Board bdFrom) 
	{    
        List<Board> listExceptions  = new List<Board>();
        listExceptions.Add(bdMain);
        if(null!=bdFrom)        listExceptions.Add( bdFrom );
		float duration = JMFUtils.GM.ShootShockWave(bdMain.PT, 7F, 1F, 0.5F,true,0.3f, listExceptions);

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
    void PerformHorizontalPower (Board bd, Vector3 scale, int color,  float delay = 0.0f) 
	{
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("horizontal_hit",bd.Position,scale, color, false,delay);
		
        //float duration          = .0f;
        //BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bd.Position, Vector3.one, color, 3.0f);
        //effect.transform.DOLocalMoveX(effect.transform.localPosition.x+50, 2.0f);
        //effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bd.Position, Vector3.one, color, 3.0f);
        //effect.transform.DOLocalMoveX(effect.transform.localPosition.x-500, 2.0f);
        ParticlePlayer player   = NNPool.GetItem<ParticlePlayer>("eff_line_H");
        player.Play(new Vector3(bd.Position.x, bd.Position.y, -100.0f));

        List<Board> boardsL = bd.GetBoardsInDirection(JMF_DIRECTION.LEFT);
        List<Board> boardsR = bd.GetBoardsInDirection(JMF_DIRECTION.RIGHT);
		DestroyBySequencial(bd, boardsL, delay, sequenceDelay, true, 1);
		DestroyBySequencial(bd, boardsR, delay, sequenceDelay, true, 1);
    }
    void PerformVerticalPower (Board bd, Vector3 scale, int color, float delay = 0.0f) 
	{
		//BlockLine effect = NNPool.GetItem<BlockLine>("BlockLine");
		//effect.Play("vertical_hit",bd.Position,scale, color, false,delay);

        //BlockCrash effect       = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bd.Position, Vector3.one, color, 3.0f);
        //ef/fect.transform.DOLocalMoveY(effect.transform.localPosition.y+50, 2.0f);
        //effect                  = NNPool.GetItem<BlockCrash>("FlyBallStar");
		//effect.Play("play", bd.Position, Vector3.one, color, 3.0f);
        //effect.transform.DOLocalMoveY(effect.transform.localPosition.y-500, 2.0f);
        ParticlePlayer player   = NNPool.GetItem<ParticlePlayer>("eff_line_V");
        player.Play(new Vector3(bd.Position.x, bd.Position.y, -100.0f));

        List<Board> boardsT = bd.GetBoardsInDirection(JMF_DIRECTION.UP);
        List<Board> boardsB = bd.GetBoardsInDirection(JMF_DIRECTION.DOWN);
		DestroyBySequencial(bd, boardsT, delay, sequenceDelay, true, 1);
		DestroyBySequencial(bd, boardsB, delay, sequenceDelay, true, 1);
	}
    #endregion


    //public IEnumerator coMakeBoosterPiece()
    //{}
    //Block blk               = gp.GO.GetComponent<Block>();
    //    if(null != blk)         blk.PerformPower();
}
