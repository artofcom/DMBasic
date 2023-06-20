using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DG.Tweening;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;

#if !NN_DEPLOY
using System.IO;
#endif

public enum JMF_DIFFICULT { EASY, NORMAL, HARD }; 
public enum JMF_GAMESTATE { PENDING, PLAY, PAUSE, SHUFFLE, FINAL, BONUS, OVER };
public enum JMF_DIRECTION { NONE, UP, DOWN, LEFT, RIGHT, UPRIGHT, UPLEFT, DOWNLEFT, DOWNRIGHT };
public enum GAMEOVER_REASON { NONE, RESTART, QUIT, TIMER, MOVE, PENGUIN, TIMEBOMB, NO_MORE_TO_MATCH, AI_LOSE }  // + [AI_MISSION]
public enum FAIL_REASON { NONE, SCORE, SHADED, SNOWMAN, JEWEL, ICECREAM, SPECIAL_JEWEL, PENGUIN, YETI }
public enum SJ_COMBINE_TYPE { L, LL, LB, LR, B, BB, BR, R, RR, MAX_COUNT };

public class GameManager : MonoBehaviour {
    public const int WIDTH  = 9;
    public const int HEIGHT = 9;

    //static bool[] startItems = new bool[4];
	static List<string> _listStartBoostId = new List<string>();
    static string _strItemId4Continue;
    public static System.Action<bool> OnEnableFrogJump;
	//public SpineBoard spineBoard;
	
    public PlayOverlayHandler playOverlayHandler;
    public PlaySceneHandler playSceneHandler;
	public GameObject pieceManager;
	public GameObject panelManager;
    // note : data index tag와 matching order data의 분리 필요성 대두.
    //        현재는 pieceManager의 index 역순으로 matching order가 진행된다. 그러나 개발 중에 새 piece가 추가되면 index가 
    //        꼬이게 되고, 이 index는 data와 연결되어 있으므로, 기존 data를 신뢰할수 없게 된다. 따라서 matching order만을 위한 
    //        array data를 별도로 추가하여 order로만 사용하고, 기존의 index data는 data indexing 용으로만 사용한다. 
    //        이렇게 하면 추가되는 piece는 index가 뒤로 추가될 뿐이고(꼬이지 않게 되고), 기존 index는 data tag로써 문제가 없게 된다. 
    //        170214 by wayne.
    public GameObject matchOrderManager;
    public PieceDefinition[] MatchOrders { get; private set; }
    //
	public int matchScore = 60;
	public int shadedScore = 1060;
	public int bonusScore = 3000;
    public int timesPerBonus = 1;
    public int movesPerBonus = 1;
    public float switchSpeed = 0.5F;
	public float idleTimer = 7F;
	public float idleLimit = 3F;
    public float gravity = 150F;
    public float startVelocity = 10F;     
    //public float maxVelocity = 30F; 
    public float maxVelocity = 1000F; 
    public float delayBeforeMatch = 0.1F;
    //
    public bool  makingSpecialPiece = false;
    [HideInInspector]               // note : makingSpecialPiece를 서서히 cntMakingSpecialPiece로 교체하자.
    public int mCntMakingSpecialPiece= 0;
    //
    public bool useJellyBounce = true;
    public float dropAccRate = 0.006f;
    public float dropAccWeight = 1.0f;
    public int jellyBounceCount = 10;
    public int animationCount = 0;
    public bool bossEntry = false;
    public bool isBossDeadEffect = false;
	public int bossSpecialAttackDamage = 2;

    public float delayForMakingSpecialPiece = 0.5f;

    // Phase delay.
    public float dropPumpDelay          = 0.1f;
    public float matchPumpDelay         = 0.1f;
    public float burstSequenceDelay     = 0.15f;
    public float slideTime              = 0.2f;
    public float initDropSpeed          = 0.2f;
    public float spawnDropDelay         = 0.05f;

    [Range(0,1)]
    public float sloshingRatio = 0.1F;
    [Range(0,1)] 
    public float bounceRatio = 0.05F;
    public float bounceDuration = 0.1F;

    int mapIdForRatePopup       = 50;

    [Range(0,100)] 
    public List<int> probability = new List<int>(){100, 100, 100, 100, 100, 100, 100, 100};
	public readonly int[] JewelMatchCounts = new int[ (int)LEItem.COLOR.NORMAL_COUNT ];
    public readonly int[] SpecialMatchCounts = new int[(int)SJ_COMBINE_TYPE.MAX_COUNT];
	
    // [ROUND_CHOCO]
    [HideInInspector] public int countMatchRoundChocho   = 0;
    [HideInInspector] public int countMatchJamBottom     = 0;   
    [HideInInspector] public int countMatchRectChocho    = 0;
    [HideInInspector] public int countMatchCottonCandy   = 0; 
    [HideInInspector] public int countMatchSodaCan       = 0;
    [HideInInspector] public int countMatchSugarBlock    = 0;  
    [HideInInspector] public int countMatchZellatto      = 0;    
    [HideInInspector] public int countMatchPotion1       = 0;     
    [HideInInspector] public int countMatchPotion2       = 0;     
    [HideInInspector] public int countMatchPotion3       = 0;     
    [HideInInspector] public int countMatchChocoBar      = 0;    
    [HideInInspector] public int countMatchCursedBottom  = 0;
    [HideInInspector] public int countMatchAiWinBoard    = 0;  
    [HideInInspector] public int countMatchCookieJelly   = 0;  
    [HideInInspector] public int countMatchColorBox      = 0;  
    [HideInInspector] public int countMatchWaffleCooker  = 0;  
    [HideInInspector] public int countMatchMudShade     = 0;  
    //

    // [CONVEYOR=>RIVER]
    [HideInInspector] public SpriteRenderer  _sprRiverWaffle;
    //

    [HideInInspector] public bool _isRemovedByConveyor  = false;
    [HideInInspector] public bool DestoryWaiterBursted  = false;  
    [HideInInspector] public bool itemUsing             = false;    // flag for item usage check when ai'mode.
    [HideInInspector] public bool isShuffleItemTriggered= false;

    [HideInInspector] public bool isLevelClearByBuff    = false;

    bool enabledPump;
    bool isJumpingFrog;
    //Tween suggestTween;
	//GameObject suggestedPiece;
	List<Board> suggestedPieces = new List<Board>();
    bool _isSuggestByCombine    = false;
	Board moverSuggested;
	Board targetSuggested;
	
	Vector3 OriginalPosition;
	Vector3 pieceOriginalSize;
	Vector3 pieceOriginalSizeFixed;
    List<Board> boards = new List<Board>();
    List<int> colorIndices = new List<int>();
    //List<GameObject> triangles = new List<GameObject>();
    Dictionary<Board, Board> portalDict = new Dictionary<Board, Board>();
    Dictionary<string, MonoBehaviour> typeDict = new Dictionary<string, MonoBehaviour>();
#if DEV_MODE && UNITY_EDITOR
    Dictionary<Board, Color> gizmoDict = new Dictionary<Board, Color>();
#endif

    Point boardMin = new Point(0, 0);
	Point boardMax = new Point(0,0);
	
    bool _isCharPopFinished     = false;
    public bool IsCharPopFinished() { return _isCharPopFinished; }

    int _idxBdTopY              = -1;
    public int getTopBoardIndex() { return _idxBdTopY; }

    [HideInInspector]           // spine board 에서 실제 그려지는 보드 목록.
    public List<Board> _listOnBoard= new List<Board>();

    [HideInInspector]           // fence list.
    public List<JMF_DIRECTION> _listFence= new List<JMF_DIRECTION>();
    List<SpriteImage>           _listSpriteFence= new List<SpriteImage>();

    [HideInInspector]
    public int _countMudCoverDestroyed  = 0;

    bool _onDestroyingAllWaffles= false;

    [HideInInspector]
    public List<Board> BlockMissionPanels   = new List<Board>();

    // when rainbow or bonus mass matching !
    bool mActMassMatching       = false;
    public bool ActMassMatching
    {
        get { return mActMassMatching; }
        set
        {
            mActMassMatching    = value;
        }
    }

	public Point BoardMin
	{
		get{ return boardMin;}
	}
	
	public Point BoardMax
	{
		get{ return boardMax;}
	}
	
	public Vector3 MovingPosition
	{
		get
		{
			float XX = (float)(boardMin.X) + (float)((boardMax.X - boardMin.X) * 0.5f - 1.26f);
			float YY = (float)(boardMin.Y) + (float)((boardMax.Y - boardMin.Y) * 0.5f - 2.7f);//  1.35f);//  0.2f);
			
			Vector3 pos = OriginalPosition;
			pos.x -= XX* Size; 
			pos.y -= YY* Size;
			return pos;
		}
	}
	
//#if BOMBx2
    List<GamePiece> destroyAwaiter = new List<GamePiece>();
    public List<GamePiece> DestroyAwaiter { get { return destroyAwaiter; } }
//#endif

    // [AI_MISSION]
    int countMyWinBoard                 = 0;
    int countEnemyWinBoard              = 0;
    public int CountMyWinBoard
    {
        private set { countMyWinBoard = value; }
        get
        {
            int ret             = 0;
            for (int y = HEIGHT - 1; y >= 0; y--) {
                for (int x = 0; x < WIDTH; x++) {
                    if(Board.AI_SIDE.MINE == this[x,y].AiSide)
                        ++ret;
                }
            }
            countMyWinBoard     = ret;
            return ret;
        }
    }
    public int CountEnemyWinBoard
    {
        private set { countEnemyWinBoard = value; }
        get
        {
            int ret             = 0;
            for (int y = HEIGHT - 1; y >= 0; y--) {
                for (int x = 0; x < WIDTH; x++) {
                    if(Board.AI_SIDE.ENEMIES == this[x,y].AiSide)
                        ++ret;
                }
            }
            countEnemyWinBoard  = ret;
            return ret;
        }
    }
    public bool isCurPlayerAI           = false;
    public bool bStartAiFight           = false;
    public bool isRainbowCreated        = false;
    public bool isAIFightMode
    {
        get { return (EditWinningConditions.MISSION_TYPE.DEFEAT == (EditWinningConditions.MISSION_TYPE)CurrentLevel.missionType); }
    }
    int _nAiMoveWaited                  = -1; // -1 none, 0 - man, 1 - ai
    public int getAiMoveWaited()        { return _nAiMoveWaited; }
    //

    [HideInInspector]   public  int PrevWebAttackMove   = 0;
    [HideInInspector]   public  int PrevIceAttackMove   = 0;
    [HideInInspector]   public  int PrevStoneAttackMove = 0;

    // [2X2_BURST]
    bool _switchDirectionV      = false;    // true-vertical, false-horize.
    public bool SwitchDirectionV{ get { return _switchDirectionV; } }

    GamePump pump;
    public GamePump Pump {
        get { return pump; }
        set {
            /*   
            if ((value != null) && (value != pump)) {
                Debug.Log(value.State);
            }
            */

            pump = value;
        }    
    }  
    
    public IdlePump idlePump = new IdlePump();
    public WaitStablePump waitStablePump = new WaitStablePump();
    public ShufflePump shufflePump = new ShufflePump();
    public DropPump dropPump = new DropPump();
    public MatchPump matchPump = new MatchPump();
    public CakePump cakePump = new CakePump();
    public TimebombPump timebombPump = new TimebombPump();
    public SnowmanPump snowmanPump = new SnowmanPump();
    public ColorChangerPump colorChangerPump = new ColorChangerPump();
    public ConveyorPump conveyorPump = new ConveyorPump();
    public DoughPump doughPump = new DoughPump();
    public MudCoverPump mudCoverPump = new MudCoverPump();
    public ChameleonPump chameleonPump = new ChameleonPump();
    public PenguinJumpPump penguinJumpPump = new PenguinJumpPump();
    public PenguinSpawnPump penguinSpawnPump = new PenguinSpawnPump();
    public YetiPump yetiPump = new YetiPump();
    public LauncherIcePump launcherIcePump = new LauncherIcePump();
    public LauncherStonePump launcherStonePump = new LauncherStonePump();
    public LauncherWebPump launcherWebPump = new LauncherWebPump();
    public BossPump bossPump = new BossPump();
    public ClearPump clearPump = new ClearPump();
    public FailPump failPump = new FailPump();
    public AiPump aiPump = new AiPump();    // [AI_MISSION]
    public DestroyAwaiterPump destroyAwaiterPump    = new DestroyAwaiterPump();

#if !NN_DEPLOY
    Stack<GameStateInfo> history = new Stack<GameStateInfo>();

    public SolverIdle _SolverIdle;
    public SolverSearch _SolverSearch;
    public static bool isSolverMode;
    public static int solverSeed;
    public static int solverRetryCount;
    public static string solverFileName = "default";
    int solverPlayCount;
    int solverRemainMove;
    public Solver solver;
#endif
    bool playMoved = false;
    public bool PlayMoved { 
        get { return playMoved; } 
        set {
            /*   
            if (value != playMoved) {
                Debug.Log("PlayMoved" + value);
            }
            */

            playMoved = value;
        }    
    }
    public int Seed { get; private set; }
    public bool ExplodedBomb { get; set; }
	public int CountinueCount { get; set; }
    public bool EscapedPenguin { get; set; }
	public int PenguinMatchCount { get; set; }
	public int FairyMatchCount { get; set; }
	public int ComboCount { get; set; }
	public float PlayTime { get; private set; }
	public int ExtraMoves { get; private set; }
	public float ExtraTimes { get; private set; }
    public int ExtraMinusAiPoint { get; private set; }
    public FAIL_REASON FailReason { get; private set; }
    public Data.Level CurrentLevel { get; private set; }
    public int Potion1CollectCount { get; private set; }    
    public int Potion2CollectCount { get; private set; }
    public int Potion3CollectCount { get; private set; }
	public PieceDefinition[] PieceTypes { get; private set; }
	public PanelDefinition[] PanelTypes { get; private set; }
    public GAMEOVER_REASON GameoverReason { get; private set; }
    public int ColorCount { get { return colorIndices.Count; } }
    public IEnumerable Boards { get { return boards as IEnumerable; } }
    //public float Size { get { return (Camera.main.aspect > 0.7F) ? 1.15F : 1.47F; } }
	//public float Size { get { return (Camera.main.aspect > 0.7F) ? 1.15F : 1.48F; } }
	public float Size { get { return  1.48F; } }
    public float GivenTime { get { return CurrentLevel.givenTime + ExtraTimes; } }
    public int AllowedMoves { get { return CurrentLevel.allowedMoves + ExtraMoves; } }
    public ReadOnlyCollection<int> ColorIndices { get { return colorIndices.AsReadOnly(); } }
	public int TreasuresCollectCount { get { return Potion1CollectCount + Potion2CollectCount + Potion3CollectCount; } }
	public int TotalRequireTreasureCount { get { return CurrentLevel.countPotion1 + CurrentLevel.countPotion2 + CurrentLevel.countPotion3; } }

    public int ShadedCount {
        get {
            int shadedCount = 0;

            for (int i = 0; i < boards.Count; i++) {
                if (this[i].ShadedDurability > -1) shadedCount++;
            }

            return shadedCount;
        }
    }

    public bool IsCollectedAllJewel {
        get {
            Debugger.Assert(CurrentLevel != null, "GameManager.IsCollectedAllJewel : Level is null.");

            bool collected = true;

            for (int i = 0; i < (int)LEItem.COLOR.NORMAL_COUNT; i++) {
                if (CurrentLevel.numToGet[i] <= 0) continue;

                int remainCount = CurrentLevel.numToGet[i] - JewelMatchCounts[i];

                if (remainCount > 0) {
                    collected = false;
                    break;
                }
            }

            return collected;
        }
    }

    public bool IsCollectedAllSpecialJewel {
        get {
            Debugger.Assert(CurrentLevel != null, "GameManager.IsCollectedAllSpecialJewel : Level is null.");

            bool collected = true;

            for (int i = 0; i < CurrentLevel.specialJewels.Length; i++) {
                if (CurrentLevel.specialJewels[i] <= 0) continue;

                int remainCount = CurrentLevel.specialJewels[i] - SpecialMatchCounts[i];

                if (remainCount > 0) {
                    collected = false;
                    break;
                }
            }

            return collected;
        }
    }

    /*public bool IsFinishDropAndMatch {
        get {
            for (int i = 0; i < boards.Count; i++){
                if (this[i].State != Board.STATE.STABLE) return false;
                if (this[i].IsNeedMatchCheck)   return false;
                if (this[i].IsNeedDropCheck)    return false;
            }

            return true;
        }
    }*/

    public bool IsAllBoardStable {
        get {
            for (int i = 0; i < boards.Count; i++){
                if (this[i].State != Board.STATE.STABLE)
                    return false;
            }

            return true;
        }
    }

    public JMF_DIFFICULT Difficult {
        get {
            if (colorIndices.Count == (int)LEItem.COLOR.NORMAL_COUNT) {
                return JMF_DIFFICULT.HARD;
            } else if (colorIndices.Count == (int)LEItem.COLOR.NORMAL_COUNT-1) {
                return JMF_DIFFICULT.NORMAL;
            } else {
                return JMF_DIFFICULT.EASY;
            }
        }
    }

    long score;
	public long Score { 
        get { return score; }
        private set {
            if (score == value) return;
            score = value;
            JMFRelay.FireOnChangeScore(score);
        }
    }

	int moves;
	public int Moves { 
        get { return moves; }
        private set {
            if (moves == value) return;
            moves = value;
        
            if (CurrentLevel.isTimerGame==false && CurrentLevel.allowedMoves>0) {
                JMFRelay.FireOnChangeRemainMove(AllowedMoves - moves);
            }
        }
    }

    bool canMove;
	public bool CanMove { 
        get { 
            if (State != JMF_GAMESTATE.PLAY) return false;
            if (IsAllBoardStable == false) return false;
            return canMove; 
        }

        set {
            if (canMove == value) return;

            canMove = value;

//            if (canMove == false)
//                StopSuggestPiece();
        }
    }

    JMF_GAMESTATE state;
	public JMF_GAMESTATE State { 
        get { return state; }
        set { 
            if (state == value) return;
            Debug.Log(string.Format("STATE CHANGED : {0} => {1}", state, value));
            state = value;
        }
    }
    
    bool isStable;
    public bool IsStable {
        get { return isStable; }
        set {
            if (isStable == value) return;
            isStable = value;
        }
    }

    int yetiHealth;
    public int YetiHealth {
        get { return yetiHealth; }
        set {
            if (yetiHealth == value) return;
            yetiHealth = value;
            JMFRelay.FireOnChangeYetiHealth(yetiHealth);
        }
    }

    int bossHealth;
    public int BossHealth {
        get { return bossHealth; }
        set {
            if (bossHealth == value) return;
            int damage = value - bossHealth;
            bossHealth = value;
            JMFRelay.FireOnChangeBossHealth(bossHealth, damage);
        }
    }

    Board readiedFrog;
    public Board ReadiedFrog {
        get { return readiedFrog; }
        set {
            readiedFrog = value;

            if (readiedFrog == null) {
                CanMove = true;
            } else {
                CanMove = false;
            }

            if (OnEnableFrogJump != null) OnEnableFrogJump(readiedFrog != null);
        }
    }

    public Board this [Point pt] { get { return this[pt.X,pt.Y]; } }

    public Board this [int x, int y] { 
        get { 
            if (((x >= 0) && (x < WIDTH)) && ((y >= 0) && (y < HEIGHT))) {
                return boards[(y*WIDTH)+x]; 
            } else {
                return null;
            }
        } 
    }

    public Board this [int index] { 
        get { 
            Debugger.Assert((index >= 0) && (index < boards.Count), 
                "GameManager : Out of range of board index.");
            return boards[index];
        } 
    }
	
	void Awake () { 
		//Debugger.Assert(spineBoard != null);
		
		JMFUtils.GM = this;
		
		PieceTypes = pieceManager.GetComponents<PieceDefinition>();
		PanelTypes = panelManager.GetComponents<PanelDefinition>();

        PieceDefinition[] arrMatchOrder = matchOrderManager.GetComponents<PieceDefinition>();
        // re-matching.
        MatchOrders             = new PieceDefinition[ arrMatchOrder.Length ];
        for(int q = 0; q < arrMatchOrder.Length; ++q)
        {
            MatchOrders[q]      = null;
            for(int gg = 0; gg < PieceTypes.Length; ++gg)
            {
                if(PieceTypes[gg].ClassName == arrMatchOrder[q].ClassName)
                {
                    MatchOrders[q]= PieceTypes[gg];
                    break;
                }
            }
            Debug.Assert( MatchOrders[q]!=null, "Match Order re-matching Error !!!");
        }
		OriginalPosition = transform.localPosition;
		
		InitTypeDictionary();

        for (int y = 0; y < HEIGHT; y++)
		{
            for (int x = 0; x < WIDTH; x++) 
			{
                //Vector3 pos = new Vector3(x-(WIDTH/2F)+0.5F, y-(HEIGHT/2F)+0.5F, 0F);
				Vector3 pos = new Vector3(x, y, 0F);
                Board bd = new Board(this, new Point(x, y), pos*Size);
                boards.Add(bd);

                _listFence.Add( JMF_DIRECTION.NONE );
            }
        }

        foreach (Board bd in boards) bd.UpdateNeighbours();

        float timeinvoke        = 0.2f;
#if UNITY_EDITOR 
        timeinvoke              = 0.1f;
#endif
        InvokeRepeating( "_abandonPoolGabages", 1.0f, timeinvoke );
	} 

    void OnDrawGizmos ()
    {
#if DEV_MODE && UNITY_EDITOR
        foreach (Board bd in gizmoDict.Keys)
        {
            Gizmos.color = gizmoDict[bd];
            Gizmos.DrawWireCube(bd.Position, Vector3.one * (Size - 0.35F));
        
            if (bd.IsNeedMatchCheck) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(bd.Position, (Size - 0.1F) * 0.5F);
            }
        }
#endif
    }

    #region ## Pump에 의한 Board state 복구 로직.

    [HideInInspector]
    public bool _hasPumpError   = false;
    List<Board.STATE>           _listBoardState4PumpError = new List<Board.STATE>();
    List<float> _listBoardYPos  = new List<float>();
    int _checkPumpState         = 0; // 0 idle, 1 checking, 2 finished.
    public int getPumpCheckState() { return _checkPumpState; }

    // 관련 로직 리셋.
    public void resetPumpCheck(bool resetWithBoardState=true)
    {
        if(resetWithBoardState)
        {
            for(int q = 0; q < boards.Count; ++q)        
                boards[q].UnstableCount = 0;
        }
        
        triggerPumpErrorCheck(false);
        _hasPumpError           = false;
    }
    public void triggerPumpErrorCheck(bool start)
    {
        if(start)
        {
            if(0 == _checkPumpState)
                StartCoroutine( "coCheckPumpError" );
        }
        else
        {
            StopCoroutine( "coCheckPumpError" );
            _checkPumpState     = 0;            
        }
    }
    IEnumerator coCheckPumpError()
    {
        if(_hasPumpError)       yield break;
        if(0!=_checkPumpState)  yield break;
        _checkPumpState         = 1;

        //Debug.Log("# Start Checking Pump State......");

        _hasPumpError           = false;
        _listBoardState4PumpError.Clear();
        _listBoardYPos.Clear();
        for(int q = 0; q < boards.Count; ++q)
        {
            _listBoardState4PumpError.Add( boards[q].State );
            _listBoardYPos.Add( null!=boards[q].Piece ? boards[q].Piece.LocalPosition.y : .0f );
        }
        
        yield return new WaitForSeconds( 1.0f );

        _hasPumpError           = true;
        for(int q = 0; q < _listBoardState4PumpError.Count; ++q)
        {
            if(_listBoardState4PumpError[q]!=boards[q].State || (null!=boards[q].Piece && boards[q].Piece.LocalPosition.y!=_listBoardYPos[q]))
            {
                _hasPumpError   = false;
                break;
            }
        }

        //Debug.Log("# Closing Checking Pump State......");
        _checkPumpState         = 2;
    }
    #endregion

    void Update () 
	{
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1)) Score = CurrentLevel.scoreToReach[0];
        if (Input.GetKeyDown(KeyCode.Alpha2)) Score = CurrentLevel.scoreToReach[1];
        if (Input.GetKeyDown(KeyCode.Alpha3)) Score = CurrentLevel.scoreToReach[2];
		if (Input.GetKeyDown(KeyCode.Alpha9)) Wallet.Gain("coin", 100);
        if (Input.GetKeyDown(KeyCode.Alpha0)) Moves = AllowedMoves - 1;
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            if (Time.timeScale > 1F) {
                Time.timeScale = 1F;                
            } else if (Time.timeScale < 2F) {
                Time.timeScale = 0.5F;
            } else {
                Time.timeScale -= 1F;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightBracket)) {
            if (Time.timeScale == 1F) {
                Time.timeScale = 10F;
            } else if (Time.timeScale < 1F) {
                Time.timeScale = 1F;
            } else {
                Time.timeScale += 1F;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetKeyUp(KeyCode.C)) StartCoroutine(CoCaptureAllLevel());
#if !NN_DEPLOY
            if (Input.GetKeyUp(KeyCode.R)) Rollback();

            if (Input.GetKeyUp(KeyCode.S)) {
                if (isSolverMode) {
                    Debug.Log("SOLVER MODE : OFF");
                    solver = null;
                    isSolverMode = false;
                } else {
                    if (IsStable) {
                        Debug.Log("SOLVER MODE : ON");
                        _SolverIdle.Reset();
                        _SolverSearch.Reset();
                        solver = _SolverIdle.Solve();
                        isSolverMode = true;
                    }
                }
            }

#endif
        } else {
            if (Input.GetKeyDown(KeyCode.C)) {
                TESTMODE_clearLevel();
            }
            else if(Input.GetKey(KeyCode.X))
            {
                if(State == JMF_GAMESTATE.PLAY)
                    EndGame(GAMEOVER_REASON.MOVE, false, 0);
            }
        }
#endif

        // reset tween's timescale.
        DOTween.timeScale       = Time.timeScale;

        // [MAKE_DROP_ACC]
        _onUpdateDropCells();
    }

    public void TESTMODE_clearLevel()
    {
#if DEV_MODE || UNITY_EDITOR
        Score                   = CurrentLevel.scoreToReach[2];
        int clearScore          = (int) Score;
        this.State              = JMF_GAMESTATE.OVER;
        JMFRelay.FireOnGameOver(true, 3, clearScore);
#endif
    }

    #region // [MAKE_DROP_ACC]
    class compareByY : IComparer<GamePiece>
    {
        public int Compare(GamePiece a, GamePiece b)
        {
            return (int)(100.0f * (a.Position.y-b.Position.y));
        }
    }
    public List<GamePiece> _listDropPieces  = new List<GamePiece>();
    public List<GamePiece> _listDeletables  = new List<GamePiece>();
    void _onUpdateDropCells()
    {
        for (int a = 0; a < _listDropPieces.Count; ++a)
            _listDropPieces[a].onMoveUpdate();
        
        if(_listDeletables.Count > 0)
        {
            for(int q = 0; q < _listDeletables.Count; ++q)
                _listDropPieces.Remove( _listDeletables[q] );
            _listDeletables.Clear();
        }
    }
    #endregion

    void OnEnable () { 
        JMFRelay.OnClickPanel += OnClickPanel;        
    } 

    void OnDisable () { 
        JMFRelay.OnClickPanel -= OnClickPanel;   
        State                   = JMF_GAMESTATE.OVER;

        removeAll();
    } 

    // Abandon all Resources.
    void removeAll()
    {
        for (int y = HEIGHT - 1; y >= 0; y--) 
		{
            for (int x = 0; x < WIDTH; x++) 
			{
                Board bd = this[x,y];
                bd.ResetBoard();
            }
        }

        for(int q = 0; q < _listSpriteFence.Count; ++q)
            NNPool.Abandon( _listSpriteFence[q].gameObject );
        _listSpriteFence.Clear();
    }

    public void Reset (Data.Level level, bool forScreenShot = false) 
	{
#if !NN_DEPLOY
        if (isSolverMode)
            NNTool.Seed = solverSeed;
#endif
        if (forScreenShot == false) StopAllCoroutines();
        CurrentLevel = level;
        colorIndices = GetColorIndices();

        if(true == isAIFightMode)
            CurrentLevel.allowedMoves   = 0;

		transform.localPosition = OriginalPosition;
		
	    Score = 0;
	    Moves = 0;
        ExtraMoves = 0;
        ExtraTimes = 0F;
        PlayTime = 0F;
	    ComboCount = 1;
		CanMove = false;
        IsStable = false;
        Seed = NNTool.Seed;
        enabledPump = false;
	    Potion1CollectCount = 0;
        Potion2CollectCount = 0;
        Potion3CollectCount = 0;
        makingSpecialPiece = false;
        bossEntry = false;
        isBossDeadEffect = false;
        _isCharPopFinished      = false;
        _countMudCoverDestroyed = 0;
        resetPumpCheck();

        isLevelClearByBuff      = false;

        for (int i = 0; i < JewelMatchCounts.Length; i++)
		{
            JewelMatchCounts[i] = 0;
        }

        for (int i = 0; i < SpecialMatchCounts.Length; i++) 
		{
            SpecialMatchCounts[i] = 0;
        }

        // fence reset.
        for(int q = 0; q < _listSpriteFence.Count; ++q)
            NNPool.Abandon( _listSpriteFence[q].gameObject );
        _listSpriteFence.Clear();

        ExplodedBomb = false;
        EscapedPenguin = false;
        PenguinMatchCount = 0;
        FairyMatchCount = 0;
        if (Boss.Current != null) Boss.Current.Recycle();
        if (Yeti.Current != null) Yeti.Current.Recycle();

		SetupBoard(CurrentLevel);
		
        Dictionary<Board, List<Board>> dicRet = GetSuggestablePieces(ref suggestedPieces, false);
        
		
        idlePump.ResetPump();
        waitStablePump.ResetPump();
        shufflePump.ResetPump();
        dropPump.ResetPump();
        matchPump.ResetPump();
        cakePump.ResetPump();
        timebombPump.ResetPump();
        snowmanPump.ResetPump();
        colorChangerPump.ResetPump();
        conveyorPump.ResetPump();
        doughPump.ResetPump();
        mudCoverPump.ResetPump();
        chameleonPump.ResetPump();
        penguinJumpPump.ResetPump();
        penguinSpawnPump.ResetPump();
        yetiPump.ResetPump();
        launcherIcePump.ResetPump();
        launcherStonePump.ResetPump();
        launcherWebPump.ResetPump();
        bossPump.ResetPump();
        clearPump.ResetPump();
        failPump.ResetPump();
        aiPump.ResetPump();
        destroyAwaiterPump.ResetPump();

        // test by wayne.
        _listDeletables.Clear();
        _listDropPieces.Clear();

        // [AI_MISSION]
        countMyWinBoard         = 0;
        countEnemyWinBoard      = 0;
        isCurPlayerAI           = false;
        bStartAiFight           = false;
        isRainbowCreated        = false;

        // [ROUND_CHOCO]
        countMatchRoundChocho   = 0;
        countMatchJamBottom     = 0; 
        countMatchRectChocho    = 0;
        countMatchCottonCandy   = 0; 
        countMatchSodaCan       = 0;
        countMatchSugarBlock    = 0; 
        countMatchZellatto      = 0; 
        countMatchPotion1       = 0; 
        countMatchPotion2       = 0; 
        countMatchPotion3       = 0; 
        countMatchChocoBar      = 0; 
        countMatchCursedBottom  = 0;
        countMatchAiWinBoard    = 0; 
        countMatchCookieJelly   = 0;
        countMatchColorBox      = 0;
        countMatchWaffleCooker  = 0;
        DestoryWaiterBursted    = false;
        itemUsing               = false;

        PrevIceAttackMove       = 0;
        PrevStoneAttackMove     = 0;
        PrevWebAttackMove       = 0;

        //JMFRelay.FireOnGameReady();

        isShuffleItemTriggered  = false;

        ActMassMatching         = false;
        _nAiMoveWaited          = -1;

        _onDestroyingAllWaffles = false;

		ShowClippingBorad(true);
        pump = dropPump;

		State = JMF_GAMESTATE.PENDING;

        launcherIcePump.setupBoard();
        launcherStonePump.setupBoard();
        launcherWebPump.setupBoard();

        //_charAni_start();
        //playSceneHandler._sprBGFilter.color = new Color(0.33f, .0f, 0.305f, .0f);
        //playSceneHandler._sprBGFilter.gameObject.SetActive(false);        

        StopCoroutine( "_coUpdateSugarPiece" );
        StartCoroutine( "_coUpdateSugarPiece" );

#if !NN_DEPLOY
        _SolverIdle = new SolverIdle(this);
        _SolverSearch = new SolverSearch(this);
        solver = null;
        history.Clear();
#endif
	} 

	public void StartGame () {
        Debugger.Assert(State == JMF_GAMESTATE.PENDING, "GameManager.StartGame : State is not 'PENDING'.");

        AllocateStartItems();

        StartCoroutine(CoPump());

        if (CurrentLevel.isTimerGame) {
            InvokeRepeating("Timer", 1F, 1F);
        }

        JMFRelay.FireOnGameStart();

        State = JMF_GAMESTATE.PLAY;
        CanMove = true;

        //
        //string idTutorial       = TutorialOverlayHandler.GetAvailable(CurrentLevel.Index+1, "");
        //string strId            = TutorialOverlayHandler.getActiveTutorialId();        
        //if(isAIFightMode && null==strId && null==idTutorial)
        //    this.fire_AiTurn(true, false);

#if !NN_DEPLOY
        if (isSolverMode) {
            solverPlayCount++;
            solver = _SolverIdle.Solve();
        }
#endif        
	}

    public void EndGame (GAMEOVER_REASON reason)
    {
        CanMove = false;
        StopAllCoroutines();
        CancelInvoke("Timer");
        GameoverReason = reason;
        State = JMF_GAMESTATE.OVER;

        //_charAni_close();
    }

	void EndGame (GAMEOVER_REASON reason, bool isWin, System.Byte grade) 
	{
        EndGame(reason);
#if NN_DEPLOY
        JMFRelay.FireOnGameOver(isWin, grade, (int)Score);
#else 
        if (isSolverMode) {
            SaveSolverLog(reason, isWin, grade);
            if (solverPlayCount < solverRetryCount) {
            //    DWATTHelper.endStageEvent(false);
                Scene.ChangeTo("PlayScene", CurrentLevel);
            } else {
//                NativeInterface.ApplicationQuit();
            }
        } else {
            JMFRelay.FireOnGameOver(isWin, grade, (int)Score);
        }
#endif
    }

#if !NN_DEPLOY
    void SaveSolverLog(GAMEOVER_REASON reason, bool isWin, int grade) 
	{
        string mode = GetModeName();
		int levelIndex = Root.Data.currentLevel.Index;
        string logInfo = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", levelIndex, solverSeed, AllowedMoves, solverRemainMove, Score, mode, reason, Root.GetPostfix());
        string filePath = Application.dataPath + "/../../SolverLog.csv";
        StreamWriter streamWriter = new StreamWriter(filePath, true);
        streamWriter.WriteLine(logInfo);
        streamWriter.Close();
    }

	string GetModeName() 
	{
		string mode = "";
		if (CurrentLevel.isClearShadedGame) {
			mode = "ICE";
		} else if (CurrentLevel.isFairyGame) {
			mode = "FAIRY";
		} else if (CurrentLevel.isClearChocolateGame) {
			mode = "SNOWMAN";
		} else if (CurrentLevel.isGetTypesGame) {
			mode = "JEWEL";
		} else if (CurrentLevel.isTreasureGame) {
			mode = "INGREDIENT";
		} else if (CurrentLevel.isSpecialJewelGame) {
			mode = "SPECIAL_JEWEL";
		} else if (CurrentLevel.isPenguinGame) {
			mode = "PENGUIN";
		} else if (CurrentLevel.isYetiGame) {
			mode = "YETI";
		} else if (CurrentLevel.isTimerGame) {
			mode = "TIMER";
		} else {
			mode = "MOVE";
		}
		return mode;
	}
#endif

    public void ContinueGame () 
    {
        Debugger.Assert(State == JMF_GAMESTATE.OVER, "GameManager.ContinueGame : State is not 'OVER'.");

        enabledPump = false;
        ExplodedBomb = false;
        EscapedPenguin = false;

        if (CurrentLevel.isTimerGame) {
            InvokeRepeating("Timer", 1F, 1F);
        } else {
            ;
        }

        if(true==isCurPlayerAI && 0!=ExtraMinusAiPoint)
            _onContinueAiPoint();
        
        AllocateStartItems(true);

        State = JMF_GAMESTATE.PLAY;

        // pre-select item if exist.
        if(null!=_strItemId4Continue && _strItemId4Continue.Length>0)
            playOverlayHandler.preSelectItemButton(_strItemId4Continue);
        _strItemId4Continue     = "";

		ShowClippingBorad(true);
        pump = dropPump;        
        CanMove = true;

        StartCoroutine(CoPump());
    }

    public void Pause () {
        State = JMF_GAMESTATE.PAUSE;
        CanMove = false;
    }

    public void Resume () {
        State = JMF_GAMESTATE.PLAY;
        CanMove = true;
    }

    // supports only normal mode !!!
    public bool isDragable()
    {
        if(isAIFightMode)
        {
            Debug.Assert(false, "This function doesn't support ai mode.");
            return false;
        }
        return (Pump==idlePump && DestroyAwaiter.Count==0);                
    }

	public void DragFromHere (Point pt1, Point pt2)
	{
        // [AI_MISSION]
        if (isAIFightMode)
        {
            string strId            = TutorialOverlayHandler.getActiveTutorialId();
            if(null==strId && _nAiMoveWaited < 0)       return;
            if(0==_nAiMoveWaited && isCurPlayerAI)      return;
            if(1==_nAiMoveWaited && !isCurPlayerAI)     return;            
            if(false==isCurPlayerAI && Pump!=idlePump)  return;

            bStartAiFight       = true;
            _nAiMoveWaited      = -1;
        }
        else
        {
            if(false == isDragable())
                return;
        }
		
        Board bdA = this[pt1];
        Board bdB = this[pt2];

        // if (CurrentLevel.isBossGame && bossEntry == false) return; - we dont use boss mode anymore.
		if (CanMove == false) return;
#if !NN_DEPLOY
        if ((CurrentLevel.isTimerGame == false) && (CurrentLevel.allowedMoves>0) && (Moves >= AllowedMoves) && (isSolverMode == false)) return;
#else
        if ((CurrentLevel.isTimerGame == false) && (CurrentLevel.allowedMoves>0) && (Moves >= AllowedMoves)) return;
#endif
		
		if (playOverlayHandler.Equipment==GAME_ITEM.MAGICSWAP && Wallet.GetItemCount("magicswap")>0)
		{
            // 매칭 & 조합 가능하면 item 사용 불가.
            bool canBeMatched   = false;
            if(bdA.PD.IsCombinable(bdB.Piece) || bdB.PD.IsCombinable( bdA.Piece))
                canBeMatched    = true;
            List<Board> listOutBoards   = new List<Board>();
            if (bdB.Piece.IsMatchable() && CanBeMatched(bdA, bdB.ColorIndex, bdA.GetNeighborDirection(bdB), ref listOutBoards))
                canBeMatched    = true;
            if (bdA.Piece.IsMatchable() && CanBeMatched(bdB, bdA.ColorIndex, bdB.GetNeighborDirection(bdA), ref listOutBoards))
                canBeMatched    = true;
            if (true == canBeMatched)
            {
                playOverlayHandler.Equipment = GAME_ITEM.NONE;
                return;
            }
            //
           
            // 이동 불가하면 item 사용 불가.
            if(false == isPassableThroughFence(bdA, bdB))
            {
                playOverlayHandler.Equipment = GAME_ITEM.NONE;
                return;
            }

            itemUsing           = true;
            NNSoundHelper.Play("IFX_block_swap");

            // note : 반드시 소문자 moves를 -1 차감해야 ui update가 없다 !
            moves--;
            //
            SwapPieceWithItem(pt1, pt2);

            playOverlayHandler.Equipment = GAME_ITEM.NONE;
            return;
        }
		
        const string strSndFail = "IFX_block_swap_fail";

        // 이동 불가 check.
        if(false == isPassableThroughFence(bdA, bdB))
        {
            NNSoundHelper.Play(strSndFail); 
            _playSwitchAnimation(bdA, bdB, false, true);
            return;
        }

		if ((pt2.X < 0) || (pt2.Y >= WIDTH))    {   NNSoundHelper.Play(strSndFail); return; } 
        if ((pt2.Y < 0) || (pt2.Y >= HEIGHT))   {   NNSoundHelper.Play(strSndFail); return; } 

        if ((bdA.IsFilled == false) || (bdB.IsFilled == false)) {   NNSoundHelper.Play(strSndFail); return; } 
        if ((bdA.IsStable == false) || (bdB.IsStable == false)) {   NNSoundHelper.Play(strSndFail); return; } 
		if ((bdA.Panel.IsSwitchable() == false) || (bdB.Panel.IsSwitchable() == false)) {   NNSoundHelper.Play(strSndFail); return; } 

		if (bdA.PT.X != bdB.PT.X)
		{
            if (bdA.PD.IsSwitchableHorizontal(bdA.Piece) == false) {   NNSoundHelper.Play(strSndFail); return; } 
            if (bdB.PD.IsSwitchableHorizontal(bdB.Piece) == false) {   NNSoundHelper.Play(strSndFail); return; } 
            _switchDirectionV   = false;    // [2X2_BURST]
        }

		if (bdA.PT.Y != bdB.PT.Y)
		{
            if (bdA.PD.IsSwitchableVertical(bdA.Piece) == false) {   NNSoundHelper.Play(strSndFail); return; } 
            if (bdB.PD.IsSwitchableVertical(bdB.Piece) == false) {   NNSoundHelper.Play(strSndFail); return; } 
            _switchDirectionV   = true;     // [2X2_BURST]
        }
                
        StopSuggestPiece();

        bool swaped             = true;
        if (bdA.PD.IsCombinable(bdB.Piece)) 
		{
            NNSoundHelper.Play("IFX_block_swap");

            bdA.setMatchingList(bdB);
            bdB.setMatchingList(bdA);

            Merge(bdA, bdB, () => { 
                waitStablePump.MinWaitTime = bdA.PD.PerformCombinationPower(bdA, bdB, bdA.Piece, bdB.Piece); 
                Pump = waitStablePump;
            });  
        }
		else if (bdB.PD.IsCombinable( bdA.Piece)) 
		{
            NNSoundHelper.Play("IFX_block_swap");

            bdA.setMatchingList(bdB);
            bdB.setMatchingList(bdA);

            Merge(bdA, bdB, () => { 
                waitStablePump.MinWaitTime = bdB.PD.PerformCombinationPower(bdA, bdB, bdB.Piece, bdA.Piece); 
                Pump = waitStablePump;
            });  
        }
		else
		{            
            swaped              = Switch(bdA, bdB);
        }	
        
        if(isAIFightMode)
        {
            // 사람이 swap했을때만 drag 시킨다. 아니면 flag를 복원하고 재시도케 해 준다.
            if(false == isCurPlayerAI)
            {
                if(swaped)      StartCoroutine( _coAiDrag(pt1, pt2) );
                else
                {
                    bStartAiFight   = false;
                    _nAiMoveWaited  = 0;
                }
            }
        }
	}

    public T GetPanelType<T> () where T : PanelDefinition { 
        System.Type type = typeof(T);
        Debugger.Assert(typeDict.ContainsKey(type.Name)); 
        return typeDict[type.Name] as T;
    } 

    public T GetPieceType<T> () where T : PieceDefinition { 
        System.Type type = typeof(T);
        Debugger.Assert(typeDict.ContainsKey(type.Name)); 
        return typeDict[type.Name] as T;
    } 

	public void IncreaseScore (int num, Point pt, int colorIndex) { 
        IncreaseScore(num, this[pt].Position, colorIndex);
	} 

    public void IncreaseScore (int num, Vector3 pos, int colorIndex) {

        // ai enemy turn filter.
        if (isAIFightMode && isCurPlayerAI)
            return;

        Score += num;
    } 

    public void CollectSpecialJewel (SJ_COMBINE_TYPE combineType) {
        int index = (int)combineType;
        SpecialMatchCounts[index]++;

        int remainCount = CurrentLevel.specialJewels[index] - SpecialMatchCounts[index];

        if (remainCount >= 0) {
            JMFRelay.FireOnCollectSpecialJewel(combineType, remainCount);
        }
    }

	public int GetRandomColorIndex (int except = -1) {
        if (except > -1) {
            int colorIndex = -1;
            do { 
                colorIndex = GetColorIndex();
            } while (colorIndex == except);

            return colorIndex;
        } else {
            return GetColorIndex();
        }
	}

    int GetColorIndex () { 
        if (State == JMF_GAMESTATE.BONUS) {
            return NNTool.Rand(0, (int)LEItem.COLOR.NORMAL_COUNT);
        } else {
            return colorIndices[NNTool.Rand(0, colorIndices.Count)];
        }    
    }

    public int GetExistColorIndex () {
        int colorIndex = 0;

        NNTool.ExecuteForEachRandomIndex(0, boards.Count - 1, (index) => {
            if (this[index].IsFilled == false) return true;
            if (this[index].PD.isMatchable == false) return true;
            colorIndex = this[index].Piece.ColorIndex;
            return false;
        });

        return colorIndex;
    }

    public Board GetExistColorBoard () {
        Board bdColor           = null;

        NNTool.ExecuteForEachRandomIndex(0, boards.Count - 1, (index) => {
            if (this[index].IsFilled == false)          return true;
            if (this[index].PD.isMatchable == false)    return true;
            if (this[index].PD is NormalPiece == false) return true;
            bdColor             = this[index];
            return false;
        });

        return bdColor;
    }

    public Board GetPortalOfTheOtherSide (Board bd) {
        Debugger.Assert(bd.IsPortal, "GameManager.GetPortalOfTheOtherSide : Board is not portal.");
        Debugger.Assert(portalDict.ContainsKey(bd) || portalDict.ContainsValue(bd),
                "GameManager.GetPortalOfTheOtherSide : Can not find pair portal.");

        if (portalDict.ContainsKey(bd)) return portalDict[bd];

        foreach (Board _bd in portalDict.Keys) {
            if (portalDict[_bd] == bd) return _bd;
        }

        return null;
    }

    public List<GamePiece> GetPieces<T> () {
        List<GamePiece> pieces = new List<GamePiece>();

        for (int i = 0; i < boards.Count; i++) {
            if (this[i].IsFilled == false) continue;
            if ((this[i].PD is T) == false) continue;
            pieces.Add(this[i].Piece);
        }

        return pieces;
    }

    public bool DidGameOverByTimeBomb () {
        List<GamePiece> pieces = GetPieces<TimeBombPiece>();

        for (int i = 0; i < pieces.Count; i++) {
            TimeBomb timeBomb = pieces[i].GO.GetComponent<TimeBomb>();

            if (timeBomb.FallBackTime <= 0) {
                return true;
            }
        }

        return false;
    }
	
	public static void EquipItems(ref List<string> ids)
	{
        _listStartBoostId.Clear();

        for(int z = 0; z < ids.Count; ++z)
            _listStartBoostId.Add( ids[z] );
    } 
    public static void selectItem4Continue(string strId)
    {
        _strItemId4Continue     = strId;
    }

    public void AnimateGainBuff(GamePiece gp)
    {
        if(State != JMF_GAMESTATE.PLAY)
            return;

#if UNITY_EDITOR
        //if(true == LevelEditorSceneHandler.EditorMode)
        //    return;
#endif
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = SR.GetComponent<SpriteLibrary>().getSpriteByName("spr_star");
        SR.transform.position   = gp.Owner.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        SR.transform.localScale = Vector3.one * 1.0f;

        object[] param          = { gp.GO.transform.position, SR.gameObject };
        playOverlayHandler.AnimateGainBuff(param, () => 
        {
			//JMFRelay.FireOnCollectJewelForDisplay(colorIndex, remainCount);
            playOverlayHandler.onAddBuff();
		});
    }

    public void AnimateGainBlockToMissionPannel(GamePiece gp, Board bdTo)
    {
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = JMFUtils.POH.collectAnyMission.getJewelSkinSprite(gp.ColorIndex);
        SR.transform.position   = gp.Owner.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        //GameObject objMissionIcon= JMFUtils.POH.collectAnyMission.getIcon(string.Format("Jewel{0}", gp.ColorIndex));

        float duration          = JMFUtils.tween_move(SR.transform, gp.Owner.Position, bdTo.Position, 10.0f);

        StartCoroutine( _coOnGainBlockToMissioinPannelComplete(duration, SR.gameObject, gp.ColorIndex) );
    }

    IEnumerator _coOnGainBlockToMissioinPannelComplete(float delay, GameObject objFly, int idxColor)
    {
        yield return new WaitForSeconds(delay);

        NNPool.Abandon( objFly );
        if(idxColor<0 || idxColor>=BlockMissionPanels.Count)
            yield break;
        if(null == BlockMissionPanels[idxColor])
            yield break;

        BoardPanel pnlMission   = this.BlockMissionPanels[ idxColor ].Panel;
        pnlMission.Durability--;
        if(pnlMission.Durability < 0)
            pnlMission.Invalidate();

        BubbleHit effect        = NNPool.GetItem<BubbleHit>("BubbleHit");
        Vector3 pos             = pnlMission.Owner.Position;
        effect.Play(pos, Vector3.one, 1, false, 0.45f);
    }

    public void AnimateGainJewel (GamePiece gp, int remainCount, int colorIndex)
    {
		animationCount++;

        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = playOverlayHandler.collectAnyMission.getJewelSkinSprite(colorIndex);
        SR.transform.position   = gp.Owner.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon(string.Format("Jewel{0}", colorIndex));

        object[] param          = { gp.GO.transform.position, gp.Owner.PD.GetImageName(gp.Owner.Piece.ColorIndex), SR.gameObject, objMissionIcon, Vector3.one };
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectJewelForDisplay(colorIndex, remainCount);
		});
    }

    public void AnimateGainSugarJewel (GamePiece gp, int remainCount, int colorIndex)
    {
		animationCount++;

        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = playOverlayHandler.collectAnyMission.getSugarJewelSkinSprite(colorIndex);
        SR.transform.position   = gp.Owner.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon("SugarBlock");

        object[] param          = { gp.GO.transform.position, gp.Owner.PD.GetImageName(gp.Owner.Piece.ColorIndex), SR.gameObject, objMissionIcon, Vector3.one };
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectSugarJewelForDisplay(colorIndex, remainCount);
		});
    }

    public void AnimateGainFairy (GamePiece gp, int remainCount)
    {
        Debug.Assert(false, "We don't use fairy blk for now !!!");
        //SpriteRenderer sr = NNPool.GetItem<SpriteRenderer>("GainedFairy");
        //sr.transform.position = gp.Owner.Position;
        //sr.transform.localScale = Vector3.one * 2f;
        //object[] param = { sr.gameObject, "Fairy" };
        // NOUSE -- playOverlayHandler.AnimateGainMissionItem(param, () => {
        //     JMFRelay.FireOnChangeRemainFairyForDisplay(remainCount);
        // });
    }

    public void AnimateGainPenguin (GamePiece gp, int remainCount)
    {		
        Debug.Assert(false, "We don't use penguin blk for now !!!");
        /*
		SkeletonRenderer SR = NNPool.GetItem<SkeletonRenderer>("GainedMissionItem");

		int slotIndex = SR.Skeleton.FindSlotIndex("RegionList");
		Slot _slot = SR.Skeleton.FindSlot("RegionList");
		// todo 리소스 스파인 추가
		_slot.Attachment = SR.Skeleton.GetAttachment(slotIndex,gp.Owner.PD.GetImageName(gp.Owner.Piece.ColorIndex));	
		
		SR.transform.position = gp.Owner.Position;
		//SR.transform.localScale = gp.Owner.Piece.GO.transform.localScale;
		JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
		
		//object[] param = { SR.gameObject, "Penguin" };
        // NOUSE -- playOverlayHandler.AnimateGainMissionItem(param, () => {
        //    JMFRelay.FireOnChangeRemainPenguinForDisplay(remainCount);
        //});
        */
    }

    public void AnimatedGainChocoBar(Board bd, string picName, Transform trPic)
    {
        ++countMatchChocoBar;
        NNSoundHelper.Play("IFX_cheesebar_earning");
		
        SpriteRenderer SR       = trPic.GetComponent<SpriteRenderer>();
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon("ChocoBar");

        object[] param          = { trPic.transform.position, picName, SR.gameObject, objMissionIcon, Vector3.one };
        int remainCount         = CurrentLevel.countChocoBar - countMatchChocoBar;
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectMissionObjectForDisplay("ChocoBar", remainCount);
		});
    }

    public void AnimateGainShaded (Board bd)
    {    
        // [JAM_SHADE]
        int remainCount         = -1;
        //string strObject        = "";
        string strPic           = "";
		
        string strPicID         = "";
        bool needFly            = true;
        bool playParticle       = false;
        switch(bd.eShadeType)
        {
        case (int)LEItem.SHADE_TYPE.CURSE:
            if(CurrentLevel.countCursedBottom <= 0)
                return;         // 모으는 미션이 아니다.
            //strObject           = "CurseShade";
			strPic              = "obstacle_bgstone_01";
            strPicID            = "CurseShade";
            ++countMatchCursedBottom;
            //needFly             = false;
            remainCount         = CurrentLevel.countCursedBottom - countMatchCursedBottom;
            break;
        case (int)LEItem.SHADE_TYPE.JAM:
            if(CurrentLevel.countJamBottom  <= 0)
                return;         // 모으는 미션이 아니다.
                                //strObject           = "JamShade";  
            
            playParticle        = true;
			strPic              = "bubble_Strawberry";
            strPicID            = "JamShade";
            ++countMatchJamBottom;  
            remainCount         = CurrentLevel.countJamBottom - countMatchJamBottom;            
            break;
        case (int)LEItem.SHADE_TYPE.MUD_COVER:
            if(CurrentLevel.countMudShade <= 0)
                return;         // 모으는 미션이 아니다.
            strPic              = "thorn_mud1";
            strPicID            = "MudShade";
            ++countMatchMudShade;
            remainCount         = CurrentLevel.countMudShade - countMatchMudShade;
            break;
        case (int)LEItem.SHADE_TYPE.NET:
        default:                return;
        }

        // 이하 연출은 미션 때만.
        if(remainCount >= 0)
        {
            int newRemainCount  = remainCount;
            int newShadeType    = bd.eShadeType;

            if(needFly)
            {
                SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
                SR.sprite               = playOverlayHandler.collectAnyMission.getObjectSkinSprite(strPicID);
                SR.transform.position   = bd.Position;
                SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
                GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon( strPicID );

                object[] param      = { bd.Position, strPic, SR.gameObject, objMissionIcon, Vector3.one };
                playOverlayHandler.AnimateGainMissionItem(param, () => 
                {
                    JMFRelay.FireOnChangeRemainShadeForDisplay(newRemainCount, newShadeType);
                });

                /*
                GameObject objSR        = null;
                if((int)LEItem.SHADE_TYPE.MUD_COVER == bd.eShadeType)
                {
                    SpriteImage img         = NNPool.GetItem<SpriteImage>("SpriteImage");
                    img.transform.position  = bd.Position;
                    img.transform.localScale= Vector3.one * 0.85f;
                    img.GetComponent<SpriteRenderer>().sprite   = img.getSpriteByName(strPic);
		            img.GetComponent<SpriteRenderer>().sortingOrder    = 11;
                    objSR                   = img.gameObject;
                }
                else
                {
                    if (false == playParticle)
                    {
                        SkeletonRenderer SR = NNPool.GetItem<SkeletonRenderer>("GainedMissionItem");
			            int slotIndex       = SR.Skeleton.FindSlotIndex("RegionList");
			            Slot _slot          = SR.Skeleton.FindSlot("RegionList");
			            _slot.Attachment    = SR.Skeleton.GetAttachment(slotIndex,strPic);	
			            JMFUtils.SpineObjectAutoScalePadded(SR.gameObject);
			            SR.transform.position = bd.Position;
                        SR.GetComponent<MeshRenderer>().sortingOrder    = 11;
                        objSR               = SR.gameObject;
                    }
                }

                object[] param      = { bd.Position, strPic, objSR };
                playOverlayHandler.AnimateGainMissionItem(param, () => {
                    JMFRelay.FireOnChangeRemainShadeForDisplay(newRemainCount, newShadeType);
                });*/
            }
            else
            {
                DOVirtual.DelayedCall(1.0f, () => 
                {
			        JMFRelay.FireOnChangeRemainShadeForDisplay(newRemainCount, newShadeType);
                });
            }
        }
    }

    // [ROUND_CHOCO]
    public void AnimationMissionObject(Board bd, int remainCount, string strObjType, string strSpriteKey)
    {
        //JMFRelay.FireOnCollectMissionObjectForDisplay(strObjType, remainCount);

        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = playOverlayHandler.collectAnyMission.getObjectSkinSprite(strObjType);
        SR.transform.position   = bd.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon(strObjType);
        // [ADJUST SCALE]
        Vector3 vScale          = Vector3.one;
        if(strObjType=="SodaCan")
            vScale *= 0.68f;
        SR.transform.localScale = vScale;

        object[] param          = { bd.Position, strSpriteKey, SR.gameObject, objMissionIcon, vScale };
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectMissionObjectForDisplay(strObjType, remainCount);
		});
    }

    public void AnimationMissionObjectWithSprite(Board bd, int remainCount, string strObjType, string strSpriteKey)
    {
        //JMFRelay.FireOnCollectMissionObjectForDisplay(strObjType, remainCount);
       
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = playOverlayHandler.collectAnyMission.getObjectSkinSprite(strSpriteKey);
        SR.transform.position   = bd.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon(strSpriteKey);

        object[] param          = { bd.Position, strSpriteKey, SR.gameObject, objMissionIcon, Vector3.one};
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectMissionObjectForDisplay(strObjType, remainCount);
		});
    }

    void AnimateGainTreasure (Board bd, TREASURE_TYPE tt, int remainCount) 
	{
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");

        string strSpriteKey     = string.Format("Potion{0}", (int)tt+1);
        SR.sprite               = playOverlayHandler.collectAnyMission.getObjectSkinSprite(strSpriteKey);
        SR.transform.position   = bd.Position;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        GameObject objMissionIcon= playOverlayHandler.collectAnyMission.getIcon(strSpriteKey);

        object[] param          = { bd.Position, strSpriteKey, SR.gameObject, objMissionIcon, Vector3.one };
        playOverlayHandler.AnimateGainMissionItem(param, () => 
        {
			JMFRelay.FireOnCollectTreasureForDisplay(tt, remainCount);
		});
    }

    public void AddExtraMoves (int addMoves) {
        ExtraMoves += addMoves;
        JMFRelay.FireOnChangeRemainMove(AllowedMoves - Moves);
    }

    public void AddExtraTimes (float addTimes) {
        ExtraTimes += addTimes;
        JMFRelay.FireOnChangeRemainTime(Mathf.Max(0F, GivenTime - PlayTime));
    }

    public void AddExtraMinusMove_Ai(int decrease_count)
    {
        // this may be minus value.
        if(decrease_count < 0)  decrease_count *= -1;

        // win 도달수에서 count뺀 도달치 설정.
        int count_dec           = countEnemyWinBoard - (CurrentLevel.countAiWinBoard - decrease_count);
        ExtraMinusAiPoint += count_dec;
    }

    void _onContinueAiPoint()
    {
        if(ExtraMinusAiPoint < 0)
            ExtraMinusAiPoint *= -1;

        countEnemyWinBoard -= ExtraMinusAiPoint;

        List<Board> listTargets = new List<Board>();
        for (int i = 0; i < boards.Count; i++)
        {
            if (Board.AI_SIDE.ENEMIES != this[i].AiSide)
                continue;
            listTargets.Add( this[i] );
        }
        int cntGoal             = listTargets.Count<ExtraMinusAiPoint ? listTargets.Count : ExtraMinusAiPoint;

        for(int k = 0; k < cntGoal; ++k)
        {
            int idxTarget       = UnityEngine.Random.Range(0, listTargets.Count);
            listTargets[ idxTarget ].AiSide = Board.AI_SIDE.NONE;
            listTargets.Remove( listTargets[ idxTarget ] );
        }
                
        // fire event.
        if(null!=JMFRelay.OnUpdateAiScoreUI)
            JMFRelay.OnUpdateAiScoreUI();

        ExtraMinusAiPoint       = 0;
    }

    public void AllocateStartItems (bool bContinue = false, Dictionary<Board,bool> fixedDict = null) 
    {
        Root.Data.gameData.SaveContext();
        
        NNTool.ExecuteForEachRandomIndex(0, boards.Count - 1, (i) => 
        {
            if (this[i].IsFilled == false)              return true;
            if ((this[i].PD is NormalPiece) == false)   return true;
            if(null!=this[i].Piece && this[i].Piece.LifeCover>0)
                return true;
            //= if ((fixedDict != null) && (fixedDict.ContainsKey(this[i]))) return true;

            PieceDefinition pdTarget    = null;
            for(int z = 0; z < _listStartBoostId.Count; ++z)
            {
                if(_listStartBoostId[z].Equals("hbombbooster"))
                    pdTarget            = GetPieceType<HorizontalPiece>();
                else if(_listStartBoostId[z].Equals("vbombbooster"))
                    pdTarget            = GetPieceType<VerticalPiece>();
                else if(_listStartBoostId[z].Equals("bombbooster"))
                    pdTarget            = GetPieceType<BombPiece>();
                else if(_listStartBoostId[z].Equals("rainbowbooster"))
                    pdTarget            = GetPieceType<SpecialFive>();

                if(null != pdTarget)
                {
                    int idxColor        = true==_listStartBoostId[z].Equals("rainbowbooster") ? 0 : this[i].ColorIndex;
                    this[i].ResetPiece(pdTarget, idxColor);
                    _listStartBoostId.Remove( _listStartBoostId[z] );
                    if(null != fixedDict)
                        fixedDict[this[i]]= false;
                    return true;
                }
                
            }
                //PanelNetHit fx  = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
				//fx.ChangeColor(1, this[i].ColorIndex);
				//fx.Play(this[i].Position, this[i].Piece.Scale, false, .0f, 1.0f, this[i].Piece.GO.transform);

            return false;
        });

        for(int z = 0; z < _listStartBoostId.Count; ++z)
        {
            if(_listStartBoostId[z].Equals("moreturnbooster"))
            {
                //if (CurrentLevel.isTimerGame)  AddExtraTimes(5F);
                //AddExtraMoves(5);
                StartCoroutine( _coAddExtraStartMove() );
                _listStartBoostId.Remove( _listStartBoostId[z] );
                break;
            }
        }

        for(int i = 0; i < _listStartBoostId.Count; i++) 
        {
            Debug.LogError(string.Format("Item Allocation Fail. - {0}", _listStartBoostId[i]));
        }
        _listStartBoostId.Clear();
    }

    IEnumerator _coAddExtraStartMove()
    {
        if(CurrentLevel.isTimerGame)
        {
            int numExtra        = InfoLoader.GetDefaultPlayOnAddTime(0);
            for(int q = 0; q < numExtra+1; ++q)
            {
                yield return new WaitForSeconds(0.05f);
                AddExtraTimes(1);
            }
        }
        else
        {
            int numExtraMove    = InfoLoader.GetDefaultPlayOnMoveCount(0);
            for(int q = 0; q < numExtraMove; ++q)
            {
                yield return new WaitForSeconds(0.1f);
                AddExtraMoves(1);
            }
        }
    }

    //public enum STATE { STABLE, SWAP, DROP, WAIT_DESTROY }
    public void OnChangeBoardState (Board bd, Board.STATE bState)
    {
#if DEV_MODE && UNITY_EDITOR
        switch (bState)
        {
        case Board.STATE.STABLE :
            gizmoDict[bd] = Color.blue;
            break;
        case Board.STATE.SWAP :
            gizmoDict[bd] = Color.yellow;
            break;
        case Board.STATE.DROP :
            gizmoDict[bd] = Color.green;
            break;
        case Board.STATE.WAIT_DESTROY :
            gizmoDict[bd] = Color.red;
            break;
        default :
            gizmoDict[bd] = Color.white;
            break;
        }
#endif
    }

	public bool FindSuggestedPieces() 
	{
		moverSuggested = null;
		targetSuggested = null;
		Dictionary<Board, List<Board>> dicRet = GetSuggestablePieces(ref suggestedPieces, false);
        return dicRet.Count>0;
	}

    // Dictionary<Board, List<Board>> : <기준보드, <매칭가능한보드>>
    // ref List<Board> bdHits : 이 매칭에 관여하는(hit되는) 보드목록
    // findAll : 모든 보드를 검색할 것인가. - false 면 하나만 찾고 멈춤.
    public Dictionary<Board, List<Board>> GetSuggestablePieces(ref List<Board> bdHits, bool findAll=true) 
	{
        Dictionary<Board, List<Board>> suggestionPieces = new Dictionary<Board, List<Board>>();
        foreach (Board bd in boards)
        {
            List<Board> bds     = GetCandidatesOfMatch(bd, ref bdHits, findAll);
            if (bds.Count > 0)
            {
                suggestionPieces.Add(bd, bds);
                if (false == findAll) 
                    break;      // note : 매칭 대상을 하나만 찾으면 될 경우.
            }
        }
        return suggestionPieces;
    }

	public List<Board> GetBoards<T> () 
	{
        List<Board> bds = new List<Board>();

        for (int i = 0; i < boards.Count; i++) 
		{
            if (typeof(PanelDefinition).IsAssignableFrom(typeof(T)))
			{
                if (this[i].PND.GetType().Equals(typeof(T)) == false) continue;

                bds.Add(this[i]);
            } 
				else if (typeof(PieceDefinition).IsAssignableFrom(typeof(T))) 
				{
                if (this[i].IsFilled == false) continue;
                if (this[i].PD.GetType().Equals(typeof(T)) == false) continue;

                bds.Add(this[i]);
            }
        }

        return bds;
    }

    public List<Board> GetShadeBoards(LEItem.SHADE_TYPE eShadeType)
    {
        List<Board> bds = new List<Board>();
        for (int i = 0; i < boards.Count; i++) 
		{
            if((int)eShadeType==boards[i].eShadeType && boards[i].ShadedDurability>=0) 
                bds.Add(this[i]);
        }
        return bds;
    }


    void InitTypeDictionary () {
        for (int i = 0; i < PanelTypes.Length; i++) {
            typeDict.Add(PanelTypes[i].GetType().Name, PanelTypes[i]);
        }

        for (int i = 0; i < PieceTypes.Length; i++) {
            typeDict.Add(PieceTypes[i].GetType().Name, PieceTypes[i]);
        }
    }

    // update launcher panels from pump.
    public void removeBoardFromLauncher(Board bdTarget)
    {
        launcherStonePump.removeLauncherBoard(bdTarget);
        launcherIcePump.removeLauncherBoard(bdTarget);
        launcherWebPump.removeLauncherBoard(bdTarget);
    }

	void SetupBoard (Data.Level level)
	{
        int index = 0;
        List<Point> blks = new List<Point>();
        Dictionary<Board, bool> fixedDict = new Dictionary<Board, bool>();
        Dictionary<Board, int> dic2x2ColorBomb = new Dictionary<Board, int>();

        BlockMissionPanels.Clear();
        for(int a = 0; a < (int)LEItem.COLOR.NORMAL_COUNT; a++) 
            BlockMissionPanels.Add( null );

        // 첫 시작 pump flag setting.
        // PlayMoved               = true;

        mCntMakingSpecialPiece  = 0;
        _idxBdTopY              = -1;
        _listOnBoard.Clear();

        for (int y = HEIGHT - 1; y >= 0; y--) 
		{
            for (int x = 0; x < WIDTH; x++) 
			{
                Board bd = this[x,y];
                bd.ResetBoard();                

                int lifeCover   = null!=level.lifePieceCover && level.lifePieceCover.Length>index ? level.lifePieceCover[index] : 0;

				PieceDefinition pd = PieceTypes[level.pieces[index]];
                PanelDefinition pnd = PanelTypes[level.panels[index]];
		        object panelInfo = pnd.ConvertToPanelInfo(level.panelInfos[index]);
                int colorIndex = GetRandomColorIndex();

                bd.eShadeType       = level.shaded[index]>=0 ? level.eShadeType[index] : -1;  // [JAM_SHADE]
                bd.ShadedDurability = level.shaded[index];
                
                int panelStrength   = level.strengths[index]-1;
                if((int)LEItem.SHADE_TYPE.MUD_COVER==bd.eShadeType && 1==level.shaded[index])
                {
                    pnd             = GetPanelType<MudCoverPanel>();
                    panelStrength   = 0;
                    // note : 패널로 cover는 빼고 shade durability는 0으로 reset.
                    bd.ShadedDurability = 0;
                }
                else if(pnd is ColorBombPanel)
                {
                    dic2x2ColorBomb.Add(bd, level.strengths[index]);
                    panelStrength   = -1;
                }
                else if(pnd is BlockMissionPanel && null!=level.panelColors)
                {
                    int idxColor        = level.panelColors[index] - 1;
                    if(idxColor>=0 && idxColor<(int)LEItem.COLOR.NORMAL_COUNT)
                        BlockMissionPanels[ idxColor ] = bd;
                }

                if(level.fenceDirection != null)
                    _listFence[x+y*WIDTH] = (JMF_DIRECTION)level.fenceDirection[ index ];

                // [NET_SHADE], [CHOCO_BAR]
				if(level.indexBar != null)
                	bd.initChocoBar(level.indexBar[index], (LEItem.CHOCO_BAR)level.eBarType[index]);

				if(level.changerColorIndex != null && level.changerColorIndex[index] >= 0)
                    bd.initColorChanger( level.changerColorIndex[index] );

                // --> Temp Code, Require Data Refactoring
                if (pnd is PortalDown) 
				{
                    pnd = GetPanelType<BasicPanel>();
                    bd.SetupPortal(PORTAL_TYPE.DOWN, level.portalUIndices[index], level.portalDIndices[index]);
                        //level.strengths[index]-1);
                }
                else if (pnd is PortalUp) 
				{
                    pnd = GetPanelType<BasicPanel>();
                    bd.SetupPortal(PORTAL_TYPE.UP, level.portalUIndices[index], level.portalDIndices[index]);
                        //level.strengths[index]-1);
                }
                else if (pnd is PortalAll) 
				{
                    pnd = GetPanelType<BasicPanel>();
                    bd.SetupPortal(PORTAL_TYPE.ALL, level.portalUIndices[index], level.portalDIndices[index]);
                        //level.strengths[index]-1);
                }
// <--
                if ( (level.portalTypes != null && level.portalTypes[index] > 0) &&
                     ((level.portalUIndices != null && level.portalUIndices[index] > 0) ||  (level.portalDIndices != null && level.portalDIndices[index] > 0)))
				{
                    bd.SetupPortal((PORTAL_TYPE)level.portalTypes[index], level.portalUIndices[index], level.portalDIndices[index]);
                }
				
                // conveyor link index. - 순환이 아닐경우 이 index로 통로를 찾는다.
				if(level.conveyorIndices != null)
                	bd._idConveyor  = level.conveyorIndices[ index ];

				bd.UpdatePosition();
                bd.ResetPanel(pnd, panelStrength, panelInfo, null!=level.panelColors ? level.panelColors[index] : -1);
                if(bd.Panel.IsNeedGuage())  bd.Panel.setMaxGaugeValue(panelStrength);
                //if(bd.Panel.IsNeedNumber()) bd.Panel.

                // 패널도 river에 의해 움직여야 하므로, 로지컬하게 패널 하위 패널을 하나 더 둔다. - 이녀석은 움직이지 않는다.
				if(level.hasRiver!=null && true==level.hasRiver[index])
                {
                    //const int idxRiverPnl   = 22;
                    PanelDefinition pnd2    = GetPanelType<ConveyorPanel>();// PanelTypes[ idxRiverPnl ];
                    bd.ResetRiverPanel(pnd2.ConvertToPanelInfo(level.riverInfos[index]), !(pd is StonePiece));
                }

                if ((pnd.GetType().Equals(typeof(EmptyPanel)) == false) && 
                    (pnd.GetType().Equals(typeof(CreatorPanel)) == false) &&
                    (pnd.GetType().Equals(typeof(InvisibleObstructionPanel)) == false)) 
				{
                    blks.Add(new Point(x,y));
                    _idxBdTopY  = y>_idxBdTopY ? y : _idxBdTopY;
                    _listOnBoard.Add( bd );
                    bd.initBG();
                }

                if (bd.Panel.IsFillable()) 
				{
                    bool fixedColor = false;

                    if (level.colors[index] > 0) 
					{
                        colorIndex = level.colors[index] - 1;
                        fixedColor = true;
                    }

                    bool hasStartPiece = true;

                    if (level.startPieces != null) hasStartPiece = level.startPieces[index];

                    if (hasStartPiece) 
					{
						if (pd is NormalPiece || pd is StrawberryPiece || pd is SugarCherryPiece || pd is ZellatoPiece) 
						{    
							bd.ResetPiece(pd, colorIndex, -1, lifeCover);
                            if (fixedColor) 
								fixedDict.Add(bd, fixedColor);   
                        }
						else 
						{
                            if (pd.isMatchable || pd is TMatch7Piece) 
							{
                                bd.ResetPiece(pd, colorIndex, -1, lifeCover);
                            }
							else 
							{
                                if (pd is StonePiece) 
								    bd.ResetPiece(pd, level.colors[index]);
                                else if(pd is RoundChocoPiece || pd is GreenBubblePiece || pd is CookiePiece)  // [ROUND_CHOCO], [GREEN_BUBBLE]
                                    bd.ResetPiece(pd, 0, level.strengths[index]-1);
                                else if(pd is CookieJellyPiece)
                                    bd.ResetPiece(pd, colorIndex, level.strengths[index]-1);
                                else
                                    bd.ResetPiece(pd, 0, -1, lifeCover);

                                fixedColor = true;
                            }

                            bd.Piece.FallBackTime = level.defaultFallBackTime;

                            fixedDict.Add(bd, fixedColor);   
                        }
                    } 
					else 
					{
                        bd.IsNeedDropCheck = true;
                    }
                }

                // [AI_MISSION]
                if(null!=level.ai_taken_index && -1!=level.ai_taken_index[index] && isAIFightMode)
                    bd.initAiTaken(level.ai_taken_index[ index ]);
                //

                // dropper.
				if(level.eHelperType != null)
                	bd._eHelperType = level.eHelperType[index];

                // check treasure goal.
                if(level.changerColorIndex==null || level.changerColorIndex[index]<0)
                    bd.removeTreasureGoal();
                if(true == CurrentLevel.isTreasureGame)
                {
                    for (int i = 0; i < level.treasureGoal.Length; i += 2)
                    {
                        if(x==level.treasureGoal[i] && y==level.treasureGoal[i+1])
                        {
                            bd.initTreasureGoal();
                            break;
                        }
                    }
                }
                index++;
            }
        }
		
        // Allocate Start Items
        // AllocateStartItems(fixedDict); - 타이밍 변경.

        // Update Portal Dictionary
        initPortals();

        initFenceView();

        initColorBombPanel(ref dic2x2ColorBomb);

        initCookieJellyCover();

        // [NET_SHADE], [CHOCO_BAR]
		if(level.indexBar != null)
        	_buildChocoBarInfo(level);

        // Update Conveyor Dictionary
        // UpdateConveyors();
        // Draw Border
		
		boardMin = new Point(blks.Min(b => b.X), blks.Min(b => b.Y));
		boardMax = new Point(blks.Max(b=>b.X)+1 ,blks.Max(b=>b.Y)+1);
		
		RepositionBoard();
		//spineBoard.CreateSpineBoardFromBlocks(blks.ToArray());
		        
        // Draw Treasure Goal
        //if (CurrentLevel.isTreasureGame)    UpdateTreasureGoal();
        //else                                ClearTreasureGoal();
        
//        if (CurrentLevel.isFairyGame)
//            SetFairyAnimation();

        YetiHealth = CurrentLevel.yetiHealth;
        BossHealth = CurrentLevel.bossHealth;

        //string strTutid         = TutorialOverlayHandler.GetAvailable(level.Index+1, "");
        //if(null==strTutid || strTutid.Length==0)
		EliminateStartMatch(fixedDict);
    }

	void RepositionBoard()
	{
//		Vector3 pos = transform.localPosition;
//		pos.x -= XX* Size; 
//		pos.y -= YY* Size;
		transform.localPosition = MovingPosition;
		
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x) 
			{
				Board bd = this[x,y];
				bd.UpdatePosition();
				
				if(bd.PND is EmptyPanel)
					bd.Panel[BoardPanel.TYPE.BACK].gameObject.SetActive(false);
			}
		}
	}
	
	public void ShowClippingBorad(bool bOn)
	{
		for (int y = 0; y < HEIGHT; ++y)
		{
			for (int x = 0; x < WIDTH; ++x) 
			{
				Board bd = this[x,y];
	
				if(bd.PND is EmptyPanel)
				{
                    bd.Panel[BoardPanel.TYPE.BACK].gameObject.SetActive(bOn);
                }
			}
		}
	}
	
	void EliminateStartMatch (Dictionary<Board,bool> fixedDict) 
	{  
        int retryCount = 0;

        while (retryCount < 10000) 
		{
            if (FindMatchAndReplace(fixedDict))     // 자동매칭 되는게 있다.     - 시작과 즉시 매칭.
                retryCount++;
            else
            {
                if(false == FindSuggestedPieces())  // 아무것도 매칭되지 않는다. - 셔플 필요
                    retryCount++;
                else
                    break;
            }
        }

        if (retryCount > 9999) Debug.Log("!!! Failed to eliminate pre-start match.!!!");
	} 

//    public void SetFairyAnimation() 
//	{
//        foreach (Board bd in JMFUtils.GM.Boards) 
//		{
//            if (bd.Piece == null || (bd.Piece.PD is FairyPiece) == false) continue;
//            Fairy fa = bd.Piece.GO.GetComponent<Fairy>();
//
//            if (bd.Panel == null) 
//			    fa.StartAnimator();
//            else 
//			{
//                if (bd.Panel.PND is FrostPanel)
//                    fa.StopAnimator();
//                else
//                    fa.StartAnimator();
//            }
//        }
//    }

    bool FindMatchAndReplace (Dictionary<Board,bool> fixedDict) {  
        bool found = false;

        for (int y = 0; y < HEIGHT; y++) {
            for (int x = 0; x < WIDTH; x++) {
                if (ReplaceMatch(this[x,y], fixedDict)) {
                    found = true;
                    break;
                }
            }

            if (found) break;
        }

        return found;
    } 
	
	bool ReplaceMatch (Board bd, Dictionary<Board,bool> fixedDict) { 
        if (bd.IsMatchable == false) return false;

        List<Board> res         = new List<Board>();
        if(false == CanBeMatched(bd, bd.ColorIndex, JMF_DIRECTION.NONE, ref res))
            res.Clear();

        if (res.Count > 1) {
            if (fixedDict.ContainsKey(bd)) {
                if (fixedDict[bd] == false) {
                    bd.ResetPiece(bd.PD, GetRandomColorIndex(bd.ColorIndex), -1, bd.Piece.LifeCover);
                    return true;
                }
            } else {
                bd.ResetPiece(GetPieceType<NormalPiece>(), GetRandomColorIndex(bd.ColorIndex), -1, bd.Piece.LifeCover);
                return true;
            }
        }

#region // old code, commented.
        /*List<Board> row = GetRowMatchableBoards(bd, bd.ColorIndex, JMF_DIRECTION.NONE, false);
        if (row.Count > 1) {
            if (fixedDict.ContainsKey(bd)) {
                if (fixedDict[bd] == false) {
                    bd.ResetPiece(bd.PD, GetRandomColorIndex(bd.ColorIndex));
                    return true;
                }
            } else {
                bd.ResetPiece(GetPieceType<NormalPiece>(), GetRandomColorIndex(bd.ColorIndex));
                return true;
            }
        }

        List<Board> col = GetColMatchableBoards(bd, bd.ColorIndex, JMF_DIRECTION.NONE);
        if (col.Count > 1) {
            if (fixedDict.ContainsKey(bd)) {
                if (fixedDict[bd] == false) {
                    bd.ResetPiece(bd.PD, GetRandomColorIndex(bd.ColorIndex));
                    return true;
                }
            } else {
                bd.ResetPiece(GetPieceType<NormalPiece>(), GetRandomColorIndex(bd.ColorIndex));
                return true;
            }
        }
        */
#endregion

        return false;
	}

    void initPortals () {
        portalDict.Clear();

        for (int i = 0; i < boards.Count; i++) {
            if (this[i].IsPortal == false) continue;            
            if (this[i].PortalType!=PORTAL_TYPE.UP && this[i].PortalType!=PORTAL_TYPE.ALL)
                continue;
            // i -> PORTAL_TYPE.UP or ALL

            for (int j = 0; j < boards.Count; j++) {
                if (this[j].IsPortal == false) continue;
                if (this[j].PortalType!=PORTAL_TYPE.DOWN && this[j].PortalType!=PORTAL_TYPE.ALL)
                    continue;
                if (this[j].PortalDIndex != this[i].PortalUIndex)
                    continue;
                // j -> PORTAL_TYPE.DOWN

                portalDict.Add(this[i], this[j]);
                break;
            }
        }
    }

    void initColorBombPanel(ref Dictionary<Board, int> dic2x2ColorBomb)
    {
        foreach(Board iterBd in dic2x2ColorBomb.Keys)
        {
            ColorBombSpritePanel sprPnl = iterBd.Panel[BoardPanel.TYPE.BACK] as ColorBombSpritePanel;
            if(null == sprPnl)          continue;
            sprPnl.initDataHolder(iterBd, 1==dic2x2ColorBomb[iterBd] ? ColorBombSpritePanel.TYPE._5 : ColorBombSpritePanel.TYPE._6);
        }
    }

    // 전체 보드가 재구성된 이후 1회 다시 반복.
    void initCookieJellyCover()
    {
        for (int i = 0; i < boards.Count; i++)
        {
            if (this[i].PD is CookieJellyPiece || this[i].PD is CookiePiece) 
                this[i].Piece.resetCookieJellyCover();
        }
    }

    void initFenceView()
    {
        _listSpriteFence.Clear();
        for(int q = 0; q < _listFence.Count; ++q)
        {
            Vector3 vEuler      = Vector3.zero;
            Vector3 vLocalPos   = Vector3.zero;

            switch(_listFence[q])
            {
            case JMF_DIRECTION.LEFT:
                vEuler          = new Vector3(0, 0, 90);
                vLocalPos       = this[q].LocalPosition + new Vector3(-Size*0.5f, 0, 0.0f);
                break;
            case JMF_DIRECTION.RIGHT:
                vEuler          = new Vector3(0, 0, 90);
                vLocalPos       = this[q].LocalPosition + new Vector3(Size*0.5f, 0, 0.0f);
                break;
            case JMF_DIRECTION.UP:
                vLocalPos       = this[q].LocalPosition + new Vector3(0, Size*0.5f, 0.0f);
                break;
            case JMF_DIRECTION.DOWN:
                vLocalPos       = this[q].LocalPosition + new Vector3(0, -Size*0.5f, 0.0f);
                break;
            default:            continue;
            }

            SpriteImage sp      = NNPool.GetItem<SpriteImage>("SpriteImage", transform);
            sp.GetComponent<SpriteRenderer>().sprite = sp.getSpriteByName( "fence" );
            sp.transform.localPosition  = new Vector3(vLocalPos.x, vLocalPos.y, -2.0f);
            sp.transform.localEulerAngles= vEuler;
            sp.GetComponent<SpriteRenderer>().sortingOrder  = 0;
            _listSpriteFence.Add( sp );
        }
    }

    public bool isPassableThroughFence(Board bdFrom, Board bdTo)
    {
        if(null==bdFrom || null==bdTo)  return false;

        if(bdTo.Equals( bdFrom.Top))    return isPassableThroughFence(bdFrom, JMF_DIRECTION.UP);
        if(bdTo.Equals( bdFrom.Bottom)) return isPassableThroughFence(bdFrom, JMF_DIRECTION.DOWN);
        if(bdTo.Equals( bdFrom.Left))   return isPassableThroughFence(bdFrom, JMF_DIRECTION.LEFT);
        if(bdTo.Equals( bdFrom.Right))  return isPassableThroughFence(bdFrom, JMF_DIRECTION.RIGHT);

        return false;
    }

    // move to target direcion check by fence.
    public bool isPassableThroughFence(Board bdFrom, JMF_DIRECTION eDirTo)
    {
        if(null == bdFrom)      return false;
        int idxFrom             = bdFrom.X + bdFrom.Y * WIDTH;
        int idxTo               = 0;
        switch(eDirTo)
        {
        case JMF_DIRECTION.DOWN:
            if(null == bdFrom.Bottom)   return true;    // 기존 구조에 맞춰, fence 영향 외는 true 처리.
            idxTo               = bdFrom.Bottom.X + bdFrom.Bottom.Y*WIDTH;
            return (_listFence[idxFrom]!=JMF_DIRECTION.DOWN && _listFence[idxTo]!=JMF_DIRECTION.UP);
        case JMF_DIRECTION.UP:
            if(null == bdFrom.Top)      return true;
            idxTo               = bdFrom.Top.X + bdFrom.Top.Y*WIDTH;
            return (_listFence[idxFrom]!=JMF_DIRECTION.UP && _listFence[idxTo]!=JMF_DIRECTION.DOWN);
        case JMF_DIRECTION.LEFT:
            if(null == bdFrom.Left)     return true;
            idxTo               = bdFrom.Left.X + bdFrom.Left.Y*WIDTH;
            return (_listFence[idxFrom]!=JMF_DIRECTION.LEFT && _listFence[idxTo]!=JMF_DIRECTION.RIGHT);
        case JMF_DIRECTION.RIGHT:
            if(null == bdFrom.Right)    return true;
            idxTo               = bdFrom.Right.X + bdFrom.Right.Y*WIDTH;
            return (_listFence[idxFrom]!=JMF_DIRECTION.RIGHT && _listFence[idxTo]!=JMF_DIRECTION.LEFT);
        default:
            break;
        }
        return false;
    }

    IEnumerator CoPump () {
        if (enabledPump) yield break;

        enabledPump = true;

        //Debug.Log(pump.State);

		while (State != JMF_GAMESTATE.OVER) {
            yield return null;

            if ((State == JMF_GAMESTATE.PLAY) || (State == JMF_GAMESTATE.BONUS)) {
                pump = Pump.Next();
            }
            //else yield return null;
        }

        enabledPump = false;
    }

    public void IncreaseTreasure (Board bd, TREASURE_TYPE tt) {
        int remainCount = 0;

        switch(tt)
        {
        case TREASURE_TYPE.POTION1:
            Potion1CollectCount++;
            remainCount = CurrentLevel.countPotion1 - Potion1CollectCount;
            break;
        case TREASURE_TYPE.POTION2:
            Potion2CollectCount++;
            remainCount = CurrentLevel.countPotion2 - Potion2CollectCount;
            break;
        case TREASURE_TYPE.POTION3:
            Potion3CollectCount++;
            remainCount = CurrentLevel.countPotion3 - Potion3CollectCount;
            break;
        default:
            return;
        }

        NNSoundHelper.Play("IFX_syrup_earning");

        JMFRelay.FireOnCollectTreasure(tt, remainCount);
        IncreaseScore(bd.PD.destroyScore, bd.PT, 6);
        AnimateGainTreasure(bd, tt, remainCount);
    }

    public bool IsClearedLevel (Data.Level level)
    {
        //if(isLevelClearByBuff)  return true;

        // note : 동시조건이더라도, 해당조건 모두를 만족해야 clear->true가 됨.
        //
        switch((EditWinningConditions.MISSION_TYPE)level.missionType)
        {
        case EditWinningConditions.MISSION_TYPE.DEFEAT: // [AI_MISSION]
        {
            if (level.countAiWinBoard <= countMyWinBoard)     return true;
            return false;
        }
        case EditWinningConditions.MISSION_TYPE.SCORE:
            if(level.goalScore <= score)                    return true;
            return false;
        case EditWinningConditions.MISSION_TYPE.COLLECT:
        case EditWinningConditions.MISSION_TYPE.FILL:       // [PIECE_POTION]
        case EditWinningConditions.MISSION_TYPE.FIND:       // [NET_SHADE], [CHOCO_BAR]
        case EditWinningConditions.MISSION_TYPE.CLEAR:
        {  
            if (level.isGetTypesGame && (IsCollectedAllJewel == false))             return false;
            if (level.isSpecialJewelGame && (IsCollectedAllSpecialJewel == false))  return false;
            if( level.countRoundChocho>0 && countMatchRoundChocho<level.countRoundChocho) return false;
            if( level.countJamBottom>0 && countMatchJamBottom<level.countJamBottom) return false;
            if(level.countRectChocho>0 && countMatchRectChocho<level.countRectChocho) return false;
            if(level.countCottonCandy>0 && countMatchCottonCandy<level.countCottonCandy) return false;
            if(level.countSodaCan>0 && countMatchSodaCan<level.countSodaCan) return false;
            if(level.countSugarBlock>0 && countMatchSugarBlock<level.countSugarBlock) return false;
            if(level.countZellatto>0 && countMatchZellatto<level.countZellatto) return false;
            if(level.countCookieJelly>0 && countMatchCookieJelly<level.countCookieJelly) return false;
            if(level.countColorBox>0 && countMatchColorBox<level.countColorBox) return false;
            if(level.countWaffleCooker>0 && countMatchWaffleCooker<level.countWaffleCooker) return false;
            if(level.countMudShade>0 && countMatchMudShade<level.countMudShade) return false;

            if (level.isTreasureGame && (TreasuresCollectCount < TotalRequireTreasureCount)) return false;

            if(level.countChocoBar>0 && countMatchChocoBar<level.countChocoBar) return false;

            if(level.countCursedBottom>0 && countMatchCursedBottom<level.countCursedBottom) return false;

            break;
        }
        default:    break;
        }
        /*
         olds....
        if (ShadedCount != 0) return false;
        if (level.isClearChocolateGame && (Snowman.TotalCount != 0)) return false;
        if (level.isGetTypesGame && (IsCollectedAllJewel == false)) return false;
        if (level.isSpecialJewelGame && (IsCollectedAllSpecialJewel == false)) return false;
        if (level.isPenguinGame && (PenguinMatchCount < level.numberOfPenguin)) return false;
        if (level.isYetiGame && (YetiHealth > 0)) return false;
        if ((level.isBossGame && (BossHealth > 0)) || Boss.Current != null) return false;
        if (level.isFairyGame && (FairyMatchCount < level.numberOfFairy)) return false;

        if (Score < level.scoreToReach[0]) Score = level.scoreToReach[0];
        */

        Debug.Log("Game Cleared !!!");
        return true;
    }

    public bool IsFailLevel () {
        // [AI_MISSION]
        if (true == isAIFightMode)
        {
            if (CurrentLevel.countAiWinBoard <= countEnemyWinBoard)
                return true;
            return false;
        }
        //
        if (IsOverTimeOrMove()) return true;
        if (ExplodedBomb) return true;
        if (EscapedPenguin) return true;
        return false;
    }

    public void ExecuteFail () {
        if (IsOverTimeOrMove()) {
            if (CurrentLevel.isTimerGame) {
                EndGame(GAMEOVER_REASON.TIMER, false, 0);
            } else {
                EndGame(GAMEOVER_REASON.MOVE, false, 0);
            }

        } else if (ExplodedBomb) {
            StartCoroutine(ShowExplosion(() => {
                EndGame(GAMEOVER_REASON.TIMEBOMB, false, 0);
            }));
        } else if (EscapedPenguin) {
            EndGame(GAMEOVER_REASON.PENGUIN, false, 0);
        }
        // [AI_MISSION]
        else if(isAIFightMode)
        {
             EndGame(GAMEOVER_REASON.AI_LOSE, false, 0);
        }
    }

    public bool CollectTreasure () {
        if (CurrentLevel.treasureGoal == null) return false;

        bool collected = false;

        foreach (Board bd in boards)
        {
            if(false == bd._isTreasureGoal)         continue;
            if (bd.IsFilled == false)               continue;
            if ((bd.PD is TreasurePiece) == false)  continue;
            if(false == (bd.PND is BasicPanel))     continue;

            collected = true;
            IncreaseTreasure(bd, (bd.PD as TreasurePiece).Type);
            bd.RemovePiece(false);
        }

        /*for (int i = 0; i < CurrentLevel.treasureGoal.Length; i += 2) {
            Point pt = new Point(CurrentLevel.treasureGoal[i], CurrentLevel.treasureGoal[i+1]);
            Board bd = this[pt];

            if (bd.IsFilled == false) continue;
            if ((bd.PD is TreasurePiece) == false) continue;

            collected = true;
            IncreaseTreasure(bd, (bd.PD as TreasurePiece).Type);
            bd.RemovePiece(false);
        }*/

        return collected;
    }

    // 모든 waffle cooker를 check하여, 모두 bake되었으면, 모두 clear 해 준다.
    public IEnumerator coCheckAllWaffleCookers(BoardPanel bpCurrent)
    {
        if(true == _onDestroyingAllWaffles)
            yield break;

        List<Board> listWaffle  = new List<Board>();
        foreach (Board bd in boards)
        {
            if(null == bd.PND)  continue;
            if(false == (bd.PND is WaffleCookerPanel))
                continue;

            // 아직 처리가 안된 녀석이 있으면 break.
            if(false==bd.Equals(bpCurrent.Owner) && bd.Panel.Durability>0)
                yield break;

            listWaffle.Add( bd );
        }
        
        // Processing !!!

        _onDestroyingAllWaffles = true;

        while(bpCurrent.Durability>0)           // wait till all is done.
            yield return null;

        yield return new WaitForSeconds(0.5f);  // wait delay.

        // Hit'em all !!!
        for(int q = 0; q < listWaffle.Count; ++q)
        {
            listWaffle[q].Panel.setIsDestroyablePanel( true );
            listWaffle[q].Hit();
        }

        _onDestroyingAllWaffles = false;
    }

    // collect treasure with single board target piece !
    public bool CollectSingleTreasure(Board targetBoard)
    {
         if (CurrentLevel.treasureGoal==null)
            return false;
         if(null==targetBoard || !(targetBoard.PD is TreasurePiece))
            return false;
         if(false == (targetBoard.PND is BasicPanel))
            return false;

          for (int i = 0; i < CurrentLevel.treasureGoal.Length; i += 2) {
            Point pt = new Point(CurrentLevel.treasureGoal[i], CurrentLevel.treasureGoal[i+1]);
            Board bd = this[pt];

            if(targetBoard.Equals (bd ))
            {
                // StartCoroutine( _coCollectTreasure(bd) );
                IncreaseTreasure(bd, (bd.PD as TreasurePiece).Type);
                bd.RemovePiece(false);
                return true;
            }
        }
        return false;
    }

    // 연출에 적용하는 것으로 방향 선회.
    IEnumerator _coCollectTreasure(Board bd)
    {
        // scale 효과.
        Vector3 vCurScale       = bd.Piece.GO.transform.localScale;
        bd.Piece.GO.transform.DOScale(vCurScale*1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(0.4f);

        IncreaseTreasure(bd, (bd.PD as TreasurePiece).Type);
        bd.RemovePiece(false);
    }

    bool IsOverTimeOrMove () {
#if !NN_DEPLOY
        if (isSolverMode) return false;
#endif
        if (CurrentLevel.isTimerGame) {
            return PlayTime >= GivenTime;
        }
        else {

            // no limit.
            if (CurrentLevel.allowedMoves <= 0)
                return false;

            return Moves >= AllowedMoves;
        }
    }

    public void ExecuteBonusTime () {

        StartCoroutine( _coExBonusTime() );
    }

    IEnumerator _coExBonusTime()
    {
        while(playOverlayHandler.IsPlayingComboText())
            yield return null;

		//playSceneHandler._sprBGFilter.gameObject.SetActive(true);
        //playSceneHandler._sprBGFilter.DOFade(0.8f, 0.5f);

        State = JMF_GAMESTATE.BONUS;
        yield return StartCoroutine( coWaitTillStable() );

        //
		Scene.ShowPopup("LevelClearNotiPopup", null, (param) => { StartCoroutine(CoBonusTime()); });
        //StartCoroutine(CoBonusTime());
        //= 팝업과 거이 비슷한 타이밍에 bonus 연출 진행 되도록 수정.
        //
        //Scene.ShowPopup("CompletePopup");
        // yield return new WaitForSeconds(0.5f);
        //yield return StartCoroutine( CoBonusTime() );

        //charAni_play(SPINE_CHAR.MAIN_GIRL, ANI_STATE.SUCCESS);
        //charAni_play(SPINE_CHAR.DOG, ANI_STATE.SUCCESS);
        ////if(true == isAIFightMode)
        //    charAni_play(SPINE_CHAR.AI_GIRL, ANI_STATE.SUCCESS);
    }

    // Fix from here !!!

	IEnumerator CoBonusTime ()
    {
#if !NN_DEPLOY
        solverRemainMove = Moves;
#endif
        if(false == ExistBonusTime())
        {
            EndGame(GAMEOVER_REASON.NONE, true, GetStarGrade());
            yield break;
        }
        
        // trigger char ani.
        //charAni_play(SPINE_CHAR.MAIN_GIRL, ANI_STATE.BONUS_BURST);
       // charAni_play(SPINE_CHAR.DOG, ANI_STATE.BONUS_BURST);
        //

        NNSoundHelper.PlayBGM("IFX_bonus_time_bgm", 0, true, false);
        NNSoundHelper.Play("IFX_bonus_time_bg", true);
        playSceneHandler.triggerDarkBGEffect(true);

        //State = JMF_GAMESTATE.BONUS;

        yield return new WaitForSeconds(1.0F);
        // while (IsFinishDropAndMatch == false) yield return null;      
        //yield return StartCoroutine( coWaitTillStable() );

        JMFRelay.FireOnStartBonus();

#region // old timer mode code. commented.
        // we don't have timer mode for now. by wayne.
        /*if (CurrentLevel.isTimerGame)
        {
            while (GivenTime > PlayTime) {
                if (ConvertNormalToSpecial()) {
                    yield return new WaitForSeconds(0.1F);
                } else {
                    yield return new WaitForSeconds(1.3F);
                    yield return StartCoroutine(CoDestroyAllSpecialPiece());
                }

                PlayTime += timesPerBonus;
                JMFRelay.FireOnChangeRemainTime(Mathf.Max(0F, GivenTime - PlayTime));
            }
        } //else*/
#endregion

            
        // ## 다수에 걸쳐 바꿔야 할 회수를 구한다.
        int bonusCount              = CurrentLevel.isTimerGame ? (int)(GivenTime-PlayTime) : AllowedMoves - Moves;
        List<int> listFireCount     = _getFireCount(bonusCount);
        bool firstFire              = true;

        // ## 수만큼 for를 돈다.
        for (int z = 0; z < listFireCount.Count; ++z)
        {
            Debug.Log("=== waiting bonus bursting fly !!! === ");
    
            float flyTime           = 1.0f;
            List<Board> listTargets = new List<Board>();
            int countChangePerPlay  = listFireCount[z];
            int cntChanged          = 0;

            int sndCounter          = 0;
            bool sndUp              = true;            

#region ## 회당 회수만큼만 특수 블럭으로 바꾼다.
            for (cntChanged = 0; cntChanged < countChangePerPlay; ++cntChanged)
            {
                if(CurrentLevel.isTimerGame)    PlayTime += (2*movesPerBonus);
                else                            Moves += movesPerBonus;

                // 대상 보드를 찾는다.
                Board target            = null;
                int limit               = 0;
                do
                {
                    // 이 보드가 유일한지 검사하고, 그렇지 않을 경우 limit check를 한다.(무한 loop방지)
                    target              = GetBonusTargetBoard(ref listTargets);

                    // Note : CPR update. 중복되도 그냥 counting 한다. by wayne. 171020.
                    if(false == listTargets.Contains( target ))
                        listTargets.Add(target);

                    break;

                    /*if(false == listTargets.Contains(target))
                    {
                        listTargets.Add(target);
                        break;
                    }
                    ++limit;
                    if(limit >= 1000000)
                    {
                        EndGame(GAMEOVER_REASON.NONE, true, GetStarGrade());
                        yield break;
                    }*/
                }while(true);

                float fDelay    = 0.1f * (float)cntChanged;
                int remains     = CurrentLevel.isTimerGame ? (int)(GivenTime-PlayTime): AllowedMoves - Moves;
                StartCoroutine( _coFireBonusStar(fDelay, remains, target) );

                flyTime         = Mathf.Max(flyTime, fDelay+0.25f);

                // 전체 종료 조건 check.
                if(CurrentLevel.isTimerGame)
                {
                    if(GivenTime <= PlayTime)
                        break;
                }
                else
                {
                    if (AllowedMoves <= Moves)
                        break;
                }
            }
#endregion

            yield return new WaitForSeconds(flyTime+0.05f);

            // ## 시간 간격을 두고 파괴시킨다.
            yield return StartCoroutine(CoDestroyAllSpecialPiece());
            yield return StartCoroutine( coWaitTillStable() );

            // ## 7Rainbow나 Rainbow블럭이 있으면 터뜨린다.
            yield return StartCoroutine( _coProcRainbowPieces() );
            yield return StartCoroutine( coWaitTillStable() );

            // 종료 조건 check.
            if(CurrentLevel.isTimerGame)
            {
                if(GivenTime <= PlayTime)
                    break;
            }
            else
            {
                if (AllowedMoves <= Moves)
                    break;
            }
        }

#region commented.
        /*
        old logic. -> commented.
        while (AllowedMoves > Moves)
        {
            if (ConvertNormalToSpecial())
            {
                yield return new WaitForSeconds(0.1F);
            } else {

                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(CoDestroyAllSpecialPiece());

                while (IsFinishDropAndMatch == false) yield return null;
                yield return new WaitForSeconds(0.3F);
            }

            Moves += movesPerBonus;
            JMFRelay.FireOnChangeRemainMove(AllowedMoves - Moves);
        }*/

        // yield return new WaitForSeconds(1.3F);
        // yield return StartCoroutine(CoDestroyAllSpecialPiece());
        // while (IsRemainSpecialPiece()) yield return new WaitForSeconds(1F);
#endregion

        yield return StartCoroutine( coWaitTillStable() );

        yield return StartCoroutine(CoDestroyAllSpecialPiece());
        yield return StartCoroutine( coWaitTillStable() );

        // 아무것도 떠뜨릴게 없을때 까지 더한다.
        while(_hasBonustBurst(true) || _hasBonustBurst(false))
        {
            if(true == _hasBonustBurst(true))
            {
                yield return StartCoroutine( _coProcRainbowPieces() );
                yield return StartCoroutine( coWaitTillStable() );
            }
            if(true == _hasBonustBurst(false))
            {
                yield return StartCoroutine(CoDestroyAllSpecialPiece());
                yield return StartCoroutine( coWaitTillStable() );
            }
            yield return null;
        }
        //

        NNSoundHelper.Stop("IFX_bonus_time_bg");
        playSceneHandler.triggerDarkBGEffect(false);

        EndGame(GAMEOVER_REASON.NONE, true, GetStarGrade());
	}

    IEnumerator _coFireBonusStar(float fDelay, int remainMoves, Board target)
    {
        yield return new  WaitForSeconds(fDelay);

        if(CurrentLevel.isTimerGame)
            JMFRelay.FireOnChangeRemainTime(remainMoves);
        else 
            JMFRelay.FireOnChangeRemainMove(remainMoves);

        // fire star effect.
        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = SR.GetComponent<SpriteLibrary>().getSpriteByName("spr_star");// playOverlayHandler.collectAnyMission.getJewelSkinSprite(colorIndex);
        SR.transform.position   = playOverlayHandler._objMoveCountUI.transform.Find("Count").position;
        SR.transform.localScale = Vector3.one * 2.0f;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;
        SR.transform.DOMove( target.Position, 0.25f);
        NNSoundHelper.Play("IFX_fire_and_fly");

        // fire point effect.
        PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		effect.ChangeColor(1, target.ColorIndex);
		effect.Play(SR.transform.position, target.Piece.Scale, false, .0f, 1.0f, target.Piece.GO.transform);

        //BonusStarEff eff= NNPool.GetItem<BonusStarEff>("BonusStarEffect");
        float flyTime           = 0.25f;// eff.Play(playOverlayHandler._objMoveCountUI.transform.position, target.Position);
        //eff.transform.position   += Vector3.back*65.0f;
        //_playBonusSound(ref sndCounter, ref sndUp);
                
        int bonus               = (int)((float)bonusScore * 1.1f);
            //firstFire ? bonusScore : (int)((float)bonusScore * 1.1f);
        //firstFire               = false;
        StartCoroutine( _coConvertNormalToSpecial(flyTime-0.05f, target, bonus) );

        yield return new WaitForSeconds(flyTime-0.05f);

        SR.transform.DOKill();
        SR.transform.localScale = Vector3.one;
        NNPool.Abandon(SR.gameObject);
    }

    // 로직에 맞게 sound 재생.
    void _playBonusSound(ref int sndCounter, ref bool sndUp)
    {
        const int MAX_SND       = 7;
        // set sounds.
        if(true == sndUp)
        {
            ++sndCounter;
            if(sndCounter > MAX_SND)
            {
                sndCounter      = MAX_SND - 1;
                sndUp           = false;
            }
        }
        else
        {
            --sndCounter;
            if(sndCounter <= 0)
            {
                sndCounter      = 2;
                sndUp           = true;
            }
        }
        NNSoundHelper.Play(string.Format("IFX_block_match_0{0}", sndCounter));
        // NNSoundHelper.Play("IFX_bomb_block_create");
        //Debug.Log(string.Format("Bonus sound played >>> IFX_block_match_0{0} <<<", sndCounter));
        //
    }

    // 특정 시간동안 pump가 안정화 되길 기다린다.
    public IEnumerator coWaitTillStable(float fTime = 0.1f)
    {
        float fElTime           = .0f;
        do
        {
            if(Pump==idlePump && IsAllBoardStable)
                fElTime += Time.deltaTime;
            else 
                fElTime         = .0f;

            if(fElTime > fTime)
                yield break;

            yield return null;

        }while(true);
    }


    // bonust burst target이 있나.
    // special5 : true (rainbow), false(just booster)
    bool _hasBonustBurst(bool special5)
    {
        foreach(Board bd in boards)
        {
            if (bd.IsFilled == false) continue;
            if (bd.Panel.IsDestroyablePiece() == false)     continue;
            if (null!=bd.Piece && bd.Piece.LifeCover>0)     continue;

            if(true==special5 && (bd.PD is SpecialFive || bd.PD is TMatch7Piece))
                return true;
            
            if(false==special5 && (bd.PD is BombPiece || bd.PD is VerticalPiece || bd.PD is HorizontalPiece))
                return true;
		}
        return false;
    }

    IEnumerator _coProcRainbowPieces()
    {
        do
        {
            Board bdRainbow     = null;
		    foreach(Board bd in boards)
            {
                if (bd.IsFilled == false) continue;
                if (bd.Panel.IsDestroyablePiece() == false)     continue;
                if (null!=bd.Piece && bd.Piece.LifeCover>0)     continue;

                if(bd.PD is SpecialFive || bd.PD is TMatch7Piece)
                {
                    bdRainbow   = bd;
                    break;
                }
		    }
            if(null == bdRainbow)
                yield break;

            yield return StartCoroutine( coWaitTillStable() );
            yield return StartCoroutine(CoDestroyAllSpecialPiece());
            yield return StartCoroutine( coWaitTillStable() );

            bdRainbow.Piece.setIsDestroyable(true);
            float duration      = bdRainbow.PD.bonusHit(bdRainbow);
            yield return new WaitForSeconds(duration+0.5f);

        }while(true);
    }

    IEnumerator _coConvertNormalToSpecial(float fDelay, Board target, int bonus)
    {
        yield return new WaitForSeconds(fDelay);

        ConvertNormalToSpecial( target );

        IncreaseScore(bonus, target.PT, 6);
    }

    List<int> _getFireCount(int countBonus)
    {
        List<int> listCount     = new List<int>();
        
        // Note : CPR update. 한번에 처리 되도록 변경. by wayne. 171020.
        listCount.Add( countBonus );

        /*
        //
        // 기획서 data 규약에 따른다.
        // https://docs.google.com/spreadsheets/d/1Rbol4tEHWMZ7jyw-34j-4M21ndO6lwk10YhjDoAs6bk/edit#gid=38060574
        //
        if(countBonus <= 6)     listCount.Add(countBonus);
        else if(7==countBonus)  { listCount.Add(4); listCount.Add(3); }
        else if(8==countBonus)  { listCount.Add(4); listCount.Add(4); }
        else if(9==countBonus)  { listCount.Add(5); listCount.Add(4); }
        else if(10==countBonus) { listCount.Add(5); listCount.Add(5); }
        else if(11==countBonus) { listCount.Add(6); listCount.Add(5); }
        else if(12==countBonus) { listCount.Add(6); listCount.Add(6); }
        else if(13==countBonus) { listCount.Add(5); listCount.Add(4); listCount.Add(4); }
        else if(14==countBonus) { listCount.Add(5); listCount.Add(5); listCount.Add(4); }
        else
        {
            int div             = 6;
            for(int k = 0; k < countBonus/div; ++k)
                listCount.Add(div);
            listCount.Add(countBonus% div);
        }
        */
        return listCount;
    }

    bool ExistBonusTime () {
        if (CurrentLevel.isTimerGame) {
            if (GivenTime > PlayTime) return true;
        } else {
            if (AllowedMoves > Moves) return true;
        }

        if (GetDestroyableSpecialPiece() != null) return true;

        return false;
    }

    //bool IsRemainSpecialPiece() {
    //    if (makingSpecialPiece) return true;
    //    Board sp = GetDestroyableSpecialPiece();
     //   if (sp != null) {
     //       sp.Hit();
    //        return true;
    //    } 
    //    if (IsFinishDropAndMatch == false) return true;
    //    return false;
    //}

    IEnumerator CoDestroyAllSpecialPiece () {

        yield return null;

        // CPR update. 소리 1회만 나도록.
        int[] soundCount        = new int[] { 1, 1 };
        //int[] soundCount        = new int[] { 3, 3 };
        ActMassMatching         = true;

        Board sp = GetDestroyableSpecialPiece();

	    while (sp != null) {

            // 적절한 수 만큼의 sound만 play 되도록 manage 한다.
            int idxTarget   = -1;
            if(soundCount[0]>=0 && (sp.PD is VerticalPiece || sp.PD is HorizontalPiece))
                idxTarget   = 0;
            else if(soundCount[1]>=0 && (sp.PD is BombPiece))
                idxTarget   = 1;
                
            if(idxTarget>=0 && idxTarget<soundCount.Length)
            {
                if(null != sp.Piece)    sp.Piece.mPlayWhenMassMatching    = true;
                --soundCount[idxTarget];
            }
            //

            sp.Hit();
            //while (IsFinishDropAndMatch == false) yield return null; //delay 0
            // delay 없이 동시에 터지게 하는 것으로 변경 by wayne [17.06.06]
            // = yield return new WaitForSeconds(0.1F);
            sp = GetDestroyableSpecialPiece();
        }

        ActMassMatching         = false;
    }

	public System.Byte GetStarGrade () 
	{
		if (Score >= CurrentLevel.scoreToReach[2]) {
			return 3;
		} else if (Score >= CurrentLevel.scoreToReach[1]) {
			return 2;
		} else if (Score >= CurrentLevel.scoreToReach[0]) {
			return 1;
		} else {
            return 0;
        }
    }

	/*bool ConvertNormalToSpecial () {
        //NNSoundHelper.Play("bonus_points");
		Board target = GetBonusTargetBoard();

        IncreaseScore(bonusScore, target.PT, 6);

        ConvertNormalToSpecial(target);

        return true;
	}*/

	Board GetBonusTargetBoard (ref List<Board> listAdded)
    {
		List<Board> candidates = new List<Board>();

		foreach (Board bd in boards) {
            if (bd.IsFilled == false) continue;
            if ((bd.PD is NormalPiece) == false) continue;
            if (bd.Panel.IsDestroyablePiece() == false) continue;
            if(null!=bd.Piece && bd.Piece.LifeCover>0)  continue;
            if(true==listAdded.Contains( bd ))  continue;           // 중복방지.

			candidates.Add(bd);
		}

        if (candidates.Count > 0) {
            return candidates[NNTool.Rand(0, candidates.Count)];
        } else {

            // 없다면 기존 목록에서 그냥 하나 반환한다.
            if(listAdded.Count > 0)
                return listAdded[ NNTool.Rand(0, listAdded.Count) ];

            return null;
        }
	}

    Board GetDestroyableSpecialPiece () {
        for (int i = 0; i < boards.Count; i++) {
            if (this[i].IsFilled == false) continue;
            if (this[i].Panel.IsDestroyablePiece() == false) continue;
            if (null!=this[i].Piece && this[i].Piece.LifeCover>0)   continue;
            if ((this[i].PD is HorizontalPiece) || (this[i].PD is VerticalPiece) ||
                (this[i].PD is BombPiece) )
            {
                // rainbow는 별도 처리 => || (this[i].PD is SpecialFive) || (this[i].PD is TMatch7Piece) ) 

                return this[i];
            }
        }
		
        return null;
    }

    public float SuggestPiece ()
	{
        bool failToSuggest      = (!_isSuggestByCombine && suggestedPieces.Count<3) || (_isSuggestByCombine && suggestedPieces.Count<=0);
		// org coe => if(suggestedPieces.Count<3 || moverSuggested == null || targetSuggested == null) 
        if(failToSuggest || moverSuggested == null || targetSuggested == null) 
		{
			// [AI_MISSION]
			moverSuggested = null;
			targetSuggested = null;
            Dictionary<Board, List<Board>> dicRet = GetSuggestablePieces(ref suggestedPieces, false);
			if(0 == dicRet.Count)
				return 0.0f;
		}

		if(!PieceTracker.isBeingDragged)
			return PlaySuggestPiece();
		else
			return 1.0f;
    }
	
    Dictionary<Board, int>      _replacedDict = new Dictionary<Board, int>();
	public float ShufflingPiece()
	{
        // logical change.
		if(!ShuffePiece())
			return 0.0f;
		
        // view replace.
        NNSoundHelper.Play( "IFX_suff" );
		float duration = playOverlayHandler.OnShuffle();

        StartCoroutine(_coShuffle(duration));

        duration += 3.0f;
        return duration;
    }

    IEnumerator _coShuffle(float delay)
    {
        yield return new WaitForSeconds(delay);

        // fox's pos.           - to world.
        Vector3 toPos           = playOverlayHandler._objBuffBox.transform.parent.TransformPoint( playOverlayHandler._objBuffBox.transform.localPosition ) + Vector3.right;
        //                      - to local.
        toPos                   = transform.InverseTransformPoint(toPos);

        List<Vector3> listPos   = new List<Vector3>();
        
        // gether all to the center.
        foreach (Board bd in _replacedDict.Keys)
        {
            if(null == bd.Piece)    continue;
            StartCoroutine( _coMoveShufflePiece(UnityEngine.Random.Range(.0f, 0.2f), bd.Piece.GO.transform, toPos) );
            listPos.Add( bd.Piece.GO.transform.localPosition );
            
        }
        yield return new WaitForSeconds(1.2f);
       
        // mix all.
        foreach (Board bd in _replacedDict.Keys)
        {
            if(null == bd.Piece)    continue;
            bd.ResetPiece( bd.PD, bd.Piece.ColorIndex );
            bd.Piece.GO.transform.localPosition = new Vector3(toPos.x, toPos.y, bd.Piece.GO.transform.localPosition.z);
        }
        yield return new WaitForSeconds(0.5f);

        // move to original board pos.
        int idx                 = 0;
        foreach (Board bd in _replacedDict.Keys)
        {
            if(null == bd.Piece)    continue;
            StartCoroutine( _coMoveShufflePiece(UnityEngine.Random.Range(.0f, 0.2f), bd.Piece.GO.transform, listPos[idx]) );
            idx++;
        }
        yield return new WaitForSeconds(1.2f);

        _replacedDict.Clear();

        #region => old codes.
        /*
		float d = 0.0f;
		int H = HEIGHT - 1;
		int XX, YY = 0;
		
		for (int n = 0; n < WIDTH * 2 -1; ++n) 
		{
			int x = n;

			XX = x;
			YY = H;

			if(n >= WIDTH)
			{
				x = (WIDTH - 1) - (x - WIDTH) - 1;
				XX = H;
				YY = x;
			}

			int i = HEIGHT - (H - x);
			bool bSum = false;	
			while(i > 0)
			{
				--i;
				Board bd = this[XX - i,YY - i];
				
                if(true == _replacedDict.ContainsKey(bd))
				{
					bSum = true;
					//d = bd.Piece.Play("shuffling", false, duration + 0.2f);
					//BlockCrash effect = NNPool.GetItem<BlockCrash>("ShufflingHit");
					//Vector3 pos = bd.Piece.Position;
					//pos.z -= 2.0f; 
					//effect.Play("play",pos,Vector3.one, bd.Piece.ColorIndex, false,duration,0.75f);	
                    d = 0.1f;
                    bd.ResetPiece( bd.PD, bd.Piece.ColorIndex );
					
				}
			}	
			
			if(bSum)
				duration += 0.1f;
		}
		
        _replacedDict.Clear();
		return duration + d + 0.2f;
        */
        #endregion

    }


    IEnumerator _coMoveShufflePiece(float delay, Transform trTarget, Vector2 vTarget)
    {
        yield return new WaitForSeconds(delay);

        trTarget.DOLocalMoveX(vTarget.x, 1.0f).SetEase(Ease.OutBack);
        trTarget.DOLocalMoveY(vTarget.y, 1.0f).SetEase(Ease.OutBack);
    }

    public float IdlePiece()
	{
		float duration = 0.0f;
		int H = HEIGHT - 1;
		int XX, YY = 0;
		for (int n = 0; n < WIDTH * 2 -1; ++n) 
		{
			int x = n;
			XX = x;
			YY = H;
			
			if(n >= WIDTH)
			{
				x = (WIDTH - 1) - (x - WIDTH) - 1;
				XX = H;
				YY = x;
			}
			
			int i = HEIGHT - (H - x);
			
			while(i > 0)
			{
				--i;
				Board bd = this[XX - i,YY - i];
				if(bd.Piece != null && (bd.PD is NormalPiece || bd.PD is VerticalPiece || bd.PD is HorizontalPiece || bd.PD is BombPiece || bd.PD is SpecialFive || bd.PD is TMatch7Piece || bd.PD is StrawberryPiece || bd.PD is SugarCherryPiece || bd.PD is SugaredPiece))
					bd.Piece.Play("idle", false, duration);
			}	
			duration += 0.1f;
		}
		
		return duration;
	}
	
	float PlaySuggestPiece()
	{
        NNSoundHelper.Play("IFX_block_hint");
        
		string aniMoverName = "";
		string aniTargetNamebd = "";
		
		if(moverSuggested.Bottom == targetSuggested)
		{
			aniMoverName = "hit_idle_mover_V_B";
			aniTargetNamebd = "blocking_target_V_B";
		}
		else if(moverSuggested.Left == targetSuggested)
		{
			aniMoverName = "hit_idle_mover_H_L";
			aniTargetNamebd = "blocking_target_H_L";
		}
		else if(moverSuggested.Right == targetSuggested)
		{
			aniMoverName = "hit_idle_mover_H_R";
			aniTargetNamebd = "blocking_target_H_R";
		}
		else if(moverSuggested.Top == targetSuggested)
		{
			aniMoverName = "hit_idle_mover_V_T";
			aniTargetNamebd = "blocking_target_V_T";
		}
		
		Vector3 pos = targetSuggested.Piece.GO.transform.position;
		Vector3 pos1 = moverSuggested.Piece.GO.transform.position;
		moverSuggested.Piece.Play(aniMoverName, false);
		
		if(targetSuggested.PD is RoundChocoPiece || targetSuggested.PD is GreenBubblePiece || targetSuggested.PD is ZellatoPiece || targetSuggested.PD is Potion1Piece || targetSuggested.PD is Potion2Piece || targetSuggested.PD is Potion3Piece)
			aniTargetNamebd = "hard_" + aniTargetNamebd;
		
		targetSuggested.Piece.Play(aniTargetNamebd, false);

		for (int i = 0; i< suggestedPieces.Count; ++i)
		{
			if(suggestedPieces[i] != moverSuggested)
				suggestedPieces[i].Piece.Play("hit_idle", false);
		}

		Sequence seq = DOTween.Sequence();
		seq.Append(moverSuggested.Piece.GO.transform.DOMoveX(targetSuggested.Position.x, switchSpeed));
        seq.Join(moverSuggested.Piece.GO.transform.DOMoveY(targetSuggested.Position.y, switchSpeed));
		seq.Insert(switchSpeed, moverSuggested.Piece.GO.transform.DOMoveX(moverSuggested.Position.x, switchSpeed).SetEase(Ease.Linear));
        seq.Join(moverSuggested.Piece.GO.transform.DOMoveY(moverSuggested.Position.y, switchSpeed).SetEase(Ease.Linear));

//		seq.Append(moverSuggested.Piece.GO.transform.DOMove(targetSuggested.Position, switchSpeed));
//		seq.Insert(switchSpeed, moverSuggested.Piece.GO.transform.DOMove(moverSuggested.Position, switchSpeed).SetEase(Ease.Linear));
		return switchSpeed * 2.0f;
	}

    void StopSuggestPiece ()
	{
		if(moverSuggested != null && moverSuggested.Piece != null && moverSuggested.Piece.GO != null)
			DOTween.Kill(moverSuggested.Piece.GO.transform);
		
		for (int i = 0; i< suggestedPieces.Count; ++i)
		{
			suggestedPieces[i].Piece.Play("normal", false);            
		}
    }

    void Timer () {
        if (State != JMF_GAMESTATE.PLAY) return;

        PlayTime++;
        float remainTime = GivenTime - PlayTime;

        JMFRelay.FireOnChangeRemainTime(Mathf.Max(0F, remainTime));

        if(PlayTime > GivenTime)
            ExecuteFail();
    }

	bool ShuffePiece() 
	{
		int tryCount = 0;
        int sameColorTryCount   = 0;
		
		moverSuggested = null;
		targetSuggested = null;
        Dictionary<Board, List<Board>> dicRet = GetSuggestablePieces(ref suggestedPieces, false);
		
		//suggestedPieces = GetSuggestablePieces2(ref targetSuggested, ref moverSuggested);
		//Dictionary<Board, int> replacedDict = new Dictionary<Board, int>();
		// Replace Logic
		while(true)             // suggestedPieces.Count == 0) 
		{
			_replacedDict.Clear();
			// replacedDict.Clear();
			
			for (int i = 0; i < boards.Count; i++) 
			{
				if (this[i].IsFilled == false)          continue;
				if (this[i].PD.isMatchable == false)    continue;
				if (this[i].PD.ignoreShuffle)           continue;
                if(null!=this[i].PND && false==this[i].PND.IsShufflable(this[i].Panel))
                    continue;
                if(null!=this[i].Piece && this[i].Piece.LifeCover>0)
                    continue;

                int idxOldColor = this[i].ColorIndex;   // violet보다 크면 일반 piece가 아니다.
                if(idxOldColor==(int)LEItem.COLOR.STRAWBERRY || idxOldColor==(int)LEItem.COLOR.ZELLATO)
                    continue;

                int colorIndex  = GetRandomColorIndex();
                while(idxOldColor == colorIndex)
                {
                    ++sameColorTryCount;
                    if (sameColorTryCount > 999)
			        {
				        EndGame(GAMEOVER_REASON.NO_MORE_TO_MATCH, false, 0);
				        return false;
			        }

                    colorIndex  = GetRandomColorIndex();
                }
				this[i].ShuffePieceColorIndex( colorIndex);
				_replacedDict.Add(this[i], colorIndex);
				//replacedDict.Add(this[i], colorIndex);
			}

            moverSuggested = null;
		    targetSuggested = null;
			//suggestedPieces = GetSuggestablePieces2(ref byCombine);
			// suggestedPieces = GetSuggestablePieces2(ref targetSuggested, ref moverSuggested);
            dicRet              = GetSuggestablePieces(ref suggestedPieces, false);
			
			// No More Move Game Over
			if (tryCount > 999)
			{
				EndGame(GAMEOVER_REASON.NO_MORE_TO_MATCH, false, 0);
				return false;
			}

            if(suggestedPieces.Count > 0)
                break;

			tryCount++;
		}

		// Replace Result
		
		foreach (Board bd in _replacedDict.Keys)
		{
			if(bd.PD is NormalPiece || bd.PD is SugaredPiece)
			{
				Block _block = bd.Piece.GO.GetComponent<Block>();
				_block.ShuffIndex = _replacedDict[bd];
			}
		}
		
		return true;
	}
	
//	IEnumerator CoShuffle () 
//	{
//        State = JMF_GAMESTATE.SHUFFLE;
//
//		JMFRelay.FireOnShuffle();
//
//        int tryCount = 0;
//        Dictionary<Board, List<Board>> suggestionPieces = GetSuggestablePieces();
//        Dictionary<Board, int> replacedDict = new Dictionary<Board, int>();
//
//        yield return new WaitForSeconds(1F);
//
//        // Replace Logic
//        while (suggestionPieces.Count == 0) 
//		{
//            replacedDict.Clear();
//
//            for (int i = 0; i < boards.Count; i++) {
//                if (this[i].IsFilled == false) continue;
//                if (this[i].PD.isMatchable == false) continue;
//                if (this[i].PD.ignoreShuffle) continue;
//
//                int colorIndex = GetRandomColorIndex();
//                this[i].ResetPiece(GetPieceType<NormalPiece>(), colorIndex);
//                replacedDict.Add(this[i], colorIndex);
//            }
//
//            suggestionPieces = GetSuggestablePieces();
//
//            if (tryCount > 999) break;
//
//            tryCount++;
//        }
//
//        // Replace Effect
//        for (int i = 0; i < 10; i++)
//		{
//            foreach (Board bd in replacedDict.Keys) 
//			{
//                bd.ResetPiece(GetPieceType<NormalPiece>(), GetRandomColorIndex());
//            }
//
//            yield return new WaitForSeconds(0.05F);
//        }
//
//        // No More Move Game Over
//        if (tryCount > 999)
//		{
//            EndGame(GAMEOVER_REASON.NO_MORE_TO_MATCH, false, 0);
//            yield break;
//        }
//
//        // Replace Result
//        foreach (Board bd in replacedDict.Keys)
//		{
//            bd.ResetPiece(GetPieceType<NormalPiece>(), replacedDict[bd]);
//        }
//
//        State = JMF_GAMESTATE.PLAY;
//	}


    public void playComboVoiceFx()
    {
        if(JMF_GAMESTATE.PLAY != State)
            return;

        int seqCount            = ComboCount - 1;

        int voiceNum            = -1;
        if (3 == seqCount)      voiceNum    = 1;
        else if(4==seqCount)    voiceNum    = 2;
        else if(5==seqCount)    voiceNum    = 3;
        else if(6<=seqCount)    voiceNum    = 4;

        if(voiceNum>=1)
            NNSoundHelper.Play("VFX_cascade");
            //NNSoundHelper.Play("VFX_cascade"+voiceNum);
    }

	public void IncreaseCombo () {
        
        // note : combo는 1부터 시작한다.
        //NNSoundHelper.Play("IFX_block_match_0"+Mathf.Min(ComboCount, 7));
        NNSoundHelper.Play("IFX_block_match_01");

        ComboCount++;
		JMFRelay.FireOnIncreaseCombo(ComboCount);
	} 

    public void SplashHit(Board baseBoard, float fDelay, List<JMF_DIRECTION> listSkip=null, int matchedColor=-1)
    {
        if( (baseBoard.PND is CagePanel) ||     // cage 제외.
            (baseBoard.PND is BubblePanel) )    // bubble 제외.
            return; 
        if(null!=baseBoard.Piece && baseBoard.Piece.LifeCover>0)
            return;

        StartCoroutine( _splashHitWithDelay(baseBoard, fDelay, listSkip, matchedColor) );
    }

    IEnumerator _splashHitWithDelay(Board baseBoard, float fDelay, List<JMF_DIRECTION> listSkip, int matchedColor)
    {
        yield return new WaitForSeconds(fDelay);

        List<Board> targetBoards = new List<Board>();
        foreach (Board bd in baseBoard.Neighbours)
        {
            if (baseBoard.Equals(bd)) continue;
            if (targetBoards.Contains(bd)) continue;
            if(null!=bd.PND && null!=bd.Panel)
                bd.Panel.setDamagingColor( matchedColor+1 );

#region // check skip direction.
            bool wouldContinue  = false;
            if(null!=listSkip)
            {
                for(int q = 0; q < listSkip.Count; ++q)
                {
                    Board bdSkip= null;
                    switch(listSkip[q])
                    {
                    case JMF_DIRECTION.DOWN:    bdSkip = baseBoard.Bottom;  break;
                    case JMF_DIRECTION.UP:      bdSkip = baseBoard.Top;     break;
                    case JMF_DIRECTION.RIGHT:   bdSkip = baseBoard.Right;   break;
                    case JMF_DIRECTION.LEFT:    bdSkip = baseBoard.Left;    break;
                    }

                    if(null!=bdSkip && true==bdSkip.Equals(bd))
                    {
                        wouldContinue= true;
                        break;
                    }
                }
            }
#endregion
            if (wouldContinue)   continue;

            targetBoards.Add(bd);
        }
        for (int i = 0; i < targetBoards.Count; i++) {
			targetBoards[i].SplashHit();
		}
    }

	public void SplashHit (List<Board> baseBoards, float fDelay=.0f)
    {
        for (int i = 0; i < baseBoards.Count; i++)
        {
            StartCoroutine( _splashHitWithDelay(baseBoards[i], fDelay, null, -1) );
        }
#region // COMMENT - old logic.
        /*List<Board> targetBoards = new List<Board>();

        for (int i = 0; i < baseBoards.Count; i++) {

            if( (baseBoards[i].PND is CagePanel) ||     // cage 제외.
                (baseBoards[i].PND is BubblePanel) )    // bubble 제외.
                continue; 

            foreach (Board bd in baseBoards[i].Neighbours) {
                if (baseBoards.Contains(bd)) continue;
                if (targetBoards.Contains(bd)) continue;
                targetBoards.Add(bd);
            }
        }

		for (int i = 0; i < targetBoards.Count; i++) {
			targetBoards[i].SplashHit(delay);
		}*/
#endregion
    }

    bool CanBeMatched (Board bd, JMF_DIRECTION except) {
        List<Board> listOut     = new List<Board>();
        return CanBeMatched(bd, bd.ColorIndex, except, ref listOut);
		// return CanBeMatched(bd, bd.ColorIndex, except);
    }

    public List<Board> GetRowMatchableBoards (Board bd, int colorIndex, JMF_DIRECTION except)
    {
        List<Board> row = new List<Board>();

        if (except != JMF_DIRECTION.LEFT) row.AddRange(GetMatchableBoards(bd, colorIndex, JMF_DIRECTION.LEFT));
        if (except != JMF_DIRECTION.RIGHT) row.AddRange(GetMatchableBoards(bd, colorIndex, JMF_DIRECTION.RIGHT));

        return row;
    }

    public List<Board> GetColMatchableBoards (Board bd, int colorIndex, JMF_DIRECTION except)
    {
        List<Board> col = new List<Board>();

        if (except != JMF_DIRECTION.DOWN) col.AddRange(GetMatchableBoards(bd, colorIndex, JMF_DIRECTION.DOWN));
        if (except != JMF_DIRECTION.UP) col.AddRange(GetMatchableBoards(bd, colorIndex, JMF_DIRECTION.UP));

        return col;
    }

    void SwapPieceWithItem(Point ptA, Point ptB) {
        NNSoundHelper.Play("IFX_block_swap");
        Board bdA = this[ptA];
        Board bdB = this[ptB];
		bdA.State = Board.STATE.SWAP;
		bdB.State = Board.STATE.SWAP;

        // curtain 등 연출 처리.
        StartCoroutine( playOverlayHandler.UseSwapJewel(bdA, bdB) );

        Sequence seq = DOTween.Sequence();
        seq.Insert(0.8F, bdA.Piece.GO.transform.DOMoveX(bdB.Position.x, switchSpeed));
        seq.Insert(0.8F, bdA.Piece.GO.transform.DOMoveY(bdB.Position.y, switchSpeed));
        seq.Insert(0.8F, bdB.Piece.GO.transform.DOMoveX(bdA.Position.x, switchSpeed));
        seq.Insert(0.8F, bdB.Piece.GO.transform.DOMoveY(bdA.Position.y, switchSpeed));

        seq.OnComplete(() => {
           // NNPool.Abandon(changeEffect.gameObject); 
            bdA.State = Board.STATE.STABLE;
            bdB.State = Board.STATE.STABLE;
			
            GamePiece holder = bdA.Piece;
            bdA.Piece = bdB.Piece;
            bdB.Piece = holder;

            bdA.IsNeedMatchCheck = true;
            bdB.IsNeedMatchCheck = true;
            //==> playOverlayHandler.UseSwapJewel();
            OnPlayerMove();
        });
    }

    public void ConvertNormalToSpecial (Board target) {
        if (target == null) return;
        switch (NNTool.Rand(0,2)) {
            case 0:
                target.ResetPiece(GetPieceType<HorizontalPiece>(), target.ColorIndex);
                break;
            case 1:
                target.ResetPiece(GetPieceType<VerticalPiece>(), target.ColorIndex);
                break;
            default:    return;
        }
        target.Piece.Play("create", false, 0.0f, null);

        NNSoundHelper.Play("IFX_bubble_bust");

        //PanelNetHit effect      = NNPool.GetItem<PanelNetHit>("Panel_net_hit");				
		//effect.ChangeColor(1, target.ColorIndex);
		//effect.Play(target.Position, target.Piece.Scale, false, .0f, 1.0f, target.Piece.GO.transform);
        BlockCrash effect       = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
		effect.Play("play", target.Position, Vector3.one, 0);
        effect.transform.position += Vector3.back * 100.9f;
    }

    List<Board> GetMatchableBoards (Board bd, int colorIndex, JMF_DIRECTION direction) {
        Debugger.Assert(bd != null, "GameManager.GetMatchableBoards : Board is null.");

        List<Board> bds = new List<Board>();
        Board targetBoard = bd[direction];
        
        while (targetBoard != null) {
            if (IsMatchableBoard(targetBoard, colorIndex)) {

                /*
                note : stone piece로 인한 block 효과는 제거한다. by wayne.
                if (bd.PD is StonePiece) {
					if (!bd.PD.IsSwitchableVertical(bd.Piece) && !bd.PD.IsSwitchableHorizontal(bd.Piece)) {
						break;
					} else if (!bd.PD.IsSwitchableVertical(bd.Piece)) {
						if(direction == JMF_DIRECTION.UP || direction == JMF_DIRECTION.DOWN) {
							break;
						}
					} else if (!bd.PD.IsSwitchableHorizontal(bd.Piece)) {
						if (direction == JMF_DIRECTION.LEFT || direction == JMF_DIRECTION.RIGHT) {
							break;
						}
					}
				}*/

				bds.Add(targetBoard);
				targetBoard = targetBoard[direction];

            } else {
                break;
            }
        }

        return bds;
    }

    bool IsMatchableBoard (Board bd, int colorIndex) {
        if (bd.IsMatchable == false) return false;
        if (bd.ColorIndex != colorIndex) return false;
//#if BOMBx2
        if (DestroyAwaiter.Contains(bd.Piece)) return false;
//#endif

        return true;
    }

    List<Board> GetSameColorBoards (Board bd, JMF_DIRECTION direction) {
        return GetSameColorBoards(bd, bd.ColorIndex, direction);
    }

    List<Board> GetSameColorBoards (Board bd, int colorIndex, JMF_DIRECTION direction) {
        Debugger.Assert(bd != null, "GameManager.GetSameColorBoards : Board is null.");

        List<Board> sameColorBoards = new List<Board>();

        Board next = bd;

        do {
            if (next[direction] == null) break;
            if (next[direction].IsMatchable == false) break;
            if (next[direction].ColorIndex != colorIndex) break;

            sameColorBoards.Add(next[direction]);
            next = next[direction];
        } while (next != null);

        return sameColorBoards;
    }
	
    List<int> GetColorIndices ()
	{
        if (CurrentLevel.normalProbability == null) 
            probability = new List<int>(){100, 100, 100, 100, 100, 100, 100, 100};
        else
            probability = new List<int>(CurrentLevel.normalProbability);
        
        List<int> indices = new List<int>(){0,1,2,3,4,5, 6, 7};

        for (int i = 0; i < probability.Count; i++) 
		{
            if (probability[i] == 0) indices.Remove(i);
        }

        /*
        if (CurrentLevel.StraightFailCount > 7) {
            if (indices.Count > 5) {
                indices.Remove(1);
            } else {
                indices.Remove(2);
            }
        } else if (CurrentLevel.StraightFailCount > 10) {
            indices.Remove(1);
            indices.Remove(2);
        }
        */

        return indices;
    }

	List<Board> GetCandidatesOfMatch(Board bd, ref List<Board> listMatches, bool findAll)
	{
        List<Board> bds = new List<Board>();
        listMatches.Clear();

        if(null!=bd.PD && false==bd.PD.IsSwitchableHorizontal(bd.Piece) && false==bd.PD.IsSwitchableVertical(bd.Piece))
            return bds;

        if (bd.IsMatchable) 
		{
            if (bd.Panel.IsSwitchable() == false) return bds;

            JMF_DIRECTION[] dirs= new JMF_DIRECTION[] { JMF_DIRECTION.DOWN, JMF_DIRECTION.UP, JMF_DIRECTION.RIGHT, JMF_DIRECTION.LEFT };
            Board[] aBoards     = new Board[] { bd.Top, bd.Bottom, bd.Left, bd.Right };
            for(int g = 0; g < dirs.Length; ++g)
            {
                if(!findAll && bds.Count>0)  // 하나만 필요하면 break.
                    break;
                if(false == isPassableThroughFence(bd, aBoards[g]))
                    continue;
                if (CanBeMatched(aBoards[g], bd.ColorIndex, dirs[g], ref listMatches))
                    bds.Add( aBoards[g]);
            }
        }

        // multi check.
        if(0 == bds.Count)
		{
            Board[] aBoards     = new Board[] { bd.Top, bd.Bottom, bd.Left, bd.Right };
            for(int g = 0; g < aBoards.Length; ++g)
            {
                if(!findAll && bds.Count>0)  // 하나만 필요하면 break.
                    break;
                if(false == isPassableThroughFence(bd, aBoards[g]))
                    continue;
                if (IsCombinable(bd, aBoards[g]))
                    bds.Add( aBoards[g]);
            }

            // 이걸왜?? -> 이전 logic comment 처리.
            //if(bds.Count > 0)
            //{
            //    bds.Add( bds[0] );
            //    bds.Add( bd );
            //}
        }

        // 단, actor는 첫번째로.
        if(bds.Count > 0)
        {
            this.moverSuggested = bd; 
            this.targetSuggested= bds[0];
        }

        return bds;
	}

   /* //List<Board> GetCandidatesOfMatch2 (Board bd, ref Board outActor1, ref Board outActor2)
	List<Board> GetCandidatesOfMatch2 (Board bd, ref Board outActor1, ref Board outActor2, ref bool matchByCombine)
	{
        List<Board> bds = new List<Board>();
        matchByCombine          = false;

        if (bd.IsMatchable) 
		{
            // [AI_MISSION]
            if (bd.Panel.IsSwitchable() == false) return bds;
			
            // bd가 top 위치로 갔을 때, matchable 한 board list를 buil-up 한다.
            CanBeMatched(bd.Top, bd.ColorIndex, JMF_DIRECTION.DOWN, ref bds);
			if(bds.Count > 1)
			{
				outActor1 = bd.Top;
			}
			else
			{
                CanBeMatched(bd.Bottom, bd.ColorIndex, JMF_DIRECTION.UP, ref bds);
				if(bds.Count > 1)
				{
					outActor1 = bd.Bottom;
				}
				else
				{
                    CanBeMatched(bd.Left, bd.ColorIndex, JMF_DIRECTION.RIGHT, ref bds);
					if(bds.Count > 1)
					{
						outActor1 = bd.Left;
					}
					else
					{
                        CanBeMatched(bd.Right, bd.ColorIndex, JMF_DIRECTION.LEFT, ref bds);
						if(bds.Count > 1)
							outActor1 = bd.Right;
					}
				}
			}			
            
			if (bds.Count > 1) 
			{
                bds.Add(bd);
                outActor2           = bd;
			}
        }

        if (bds.Count == 0 )//&& (bd.PD is HorizontalPiece || bd.PD is VerticalPiece || bd.PD is BombPiece || bd.PD is SpecialFive || bd.PD is TMatch7Piece ))
		{
			if(IsCombinable(bd, bd.Top))
			{
				bds.Add(bd.Top);
				outActor1 = bd.Top;
			}

			if(IsCombinable(bd, bd.Bottom))
			{
				bds.Add(bd.Bottom);
				outActor1 = bd.Bottom;
			}

			if(IsCombinable(bd, bd.Left))
			{
				bds.Add(bd.Left);
				outActor1 = bd.Left;
			}

			if(IsCombinable(bd, bd.Right))
			{
				bds.Add(bd.Right);
				outActor1 = bd.Right;
			}

			if(bds.Count >= 1)
			{
                matchByCombine      = true;
				bds.Add(bd);
				outActor2 = bd;
			}
		}
        return bds;
	}*/

    bool IsCombinable (Board bdA, Board bdB) {
        if ((bdA == null) || (bdB == null))return false;
        if ((bdA.IsFilled == false) || (bdB.IsFilled == false)) return false;
        if ((bdA.IsStable == false) || (bdB.IsStable == false)) return false;
		if ((bdA.Panel.IsSwitchable() == false) || (bdB.Panel.IsSwitchable() == false)) return false;

		return bdA.PD.IsCombinable(bdB.Piece) || bdB.PD.IsCombinable( bdA.Piece);
    }

    bool CanBeMatched (Board bd, int colorIndex, JMF_DIRECTION except, ref List<Board> listOut)
    {
        if (null==bd || false==bd.IsFilled || false==bd.Panel.IsSwitchable())
            return false;

        if ((except == JMF_DIRECTION.LEFT) || (except == JMF_DIRECTION.RIGHT))
        {
            if (bd.PD.IsSwitchableHorizontal(bd.Piece) == false)
                return false;
        }
        if ((except == JMF_DIRECTION.DOWN) || (except == JMF_DIRECTION.UP))
        {
            if (bd.PD.IsSwitchableVertical(bd.Piece) == false)
                return false;
        }
        if(bd.PD is StonePiece) return false;

        
        List<Board> row         = GetRowMatchableBoards(bd, colorIndex, except);
        List<Board> col         = GetColMatchableBoards(bd, colorIndex, except);
        if(row.Count>=2 || col.Count >= 2)   // almost match.
        {
            if(row.Count>=2)    JMFUtils.AddItemWithoutConflict(ref listOut, row );
            if(col.Count>=2)    JMFUtils.AddItemWithoutConflict(ref listOut, col );
            return true;
        }
        
        // check simple 2x2 match.
        if(1<=row.Count && 1<=col.Count)
        {
            List<Board> exX     = new List<Board>();
            List<Board> exY     = new List<Board>();
            // 가로 매칭 대상에 대해 세로로 검색하고, 
            for(int z = 0; z < row.Count; ++z)
                JMFUtils.AddItemWithoutConflict(ref exX, JMFUtils.GM.GetColMatchableBoards(row[z], colorIndex, JMF_DIRECTION.NONE));
            // 세로 매칭 대상에 대해 가로로 검색하고, 
            for(int z = 0; z < col.Count; ++z)
                JMFUtils.AddItemWithoutConflict(ref exY, JMFUtils.GM.GetRowMatchableBoards(col[z], colorIndex, JMF_DIRECTION.NONE));
            // 두 결과간에 동일한 원소가 있으면 매칭 조건 만족.
            for(int u = 0; u < exX.Count; ++u)
            {
                if(true == exY.Contains( exX[u] ))
                {
                    JMFUtils.AddItemWithoutConflict(ref listOut, row );
                    JMFUtils.AddItemWithoutConflict(ref listOut, col );
                    if(false == listOut.Contains(exX[u]))
                        listOut.Add( exX[u] );
                    return true;
                }
            }
        }
        return false;
    }

    bool Switch (Board bdA, Board bdB)
	{
        bool canBeMatched = false;
				
        List<Board> listOutBoards   = new List<Board>();
        if (bdB.Piece.IsMatchable() && CanBeMatched(bdA, bdB.ColorIndex, bdA.GetNeighborDirection(bdB), ref listOutBoards))
            canBeMatched = true;
        
        if (bdA.Piece.IsMatchable() && CanBeMatched(bdB, bdA.ColorIndex, bdB.GetNeighborDirection(bdA), ref listOutBoards))
            canBeMatched = true;

        //if (bdB.Piece.IsMatchable() && CanBeMatched(bdA, bdB.ColorIndex, bdA.GetNeighborDirection(bdB)))
        //    canBeMatched = true;        
        //if (bdA.Piece.IsMatchable() && CanBeMatched(bdB, bdA.ColorIndex, bdB.GetNeighborDirection(bdA)))
        //    canBeMatched = true; 

        _playSwitchAnimation(bdA, bdB, canBeMatched);

        return canBeMatched;
    }

    void _playSwitchAnimation(Board bdA, Board bdB, bool canBeMatched, bool blockedByFence=false)
    {
		bdA.State           = Board.STATE.SWAP;
		bdB.State           = Board.STATE.SWAP;
		
        Sequence seq        = DOTween.Sequence();
		
		string aniNamebdA   = "";
		string aniNamebdB   = "";
		Vector3 posbdB = bdB.Piece.Position;
		posbdB.z += bdB.Position.z;
		
		if(bdA.PT.Y != bdB.PT.Y)
		{
			if(canBeMatched)
			{
				if(bdA.PT.Y > bdB.PT.Y)
				{
					aniNamebdA = "block_mover_V_B";
					aniNamebdB = "block_target_V_T";
				}
				else
				{
					aniNamebdA = "block_mover_V_T";
					aniNamebdB = "block_target_V_B";
				}
			}
			else
			{
				if(bdA.PT.Y > bdB.PT.Y)
				{
					aniNamebdA = "blocking_mover_V_T";
					aniNamebdB = "blocking_target_V_B";
				}
				else
				{
					aniNamebdA = "blocking_mover_V_B";
					aniNamebdB = "blocking_target_V_T";
				}
			}
		}
		else
		{
			if(canBeMatched)
			{
				if(bdA.PT.X > bdB.PT.X)
				{
					aniNamebdB = "block_target_H_L";
					aniNamebdA = "block_mover_H_R";
				}
				else
				{
					aniNamebdB = "block_target_H_R";
					aniNamebdA = "block_mover_H_L";
				}
			}
			else
			{
				if(bdA.PT.X > bdB.PT.X)
				{
					aniNamebdB = "blocking_target_H_L";
					aniNamebdA = "blocking_mover_H_R";
				}
				else
				{
					aniNamebdB = "blocking_target_H_R";
					aniNamebdA = "blocking_mover_H_L";
				}
			}
		}
		
		if(bdA.PD is RoundChocoPiece || bdA.PD is GreenBubblePiece || bdA.PD is ZellatoPiece || bdA.PD is Potion1Piece || bdA.PD is Potion2Piece || bdA.PD is Potion3Piece)
			aniNamebdA = "hard_" + aniNamebdA;
		
		if(bdB.PD is RoundChocoPiece || bdB.PD is GreenBubblePiece || bdB.PD is ZellatoPiece || bdB.PD is Potion1Piece || bdB.PD is Potion2Piece || bdB.PD is Potion3Piece)
			aniNamebdB = "hard_" + aniNamebdB;
		
		bdA.Piece.Play(aniNamebdA, false);

        if(false == blockedByFence)
            bdB.Piece.Play(aniNamebdB, false);	
		
        if (canBeMatched)
		{
            NNSoundHelper.Play("IFX_block_swap");

#if !NN_DEPLOY
            history.Push(new GameStateInfo(this));
#endif
			seq.Append(bdA.Piece.GO.transform.DOMove(posbdB, switchSpeed));
			seq.Insert(0F, bdB.Piece.GO.transform.DOMove(bdA.Piece.Position,switchSpeed));
        }
		else 
		{
            NNSoundHelper.Play("IFX_block_swap_fail");

            if(false == blockedByFence)
            {
			    seq.Append(bdA.Piece.GO.transform.DOMove(posbdB, switchSpeed));
			    seq.AppendInterval(0.2f);
			    seq.Append(bdA.Piece.GO.transform.DOMove(bdA.Piece.Position, switchSpeed));
                //seq.Insert(switchSpeed, bdA.Piece.GO.transform.DOMove(bdA.Piece.Position, switchSpeed));
            }
            else
            {
                Vector3 vTarget = bdA.Piece.Position + Vector3.Normalize(posbdB-bdA.Piece.Position)*7.0f;
                seq.Append(bdA.Piece.GO.transform.DOMove(vTarget, switchSpeed));
			    seq.AppendInterval(0.2f);
			    seq.Append(bdA.Piece.GO.transform.DOMove(bdA.Piece.Position, switchSpeed));
            }
        }

        seq.OnComplete(() => {
            bdA.State = Board.STATE.STABLE;
            bdB.State = Board.STATE.STABLE;

            if (canBeMatched) 
			{
                GamePiece holder = bdA.Piece;
                bdA.Piece = bdB.Piece;
                bdB.Piece = holder;

                bdA.IsNeedMatchCheck = true;
                bdB.IsNeedMatchCheck = true;

                OnPlayerMove();
            }
			bdA.Piece.ResetSortingOrder();
			bdB.Piece.ResetSortingOrder();
        });
    }

     IEnumerator _stopAni(TrackEntry tr, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);
        tr.timeScale            = .0f;
    }

    void Merge (Board from, Board to, System.Action onComplete)
	{
		from.State = Board.STATE.SWAP;
		to.State = Board.STATE.SWAP;

#if !NN_DEPLOY
        history.Push(new GameStateInfo(this));
#endif
        Sequence seq = DOTween.Sequence();
		string aniNamebdFrom = "";
		string aniNamebdTo = "";
				
        if( (from.PD is SpecialFive || from.PD is TMatch7Piece) && 
            (to.PD   is SpecialFive || to.PD   is TMatch7Piece))
        {
#region => Rainbow + Rainbow => 특수 결합.

            // = cs 하단에 봉인..... _oldTypeRainbowMerge(ref from, ref to, ref seq);

            if (from.PT.Y != to.PT.Y)
			{	
                if(from.PT.Y > to.PT.Y)
                {
				    aniNamebdFrom = "blocking_mover_V_T";
				    aniNamebdTo = "blocking_mover_V_B";
                }
                else
                {
                    aniNamebdFrom = "blocking_mover_V_B";
				    aniNamebdTo = "blocking_mover_V_T";
                }
            }
            else
            {
                if(from.PT.X > to.PT.X)
                {
                    aniNamebdFrom = "blocking_target_H_R";
		            aniNamebdTo = "blocking_target_H_L";
                }
                else
                {
                    aniNamebdFrom = "blocking_target_H_L";
		            aniNamebdTo = "blocking_target_H_R";
                }
            }

            NNSoundHelper.Play("IFX_double_rainbow_bust");

            from.Piece.Play(aniNamebdFrom, false);
			to.Piece.Play(aniNamebdTo, false);

            float fHoldDelay    = 1.0f;
            seq.Append(     from.Piece.GO.transform.DOMoveX(to.Position.x, switchSpeed));	
            seq.Insert(.0f, from.Piece.GO.transform.DOMoveY(to.Position.y, switchSpeed));
            seq.AppendInterval( fHoldDelay );

            // 하나는 안보이는 데로 치운다.
            DOVirtual.DelayedCall(fHoldDelay*0.5f, () => to.Piece.GO.transform.position += Vector3.right*1000.0f);
            
            BlockCrash effect   = NNPool.GetItem<BlockCrash>("SpecialFiveBubbelHit");
            Vector3 pos         = to.Position;
			pos.z -= 50.0f; 		
			effect.Play("play",pos,Vector3.one, 0);//, false, switchSpeed+0.2f, 1.0f);
            //   0.75f);	
            
            StartCoroutine( _coPlaySound("IFX_bomb_block_create", switchSpeed) );
            StartCoroutine( _coPlaySound("IFX_bombblock_bust", switchSpeed+fHoldDelay) );

#endregion
        }
        //if(from.PD is BombPiece && to.PD is BombPiece)
        else if( (from.PD is SpecialFive || to.PD is SpecialFive) || 
                 (from.PD is TMatch7Piece || to.PD is TMatch7Piece))
		{
#region => 서로 자리 바꿈. ->  ani 필요 없음.
            float duration = 0.0f;
			Vector3 posbdB = to.Position;

#region // comment.
            /*if(from.PT.Y != to.PT.Y)
			{
				if(from.PT.Y > to.PT.Y)
				{
					aniNamebdFrom = "block_mover_V_B";
					aniNamebdTo = "block_target_V_T";
				}
				else
				{
					aniNamebdFrom = "block_mover_V_T";
					aniNamebdTo = "block_target_V_B";
				}
			}
			else
			{
				if(from.PT.X > to.PT.X)
				{
					aniNamebdTo = "block_target_H_L";
					aniNamebdFrom = "block_mover_H_R";
				}
				else
				{
					aniNamebdTo = "block_target_H_R";
					aniNamebdFrom = "block_mover_H_L";
				}
			}
			*/
#endregion

            duration = .0f;// from.Piece.Play(aniNamebdFrom, false);
			//duration *= 0.3f;
			//to.Piece.Play(aniNamebdTo, false);	

			seq.Append(from.Piece.GO.transform.DOMoveX(posbdB.x, switchSpeed));
            seq.Insert(0F, from.Piece.GO.transform.DOMoveY(posbdB.y, switchSpeed));
			seq.Insert(0F, to.Piece.GO.transform.DOMoveX(from.Position.x, switchSpeed));
            seq.Insert(0F, to.Piece.GO.transform.DOMoveY(from.Position.y, switchSpeed));
			seq.SetDelay(duration);
#endregion
        }
        else
		{
#region => from은 to로 이동.., to는 제자리에서 사라짐. -> ani가 필요 없음. 

            Vector3 posbdB = to.Position;

#region // comment.
            /*if(from.PT.Y != to.PT.Y)
			{
				if(from.PT.Y > to.PT.Y)
				{
					aniNamebdFrom = "blocking_mover_V_B";
					aniNamebdTo = "blocking_target_V_B";
				}
				else
				{
					aniNamebdFrom = "blocking_mover_V_T";
					aniNamebdTo = "blocking_target_V_T";
				}
			}
			else
			{
				if(from.PT.X > to.PT.X)
				{
					aniNamebdFrom = "blocking_mover_V_B";
					aniNamebdTo = "blocking_target_V_B";
				}
				else
				{
					aniNamebdFrom = "blocking_mover_V_T";
					aniNamebdTo = "blocking_target_V_T";	
				}
			}*/
#endregion

            //from.Piece.Play(aniNamebdFrom, false);
            //to.Piece.Play(aniNamebdTo, false);	
            seq.Append(     from.Piece.GO.transform.DOMoveX(to.Position.x, switchSpeed));	
            seq.Insert(.0f, from.Piece.GO.transform.DOMoveY(to.Position.y, switchSpeed));
#endregion
        }

        seq.OnComplete(() => {
			from.State = Board.STATE.STABLE;
			to.State = Board.STATE.STABLE;

			from.IsNeedMatchCheck = true;
			to.IsNeedMatchCheck = true;

			OnPlayerMove();

			if (onComplete != null) onComplete();
		});
    }

    IEnumerator _coPlaySound(string strSndName, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);

        NNSoundHelper.Play(strSndName, false);
    }

	void OnPlayerMove () 
	{
//        StopSuggestPiece();

		Moves++;

        PlayMoved = true;

		JMFRelay.FireOnPlayerMove();
	}

    IEnumerator ShowExplosion (System.Action onComplete) {
        bool explodedFirst = false;

        for (int i = 0; i < boards.Count; i++) {
            if (this[i].IsFilled == false) continue;
            if ((this[i].PD is TimeBombPiece) == false) continue;

            TimeBomb timeBomb = this[i].Piece.GO.GetComponent<TimeBomb>();

            if (timeBomb.FallBackTime > 0) continue;

            if (explodedFirst) {
                timeBomb.SmallExplosion();
            } else {
                explodedFirst = true;
                timeBomb.LargeExplosion();
            }
        }

        if (explodedFirst) yield return new WaitForSeconds(1F);

        if (onComplete != null) onComplete();
    }

	public float ShootShockWave (Point pt, float radius, float force, float duration, bool bInForce, float rate = 0.2f, List<Board> listExceptions=null) 
	{
        // note : CPR update의 일환으로 일단 함수 기능 kill. by wayne. 071018
        return 0.2f;

        /*
		float outTime = duration * rate;
		float outTime1 = 0.0f;
        float inTime = duration * 0.8F;

        List<Board> bds = this[pt].GetBoardsFromRadius(radius);

        foreach (Board bd in bds) 
		{
            if (bd.IsFilled == false) continue;
            if (DOTween.IsTweening(bd.Piece.GO)) continue;
            if(null!=listExceptions && listExceptions.Contains(bd))
                continue;
            if(null!=bd.PND && false==bd.PND.IsApplyShockWave())
                continue;
            if(null!=bd.Piece && (bd.Piece.LifeCover>0 || bd.PD is CookieJellyPiece || bd.PD is CookiePiece))
                continue;

            Vector3 length = bd.Position - this[pt].Position;
            float ratio = Mathf.Pow((radius - length.magnitude) / radius, 2);
			Vector3 impuls = length * force * ratio;
			
			float delay = outTime - (outTime * ratio);
			Vector3 newPos = bd.Piece.GO.transform.position + impuls;
			
			Sequence seq = DOTween.Sequence();
			seq.SetId(bd.Piece.GO.GetInstanceID()+1);
			seq.AppendInterval(delay);
			if(bInForce)
			{
				outTime1 = outTime * 4.0f;
				Vector3 impuls2 = length * force * (ratio * -1.0f);
				Vector3 newPos2 = bd.Piece.GO.transform.position + impuls2;	
				seq.Append(bd.Piece.GO.transform.DOMoveX(newPos2.x, outTime1 ).SetEase(Ease.OutCirc));
                seq.Join(bd.Piece.GO.transform.DOMoveY(newPos2.y, outTime1 ).SetEase(Ease.OutCirc));
				outTime1 += delay;
			}
			else
				outTime1 = delay;
			
			seq.Append(bd.Piece.GO.transform.DOMoveX(newPos.x, outTime).SetEase(Ease.OutCubic));
            seq.Join(bd.Piece.GO.transform.DOMoveY(newPos.y, outTime).SetEase(Ease.OutCubic));
			seq.Append(bd.Piece.GO.transform.DOMoveX(bd.Position.x, inTime).SetEase(Ease.InCubic));
            seq.Join(bd.Piece.GO.transform.DOMoveY(bd.Position.y, inTime).SetEase(Ease.InCubic));
        }
		
		return outTime1;
        */
    }

    void OnClickPanel (Point pt) {
        if (State != JMF_GAMESTATE.PLAY) return;
        if (ReadiedFrog == null) return;
        if (IsStable == false) return;
        if (this[pt].Panel.IsDestroyablePiece() == false) return;
        if (this[pt].IsFilled && (this[pt].Piece.IsDestroyable() == false)) return;
        
        CanMove = false;

        JumpFrog(this[pt], () => { 
            CanMove = true;
        });
    }

    void JumpFrog (Board destination, System.Action onComplete) {
        Debugger.Assert(destination != null, "GameManager.JumpFrog : Board is null.");
        Debugger.Assert(ReadiedFrog != null, "GameManager.JumpFrog : Frog is null.");
        Debugger.Assert(ReadiedFrog.Piece != null, "GameManager.JumpFrog : Frog is null.");
        Debugger.Assert(ReadiedFrog.Piece.GO != null, "GameManager.JumpFrog : Frog is null.");

        ReadiedFrog.Piece.GO.GetComponent<Frog>().EatingCount = 0;

        Transform frogTF = ReadiedFrog.Piece.GO.transform;
        Vector3 orgScale = frogTF.localScale;

        Vector3 targetPos = destination.Position;
        targetPos.z -= 10F;

        Sequence seq = DOTween.Sequence();
        seq.Append(frogTF.DOMove(targetPos, 0.3F).SetEase(Ease.OutQuad));
        seq.Insert(0F, frogTF.DOScale(orgScale * 2F, 0.3F).SetEase(Ease.OutQuad));
        seq.AppendInterval(0.1F);
        seq.Insert(0.4F, frogTF.DOScale(orgScale, 0.2F).SetEase(Ease.InQuad));
        seq.OnComplete(() => { 
            frogTF.position = destination.Position;

            destination.Hit();

            List<Board> bds = destination.GetBoardsFromArea(0, 1);

            for (int i = 0; i < bds.Count; i++) {
                bds[i].Hit();
            }

            int frogColor = ReadiedFrog.Piece.ColorIndex;

            destination.Piece = ReadiedFrog.Piece;
            ReadiedFrog.Piece = null;
            ReadiedFrog = null;

            destination.ResetPiece(GetPieceType<FrogPiece>(), GetRandomColorIndex(frogColor));

            if (onComplete != null) onComplete();
        });

        //NNSoundHelper.Play("IFX_goal_earning");
    }

    public override string ToString () {
        string result = "";

		for (int y = HEIGHT - 1; y >= 0; y--) {
            for (int x = 0; x < WIDTH; x++) {
                if (this[x,y].Panel.PND is EmptyPanel) {
                    result += "9";
                } else {
                    if (this[x,y].IsFilled) result += this[x,y].Piece.ColorIndex.ToString();
                } 
            }

            result += "\n";
        }

        return result;
    }

#if UNITY_EDITOR
    IEnumerator CoCaptureAllLevel () {
        string rootPath = Application.dataPath + "/../ScreenShot/Levels";

        if (!System.IO.Directory.Exists(rootPath)) {
            System.IO.Directory.CreateDirectory(rootPath);
        }

		string themePath = rootPath + "/" + "LevelDatas";

		if (!System.IO.Directory.Exists(themePath))
		{
			System.IO.Directory.CreateDirectory(themePath);
		}

        yield break;
	/*	foreach (Level l in Root.Data.levels) 
		{
            Scene.CloseOverlay("PlayOverlay");
			Reset(l, true);
            playSceneHandler.ScreenShot();
            //yield return null;
            yield return new WaitForSeconds(3.0f);

			string fileName = themePath + "/level_" + (l.Index + 1).ToString() + ".png";
			Application.CaptureScreenshot(fileName);
			yield return null;
		}*/
    }
#endif

#if !NN_DEPLOY
    public void Rollback () {
        if (history.Count == 0) return;

        GameStateInfo gsi = history.Pop();

        for (int y = 0; y < HEIGHT; y++) {
            for (int x = 0; x < WIDTH; x++) {
                BoardStateInfo bsi = gsi[x,y];

                this[x,y].ShadedDurability = bsi.ShadedDurability;
                
                this[x,y].ResetPanel(bsi.PND, bsi.PanelDurability, bsi.PanelInfo);

                if (bsi.PD == null) {
                    if (this[x,y].IsFilled) this[x,y].RemovePiece();
                } else {
                    this[x,y].ResetPiece(bsi.PD, bsi.PieceColor);
                    this[x,y].Piece.FallBackTime = bsi.FallBackTime;
                    if (bsi.PD is ChameleonPiece) {
                        this[x,y].Piece.GO.GetComponent<Chameleon>().NextIndex = bsi.ChameleonColor;
                    }
                }
            }
        }

        Moves = gsi.Move;
        Score = gsi.Score;
        PlayTime = gsi.PlayTime;
        YetiHealth = gsi.YetiHealth;
        BossHealth = gsi.BossHealth;
        PenguinMatchCount = gsi.PenguinMatchCount;
        FairyMatchCount = gsi.FairyMatchCount;
        Potion1CollectCount = gsi.Potion1CollectCount;
        Potion2CollectCount = gsi.Potion2CollectCount;
        Potion3CollectCount = gsi.Potion3CollectCount;
        System.Array.Copy(gsi.JewelMatchCounts.ToArray(), 0, JewelMatchCounts, 0, gsi.JewelMatchCounts.Count);
        System.Array.Copy(gsi.SpecialJewelCollectCounts.ToArray(), 0, SpecialMatchCounts, 0, gsi.SpecialJewelCollectCounts.Count);

        NNTool.Seed = gsi.Seed;

        //PlayOverlayHandler.Invalidate();
    }
#endif

#region [CHOCO_BAR], [NET_SHADE]
    public struct CHOCO_BAR_INFO
    {
        public LEItem.CHOCO_BAR eType;
        public List<int>        listIdxBoards;
        public CHOCO_BAR_INFO(LEItem.CHOCO_BAR type)
        {
            eType               = type;
            listIdxBoards       = new List<int>();
        }
    };
    public List<CHOCO_BAR_INFO>  _listChocoBarInfos = new List<CHOCO_BAR_INFO>();

    public void _buildChocoBarInfo(Data.Level level)
    {
        _listChocoBarInfos.Clear();

        List<int> listTargets   = new List<int>();
        int index               = 0;
        for(int y = HEIGHT-1; y >= 0; --y)
        {
            for(int x = 0; x < WIDTH; ++x)
            {
                if(0==level.indexBar[index])
                {
                    listTargets.Clear();

#region -> search targets.
                    switch ( (LEItem.CHOCO_BAR)level.eBarType[index] )
                    {
                    case LEItem.CHOCO_BAR._1X1:
                        listTargets.Add( index );
                        break;
                    case LEItem.CHOCO_BAR._1X2:
                        listTargets.Add( index );
                        listTargets.Add( index - WIDTH );
                        break;
                    case LEItem.CHOCO_BAR._1X3:
                        listTargets.Add( index );
                        listTargets.Add( index - WIDTH );
                        listTargets.Add( index - 2*WIDTH );
                        break;
                    case LEItem.CHOCO_BAR._2X2:
                        listTargets.Add( index );           listTargets.Add( index + 1 );
                        listTargets.Add( index - WIDTH );   listTargets.Add( index - WIDTH + 1 );
                        break;
                    case LEItem.CHOCO_BAR._3X3:
                        listTargets.Add( index );           listTargets.Add( index + 1 );       listTargets.Add( index + 2 );
                        listTargets.Add( index - WIDTH );   listTargets.Add( index - WIDTH + 1 ); listTargets.Add( index - WIDTH + 2 );
                        listTargets.Add( index - 2*WIDTH ); listTargets.Add( index - 2*WIDTH + 1 ); listTargets.Add( index - 2*WIDTH + 2 );
                        break;
                    case LEItem.CHOCO_BAR._2X3:
                        listTargets.Add( index );           listTargets.Add( index + 1 );
                        listTargets.Add( index-WIDTH );     listTargets.Add( index-WIDTH+1 );
                        listTargets.Add( index-2*WIDTH );   listTargets.Add( index-2*WIDTH+1 );
                        break;
                    case LEItem.CHOCO_BAR._2X4:
                        listTargets.Add( index );           listTargets.Add( index + 1 );
                        listTargets.Add( index-WIDTH );     listTargets.Add( index-WIDTH+1 );
                        listTargets.Add( index-2*WIDTH );   listTargets.Add( index-2*WIDTH+1 );
                        listTargets.Add( index-3*WIDTH );   listTargets.Add( index-3*WIDTH+1 );
                        break;
                    case LEItem.CHOCO_BAR._2X1:
                        listTargets.Add( index );   listTargets.Add( index + 1 );
                        break;
                    case LEItem.CHOCO_BAR._3X1:
                        listTargets.Add( index );   listTargets.Add( index + 1 );   listTargets.Add( index + 2 );
                        break;
                    case LEItem.CHOCO_BAR._3X2:
                        listTargets.Add( index );           listTargets.Add( index+1 );         listTargets.Add( index+2 );
                        listTargets.Add( index-WIDTH );     listTargets.Add( index-WIDTH+1 );   listTargets.Add( index-WIDTH+2 );            
                        break;
                    case LEItem.CHOCO_BAR._4X2:
                        listTargets.Add( index );           listTargets.Add( index+1 );
                        listTargets.Add( index+2 );         listTargets.Add( index+3 );
                        listTargets.Add( index-WIDTH );     listTargets.Add( index-WIDTH+1 );
                        listTargets.Add( index-WIDTH+2 );   listTargets.Add( index-WIDTH+3 );
                        break;
                    }
#endregion

#region -> build info structure.

                    if (0 == listTargets.Count)
                        continue;
                    
                    bool all_is_ok          = true;
                    CHOCO_BAR_INFO info     = new CHOCO_BAR_INFO((LEItem.CHOCO_BAR)level.eBarType[index]);
                    for(int zz = 0; zz < listTargets.Count; ++zz)
                    {
                        if(level.indexBar[ listTargets[zz] ] < 0)
                        {
                            all_is_ok       = false;
                            break;
                        }
                        info.listIdxBoards.Add( listTargets[zz] );
                    }
                    if(all_is_ok)
                        this._listChocoBarInfos.Add( info );
#endregion
                }
                ++index;
            }
        }
    }
#endregion


#region [AI Mode]

    public void fire_AiTurn(bool myTurn, bool rainbowBonus)
    {
        if(false == isAIFightMode)
            return;

        _nAiMoveWaited          = myTurn ? 0 : 1;

        JMFRelay.FireOnChangeAITurn(myTurn);
        playOverlayHandler.fire_AiTurn(myTurn, rainbowBonus);
       
    }

    // ai가 해당 위치로 swap하려고 준비, 커튼을 견다, 마스킹을 켠다.. 등.
    public void AiCurtainReady(ref List<Board> listBoards, float fDuration)
    {
        playOverlayHandler.AiCurtainReady(ref listBoards, fDuration);
    }
    // ai 가 실제 두 피스를 이동시킴. - 커튼, 마스킹을 끈다.
    public void AiDrag(Point ptFrom, Point ptTo)
    {
        StartCoroutine( _coAiDrag(ptFrom, ptTo) );
    }

    IEnumerator _coAiDrag(Point ptFrom, Point ptTo)
    {
        float fGap              = isCurPlayerAI ? 0.5f : .0f;
     //   if(isCurPlayerAI)       charAni_play(SPINE_CHAR.AI_GIRL, ANI_STATE.AI_BOT_MOVE);
     //   else                    charAni_play(SPINE_CHAR.MAIN_GIRL, ANI_STATE.AI_MY_MOVE);
        yield return new WaitForSeconds( fGap );

        float duration          = _fireAiModeAttack(isCurPlayerAI, this[ptTo].Position);
        yield return new WaitForSeconds(duration);

        if(isCurPlayerAI)
        {
            playOverlayHandler.AiCurtainClose();
            DragFromHere(ptFrom, ptTo);
        }
    }

    // fire magic wand bullet to target board.
    public IEnumerator fireItemMagicAnimation(Board bdTarget)
    {
     //   charAni_play(SPINE_CHAR.MAIN_GIRL, ANI_STATE.AI_MY_MOVE);
        yield return new WaitForSeconds( 0.2f );

        float duration          = _fireAiModeAttack(false, bdTarget.Position);
        yield return new WaitForSeconds(duration);        
    }


    float _fireAiModeAttack(bool isAiBot, Vector3 ptDest)
    {
        NNSoundHelper.Play("IFX_bomb_block_create");
        return .0f;// ani.Duration;
    }

#endregion

    void _abandonPoolGabages()
    {
        if(true == NNPool.destroyAbandoned())
        {
            // Debug.Log("@@@ NNPOOL item was deleted.");
        }
    }
    public bool canOpenRatePopup()
    {
        /*
        int idxMap              = Root.Data.currentLevel.Index>0 ? Root.Data.currentLevel.Index-1 : 0;
        if(idxMap >= Root.Data.levels.Length)
            return false;
        Data.Level lastCleared  = Root.Data.levels[ idxMap ];

        int idxPopup            = mapIdForRatePopup - 1;
        if( false == Root.Data.gameData.record.BaseData.Rated &&            // no rated ?
            ((lastCleared.Index == idxPopup) ||                             // last cleared == rate condition ?
             (lastCleared.Index>idxPopup && 0==((lastCleared.Index+1)%mapIdForRatePopup))) )// % mapIdForRatePopup ?
        {
            return true;
        }*/

#if UNITY_EDITOR
        //if(true == RatePopupHandler._TESTMODE)
        //    return true;
#endif
        //
        return false;
    }

    void _oldTypeRainbowMerge(ref Board from, ref Board to, ref Sequence seq)
    {
        /*string aniNamebdFrom = "";
		string aniNamebdTo = "";

        if (from.PT.Y != to.PT.Y)
		{	
            if(from.PT.Y > to.PT.Y)
            {
				aniNamebdFrom = "blocking_mover_V_T";
				aniNamebdTo = "blocking_mover_V_B";
            }
            else
            {
                aniNamebdFrom = "blocking_mover_V_B";
				aniNamebdTo = "blocking_mover_V_T";
            }
        }
        else
        {
            if(from.PT.X > to.PT.X)
            {
                aniNamebdFrom = "blocking_target_H_R";
		        aniNamebdTo = "blocking_target_H_L";
            }
            else
            {
                aniNamebdFrom = "blocking_target_H_L";
		        aniNamebdTo = "blocking_target_H_R";
            }
        }

        from.Piece.Play(aniNamebdFrom, false);
		to.Piece.Play(aniNamebdTo, false);	

        Spine.Animation aniFrom = from.Piece.GO.GetComponent<Block>().getSA().skeleton.Data.FindAnimation(aniNamebdFrom);
        Spine.Animation aniTo   = to.Piece.GO.GetComponent<Block>().getSA().skeleton.Data.FindAnimation(aniNamebdTo);
            
        TrackEntry trNowFrom    = from.Piece.GO.GetComponent<Block>().getSA().AnimationState.GetCurrent(0);
        TrackEntry trNowTo      = to.Piece.GO.GetComponent<Block>().getSA().AnimationState.GetCurrent(0);
        StartCoroutine( _stopAni(trNowFrom, aniFrom.duration-0.5f) );
        StartCoroutine( _stopAni(trNowTo, aniTo.duration-0.5f) );

        //seq.Append(from.Piece.GO.transform.DOMove(to.Position, switchSpeed));	

        Vector3 vFrom       = from.Position + (to.Position - from.Position) * 0.35f;
        Vector3 vTo         = from.Position + (to.Position - from.Position) * 0.65f;
        seq.Append(from.Piece.GO.transform.DOMoveX(vFrom.x, switchSpeed));
        seq.Insert(.0f, from.Piece.GO.transform.DOMoveY(vFrom.y, switchSpeed));
		seq.Insert(.0f, to.Piece.GO.transform.DOMoveX(vTo.x, switchSpeed));
        seq.Insert(.0f, to.Piece.GO.transform.DOMoveY(vTo.y, switchSpeed));
        seq.Insert(switchSpeed+0.06f, from.Piece.GO.transform.DOShakePosition(1.0f, 0.5f) );
        seq.Insert(switchSpeed+0.1f, to.Piece.GO.transform.DOShakePosition(1.0f, 0.6f) );

        BlockCrash effect   = NNPool.GetItem<BlockCrash>("SpecialFiveBubbelHit");
        Vector3 pos         = from.Position + (to.Position - from.Position) * 0.5f;
		pos.z -= 2.0f; 		
		effect.Play("play",pos,Vector3.one, 0, false, switchSpeed+0.8f, 1.0f);
        //   0.75f);	

        StartCoroutine( _coPlaySound("IFX_bomb_block_create", switchSpeed) );
        StartCoroutine( _coPlaySound("IFX_bombblock_bust", switchSpeed+1.0f) );
        */
        //seq.AppendInterval(3.0f); 
        //seq.SetDelay(duration);	
    }

    IEnumerator _coUpdateSugarPiece()
    {
        List<Board> listSugars  = new List<Board>();
        List<Board> listJellyCookie = new List<Board>();
        List<Board> listWaffle  = new List<Board>();
        while(true)
        {
            if(pump != idlePump)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            listSugars.Clear();
            listJellyCookie.Clear();
            listWaffle.Clear();
            for (int i = 0; i < boards.Count; i++)
            {
                if(null != boards[i].PD)
                {
                    if(boards[i].PD is SugarCherryPiece || boards[i].PD is SugaredPiece)
                        listSugars.Add( boards[i] );
                    else if(boards[i].PD is CookieJellyPiece)
                        listJellyCookie.Add( boards[i] );
                }
                if(null != boards[i].PND)
                {
                    if(boards[i].PND is WaffleCookerPanel && 0==boards[i].Panel.Durability)
                        listWaffle.Add( boards[i] );
                }
            }
            //
            if(0 < listSugars.Count)
            {
                Board target    = listSugars[ Random.Range(0, listSugars.Count ) ];
              //  SpineEffect eff = NNPool.GetItem<SpineEffect>("S_piece_shiny");
              //  eff.play( string.Format("shiny{0}", Random.Range(1, 4)), .0f );
              //  eff.GetComponent<MeshRenderer>().sortingOrder    = 11;
               // eff.transform.position  = new Vector3(target.Position.x, target.Position.y, -100.0f);
            }
            if(0 < listJellyCookie.Count)
            {
                Board target    = listJellyCookie[ Random.Range(0, listJellyCookie.Count ) ];
             //   SpineEffect eff = NNPool.GetItem<SpineEffect>("JellyCookieEffect");
              //  eff.play( string.Format("jelly{0}_level1_idle", target.Piece.CookieJellyType+1), .0f );
              //  eff.GetComponent<MeshRenderer>().sortingOrder    = 11;
              //  eff.transform.position  = new Vector3(target.Position.x, target.Position.y, -100.0f);
             //   Director.Instance.showMeshNextFrame(eff.GetComponent<MeshRenderer>());

                yield return new WaitForSeconds( 1.0f );
            }
            if(0 < listWaffle.Count)
            {
                Board target    = listWaffle[ Random.Range(0, listWaffle.Count ) ];
                Panel pnl       = target.Panel[BoardPanel.TYPE.BACK];

                pnl.Play( "waffle_cooker_level2", false );
                yield return new WaitForSeconds( 1.0f );
            }

            yield return new WaitForSeconds( UnityEngine.Random.Range(0.1f, 0.5f) );
        }
    }
}
