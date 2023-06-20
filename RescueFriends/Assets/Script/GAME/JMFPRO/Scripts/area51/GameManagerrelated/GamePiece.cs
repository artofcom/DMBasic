using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE.Diagnostics;

public class GamePiece {
    public enum STATE { STABLE, DROP }

	public int ColorIndex { get; set; }

    GameManager GM { get { return JMFUtils.GM; } }

    Vector3 orgScale;

    // [ROUND_CHOCO] : 이 개념이 piece에도 필요.
    int durability = -1;
    public int Durability {
        get { return durability; }
        set {
            value = Mathf.Max(-1, value);
            if (durability == value) return;
            durability = value;
        }
    }

    STATE state;
    public STATE State { 
        get { return state; }
        private set {
            if (state == value) return;
            /*
            Debug.Log(string.Format("[Piece State : {0} {1}->{2} ]", 
                Owner == null ? "NONE" : Owner.PT.ToString(), state, value));
            */
            state = value;
        } 
    }

    GameObject piece;
	public GameObject GO { 
        get { return piece; }
        private set { 
            piece = value;

            if ((piece != null) && (Owner != null)) {
                piece.GetComponent<Block>().Owner = this;
                Position = Owner.Position; 
            }
        }
    }

    bool _needMakeBuffWhenDestroy = false;
    public bool NeedMakeBuffWhenDestroy
    {
        get     { return _needMakeBuffWhenDestroy; }
        set
        {
            _needMakeBuffWhenDestroy = value;
        }
    }

    bool _isDestroying          = false;
    public bool IsDestroying
    {
        get     { return _isDestroying; }
        private set
        {
            _isDestroying           = value;
        }
    }


    GameObject _objCover;
    int _nLifeCover             = 0;
    public int LifeCover
    {
        get { return _nLifeCover; }
        set
        {
            _nLifeCover         = value;
            if(null == GO)      return;

            if(1==value || 2==value)
            {
                SpriteImage sp  = null;
                if(null == _objCover)
                {
                    sp          = NNPool.GetItem<SpriteImage>("SpriteImage", GO.transform);
                    _objCover   = sp.gameObject;
                    _objCover.transform.localPosition   = Vector3.back;
                    _objCover.transform.localScale      = Vector3.one;
                }
                else sp         = _objCover.GetComponent<SpriteImage>();
                
                _objCover.GetComponent<SpriteRenderer>().sprite = sp.getSpriteByName( "OvericeL" + _nLifeCover);
            }
            else
            {
                _nLifeCover     = 0;
                if(null != _objCover)
                    NNPool.Abandon( _objCover );
                
                _objCover       = null;
            }
        }
    }

    // Cookie Jelly.
    int _cookieJellyType        = 0;    // (0 - type1, 1 - type2)
    public int CookieJellyType
    {
        get { return _cookieJellyType; }
        set
        {
            if(PD is CookieJellyPiece)
                _cookieJellyType= value;
        }
    }
    SpriteImage _sprCookieJellyCoverL, _sprCookieJellyCoverU;   // left, up cover.

	Board owner;
	public Board Owner { 
        get { return owner; }
        set {
            if (owner == value) return;
            owner = value;

            if (GO != null) {
                GO.GetComponent<PieceTracker>().PT = owner.PT;
            }
        }
    }

    // [PASS_EMPTY_BOARD]
    public List<Board> _listEmptyBoards = new List<Board>();
    //

    PieceDefinition pd;
	public PieceDefinition PD { 
        get { return pd; }
        private set {
            if (pd == value) return;
            pd = value;

            if (pd != null) {
                Debugger.Assert(ColorIndex < pd.skin.Length, "PieceDefinition.PD : Color index is wrong.");
                ChangePiece();
            }
        }
    }

    int fallBackTime;
    public int FallBackTime {
        get { 
            if ((GO != null) && (PD is TimeBombPiece)) {
                TimeBomb tb = GO.GetComponent<TimeBomb>();
                return tb.FallBackTime;
            } else {
                return 0;
            }
        }
        set {
            if (fallBackTime == value) return;

            fallBackTime = value;

            if ((GO != null) && (PD is TimeBombPiece)) {
                TimeBomb tb = GO.GetComponent<TimeBomb>();
                tb.FallBackTime = fallBackTime;
            }
        }
    }

	public Vector3 Scale
	{ 
		get {
			if (GO == null) {
				return Vector3.one;
			} else {
				return GO.transform.localScale;
			}
		}
		set {
			if (GO == null) return;
			GO.transform.localScale = value;
		}
	}
	
	public Vector3 Position { 
        get {
            if (GO == null) {
                return Vector3.zero;
            } else {
                return GO.transform.position;
            }
        }
        set {
            if (GO == null) return;
            GO.transform.position = value;
        }
    }

	public Vector3 LocalPosition { 
        get {
            if (GO == null) {
                return Vector3.zero;
            } else {
                return GO.transform.localPosition;
            }
        }
        set {
            Debugger.Assert(GO != null, "GamePiece.LocalPosition : GO is null");
            GO.transform.localPosition = value;
        }
    }

    public bool IsStable { get { return State == STATE.STABLE; } }


    // 특수 case sound play를 위한 임시 변수.
    public bool mPlayWhenMassMatching   = false;
    //

    // 
    public bool IsGoingToExp    {   get;    set;    }

    //
    enum eBOMB_BURST { eIDLE, e1st, e2nd };
    eBOMB_BURST eBombBurstState = eBOMB_BURST.eIDLE;
    public bool triggerBombMultiBursting()  // 폭탄의 2중 버스트 trigger 함수. - board.hit 직전에 호출한다.
    {
        // 일단 특수 블럭 대안 로직이 마련될때까지 봉인.
        //if(null!=PD && PD is BombPiece && eBOMB_BURST.eIDLE==eBombBurstState)
        //{
        //    eBombBurstState     = eBOMB_BURST.e1st;
        //    return true;
        //}
        return false;
    }
    //

    public float velocity;

	public GamePiece (Board _Owner) {
		Owner = _Owner;
        fallBackTime = Owner.GM.CurrentLevel.defaultFallBackTime;
        IsGoingToExp            = false;
	}

	public GamePiece (Board _Owner, PieceDefinition _pd, int _colorIndex, int _durability, int lifeCover)
	{
		Owner = _Owner;
        fallBackTime = Owner.GM.CurrentLevel.defaultFallBackTime;
        Reset(_pd, _colorIndex, _durability, lifeCover);   // [ROUND_CHOCO]
        IsGoingToExp            = false;
	}

	public void Reset (PieceDefinition _pd, int _colorIndex, int _durability, int lifeCover=0) 
	{
        Debugger.Assert(_pd != null, "GamePiece.Reset : PieceDefinition is null.");
        State = STATE.STABLE;
        velocity = GM.startVelocity;
		pd = _pd;
        if(_pd is CookieJellyPiece) // note : jelly type을 color를 사용해서 구분한다.
        {            
            ColorIndex          = -1;
            _cookieJellyType    = _colorIndex;            
        }
        else
        {
            ColorIndex = _colorIndex;
            _cookieJellyType    = 0;
        }


        IsGoingToExp            = false;
        Durability  = _durability;              // [ROUND_CHOCO]
        eBombBurstState         = eBOMB_BURST.eIDLE;
        mPlayWhenMassMatching   = false;

		ChangePiece();
		ResetSortingOrder();
		
        // [MAKE_DROP_ACC]
        _dropAccel              = GM.initDropSpeed;
        _onMoveComplete         = null;
        _isEndOfDrop            = false;
        _isDestroyable          = (_pd is SpecialFive || _pd is TMatch7Piece) ? false : true;
        _needMakeBuffWhenDestroy= false;
        _isDestroying           = false;

        if(1==lifeCover || 2==lifeCover)
            LifeCover           = lifeCover;
        else 
            LifeCover           = 0;
	}

    bool _isKindOfCookie(GamePiece piece)
    {
        if(null == piece)       return false;
        if(piece.PD is CookieJellyPiece || piece.PD is CookiePiece)
            return true;

        return false;
    }

    public void resetCookieJellyCover()
    {
        // ======================================
        // discard old one.
        //
        if(null != _sprCookieJellyCoverL)
            NNPool.Abandon(_sprCookieJellyCoverL.gameObject);
        
        if(null != _sprCookieJellyCoverU)
            NNPool.Abandon(_sprCookieJellyCoverU.gameObject);
        
        _sprCookieJellyCoverL   = null;
        _sprCookieJellyCoverU   = null;

        // ======================================
        // build new one.
        //
        if(false == _isKindOfCookie(this))
            return;

        // == left check.
        if(null!=Owner.Left && _isKindOfCookie(Owner.Left.Piece))
        {
            _sprCookieJellyCoverL   = NNPool.GetItem<SpriteImage>("SpriteImage", GO.transform);
            _sprCookieJellyCoverL.GetComponent<SpriteRenderer>().sprite = _sprCookieJellyCoverL.getSpriteByName("cookie_join_h");
            _sprCookieJellyCoverL.transform.localPosition   = new Vector3(-GM.Size*0.5f, .0f, -1.0f);
            _sprCookieJellyCoverL.transform.localScale      = new Vector3(1.05f, 1.0f, 1.0f);
        }

        // == up check
        if(null!=Owner.Top && _isKindOfCookie(Owner.Top.Piece))
        {
            _sprCookieJellyCoverU   = NNPool.GetItem<SpriteImage>("SpriteImage", GO.transform);
            _sprCookieJellyCoverU.transform.localPosition   = new Vector3(.0f, GM.Size*0.5f, -2.0f);
            _sprCookieJellyCoverU.transform.localScale      = new Vector3(1.05f, 1.0f, 1.0f);

            string strPicName       = "";
            if(null!=Owner.Left && _isKindOfCookie(Owner.Left.Piece) && null!=Owner.Right && _isKindOfCookie(Owner.Right.Piece))
                strPicName          = "cookie_join_v_04";
            if( (null==Owner.Left || (null!=Owner.Left && !_isKindOfCookie(Owner.Left.Piece))) && 
                null!=Owner.Right && _isKindOfCookie(Owner.Right.Piece))
                strPicName          = "cookie_join_v_02";
            if( null!=Owner.Left && _isKindOfCookie(Owner.Left.Piece) && 
                (null==Owner.Right || (null!=Owner.Right && !_isKindOfCookie(Owner.Right.Piece))) )
                strPicName          = "cookie_join_v_03";
            else if((null==Owner.Left || (null!=Owner.Left && !_isKindOfCookie(Owner.Left.Piece))) && 
                    (null==Owner.Right || (null!=Owner.Right && !_isKindOfCookie(Owner.Right.Piece))) )
                strPicName          = "cookie_join_v_01";

            if(false == strPicName.Equals(""))
                _sprCookieJellyCoverU.GetComponent<SpriteRenderer>().sprite = _sprCookieJellyCoverU.getSpriteByName( strPicName );       
        }

    }
	
	public float Play (string aniName, bool loop,float delay = 0.0f, System.Action onComplete = null) 
	{
		if(piece == null)       return 0.0f;
			
		Block _block            = piece.GetComponent<Block>();
        if(null==_block)        return .0f;
        if(_block is SugarCherry)
        {
            // sugar cherry 는 1번 track index에 animation을 write 한다.
            return (_block as SugarCherry).Play(aniName, loop, 1, delay, onComplete);
        }

		return _block.Play(aniName, loop,delay,onComplete);	
	}

    public int Destroy (bool isByMatch, bool performPower, bool force, bool isShowEffect, System.Action onComplete = null)
	{
        Debugger.Assert(PD != null, "GamePiece.Destroy : PieceDefinition is null.");
        Debugger.Assert(GO != null, "GamePiece.Destroy : GameObject is null.");

        if ((force == false) && GM.DestroyAwaiter.Contains(this))
		{
            if (onComplete != null) 
				onComplete();
            _isDestroying       = false;
            return 0;
        }

        // cover가 있다면 cover만 까자.
        if(LifeCover > 0)
        {
            BubbleHit eff     = NNPool.GetItem<BubbleHit>("frozen_overlay");
            //eff.play("break", .0f);
            //eff.GetComponent<MeshRenderer>().sortingOrder    = 11;
            //eff.transform.position  = new Vector3(Position.x, Position.y, -100.0f);
            eff.Play(new Vector3(Position.x, Position.y, -100.0f));
            NNSoundHelper.Play( "IFX_hard_bust" );

            LifeCover -= 1;
            if (onComplete != null)
                onComplete();

            _isDestroying       = false;
            return 0;
        }

        //if ((PD is VerticalPiece) || (PD is HorizontalPiece)) 
		//	this.GO.SetActive(false);

        _isDestroying           = true;
        float duration          = 0.1f;

        if(PD is SpecialFive==true)
            duration            = 0.2f;
        // old code.
        //if (isShowEffect)
		//{
		//	duration = 0.6F;
		//	if((PD is PenguinPiece) || (PD is FairyPiece))
		//		this.GO.SetActive(false);
		//}
		//else
        // => all should be displayed it's effects.
		{            
            Board.AI_SIDE eSide = JMFUtils.GM.isCurPlayerAI ? Board.AI_SIDE.ENEMIES : Board.AI_SIDE.MINE;
            if(GM.isAIFightMode && Owner.AiSide!=eSide && 
                (PD.isMatchable  || PD is SpecialFive || PD is TMatch7Piece)) 
                Owner.AiSide    = eSide;            
            else
			    duration        = PD.ShowDestroyEffect(this);

            // note : 조건이 길어지면 함수화 할 것.
            if(duration>=0.5f && PD.isShowDestroyAnimation())
                Play("destroy", false, duration-0.5f);
		}

        // [ROUND_CHOCO]
        if(Durability >= 0)
            Durability--;

        if(performPower && Durability<0)
			PD.PerformPower(this);

        //PieceDefinition _pd = PD;
        GameObject _piece = GO;
        Vector3 scorePos = GO.transform.position;
        int _colorIndex = ColorIndex;
        scorePos.z = Owner.Position.z;

        // 2중 bomb burst 처리. rainbow burst가 아니고, 게임중일 때만.
        PieceDefinition pdBomb = null;
        if (PD is BombPiece && eBOMB_BURST.e1st==eBombBurstState && (false==GM.ActMassMatching && JMF_GAMESTATE.PLAY==GM.State))
        {
            pdBomb              = PD;
            duration            = .0f;          // delayed burst가 필요 없음.
        }

        int score               = 0;
        if(Durability < 0)      // [ROUND_CHOCO]
        {
            score               = PD.FireOnPieceDestroy(this, isByMatch, ColorIndex);
            PD.FireOnPieceDestroyed(Owner, ColorIndex);
            PD                  = null;
            GO                  = null;
        }

        if (duration > 0F)
		{            
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(duration);
            seq.AppendCallback(() => 
            {
                if ((isByMatch == false) && (score > 0)) 
				    GM.IncreaseScore(score, scorePos, _colorIndex);
                
                // [ROUND_CHOCO]
                if(Durability < 0) RemovePiece(_piece);
				else
                {
                    ChangePiece();
                    ResetSortingOrder();
                }
				//else ChangeSpineRoundChocoPiece(PD.GetMold(0).name,Durability );
                //

                if (onComplete != null)
                    onComplete();

                _isDestroying       = false;
                /*
                if (_pd.isMatchable) {
                    GM.CollectJewel(_colorIndex); //skkim
                }
                */
            });
        } 
		else
		{
            if ((isByMatch == false) && (score > 0))
                GM.IncreaseScore(score, scorePos, _colorIndex);
            
            // [ROUND_CHOCO]
			if(Durability < 0)      RemovePiece(_piece);
			else
            {
                ChangePiece();     // ChangeSpineRoundChocoPiece( PD.GetMold(0).name,Durability );
                ResetSortingOrder();
            }
            //

            if (onComplete != null) 
                onComplete();

            _isDestroying       = false;
        }

        // 2중 bomb burst 처리 ! 
        if(null!=pdBomb && false==GM.DestroyAwaiter.Contains(Owner.Piece))
        {
            Owner.ResetPiece(pdBomb, _colorIndex);
            Owner.Piece.ShineAndBlink(.0f);
            GM.DestroyAwaiter.Add(Owner.Piece);
            eBombBurstState     = eBOMB_BURST.e2nd;
        }

        return score;
    }
	
    public void Remove () 
	{
        Remove(0F, null);
    }

    public void Remove (float delay, System.Action onComplete) 
	{
        GameObject _go = GO;
        PD = null;
        GO = null;

        if (delay > 0F) 
		{
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() => { 
                RemovePiece(_go); 
                if (onComplete != null)
					onComplete();
            });
        } 
		else
		{
            RemovePiece(_go);
            if (onComplete != null) 
				onComplete();
        }
    }
    
    #region // [MAKE_DROP_ACC]
    System.Action               _onMoveComplete = null;
    public float _dropAccel     = .0f;// _initDropSpeed;
    bool _isEndOfDrop           = false;

    public bool onMoveUpdate()
    {
        if(null == GO)
        {
            Debugger.Assert(GO != null, "onMoveUpdate : GamePiece.Drop GO is null.");
            return false;
        }

        float dropWeight         = .0f;
        {   
            for(int yy = Owner.Y-1; yy >= 0; --yy)
            {
                if(false == GM[Owner.X, yy].IsFillable)
                    break;
                dropWeight += GM.dropAccWeight;
            }
        }

        // 정속.
        //_dropAccel            = GM.dropAccRate * Time.deltaTime;
        //
        // 2중 가속. (시간누적, 가속자체 누적)
        _dropAccel += (GM.dropAccRate * Time.deltaTime * dropWeight);   //_dropAccel += (GM.dropAccRate * 0.03f * dropWeight);

        float newY              = Position.y - _dropAccel * Time.timeScale;
		//Debug.Log("====================[" + Owner.X +", " + Owner.Y + "].. _dropAccel : " + _dropAccel + "  newY : " + newY + " TimeScale : " + Time.timeScale + " orgScale.y:" + orgScale.y + " GO.transform.localScale.y:" + GO.transform.localScale.y );
		//Debug.Log(GM.Size);
        // [PASS_EMPTY_BOARD]
        if(_listEmptyBoards.Count > 0)
        {
            int idxTop          = _listEmptyBoards.Count - 1;
            if(newY < (_listEmptyBoards[idxTop].Position.y+(GM.Size*0.5f)))
            {
                newY -= GM.Size;
                _listEmptyBoards.RemoveAt( idxTop );
                // Debug.Log("==== skipped empty board.....");
            }
        }
        //
		
        float fTargetY          = Owner.Position.y - 0.5f*(orgScale.y-GO.transform.localScale.y);
        if(newY < fTargetY)
        {
            GM._listDeletables.Add( this );
            
            if(_isEndOfDrop)// && .0f==_fSlideRate)
            {
                _dropAccel      = GM.initDropSpeed;
               // _fDropElTime    = .0f;
                
				ResetSortingOrder();
                // overwrite y pos.
                Position        = new Vector3(Position.x, fTargetY, Position.z);

                //if(1 == GM._nDropSoundPlay[Owner.X])
                //  NNSoundHelper.Play( pd.getDropSndName() );
                // 로직 보강이 좀더 필요하다..음...
                if(pd.getDropSndName().Length > 0)
                    NNSoundHelper.Play( pd.getDropSndName() );

                // zelly 연출만.
                _doZellyBounce();
            }
            else
            {
                Position        = new Vector3(Owner.Position.x, newY, Position.z);  // GO.transform.position.z);    
            }

            // if treasure ? -> collect it !!!
            GM.CollectSingleTreasure(owner);

            if(_onMoveComplete != null) _onMoveComplete();
            _onMoveComplete     = null;
            State               = STATE.STABLE;
            _isEndOfDrop        = false;

            // update cookie cover all direction.
            resetCookieJellyCover();
            if(null!=Owner.Top && null!=Owner.Top.Piece)        Owner.Top.Piece.resetCookieJellyCover();
            if(null!=Owner.Left && null!=Owner.Left.Piece)      Owner.Left.Piece.resetCookieJellyCover();
            if(null!=Owner.Right && null!=Owner.Right.Piece)    Owner.Right.Piece.resetCookieJellyCover();
            if(null!=Owner.Bottom && null!=Owner.Bottom.Piece)  Owner.Bottom.Piece.resetCookieJellyCover();
            //

            return true;
        }
        Position                = new Vector3(Owner.Position.x, newY, Position.z);  // GO.transform.position.z);
        
        return false;   
    }

    // zelly-bounce 효과를 줄시, y-achor를 0에 맞추는 효과를 낸다. => y position 이동.
    void _onUpdateZellyBounce()
    {
        // CPR update. 츄잉 효과 제거. - 잠시 보류.
        if (null == GO)          return;
        float fDefaultScale     = orgScale.y;
        float fCurScale         = GO.transform.localScale.y;
        Position                = new Vector3(Position.x, Owner.Position.y - 0.5f*(fDefaultScale-fCurScale), Position.z);
    }

    public void killDropAction(bool adjustScale=true)
    {
        if(null == GO)          return;

        DOTween.Kill(GO.GetInstanceID()+3);  
        DOTween.Kill(GO.GetInstanceID()+2);  
        GO.transform.localScale = orgScale;

        if(_onMoveComplete != null)
            _onMoveComplete();
        _onMoveComplete         = null;
    }

    // [MAKE_DROP_ACC]
    void _doZellyBounce()
    {
        this.killDropAction();

        Sequence seq            = DOTween.Sequence();
        seq.SetId(GO.GetInstanceID()+2);

        if (GM.useJellyBounce) 
        {
            // CPR update. - 분기 없이 모두 이 로직을 타도록 변경.
            if(true==PD.hasSoftChewing && LifeCover<=0)
            {
                // org code - Vector3 bouncePos = targetPos + (Vector3.up * GM.Size * GM.bounceRatio);
                // 아래로 눌림 처리를 위해 down으로 수정.
                //float orgZ              = Owner.Position.z;
                Vector3 bouncePos       = Owner.Position + (Vector3.down * GM.Size * GM.bounceRatio);
                bouncePos               = new Vector3(bouncePos.x, bouncePos.y, GO.transform.position.z);
                Tween bounceTween       = GO.transform.DOMove(bouncePos, 2 * GM.bounceDuration);
                bounceTween.SetEase(Ease.OutQuad);
                bounceTween.SetLoops(1, LoopType.Yoyo);
                seq.Append(bounceTween);

                // CPR update.
                Vector3 targetScale = new Vector3(orgScale.x * (1F + GM.sloshingRatio), orgScale.y * (1F - GM.sloshingRatio), orgScale.z);
            
                seq.Insert(0F, GO.transform.DOScale(targetScale, GM.bounceDuration).SetEase(Ease.OutQuad)).OnUpdate( () => _onUpdateZellyBounce() );

                int count           = 2 * GM.jellyBounceCount;
                for(int j = 0; j < count; ++j)
                {
                    float fRate     = 0==(j%2) ? -1.0f : 1.0f;
                    float fValue    = 1.0f - (float)(j+1) / (float)count;
                    targetScale     = new Vector3(orgScale.x * (1F + fRate * GM.sloshingRatio * fValue),orgScale.y * (1F + -fRate * GM.sloshingRatio * fValue), orgScale.z);
                    seq.Append(GO.transform.DOScale(targetScale, GM.bounceDuration).SetEase(Ease.OutQuad)).OnUpdate( () => _onUpdateZellyBounce() );
                }

                seq.Append(GO.transform.DOScale(orgScale, GM.bounceDuration).SetEase(Ease.InQuad)).OnUpdate( () => _onUpdateZellyBounce() );
                
                //seq.Append(GO.transform.DOMoveY(Owner.Position.y+0.25f, 0.4f*GM.bounceDuration).SetEase(Ease.OutQuad));
                //seq.Append(GO.transform.DOMoveY(Owner.Position.y,      0.4f*GM.bounceDuration).SetEase(Ease.OutQuad));
                //float tempDuration  = 0.3f;
        //        seq.Append(GO.transform.DOMoveY(Owner.Position.y-0.05f, 0.15f*GM.bounceDuration).SetEase(Ease.OutQuad));
        //        seq.Append(GO.transform.DOMoveY(Owner.Position.y+0.05f, 0.15f*GM.bounceDuration).SetEase(Ease.OutQuad));
        //        seq.Append(GO.transform.DOMoveY(Owner.Position.y,      0.3f*GM.bounceDuration).SetEase(Ease.OutQuad));

              //-  seq.Append(GO.transform.DOMoveY(Owner.Position.y-0.1f, 0.15f*GM.bounceDuration).SetEase(Ease.OutQuad));
              //-  seq.Append(GO.transform.DOMoveY(Owner.Position.y,      0.3f*GM.bounceDuration).SetEase(Ease.OutQuad));
            }
            else
            {
               // seq.Append(GO.transform.DOMoveY(Owner.Position.y+0.5f, 0.4f*GM.bounceDuration).SetEase(Ease.Linear));
               // seq.Append(GO.transform.DOMoveY(Owner.Position.y,      0.4f*GM.bounceDuration).SetEase(Ease.Linear));
               // seq.Append(GO.transform.DOMoveY(Owner.Position.y+0.2f, 0.3f*GM.bounceDuration).SetEase(Ease.Linear));
               // seq.Append(GO.transform.DOMoveY(Owner.Position.y,      0.3f*GM.bounceDuration).SetEase(Ease.Linear));
            }
        }   

        seq.OnComplete(() => {
            if ((GO != null) && (Owner != null)) 
			{
				//ResetSortingOrder();
				GO.transform.localScale = orgScale;
            }   
        }); 
    }
	
	public void ResetSortingOrder()
	{
		Vector3 pos = Owner.LocalPosition;
		pos.z = (Owner.PT.Y + (GameManager.WIDTH -1 - Owner.PT.X)) * 0.01f;
		LocalPosition = pos;
		Position = GM.transform.TransformPoint(pos);
	}

    // [MAKE_DROP_ACC]
    public void Drop(bool isEndOfDrop, bool isDropFromSide, bool isSpawned, System.Action onComplete = null)
    {
        Debugger.Assert(GO != null, "onDrop_org : GamePiece.Drop GO is null.");

        Vector3 targetPos       = Owner.Position;
		//Vector3 pos = GO.transform.localPosition;
		//pos.z = 1.0f;
		//GO.transform.localPosition = pos;
        targetPos.z             = Position.z;   // GO.transform.position.z;

        State                   = STATE.DROP;
        Vector3 scaleAmount     = Vector3.zero;
        DOTween.Complete(GO.GetInstanceID()+1);     // 깜빡임 tween.
       
        if(isDropFromSide)
        {
            float ratio         = GM.sloshingRatio; // Mathf.Min(GM.sloshingRatio, (velocity - GM.startVelocity) / (GM.maxVelocity - GM.startVelocity));
            scaleAmount         = PD.hasSoftChewing && LifeCover<=0 ? orgScale * ratio : Vector3.zero;

            if(true == DOTween.IsTweening(GO.GetInstanceID()+2))
                GO.transform.localScale = new Vector3(orgScale.x - scaleAmount.x, orgScale.y + scaleAmount.y, orgScale.z);
            else 
                GO.transform.DOScale(new Vector3(orgScale.x - scaleAmount.x, orgScale.y + scaleAmount.y, orgScale.z), 0.27f).SetId(GO.GetInstanceID()+3);
            
            DOTween.Kill(GO.GetInstanceID()+2);     // bounce tween.

            //GO.transform.localScale = orgScale;
            GO.transform.DOMove(targetPos, GM.slideTime).SetEase(Ease.Linear).OnComplete( () =>
            {
                // check treasure.
                if(false == GM.CollectSingleTreasure(owner))
                    _doZellyBounce();

                if (null != onComplete) onComplete();
                state           = STATE.STABLE;
            });
            return;
        }

        DOTween.Kill(GO.GetInstanceID()+2);         // bounce tween.
        GO.transform.localScale = orgScale;

        //Debug.Log("!!! Drop Started. !!! id..." + GO.GetInstanceID() + "  owner[" + Owner.X +", " + Owner.Y+"]");
        _onMoveComplete         = onComplete;
        _isEndOfDrop            = isEndOfDrop;

        if(isSpawned)
            DOVirtual.DelayedCall(GM.spawnDropDelay, () => _addDropList( isDropFromSide ), false);        
        else 
            _addDropList( isDropFromSide );

        //Debug.Log("====================[" + Owner.X +", " + Owner.Y + "].. _dropAccel : " + _dropAccel);
    }

    void _addDropList(bool isDropFromSide)
    {
        Vector3 scaleAmount     = Vector3.zero;

         if(false == GM._listDropPieces.Contains(this))
            GM._listDropPieces.Add( this );

        if (GM.useJellyBounce && (isDropFromSide == false) && true==GO.transform.localScale.Equals(orgScale))
        {
            float ratio         = GM.sloshingRatio; // Mathf.Min(GM.sloshingRatio, (velocity - GM.startVelocity) / (GM.maxVelocity - GM.startVelocity));
            scaleAmount         = orgScale * ratio;
            GO.transform.DOScale(new Vector3(orgScale.x - scaleAmount.x, orgScale.y + scaleAmount.y, orgScale.z), 0.27f).SetId(GO.GetInstanceID()+3);
        }
    }

    #endregion

    // 하나의 셀 단위로 drop 처리. 
    // 한번의 drop이 끝나면 하단을 check하여 다시 또 하나를 내리는 식으로 multi-drop을 처리.
    public void Drop_old (bool isEndOfDrop, bool isDropFromSide, System.Action onComplete = null)
    {
        Debugger.Assert(GO != null, "onDrop_org : GamePiece.Drop GO is null.");

        velocity += Time.deltaTime * GM.gravity;
        velocity = Mathf.Min(GM.maxVelocity, velocity);

        Vector3 targetPos = Owner.Position;
        targetPos.z = GO.transform.position.z;

        State = STATE.DROP;

        DOTween.Complete(GO.GetInstanceID()+1);     // 깜빡임 tween.
        DOTween.Kill(GO.GetInstanceID()+2);         // bounce tween.

        Tween dropTween = GO.transform.DOMove(targetPos, 0.5f * velocity).SetSpeedBased();
        ///Tween dropTween = GO.transform.DOMove(targetPos, 0.15f * velocity).SetSpeedBased();
        dropTween.SetEase(Ease.Linear);
        dropTween.SetId(GO.GetInstanceID());

        Vector3 scaleAmount = Vector3.zero;

        if (GM.useJellyBounce && (isDropFromSide == false)) {
            float ratio = Mathf.Min(GM.sloshingRatio, 
                (velocity - GM.startVelocity) / (GM.maxVelocity - GM.startVelocity));
            scaleAmount = orgScale * ratio;
            GO.transform.localScale = new Vector3(
                orgScale.x - scaleAmount.x, 
                orgScale.y + scaleAmount.y, 
                orgScale.z);
        }   
     
        if (isEndOfDrop && (isDropFromSide == false)) {
            dropTween.OnComplete(() => {
                velocity = GM.startVelocity;
                State = STATE.STABLE;
                if (onComplete != null) onComplete();

                Sequence seq = DOTween.Sequence();
                seq.SetId(GO.GetInstanceID()+2);

                // org code - Vector3 bouncePos = targetPos + (Vector3.up * GM.Size * GM.bounceRatio);
                // 아래로 눌림 처리를 위해 down으로 수정.
                Vector3 bouncePos = targetPos + (Vector3.down * GM.Size * GM.bounceRatio);

                Tween bounceTween = GO.transform.DOMove(bouncePos, GM.bounceDuration);
                bounceTween.SetEase(Ease.OutQuad);
                bounceTween.SetLoops(2, LoopType.Yoyo);
                seq.Append(bounceTween);

                if (GM.useJellyBounce) 
                {
                    Vector3 targetScale = new Vector3(orgScale.x * (1F + GM.sloshingRatio),orgScale.y * (1F - GM.sloshingRatio), orgScale.z);
                
                    //org code.
                    //seq.Insert(0F, GO.transform.DOScale(targetScale, GM.bounceDuration).SetEase(Ease.OutQuad));
                    //seq.Insert(GM.bounceDuration, GO.transform.DOScale(orgScale, GM.bounceDuration)
                    //    .SetEase(Ease.InQuad));
                        
                    seq.Insert(0F, GO.transform.DOScale(targetScale, GM.bounceDuration).SetEase(Ease.OutQuad));

                    int count   = 2 * GM.jellyBounceCount;
                    for(int j = 0; j < count; ++j)
                    {
                        float fRate     = 0==(j%2) ? -1.0f : 1.0f;
                        float fValue    = 1.0f - (float)(j+1) / (float)count;
                        targetScale = new Vector3(orgScale.x * (1F + fRate * GM.sloshingRatio * fValue),orgScale.y * (1F + -fRate * GM.sloshingRatio * fValue), orgScale.z);
                        seq.Append(GO.transform.DOScale(targetScale, GM.bounceDuration).SetEase(Ease.OutQuad));
                    }

                    seq.Append(GO.transform.DOScale(orgScale, GM.bounceDuration)
                        .SetEase(Ease.InQuad));
                }   

                seq.OnComplete(() => {
                    if ((GO != null) && (Owner != null)) {
                    GO.transform.position   = Owner.Position;
                    GO.transform.localScale = orgScale;
                    }   
                }); 
            }); 
        } else {
            dropTween.OnComplete(() => {
                State = STATE.STABLE;

                if ((GO != null) && (Owner != null)) {
                    GO.transform.position   = Owner.Position;
                    GO.transform.localScale = orgScale;
                }   

                if (onComplete != null) onComplete();
            }); 
        }   
    }  

	public bool IsMatchable () {
        Debugger.Assert(PD != null, "GamePiece.IsMatchable : PD is null.");
        return PD.isMatchable;
    }

	public bool IsFallable () {
        Debugger.Assert(PD != null, "GamePiece.AllowGravity : PD is null.");
        return PD.isFallable;
    }
    
    bool _isDestroyable         = true;
    public void setIsDestroyable(bool val)
    {
        if(PD is SpecialFive || PD is TMatch7Piece)
            _isDestroyable      = val;
    }
	public bool IsDestroyable () {
        Debugger.Assert(PD != null, "GamePiece.IsDestroyable : PD is null.");

        if(PD is SpecialFive || PD is TMatch7Piece)
            return _isDestroyable;

        return PD.isDestroyable;
    }

	public bool IsSlidableSide () {
        Debugger.Assert(PD != null, "GamePiece.IsSlidableSide : PD is null.");
        if (IsFallable()) return false;
        return PD.isSlidableSide;
    }

	public bool IsSplashHitable () {
        Debugger.Assert(PD != null, "GamePiece.IsSplashHitable : PD is null.");
        return PD.isSplashHitable;
    }

    public void ShineAndBlink (float fDelay) 
	{
        IsGoingToExp            = true;

        DOVirtual.DelayedCall(fDelay, () =>
        {
		    if(PD == null || GO == null)
			    return;
		
		    Block _block        = GO.GetComponent<Block>();
		    if(_block != null)
            {
                //if(PD is BombPiece) _block.Play("blink", true);
                //else                _block.Play("hit_blending", true);

                _block.transform.DOScale(_block.transform.localScale.x*1.2f, 0.3f).SetLoops(-1, LoopType.Yoyo);

                // CPR update. => roll back.
                //_block.Play("hit_blending", true);
            }
        });
    }
	
	void ChangeSpineRainbowPiece (string name) 
	{
		RemovePiece(GO);
		BlockRainbow _block = NNPool.GetItem<BlockRainbow>(name);
		//_block.ChangeBlockColor( 0, 0);
		_block.Play("normal_idle", true);
		GO = _block.gameObject;
	}
	
	void ChangeSpineStonePiece (string name) 
	{
		RemovePiece(GO);
		Block _block = NNPool.GetItem<Block>(name);
		GO = _block.gameObject;
	}
	
	void ChangeSpinePiece (string name, int colorIndex, int type) 
	{
		RemovePiece(GO);
		Block _block = NNPool.GetItem<Block>(name);
		//_block.ChangeBlockColor(colorIndex, type);
        _block.resetBlockColor(colorIndex);
		GO = _block.gameObject;
		_block.Play("normal", false);
	}
	
	void ChangeSpinePotionPiece (string name) 
	{
		RemovePiece(GO);
		GameObject _block = null;
		if(name == "Potion1")
		{
			Potion1 _potion1 = NNPool.GetItem<Potion1>(name);
			_potion1.ChangeBlockColor(0, 0);
			_potion1.Play("normal", false);
			_block = _potion1.gameObject;
		}
		else if(name == "Potion2")
		{
			Potion2 _potion2 = NNPool.GetItem<Potion2>(name);
			_potion2.ChangeBlockColor(0, 0);
			_potion2.Play("normal", false);
			_block = _potion2.gameObject;
		}
		else
		{
			Potion3 _potion3 = NNPool.GetItem<Potion3>(name);
			_potion3.ChangeBlockColor(0, 0);
			_potion3.Play("normal", false);
			_block = _potion3.gameObject;
		}
			
		GO = _block;
	}
	
	bool ChangeSpineGreenBubblePiece (string name, int index) 
	{
		bool bSetup = true;
		GreenBubble _greenBubble;
		if(GO == null)
		{
			_greenBubble = NNPool.GetItem<GreenBubble>(name);	
			GO = _greenBubble.gameObject;
			GO.transform.parent = GM.gameObject.transform;
			GO.GetComponent<PieceTracker>().PT = Owner.PT;
			JMFUtils.SpineObjectAutoScalePadded(GO);
		}
		else
		{
			_greenBubble = GO.GetComponent<GreenBubble>();
			bSetup = false;
		}

		//_greenBubble.ChangeBlockColor(index, 0);
        _greenBubble.resetBlockColor(index);
		_greenBubble.Play("normal", false);
		return bSetup;
	}
	
	void ChangeSpineSugarCherryPiece (string name) 
	{
		RemovePiece(GO);
		SugarCherry _sugarCherryPiece = NNPool.GetItem<SugarCherry>(name);
		// _sugarCherryPiece.ChangeBlockColor(0, 0); - sugar cherry의 color를 바꿀 필요 없음.
		GO = _sugarCherryPiece.gameObject;
        _sugarCherryPiece.Play("normal_idle", true, 0);
	}
	
	void ChangeSpineZellatoPiece (string name) 
	{
		RemovePiece(GO);
		Zellato _zellatoPiece = NNPool.GetItem<Zellato>(name);
		_zellatoPiece.ChangeBlockColor(0, 0);
		GO = _zellatoPiece.gameObject;
		_zellatoPiece.Play("normal", false);
	}
	
	void ChangeSpineStrawberryPiece (string name) 
	{
		RemovePiece(GO);
		Strawberry _strawberry = NNPool.GetItem<Strawberry>(name);
		//_strawberry.ChangeBlockColor(0, 0);
		GO = _strawberry.gameObject;
		_strawberry.Play("normal", false);
	}
	
	bool ChangeSpineRoundChocoPiece (string name, int index) 
	{
		bool bSetup = true;
		SpiralSnow _spiralSnow;
		if(GO == null)
		{
			_spiralSnow = NNPool.GetItem<SpiralSnow>(name);	
			GO = _spiralSnow.gameObject;
			GO.transform.parent = GM.gameObject.transform;
			GO.GetComponent<PieceTracker>().PT = Owner.PT;
			JMFUtils.SpineObjectAutoScalePadded(GO);
		}
		else
		{
			_spiralSnow = GO.GetComponent<SpiralSnow>();
			bSetup = false;
		}
		
		//_spiralSnow.ChangeBlockColor(index, 0);
        _spiralSnow.resetBlockColor(index);
		_spiralSnow.Play("normal", false);
		return bSetup;
	}
	
    bool ChangeCookieJellyPiece (int durability)
    {
        RemovePiece(GO);
        SpriteBlock sp          = NNPool.GetItem<SpriteBlock>("SpriteBlock", GM.transform);
        sp.GetComponent<SpriteRenderer>().sprite = sp.getSpriteByName( string.Format("cookie_jelly_{0}{1}", _cookieJellyType+1, durability+1) );
        GO                      = sp.gameObject;
        return true;
    }
    bool ChangeCookiePiece (int durability)
    {
        RemovePiece(GO);
        SpriteBlock sp          = NNPool.GetItem<SpriteBlock>("SpriteBlock", GM.transform);
        sp.GetComponent<SpriteRenderer>().sprite = sp.getSpriteByName( string.Format("cookie_{0}", durability+1) );
        GO                      = sp.gameObject;
        return true;
    }


	void ChangeSpinePieceBomb (string name, int colorIndex, int type) 
	{
		RemovePiece(GO);
		BlockBomb _block = NNPool.GetItem<BlockBomb>(name);
		//_block.ChangeBlockColor(colorIndex, type);
        _block.resetBlockColor(colorIndex);
		GO = _block.gameObject;
		_block.Play("normal_idle", true);
	}
	
    void ChangePiece() 
	{
		bool bSetup = true;
		if(PD is NormalPiece)
			ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 0);
		else if(PD is HorizontalPiece)
			ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 1);
		else if(PD is VerticalPiece)
			ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 2);
        //
        else if(PD is BombPiece)
			ChangeSpinePieceBomb(PD.GetMold(0).name, ColorIndex, 3);
            // ChangeSpinePiece("block", ColorIndex, 5);           // CPR update. - skin 변경. => roll back.
        //
		else if(PD is SpecialFive)
			ChangeSpineRainbowPiece(PD.GetMold(0).name);
        //
        //else if(PD is TMatch7Piece)
			//ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 5);
        //    ChangeSpineRainbowPiece(PD.GetMold(0).name);        // CPR update. - skin 변경.
        //
		else if(PD is RoundChocoPiece)                          // [ROUND_CHOCO]
			bSetup = ChangeSpineRoundChocoPiece(PD.GetMold(0).name, Durability);
		else if(PD is GreenBubblePiece)                          // [GREEN_BUBBLE]
			bSetup = ChangeSpineGreenBubblePiece(PD.GetMold(0).name, Durability);
		else if(PD is Potion1Piece || PD is Potion2Piece || PD is Potion3Piece)
			ChangeSpinePotionPiece(PD.GetMold(0).name);
		else if(PD is StrawberryPiece)
			ChangeSpineStrawberryPiece(PD.skin[0].name);
		else if(PD is SugarCherryPiece)
			ChangeSpineSugarCherryPiece(PD.skin[0].name);
		else if(PD is ZellatoPiece)
			ChangeSpineZellatoPiece(PD.skin[0].name);
		else if(PD is SugaredPiece)
			ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 6);
        else if(PD is AddTimePiece)
			ChangeSpinePiece(PD.GetMold(0).name, ColorIndex, 6);
		else if(PD is StonePiece)
			ChangeSpineStonePiece(PD.GetMold(0).name);
        else if(PD is CookieJellyPiece) 
			bSetup = ChangeCookieJellyPiece(Durability);
        else if(PD is CookiePiece) 
			bSetup = ChangeCookiePiece(Durability);
		else
		{
			RemovePiece(GO);
			GO = NNPool.GetItem(PD.GetMold(ColorIndex).name);

            if(PD is StonePiece)    // kill collider.
                GO.GetComponent<BoxCollider>().enabled = false;
		}
		
        if(bSetup)
		{
			GO.transform.parent = GM.transform;
			GO.GetComponent<PieceTracker>().PT = Owner.PT;
			JMFUtils.SpineObjectAutoScalePadded(GO);			
		}

        // [ADJUST SCALE]
        // bomb piece do more scale.
        if (PD is BombPiece)            GO.transform.localScale = Vector3.one;// * 1.1f;
        else if (PD is CookieJellyPiece || PD is CookiePiece)
                                        GO.transform.localScale = Vector3.one * 1.1f;
        //else if(PD is GreenBubblePiece) GO.transform.localScale = new Vector3(0.8f, 0.7f, 1.0f);
        else if(PD is StrawberryPiece)  GO.transform.localScale = new Vector3(1.2f, 1.0f, 1.0f);
        else if(PD is Potion1Piece || PD is Potion2Piece || PD is Potion3Piece)
            GO.transform.localScale     = Vector3.one * 0.75f;
        else if(PD is ZellatoPiece)     GO.transform.localScale = Vector3.one * 0.9f;
        else if(PD is GreenBubblePiece) GO.transform.localScale = Vector3.one * 1.1f;
        else                            GO.transform.localScale = Vector3.one;

        orgScale                = GO.transform.localScale;

        resetCookieJellyCover();

        // reset collider scale.
        GO.GetComponent<Block>().resetColliderSize();
        
		PD.FireOnPieceCreate(this);
    }

    void RemovePiece (GameObject go) 
	{
        // discard cookie-jelly.
        if(null != _sprCookieJellyCoverL)
            NNPool.Abandon(_sprCookieJellyCoverL.gameObject);
        
        if(null != _sprCookieJellyCoverU)
            NNPool.Abandon(_sprCookieJellyCoverU.gameObject);
        
        if(null != _objCover)
        {
            _objCover.transform.DOKill();
            NNPool.Abandon(_objCover);
            _objCover           = null;
        }
        if (go != null) 
		{
            DOTween.Complete(go.GetInstanceID());
            DOTween.Kill(go.GetInstanceID()+1);
            DOTween.Kill(go.GetInstanceID()+2);
            go.transform.DOKill();
            NNPool.Abandon(go);
        }
        
        _sprCookieJellyCoverL   = null;
        _sprCookieJellyCoverU   = null;
        _isDestroying           = false;

        LifeCover               = 0;

        eBombBurstState         = eBOMB_BURST.eIDLE;
    }

    public override string ToString () {
        return string.Format("GP[Point:{0}, PD:{1}, Color:{2}]", 
            (Owner == null) ? "(?,?)" : Owner.PT.ToString(),
            (PD == null) ? "Unknown" : PD.GetType().FullName,
            ColorIndex);
    }
}
