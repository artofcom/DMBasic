using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DG.Tweening;
using NOVNINE.Diagnostics;
using Spine.Unity;
using Spine;

public enum PORTAL_TYPE { NONE, DOWN, UP, ALL }
public enum BoardDirection {Top, Bottom, Left, Right, BottomRight, BottomLeft, TopLeft, TopRight }

public class Board {
    public enum STATE { STABLE, SWAP, DROP, WAIT_DESTROY }

    public const float _SCALE_SHADE_ON_RIVER    = 0.8f;         

	public Point PT { get; set; }
    public bool IsStealed { get; set; }
    public int X { get { return PT.X; } }
    public int Y { get { return PT.Y; } }
	public bool IsNeedDropCheck { get; set; }
	public bool IsNeedMatchCheck { get; set; }
	public GameManager GM { get; private set; }
	public int PortalDIndex { get; private set; }
    public int PortalUIndex { get; private set; }
	public Vector3 Position { get; private set; }
	public Vector3 LocalPosition { get; private set; } 
    public PORTAL_TYPE PortalType { get; private set; }

	public Board Top { get { return GM[X,Y+1]; } }
	public Board Bottom { get { return GM[X,Y-1]; } }
	public Board Right { get { return GM[X+1,Y]; } }
	public Board Left { get { return GM[X-1,Y]; } }
	public Board TopLeft { get { return GM[X-1,Y+1]; } }
	public Board TopRight { get { return GM[X+1,Y+1]; } }
	public Board BottomLeft { get { return GM[X-1,Y-1]; } }
	public Board BottomRight { get { return GM[X+1,Y-1]; } }

    public bool IsStable { get { return State == STATE.STABLE; } }
    public bool IsPortal { get { return PortalType != PORTAL_TYPE.NONE; } }
    public int ColorIndex { get { return IsFilled ? Piece.ColorIndex : 0; } }
	public PieceDefinition PD { get { return IsFilled ? Piece.PD : null; } }
	public PanelDefinition PND { get { return Panel == null ? null : Panel.PND; } }
    public ReadOnlyCollection<Board> Neighbours { get { return neighbours.AsReadOnly(); } }

    public Board SkipTarget     { get; set; }

    // View at Boards Layer Structure.
    GameObject portal_in, portal_out;       // Layer 1. 최상단.
    BoardPanel panel;                       // Layer 2 or 3.
    GamePiece piece;                        // Layer 3.
    //SkeletonRenderer shaded;                // Layer 4.
    SpriteRenderer _shaded      = null;
    SpriteRenderer _bg          = null;
	//SkeletonAnimation sprAiTaken = null;     // - shaded 와 동일 Layer.
    SpriteImage _sprAiTaken     = null;
    GameObject _objChocoBar     = null;     // Layer 5.
    GameObject _objOverRiver    = null;     // Layer 6.
    BoardPanel _pnlRiver        = null;     // Layer 7.
                                            //
    // for New Object Test.
    SpriteImage _sprShaded      = null;     // Layer 4 
    public SpriteImage getSprShaed()        { return _sprShaded; }

    public int _eHelperType     = -1;   // LEItem.HELPER_TYPE
    public int _idConveyor      = -1;   // 1 or more is valid.

    public bool _isTreasureGoal = false;

    // Matching Info.
    List<Board> _listMatchings  = new List<Board>();
    public List<Board> ListMatchings
    {
        get { return _listMatchings; }
        private set { }
    }

    // [AI_MISSION]	
    public enum AI_SIDE { NONE, MINE, ENEMIES };
    AI_SIDE eSide               = AI_SIDE.NONE;
    
	public AI_SIDE AiSide
    {
        get { return eSide; }
        set
        {
            if(eSide == value)  return;
            if(_changeColorIndex >= 0)
                return;         // color changer가 있으면 ai taken 처리 않는다.

            eSide               = value;

            // note : side에 따라서 burst 처리.
			BlockCrash fx       = null;
		    Vector3 pos         = Position; pos.z -= 2.0f;
            switch(eSide)
            {
			case AI_SIDE.ENEMIES:   fx = NNPool.GetItem<BlockCrash>("ai_board_enemies_hit");break;
			case AI_SIDE.MINE:      fx = NNPool.GetItem<BlockCrash>("ai_board_mine_hit");   break;
            case AI_SIDE.NONE:      fx = NNPool.GetItem<BlockCrash>("ai_board_enemies_hit");break;
            }
				
			if(fx!=null && false==GM._isRemovedByConveyor)
                fx.Play("play", pos, Vector3.one, 0);
                
            if(AI_SIDE.NONE == eSide)
            {
                if(null != _sprAiTaken)
                    NNPool.Abandon( _sprAiTaken.gameObject );
                _sprAiTaken     = null;
                return;
            }

			if(_sprAiTaken == null)
			{
				_sprAiTaken = NNPool.GetItem<SpriteImage>("AIPanel",GM.transform);	
				_sprAiTaken.transform.position       = new Vector3(Position.x, Position.y, GM.transform.position.z+4);
				JMFUtils.SpineObjectAutoScalePadded(_sprAiTaken.gameObject);
                _sprAiTaken.transform.localScale     = null==_pnlRiver ? _sprAiTaken.transform.localScale : _sprAiTaken.transform.localScale*0.8f;

				//Debug.Log( string.Format( "Pos AI Board....{0}, {1}" ,Position.x, Position.y) );
			}

            //int slotIndex = sprAiTaken.Skeleton.FindSlotIndex("ai_board_enemies");
            //Slot _slot = sprAiTaken.Skeleton.FindSlot("ai_board_enemies");
            //if(AI_SIDE.MINE == eSide) _slot.Attachment = sprAiTaken.Skeleton.GetAttachment(slotIndex,"ai_board_mine");	
            //else _slot.Attachment = sprAiTaken.Skeleton.GetAttachment(slotIndex,"ai_board_enemies");	                
            string strPicFName  = AI_SIDE.MINE==eSide ? "ai_board_mine" : "ai_board_enemies";
            _sprAiTaken.GetComponent<SpriteRenderer>().sprite = _sprAiTaken.getSpriteByName( strPicFName );	
            //
        }
    }
    //

    STATE state;
    public STATE State { 
        get { return state; }
        set {
            if (state == value) return;

            switch (value) {
                case STATE.SWAP :
                case STATE.DROP :
                    Debugger.Assert(state == STATE.STABLE,
                        string.Format("Board.State : {0} {1}->{2} ]", PT.ToString(), state, value));
                    break;
                case STATE.WAIT_DESTROY :
                    Debugger.Assert(state != STATE.SWAP,
                        string.Format("Board.State : {0} {1}->{2} ]", PT.ToString(), state, value));
                    break;
            }

            // Debug.Log(string.Format("[Board State : {0} {1}->{2} ]", PT.ToString(), state, value));
            state = value;

#if DEV_MODE && UNITY_EDITOR
            if (GM != null) GM.OnChangeBoardState(this, state);
#endif
        }
    }

	public BoardPanel Panel { 
        get { return panel; }
        set {
            if (panel == value) return;

            panel = value;
            
			if (panel == null) return;

            panel.Owner = this;
        }
    }
	
	public GamePiece Piece {
		get { return piece; }
		set {
            if (piece == value) return;

			piece = value;

			if (piece == null) return;

            piece.Owner = this;

            if (piece.GO != null) {
                PieceTracker tracker = piece.GO.GetComponent<PieceTracker>();
                if (tracker != null) tracker.PT = PT;
            }
        }
    }

	public bool IsFilled {
		get { 
			if (Piece == null) return false;
            if (Piece.PD == null) return false;
			return true;
		}
	}

    public bool IsStealable {
        get {
            if (PND is CreatorPanel) return true;
            if (IsFilled == false && State != STATE.WAIT_DESTROY) return false;
            if (IsFilled == true && Piece.IsFallable() == false) return false;
            if (Panel.IsStealable() == false) return false;
            if (false == GM.isPassableThroughFence(this, JMF_DIRECTION.DOWN)) return false;
            if(null!=Piece && true==Piece.IsGoingToExp) return false;

            return true;
        }
    }

    public bool IsFillable {                                // 뭔가를 채울있는, 현재 빈 상태인가 ?
        get {
            if (IsFilled) return false;                     // 보드에 셀이 차 있지 않아야 하고
            if (IsStable == false) return false;            // stable 해야 하며
            if (Panel.IsFillable() == false) return false;  // 패널이 fillable 해야 

            return true;                                    // return true.
        }
    }

    public bool IsDropable {
        get {
            if (IsPortal && (PortalType == PORTAL_TYPE.DOWN))   return false;
            if (IsPortal && (PortalType == PORTAL_TYPE.ALL))    return false;
            if (Panel.IsStealable() == false) return false;
            if (IsFilled && (Piece.IsFallable() == false)) return false;
            if (false == GM.isPassableThroughFence(this, JMF_DIRECTION.DOWN)) return false;
            if(null!=Piece && true==Piece.IsGoingToExp) return false;

            return true;
        }
    }

	public bool IsMatchable {
        get {
            if (IsFilled == false) return false;
            if (IsStable == false) return false;
            if (Panel.IsMatchable() == false) return false;
            if (Piece.IsMatchable() == false) return false;
            if(null!=Piece && true==Piece.IsGoingToExp) return false;

            return true;
        }
	}

    public int BottomUnfilledCount {
        get {
            int count = 0;

            if (Panel.IsStealable() == false) return 0;
            if (false == GM.isPassableThroughFence(this, JMF_DIRECTION.DOWN))
                return 0;

            for (int y = Y-1; y >= 0 ; y--) {

                if (GM[X,y].IsFillable && false==GM[X,y].IsPortal)
                    count++;

                // [PASS_EMPTY_BOARD]
                if (GM[X,y].PND is EmptyPanel)
                    continue;

                if (false == GM.isPassableThroughFence(GM[X,y], JMF_DIRECTION.DOWN))
                    break;

                // if river-stone piece ? then break. -> fix river drop bugs.
                if(null!=GM[X,y].PD && GM[X,y].PD is StonePiece)
                    break;

                if(null!=GM[X,y].Piece && true==GM[X,y].Piece.IsGoingToExp)
                    break;
                
                if(GM[X,y].Panel.IsStealable() == false)
                    break;
                if (GM[X,y].Panel.IsFallable() == false)
                    break;
            }

            return count;
        }
    }

    public int eShadeType = -1; // LEItem.SHADE_TYPE - [JAM_SHADE]

    int shadedDurability = -1;
    public int ShadedDurability {
        get { return shadedDurability; }
        set {
            value = Mathf.Max(-1, value);

            if (shadedDurability == value) return;

            bool showEffect     = (GM.State == JMF_GAMESTATE.PLAY) && (value < shadedDurability);

            shadedDurability = value;
            UpdateShaded();

            if(GM._isRemovedByConveyor)
                return;

            JMFRelay.FireOnChangeRemainShade();
            if (showEffect)
			{                
//				ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("StoneCrash", GM.transform);
//                pp.transform.localPosition = LocalPosition;
//                pp.Play();

                switch((LEItem.SHADE_TYPE)eShadeType)
                {
                case LEItem.SHADE_TYPE.NET:
                {
                    NNSoundHelper.Play("IFX_mesh_clear");

                    PanelNetHit eff = NNPool.GetItem<PanelNetHit>("Panel_net_hit");
		            Vector3 pos     = Position; pos.z -= 2.0f;
                    //eff.Play(pos, Vector3.one, false, .0f);
                    eff.Play(pos, Vector3.one, false, .0f, 1.0f, null, "bubble_overbubbleL" );

                    if (ShadedDurability >= 0)  break;

                    GameManager.CHOCO_BAR_INFO info = new GameManager.CHOCO_BAR_INFO(LEItem.CHOCO_BAR.NONE);
                        
                    #region -> find this boards choco bar type info structure.
                    int index_of_this= X+(GameManager.HEIGHT-1-Y)*GameManager.WIDTH;
                    for(int qq = 0; qq < GM._listChocoBarInfos.Count; ++qq)
                    {
                        for(int rr = 0; rr < GM._listChocoBarInfos[qq].listIdxBoards.Count; ++rr)
                        {
                            if(index_of_this == GM._listChocoBarInfos[qq].listIdxBoards[rr])
                            {
                                info    = GM._listChocoBarInfos[qq];
                                break;
                            }
                        }
                        if(info.eType != LEItem.CHOCO_BAR.NONE)
                            break;
                    }
                    #endregion

                    #region -> if all net shade is cleared, then fly choco-bars~~~
                    if (info.eType != LEItem.CHOCO_BAR.NONE)
                    {
                        bool all_clear  = true;
                        Board bd0       = null;
                        for(int uu = 0; uu < info.listIdxBoards.Count; ++uu)
                        {
                            int idxCooked   = GameManager.WIDTH * (GameManager.HEIGHT-1- info.listIdxBoards[uu]/GameManager.WIDTH) + info.listIdxBoards[uu]%GameManager.WIDTH;
                            if(0 == GM[ idxCooked ]._indexBar)
                                bd0     = GM[ idxCooked ];
                            if(GM[ idxCooked ].ShadedDurability >= 0)
                            {
                                all_clear   = false;
                                break;
                            }
                        }
                        if(true==all_clear && null!=bd0)
                        {
                            // 단지 연출을 조금 뒤로 한다.
                            DOVirtual.DelayedCall(0.5f, () =>
                            {
                                GM.AnimatedGainChocoBar(this, bd0._strChocoBarName, bd0._objChocoBar.transform);

                                //note : 날려서 미션에 보낸다음 abandon 하자.
                                //= NNPool.Abandon(bd0._objChocoBar);
                                bd0._objChocoBar    = null;
                            });
                            // de-init.
                            _eBarType           = LEItem.CHOCO_BAR.NONE;
                        }
                    }
                    #endregion

                    break;
                }
                case LEItem.SHADE_TYPE.JAM:
                {
                    // NNSoundHelper.Play("IFX_mud_clear");

                    JMFUtils.GM.AnimateGainShaded(this);  
                    
                    BubbleHit effect= NNPool.GetItem<BubbleHit>("BubbleHit");
		            Vector3 pos     = Position; pos.z -= 2.0f;
                    effect.Play(pos, Vector3.one, 7, false, .0f);   // violet.                
                    break;
                }
                case LEItem.SHADE_TYPE.CURSE:
                {
                    NNSoundHelper.Play("IFX_mud_clear");

                    JMFUtils.GM.AnimateGainShaded(this);

                    BlockCrash effect= NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
		            Vector3 pos      = Position; pos.z -= 2.0f;
                    effect.Play("play", pos, Vector3.one, 0);
                    break;
                }
                case LEItem.SHADE_TYPE.MUD_COVER:
                {
                    NNSoundHelper.Play("IFX_mud_clear");

                    JMFUtils.GM.AnimateGainShaded(this);

                    BlockCrash effect= NNPool.GetItem<BlockCrash>("ObstacleBgstoneHit");
		            Vector3 pos      = Position; pos.z -= 2.0f;
                    effect.Play("play", pos, Vector3.one, 0);
                    break;
                }
                default:    break;

                }   // end of switch.
                
                if (ShadedDurability < 0)
                    eShadeType  = (int)LEItem.SHADE_TYPE.NONE;
            } else {
                //JMFRelay.FireOnChangeRemainShadeForDisplay(ShadedDurability, eShadeType);
            }
        }
    }

    public void removeTreasureGoal()
    {
        _isTreasureGoal         = false;
        if(null!=_shaded && (int)LEItem.SHADE_TYPE.NONE==eShadeType)
        {
           NNPool.Abandon(_shaded.gameObject);
           _shaded              = null;
        }
    }

    // overwrite shades.
    public void initTreasureGoal()
    {
        _isTreasureGoal         = true;

        if(null!=_shaded)       NNPool.Abandon( _shaded.gameObject );

		potionOut SR = NNPool.GetItem<potionOut>("potion_out", GM.transform);
		Vector3 newPos = this.LocalPosition;
		// [PIECE_POTION]
		//newPos.y -= Size * 0.5F;
		//newPos.z = -10F;
		newPos.z = 4.0F;
		SR.transform.localPosition  = newPos;
        SR.transform.localScale     = Vector3.one * 1.2f;
		JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
		SR.Play(UnityEngine.Random.Range(0.0f,1.0f));

		_shaded                 = SR.GetComponent<SpriteRenderer>();
        eShadeType              = (int)LEItem.SHADE_TYPE.NONE;
    }

    public Board this[JMF_DIRECTION direction] {
        get {
            switch (direction) {
                case JMF_DIRECTION.UP : return Top;
                case JMF_DIRECTION.DOWN : return Bottom;
                case JMF_DIRECTION.LEFT : return Left;
                case JMF_DIRECTION.RIGHT : return Right;
                case JMF_DIRECTION.UPRIGHT : return TopRight;
                case JMF_DIRECTION.UPLEFT : return TopLeft;
                case JMF_DIRECTION.DOWNLEFT : return BottomLeft;
                case JMF_DIRECTION.DOWNRIGHT : return BottomRight;
                default : return null;
            }
        }
    }

    int destroyCount;
    public int UnstableCount { 
        private get { return destroyCount; }
        set { 
            destroyCount = value;

            if (destroyCount > 0) {
                State = STATE.WAIT_DESTROY;
            } else {
                State = STATE.STABLE;
            }
        }
    }

	List<Board> neighbours = new List<Board>();

	public Board (GameManager gm, Point pt, Vector3 pos) {
		GM = gm;
		PT = pt;
		LocalPosition = pos;
		Position = GM.transform.TransformPoint(pos);
	}
	
	public void UpdatePosition() 
	{	
		Vector3 pos = LocalPosition;
		pos.z = (PT.Y + (GameManager.WIDTH -1 - PT.X));
		LocalPosition = pos;
		Position = GM.transform.TransformPoint(pos);
        
		// Position = GM.transform.TransformPoint(LocalPosition);		
	}

    public void ResetBoard () 
	{
        if (Panel != null) 
		{
            Panel.Remove();
            Panel = null;
        }

        if (Piece != null) {
            Piece.Remove();
            Piece = null;
        }

        if (portal_in != null) {
			NNPool.Abandon(portal_in);
            portal_in = null;
        }
        if (portal_out != null) {
			NNPool.Abandon(portal_out);
            portal_out = null;
        }

        // [AI_MISSION]
        if(null != _sprAiTaken)
        {
			NNPool.Abandon( _sprAiTaken.gameObject );
            _sprAiTaken         = null;
        }
        eSide                   = AI_SIDE.NONE;
        //

        if(null != _shaded)
        {
            NNPool.Abandon(_shaded.gameObject);
            _shaded             = null;
        }
        if(null != _sprShaded)
        {
            NNPool.Abandon(_sprShaded.gameObject);
            _sprShaded          = null;
        }
        if(null != _objChocoBar)
        {
            NNPool.Abandon(_objChocoBar);
            _objChocoBar        = null;
        }
        if(null != _objOverRiver)
        {
            NNPool.Abandon(_objOverRiver);
            _objOverRiver       = null;
        }
        if(null != _pnlRiver) 
		{
            _pnlRiver.Remove();
            _pnlRiver           = null;
        }
        if(null != _bg)
        {
            NNPool.Abandon( _bg.gameObject );
            _bg                 = null;
        }

        PortalUIndex = -1;      PortalDIndex = -1;
        ShadedDurability = -1;
        IsNeedMatchCheck = false;
        PortalType = PORTAL_TYPE.NONE;
        _changeColorIndex       = -1;
        SkipTarget              = null;

        clearMatchingList();
    }

    public void initBG()
    {
        if(null != this._bg)
            NNPool.Abandon( _bg.gameObject );

        this._bg                = NNPool.GetItem<SpriteRenderer>("BoardBG", GM.transform);
		JMFUtils.SpineObjectAutoScalePadded(_bg.gameObject);
        _bg.sortingOrder        = 0;
        this._bg.transform.localScale   = Vector3.one * 0.82f;
        this._bg.transform.localPosition= new Vector3(LocalPosition.x, LocalPosition.y, 10);
    }

    // [AI_MISSION]
    public void initAiTaken(int ai_taken_index)
    {
        AiSide                  = ai_taken_index == 0 ? AI_SIDE.MINE : ( ai_taken_index==1 ? AI_SIDE.ENEMIES : AI_SIDE.NONE );
    }

	public void UpdateNeighbours () {
        if (Top != null) neighbours.Add(Top);
        if (Bottom != null) neighbours.Add(Bottom);
        if (Right != null) neighbours.Add(Right);
        if (Left != null) neighbours.Add(Left);
	}

    public void ResetPanel (PanelDefinition pnd, int durability, object info = null, int color=-1) 
	{
        if(pnd.GetType() == typeof(ConveyorPanel))
        {
            Debug.Assert(false, "Use ResetRiverPanel() instead.");
            return;
        }
        
        if (Panel == null)
            Panel = new BoardPanel(this, pnd, durability, info, color);
        else
            Panel.Reset(pnd, durability, info, color);
    }

    public BoardPanel PnlRiver()    {   return _pnlRiver;   }
    public void ResetRiverPanel(object info, bool turnOnWaffle)
    {
        if(null == _pnlRiver)   _pnlRiver   = new BoardPanel(this, GM.PanelTypes[22], 0, info);
        else                    _pnlRiver.Reset(GM.PanelTypes[22], 0, info);

        _initOverRiverWaffle( turnOnWaffle );

        // shade re scale.
        if(null != _shaded)     _shaded.transform.localScale     = Vector3.one * _SCALE_SHADE_ON_RIVER;
    }

    void _initOverRiverWaffle(bool turnOnWaffle)
    {
        if(null!=_objOverRiver) NNPool.Abandon(_objOverRiver);
        _objOverRiver           = null;

        if(turnOnWaffle)
        {
            _objOverRiver       = NNPool.GetItem("River_Waffle", GM.transform);
            // string strPath      = "Assets/UI/block_new/Panels/river_panel.png";
            // _objOverRiver.GetComponent<SpriteRenderer>().sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(strPath) as Sprite;
            _objOverRiver.transform.localPosition = new Vector3(LocalPosition.x, LocalPosition.y, 6.0f);
        }
    }

	public void ShuffePieceColorIndex( int colorIndex) 
	{
		if(Piece.PD is NormalPiece || Piece.PD is SugaredPiece)
		{
			Piece.ColorIndex = colorIndex;
			IsNeedMatchCheck = true;
			State = STATE.STABLE;			
		}
	}
	
    // [ROUND_CHOCO]
	public void ResetPiece (PieceDefinition pd, int colorIndex, int durability=-1, int lifeCover=0) 
	{
        if (Piece == null) 
		    Piece = new GamePiece(this, pd, colorIndex, durability, lifeCover);
		else 
		    Piece.Reset(pd, colorIndex, durability, lifeCover);
	
		IsNeedMatchCheck = true;
        State = STATE.STABLE;
	}

    public void SetupPortal (PORTAL_TYPE portalType, int idxU, int idxD) {
        Debugger.Assert(portalType != PORTAL_TYPE.NONE, "Board.SetupPortal : Type is none.");

        PortalUIndex = idxU;    PortalDIndex    = idxD;
        PortalType = portalType;
        
        switch(PortalType)
        {
        case PORTAL_TYPE.DOWN:
        {
            portal_in           = NNPool.GetItem("PortalA", GM.transform);
            portal_in.transform.localPosition = new Vector3(LocalPosition.x, LocalPosition.y, 0.1f);
            portal_in.GetComponent<BoxCollider>().enabled  = false;
            //SkeletonAnimation   particle = portal_in.transform.Find("portal_eff").GetChild(0).GetComponent<SkeletonAnimation>();
            //TrackEntry tr       = particle.AnimationState.SetAnimation(0, "in", true);
            //tr.delay            = 1.0f;
            //JMFUtils.autoScale(portal_in);
            break;
        }
        case PORTAL_TYPE.UP:
        {
            portal_out          = NNPool.GetItem("PortalB", GM.transform);
            portal_out.transform.localPosition = new Vector3(LocalPosition.x, LocalPosition.y, 0.1f); // LocalPosition;
            portal_out.GetComponent<BoxCollider>().enabled  = false;
            //JMFUtils.autoScale(portal_out);
            //SkeletonAnimation   particle = portal_out.transform.Find("portal_eff").GetChild(0).GetComponent<SkeletonAnimation>();
            //TrackEntry tr       = particle.AnimationState.SetAnimation(0, "out", true);
            //tr.delay            = 1.0f;
            break;
        }
        case PORTAL_TYPE.ALL:
        {
            portal_in           = NNPool.GetItem("PortalA", GM.transform);            
            //portal_in.transform.localPosition = LocalPosition;
            portal_in.transform.localPosition = new Vector3(LocalPosition.x, LocalPosition.y, 0.1f);
            portal_in.GetComponent<BoxCollider>().enabled  = false;
            //SkeletonAnimation   particle = portal_in.transform.Find("portal_eff").GetChild(0).GetComponent<SkeletonAnimation>();
            //TrackEntry tr       = particle.AnimationState.SetAnimation(0, "in", true);
            //tr.delay            = 1.0f;
            //JMFUtils.autoScale(portal_in);
            portal_out          = NNPool.GetItem("PortalB", GM.transform);
            portal_out.transform.localPosition = new Vector3(LocalPosition.x, LocalPosition.y, 0.1f); //LocalPosition;
            portal_out.GetComponent<BoxCollider>().enabled  = false;
            //particle            = portal_out.transform.Find("portal_eff").GetChild(0).GetComponent<SkeletonAnimation>();
            //tr                  = particle.AnimationState.SetAnimation(0, "out", true);
            //tr.delay            = 1.0f;
            //JMFUtils.autoScale(portal_out);
            break;
        }
        default:    break;
        }

        //JMFUtils.autoScale(portal);
    }
	
    public int SplashHit(float delay)
    {
        return Hit(false, true, delay, true, false, false, null);
    }

    public int SplashHit () {
        return Hit(false, true, 0F, true, false, false, null);
    }

    public int SplashHit (System.Action onComplete) {
        return Hit(false, true, 0F, true, false, false, onComplete);
    }

    public int Hit () {
        return Hit(false, true, 0F, false, false, false, null);
    }

    public int Hit (bool isByMatch) {
        return Hit(isByMatch, true, 0F, false, false, false, null);
    }

    public int Hit (bool isByMatch, bool isShowEffect) {
        return Hit(isByMatch, true, 0F, false, isShowEffect, false, null);
    }

    public int Hit (float delay) {
        return Hit(false, true, delay, false, false, false, null);
    }

    public int Hit (float delay, bool isSpecialAttack) {
        return Hit(false, true, delay, false, false, isSpecialAttack, null);
    }

    public int Hit (bool performPower, float delay) {
        return Hit(false, performPower, delay, false, false, false, null);
    }

    public int Hit (bool isByMatch, System.Action onComplete) {
        return Hit(isByMatch, true, 0F, false, false, false, onComplete);
    }

    public int Hit (System.Action onComplete) {
        return Hit(false, true, 0F, false, false, false, onComplete);
    }

    public int Hit (float delay, System.Action onComplete) {
        return Hit(false, true, delay, false, false, false, onComplete);
    }

    public int Hit (bool isByMatch, bool performPower, float delay, bool isSplashHit, bool isShowEffect, bool isSpecialAttack, System.Action onComplete)
    {
        if(null!=piece && true==piece.IsDestroying)
        {
            Debug.Log("This Blk has been already destroying !!!");
            return 0;
        }

        UnstableCount++;

        if (delay > 0F) {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() => { 
                Destroy(isByMatch, performPower, isSplashHit, isShowEffect, isSpecialAttack, () => {
                    if (onComplete != null) onComplete();
                    UnstableCount--;
                });
            });

            //return 0;
            return null==PD ? 0 : this.PD.destroyScore;
        } else {
            return Destroy(isByMatch, performPower, isSplashHit, isShowEffect, isSpecialAttack, () => {
                if (onComplete != null) onComplete();
                UnstableCount--;
            }); 
        }
    }

	public void ChangePiece(float delay, Vector3 effectPos, Vector3 effectSacle, int Color ,System.Action onComplete) 
	{
//		BlockCrash effect = NNPool.GetItem<BlockCrash>("BubbleHit");
//		effect.Play("play",effectPos, effectSacle, Color, false, delay);
		
		if (delay > 0F) 
		{
			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(delay);
			seq.AppendCallback(() => { 
				if (onComplete != null) onComplete();
			});
		}
		else
		{
			if (onComplete != null) onComplete();
		}		
	}
	
    public void RemovePiece () {
        RemovePiece(true, 0F, null);
    }

    public void RemovePiece (bool force) {
        RemovePiece(force, 0F, null);
    }

    public void RemovePiece (float delay) {
        RemovePiece(true, delay, null);
    }

    public void RemovePiece (bool force, float delay, System.Action onComplete) {
        if (IsFilled == false) return;

        if (force == false) {
            Piece.PD.FireOnPieceDestroy(Piece, false, Piece.ColorIndex);
        }

        UnstableCount++;

        Piece.Remove(delay, () => {
            Piece = null;
            UnstableCount--;
            IsNeedDropCheck = true;
            if (onComplete != null) onComplete();
        });
    }

     // [MAKE_DROP_ACC]
     public bool StealPiece () {
        IsNeedDropCheck = false;

        if(IsStealed) return false;
       
        IsStealed = true;

        Board topFromPortal = GetTopFromPortal();
        Board top = GetTopBoard();

        if(null != topFromPortal)
            _processSteal(topFromPortal);

        return _processSteal(top);
    }
       
    bool _processSteal(Board top)
    {
        // stable 한셀. return 하고 위 board에서 땡긴다.
        if (IsFillable == false) {
            if (top != null)    top.StealPiece();
            return (IsStealable && GM.isPassableThroughFence(this, JMF_DIRECTION.DOWN));
        }

        bool scheduledToDrop = false;
        bool isDropFromSide = false;

        #region // 현재 보드 정보를 바탕으로 dropable 여부를 체크한다. -> 불가능 하다면 return 한다.

        #region // 최상단에서 땡겨질수 있는 상단인지 체크.
        // 맨위쪽 보드 인가?
        if (null==top || LEItem.HELPER_TYPE.DROPPER==(LEItem.HELPER_TYPE)_eHelperType)
        {
            bool isSpawnColumn = false;

            // 위에서 새로 생성해서 떨어지도록 되어 있는 열인가.
            if (GM.CurrentLevel.defaultSpawnColumn == null) {
                isSpawnColumn = true;
            } else {
                for (int i = 0; i < GM.CurrentLevel.defaultSpawnColumn.Length; i++) {
                    if (GM.CurrentLevel.defaultSpawnColumn[i] == X) {
                        isSpawnColumn = true;
                        break;
                    }
                }
            }

            // 그렇다.
            if (isSpawnColumn) {
                scheduledToDrop = true;
            }
            // 아니면 더이상 진행하지 않는다.
            else
            {
                return false;
            }
        }
        #endregion

        #region // 상단 블록이 내려올수 있는 것인지 체크.
        else
        {
            // 이 판이 비었지만 위 셀이 아직 drop 하지 않았나.
            if (top.IsStealable && top.IsStable && true==GM.isPassableThroughFence(top, JMF_DIRECTION.DOWN))
            { 
                scheduledToDrop = true;

                // 이 move로 위 판이 unstable해 질 것이므로 더 위에서 또 댕긴다.
                top.StealPiece();
            }
            // 이 판으로 위셀이 drop 중인가.
            else
            {
                // 더 위에서 셀을 땡긴다.
                scheduledToDrop = top.StealPiece();

                // 위 블럭이 dropable 한가.
                if (top.IsDropable)
                { 
                    if (scheduledToDrop) {  // 위쪽에서 땡겨온 셀이 있나.
                        return true;
                    } else {                // 없다면 slide로 땡긴다.
                        var newTop = GetSideBoardToSteal(this);
                        scheduledToDrop = (newTop != null);
                        if(scheduledToDrop) {
                            top = newTop;
                            isDropFromSide = true;
                        }
                    }
                }
                // 위 블럭이 떨어질 수 없다면, slide로 땡긴다.
                else
                {
                    var newTop = GetSideBoardToSteal(this);
                    scheduledToDrop = (newTop != null);
                    if(scheduledToDrop) {
                        top = newTop;
                        isDropFromSide = true;
                    }
                }

                // 다 해도 땡길것이 없다면 반환.
                if (scheduledToDrop == false)
                    return false;
            }
        }
        #endregion

        #endregion

        #region // 실제로 내릴 셀을 찾아 this.Piece에 세팅한다.
        bool spawned = false;

        //if (top == null) {
        if (null==top || LEItem.HELPER_TYPE.DROPPER==(LEItem.HELPER_TYPE)_eHelperType)
        {
            PieceDefinition pd = null;

            for (int i = GM.PieceTypes.Length - 1; i >= 0; i--) {
                pd = GM.PieceTypes[i].GetSpawnableDefinition(PT);

                if (pd != null) {

                    // test code.
                    //int val = Random.Range(1, 3);
                    //int color = 0==(val%2) ? 1 : pd.GetColorIndex();
                    //SpawnPiece(pd, color);
                    //

                    if(pd is AddTimePiece)
                        SpawnPiece(pd, GM.GetRandomColorIndex());
                    else
                        SpawnPiece(pd, pd.GetColorIndex());
                    spawned = true;
                    break;
                }
            }

            if (spawned == false) SpawnPiece(GM.GetPieceType<NormalPiece>(), GM.GetRandomColorIndex());

            spawned     = true;            
        }
        else if (top.PND is CreatorPanel) {
            if (top.PND is CreatorBombPanel) {
                spawned = SpawnTimeBomb(this);
            } else if (top.PND is CreatorSpiralPanel) {
                spawned = SpawnRoundChoco(this);
            } else if (top.PND is CreatorIcecreamPanel) {
                spawned = SpawnIcecream(this);
            } else if (top.PND is CreatorSpiralBombPanel) {
                spawned = SpawnTimeBomb(this);

                if (spawned == false) {
                    spawned = SpawnRoundChoco(this);
                }
            } else if (top.PND is CreatorIcecreamBombPanel) {
                spawned = SpawnIcecream(this);

                if (spawned == false) {
                    spawned = SpawnTimeBomb(this);
                }
            } else if (top.PND is CreatorIcecreamSpiralPanel) {
                spawned = SpawnIcecream(this);

                if (spawned == false) {
                    spawned = SpawnRoundChoco(this);
                }
            } else {
                spawned = SpawnSpecial(this);
            }

            if (spawned == false) SpawnPiece(GM.GetPieceType<NormalPiece>(), GM.GetRandomColorIndex());

            spawned     = true;

            top.Panel[BoardPanel.TYPE.FRONT].GetComponent<Creator>().Spin();
        }
        else if (top.IsPortal && (top.PortalType==PORTAL_TYPE.DOWN || top.PortalType==PORTAL_TYPE.ALL))
        {   
            // note : 이 보드로 끌어올 top 이 포탈 진입구(사라짐) 라면, piece를 하단 이동후 out 시킨다.
            //        => 이 보드는 포탈 출구(나옴)이다.
            if (top.IsFilled) {
                int fallBackTime = top.Piece.FallBackTime;
                if(top.PD is CookieJellyPiece)
                    SpawnPiece(top.PD, top.Piece.CookieJellyType, GM.Size * 0.8f, -0.15F, null!=top.Piece? top.Piece.LifeCover:0, top.Piece.Durability);
                else
                    SpawnPiece(top.PD, top.ColorIndex, GM.Size * 0.8f, -0.15F, null!=top.Piece? top.Piece.LifeCover:0, top.Piece.Durability);

                // portal 처리.
                if(true == JMFUtils.GM.DestroyAwaiter.Contains( top.Piece ))
                {
                    JMFUtils.GM.DestroyAwaiter.Remove( top.Piece );
                    JMFUtils.GM.DestroyAwaiter.Add( Piece );
                    Piece.ShineAndBlink(0.2f);
                }

                Piece.FallBackTime = fallBackTime;
                top.MoveOutPiece();

                spawned     = true;
            }
        }
        else
        {
            if (top.IsStable == false)
                return true;

            Piece = top.Piece;
            top.Piece = null;
        }

        if ((Piece == null) || (Piece.GO == null))
            return true;

        // 새로만드는 녀석을 가속을 연계해야 한다.
        if(true == spawned)
        {            
            if(null!=Bottom && null!=Bottom.Piece)
            {
                //Piece._fDropElTime  = Bottom.Piece._fDropElTime;
               // Piece._dropAccel    = Bottom.Piece._dropAccel;
            }
        }

        #endregion

        // [PASS_EMPTY_BOARD]
        #region => 이동중에 pass할 빈 보드를 얻어 list화 한다.
        Piece._listEmptyBoards.Clear();
        if(null!=top && top.Y-PT.Y>1)
        {
            int iTop            = top.Y-1;
            do
            {
                if(iTop <= PT.Y)
                    break;
                if(GM[X, iTop].PND is EmptyPanel)
                    Piece._listEmptyBoards.Add( GM[X, iTop] );
                --iTop;
            }while(true);
        }
        #endregion
        //

        // 찾은 셀을 최종적으로 drop 시킨다.
        UnstableCount++;
        
        Piece.Drop(BottomUnfilledCount == 0, isDropFromSide, spawned, () => {
            UnstableCount--;
            IsNeedMatchCheck = true;
        });
        
        return scheduledToDrop;
    }

   /* public bool StealPiece_org () {
        IsNeedDropCheck = false;

        if (IsStealed) return false;

        IsStealed = true;

        Board top = GetTopBoard();

        if (IsFillable == false) {  // 이미 차 있다. return 하고 위 board에서 수행.
            if (top != null) top.StealPiece();
            return IsStealable;
        }

        bool scheduledToDrop = false;
        bool isDropFromSide = false;
        
        if (top == null) {          // 맨위쪽 보드 인가?
            bool isSpawnColumn = false;

            if (GM.CurrentLevel.defaultSpawnColumn == null) {
                isSpawnColumn = true;
            } else {
                for (int i = 0; i < GM.CurrentLevel.defaultSpawnColumn.Length; i++) {
                    if (GM.CurrentLevel.defaultSpawnColumn[i] == X) {
                        isSpawnColumn = true;
                        break;
                    }
                }
            }

            if (isSpawnColumn) {
                scheduledToDrop = true;
            } else {
                return false;
            }
        } else {
            if (top.IsStealable && top.IsStable) { // 위쪽에 드롭가능한 블럭이 있는가?
                scheduledToDrop = true;
            } else {
                // 리턴하거나 좌우에서 가저와야함
                scheduledToDrop = top.StealPiece();

                if (top.IsDropable) { // 블럭은 없지만 드롭은 가능한 조건인가?
                    if (scheduledToDrop) { // 위에서 떨어질 예정인가?
                        return true;
                    } else {
                        var newTop = GetSideBoardToSteal(this);
                        scheduledToDrop = (newTop != null);
                        if(scheduledToDrop) {
                            top = newTop;
                            isDropFromSide = true;
                        }
                    }
                } else {
                    var newTop = GetSideBoardToSteal(this);
                    scheduledToDrop = (newTop != null);
                    if(scheduledToDrop) {
                        top = newTop;
                        isDropFromSide = true;
                    }
                }

                if (scheduledToDrop == false)
                    return false;
            }
        }

        bool spawned = false;

        if (top == null) {
            PieceDefinition pd = null;

            for (int i = GM.PieceTypes.Length - 1; i >= 0; i--) {
                pd = GM.PieceTypes[i].GetSpawnableDefinition(PT);

                if (pd != null) {
                    SpawnPiece(pd, pd.GetColorIndex());
                    spawned = true;
                    break;
                }
            }

            if (spawned == false) SpawnPiece(GM.GetPieceType<NormalPiece>(), GM.GetRandomColorIndex());
        } else if (top.PND is CreatorPanel) {
            if (top.PND is CreatorBombPanel) {
                spawned = SpawnTimeBomb(this);
            } else if (top.PND is CreatorSpiralPanel) {
                spawned = SpawnRoundChoco(this);
            } else if (top.PND is CreatorIcecreamPanel) {
                spawned = SpawnIcecream(this);
            } else if (top.PND is CreatorSpiralBombPanel) {
                spawned = SpawnTimeBomb(this);

                if (spawned == false) {
                    spawned = SpawnRoundChoco(this);
                }
            } else if (top.PND is CreatorIcecreamBombPanel) {
                spawned = SpawnIcecream(this);

                if (spawned == false) {
                    spawned = SpawnTimeBomb(this);
                }
            } else if (top.PND is CreatorIcecreamSpiralPanel) {
                spawned = SpawnIcecream(this);

                if (spawned == false) {
                    spawned = SpawnRoundChoco(this);
                }
            } else {
                spawned = SpawnSpecial(this);
            }

            if (spawned == false) SpawnPiece(GM.GetPieceType<NormalPiece>(), GM.GetRandomColorIndex());

            top.Panel[BoardPanel.TYPE.FRONT].GetComponent<Creator>().Spin();
        } else if (top.IsPortal && (top.PortalType==PORTAL_TYPE.DOWN || top.PortalType==PORTAL_TYPE.ALL)) {
            if (top.IsFilled) {
                int fallBackTime = top.Piece.FallBackTime;
                SpawnPiece(top.PD, top.ColorIndex, -0.15F);
                Piece.FallBackTime = fallBackTime;
                top.MoveOutPiece();
            }
        } else {
            if (top.IsStable == false)
                return true;

            Piece = top.Piece;
            top.Piece = null;
        }

        if ((Piece == null) || (Piece.GO == null))
            return true;

        UnstableCount++;

        
      //  Piece.Drop(BottomUnfilledCount == 0, isDropFromSide, () => {
      //      UnstableCount--;
      //      IsNeedMatchCheck = true;
      //  });
        
        return scheduledToDrop;
    }
    */

	public List<Board> GetBoardsInDirection (JMF_DIRECTION direction) {
        List<Board> bds = new List<Board>();
        Board targetBoard = this[direction];
        
        while (targetBoard != null) {
            bds.Add(targetBoard);
            targetBoard = targetBoard[direction];
        }

        return bds;
	}

    public List<Board> GetBoardsFromRadius (float radius) {
        List<Board> boards = new List<Board>();

        foreach (Board bd in GM.Boards) {
            if (bd == this) continue;
			if ((Position - bd.Position).magnitude > radius) continue;
            boards.Add(bd);
        }

        return boards;
    }

    public JMF_DIRECTION GetNeighborDirection (Board neighbor) {
	    if (neighbor == Top) return JMF_DIRECTION.UP;
	    if (neighbor == Bottom) return JMF_DIRECTION.DOWN;
	    if (neighbor == Left) return JMF_DIRECTION.LEFT;
	    if (neighbor == Right) return JMF_DIRECTION.RIGHT;
	    if (neighbor == TopLeft) return JMF_DIRECTION.UPLEFT;
	    if (neighbor == TopRight) return JMF_DIRECTION.UPRIGHT;
	    if (neighbor == BottomLeft) return JMF_DIRECTION.DOWNLEFT;
	    if (neighbor == BottomRight) return JMF_DIRECTION.DOWNRIGHT;

        return JMF_DIRECTION.NONE;
    }

    public List<Board> GetBoardsFromDistance (int distance) {
        return GetBoardsFromArea(distance - 1, distance);
    }

    public List<Board> GetBoardsFromArea (int min, int max)
	{
        Debugger.Assert(min >= 0, "Board.GetBoardsFromDistance : Min can be minus.");
        Debugger.Assert(max > min, "Board.GetBoardsFromDistance : Max must bigger than min.");

        List<Board> boards = new List<Board>();

        for (int y = Y - max; y <= Y + max; y++) {
            for (int x = X - max; x <= X + max; x++) {
                if (((x >= (X - min)) && (x <= (X + min))) && 
                    ((y >= (Y - min)) && (y <= (Y + min)))) continue;
                if (GM[x,y] == this) continue;
                if (GM[x,y] == null) continue;

                boards.Add(GM[x,y]);
            }
        }

        return boards;
    }

    public BoardStateInfo GetStateInfo () {
        return new BoardStateInfo(this);
    }

    int Destroy (bool isByMatch, bool performPower, bool isSplashHit, bool isShowEffect, bool isSpecialAttack, System.Action onComplete) 
	{
        if (IsFilled && Panel.IsDestroyablePiece() && Piece.IsDestroyable() && Piece.IsStable) 
		{
            if (isSplashHit && (Piece.IsSplashHitable() == false)) 
			{
                if (onComplete != null) onComplete();
                return 0;
            }

            //Debug.Log("Destoryed..X : " + X + "  Y : " + Y);

            PieceDefinition pd = PD;
            int colorIndex = Piece.ColorIndex;

            if(1 == Piece.LifeCover)
                IsNeedMatchCheck= true;

            return Piece.Destroy(isByMatch, performPower, false, isShowEffect, () => { 
                //Piece = null;
                IsNeedDropCheck = true;

                // [ROUND_CHOCO] -> Piece.Destory 내부로 이동.
                //if(null!=Piece && Piece.Durability < 0)
                //    pd.FireOnPieceDestroyed(this, colorIndex);

                if (onComplete != null) onComplete();
            });
        }
		else 
		{
            int ret             = Panel.checkSkill();
            switch(ret)
            {
            case 0:             // 대상 아님.
                break;
            case 1:             // 처리 시작.
                Panel.PND.ShowHitEffect( Panel );
                if (onComplete != null) onComplete();
                return 0;
            case 2:             // 처리 중 - 무시.
                if (onComplete != null) onComplete();
                return 0;
            default:
                break;
            }
            //

            Panel.PND.FireOnHit(Panel, isSpecialAttack);

            if (Panel.IsDestroyable())
			{
                if (isSplashHit && (Panel.IsSplashHitable() == false)) 
				{
                    if (onComplete != null) onComplete();
                    return 0;
                }

                return Panel.Destroy(isByMatch, false, isSpecialAttack, (success) => { 
                    if (success) {
                        IsNeedDropCheck = true;
                        IsNeedMatchCheck = true; 
                    }
                    if (onComplete != null) onComplete();
                });
            } 
			else
			{
                if (onComplete != null) onComplete();

                if (isSplashHit == false)
				{
                    if (ShadedDurability >= 0) {
                        ShadedDurability--;
                        GM.IncreaseScore(GM.shadedScore, PT, 6);
                    }
                }

                return 0;
            }
        }
    }

    Board GetTopFromPortal()
    {
        // 이 board가 포탈 출구(나타남)면, 입구를 찾아 반환.
        if (IsPortal)
        {
            if(PortalType==PORTAL_TYPE.UP || PortalType==PORTAL_TYPE.ALL) 
                return GM.GetPortalOfTheOtherSide(this);    // 입구 보드 찾음.
        }
        return null;
    }

    Board GetTopBoard () {

        Board targetBoard = Top;

        while ((targetBoard != null) && (targetBoard.PND is EmptyPanel)) {
            
            // valid 하지만, 셀이 지나갈 수 없는 보드면 그 위에서 찾는다.
            targetBoard = targetBoard.Top;

            // 보드 영역을 넘어서면(상위도달) break.
            if (targetBoard == null) break;
        }

        return targetBoard;
    }

    bool IsColStable (Board bd) {
        for (int y = 0; y < GameManager.HEIGHT; y++) {
            if (GM[bd.X,y].IsStable == false) return false;
        }
        return true;
    }

    static bool tryRightFirst = false;

    Board GetSideBoardToSteal (Board bd) {
        Board bd1 = bd.TopLeft;
        Board bd2 = bd.TopRight;

        if (tryRightFirst) {
            bd1 = bd.TopRight;
            bd2 = bd.TopLeft;
        }

        tryRightFirst = !tryRightFirst;

        if ((bd1 != null) && (bd1.IsStealable) && (bd1.BottomUnfilledCount == 0)) {
            return bd1;
        } else if ((bd2 != null) && (bd2.IsStealable) && (bd2.BottomUnfilledCount == 0)) {
            return bd2;
        } else {
            return null;
        }
    }


    bool SpawnIcecream (Board board) {
        Debugger.Assert(board != null, "Board.SpawnIcecream : Board is null.");

        if (GM.GetPieceType<Potion1Piece>().IsChanceOfSpawn()) {
            board.SpawnPiece(GM.GetPieceType<Potion1Piece>(), 0, 0F);
            return true;
        } else if (GM.GetPieceType<Potion2Piece>().IsChanceOfSpawn()) {
            board.SpawnPiece(GM.GetPieceType<Potion2Piece>(), 0, 0F);
            return true;
        } else if (GM.GetPieceType<Potion3Piece>().IsChanceOfSpawn()) {
            board.SpawnPiece(GM.GetPieceType<Potion3Piece>(), 0, 0F);
            return true;
        }

        return false;
    }

    bool SpawnTimeBomb (Board board) {
        Debugger.Assert(board != null, "Board.SpawnTimeBomb : Board is null.");

        if (GM.GetPieceType<TimeBombPiece>().IsChanceOfSpawn()) {
            board.SpawnPiece(GM.GetPieceType<TimeBombPiece>(), GM.GetRandomColorIndex(), 0F);
            return true;
        }

        return false;
    }

    // [ROUND_CHOCO]
    bool SpawnRoundChoco (Board board) {
        Debugger.Assert(board != null, "Board.SpawnRoundChoco : Board is null.");

        if (GM.GetPieceType<RoundChocoPiece>().IsChanceOfSpawn())
		{
            board.SpawnPiece(GM.GetPieceType<RoundChocoPiece>(), 0, 0F);
            return true;
        }

        return false;
    }

    bool SpawnSpecial (Board bd) {
        for (int i = GM.PieceTypes.Length - 1; i >= 0; i--) {
            PieceDefinition pd = GM.PieceTypes[i].GetSpawnableDefinition(bd.PT);

            if (pd != null) {
                bd.SpawnPiece(pd, pd.GetColorIndex());
                return true;
            }
        }

        return false;
    }

    void SpawnPiece (PieceDefinition pd, int skinNum) {
        SpawnPiece(pd, skinNum, GM.Size * 0.8f, 0F);
    }

    void SpawnPiece (PieceDefinition pd, int skinNum, float depth) {
        SpawnPiece(pd, skinNum, GM.Size * 0.8f, depth);
    }

    void SpawnPiece (PieceDefinition pd, int skinNum, float spawnHeight, float depth, int lifeCover=0, int forceDurability=-1)
    {
        // [ROUND_CHOCO], [GREEN_BUBBLE]
        int durability          = -1;
        if(pd is RoundChocoPiece)    
			durability  = GM.CurrentLevel.lifeNewRoundChoco - 1;
        else if(pd is GreenBubblePiece)
			durability  = GM.CurrentLevel.lifeNewGreenBubble - 1;
		//
        if(forceDurability >= 0)
            durability          = forceDurability;

        if (Piece == null)
            Piece = new GamePiece(this, pd, skinNum, durability, lifeCover);
        else 
            Piece.Reset(pd, skinNum, durability, lifeCover);
    	
		Vector3 spawnPos = Position;
		spawnPos.y += spawnHeight;
		spawnPos.z = Piece.Position.z;
		//spawnPos.z += depth;
        Piece.Position = spawnPos;
		
		IsNeedMatchCheck = true;
    }

    void UpdateShaded () 
	{
		if(shadedDurability < 0)
		{
            if(null != _shaded)
            {
			    NNPool.Abandon(_shaded.gameObject);
			    _shaded = null;
            }
            if(null != _sprShaded)
            {
                NNPool.Abandon(_sprShaded.gameObject);
			    _sprShaded = null;
            }
            if((int)LEItem.SHADE_TYPE.MUD_COVER == eShadeType)
                GM._countMudCoverDestroyed++;
			return;
		}
        
        if (shadedDurability >= 0) 
		{
            // [ADJUST SCALE]
            // [JAM_SHADE]
			string fileName = null;
            bool blockZFight    = false;
            float fAddScale     = 1.0f;
            switch(eShadeType)
            {
			case (int)LEItem.SHADE_TYPE.CURSE:
                fAddScale       = 1.3f;
				fileName = "obstacle_bgstone_0" + (shadedDurability + 1);
                blockZFight = true;
                break;
            case (int)LEItem.SHADE_TYPE.JAM:
                fAddScale       = 1.3f;
				fileName = "obstacle_jam";
                break;
		    case (int)LEItem.SHADE_TYPE.NET:
                fAddScale       = 1.3f;
				fileName = "Panel_netL" + (shadedDurability + 1);
                break;
            case (int)LEItem.SHADE_TYPE.MUD_COVER:
            {
                if(null!=_sprShaded)    NNPool.Abandon(_sprShaded.gameObject);
                _sprShaded              = NNPool.GetItem("SpriteImage", GM.transform).GetComponent<SpriteImage>();
                _sprShaded.GetComponent<SpriteRenderer>().sprite       = _sprShaded.getSpriteByName("thorn_mud1");

                float localPosZ = 4.0f;
                if(true == blockZFight)
                    localPosZ   += ( ((float)X)*0.1f + ((float)Y)*0.1f );
                _sprShaded.transform.localPosition  = new Vector3(LocalPosition.x, LocalPosition.y, localPosZ);
                _sprShaded.transform.localScale     = Vector3.one * 0.85f;
                return;
            }
            default:
                Debug.Log("Shade Type Error !!!");
                return;
            }
			
            float fScale        = null==_pnlRiver ? 1.0f : _SCALE_SHADE_ON_RIVER;
            fScale *= fAddScale;
			if(_shaded == null)
			{
				_shaded = NNPool.GetItem<SpriteRenderer>("GainedMissionItem",GM.transform);
				JMFUtils.SpineObjectAutoScalePadded(_shaded.gameObject);
                _shaded.sortingOrder   = 0;
                // z-fighting을 방지키위해 z 값을 차등 setting 한다.
                float localPosZ = 4.0f;
                if(true == blockZFight)
                    localPosZ   += ( ((float)X)*0.1f + ((float)Y)*0.1f );
                _shaded.transform.localPosition  = new Vector3(LocalPosition.x, LocalPosition.y, localPosZ);
                _shaded.transform.localScale     = Vector3.one * fScale;
				//shaded.transform.localPosition += new Vector3(0F, 0F, 4F);// * GM.Size * shaded.transform.localScale.z);
			}
			
			//int slotIndex = shaded.Skeleton.FindSlotIndex("RegionList");
			//Slot _slot = shaded.Skeleton.FindSlot("RegionList");
			//_slot.Attachment = shaded.Skeleton.GetAttachment(slotIndex,fileName);	
            _shaded.sprite  = _shaded.GetComponent<SpriteImage>().getSpriteByName( fileName );
        }
    }

    void MoveOutPiece () {
        Debugger.Assert(IsFilled, "Board.MoveOutPiece : Board is not filled.");
        Debugger.Assert(IsPortal && (PortalType==PORTAL_TYPE.DOWN || PortalType==PORTAL_TYPE.ALL), "Board.MoveOutPiece : Board is not portal.");

        GamePiece p = Piece;
        Piece = null;

        p.GO.transform.Translate(Vector3.back * 0.15F);

        Vector3 targetPos = Position;
        targetPos.y -= GM.Size;
        targetPos.z = p.GO.transform.position.z;

        p.velocity += Time.deltaTime * GM.gravity;
        p.velocity = Mathf.Min(GM.maxVelocity, p.velocity);

        Tween dropTween = p.GO.transform.DOMove(targetPos, p.velocity).SetSpeedBased();
        dropTween.SetEase(Ease.Linear);
        dropTween.OnComplete(() => { p.Remove(); });
    }

    public override string ToString () {
        return string.Format("BD[Point:{0}, PND:{1}, PD:{2}]", PT.ToString(),
            (PND == null) ? "Unknown" : PND.GetType().FullName,
            (IsFilled) ? PD.GetType().FullName : "EMPTY");
    }

    #region [CHOCO_BAR], [NET_SHADE]    
    public int _indexBar        = -1;
    public LEItem.CHOCO_BAR     _eBarType   = LEItem.CHOCO_BAR.NONE;
    public string               _strChocoBarName    = "";
    public GameObject           ChocoBar
    { 
        get { return _objChocoBar; }
        set { _objChocoBar = value; }
    }
	public void initChocoBar(int indexBar, LEItem.CHOCO_BAR eType)
    {
        // note : only 0 indexed boards can have it's bar.	
		
        if(null != _objChocoBar)
            NNPool.Abandon( _objChocoBar );

        _indexBar               = indexBar;
        _eBarType               = eType;
        _objChocoBar            = null;
		
		if (0 != _indexBar || eType == LEItem.CHOCO_BAR.NONE) return;
		
        float fX                = .0f;
        float fY                = .0f;
        string subStr           = "";
		
        switch(eType)
        {
        case LEItem.CHOCO_BAR._1X1: subStr  = "1x1";    break;
        case LEItem.CHOCO_BAR._2X2: subStr  = "2x2";    fX = 0.5f*GM.Size; fY = fX;    break;
        case LEItem.CHOCO_BAR._1X2: subStr  = "1x2";    fY = 0.5f*GM.Size; break;
        case LEItem.CHOCO_BAR._1X3: subStr  = "1x3";    fY=GM.Size; break;
        case LEItem.CHOCO_BAR._2X3: subStr  = "2x3";    fX = 0.5f*GM.Size; fY = GM.Size;  break;
        case LEItem.CHOCO_BAR._2X4: subStr  = "2x4";    fX = 0.5f*GM.Size; fY = 1.5f*GM.Size;  break;
        case LEItem.CHOCO_BAR._2X1: subStr  = "2x1";    fX = 0.5f * GM.Size;  break;
        case LEItem.CHOCO_BAR._3X1: subStr  = "3x1";    fX = GM.Size;  break;
        case LEItem.CHOCO_BAR._3X2: subStr  = "3x2";    fX = GM.Size; fY=0.5f*GM.Size; break;
		case LEItem.CHOCO_BAR._3X3: subStr  = "3x3";    fX = 1.0f*GM.Size; fY = fX;  break;  // scale = 0.5f; fX = 0.5f*GM.Size; fY = fX;    break;
        case LEItem.CHOCO_BAR._4X2: subStr  = "4x2";    fX= 1.5f*GM.Size;  fY=0.5f*GM.Size; break;
        }

        // == Debug.Log("=== bar created...");
		
		_strChocoBarName        = string.Format("Panel_chocobar{0}", subStr);
		
		//SkeletonRenderer SR = NNPool.GetItem<SkeletonRenderer>("GainedMissionItem", GM.transform);
        //SR.GetComponent<MeshRenderer>().sortingOrder    = 0;
		//int slotIndex = SR.Skeleton.FindSlotIndex("RegionList");
		//Slot _slot = SR.Skeleton.FindSlot("RegionList");
		//_slot.Attachment = SR.Skeleton.GetAttachment(slotIndex,_strChocoBarName);
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("GainedMissionItem", GM.transform);
        SR.sortingOrder         = 0;
        SR.sprite               = SR.GetComponent<SpriteImage>().getSpriteByName( _strChocoBarName );
		
		_objChocoBar = SR.gameObject;
		JMFUtils.SpineObjectAutoScalePadded(_objChocoBar);
		
		Vector3 pos = LocalPosition;
        pos.x += fX;            pos.y += fY;
		pos.z = (PT.Y + (GameManager.WIDTH -1 - PT.X)) * 0.01f;
		pos.z += 5.0f;
		SR.transform.localPosition = pos;// * GM.Size * _objChocoBar.transform.localScale.z);
    }
    #endregion

    public int _changeColorIndex{ get; private set; } 
    
    public void initColorChanger(int changerColorIndex)
    {
        _changeColorIndex       = changerColorIndex;

        if(changerColorIndex < 0)
            return;
		
	/*	string strKey           = "Color_changer_";
        switch(changerColorIndex)
        {
			case (int)LEItem.COLOR.RED:     strKey += "red";    break;
			case (int)LEItem.COLOR.YELLOW:  strKey += "yellow";    break;
			case (int)LEItem.COLOR.GREEN:   strKey += "Green";    break;
			case (int)LEItem.COLOR.BLUE:    strKey += "blue";    break;
			case (int)LEItem.COLOR.PURPLE:  strKey += "pink";    break;
			case (int)LEItem.COLOR.ORANGE:  strKey += "orange";    break;
			case (int)LEItem.COLOR.SKYBULE: strKey += "skyBlue";   break;
			case (int)LEItem.COLOR.VIOLET:  strKey += "violet";    break;
            case (int)LEItem.COLOR.RANDOM:  strKey += "Rainbow";    break;
        	default:    return;
        }

        Vector3 pos             = LocalPosition;
		pos.z                   = (PT.Y + (GameManager.WIDTH -1 - PT.X)) * 0.01f;
		pos.z += 4.0f;

//#if UNITY_EDITOR              => For Test only !!!
//        // note : 정식 연출이 나오기까진 여기에 부착.
//        if(null!=_sprShaded)    NNPool.Abandon(_sprShaded.gameObject);
//        _sprShaded              = NNPool.GetItem("River_Waffle", GM.transform).GetComponent<SpriteRenderer>();
//        string strPath          = "Assets/UI/block_new/Panels/" + strKey + ".png";
//        _sprShaded.sprite       = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(strPath) as Sprite;
//        _sprShaded.transform.localPosition = pos;
//#else
        if(null!=shaded && null!=shaded.gameObject)
            NNPool.Abandon( shaded.gameObject );
		
		SkeletonAnimation SR    = NNPool.GetItem<SkeletonAnimation>("Color_changer", GM.transform);
        SR.Initialize(true);
        SR.GetComponent<MeshRenderer>().sortingOrder    = 0;
		int slotIndex           = SR.Skeleton.FindSlotIndex("Color_changer");
		Slot _slot              = SR.Skeleton.FindSlot("Color_changer");
		_slot.Attachment        = SR.Skeleton.GetAttachment(slotIndex, strKey);

        slotIndex               = SR.Skeleton.FindSlotIndex("Color_changer_cyclone");
		_slot                   = SR.Skeleton.FindSlot("Color_changer_cyclone");
		_slot.Attachment        = SR.Skeleton.GetAttachment(slotIndex, strKey+"_cyclone");

		JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
		SR.transform.localPosition = pos;
        SR.AnimationState.SetAnimation(0, "idle", true);

        shaded                  = SR;
//#endif

        eShadeType              = (int)LEItem.SHADE_TYPE.NONE;*/
    }

    // color changer 연출.
    public void AnimateColorChanger()
    {
     /*   if(null == shaded)      return;
        if(_changeColorIndex<0) return;

        SkeletonAnimation ani   = (shaded as SkeletonAnimation);
        if(null == ani)         return;

        SkeletonAnimation SR    = NNPool.GetItem<SkeletonAnimation>("Color_changer", GM.transform);
        SR.GetComponent<MeshRenderer>().sortingOrder    = 10;
		JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
        Director.Instance.showMeshNextFrame(SR.GetComponent<MeshRenderer>());

        Vector3 pos             = LocalPosition;
		pos.z                   = (PT.Y + (GameManager.WIDTH -1 - PT.X)) * 0.01f;
		pos.z += 4.0f;
        SR.transform.localPosition = pos;
        SR.AnimationState.TimeScale = 2.0f;
        SR.AnimationState.SetAnimation(0, "change", false);
        DOVirtual.DelayedCall(1.0f, () => NNPool.Abandon( SR.gameObject ) );

        //ani.AnimationState.SetAnimation(0, "change", false);
        //ani.AnimationState.AddAnimation(0, "idle", true, .0f);*/
    }

#region // CONVEYOR RELATIVES.
    // return : 0 - choco-bar를 제외한 모든것이 제대로 create 됨.
    //          1 - choco-bar도 조건을 만족하여 copy됨.
    public int createBoardData4Conveyor(Board from, Vector2 vInitPos)
    {
        GM._isRemovedByConveyor = true;

        int nRet                = 0;

        // piece.
        if(null != Piece)       Piece.Remove();
        if(null!=from.piece && null!=from.PD)
        {
            if(from.piece.PD is CookieJellyPiece)
                this.ResetPiece(from.piece.PD, from.Piece.CookieJellyType, from.piece.Durability, from.Piece.LifeCover);
            else
                this.ResetPiece(from.piece.PD, from.ColorIndex, from.piece.Durability, from.Piece.LifeCover);

            Piece.GO.transform.localPosition    = new Vector3(vInitPos.x, vInitPos.y, from.piece.GO.transform.localPosition.z);
                //from.piece.GO.transform.localPosition;

            // deep copy를 하므로, list를 갱신해 줘야 한다.
            if(GM.DestroyAwaiter.Contains(from.piece))
            {
                GM.DestroyAwaiter.Remove( from.piece );
                GM.DestroyAwaiter.Add( piece );
                piece.ShineAndBlink(0.2f);
            }
        }

        // pannel.
        if(null != from.Panel)
        {
            if(null == Panel)   Panel   = new BoardPanel(this, from.Panel.PND, from.Panel.Durability, from.Panel.info, from.Panel.getBoardColor());
            else
            {
                Panel.Remove();
                Panel.Reset(from.Panel.PND, from.Panel.Durability, from.Panel.info, from.Panel.getBoardColor());
            }
            Dictionary<BoardPanel.TYPE, Panel> dicOldPanel = from.Panel.PanelDict();
            Dictionary<BoardPanel.TYPE, Panel> dicPanel = panel.PanelDict();
            Debug.Assert(dicOldPanel.Count == dicPanel.Count);
            foreach (BoardPanel.TYPE type in dicPanel.Keys) 
		    {
                if(null!=dicPanel[type] && null!=dicOldPanel[type])
                {
                    Transform trOld = dicOldPanel[type].gameObject.transform;
                    dicPanel[ type ].gameObject.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, trOld.localPosition.z);// dicOldPanel[type].gameObject.transform.localPosition;
                }
            }
            // for WaffleCookerPanel
            Panel.setIsDestroyablePanel( from.Panel.IsDestroyable() );
            from.Panel.setIsDestroyablePanel( true );
        }
        else
        {
            Debugger.Assert(false, "Panel Can't be NULL !");
        }

        // shade.
        if(from.eShadeType!=(int)LEItem.SHADE_TYPE.NONE && from.ShadedDurability>=0)
        {
            eShadeType          = from.eShadeType;
            ShadedDurability    = from.ShadedDurability;    // build the view in here !!!
            if(null != _shaded) _shaded.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, from._shaded.transform.localPosition.z);
            if(null != _sprShaded) 
                _sprShaded.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, from._sprShaded.transform.localPosition.z);
        }

        // color-changer.
        initColorChanger( from._changeColorIndex );
        if(null!= _shaded)      _shaded.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, from._shaded.transform.localPosition.z);

        // choco-bar.
        if(from._eBarType==LEItem.CHOCO_BAR._1X1 && (_eBarType==LEItem.CHOCO_BAR.NONE || _eBarType==LEItem.CHOCO_BAR._1X1))
        {
            this.initChocoBar(from._indexBar, from._eBarType);
            _objChocoBar.transform.localPosition    = new Vector3(vInitPos.x, vInitPos.y, from._objChocoBar.transform.localPosition.z);
            nRet                = 1;
        }

        if(AI_SIDE.NONE!=from.AiSide && null!=from._sprAiTaken)
        {
            this.AiSide         = from.AiSide;
            _sprAiTaken.transform.localPosition   = new Vector3(vInitPos.x, vInitPos.y, from._sprAiTaken.transform.localPosition.z);
        }

        // treasure-goal. 
        if(true == from._isTreasureGoal)
        {
            this.initTreasureGoal();
            _shaded.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, from._shaded.transform.localPosition.z);
        }
        else if(true==_isTreasureGoal && false==from._isTreasureGoal)
            this.removeTreasureGoal();

        _initOverRiverWaffle(null!=from._objOverRiver);
        if(null != from._objOverRiver)
            _objOverRiver.transform.localPosition = new Vector3(vInitPos.x, vInitPos.y, from._objOverRiver.transform.localPosition.z);
        
        GM._isRemovedByConveyor = false;
        return nRet;
    }

    public void removeBoardData4Conveyor(bool bRemoveChcobar=true)
    {
        GM._isRemovedByConveyor = true;

        // piece.
        if(null != Piece)       Piece.Remove();
        Piece                   = null;

        // pannel.
        if(null != Panel)       {  Panel.Remove(); Panel.setIsDestroyablePanel(true); }
        Panel                   = null;
        
        // shade.
        eShadeType              = (int)LEItem.SHADE_TYPE.NONE;
        _changeColorIndex       = -1;
        shadedDurability        = -1;
        if(null != _shaded)     NNPool.Abandon(_shaded.gameObject);
        _shaded                 = null;
        if(null != _sprShaded)  NNPool.Abandon(_sprShaded.gameObject);
        _sprShaded              = null;

        // choco-bar.
        if(bRemoveChcobar)      this.initChocoBar(-1, LEItem.CHOCO_BAR.NONE);

        // over river waffle.
        if(null!=_objOverRiver) NNPool.Abandon(_objOverRiver);
        _objOverRiver           = null;

        if(AI_SIDE.NONE!=AiSide && null!=_sprAiTaken)
            AiSide              = AI_SIDE.NONE;

        // treasure goal.
        removeTreasureGoal();

        GM._isRemovedByConveyor = false;
    }

    public void move4Conveyor(Vector3 vTo, bool bRemove, System.Action<Board> onComplete=null)
    {
        float fDuration         = 0.8f; // 0.5f;
        Ease eType              = Ease.OutQuad;//.InOutBack;//.InQuint;

        if(null != piece)
        {
            piece.killDropAction();
            piece.GO.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
            piece.GO.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
        }

        if(null != panel)
        {
            Dictionary<BoardPanel.TYPE, Panel> dicPanel = panel.PanelDict();
            if(dicPanel.Count > 0)
            {
                foreach (BoardPanel.TYPE type in dicPanel.Keys) 
		        {
                    if(null != dicPanel[type])
                    {
                        dicPanel[ type ].gameObject.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
                        dicPanel[ type ].gameObject.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
                    }
                }
            }
        }
        
        if(null != _shaded)
        {
            _shaded.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
            _shaded.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
        }

        if(null != _sprShaded)
        {
            _sprShaded.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
            _sprShaded.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
        }

        if(_eBarType==LEItem.CHOCO_BAR._1X1 && null!=_objChocoBar)
        {
            _objChocoBar.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
            _objChocoBar.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
        }
        
        if(null != _objOverRiver)
        {
            _objOverRiver.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
            _objOverRiver.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
        }

        if(AI_SIDE.NONE != AiSide)
        {
            _sprAiTaken.transform.DOMoveX(vTo.x, fDuration).SetEase(eType);
            _sprAiTaken.transform.DOMoveY(vTo.y, fDuration).SetEase(eType);
        }

        DOVirtual.DelayedCall(fDuration+0.1f, () =>
        {
            if(bRemove)
            {
                GM._isRemovedByConveyor = true;

                if(null!=piece)         piece.Remove();
                if(null!=panel)         panel.Remove();
                if(null!=_shaded)       NNPool.Abandon(_shaded.gameObject);
                if(null!=_sprShaded)    NNPool.Abandon(_sprShaded.gameObject);
                if(null!=_objChocoBar)  NNPool.Abandon(_objChocoBar);
                if(null!=_objOverRiver) NNPool.Abandon(_objOverRiver);
                if(null!=_sprAiTaken)   NNPool.Abandon(_sprAiTaken.gameObject);

                piece                   = null;
                panel                   = null;

                _shaded                 = null;
                _objChocoBar            = null;
                _objOverRiver           = null;
                _sprAiTaken             = null;
                _sprShaded              = null;

                GM._isRemovedByConveyor = false;
            }

            if (onComplete != null) onComplete(this);
        });
    }

#endregion

    public void setMatchingList(List<Board> listMatches, bool resetList=true)
    {
        if(resetList)           _listMatchings.Clear();
        for(int q = 0; q < listMatches.Count; ++q)
            _listMatchings.Add( listMatches[q] );
    }
    public void setMatchingList(Board target, bool resetList=true)
    {
        if(resetList)           _listMatchings.Clear();
        _listMatchings.Add( target );
    }
    public void clearMatchingList()
    {
        _listMatchings.Clear();
    }

    // 레벨은 clear하는데 직접적인 도움이 되는 보드인가.
    // 타게팅 block 목표 측정.
    public bool isLevelMissionTarget()
    {
        bool ret            = false;

        // block check.
        if(null!=Piece && null!=PD)
            ret             = PD.isLevelMissionTarget(Piece);
        if(ret)             return ret;

        // pancel check.
        if(null!=Panel && null!=PND)
            ret             = PND.isLevelMissionTarget();
        if(ret)             return ret;

        // shade check.
        if(eShadeType>0 && ShadedDurability>=0)
            return true;

        // else  !!!
        return false;
    }
}
