using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;

/// <summary> ##################################
/// 
/// NOTICE :
/// This script is conditions set to win/end the current game.
/// 
/// </summary> ##################################

public class EditWinningConditions : MonoBehaviour {
	public enum GAME_MODE { TIMER, MOVE, ICE, SNOWMAN, JEWEL, INGREDIENT, SPECIAL_JEWEL, PENGUIN, YETI, FAIRY, BOSS, COUNT };

    // [FIX_MISSION]
	public enum MISSION_TYPE { NONE=-1, COLLECT, FILL, DEFEAT, FIND, CLEAR, SCORE, OLD_TYPE, MAX_TYPE };    
    //

    [Range(0,100)]
    public List<int> normalProbability = new List<int>(new int[]{100, 100, 100, 100, 100, 100, 100, 100});
		
    public bool enableSpiralSnow = false;
	public int movePerSpiralSnow = 5;
    [Range(1,3)] public int lifeNewRoundChoco = 1;      // [ROUND_CHOCO]
	[Range(1,81)] public int minSpiralSnowCount = 1;
	[Range(1,81)] public int maxSpiralSnowCount = 1;
	public int spiralSnowSpawnCountPerMove = 1;
    public bool enableTimeBomb = false;
	public int movePerTimeBomb = 5;
	[Range(1,81)] public int minTimeBombCount = 1;
	[Range(1,81)] public int maxTimeBombCount = 1;
	public int timeBombSpawnCountPerMove = 1;
    public int defaultFallBackTime = 9;

	// timer game
	public bool isTimerGame { get; set; } 
	public float TimeGiven = 120;
	
	[Space(20)] // just some seperation
	// max move game
	public bool isMaxMovesGame = false;
	public int allowedMoves = 40;

	//[Space(20)] // just some seperation
	// score game
	//public bool isScoreGame = true;
	[Tooltip("If enabled, player must obtain a minimum score of 'scoreToReach'")]
	public bool scoreRequiredWin = true;
	[Tooltip("If enabled, obtaining the minimum score will trigger end-game.")]
	public bool scoreEndsGame = false;
	[Space(20)] // just some seperation
	public int scoreToReach = 10000;
	public int scoreMilestone2 = 20000;
	public int scoreMilestone3 = 30000;
	
	[Space(20)] // just some seperation
	// clear shaded game
	public bool isClearShadedGame = false;
	[Tooltip("If enabled, player must clear all the shaded panels to win.")]
	public bool shadesRequiredWin = true;
	[Tooltip("If enabled, clearing all the shaded panels will trigger end-game.")]
	public bool shadesEndsGame = true;
	//[Space(20)] // just some seperation
	// clear chocolate game
	public bool isClearChocolateGame = false;
	[Tooltip("If enabled, player must clear all the chocolate panels to win.")]
	public bool chocolateRequiredWin = true;
	[Tooltip("If enabled, clearing all the chocolate panels will trigger end-game.")]
	public bool chocolateEndsGame = true;
	//[Space(20)] // just some seperation
	// get type game
    public bool isHardLevelGame = false;
    public bool isGetTypesGame = false;
	[Tooltip("If enabled, player must get all the specified types to win.")]
	public bool typesRequiredWin = true;
	[Tooltip("If enabled, getting all the specified types will trigger end-game.")]
	public bool typeEndsGame = true;
	public int[] numToGet = new int[9];
	//[Space(20)] // just some seperation
	// treasure game
	public bool isTreasureGame = false;
	[Tooltip("If enabled, player must all the treasures to win.")]
	public bool treasureRequiredWin = true;
	[Tooltip("If enabled, getting all the treasures will trigger end-game.")]
	public bool treasureEndsGame = true;
    public bool enableTreasure = false;
	public int maxOnScreen = 1;
	public int treasureSpawnCountPerMove = 1;

    public bool isSpecialJewelGame = false;
    public int[] specialJewels = new int[(int)SJ_COMBINE_TYPE.MAX_COUNT];

    public bool isFairyGame = false;
    public bool enableFairyPiece = false;
    public int numberOfFairy = 0;

    public bool isPenguinGame = false;
	public int maxPenguinOnScreen = 4;
    public int numberOfPenguin = 0;
	[Range(0,100)] public int chanceToPenguinSpawn = 10;

    public bool enableMysteryPiece = false;
    public int movePerMysteryPiece = 5;
    [Range(1, 81)] public int minMysteryPieceCount = 0;
    [Range(1, 81)] public int maxMysteryPieceCount = 0;
	public int mysteryPieceSpawnCountPerMove = 1;
    public int[] mysteryPieceConvertProbability = new int[18];

    public int movePerFairyPiece = 5;
    [Range(1, 81)] public int minFairyPieceCount = 0;
    [Range(1, 81)] public int maxFairyPieceCount = 0;
	public int fairyPieceSpawnCountPerMove = 1;

    public bool enableChameleon = false;
    public int movePerChameleon = 5;
    [Range(1, 81)] public int minChameleonPieceCount = 0;
    [Range(1, 81)] public int maxChameleonPieceCount = 0;
	public int chameleonSpawnCountPerMove = 1;
    public int[] roundChocoSpawnColumn = new int[9];

    public bool isYetiGame = false;
    public int yetiHealth = 30;
    public int attackPerMove = 5;
    [Range(0, 100)] public int yetiBallDropProbability = 20;
    [Range(1, 81)] public int minYetiBallPieceCount = 0;
    [Range(1, 81)] public int maxYetiBallPieceCount = 0;
    [Range(1, 81)] public int minYetiAttackCount = 1;
    [Range(1, 81)] public int maxYetiAttackCount = 3;
    public int[] attackConvertProbability = new int[9];
    public int[] greenBubbleSpawnColumn = new int[9];

    public int goalScore        = 0;

    public bool isBossGame = false;
    public int bossHealth = 30;
    public int bossType = 0;
    public int bossHealCount = 0;
    public int bossActionPerMove = 0;
    public int[] bossAttackConvertProbability = new int[9];
    [Range(1, 81)] public int minBossAttackCount = 1;
    [Range(1, 81)] public int maxBossAttackCount = 3;

    public int movePerTreasure = 3;

    public float magnification2 = 2.5f; // 1.7F;
    public float magnification3 = 4.0f; // 2.7F;

    // [FIX_MISSION]
    public MISSION_TYPE missionType = MISSION_TYPE.COLLECT;
    // mix objects.
    public bool isMixMission    = false;
    public int countRoundChocho = 0;
    public int countJamBottom   = 0;
    public int countRectChocho  = 0;
    public int countCottonCandy = 0;
    public int countSodaCan     = 0;
    public int countSugarBlock  = 0;
    public int countZellatto    = 0;
    public int countCookieJelly = 0;
    public int countColorBox    = 0;
    public int countWaffleCooker= 0;
    public int countMudShade    = 0;
    // fill(drop) objects.
    public int countPotion1     = 0;
    public int countPotion2     = 0;
    public int countPotion3     = 0;
    // find objects
    public int countChocoBar    = 0;
    // clear objects.
    public int countCursedBottom= 0;
    // ai score.
    public int countAiWinBoard  = 0;
    [Range(0, 100)]
    public int  ai_intelligence = 20;

    // stawberry drop(gen) rate.
    public bool enableStrawberry    = false;
    public int movePerStrawberry    = 0;
    public int minStrawberryCount   = 0;
    public int maxStrawberryCount   = 0;
    public int strawberrySpawnCountPerMove = 0;
    //

    public bool enableGreenBubble   = false;
    public int movePerGreenBubble   = 0;
    public int lifeNewGreenBubble   = 0;
    public int minGreenBubble       = 0;
    public int maxGreenBubble       = 0;
    public int greenBubbleSpawnCountPerMove = 0;

    // sugarcherry drop(gen) rate.
    public bool enableSugarCherry   = false;
    public int movePerSugarCherry   = 0;
    public int minSugarCherryCount  = 0;
    public int maxSugarCherryCount  = 0;
    public int sugarCherrySpawnCountPerMove = 0;
    //

    // zellato drop(gen) rate.
    public bool enableZellato   = false;
    public int movePerZellato   = 0;
    public int minZellatoCount  = 0;
    public int maxZellatoCount  = 0;
    public int zellatoSpawnCountPerMove = 0;
    //

    // add time block drop(gen) rate.
    public bool enableAddTimeBlock   = false;
    public int movePerAddTimeBlock   = 0;
    public int minAddTimeBlockCount  = 0;
    public int maxAddTimeBlockCount  = 0;
    public int addTimeBlockSpawnCountPerMove = 0;
    //
	
//	// Treasure Level info
//	public bool isTreasure = false;
//	public string TreasureItemKey = "";
//	public int TreasureItemCount = 0;

    public class WinningConditionsEditorSettings {
        public int[][] typeProbabilities;
    }

    public WinningConditionsEditorSettings wcEditorSettings;
    string wcEditorSettingPath;

    void Awake ()
    {
        wcEditorSettingPath = Application.dataPath+"/Data/Editor/editorSettings.txt";
        string jsonStr = System.IO.File.ReadAllText(wcEditorSettingPath);
        wcEditorSettings = JsonFx.Json.JsonReader.Deserialize<WinningConditionsEditorSettings>(jsonStr);

        // [FIX_MISSION] : This is always false. !!!
        //isTimerGame             = false;
    }

	public void SetConditionsInspector(Data.Level level) 
	{
//		isTreasure = level.isTreasure;
//		TreasureItemKey = level.TreasureItemKey;
//		TreasureItemCount = level.TreasureItemCount;
		
        enableSpiralSnow = level.enableSpiralSnow;
        movePerSpiralSnow = level.movePerSpiralSnow;
        lifeNewRoundChoco   = level.lifeNewRoundChoco;
        minSpiralSnowCount = level.minSpiralSnowCount;
        maxSpiralSnowCount = level.maxSpiralSnowCount;
        spiralSnowSpawnCountPerMove = level.spiralSnowSpawnCountPerMove;
		enableTimeBomb = level.enableTimeBomb;
		movePerTimeBomb = level.movePerTimeBomb;
		minTimeBombCount = level.minTimeBombCount;
		maxTimeBombCount = level.maxTimeBombCount;
        timeBombSpawnCountPerMove = level.timeBombSpawnCountPerMove;
        defaultFallBackTime = level.defaultFallBackTime;
		isTimerGame = level.isTimerGame;
		isMaxMovesGame = level.isMaxMovesGame;
		//isScoreGame = level.isScoreGame;
		isClearShadedGame = level.isClearShadedGame;
		isClearChocolateGame = level.isClearChocolateGame;
		isGetTypesGame = level.isGetTypesGame;
        isHardLevelGame = level.hardLevel;
		isTreasureGame = level.isTreasureGame;
		isSpecialJewelGame = level.isSpecialJewelGame;
		isPenguinGame = level.isPenguinGame;
		isFairyGame = level.isFairyGame;
        isYetiGame = level.isYetiGame;
        isBossGame = level.isBossGame;
		
		TimeGiven = level.givenTime;
		allowedMoves = level.allowedMoves;
		scoreRequiredWin = level.scoreRequiredWin;
		scoreEndsGame = level.scoreEndsGame;
		
		if (level.scoreToReach == null) level.scoreToReach = new int[3];
		
        goalScore   = level.goalScore;

		scoreToReach = level.scoreToReach[0];
		scoreMilestone2 = level.scoreToReach[1];
		scoreMilestone3 = level.scoreToReach[2];
		
		shadesRequiredWin = level.shadesRequiredWin;
		shadesEndsGame = level.shadesEndsGame;
		chocolateRequiredWin = level.chocolateRequiredWin;
		chocolateEndsGame = level.chocolateEndsGame;
		typesRequiredWin = level.typesRequiredWin;
		typeEndsGame = level.typesEndsGame;
		
		if (level.numToGet == null) level.numToGet = new int[(int)LEItem.COLOR.NORMAL_COUNT];
		
		numToGet = level.numToGet;
        enableTreasure = level.enableTreasure;
		treasureRequiredWin = level.treasureRequiredWin;
		treasureEndsGame = level.treasureEndsGame;
        countPotion1    = level.countPotion1;
        countPotion2    = level.countPotion2;
        countPotion3    = level.countPotion3;
		
		maxOnScreen = level.maxOnScreen;
	    treasureSpawnCountPerMove = level.treasureSpawnCountPerMove;

        if (level.specialJewels == null) level.specialJewels = new int[9];

        specialJewels = level.specialJewels;

        maxPenguinOnScreen = level.maxPenguinOnScreen;
        numberOfPenguin = level.numberOfPenguin;
        chanceToPenguinSpawn = level.chanceToPenguinSpawn;

        enableFairyPiece = level.enableFairyPiece;
        numberOfFairy = level.numberOfFairy;
        movePerFairyPiece = level.movePerFairyPiece;
        minFairyPieceCount = level.minFairyPieceCount;
        maxFairyPieceCount = level.maxFairyPieceCount;
	    fairyPieceSpawnCountPerMove = level.fairyPieceSpawnCountPerMove;

        enableMysteryPiece = level.enableMysteryPiece;
        movePerMysteryPiece = level.movePerMysteryPiece;
        minMysteryPieceCount = level.minMysteryPieceCount;
        maxMysteryPieceCount = level.maxMysteryPieceCount;
	    mysteryPieceSpawnCountPerMove = level.mysteryPieceSpawnCountPerMove;

        if (level.mysteryPieceConvertProbability == null) {
            level.mysteryPieceConvertProbability = new int[] { 100, 100, 30, 80, 70, 29, 80, 80, 50, 30, 20, 100, 1, 80, 50, 50, 20, 30};
        }
        mysteryPieceConvertProbability = level.mysteryPieceConvertProbability;

        enableChameleon = level.enableChameleon;
        movePerChameleon = level.movePerChameleon;
        minChameleonPieceCount = level.minChameleonPieceCount;
        maxChameleonPieceCount = level.maxChameleonPieceCount;
	    chameleonSpawnCountPerMove = level.chameleonSpawnCountPerMove;

        if (level.roundChocoSpawnColumn == null) level.roundChocoSpawnColumn = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1};

        roundChocoSpawnColumn = level.roundChocoSpawnColumn;

        bossHealth = level.bossHealth;
        bossType = level.bossType;
        bossActionPerMove = level.bossActionPerMove;
        bossHealCount = level.bossHealCount;
        minBossAttackCount = level.minBossAttackCount;
        maxBossAttackCount = level.maxBossAttackCount;
        if (level.bossAttackConvertProbability == null) {
            level.bossAttackConvertProbability = new int[] { 25, 20, 10, 5, 5, 5, 5, 20, 5 };
        }
        bossAttackConvertProbability = level.bossAttackConvertProbability;

        yetiHealth = level.yetiHealth;
        attackPerMove = level.attackPerMove;
        yetiBallDropProbability = level.yetiBallDropProbability;
        minYetiBallPieceCount = level.minYetiBallPieceCount;
        maxYetiBallPieceCount = level.maxYetiBallPieceCount;
        minYetiAttackCount = level.minYetiAttackCount;
        maxYetiAttackCount = level.maxYetiAttackCount;

        if (level.greenBubbleSpawnColumn == null) level.greenBubbleSpawnColumn = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1};

        greenBubbleSpawnColumn = level.greenBubbleSpawnColumn;

        if (level.attackConvertProbability == null) {
            level.attackConvertProbability = new int[] { 25, 20, 10, 5, 5, 5, 5, 20, 5 };
        }

        attackConvertProbability = level.attackConvertProbability;

        if (level.normalProbability == null) {
            normalProbability = new List<int>(new int[]{100, 100, 100, 100, 100, 100, 100, 100});
        } else {
            normalProbability = new List<int>(level.normalProbability);

            // adjust array size.
            do
            {
                if(normalProbability.Count < (int)LEItem.COLOR.NORMAL_COUNT)
                    normalProbability.Add(100);                
                if(normalProbability.Count >= (int)LEItem.COLOR.NORMAL_COUNT)
                    break;
            }while(true);            
        }

        movePerTreasure = level.movePerTreasure;

        // [FIX_MISSION]
        missionType         = (MISSION_TYPE)level.missionType;
        isMixMission        = level.isMixMission;     
        countRoundChocho    = level.countRoundChocho;
        countJamBottom      = level.countJamBottom;   
        countRectChocho     = level.countRectChocho;  
        countCottonCandy    = level.countCottonCandy; 
        countSodaCan        = level.countSodaCan;     
        countSugarBlock     = level.countSugarBlock;  
        countZellatto       = level.countZellatto;    
        countPotion1        = level.countPotion1;     
        countPotion2        = level.countPotion2;     
        countPotion3        = level.countPotion3;     
        countChocoBar       = level.countChocoBar;    
        countCursedBottom   = level.countCursedBottom;
        countAiWinBoard     = level.countAiWinBoard;  
        ai_intelligence     = level.ai_intelligence;
        countCookieJelly    = level.countCookieJelly;
        countColorBox       = level.countColorBox;
        countWaffleCooker   = level.countWaffleCooker;
        countMudShade       = level.countMudShade;

        enableStrawberry    = level.enableStrawberry;
        movePerStrawberry   = level.movePerStrawberry;
        minStrawberryCount  = level.minStrawberryCount;
        maxStrawberryCount  = level.maxStrawberryCount;
        strawberrySpawnCountPerMove = level.strawberrySpawnCountPerMove;

        enableGreenBubble   = level.enableGreenBubble;
        movePerGreenBubble  = level.movePerGreenBubble;
        lifeNewGreenBubble  = level.lifeNewGreenBubble;
        minGreenBubble      = level.minGreenBubble;
        maxGreenBubble      = level.maxGreenBubble;
        greenBubbleSpawnCountPerMove = level.greenBubbleSpawnCountPerMove;

        // [Sugar_Cherry]
        enableSugarCherry    = level.enableSugarCherry;
        movePerSugarCherry   = level.movePerSugarCherry;
        minSugarCherryCount  = level.minSugarCherryCount;
        maxSugarCherryCount  = level.maxSugarCherryCount;
        sugarCherrySpawnCountPerMove = level.sugarCherrySpawnCountPerMove;

        // [Zellato]
        enableZellato        = level.enableZellato;
        movePerZellato       = level.movePerZellato;
        minZellatoCount      = level.minZellatoCount;
        maxZellatoCount      = level.maxZellatoCount;
        zellatoSpawnCountPerMove = level.zellatoSpawnCountPerMove;

        // [Zellato]
        enableAddTimeBlock   = level.enableAddTimeBlock;
        movePerAddTimeBlock  = level.movePerAddTimeBlock;
        minAddTimeBlockCount = level.minAddTimeBlockCount;
        maxAddTimeBlockCount = level.maxAddTimeBlockCount;
        addTimeBlockSpawnCountPerMove = level.addTimeBlockSpawnCountPerMove;
	}

	public Data.Level GetConditionsInspector(Data.Level level) {
        level.enableSpiralSnow = enableSpiralSnow;
        level.movePerSpiralSnow = movePerSpiralSnow;
        level.minSpiralSnowCount = minSpiralSnowCount;
        level.maxSpiralSnowCount = maxSpiralSnowCount;
        level.spiralSnowSpawnCountPerMove = spiralSnowSpawnCountPerMove;
        level.lifeNewRoundChoco    = lifeNewRoundChoco;    // [ROUND_CHOCO]
		level.enableTimeBomb = enableTimeBomb;
		level.movePerTimeBomb = movePerTimeBomb;
		level.minTimeBombCount = minTimeBombCount;
		level.maxTimeBombCount = maxTimeBombCount;
        level.timeBombSpawnCountPerMove = timeBombSpawnCountPerMove;
        level.defaultFallBackTime = defaultFallBackTime;
		level.isTimerGame = isTimerGame;
		level.isMaxMovesGame = isMaxMovesGame;
		//level.isScoreGame = isScoreGame;
		level.isClearShadedGame = isClearShadedGame;
		level.isClearChocolateGame = isClearChocolateGame;
		level.isGetTypesGame = isGetTypesGame;
        level.hardLevel = isHardLevelGame;
		level.isTreasureGame = isTreasureGame;
		level.isSpecialJewelGame = isSpecialJewelGame;
		level.isPenguinGame = isPenguinGame;
		level.isFairyGame = isFairyGame;
        level.isYetiGame = isYetiGame;
        level.isBossGame = isBossGame;
		
		level.givenTime = TimeGiven;
		level.allowedMoves = allowedMoves;
		level.scoreRequiredWin = scoreRequiredWin;
		level.scoreEndsGame = scoreEndsGame;

        level.goalScore         = goalScore;

		level.scoreToReach[0] = scoreToReach;
		level.scoreToReach[1] = scoreMilestone2;
		level.scoreToReach[2] = scoreMilestone3;
		
		level.shadesRequiredWin = shadesRequiredWin;
		level.shadesEndsGame = shadesEndsGame;
		level.chocolateRequiredWin = chocolateRequiredWin;
		level.chocolateEndsGame = chocolateEndsGame;
		level.typesRequiredWin = typesRequiredWin;
		level.typesEndsGame = typeEndsGame;

		level.numToGet = numToGet;
        level.enableTreasure = enableTreasure;
		level.treasureRequiredWin = treasureRequiredWin;
		level.treasureEndsGame = treasureEndsGame;
		level.countPotion1  = countPotion1;
        level.countPotion2  = countPotion2;
        level.countPotion3  = countPotion3;
		level.maxOnScreen = maxOnScreen;
	    level.treasureSpawnCountPerMove = treasureSpawnCountPerMove;
        level.specialJewels = specialJewels;
        level.maxPenguinOnScreen = maxPenguinOnScreen;
        level.numberOfPenguin = numberOfPenguin;
        level.numberOfFairy = numberOfFairy;
        level.chanceToPenguinSpawn = chanceToPenguinSpawn;

        level.enableFairyPiece = enableFairyPiece;
        level.movePerFairyPiece = movePerFairyPiece;
        level.minFairyPieceCount = minFairyPieceCount;
        level.maxFairyPieceCount = maxFairyPieceCount;
	    level.fairyPieceSpawnCountPerMove = fairyPieceSpawnCountPerMove;

        level.enableMysteryPiece = enableMysteryPiece;
        level.movePerMysteryPiece = movePerMysteryPiece;
        level.minMysteryPieceCount = minMysteryPieceCount;
        level.maxMysteryPieceCount = maxMysteryPieceCount;
	    level.mysteryPieceSpawnCountPerMove = mysteryPieceSpawnCountPerMove;
        level.mysteryPieceConvertProbability = mysteryPieceConvertProbability;

        level.enableChameleon = enableChameleon;
        level.movePerChameleon = movePerChameleon;
        level.minChameleonPieceCount = minChameleonPieceCount;
        level.maxChameleonPieceCount = maxChameleonPieceCount;
	    level.chameleonSpawnCountPerMove = chameleonSpawnCountPerMove;
        level.roundChocoSpawnColumn = roundChocoSpawnColumn;

        level.bossHealth = bossHealth;
        level.bossType = bossType;
        level.bossActionPerMove = bossActionPerMove;
        level.bossHealCount = bossHealCount;
        level.minBossAttackCount = minBossAttackCount;
        level.maxBossAttackCount = maxBossAttackCount;
        level.bossAttackConvertProbability = bossAttackConvertProbability;

        level.yetiHealth = yetiHealth;
        level.attackPerMove = attackPerMove;
        level.yetiBallDropProbability = yetiBallDropProbability;
        level.minYetiBallPieceCount = minYetiBallPieceCount;
        level.maxYetiBallPieceCount = maxYetiBallPieceCount;
        level.minYetiAttackCount = minYetiAttackCount;
        level.maxYetiAttackCount = maxYetiAttackCount;
        level.attackConvertProbability = attackConvertProbability;
        level.greenBubbleSpawnColumn = greenBubbleSpawnColumn;

		if (normalProbability != null) level.normalProbability = normalProbability.ToArray ();

        level.movePerTreasure = movePerTreasure;

        // [FIX_MISSION]
        level.missionType           = (int)missionType;
        level.isMixMission          = isMixMission;     
        level.countRoundChocho      = countRoundChocho;
        level.countJamBottom        = countJamBottom;   
        level.countRectChocho       = countRectChocho;  
        level.countCottonCandy      = countCottonCandy; 
        level.countSodaCan          = countSodaCan;     
        level.countSugarBlock       = countSugarBlock;  
        level.countZellatto         = countZellatto;    
        level.countPotion1          = countPotion1;     
        level.countPotion2          = countPotion2;     
        level.countPotion3          = countPotion3;     
        level.countChocoBar         = countChocoBar;    
        level.countCursedBottom     = countCursedBottom;
        level.countAiWinBoard       = countAiWinBoard;  
        level.ai_intelligence       = ai_intelligence;
        level.countCookieJelly      = countCookieJelly;
        level.countColorBox         = countColorBox;
        level.countWaffleCooker     = countWaffleCooker;
        level.countMudShade         = countMudShade;

        //
        level.enableStrawberry      = enableStrawberry;
        level.movePerStrawberry     = movePerStrawberry;
        level.minStrawberryCount    = minStrawberryCount;
        level.maxStrawberryCount    = maxStrawberryCount;
        level.strawberrySpawnCountPerMove = strawberrySpawnCountPerMove;
        //

        //
        level.enableGreenBubble     = enableGreenBubble;
        level.movePerGreenBubble    = movePerGreenBubble;
        level.lifeNewGreenBubble    = lifeNewGreenBubble;
        level.minGreenBubble        = minGreenBubble;
        level.maxGreenBubble        = maxGreenBubble;
        level.greenBubbleSpawnCountPerMove = greenBubbleSpawnCountPerMove;
        //

        // [SUGAR_CHERRY]
        level.enableSugarCherry      = enableSugarCherry;
        level.movePerSugarCherry     = movePerSugarCherry;
        level.minSugarCherryCount    = minSugarCherryCount;
        level.maxSugarCherryCount    = maxSugarCherryCount;
        level.sugarCherrySpawnCountPerMove = sugarCherrySpawnCountPerMove;

        // [ZELLATO]
        level.enableZellato      = enableZellato;
        level.movePerZellato     = movePerZellato;
        level.minZellatoCount    = minZellatoCount;
        level.maxZellatoCount    = maxZellatoCount;
        level.zellatoSpawnCountPerMove = zellatoSpawnCountPerMove;

        // [Add Time Block]
        level.enableAddTimeBlock = enableAddTimeBlock;
        level.movePerAddTimeBlock= movePerAddTimeBlock;
        level.minAddTimeBlockCount= minAddTimeBlockCount;
        level.maxAddTimeBlockCount= maxAddTimeBlockCount;
        level.addTimeBlockSpawnCountPerMove = addTimeBlockSpawnCountPerMove;
		
//		level.isTreasure = isTreasure;
//		level.TreasureItemCount = TreasureItemCount;
//		level.TreasureItemKey = TreasureItemKey;
		
		
		return level;
	}

    public void UpdateGameModeState(GAME_MODE mode)
    {
        this.isTimerGame = (mode == GAME_MODE.TIMER) ? true : false;
        this.TimeGiven = (mode == GAME_MODE.TIMER) ? this.TimeGiven : 0;

        this.isMaxMovesGame = (mode == GAME_MODE.TIMER) ? false : true;
        this.allowedMoves = (mode == GAME_MODE.TIMER) ? 0 : this.allowedMoves;
        //this.isScoreGame = true;
        this.scoreRequiredWin = (mode == GAME_MODE.TIMER) ? false : this.scoreRequiredWin;
        this.scoreEndsGame = (mode == GAME_MODE.TIMER) ? false : this.scoreEndsGame;

        this.isClearShadedGame = (mode == GAME_MODE.ICE) ? true : false;
        this.shadesRequiredWin = (mode == GAME_MODE.ICE) ? this.shadesRequiredWin : false;
        this.shadesEndsGame = (mode == GAME_MODE.ICE) ? this.shadesEndsGame : false;

        this.isClearChocolateGame = (mode == GAME_MODE.SNOWMAN) ? true : false;
        this.chocolateRequiredWin = (mode == GAME_MODE.SNOWMAN) ? this.chocolateRequiredWin : false;
        this.chocolateEndsGame = (mode == GAME_MODE.SNOWMAN) ? this.chocolateEndsGame : false;

        this.isGetTypesGame = (mode == GAME_MODE.JEWEL) ? true : false;
        this.typesRequiredWin = (mode == GAME_MODE.JEWEL) ? this.typesRequiredWin : false;
        this.typeEndsGame = (mode == GAME_MODE.JEWEL) ? this.typeEndsGame : false;
        for (int i = 0; i < this.numToGet.Length; i++) {
            this.numToGet[i] = (mode == GAME_MODE.JEWEL) ? this.numToGet[i] : 0;
        }

        this.isTreasureGame = (mode == GAME_MODE.INGREDIENT) ? true : false;
        this.treasureRequiredWin = (mode == GAME_MODE.INGREDIENT) ? this.treasureRequiredWin : false;
        this.treasureEndsGame = (mode == GAME_MODE.INGREDIENT) ? this.treasureEndsGame : false;
        this.countPotion1 = (mode == GAME_MODE.INGREDIENT) ? this.countPotion1 : 0;
        this.countPotion2 = (mode == GAME_MODE.INGREDIENT) ? this.countPotion2 : 0;
        this.countPotion3 = (mode == GAME_MODE.INGREDIENT) ? this.countPotion3 : 0;
        
        this.maxOnScreen = (mode == GAME_MODE.INGREDIENT) ? this.maxOnScreen : 3;
        this.treasureSpawnCountPerMove = (mode == GAME_MODE.INGREDIENT) ? this.treasureSpawnCountPerMove : 1;

        this.isSpecialJewelGame = (mode == GAME_MODE.SPECIAL_JEWEL) ? true : false;
        for (int i = 0; i < this.specialJewels.Length; i++) {
            this.specialJewels[i] = (mode == GAME_MODE.SPECIAL_JEWEL) ? this.specialJewels[i] : 0;
        }

        this.isPenguinGame = (mode == GAME_MODE.PENGUIN) ? true : false;
        this.numberOfPenguin = (mode == GAME_MODE.PENGUIN) ? this.numberOfPenguin : 0;

        this.isFairyGame = (mode == GAME_MODE.FAIRY) ? true : false;
        this.numberOfFairy = (mode == GAME_MODE.FAIRY) ? this.numberOfFairy : 0;

        this.isBossGame = (mode == GAME_MODE.BOSS) ? true : false;
        this.bossHealth = (mode == GAME_MODE.BOSS) ? this.bossHealth : 0;
        this.bossType = (mode == GAME_MODE.BOSS) ? this.bossType : 0;
        this.bossActionPerMove = (mode == GAME_MODE.BOSS) ? this.bossActionPerMove : 0;
        this.bossHealCount = (mode == GAME_MODE.BOSS) ? this.bossHealCount : 0;

        this.isYetiGame = (mode == GAME_MODE.YETI) ? true : false;
        this.yetiHealth = (mode == GAME_MODE.YETI) ? this.yetiHealth : 0;
        this.attackPerMove = (mode == GAME_MODE.YETI) ? this.attackPerMove : 0;
        this.minYetiBallPieceCount = (mode == GAME_MODE.YETI) ? this.minYetiBallPieceCount : 0;
        this.maxYetiBallPieceCount = (mode == GAME_MODE.YETI) ? this.maxYetiBallPieceCount : 0;
    }

    public GAME_MODE GetGameMode()
    {
        //EditWinningConditions this = (EditWinningConditions)target;
        GAME_MODE mode = GAME_MODE.COUNT;
        if (this.isTimerGame == true) {
            mode = GAME_MODE.TIMER;
        } else if ((this.isClearShadedGame == true)) {
            mode = GAME_MODE.ICE;
        } else if ((this.isClearChocolateGame == true)) {
            mode = GAME_MODE.SNOWMAN;
        } else if ((this.isGetTypesGame == true)) {
            mode = GAME_MODE.JEWEL;
        } else if ((this.isTreasureGame == true)) {
            mode = GAME_MODE.INGREDIENT;
        } else if (this.isSpecialJewelGame == true) {
            mode = GAME_MODE.SPECIAL_JEWEL;
        } else if (this.isPenguinGame == true) {
            mode = GAME_MODE.PENGUIN;
        } else if (this.isFairyGame == true) {
            mode = GAME_MODE.FAIRY;
        } else if (this.isYetiGame == true) {
            mode = GAME_MODE.YETI;
        } else if (this.isBossGame == true) {
            mode = GAME_MODE.BOSS;
        } else if (this.isMaxMovesGame == true) {
            mode = GAME_MODE.MOVE;
        } else {
            Debug.LogError("Error, Check Game Mode !!");
            mode = GAME_MODE.COUNT;
        }
        return mode;
    }
}
