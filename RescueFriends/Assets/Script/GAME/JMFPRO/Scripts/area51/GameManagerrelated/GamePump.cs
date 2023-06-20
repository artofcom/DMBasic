using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE;
using NOVNINE.Diagnostics;

public enum PUMP_STATE { 
    IDLE, WAIT, SHUFFLE, DROP, MATCH, CAKE, TIMEBOMB, COLOR_CHANGER, SNOWMAN, DOUGH, MUD_COVER, CONVEYOR, CHAMELEON, PENGUIN_JUMP, 
    PENGUIN_SPAWN, YETI, LAUNCHER_ICE, LAUNCHER_STONE, LAUNCHER_WEB, BOSS, CLEAR, FAIL, AI_PROC, DESTROY_AWAITER,
};

public abstract class GamePump {
    protected GameManager GM { get { return JMFUtils.GM; } }

    public abstract PUMP_STATE State { get; }
    public abstract GamePump Next ();

    public virtual void ResetPump () {}

    protected GamePump GetNextPhaseOfEnd () {

        // if(GM.PlayMoved)    GM.PlayMoved = false;

//#if BOMBx2
        /* Moved to "DestroyAwaiterPump"
        if (GM.DestroyAwaiter.Count > 0) {
            List<GamePiece> gps = new List<GamePiece>(GM.DestroyAwaiter);
            GM.DestroyAwaiter.Clear();

            foreach (GamePiece gp in gps) {
                if ((gp.PD == null) || (gp.GO == null)) continue;
                gp.Destroy(false, true, true, false);
            }

			GM.ShowClippingBorad(true);
            return GM.dropPump;
        }*/
//#endif

        GM.PlayMoved            = false;
        GM.DestoryWaiterBursted = false;

        if (GM.CollectTreasure()) 
		{
			GM.ShowClippingBorad(true);
            return GM.dropPump;
        }
		else
		{
            if (GM.State == JMF_GAMESTATE.PLAY) {
                //Debug.Log("FireOnBoardStable");
                JMFRelay.FireOnBoardStable();
                JMFRelay.FireOnFinishCombo(GM.ComboCount - 1);
                GM.ComboCount = 1;
            }

            if (GM.IsClearedLevel(GM.CurrentLevel)) {

                switch(GM.State)
                {
                case JMF_GAMESTATE.PLAY:
                {                    
                    return GM.clearPump;
                }
                case JMF_GAMESTATE.BONUS:
                    GM.ShowClippingBorad(true);
                    break;
                default:
                    break;
                }
                return GM.idlePump;

            } else if (GM.IsFailLevel()) {

                if (GM.State == JMF_GAMESTATE.PLAY)
                {
                  //  GM.charAni_play(SPINE_CHAR.MAIN_GIRL, ANI_STATE.FAILED);
                  ///  GM.charAni_play(SPINE_CHAR.DOG, ANI_STATE.FAILED);
                  //  if(true == GM.isAIFightMode)
                   //     GM.charAni_play(SPINE_CHAR.AI_GIRL, ANI_STATE.FAILED);
                }

                if (GM.CurrentLevel.isBossGame && GM.isBossDeadEffect)
                {
                    return GM.idlePump;
                } else {
                    return GM.failPump;
                }
            } else {
                return GM.idlePump;
            }
        }
    }
}

public class IdlePump : GamePump {
    float idleTime = 0F;
	bool bIdle = false;
	bool bFirst = true;
    public override PUMP_STATE State { get { return PUMP_STATE.IDLE; } }


    const float                 TIME_TRANSIT_AI_MODE    = 0.2f;
    float _fAiTransitTime       = .0f;
    bool _isAiModeCalled        = false;    // AI Mode play 진행 중인가.
    bool _resSuggestPiece       = false;

	public override void ResetPump () 
	{ 
		idleTime = GM.idleTimer;
		bIdle = false;
		bFirst = true;
	}
	
    public override GamePump Next ()
	{
        if (GM.State == JMF_GAMESTATE.OVER) return this;
        if (GM.IsAllBoardStable == false) 
			return this;

        // force clear !!!.
        if(GM.isLevelClearByBuff)
        {
            GM.isLevelClearByBuff   = false;
            return GetNextPhaseOfEnd();
        }

        // item shuffle.
        if(true == GM.isShuffleItemTriggered)
        {
            GM.isShuffleItemTriggered = false;
            ResetPump();
			return GM.shufflePump;
        }

        GM.IsStable = true;

        bool swapping = false;
        bool needDrop = false;
        bool needMatch = false;

        foreach (Board bd in GM.Boards) 
		{
            if (bd.State == Board.STATE.SWAP) 
                swapping = true;
            
            if (bd.IsNeedDropCheck)
                needDrop = true;
            
            if (bd.IsNeedMatchCheck)
                needMatch = true;

            // reset board when idle.
            bd.clearMatchingList();
            bd.SkipTarget       = null;
        }

        if (swapping || needDrop || needMatch) 
		{
            GM.IsStable = false;
			idleTime = GM.idleTimer;
        }

        if (swapping) return this;
		
        if (needDrop)
		{
			ResetPump();
			GM.ShowClippingBorad(true);
			return GM.dropPump;
		}
		
        if (needMatch)
		{
			ResetPump();
			return GM.matchPump;
		}

        GM.ComboCount = 1;

#if !NN_DEPLOY
        if (GM.State == JMF_GAMESTATE.PLAY) 
		{
            if (GM.solver != null) GM.solver = GM.solver.Solve();
        }
#endif
        if (GM.State != JMF_GAMESTATE.PLAY) return this;
		
		if(bFirst)
		{
			_resSuggestPiece    = GM.FindSuggestedPieces();
			// GM.FindSuggestedPieces();

			bFirst = false;
            _isAiModeCalled     = false;
            _fAiTransitTime     = .0f;
            // Debug.Log("=============== 1st call for idle pump !!! ================");
		}
        
        //string strTutorialId    = TutorialOverlayHandler.getActiveTutorialId();
		if(false == _resSuggestPiece)// && null==strTutorialId)   // => old code.... GM.GetsuggestedPieces().Count < 3 )
		// if(GM.GetsuggestedPieces().Count < 2 )
		{
			ResetPump();
			return GM.shufflePump;
		}
		
		idleTime -= Time.deltaTime * Time.timeScale;
		
		if(idleTime <= 0 )
		{
			if(bIdle)
			{
				idleTime += GM.IdlePiece() + GM.idleTimer;
				bIdle = false;
			}
			else
			{
				idleTime = GM.idleLimit;
				bIdle = true;

				if(GM.SuggestPiece() <= 0.0f)
				{
					bFirst = true;
                    //if(null == strTutorialId)
                    return GM.shufflePump;
                    //else return this;
				}
			}
		}
		
		// main idle flow.
        string strTutorialId    = TutorialOverlayHandler.getActiveTutorialId();
        if(GM.isAIFightMode) 
        {
            if((GM.itemUsing || GM.bStartAiFight) && false==_isAiModeCalled)
            {
                if(null != strTutorialId)
                    return this;

                return _processAiTurn();
            }
        }
        else
        {
            // Mostly game loop goes here !!!
            //
            GM.itemUsing        = false;
            //
        }
        //

        return this;
    }

    // [AI_MISSION] - change player.
    GamePump _processAiTurn()
    {
        _fAiTransitTime += Time.deltaTime;
        if(_fAiTransitTime < TIME_TRANSIT_AI_MODE)
            return this;

        // do not call this function any futher.
        _isAiModeCalled         = true;
            
        // turn을 변화 시키지 않고 1회 더 하게 해 줌.
        if(GM.isRainbowCreated || GM.itemUsing)
        {
            if(GM.itemUsing)    // AI가 item을 사용했을리는 없다.
            {
                GM.isCurPlayerAI= false;
                GM.itemUsing    = false;
            }

            GM.fire_AiTurn(!GM.isCurPlayerAI, GM.isRainbowCreated);

            GM.isRainbowCreated = false;
            Debug.Log("=== Bonus 1 more Turn !!! ");

            if (GM.isCurPlayerAI)   return GM.aiPump;
            else                    return this;
        }
        
        // swap turn !!!
        GM.isCurPlayerAI        = //true;//  - only for test !!!
                                  !GM.isCurPlayerAI;

        GM.fire_AiTurn(!GM.isCurPlayerAI, GM.isRainbowCreated);

        if(GM.isCurPlayerAI)
        {
            Debug.Log("!! Ai Turn !!!");
            return GM.aiPump;
        }
        else
            Debug.Log("!! My Turn !!!");
        
        return GM.idlePump;
    }

}

public class WaitStablePump : GamePump {
    float elapsedTime = 0F;

    public float MinWaitTime { get; set; }

    public override PUMP_STATE State { get { return PUMP_STATE.WAIT; } }

    public override GamePump Next () { 
        elapsedTime += (Time.deltaTime*Time.timeScale);

        if (elapsedTime <= MinWaitTime) return this;

        foreach (Board bd in GM.Boards)
		{
            if (bd.State != Board.STATE.STABLE)
                return this;
        }    

        elapsedTime = 0F;

		return GM.idlePump;
    }    

    public override void ResetPump () { 
        elapsedTime = 0F;
    }    
}

public class ShufflePump : GamePump {
    bool shuffled = false;
	float elapsedTime = 0F;
	
    public override PUMP_STATE State { get { return PUMP_STATE.SHUFFLE; } }

    public override GamePump Next () 
	{	
		if(!shuffled)
		{
			elapsedTime = GM.ShufflingPiece();
			shuffled = true;
		}
		else
			elapsedTime -= Time.deltaTime * Time.timeScale;
		
		if(elapsedTime <= 0)
		{
			shuffled = false;
			return GM.idlePump;
		}
		
		return this;
    }

    public override void ResetPump () {
        shuffled = false;
    }
}

public class DropPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.DROP; } }

    // [RESET_PHASE_TIMING] : drop pump가 적정 delay 이후에 시작되도록 한다. 
    float _fElTime4EnterDelay   = .0f;
    int tempCounter             = 0;
    
    public override GamePump Next () {

        // Debug.Log("[PUMP] ==== Drop Pump");

        _fElTime4EnterDelay += (Time.deltaTime*Time.timeScale);
        if(_fElTime4EnterDelay < GM.dropPumpDelay)
            return this;

        // note : special piece making을 하는 동안 piece의 상태가 바뀌면 안된다.
        //        하지만, flag 하나만으로는 부족하다. 
        //        makingSpecialPiece 를 점진적으로 mCntMakingSpecialPiece로 교체한다.
        //        by wayne. [17.07.20]
        if (GM.makingSpecialPiece)          return this;
        if(0 < GM.mCntMakingSpecialPiece)   return this;

        // 최하단 행에서 빈셀을 체크하여 위에서 당겨 채운다.
        for (int x = 0; x < GameManager.WIDTH; x++) {
            GM[x,0].StealPiece();
        }
        
        // 보드 전체에서 portal_type_out을 체크하여 처리한다.
        foreach (Board bd in GM.Boards) {
            if (bd.PortalType == PORTAL_TYPE.UP) {
                if (bd.Top != null) bd.Top.StealPiece();
            }
        }

        // ?
        foreach (Board bd in GM.Boards) {
            bd.IsStealed = false;
        }

        if (GM.IsAllBoardStable == false)
        {
            bool goToNextPump   = false;
            GM.triggerPumpErrorCheck(true);     // pump error check 시작.
            if(2 == GM.getPumpCheckState())     // 끝났고, 
            {
                if(GM._hasPumpError)            // 오류가 있으면
                {
                    Debug.Log("=== pump state error at Drop pump !!!");
                    GM.resetPumpCheck();        // 보드 스테이트를 초기화 하고
                    goToNextPump= true;         // next pump로 보낸다.
                }
                else GM.resetPumpCheck(false);
            }
                 
            if(false == goToNextPump)
                return this;
        }

        if (GM.CollectTreasure()) {
            return this;
        } else {
            return _goNextPump();
        }
    }

    GamePump _goNextPump()
    {
        _fElTime4EnterDelay     = .0f;
		GM.ShowClippingBorad(false);

        //Debug.Log("End of Drop Pump !!! " + (tempCounter++));
        JMFRelay.FireOnFinishDrop();

        return GM.matchPump;
    }

    public override void ResetPump ()
    {
        _fElTime4EnterDelay     = .0f;
    }
}

// [AI_MISSION]
public class AiPump : GamePump
{
    public override PUMP_STATE State { get { return PUMP_STATE.AI_PROC; } }

    //float fElTime               = 2.0f;
    Dictionary<Board, List<Board>> suggestionPieces = new Dictionary<Board, List<Board>>();
    List<SwapInfo> candidates   = new List<SwapInfo>();
    int _state                  = 0;
    public override GamePump Next ()
    {
        switch(_state)
        {
        case 0:
            _state              = 1;
            if(suggestionPieces.Count == 0) // 자동 매칭할 piece가 없으면 섞는다.
            {
                List<Board> listDummy = new List<Board>();
                suggestionPieces= GM.GetSuggestablePieces(ref listDummy);
                if (suggestionPieces.Count <= 0) {
                    return GM.shufflePump;
                }
            }
            DOVirtual.DelayedCall(0.5f, () => { _state = 2; } );
            break;
        case 1:                 break;
        case 2:
        {
            _state              = 1;
            _sortCandidates(suggestionPieces);
            List<Board>         listBoards  = new List<Board>();
            listBoards.Add(candidates[0].From);
            listBoards.Add(candidates[0].To);
            GM.AiCurtainReady(ref listBoards, 0.7f);
            DOVirtual.DelayedCall(0.7f, () => { _state = 4; } );
            break;
        }
        case 4:
            _state              = 0;
            GM.AiDrag(candidates[0].From.PT, candidates[0].To.PT);
            candidates.Clear();
            suggestionPieces.Clear();
            return GM.idlePump;
        }
       
        return this;
    }

    public override void ResetPump ()
    {
        base.ResetPump();
        _state                  = 0;
    }

    #region => from solver class.
    void _sortCandidates (Dictionary<Board, List<Board>> dict) {

        // GM.CurrentLevel.ai_intelligence ; 0 ~ 100
        foreach (Board bd in dict.Keys) {
            List<Board> bds = dict[bd];

            for (int i = 0; i < bds.Count; i++) {
                SwapInfo info = new SwapInfo(bd, bds[i]);
                info.SwapValue = GetSwapValue(info.From, info.To);
                candidates.Add(info);
            }
        }

        if (candidates.Count <= 0) return;
        candidates.Sort((a,b) => { return b.SwapValue.CompareTo(a.SwapValue); });

#if UNITY_EDITOR
        if(candidates.Count > 0)
        {
            //Debug.Log( "### Ai choosed... " + candidates[0].SwapValue + " value selected ! (" + 
            //            candidates[0].From.X + ", " + candidates[0].From.Y + ") => (" + 
            //            candidates[0].To.X + ", " + candidates[0].To.Y + ")" );
        }
#endif

        if (candidates[0].SwapValue > 0) {
            // 1 뒤로 절삭.
            if (candidates.Count > 1) {
                candidates.RemoveRange(1, candidates.Count-1);
            }
        }

        // 에러 검출용 로그 !
        {
            if(0 == candidates.Count)                   
                Debug.Log("wrong cadidate size !!!");   // 대상이 없다 ! => 애초에 여기 들어오면 안된다 ! -> removeRange 실패 !
            if(candidates.Count>=1 && candidates[0].From.Equals( candidates[0].To) )
                Debug.Log("wrong cadidate data !!!");   // 이동 소스와 타겟이 같다 -> 못움직인다 !!! 
        }
    }

    enum M_CASE
    {   MATCH_OTHER_AREA, MATCH_MY_AREA, SPECIAL_MACH, R_CREATE, R_MATCH, MAX  };
    long GetSwapValue (Board from, Board to) {

        // 가중치 조절 factor.
        //
        // = 매칭 되는 piece 수. (2x2 추가 할 것) - later. 
        // = 영역 구분(상대 영역일수록 고점) - done.
        // = 매칭 piece가 특수 셀을 포함하고 있는가. - done.
        
        // 수정 leveling.
        // 0 단계 - random
        // 1 단계 - 일반(상대영역) -> 일반(내영역) -> 특수셀작동 -> R 생성 -> R 작동
        // 2 단계 - 특수셀작동 -> R 생성 -> R 작동 ->  일반(상대영역) -> 일반(내영역)
        // 3 단계 - R 생성 -> R 작동 -> 특수셀작동 -> 일반(상대영역) -> 일반(내영역)
       
        int[] arrFactors        = new int[ (int)M_CASE.MAX ];
        int factor              = GM.CurrentLevel.ai_intelligence;
        switch(factor)
        {        
        case 1:                 // 1 단계 - 일반(상대영역) -> 일반(내영역) -> 특수셀작동 -> R 생성 -> R 작동
            arrFactors[ (int)M_CASE.MATCH_OTHER_AREA]   = 5;
            arrFactors[ (int)M_CASE.MATCH_MY_AREA]      = 4;
            arrFactors[ (int)M_CASE.SPECIAL_MACH]       = 3;
            arrFactors[ (int)M_CASE.R_CREATE]           = 2;
            arrFactors[ (int)M_CASE.R_MATCH]            = 1;
            break;
        case 2:                 // 2 단계 - 특수셀작동 -> R 생성 -> R 작동 ->  일반(상대영역) -> 일반(내영역)
            arrFactors[ (int)M_CASE.MATCH_OTHER_AREA]   = 2;
            arrFactors[ (int)M_CASE.MATCH_MY_AREA]      = 1;
            arrFactors[ (int)M_CASE.SPECIAL_MACH]       = 5;
            arrFactors[ (int)M_CASE.R_CREATE]           = 4;
            arrFactors[ (int)M_CASE.R_MATCH]            = 3;
            break;
        case 3:                 // 3 단계 - R 생성 -> R 작동 -> 특수셀작동 -> 일반(상대영역) -> 일반(내영역)
            arrFactors[ (int)M_CASE.MATCH_OTHER_AREA]   = 2;
            arrFactors[ (int)M_CASE.MATCH_MY_AREA]      = 1;
            arrFactors[ (int)M_CASE.SPECIAL_MACH]       = 3;
            arrFactors[ (int)M_CASE.R_CREATE]           = 5;
            arrFactors[ (int)M_CASE.R_MATCH]            = 4;
            break;
        case 0:                 // 0 단계 - full random.    
        default:
            return UnityEngine.Random.Range(1, 6);
        }

        if (from.IsMatchable) {

            //Board _2x2          = GM.get2x2MatchableBoard(to, from.ColorIndex);
            List<Board> row     = GM.GetRowMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));
            List<Board> col     = GM.GetColMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));
            // - List<Board> row = GM.GetRowMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));
            // - List<Board> col = GM.GetColMatchableBoards(to, from.ColorIndex, to.GetNeighborDirection(from));

            // play rainbow !!!
            if(to.PD is SpecialFive)
                return arrFactors[ (int)M_CASE.R_MATCH ];

            // making rainbow.
            if ((row.Count > 3) || (col.Count > 3))
                return arrFactors[ (int)M_CASE.R_CREATE ];

            // check with special pieces.
            if((row.Count>0 || col.Count>0) && _isSpecialPiece(from))
                return arrFactors[ (int)M_CASE.SPECIAL_MACH];

            // note : special match making 은 case 없음.
            // if(null!=_2x2 && (1<=row.Count && 1<=col.Count))// 2x2 match.
            //    return arrFactors[ (int)M_CASE.SPECIAL_MACH];

            bool oterSide       = false;
            for (int k = 0; k < row.Count; ++k)
            {
                if(_isSpecialPiece( row[k] ))
                    return arrFactors[ (int)M_CASE.SPECIAL_MACH];  // 특수 셀은 무조건 바로 exe !!!
                if(Board.AI_SIDE.ENEMIES != row[k].AiSide)
                    oterSide    = true;
            }
            for(int k = 0; k < col.Count; ++k)
            {
                if(_isSpecialPiece( col[k] ))
                    return arrFactors[ (int)M_CASE.SPECIAL_MACH];  // 특수 셀은 무조건 바로 exe !!!
                if(Board.AI_SIDE.ENEMIES != col[k].AiSide)
                    oterSide    = true;
            }
            
            // normal matching.            
            if(row.Count>=2 || col.Count>=2) 
            {
                if (oterSide)
                    return arrFactors[ (int)M_CASE.MATCH_OTHER_AREA];
                else
                    return arrFactors[ (int)M_CASE.MATCH_MY_AREA];
            }
        } else {

            if (from.PD is SpecialFive) 
                return arrFactors[ (int)M_CASE.R_MATCH ];
        }

        return 0;
    }
    bool _isSpecialPiece(Board board)
    {
        if(null == board)       return false;
        if(null == board.PD)    return false;

        if(board.PD is BombPiece || board.PD is VerticalPiece || board.PD is HorizontalPiece ||
            board.PD is SpecialFive)
            return true;

        return false;
    }
    #endregion
}


public class MatchPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.MATCH; } }

    // [RESET_PHASE_TIMING] 
    float _fElTime4EnterDelay   = .0f;
    bool _waitTillMatchingIsOver= false;
    // float _fElTimeMatchDelay    = .0f; => 해당 로직 제거 & Logic이 안정화 될때까지 일단 주석처리.
    // float _fTimeMatchDelay      = .0f;

    public override GamePump Next () {

        // Debug.Log("[PUMP] ==== Match Pump");

        #region ==> Wait Till Matching is Over.
        if (true == _waitTillMatchingIsOver)
        {
            bool goToNextPump   = false;
            if(GM.IsAllBoardStable == false)
            {
                GM.triggerPumpErrorCheck(true);     // pump error check 시작.
                if(2 == GM.getPumpCheckState())     // 끝났고, 
                {
                    if(GM._hasPumpError)            // 오류가 있으면
                    {
                        Debug.Log("=== pump state error at Match pump !!!");
                        GM.resetPumpCheck();        // 보드 스테이트를 초기화 하고
                        goToNextPump    = true;     // drop pump로 보낸다.
                    }
                    else GM.resetPumpCheck(false);
                }
                 
                if(false == goToNextPump)
                    return this;
            }

            //_fElTimeMatchDelay += (Time.deltaTime*Time.timeScale);
            //if(_fElTimeMatchDelay < _fTimeMatchDelay)
            //    return this;

            _waitTillMatchingIsOver= false;
			GM.ShowClippingBorad(true);

            return GM.dropPump;
        }
        #endregion

        _fElTime4EnterDelay += (Time.deltaTime*Time.timeScale);
        if(_fElTime4EnterDelay < GM.matchPumpDelay)
            return this;

        bool matched = false;
		GM.animationCount = 0;
		
        if(GM.IsAllBoardStable==false)// && false==GM._forceProcMatchPump) 
        {
            //Debug.Log("On Match Pump : Wait Till Stable...." + counter);
            return this;
        }

        //Debug.Log("On Match Pump : Processing.... " + counter);
        
        //_fTimeMatchDelay        = .0f;
        //for (int i = GM.PieceTypes.Length - 1; i >= 0; i--) {
        //    if (GM.PieceTypes[i].hasMatchCondition == false) continue;
        for (int i = GM.MatchOrders.Length - 1; i >= 0; i--)
        {
            PieceDefinition     targetPD    = GM.MatchOrders[i];
            foreach (Board bd in GM.Boards) {
                if (bd.IsNeedMatchCheck == false) continue;

                if (bd.IsMatchable) {

                    // [2X2_BURST]
                    //Board _2x2  = GM.get2x2MatchableBoard(bd, bd.ColorIndex);
                    
                    List<Board> row = GM.GetRowMatchableBoards(bd, bd.ColorIndex, JMF_DIRECTION.NONE);
                    List<Board> col = GM.GetColMatchableBoards(bd, bd.ColorIndex, JMF_DIRECTION.NONE);

                    // note : 폭탄만 유일하게 need match check board 이외의 매칭 부분에서 bomb piece가 될 조건을 가질 수 있다.
                    bool bombMatch  = false;
                    if(0==i && (row.Count>=2 || col.Count>=2))
                        bombMatch   = _checkBombMatch(i, ref row, ref col);

                    // 해당 안되는 일반 매칭 체크.
                    if(false == bombMatch)
                    {
                        float fDelay= .0f;
					    if(Match(targetPD, bd, row, col, ref fDelay))
						    matched = true;
                    }
					
                    // 제일큰 delay 값으로 대처.
                    //_fTimeMatchDelay    = _fTimeMatchDelay<fDelay ? fDelay : _fTimeMatchDelay;

                    if (i == 0) bd.IsNeedMatchCheck = false;
                } else {
                    bd.IsNeedMatchCheck = false;
                }
            }
        }

        if(_check2x2ColorBomb())   matched = true;

        _fElTime4EnterDelay     = .0f;
		
        if (matched)
		{
            GM.IncreaseCombo();

            _waitTillMatchingIsOver = true;
            //_fElTimeMatchDelay      = .0f;

            return this;
            //return GM.dropPump;
        }
		else
		{   
            if(GM.itemUsing)    return GetNextPhaseOfEnd();
            else                return GM.cakePump;
        }
    }


    // note : 폭탄만 유일하게 need match check board 이외의 매칭 부분에서 bomb piece가 될 조건을 가질 수 있다.
    bool _checkBombMatch(int idxOrder, ref List<Board> row, ref List<Board> col)
    {
        if(0 != idxOrder)       return false;

        bool ret                = false;
        List<Board> listMembers = new List<Board>();

        // 일반 매칭일때, 매칭 조건이 되는 블럭들 중 폭탄 생성 조건이 되는 녀석이 있으면, 
        // 일반 매칭을 무시하고 폭탄 매칭으로 대처.
        if(row.Count >= 2)
        {
            for(int qq = 0; qq < row.Count; ++qq)
            {
                listMembers.Add( row[qq] );

                List<Board> rrow = GM.GetRowMatchableBoards(row[qq], row[qq].ColorIndex, JMF_DIRECTION.NONE);
                List<Board> ccol = GM.GetColMatchableBoards(row[qq], row[qq].ColorIndex, JMF_DIRECTION.NONE);

                float fDelay2   = .0f;
				if(Match( JMFUtils.GM.GetPieceType<BombPiece>(), row[qq], rrow, ccol, ref fDelay2))
                    ret         = true;
            }
        }
        if(col.Count >= 2)
        {
            for(int qq = 0; qq < col.Count; ++qq)
            {
                listMembers.Add( col[qq] );

                List<Board> rrow = GM.GetRowMatchableBoards(col[qq], col[qq].ColorIndex, JMF_DIRECTION.NONE);
                List<Board> ccol = GM.GetColMatchableBoards(col[qq], col[qq].ColorIndex, JMF_DIRECTION.NONE);

                float fDelay2= .0f;
				if(Match( JMFUtils.GM.GetPieceType<BombPiece>(), col[qq], rrow, ccol, ref fDelay2))
                    ret         = true;
            }
        }

        // 조건 해당 된다면, 중복 check 하지 않도록 한다.
        if(true == ret)
        {
            for(int qq = 0; qq < listMembers.Count; ++qq)
                listMembers[qq].IsNeedMatchCheck = false;
        }

        return ret;
    }

    bool Match (PieceDefinition pd, Board bd, List<Board> linkedCubesX, List<Board> linkedCubesY, ref float fOutWaitDelay)
    {
        Debugger.Assert(linkedCubesX != null, "GameManager.Match : X List is null.");
        Debugger.Assert(linkedCubesY != null, "GameManager.Match : Y List is null.");

        fOutWaitDelay           = .0f;
        int _score = 0;
        int colorIndex = bd.Piece.ColorIndex;
        bool isShowEffect = false;
        PieceDefinition oriPD = bd.Piece.PD;

        // [2X2_BURST]
        //Board bdCornor          = null;
        // pd.set2x2BoardInfo( bdCornor );

        if (pd.Match(ref bd, linkedCubesX, linkedCubesY, out _score)) {

            // [2X2_BURST] -- Test.
            //if (pd is NormalPiece) {
            if( (pd is NormalPiece) || (pd is RandLine2x23Piece) || (pd is TMatch7Piece) || 
                (pd is VerticalPiece) || (pd is HorizontalPiece) || (pd is BombPiece) )
            {
                //
                //_score += (GM.matchScore * GM.ComboCount);
                //
            } else {
                if (IsShowEffectPiece(oriPD)) isShowEffect = true;
            }

            // [RESET_PHASE_TIMING]
            fOutWaitDelay       = (null!=bd && null!=bd.PD) ? bd.PD.effectTime : .0f;

            // build splash target first.
            List<Board> splashTargets = new List<Board>();
            splashTargets.Add(bd);
            GM.SplashHit(bd, .0f, null, colorIndex);

            if(colorIndex == (int)(LEItem.COLOR.SUGAR_CHERRY-1))
                GM.ShootShockWave(bd.PT, 4.5f, 1F, 0.5F, false, 0.2f);

            List<Board> rand2x3Targets  = null;
            if(pd is RandLine2x23Piece)
            {
                rand2x3Targets  = (pd as RandLine2x23Piece).getListBurstTarget();
                if(rand2x3Targets.Count > 0)
                    splashTargets.AddRange( rand2x3Targets );
            }
            else
            {
                // 2x2 매치가 아닌 경우, 2보다 작은 수는 매칭되지 못하므로 버린다.
                if(linkedCubesX.Count<2)    linkedCubesX.Clear();
                if(linkedCubesY.Count<2)    linkedCubesY.Clear();
            }
            
            foreach (Board _board in linkedCubesX) {
                splashTargets.Add(_board);
			}
			foreach (Board _board in linkedCubesY) {
                splashTargets.Add(_board);
			}

            // 매칭된 모든 각자에게 전체 매칭 정보 setting.
            for(int rr=0; rr<splashTargets.Count; ++rr)
                splashTargets[rr].setMatchingList( splashTargets );
            //


			foreach (Board _board in linkedCubesX) {
                _board.IsNeedMatchCheck = false;

                // [RESET_PHASE_TIMING]
                float fDistDelay    = Mathf.Abs(_board.X-bd.X) * GM.burstSequenceDelay;
                if(null!=_board.PD && fOutWaitDelay<(_board.PD.getBurstTime()+fDistDelay))
                    fOutWaitDelay   = _board.PD.getBurstTime() + fDistDelay;

                //_score += _board.Hit(true, isShowEffect);
                int idxColor    = _board.Piece.ColorIndex;
                // _board.Piece.triggerBombMultiBursting();
                _score += _board.Hit(true, true, fDistDelay, false, isShowEffect, false, null);
                GM.SplashHit(_board, fDistDelay, null, idxColor);
			}

			foreach (Board _board in linkedCubesY) {
                _board.IsNeedMatchCheck = false;

                // [RESET_PHASE_TIMING]
                float fDistDelay    = Mathf.Abs(_board.Y-bd.Y) * GM.burstSequenceDelay;
                if(null!=_board.PD && fOutWaitDelay<(_board.PD.getBurstTime()+fDistDelay))
                    fOutWaitDelay   = _board.PD.getBurstTime() + fDistDelay;

                int idxColor    = _board.Piece.ColorIndex;
                //_score += _board.Hit(true, isShowEffect);
                //_board.Piece.triggerBombMultiBursting();
                _score += _board.Hit(true, true, fDistDelay, false, isShowEffect, false, null);
                GM.SplashHit(_board, fDistDelay, null, idxColor);
			}


            // [2X2_BURST]
            if(null != rand2x3Targets)
            {
                // !!!
                bd.Piece.NeedMakeBuffWhenDestroy    = true;

                for(int uu = 0; uu < rand2x3Targets.Count; ++uu)
                {
                    // !!!
                    rand2x3Targets[uu].Piece.NeedMakeBuffWhenDestroy = true;

                    // duplication check.
                    if(true==linkedCubesX.Contains(rand2x3Targets[uu]) || true==linkedCubesY.Contains(rand2x3Targets[uu]))
                        continue;

                    rand2x3Targets[uu].IsNeedMatchCheck = false;

                    // [RESET_PHASE_TIMING]
                    if(null!=rand2x3Targets[uu].PD && fOutWaitDelay<(rand2x3Targets[uu].PD.getBurstTime()+GM.burstSequenceDelay))
                        fOutWaitDelay   = rand2x3Targets[uu].PD.getBurstTime() + GM.burstSequenceDelay;

                    int idxColor    = rand2x3Targets[uu].Piece.ColorIndex;
                    //rand2x3Targets[uu].Piece.triggerBombMultiBursting();
                    _score += rand2x3Targets[uu].Hit(true, true, GM.burstSequenceDelay, false, isShowEffect, false, null);
                    GM.SplashHit(rand2x3Targets[uu], GM.burstSequenceDelay, null, idxColor);
                }
            }

            GM.IncreaseScore(_score, bd.PT, colorIndex);

			linkedCubesX.Clear();
			linkedCubesY.Clear();

            //= 각자 delay에 맞게 splash 되도록 변경.
            //= GM.SplashHit(splashTargets, 0.25f);

            // NNSoundHelper.Play("IFX_block_match_01"); => GM.IncreaseCombo(); 안에서 처리.

            return true;
		} else {
            return false;
        }
	}

    bool _check2x2ColorBomb()
    {
        List<Board> cakes       = new List<Board>();
        foreach (Board bd in GM.Boards)
        {
            if ((bd.PND is ColorBombPanel) == false)
                continue;

            if ((bd.PND as ColorBombPanel).IsExplodable(bd.Panel))
            {
                List<Board>     others = (bd.PND as ColorBombPanel).getOtherColorBombPanel(bd.Panel);
                cakes.Add(bd);
                for (int j = 0; j < others.Count; j++) {
                    cakes.Add(others[j]);
                }
                //exploded = true;

                for (int i = 0; i < cakes.Count; i++)
                {
                    //explodeCount++;
                    cakes[i].Panel.Destroy(false, true, true, (success) => { } );
                    //{ explodeCount--; });
                }


                // Bomb All Board !!!
                for (int i = 1; i < GameManager.WIDTH; i++) 
		        {
                    List<Board> boards = bd.GetBoardsFromDistance(i);
                    if (boards.Count > 0) 
			        {
                        foreach (Board _bd in boards) 
				        {
                            // 이 셀중에 rainbow cell은 destory제외 한다.
                            if((_bd.PD is SpecialFive) || (_bd.PD is TMatch7Piece))
                                continue;
                            if(null != _bd.Panel)   _bd.Panel.setDamagingColor(-1);
                            _bd.Hit(0.05f * (i - 1));
                        }
                    }
			        else
			            break;
                }

                return true;
            }
        }

        return false;
    }

    bool IsShowEffectPiece(PieceDefinition pd) {
        if (pd is NormalPiece) return true;
        if (pd is FairyPiece) return true;
        if (pd is PenguinPiece) return true;
        return false;
    }
}

// ColorBombPanel 2x2 Pump를 여기서 처리 함.
public class CakePump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.CAKE; } }

    int explodeCount = 0;
    bool exploded = false;
    List<Board> cakes = new List<Board>();

    public override GamePump Next ()
    {
        // test.
        if(GM.DestoryWaiterBursted) return GM.destroyAwaiterPump;
        else if(GM.PlayMoved)       return GM.colorChangerPump;    // GM.snowmanPump;      - go to next pump.
        else                        return GM.destroyAwaiterPump;  // GetNextPhaseOfEnd();    // - finalize pump.

        /*
        if (explodeCount > 0) return this;

        if (exploded)
		{
            exploded = false;
			GM.ShowClippingBorad(true);
            return GM.dropPump;
        }

        cakes.Clear();

        foreach (Board bd in GM.Boards)
        {
            if ((bd.PND is ColorBombPanel) == false) continue;

            if ((bd.PND as ColorBombPanel).IsExplodable(bd.Panel))
            {
                List<Board> otherCakes = (bd.PND as ColorBombPanel).getOtherColorBombPanel(bd.Panel);

                cakes.Add(bd);

                for (int j = 0; j < otherCakes.Count; j++) {
                    cakes.Add(otherCakes[j]);
                }

                exploded = true;
                break;
            }
        }

        if (exploded) {
            PerformPower();
            return this;
        } else {
            // ==> we don't use timebomb for now.
            //if (GM.PlayMoved) {
                // GM.PlayMoved = false;
            //    return GM.timebombPump;
            //} else {

                //return GM.colorChangerPump;

                if(GM.DestoryWaiterBursted) return GM.destroyAwaiterPump;
                else if(GM.PlayMoved)       return GM.colorChangerPump;    // GM.snowmanPump;      - go to next pump.
                else                        return GM.destroyAwaiterPump;  // GetNextPhaseOfEnd();    // - finalize pump.
            //}
        }
        */
    }

    public override void ResetPump () {
        explodeCount = 0;
        exploded = false;
        cakes.Clear();
    }

	void PerformPower () {
        List<Board> processed = new List<Board>();

        processed.AddRange(cakes);

        for (int i = 0; i < cakes.Count; i++) {
            explodeCount++;
            cakes[i].Panel.Destroy(false, true, true, (success) => { explodeCount--; });
        }

  /*      for (int i = 1; i < GameManager.WIDTH; i++) {
            for (int j = 0; j < cakes.Count; j++) {
                List<Board> targets = new List<Board>();
                List<Board> candidates = cakes[j].GetBoardsFromDistance(i);

                for (int k = 0; k < candidates.Count; k++) {
                    if (processed.Contains(candidates[k])) continue;
                    if (candidates[k].PND is CakePanel) continue;

                    targets.Add(candidates[k]);
                }

                for (int k = 0; k < targets.Count; k++) {
                    explodeCount++;
                    targets[k].Hit(0.05F * i, () => { explodeCount--; });
                }

                processed.AddRange(candidates);
            }
        }*/
    }
}

public class TimebombPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.TIMEBOMB; } }

    public override GamePump Next () {        
        GM.ExplodedBomb = false;

        foreach (Board bd in GM.Boards) {
            if (bd.IsFilled == false) continue;
            if ((bd.PD is TimeBombPiece) == false) continue;
            if (bd.Piece.FallBackTime > 0) continue;

            GM.ExplodedBomb = true;
            break;
        }

        return GM.colorChangerPump; //.snowmanPump;
    }
}

// [COLOR_CHANGER]
public class ColorChangerPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.COLOR_CHANGER; } }

    bool _firstUpdate           = true;
    float _fElTime              = .0f;
    float _fFullDelay           = .0f;

    public override GamePump Next () {

        // Debug.Log("[PUMP] ==== ColorChanger Pump");

        if(true == _firstUpdate)
        {
            #region -> find the changer board, and change its piece's color.
            _fFullDelay         = .0f;
            bool bChange        = false;
            foreach (Board bd in GM.Boards)
            {
                if(null!=bd.PD && true == bd.PD.canColorChange(bd))
                {
                    bd.PD.StartCoroutine( bd.PD.changePieceColorAsChanger(bd, _fFullDelay) );

                    // note : 동시에 터뜨리는 것으로 수정.
                    // _fFullDelay += 0.1f;

                    bChange     = true;
                }
            }
            _fElTime            = .0f;

            // player가 아무 터치 하지 않았는데, 첫 자동 매칭에서 level clear한 경우.
            // note : 이 경우는 세팅 오류지만, 정상 처리를 위해 code를 넣는다.
            //if (GM.IsClearedLevel(GM.CurrentLevel)) 
            //    GM.PlayMoved    = true;
            //

            if(false == bChange)
            {
                return GM.conveyorPump;
            }       

            // trigger sound only once !!!
            NNSoundHelper.Play("IFX_line_block_wind");

            #endregion

            _firstUpdate        = false;
        }
        else
        {
            #region -> wait untill all change finishes.
            _fElTime += Time.deltaTime;
            if(_fElTime >= _fFullDelay+0.1f)
            {
                _firstUpdate     = true;
                return GM.conveyorPump;
            }
            #endregion
        }
        return this;
    }
}

// [COTTON_CANDY]
public class SnowmanPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.SNOWMAN; } }

    int _proc                   = 0;

    public override GamePump Next () {

        //Debug.Log("[PUMP] ==== SnowMan Pump");

        if (SnowmanPanel.DestroyedCount > 0) {
            SnowmanPanel.ResetDestroyedCount();
            return GM.mudCoverPump;
        }

        switch(_proc)
        {
        case 0:                 break;
        case 1:                 return this;
        case 2:
            _proc               = 0;
            return GM.mudCoverPump;
        }

        List<Board> snowmans    = GM.GetBoards<SnowmanPanel>();
        List<Board> factorys    = GM.GetBoards<SnowManFactoryPanel>();
        Dictionary<Board, int> candidates = new Dictionary<Board, int>();
        Dictionary<Board, int> candidatesSpecials = new Dictionary<Board, int>();

        _collectCandidates(ref snowmans, ref candidates, ref candidatesSpecials);
        _collectCandidates(ref factorys, ref candidates, ref candidatesSpecials);
        
        if (candidates.Keys.Count > 0)
            _changePiece(ref candidates);
        //else if(candidatesSpecials.Keys.Count > 0) -> 특수 블럭은 안먹는걸로.. 
        //    _changePiece(ref candidatesSpecials);
        else 
             return GM.mudCoverPump;

        SnowmanPanel.ResetDestroyedCount();

        _proc                   = 1;
        DOVirtual.DelayedCall(0.5f, () => { _proc = 2; });

        return this;    // GM.doughPump;
    }

    void _changePiece(ref Dictionary<Board, int> candidates)
    {
        NNSoundHelper.Play("IFX_block_swap_fail");

        List<Board> bds = new List<Board>(candidates.Keys);

        int index = NNTool.Rand(0, bds.Count);

        bds[index].RemovePiece();
        bds[index].ResetPanel(GM.GetPanelType<SnowmanPanel>(), candidates[bds[index]]);

//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("CreateChocolate", GM.transform);
//        pp.transform.localPosition = bds[index].LocalPosition;
//        pp.Play();

        //= NNSoundHelper.Play("snowbaby");
    }

    void _collectCandidates(ref List<Board> listBoards, ref Dictionary<Board, int> dicNoraml, ref Dictionary<Board, int> dicSpecials)
    {
        for (int i = 0; i < listBoards.Count; i++)
        {
            List<Board> bds     = GetProliferatableBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicNoraml.ContainsKey(bds[j])) continue;
                dicNoraml.Add(bds[j], listBoards[i].Panel.Durability);
            }

            bds                 = GetProliferatableSpecialBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicSpecials.ContainsKey(bds[j])) continue;
                dicSpecials.Add(bds[j], listBoards[i].Panel.Durability);
            }
        }
    }

    List<Board> GetProliferatableBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableBoards : Board is null.");
        Debugger.Assert((bd.PND is SnowmanPanel) || (bd.PND is SnowManFactoryPanel), 
            "GameManager.GetProliferatableBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableBoard(bd.Top)) bds.Add(bd.Top);
        if (IsProliferatableBoard(bd.Bottom)) bds.Add(bd.Bottom);
        if (IsProliferatableBoard(bd.Left)) bds.Add(bd.Left);
        if (IsProliferatableBoard(bd.Right)) bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableBoard (Board bd) {
        if (bd == null)                         return false;
        if ((bd.PND is BasicPanel) == false)    return false;
        if ((bd.PD is NormalPiece) == false)    return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;

        return true;
    }

    List<Board> GetProliferatableSpecialBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableSpecialBoards : Board is null.");
        Debugger.Assert((bd.PND is SnowmanPanel) || (bd.PND is SnowManFactoryPanel), 
            "GameManager.GetProliferatableSpecialBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableSpecialBoard(bd.Top))       bds.Add(bd.Top);
        if (IsProliferatableSpecialBoard(bd.Bottom))    bds.Add(bd.Bottom);
        if (IsProliferatableSpecialBoard(bd.Left))      bds.Add(bd.Left);
        if (IsProliferatableSpecialBoard(bd.Right))     bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableSpecialBoard (Board bd) {
        if (bd == null) return false;
        if ((bd.PND is BasicPanel) == false) return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;
        if ((bd.PD is NormalPiece) == false) return true;

        return false;
    }
}


// [MUD_COVER]
public class MudCoverPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.MUD_COVER; } }

    int _proc                   = 0;
    int _counter                = 0;

    public override GamePump Next () {

        // Debug.Log("[PUMP] ==== Dough Pump");

        if (JMFUtils.GM._countMudCoverDestroyed > 0) {
            JMFUtils.GM._countMudCoverDestroyed = 0;
            return GM.doughPump;
        }

        switch(_proc)
        {
        case 0:                 break;
        case 1:                 return this;
        case 2:
            _proc               = 0;
            return GM.doughPump;
        }

        List<Board> listMuds    = GM.GetShadeBoards(LEItem.SHADE_TYPE.MUD_COVER);
        if(0 == listMuds.Count) return GM.doughPump;


        Dictionary<Board, int> candidates = new Dictionary<Board, int>();
        Dictionary<Board, int> candidatesSpecials = new Dictionary<Board, int>();

        _collectCandidates(ref listMuds, ref candidates, ref candidatesSpecials);

        int ret                 = _counter % 2;
        _counter++;
        if(0 == ret)
        {
            if (candidates.Keys.Count > 0)
                _expandShade(ref candidates);
            else if(candidatesSpecials.Keys.Count > 0)  // -> 특수블럭은 안먹는 걸로.
                _expandShade(ref candidatesSpecials);
            else 
                return GM.doughPump;
        }
        else
             _coverShade(ref listMuds);
        
        JMFUtils.GM._countMudCoverDestroyed = 0;

        _proc                   = 1;
        DOVirtual.DelayedCall(0.5f, () => { _proc = 2; });

        return this;    // GM.conveyorPump;
    }

    void _collectCandidates(ref List<Board> listBoards, ref Dictionary<Board, int> dicNoraml, ref Dictionary<Board, int> dicSpecials)
    {
        for (int i = 0; i < listBoards.Count; i++)
        {
            List<Board> bds     = GetProliferatableBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicNoraml.ContainsKey(bds[j])) continue;
                dicNoraml.Add(bds[j], listBoards[i].Panel.Durability);
            }

            bds                 = GetProliferatableSpecialBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicSpecials.ContainsKey(bds[j])) continue;
                dicSpecials.Add(bds[j], listBoards[i].Panel.Durability);
            }
        }
    }

    // convert normal mud to cover mud.
    void _coverShade(ref List<Board> listMuds)
    {
        if(0 == listMuds.Count) return;

        for (int z = 0; z < listMuds.Count; ++z)
        {
            if(false==listMuds[z].PND is BasicPanel || 0!=listMuds[z].ShadedDurability)
            {
                listMuds.RemoveAt( z );
                z               = -1;
            }
        }
        if(0 == listMuds.Count) 
            return;

        int index               = NNTool.Rand(0, listMuds.Count);

        // show effect.
   /*     SpineEffect spEffect    = NNPool.GetItem<SpineEffect>("Thorn");
        Vector3 pos             = listMuds[index].Position;
		pos.z -= 2.0f;
        spEffect.play("grow-up", .0f);// pos, Vector3.one, 0, false, 0);// 0.35f);
        spEffect.transform.parent = JMFUtils.GM.transform;
        spEffect.transform.position = pos;        
        spEffect.transform.localScale = Vector3.one;// listMuds[index].Panel[BoardPanel.TYPE.FRONT].transform.localScale;
        */
        Board bdTarget          = listMuds[index];
        DOVirtual.DelayedCall(0.45f, () => bdTarget.ResetPanel(GM.GetPanelType<MudCoverPanel>(), 0) );
    } 


    // expand shades.
    void _expandShade(ref Dictionary<Board, int> candidates)
    {
        NNSoundHelper.Play("IFX_block_swap_fail");

        List<Board> bds = new List<Board>(candidates.Keys);

        int index = NNTool.Rand(0, bds.Count);

        //bds[index].RemovePiece();
        //bds[index].ResetPanel(GM.GetPanelType<DoughPanel>(), candidates[bds[index]]);
        bds[index].eShadeType   = (int)LEItem.SHADE_TYPE.MUD_COVER;// .ResetPanel(GM.GetPanelType<DoughPanel>(), candidates[bds[index]]);
        bds[index].ShadedDurability = 0;
    }

    List<Board> GetProliferatableBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableBoards : Board is null.");
        //Debugger.Assert((bd.PND is DoughPanel) || (bd.PND is DoughFactoryPanel), 
        //    "GameManager.GetProliferatableBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableBoard(bd.Top)) bds.Add(bd.Top);
        if (IsProliferatableBoard(bd.Bottom)) bds.Add(bd.Bottom);
        if (IsProliferatableBoard(bd.Left)) bds.Add(bd.Left);
        if (IsProliferatableBoard(bd.Right)) bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableBoard (Board bd) {
        if (bd == null) return false;
        if ((bd.PND is BasicPanel) == false) return false;
        if ((bd.PD is NormalPiece) == false) return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;
        if(bd.eShadeType != (int)LEItem.SHADE_TYPE.NONE)
            return false;

        return true;
    }

    List<Board> GetProliferatableSpecialBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableSpecialBoards : Board is null.");
        //Debugger.Assert((bd.PND is DoughPanel) || (bd.PND is DoughFactoryPanel), 
        //    "GameManager.GetProliferatableSpecialBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableSpecialBoard(bd.Top))       bds.Add(bd.Top);
        if (IsProliferatableSpecialBoard(bd.Bottom))    bds.Add(bd.Bottom);
        if (IsProliferatableSpecialBoard(bd.Left))      bds.Add(bd.Left);
        if (IsProliferatableSpecialBoard(bd.Right))     bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableSpecialBoard (Board bd) {
        if (bd == null) return false;
        if ((bd.PND is BasicPanel) == false) return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;
        if(bd.eShadeType != (int)LEItem.SHADE_TYPE.NONE)
            return false;

        if ((bd.PD is NormalPiece) == false) return true;

        return false;
    }
}


// [DOUGH]
public class DoughPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.DOUGH; } }

    int _proc                   = 0;

    public override GamePump Next () {

        // Debug.Log("[PUMP] ==== Dough Pump");

        if (DoughPanel.DestroyedCount > 0) {
            DoughPanel.ResetDestroyedCount();
            return GetNextPhaseOfEnd();;
        }

        switch(_proc)
        {
        case 0:                 break;
        case 1:                 return this;
        case 2:
            _proc               = 0;
            return GetNextPhaseOfEnd();
        }

        List<Board> snowmans    = GM.GetBoards<DoughPanel>();
        List<Board> factorys    = GM.GetBoards<DoughFactoryPanel>();
        Dictionary<Board, int> candidates = new Dictionary<Board, int>();
        Dictionary<Board, int> candidatesSpecials = new Dictionary<Board, int>();

        _collectCandidates(ref snowmans, ref candidates, ref candidatesSpecials);
        _collectCandidates(ref factorys, ref candidates, ref candidatesSpecials);
        
        if (candidates.Keys.Count > 0)
            _changePiece(ref candidates);
        //else if(candidatesSpecials.Keys.Count > 0)  -> 특수블럭은 안먹는 걸로.
        //    _changePiece(ref candidatesSpecials);
        else 
            return GetNextPhaseOfEnd();  // GM.conveyorPump;

        SnowmanPanel.ResetDestroyedCount();

        _proc                   = 1;
        DOVirtual.DelayedCall(0.5f, () => { _proc = 2; });

        return this;    // GM.conveyorPump;
    }

    void _collectCandidates(ref List<Board> listBoards, ref Dictionary<Board, int> dicNoraml, ref Dictionary<Board, int> dicSpecials)
    {
        for (int i = 0; i < listBoards.Count; i++)
        {
            List<Board> bds     = GetProliferatableBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicNoraml.ContainsKey(bds[j])) continue;
                dicNoraml.Add(bds[j], listBoards[i].Panel.Durability);
            }

            bds                 = GetProliferatableSpecialBoards(listBoards[i]);
            for (int j = 0; j < bds.Count; j++) {
                if (dicSpecials.ContainsKey(bds[j])) continue;
                dicSpecials.Add(bds[j], listBoards[i].Panel.Durability);
            }
        }
    }

    void _changePiece(ref Dictionary<Board, int> candidates)
    {
        NNSoundHelper.Play("IFX_block_swap_fail");

        List<Board> bds = new List<Board>(candidates.Keys);

        int index = NNTool.Rand(0, bds.Count);

        bds[index].RemovePiece();
        bds[index].ResetPanel(GM.GetPanelType<DoughPanel>(), candidates[bds[index]]);

//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("CreateChocolate", GM.transform);
//        pp.transform.localPosition = bds[index].LocalPosition;
//        pp.Play();

        //= NNSoundHelper.Play("snowbaby");
    }

    List<Board> GetProliferatableBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableBoards : Board is null.");
        Debugger.Assert((bd.PND is DoughPanel) || (bd.PND is DoughFactoryPanel), 
            "GameManager.GetProliferatableBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableBoard(bd.Top)) bds.Add(bd.Top);
        if (IsProliferatableBoard(bd.Bottom)) bds.Add(bd.Bottom);
        if (IsProliferatableBoard(bd.Left)) bds.Add(bd.Left);
        if (IsProliferatableBoard(bd.Right)) bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableBoard (Board bd) {
        if (bd == null) return false;
        if ((bd.PND is BasicPanel) == false) return false;
        if ((bd.PD is NormalPiece) == false) return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;

        return true;
    }

    List<Board> GetProliferatableSpecialBoards (Board bd) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableSpecialBoards : Board is null.");
        Debugger.Assert((bd.PND is DoughPanel) || (bd.PND is DoughFactoryPanel), 
            "GameManager.GetProliferatableSpecialBoards : Board is not Snowman.");

        List<Board> bds = new List<Board>();

        if (IsProliferatableSpecialBoard(bd.Top))       bds.Add(bd.Top);
        if (IsProliferatableSpecialBoard(bd.Bottom))    bds.Add(bd.Bottom);
        if (IsProliferatableSpecialBoard(bd.Left))      bds.Add(bd.Left);
        if (IsProliferatableSpecialBoard(bd.Right))     bds.Add(bd.Right);

        return bds;
    }

    bool IsProliferatableSpecialBoard (Board bd) {
        if (bd == null) return false;
        if ((bd.PND is BasicPanel) == false) return false;
        if(null!=bd.Piece && bd.Piece.LifeCover > 0)
            return false;

        if ((bd.PD is NormalPiece) == false) return true;

        return false;
    }
}


public class ConveyorPump : GamePump {
    int moveCount = 0;
    bool moved = false;

    List<List<Board>> conveyors = new List<List<Board>>();
    
    public override PUMP_STATE State { get { return PUMP_STATE.CONVEYOR; } }

    public override GamePump Next () {

        if (moveCount > 0) return this;

        if (moved) {
            moved = false;
            return GM.chameleonPump;
        }

        if (GM.IsAllBoardStable == false) return this;

        Dictionary<Board, Board> dicLinkerFromTo = new Dictionary<Board, Board>();
        #region // 이동시키기 전에 먼저 linker piece들을 찾는다.
        foreach (Board bd in GM.Boards)
        {
            if(null == bd.PnlRiver())
                continue;

            ConveyorPanel.Info  info = bd.PnlRiver().info as ConveyorPanel.Info;
            if(info.Type!=ConveyorPanel.TYPE.BEGIN || bd._idConveyor<=0)
                continue;
            
            foreach (Board innderBd in GM.Boards)
            {
                if(null == innderBd.PnlRiver())
                    continue;

                if(false==bd.Equals(innderBd) && innderBd._idConveyor==bd._idConveyor)
                {
                    Board bdBuff    = new Board(GM, new Point(innderBd.X, innderBd.Y), new Vector3(innderBd.X, innderBd.Y, 0F)*GM.Size);
                    Vector3 inPos   = (bd.PnlRiver().PND as ConveyorPanel).GetInPosition(bd);
                    bdBuff.createBoardData4Conveyor(innderBd, GM.transform.InverseTransformPoint(inPos));
                    dicLinkerFromTo.Add( bd, bdBuff );
                    Debug.Log("=== conveyor linker founded X:" + innderBd.X + " Y:" + innderBd.Y);
                    break;
                }
            }    
        }
        #endregion
                
        Board bdBuffer          = null;
        for (int i = 0; i < conveyors.Count; i++)           // 여러 컨베이어 일 수 있다.
        {
            List<Board> items  = conveyors[i];
            List<int>           idxChocoMoved = new List<int>();

            for (int j = items.Count - 1; j >= 0; j--)      // 하나 내의 순환 items.
            {
                Board bd        = items[j];
                Board prev      = (bd.PnlRiver().PND as ConveyorPanel).GetPrev(bd);
                ConveyorPanel.Info info = bd.PnlRiver().info as ConveyorPanel.Info;
                int nRet        = 0;
                int idxPrev     = prev==null ? -1 : prev.X+(GameManager.HEIGHT-1-prev.Y)*GameManager.WIDTH;

                // START 지점 : 현재 piece를 해당 방향으로 이동시키고 없애버린다.
                // END 지점 : linker가 있다면 그 piece를 가져와서 이동시키고, 없다면 START piece(LOOP)를 가져온다.
                #region // 컨베이어 위치에 따른 이동 설정을 한다.
                if (j == items.Count - 1)
                {
                    // create linker buffer.
                    bdBuffer    = new Board(GM, new Point(bd.X, bd.Y), new Vector3(bd.X, bd.Y, 0F)*GM.Size);

                    // End 보드 데이터를 buffer에 백업한다.
                    nRet        = bdBuffer.createBoardData4Conveyor(bd, bd.LocalPosition);
                    bd.removeBoardData4Conveyor(nRet==1);
                    // 이전 보드 데이를 여기(End)에 덮어 쓴다.
                    nRet        = bd.createBoardData4Conveyor(prev, prev.LocalPosition);
                    prev.removeBoardData4Conveyor(nRet==1);

                    if (info.Type == ConveyorPanel.TYPE.END)
                    {   
                        // 마지막 위치에서 나가게 한 후 remove 한다.
                        moveCount++;
                        Vector3 outPos = (bd.PnlRiver().PND as ConveyorPanel).GetOutPosition(bd);
                        bdBuffer.move4Conveyor(outPos, true, (bdParam) => { moveCount--; });
                        // note : 이 경우 buffer board는 이동 후에 삭제해야 한다.
                    }
                }
                else if (j == 0)
                {
                    if (info.Type == ConveyorPanel.TYPE.BEGIN)
                    {
                        // tail에서 끌어오는게 아니고, 진입 포인트에서 bd로 끌어옴.
                        Vector3 inPos   = (bd.PnlRiver().PND as ConveyorPanel).GetInPosition(bd);

                        // search target from link.
                        if(true == dicLinkerFromTo.ContainsKey(bd))
                        {
                            Board bdLinkedPrev    = dicLinkerFromTo[bd];
                            bd.createBoardData4Conveyor(bdLinkedPrev, GM.transform.InverseTransformPoint(inPos));                            
                            Debug.Log("=> conveyor linker removed. X:" + bdLinkedPrev.X + " Y:" + bdLinkedPrev.Y);
                            bdLinkedPrev.removeBoardData4Conveyor(true);
                            idxPrev     = bdLinkedPrev.X+(GameManager.HEIGHT-1-bdLinkedPrev.Y)*GameManager.WIDTH;
                            dicLinkerFromTo.Remove( bd );
                        }
                        else
                        {
                            idxPrev     = bdBuffer.X+(GameManager.HEIGHT-1-bdBuffer.Y)*GameManager.WIDTH;
                            bd.createBoardData4Conveyor(bdBuffer, GM.transform.InverseTransformPoint(inPos));
                            // note: dbBuffer는 End에서 move-out 중이므로, 그때 remove 처리한다.
                        }

                        // bd.Panel[BoardPanel.TYPE.BACK].GetComponent<Conveyor>().IsLightIsOn = true;
                    }
                    else
                    {
                        bd.createBoardData4Conveyor(bdBuffer, bdBuffer.LocalPosition);
                        // 시작 포인트에 copy후 버퍼 보드 삭제.
                        bdBuffer.removeBoardData4Conveyor(true);
                    }
                }                
                else
                {
                    nRet        = bd.createBoardData4Conveyor( prev, prev.LocalPosition );
                    prev.removeBoardData4Conveyor(nRet==1);
                }

                // cho-co index list를 update 한다.
                // index update.

                for(int qq = 0; qq < GM._listChocoBarInfos.Count; ++qq)
                {
                    if(false==idxChocoMoved.Contains(qq) && true==GM._listChocoBarInfos[qq].listIdxBoards.Contains(idxPrev))
                    {
                        int     idxNext = bd.X+(GameManager.HEIGHT-1-bd.Y)*GameManager.WIDTH ;
                        GM._listChocoBarInfos[qq].listIdxBoards.Remove(idxPrev);
                        GM._listChocoBarInfos[qq].listIdxBoards.Add( idxNext );
                        idxChocoMoved.Add( qq );

                        // Debug.Log(string.Format("Panel Choco[{2}] Moved from-{0} to-{1}", idxPrev, idxNext, qq));
                        break;
                    }
                }
                //

                #endregion

                #region // 설정 데이터를 바탕으로 실제 piece를 이동시킨다.
                {
                    Vector3 newPos = bd.Position;
                    //newPos.z = bd.Piece.GO.transform.position.z;
                    bd.State = Board.STATE.SWAP;
                    moveCount++;

                    bd.move4Conveyor(newPos, false, (bdParam) =>
                    {
                        if (bdParam.IsFilled) {                           
                            //bdParam.Piece.GO.transform.position = bdParam.Position;
                            bdParam.IsNeedMatchCheck = true;
                        }

                        bdParam.State = Board.STATE.STABLE;
                        //bdParam._ppa.Panel[BoardPanel.TYPE.BACK].GetComponent<Conveyor>().IsLightIsOn = false;
                        moveCount--;
                    });
                }
                #endregion

                // river animation trigger.
                // ==> bd.Panel[BoardPanel.TYPE.BACK].GetComponent<Conveyor>().Move();

            }   // End of for (int j = items.Count - 1; j >= 0; j--)
        }       // End of for (int i = 0; i < conveyors.Count; i++)

        if (moveCount > 0) {
            NNSoundHelper.Play("move_panel");
            moved = true;
            return this;
        } else {

            dicLinkerFromTo.Clear();
            return GM.chameleonPump;
        }
    }

    public override void ResetPump () {
        moveCount = 0;
        moved = false;
        conveyors.Clear();
        initConveyors();
    }

    void initConveyors () { 
        conveyors.Clear();

        List<Board> processed = new List<Board>();

        // Step 1. process begin-end type
        foreach (Board bd in GM.Boards) {
            if (null == bd.PnlRiver()) continue;

            if ((bd.PnlRiver().PND as ConveyorPanel).GetType(bd.PnlRiver()) == ConveyorPanel.TYPE.BEGIN) {
                List<Board> items = new List<Board>();
                items.Add(bd);
                processed.Add(bd);
                conveyors.Add(items);
            }
        }

        for (int i = 0; i < conveyors.Count; i++) {
            List<Board> items = conveyors[i];

            Board bd = (items[0].PnlRiver().PND as ConveyorPanel).GetNext(items[0]);

            while (bd != null) {
                items.Add(bd);
                processed.Add(bd);
                bd = (bd.PnlRiver().PND as ConveyorPanel).GetNext(bd);
            }
        }

        // Step 2. process circle type
        foreach (Board bd in GM.Boards) {
            if (processed.Contains(bd)) continue;
            if (null == bd.PnlRiver())  continue;

            List<Board> items = new List<Board>();
            items.Add(bd);
            processed.Add(bd);
            conveyors.Add(items);

            Board next = (bd.PnlRiver().PND as ConveyorPanel).GetNext(bd);

            while (next != null) {
                if (processed.Contains(next)) break;

                items.Add(next);
                processed.Add(next);
                next = (next.PnlRiver().PND as ConveyorPanel).GetNext(next);
            }
        }
    }

    /*void Move4ConveyorAndRemove(Board bdTarget, Vector3 pos, System.Action<Board> onComplete)
    {
        if(null == bdTarget)    return;

        bdTarget.move4Conveyor(pos, true, onComplete);
    }

    void MovePieceAndRemove (GamePiece go, Vector3 pos, System.Action onComplete) {

        go.killDropAction();

        go.GO.transform.DOMove(pos, 1F).SetEase(Ease.Linear).OnComplete(() => {
            NNPool.Abandon(go.GO);
            if (onComplete != null) onComplete();
        });
    }

    void MovePieceToBoardCenter (Board bd, Vector3 pos, System.Action<Board> onComplete) {

        bd.Piece.killDropAction();

        bd.Piece.GO.transform.DOMove(pos, 1F).SetEase(Ease.Linear).OnComplete(() => {
            if (onComplete != null) onComplete(bd);
        });
    }*/
}

public class ChameleonPump : GamePump {
    int transformCount = 0;
    bool transformed = false;

    public override PUMP_STATE State { get { return PUMP_STATE.CHAMELEON; } }

    public override GamePump Next () {
        if (transformCount > 0) return this;

        if (transformed) {
            transformed = false;
            return GM.penguinJumpPump;
        }

        if (GM.IsAllBoardStable == false) return this;
        
        foreach (Board bd in GM.Boards) {
            if (bd.IsFilled == false) continue;
            if ((bd.PD is ChameleonPiece) == false) continue;

            transformCount++;

            (bd.PD as ChameleonPiece).Change(bd.Piece, (_bd) => {
                transformCount--;
                _bd.IsNeedMatchCheck = true;
            });
        }

        if (transformCount > 0) {
            transformed = true;
            return this;
        } else {
            return GM.penguinJumpPump;
        }
    }

    public override void ResetPump () {
        transformCount = 0;
        transformed= false;
    }
}

public class PenguinJumpPump : GamePump {
    int jumpCount = 0;
    bool jumped = false;

    public override PUMP_STATE State { get { return PUMP_STATE.PENGUIN_JUMP; } }

    public override GamePump Next () {
        if (jumpCount > 0) return this;

        if (jumped) {
            jumped = false;
            return GM.penguinSpawnPump;
        }

        if (GM.IsAllBoardStable == false) return this;

        GM.EscapedPenguin = false;

        for (int y = GameManager.HEIGHT - 1; y >= 0; y--) {
            for (int x = 0; x < GameManager.WIDTH; x++) {
                Board bd = GM[x,y];

                if (bd.IsFilled == false) continue;
                if ((bd.PD is PenguinPiece) == false) continue;
                if (bd.Panel.IsStealable() == false) continue;

                Board top = GetTopBoard(bd);

                if (top == null) {
                    bd.Piece.Remove();
                    //GM.EscapedPenguin = true;
                    //NNSoundHelper.Play("IFX_goal_earning");
                } else {
                    if (top.IsFilled == false) continue;
                    if (top.PD is PenguinPiece) continue;
                    if (top.Panel.IsStealable() == false) continue;

                    jumpCount++;
                    JumpPenguin(bd, top, () => { jumpCount--; });
                }
            }
        }

        if (jumpCount > 0) {
            jumped = true;
            return this;
        } else {
            return GM.penguinSpawnPump;
        }
    }

    public override void ResetPump () {
        jumpCount = 0;
        jumped = false;
    }

    Board GetTopBoard (Board bd) {
        Board top = bd.Top;

        while ((top != null) && (top.PND is EmptyPanel)) {
            top = top.Top;
        }

        return top;
    }

    void JumpPenguin (Board bd, Board top, System.Action onComplete) {
        Debugger.Assert(bd != null, "GameManager.GetProliferatableBoards : Board is null.");

        Transform penguinTF = bd.Piece.GO.transform;
        PieceTracker penguinPT = bd.Piece.GO.GetComponent<PieceTracker>();
        Transform topTF = top.Piece.GO.transform;
        PieceTracker topPT = top.Piece.GO.GetComponent<PieceTracker>();

        Vector3 orgScale = penguinTF.localScale;

        bd.State = Board.STATE.SWAP;
        top.State = Board.STATE.SWAP;

        penguinPT.PT = top.PT;
        topPT.PT = bd.PT;

        GamePiece tmpPiece = bd.Piece;
        bd.Piece = top.Piece;
        top.Piece = tmpPiece;

        Sequence seq = DOTween.Sequence();
        seq.Append(penguinTF.DOScaleY(orgScale.y * 0.8F, 0.1F).SetEase(Ease.InQuart));
        seq.Append(penguinTF.DOScaleY(orgScale.y * 1.1F, 0.3F).SetEase(Ease.OutQuart));
        seq.Append(penguinTF.DOScaleY(orgScale.y, 0.1F).SetEase(Ease.InQuart));
        seq.Insert(0F, topTF.DOMoveY(bd.Position.y, 0.5F).SetEase(Ease.OutQuart));
        seq.Insert(0F, penguinTF.DOMoveY(top.Position.y, 0.5F).SetEase(Ease.InOutBack));
        seq.Insert(0.1F, penguinTF.DOScaleX(orgScale.x * 0.9F, 0.1F).SetEase(Ease.OutQuart));
        seq.Insert(0.4F, penguinTF.DOScaleX(orgScale.x, 0.1F).SetEase(Ease.OutQuart));

        seq.OnComplete(() => { 
            bd.IsNeedMatchCheck = true;
            top.IsNeedMatchCheck = true;

            bd.State = Board.STATE.STABLE;
            top.State = Board.STATE.STABLE;

            if (onComplete != null) onComplete();
        });

        NNSoundHelper.Play("penguin_jump");
    }
}

public class PenguinSpawnPump : GamePump {
    int spawnCount = 0;
    bool spawned = false;

    public override PUMP_STATE State { get { return PUMP_STATE.PENGUIN_SPAWN; } }
    
    public override GamePump Next () {
        if (spawnCount > 0) return this;

        if (spawned) {
            spawned = false;
            return GM.yetiPump;
        }

        if (GM.IsAllBoardStable == false) return this;

        if (GM.CurrentLevel.penguinSpawn == null) return GM.yetiPump;

        int spawnableCount = GetSpawnableCountOfPenguin();

        if (spawnableCount <= 0) return GM.yetiPump;

        List<Board> candidates = new List<Board>();

        for (int i = 0; i < GM.CurrentLevel.penguinSpawn.Length; i += 2) {
            Point pt = new Point(GM.CurrentLevel.penguinSpawn[i], GM.CurrentLevel.penguinSpawn[i+1]);
            candidates.Add(GM[pt]);
        }

        if (candidates.Count <= 0) return GM.yetiPump;

        while (candidates.Count > 0) {
            int index = NNTool.Rand(0, candidates.Count);
            Board bd = candidates[index];
            candidates.RemoveAt(index);

            if ((bd.PND is BasicPanel) == false) continue;

            if (bd.IsFilled) {
                if (bd.PD is PenguinPiece) continue;
                if (bd.PD is HorizontalPiece) continue;
                if (bd.PD is VerticalPiece) continue;
                if (bd.PD is BombPiece) continue;
                if (bd.PD is SpecialFive) continue;
            } else {
                continue;
            }

            if (bd.IsStable == false) continue;

            bd.State = Board.STATE.SWAP;

            bd.ResetPiece(GM.GetPieceType<PenguinPiece>(), bd.PD.GetColorIndex());

            spawnCount++;

            ShowPenguinSpawnEffect(bd, (_bd) => {
                    spawnCount--;
                    _bd.State = Board.STATE.STABLE;
                    _bd.IsNeedMatchCheck = true;
                });

            NNSoundHelper.Play("penguin_jump");
            break;
        }

        if (spawnCount > 0) {
            spawned = true;
            return this;
        } else {
            return GM.yetiPump;
        }
    }

    public override void ResetPump () {
        spawnCount = 0;
        spawned = false;
    }

    void ShowPenguinSpawnEffect (Board bd, System.Action<Board> onComplete) {
        Vector3 originalScale = bd.Piece.GO.transform.localScale;
        bd.Piece.GO.transform.localScale = Vector3.zero;

        bd.Piece.GO.transform.DOScale(originalScale, 0.5F)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { if (onComplete != null) onComplete(bd); });
    }

    int GetSpawnableCountOfPenguin () {
        if (GM.CurrentLevel.chanceToPenguinSpawn <= NNTool.Rand(0, 100)) return 0;

        int spawnableCount = Mathf.Max(0, GM.CurrentLevel.maxPenguinOnScreen - Penguin.TotalCount);

        if (GM.CurrentLevel.isPenguinGame) {
            int maxSpawnableCount = Mathf.Max(0, 
                GM.CurrentLevel.numberOfPenguin - (Penguin.TotalCount + GM.PenguinMatchCount));
            spawnableCount = Mathf.Min(spawnableCount, maxSpawnableCount);
        }

        return spawnableCount;
    }
}

public class YetiPump : GamePump {
    int throwCount = 0;
    bool thrown = false;

    public override PUMP_STATE State { get { return PUMP_STATE.YETI; } }

    public override GamePump Next () {
        if (throwCount > 0) return this;

        if (thrown) {
            thrown = false;
            return GM.launcherIcePump;
        }

        if (GM.CurrentLevel.isYetiGame == false) return GM.launcherIcePump;
        if ((Yeti.Current == null) || (Yeti.Current.Dead)) return GM.launcherIcePump;
        if ((GM.Moves - Yeti.Current.PrevAttackMove) < GM.CurrentLevel.attackPerMove) return GM.launcherIcePump;

        int attackCount = NNTool.Rand(GM.CurrentLevel.minYetiAttackCount, GM.CurrentLevel.maxYetiAttackCount+1);

        if (attackCount <= 0) return GM.launcherIcePump;

        Yeti.Current.PrevAttackMove = GM.Moves;
        Yeti.Current.ShowAnimation("yeti_angry");

        NNSoundHelper.Play("yeti_attack");

        int attackType = GetYetiAttackType();
        int randomIndex = GM.GetRandomColorIndex();

        List<Board> candidates = new List<Board>();
        List<Board> targetBoards = new List<Board>();

		foreach (Board bd in GM.Boards) {
            if ((bd.PND is BasicPanel) == false) continue;
            if (bd.IsFilled == false) continue;
            if (bd.Piece.IsMatchable() == false) continue;
            if ((attackType < 6) && (bd.PND is ConveyorPanel)) continue;

            candidates.Add(bd);
        }

        NNTool.ExecuteForEachRandomIndex(0, candidates.Count - 1, (index) => {
            targetBoards.Add(candidates[index]);
            attackCount--;
            return attackCount > 0;
        });

        for (int i = 0; i < targetBoards.Count; i++) {
            Board bd = targetBoards[i];

            GameObject block = GetAttackBlock(attackType, 
                bd.Panel[BoardPanel.TYPE.DEFAULT].transform.localScale, randomIndex);

            throwCount++;

            ThrowBlock(block, bd, (_bd) => {
                ConvertBoard(_bd, attackType, randomIndex);

                if (_bd.IsFilled && (_bd.PD is TimeBombPiece)) {
                    if (GM.CurrentLevel.defaultFallBackTime > 0) {
                        _bd.Piece.FallBackTime = GM.CurrentLevel.defaultFallBackTime;
                    } else {
                        _bd.Piece.FallBackTime = 9;
                    }

                    _bd.IsNeedMatchCheck = true;
                }

                throwCount--;
            });
        }

        if (throwCount > 0) {
            thrown = true;
            return this;
        } else {
            return GM.launcherIcePump;
        }
    }

    public override void ResetPump () {
        throwCount = 0;
        thrown = false;
    }

    void ThrowBlock (GameObject block, Board target, System.Action<Board> onComplete) {
        Debugger.Assert(block != null, "GameManager.ThrowBlock : Block is null.");
        Debugger.Assert(target != null, "GameManager.ThrowBlock : Board is null.");

        Vector3 scale = block.transform.localScale;
        Vector3 yetiPos = Yeti.Current.transform.position;
        yetiPos.y += 3F;
        yetiPos.z -= 1F;
        block.transform.position = yetiPos;
        block.transform.localScale = scale * 0.5F;
        block.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingLayerName = "Particle";

        Sequence seq = DOTween.Sequence();
        seq.Append(block.transform.DOMove(target.Position, 0.5F).SetEase(Ease.InQuad));
        seq.Insert(0F, block.transform.DOScale(scale, 0.5F).SetEase(Ease.InQuad));
        seq.Append(block.transform.DOScale(scale * 1.15F, 0.2F).SetEase(Ease.OutQuad));
        seq.Append(block.transform.DOScale(scale, 0.2F).SetEase(Ease.InQuad));

        seq.OnComplete(() => { 
            block.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingLayerName = "Default";
            NNPool.Abandon(block);
            if (onComplete != null) onComplete(target); 
        });
    }

    int GetYetiAttackType () {
        int rangeMax = 0;
        int probabilitySum = 0;
        int[] probability = GM.CurrentLevel.attackConvertProbability;
        
        foreach (int i in probability) rangeMax += i;

        int index = NNTool.Rand(0, rangeMax);

        for (int i = 0; i < probability.Length; i++) {
            probabilitySum += probability[i];
            if (index < probabilitySum) return i;
        }

        return -1;
    }

    void ConvertBoard (Board bd, int attackType, int index) {
        Debugger.Assert(bd != null, "GameManager.ConvertBlockByYeti : Board is null.");

        bd.RemovePiece();

        switch (attackType) {
            case 5 :
                bd.ResetPanel(GM.GetPanelType<SnowmanPanel>(), 0);
                break;
            case 6 :
                bd.ResetPiece(GM.GetPieceType<PenguinPiece>(), index);
                break;
            case 7 :
                bd.ResetPiece(GM.GetPieceType<RoundChocoPiece>(), 0);
                break;
            case 8 :
                bd.ResetPiece(GM.GetPieceType<TimeBombPiece>(), index);
                break;
            default :
                bd.Panel.Reset(GM.GetPanelType<RectChocoPanel>(), attackType);
                break;
        }
    }

    GameObject GetAttackBlock (int attackType, Vector3 scale, int index) {
        GameObject block = null;

        switch (attackType) {
            case 5 :
                block = NNPool.GetItem(GM.GetPanelType<SnowmanPanel>().skin[0].name, GM.transform);
                break;
            case 6 :
                block = NNPool.GetItem(GM.GetPieceType<PenguinPiece>().skin[index].name, GM.transform);
                break;
            case 7 :
                block = NNPool.GetItem(GM.GetPieceType<RoundChocoPiece>().skin[0].name, GM.transform);
                break;
            case 8 :
                block = NNPool.GetItem(GM.GetPieceType<TimeBombPiece>().skin[index].name, GM.transform);
                block.GetComponent<TimeBomb>().FallBackTime = GM.CurrentLevel.defaultFallBackTime;
                break;
            default :
                block = NNPool.GetItem(GM.GetPanelType<RectChocoPanel>().skin[0].name, GM.transform);
                break;
        }

        block.transform.localScale = scale;

        return block;
    }
}

public class LauncherIcePump : GamePump
{
    enum WORK { READY, DOING, DONE  };
    List<Board> _listLaunchers  = new List<Board>();
    WORK _state                 = WORK.READY;

    public override PUMP_STATE State { get { return PUMP_STATE.LAUNCHER_ICE; } }

    GamePump _gotoNextPump()
    {
        _state                  = WORK.READY;
        return GM.launcherStonePump;
    }

    public void setupBoard()
    {
        _listLaunchers.Clear();
        foreach (Board bd in GM.Boards)
        {
            if (bd.PND is FireIcePanel)
                _listLaunchers.Add(bd);
        }
    }

    public void removeLauncherBoard(Board bd)
    {
        if(_listLaunchers.Contains(bd))
            _listLaunchers.Remove(bd);
    }

    public override GamePump Next ()
    {
        switch(_state)
        {
        case WORK.DOING:        return this;
        case WORK.DONE:         return _gotoNextPump();
        case WORK.READY:        return _init();
        default:                break;
        }
        return this;
    }

    GamePump _init()
    {
        // No Launcher Game Here !
        if(0 == _listLaunchers.Count)
            return _gotoNextPump();

        float delay             = -1.0f;
        for(int z = 0; z < _listLaunchers.Count; ++z)
        {
            if((GM.Moves - GM.PrevIceAttackMove) >= FireIcePanel.MOVE_TO_FIRE)
                delay           = ((FireIcePanel)_listLaunchers[z].PND).fire( _listLaunchers[z].Panel );
        }
        if(delay < 0)           return _gotoNextPump();

        GM.PrevIceAttackMove    = GM.Moves;
        _state                  = WORK.DOING;

        DOVirtual.DelayedCall( delay, () => _state=WORK.DONE );

        return this;
    }
    public override void ResetPump ()
    {
        _listLaunchers.Clear();
        _state                  = WORK.READY;
    }
}

public class LauncherStonePump : GamePump
{
    enum WORK { READY, DOING, DONE  };
    List<Board> _listLaunchers  = new List<Board>();
    WORK _state                 = WORK.READY;

    public override PUMP_STATE State { get { return PUMP_STATE.LAUNCHER_STONE; } }

    GamePump _gotoNextPump()
    {
        _state                  = WORK.READY;
        return GM.launcherWebPump;
    }

    public void setupBoard()
    {
        _listLaunchers.Clear();
        foreach (Board bd in GM.Boards)
        {
            if (bd.PND is FireStonePanel)
                _listLaunchers.Add(bd);
        }
    }

    public void removeLauncherBoard(Board bd)
    {
        if(_listLaunchers.Contains(bd))
            _listLaunchers.Remove(bd);
    }

    public override GamePump Next ()
    {
        switch(_state)
        {
        case WORK.DOING:        return this;
        case WORK.DONE:         return _gotoNextPump();
        case WORK.READY:        return _init();
        default:                break;
        }
        return this;
    }

    GamePump _init()
    {
        // No Launcher Game Here !
        if(0 == _listLaunchers.Count)
            return _gotoNextPump();

        float delay             = -1.0f;
        for(int z = 0; z < _listLaunchers.Count; ++z)
        {
            if((GM.Moves - GM.PrevStoneAttackMove) >= FireStonePanel.MOVE_TO_FIRE)
                delay           = ((FireStonePanel)_listLaunchers[z].PND).fire( _listLaunchers[z].Panel );
        }
        if(delay < 0)           return _gotoNextPump();

        GM.PrevStoneAttackMove  = GM.Moves;
        _state                  = WORK.DOING;

        DOVirtual.DelayedCall( delay, () => _state=WORK.DONE );

        return this;
    }
    public override void ResetPump ()
    {
        _listLaunchers.Clear();
        _state                  = WORK.READY;
    }
}

public class LauncherWebPump : GamePump
{
    enum WORK { READY, DOING, DONE  };
    List<Board> _listLaunchers  = new List<Board>();
    WORK _state                 = WORK.READY;

    public override PUMP_STATE State { get { return PUMP_STATE.LAUNCHER_WEB; } }

    GamePump _gotoNextPump()
    {
        _state                  = WORK.READY;
        return GM.bossPump;
    }

    public void setupBoard()
    {
        _listLaunchers.Clear();
        foreach (Board bd in GM.Boards)
        {
            if (bd.PND is FireWebPanel)
                _listLaunchers.Add(bd);
        }
    }

    public void removeLauncherBoard(Board bd)
    {
        if(_listLaunchers.Contains(bd))
            _listLaunchers.Remove(bd);
    }

    public override GamePump Next ()
    {
        switch(_state)
        {
        case WORK.DOING:        return this;
        case WORK.DONE:         return _gotoNextPump();
        case WORK.READY:        return _init();
        default:                break;
        }
        return this;
    }

    GamePump _init()
    {
        // No Launcher Game Here !
        if(0 == _listLaunchers.Count)
            return _gotoNextPump();

        float delay             = -1.0f;
        for(int z = 0; z < _listLaunchers.Count; ++z)
        {
            if((GM.Moves - GM.PrevWebAttackMove) >= FireWebPanel.MOVE_TO_FIRE)
                delay           = ((FireWebPanel)_listLaunchers[z].PND).fire( _listLaunchers[z].Panel );
        }
        if(delay < 0)           return _gotoNextPump();

        GM.PrevWebAttackMove    = GM.Moves;
        _state                  = WORK.DOING;

        DOVirtual.DelayedCall( delay, () => _state=WORK.DONE );

        return this;
    }
    public override void ResetPump ()
    {
        _listLaunchers.Clear();
        _state                  = WORK.READY;
    }
}
public class BossPump : GamePump {
    int throwCount = 0;
    bool thrown = false;
    bool didAttack = false;
    bool attacking = false;

    public override PUMP_STATE State { get { return PUMP_STATE.BOSS; } }

    public override GamePump Next () {
        if (attacking) return this;
        if (throwCount > 0) return this;

        if (thrown) {
            thrown = false;
            return GM.destroyAwaiterPump;   //GetNextPhaseOfEnd();
        }

        // Note : No Boss Pumps in Here !!!
        return GM.destroyAwaiterPump;       //  GetNextPhaseOfEnd();
        /*
        if (didAttack == false) {
            if (GM.CurrentLevel.isBossGame == false) return GetNextPhaseOfEnd();
            if (GM.CurrentLevel.bossType != 2) return GetNextPhaseOfEnd(); 
            if ((Boss.Current == null) || (Boss.Current.bossDead)) return GetNextPhaseOfEnd();
            if ((GM.Moves - Boss.Current.PrevAttackMove) < GM.CurrentLevel.bossActionPerMove) return GetNextPhaseOfEnd();
            if (BossPanel.HitThisTurn || 
            BossPanel.StraightNotHitCount+1 < GM.CurrentLevel.bossActionPerMove) return GetNextPhaseOfEnd();

            attacking = true;
            didAttack = true;

            Boss.Current.BossAttack(() => { attacking = false; });

            return this;
        } else {
            didAttack = false;
        }

        int attackCount = NNTool.Rand(GM.CurrentLevel.minBossAttackCount, GM.CurrentLevel.maxBossAttackCount+1);

        if (attackCount <= 0) return GetNextPhaseOfEnd();

        Boss.Current.PrevAttackMove = GM.Moves;

        int attackType = GetBossAttackType();
        int randomIndex = GM.GetRandomColorIndex();

        List<Board> candidates = new List<Board>();
        List<Board> targetBoards = new List<Board>();

		foreach (Board bd in GM.Boards) {
            if ((bd.PND is BasicPanel) == false) continue;
            if (bd.IsFilled == false) continue;
            if (bd.Piece.IsMatchable() == false) continue;
            if ((attackType < 6) && (bd.PND is ConveyorPanel)) continue;

            candidates.Add(bd);
        }

        NNTool.ExecuteForEachRandomIndex(0, candidates.Count - 1, (index) => {
            targetBoards.Add(candidates[index]);
            attackCount--;
            return attackCount > 0;
        });

        for (int i = 0; i < targetBoards.Count; i++) {
            Board bd = targetBoards[i];

            GameObject block = GetAttackBlock(attackType, 
                bd.Panel[BoardPanel.TYPE.DEFAULT].transform.localScale, randomIndex);

            throwCount++;

            /* 
            // note : boss code는 처리하지 않음. -> [ROUND_CHOCO_PANEL]
            ThrowBlock(block, bd, (_bd) => {
                ConvertBoard(_bd, attackType, randomIndex);

                if (_bd.IsFilled && (_bd.PD is TimeBombPiece)) {
                    if (GM.CurrentLevel.defaultFallBackTime > 0) {
                        _bd.Piece.FallBackTime = GM.CurrentLevel.defaultFallBackTime;
                    } else {
                        _bd.Piece.FallBackTime = 9;
                    }

                    _bd.IsNeedMatchCheck = true;
                }

                throwCount--;
            });
            */
        /*}

        if (throwCount > 0) {
            thrown = true;
            return this;
        } else {
            return GetNextPhaseOfEnd();
        }*/        
    }

    public override void ResetPump () {
        throwCount = 0;
        thrown = false;
    }

    void ThrowBlock (GameObject block, Board target, System.Action<Board> onComplete) {
        Debugger.Assert(block != null, "GameManager.ThrowBlock : Block is null.");
        Debugger.Assert(target != null, "GameManager.ThrowBlock : Board is null.");

        Vector3 scale = block.transform.localScale;
        Vector3 bossPos = Boss.Current.transform.position;
        bossPos.y += 3F;
        bossPos.z -= 1F;
        block.transform.position = bossPos;
        block.transform.localScale = scale * 0.5F;
        block.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingLayerName = "Particle";

        Sequence seq = DOTween.Sequence();
        seq.Append(block.transform.DOMove(target.Position, 0.5F).SetEase(Ease.InQuad));
        seq.Insert(0F, block.transform.DOScale(scale, 0.5F).SetEase(Ease.InQuad));
        seq.Append(block.transform.DOScale(scale * 1.15F, 0.2F).SetEase(Ease.OutQuad));
        seq.Append(block.transform.DOScale(scale, 0.2F).SetEase(Ease.InQuad));

        seq.OnComplete(() => { 
            block.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingLayerName = "Default";
            NNPool.Abandon(block);
            if (onComplete != null) onComplete(target); 
        });
    }

    int GetBossAttackType () {
        int rangeMax = 0;
        int probabilitySum = 0;
        int[] probability = GM.CurrentLevel.bossAttackConvertProbability;
        
        foreach (int i in probability) rangeMax += i;

        int index = NNTool.Rand(0, rangeMax);

        for (int i = 0; i < probability.Length; i++) {
            probabilitySum += probability[i];
            if (index < probabilitySum) return i;
        }

        return -1;
    }

    void ConvertBoard (Board bd, int attackType, int index) {
        Debugger.Assert(bd != null, "GameManager.ConvertBlockByBoss : Board is null.");

        bd.RemovePiece();

        switch (attackType) {
            case 5 :
                bd.ResetPanel(GM.GetPanelType<SnowmanPanel>(), 0);
                break;
            case 6 :
                bd.ResetPiece(GM.GetPieceType<PenguinPiece>(), index);
                break;
            case 7 :
                bd.ResetPiece(GM.GetPieceType<RoundChocoPiece>(), 0);
                break;
            case 8 :
                bd.ResetPiece(GM.GetPieceType<TimeBombPiece>(), index);
                break;
            default :
                bd.Panel.Reset(GM.GetPanelType<RectChocoPanel>(), attackType);
                break;
        }
    }

    GameObject GetAttackBlock (int attackType, Vector3 scale, int index) {
        GameObject block = null;

        switch (attackType) {
            case 5 :
                block = NNPool.GetItem(GM.GetPanelType<SnowmanPanel>().skin[0].name, GM.transform);
                break;
            case 6 :
                block = NNPool.GetItem(GM.GetPieceType<PenguinPiece>().skin[index].name, GM.transform);
                break;
            case 7 :
                block = NNPool.GetItem(GM.GetPieceType<RoundChocoPiece>().skin[0].name, GM.transform);
                break;
            case 8 :
                block = NNPool.GetItem(GM.GetPieceType<TimeBombPiece>().skin[index].name, GM.transform);
                block.GetComponent<TimeBomb>().FallBackTime = GM.CurrentLevel.defaultFallBackTime;
                break;
            default :
                block = NNPool.GetItem(GM.GetPanelType<RectChocoPanel>().skin[0].name, GM.transform);
                break;
        }

        block.transform.localScale = scale;

        return block;
    }
}

public class ClearPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.CLEAR; } }

    public override GamePump Next () {
		//if(GM.State == JMF_GAMESTATE.BONUS)
		//{
		//	GM.ShowClippingBorad(true);
		//	return GM.dropPump;
		//}
		
        switch(GM.State)
        {
        case JMF_GAMESTATE.PLAY:
            GM.State            = JMF_GAMESTATE.FINAL;
            GM.ExecuteBonusTime();
            return GM.dropPump;
        default:
            break;
        }
        return GM.idlePump;
    }
}

public class FailPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.FAIL; } }

    public override GamePump Next () {
        if (GM.State == JMF_GAMESTATE.FINAL) return this;
        if (GM.State == JMF_GAMESTATE.OVER) return GM.idlePump;

        GM.State = JMF_GAMESTATE.FINAL;
        GM.ExecuteFail();
        return this;
    }
}

public class DestroyAwaiterPump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.DESTROY_AWAITER; } }

    bool _waitTillMatchingIsOver= false;

    public override GamePump Next () {

        if(true == _waitTillMatchingIsOver)
        {
            if(GM.IsAllBoardStable == false) 
                return this;

            _waitTillMatchingIsOver = false;

            return GM.dropPump;
        }

        if (GM.DestroyAwaiter.Count > 0) {
            List<GamePiece> gps = new List<GamePiece>(GM.DestroyAwaiter);
            GM.DestroyAwaiter.Clear();

            foreach (GamePiece gp in gps) {
                if ((gp.PD == null) || (gp.GO == null)) continue;
                gp.Destroy(false, true, true, false);
            }

            _waitTillMatchingIsOver = true;
            GM.DestoryWaiterBursted = true;

			GM.ShowClippingBorad(true);
            return this;    // GM.dropPump;
        }

        if(!GM.PlayMoved)       return this.GetNextPhaseOfEnd();

        return GM.snowmanPump;  // GetNextPhaseOfEnd();
    }
}


/*
public class CakePump : GamePump {
    public override PUMP_STATE State { get { return PUMP_STATE.CAKE; } }

    int explodeCount = 0;
    bool exploded = false;
    List<Board> cakes = new List<Board>();

    public override GamePump Next () {
        if (explodeCount > 0) return this;

        if (exploded)
		{
            exploded = false;
			GM.ShowClippingBorad(true);
            return GM.dropPump;
        }

        cakes.Clear();

        foreach (Board bd in GM.Boards) {
            if ((bd.PND is CakePanelA) == false) continue;

            if ((bd.PND as CakePanelA).IsExplodable(bd.Panel)) {
                List<Board> otherCakes = (bd.PND as CakePanel).GetOtherCakes(bd.Panel);

                cakes.Add(bd);

                for (int j = 0; j < otherCakes.Count; j++) {
                    cakes.Add(otherCakes[j]);
                }

                exploded = true;
                break;
            }
        }

        if (exploded) {
            PerformPower();
            return this;
        } else {
            // ==> we don't use timebomb for now.
            //if (GM.PlayMoved) {
                // GM.PlayMoved = false;
            //    return GM.timebombPump;
            //} else {

                //return GM.colorChangerPump;

                if(GM.DestoryWaiterBursted) return GM.destroyAwaiterPump;
                else if(GM.PlayMoved)       return GM.colorChangerPump;    // GM.snowmanPump;      - go to next pump.
                else                        return GM.destroyAwaiterPump;  // GetNextPhaseOfEnd();    // - finalize pump.
            //}
        }
    }

    public override void ResetPump () {
        explodeCount = 0;
        exploded = false;
        cakes.Clear();
    }

	void PerformPower () {
        List<Board> processed = new List<Board>();

        processed.AddRange(cakes);

        for (int i = 0; i < cakes.Count; i++) {
            explodeCount++;
            cakes[i].Panel.Destroy(false, true, true, (success) => { explodeCount--; });
        }

        for (int i = 1; i < GameManager.WIDTH; i++) {
            for (int j = 0; j < cakes.Count; j++) {
                List<Board> targets = new List<Board>();
                List<Board> candidates = cakes[j].GetBoardsFromDistance(i);

                for (int k = 0; k < candidates.Count; k++) {
                    if (processed.Contains(candidates[k])) continue;
                    if (candidates[k].PND is CakePanel) continue;

                    targets.Add(candidates[k]);
                }

                for (int k = 0; k < targets.Count; k++) {
                    explodeCount++;
                    targets[k].Hit(0.05F * i, () => { explodeCount--; });
                }

                processed.AddRange(candidates);
            }
        }
    }
}

    */