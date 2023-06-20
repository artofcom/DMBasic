//#if !USE_DLLDATACLASS

using UnityEngine;
using JsonFx.Json;
using ProtoBuf;
using System.Collections.Generic;
//using NOVNINE.Diagnostics;
using System;

namespace Data {

    public enum BASIC_INFOS
    {
        COUNT_CHAPTER           = 9,
        COUNT_LEVEL_PER_CHAPTER = 20,
        MAX_LEVEL_ID            = COUNT_CHAPTER*COUNT_LEVEL_PER_CHAPTER,
    };
    //public static int COUNT_TOTAL_CHAPTER    = 10;
    //public const int COUNT_LEVEL_PER_CHAPTER= 10;

	public enum ITEM_TYPE {NONE = 0,  COIN,LIFE,HAMMER,FIRECRACKER,MAGICSWAP,MAGICSHUFFLE, MORETURNBOOSTER, HBOMBBOOSTER, VBOMBBOOSTER, BOMBBOOSTER, RAINBOWBOOSTER,
        /*ING_SW01,ING_SW02,ING_SW03,ING_SW04,ING_FR01,ING_FR02,ING_FR03,ING_FR04,ING_SO01,ING_SO02,ING_SO03,ING_SO04,ING_FA01,ING_FA02,ING_FA03,ING_FA04, INFINITESTAMINA,*/
        MAX};
		
	public enum LEVEL_TYPE { RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE, PENGUIN, PURPLE2, YETI, FAIRY, BOSS}

	[JsonOptIn]
	[ProtoContract]
	public class Level
	{
		[JsonMember] public int Index { get; set; }
		[JsonIgnore] public Root root;

		public int ShadeCount 
		{ 
			get { 
				int shadeCount = 0;

				for (int i = 0; i < panels.Length; i++)
				{
					if (shaded[i] > -1) shadeCount++;
				}

				return shadeCount;
			}
		}

		public int SnowmanCount 
		{ 
			get { 
				int snowmanCount = 0;

				for (int i = 0; i < panels.Length; i++) 
				{ 
					if (panels[i] == 7) snowmanCount++;
				}

				return snowmanCount;
			}
		}

		[ProtoMember(1),JsonMember] public string key;
		[ProtoMember(2),JsonMember] public int[] panels;
		[ProtoMember(3),JsonMember] public int[] strengths;
		[ProtoMember(4),JsonMember] public int[] shaded;
		[ProtoMember(5),JsonMember] public int[] pieces;
		[ProtoMember(6),JsonMember] public int[] colors;
		[ProtoMember(7),JsonMember] public string[] panelInfos;
		[ProtoMember(8),JsonMember] public bool[] startPieces;
		[ProtoMember(9),JsonMember] public bool isTimerGame;
		[ProtoMember(10),JsonMember] public bool isMaxMovesGame;
		//[ProtoMember(11)] public bool isScoreGame;
		[ProtoMember(11),JsonMember] public bool isFairyGame;
		[ProtoMember(12),JsonMember] public bool isClearShadedGame;
		[ProtoMember(13),JsonMember] public bool isClearChocolateGame;
		[ProtoMember(14),JsonMember] public bool isGetTypesGame;
		[ProtoMember(15),JsonMember] public bool isTreasureGame;
		[ProtoMember(16),JsonMember] public bool isSpecialJewelGame;
		[ProtoMember(17),JsonMember] public bool isPenguinGame;
		[ProtoMember(18),JsonMember] public bool isYetiGame;
		[ProtoMember(19),JsonMember] public float givenTime;
		[ProtoMember(20),JsonMember] public int allowedMoves;
		[ProtoMember(21),JsonMember] public bool scoreRequiredWin;
		[ProtoMember(22),JsonMember] public bool scoreEndsGame;
		[ProtoMember(23),JsonMember] public int[] scoreToReach;
		[ProtoMember(24),JsonMember] public bool shadesRequiredWin;
		[ProtoMember(25),JsonMember] public bool shadesEndsGame;
		[ProtoMember(26),JsonMember] public bool chocolateRequiredWin;
		[ProtoMember(27),JsonMember] public bool chocolateEndsGame;
		[ProtoMember(28),JsonMember] public bool typesRequiredWin;
		[ProtoMember(29),JsonMember] public bool typesEndsGame;
		[ProtoMember(30),JsonMember] public int[] numToGet;
		[ProtoMember(31),JsonMember] public bool treasureRequiredWin;
		[ProtoMember(32),JsonMember] public bool treasureEndsGame;
		[ProtoMember(33),JsonMember] public bool enableTreasure;
		//[ProtoMember(34)] public int potion1; => countPotion1
		//[ProtoMember(36)] public int potion2; => countPotion2
		//[ProtoMember(37)] public int potion3; => countPotion3
		[ProtoMember(34),JsonMember] public int maxOnScreen;
		[ProtoMember(35),JsonMember] public int chanceToSpawn;
		[ProtoMember(36),JsonMember] public int[] treasureGoal;
		[ProtoMember(37),JsonMember] public int[] changerColorIndex;
		//[ProtoMember(39)] public bool useTypeProbability;
		//[ProtoMember(40)] public int[] easyProbability;
		[ProtoMember(38),JsonMember] public int[] normalProbability;
		//[ProtoMember(42)] public int[] hardProbability;
		[ProtoMember(39),JsonMember] public bool enableTimeBomb;
		[ProtoMember(40),JsonMember] public int movePerTimeBomb;
		[ProtoMember(41),JsonMember] public int minTimeBombCount;
		[ProtoMember(42),JsonMember] public int maxTimeBombCount;
		[ProtoMember(43),JsonMember] public int defaultFallBackTime;
		[ProtoMember(44),JsonMember] public int[] fallBackTime;
		[ProtoMember(45),JsonMember] public bool enableSpiralSnow;
		[ProtoMember(46),JsonMember] public int movePerSpiralSnow;
		[ProtoMember(47),JsonMember] public int minSpiralSnowCount;
		[ProtoMember(48),JsonMember] public int maxSpiralSnowCount;
		[ProtoMember(49),JsonMember] public int[] specialJewels;
		[ProtoMember(50),JsonMember] public int maxPenguinOnScreen;
		[ProtoMember(51),JsonMember] public int numberOfPenguin;
		[ProtoMember(52),JsonMember] public int[] penguinSpawn;
		[ProtoMember(53),JsonMember] public int chanceToPenguinSpawn;
		[ProtoMember(54),JsonMember] public bool enableMysteryPiece;
		[ProtoMember(55),JsonMember] public int movePerMysteryPiece;
		[ProtoMember(56),JsonMember] public int minMysteryPieceCount;
		[ProtoMember(57),JsonMember] public int maxMysteryPieceCount;
		[ProtoMember(58),JsonMember] public int[] mysteryPieceConvertProbability;
		[ProtoMember(59),JsonMember] public bool enableChameleon;
		[ProtoMember(60),JsonMember] public int movePerChameleon;
		[ProtoMember(61),JsonMember] public int minChameleonPieceCount;
		[ProtoMember(62),JsonMember] public int maxChameleonPieceCount;
		[ProtoMember(63),JsonMember] public int[] roundChocoSpawnColumn;
		[ProtoMember(64),JsonMember] public int yetiHealth;
		[ProtoMember(65),JsonMember] public int attackPerMove;
		[ProtoMember(66),JsonMember] public int yetiBallDropProbability;
		[ProtoMember(67),JsonMember] public int minYetiBallPieceCount;
		[ProtoMember(68),JsonMember] public int maxYetiBallPieceCount;
		[ProtoMember(69),JsonMember] public int minYetiAttackCount;
		[ProtoMember(70),JsonMember] public int maxYetiAttackCount;
		[ProtoMember(71),JsonMember] public int[] attackConvertProbability;
		[ProtoMember(72),JsonMember] public int[] greenBubbleSpawnColumn;
		[ProtoMember(73),JsonMember] public int[] portalTypes;
		[ProtoMember(74),JsonMember] public int[] portalDIndices;
		[ProtoMember(75),JsonMember] public int[] portalUIndices;
		[ProtoMember(76),JsonMember] public int[] conveyorIndices;
		[ProtoMember(77),JsonMember] public bool[] hasRiver;
		[ProtoMember(78),JsonMember] public int[] idxOverRiver;        
		[ProtoMember(79),JsonMember] public int[] defaultSpawnColumn;
		[ProtoMember(80),JsonMember] public int movePerTreasure;
		[ProtoMember(81),JsonMember] public int treasureSpawnCountPerMove;
		[ProtoMember(82),JsonMember] public int spiralSnowSpawnCountPerMove;
		[ProtoMember(83),JsonMember] public int timeBombSpawnCountPerMove;
		[ProtoMember(84),JsonMember] public int mysteryPieceSpawnCountPerMove;
		[ProtoMember(85),JsonMember] public int chameleonSpawnCountPerMove;
		[ProtoMember(86),JsonMember] public int numberOfFairy;
		[ProtoMember(87),JsonMember] public int fairyPieceSpawnCountPerMove;
		[ProtoMember(88),JsonMember] public int movePerFairyPiece;
		[ProtoMember(89),JsonMember] public int minFairyPieceCount;
		[ProtoMember(90),JsonMember] public int maxFairyPieceCount;
		[ProtoMember(91),JsonMember] public bool enableFairyPiece;
		[ProtoMember(92),JsonMember] public bool isBossGame;
		[ProtoMember(93),JsonMember] public int bossHealth;
		[ProtoMember(94),JsonMember] public int bossType;
		[ProtoMember(95),JsonMember] public int minBossAttackCount;
		[ProtoMember(96),JsonMember] public int maxBossAttackCount;
		[ProtoMember(97),JsonMember] public int bossActionPerMove;
		[ProtoMember(98),JsonMember] public int bossHealCount;
		[ProtoMember(99),JsonMember] public int[] bossAttackConvertProbability;
		[ProtoMember(100),JsonMember] public int[] treasureSpawnColumn;

		// [FIX_MISSION]
		// mix objects.
		[ProtoMember(101),JsonMember] public int missionType = 0;
		[ProtoMember(102),JsonMember] public bool isMixMission    = false;
		[ProtoMember(103),JsonMember] public int countRoundChocho = 0;
		[ProtoMember(104),JsonMember] public int countJamBottom   = 0;
		[ProtoMember(105),JsonMember] public int countRectChocho  = 0;
		[ProtoMember(106),JsonMember] public int countCottonCandy = 0;
		[ProtoMember(107),JsonMember] public int countSodaCan     = 0;
		[ProtoMember(108),JsonMember] public int countSugarBlock  = 0;
		[ProtoMember(109),JsonMember] public int countZellatto    = 0;
		// fill(drop) objects.
		[ProtoMember(110),JsonMember] public int countPotion1     = 0;
		[ProtoMember(111),JsonMember] public int countPotion2     = 0;
		[ProtoMember(112),JsonMember] public int countPotion3     = 0;
		// find objects
		[ProtoMember(113),JsonMember] public int countChocoBar    = 0;
		// clear objects.
		[ProtoMember(114),JsonMember] public int countCursedBottom= 0;
		// ai score.
		[ProtoMember(115),JsonMember] public int countAiWinBoard  = 0; // 승리를 위한 ai win score.
		//
		[ProtoMember(116),JsonMember] public int[] ai_taken_index;     // 보드당 초기화된 ai taken index (-1;none, 0;me, 1;ai)
		[ProtoMember(117),JsonMember] public int ai_intelligence   = 0;
		[ProtoMember(118),JsonMember] public int[] eShadeType;             // [JAM_SHADE]
		[ProtoMember(119),JsonMember] public int lifeNewRoundChoco = 1;    // [ROUND_CHOCO]
		[ProtoMember(120),JsonMember] public int[] eBarType;               // [NET_SHADE] : LEItem.CHOCO_BAR
		[ProtoMember(121),JsonMember] public int[] indexBar;               // [NET_SHADE] : 0, 1, 2..
		[ProtoMember(122),JsonMember] public bool enableStrawberry    = false;
		[ProtoMember(123),JsonMember] public int movePerStrawberry    = 0;
		[ProtoMember(124),JsonMember] public int minStrawberryCount   = 0;
		[ProtoMember(125),JsonMember] public int maxStrawberryCount   = 0;
		[ProtoMember(126),JsonMember] public int strawberrySpawnCountPerMove = 0;
		[ProtoMember(127),JsonMember] public bool enableGreenBubble    = false;
		[ProtoMember(128),JsonMember] public int movePerGreenBubble    = 0;
		[ProtoMember(129),JsonMember] public int lifeNewGreenBubble   = 0;
		[ProtoMember(130),JsonMember] public int minGreenBubble   = 0;
		[ProtoMember(131),JsonMember] public int maxGreenBubble   = 0;
		[ProtoMember(132),JsonMember] public int greenBubbleSpawnCountPerMove = 0;
		// [SUGAR_CHERRY]
		[ProtoMember(133),JsonMember] public bool enableSugarCherry    = false;
		[ProtoMember(134),JsonMember] public int movePerSugarCherry    = 0;
		[ProtoMember(135),JsonMember] public int minSugarCherryCount   = 0;
		[ProtoMember(136),JsonMember] public int maxSugarCherryCount   = 0;
		[ProtoMember(137),JsonMember] public int sugarCherrySpawnCountPerMove = 0;

		// [ZELLATO]
		[ProtoMember(138),JsonMember] public bool enableZellato    = false;
		[ProtoMember(139),JsonMember] public int movePerZellato    = 0;
		[ProtoMember(140),JsonMember] public int minZellatoCount   = 0;
		[ProtoMember(141),JsonMember] public int maxZellatoCount   = 0;
		[ProtoMember(142),JsonMember] public int zellatoSpawnCountPerMove = 0;
		[ProtoMember(143),JsonMember] public int[] strawberrySpawnColumn;
		[ProtoMember(144),JsonMember] public int[] sugarCherrySpawnColumn;
		[ProtoMember(145),JsonMember] public int[] zellattoSpawnColumn;
		[ProtoMember(146),JsonMember] public int[] eHelperType;            // LEItem.HELPER_TYPE
		[ProtoMember(147),JsonMember] public string[] riverInfos;

        [ProtoMember(148),JsonMember] public int[] lifePieceCover;      // cover_ice.
        [ProtoMember(149),JsonMember] public int[] fenceDirection;      // direction of fence.

        [ProtoMember(150),JsonMember] public int countCookieJelly   = 0;
        [ProtoMember(151),JsonMember] public int[] panelColors;         // color of panel.
        [ProtoMember(152),JsonMember] public int countColorBox      = 0;
        [ProtoMember(153),JsonMember] public int countWaffleCooker  = 0;
        [ProtoMember(154),JsonMember] public int countMudShade      = 0;
        [ProtoMember(155),JsonMember] public bool hardLevel      = false;
        [ProtoMember(156),JsonMember] public int goalScore       = 0;
        [ProtoMember(157),JsonMember] public int[] addTimeBlockSpawnColumn;
        [ProtoMember(158),JsonMember] public bool enableAddTimeBlock    = false;
		[ProtoMember(159),JsonMember] public int movePerAddTimeBlock  = 0;
		[ProtoMember(160),JsonMember] public int minAddTimeBlockCount   = 0;
		[ProtoMember(161),JsonMember] public int maxAddTimeBlockCount   = 0;
		[ProtoMember(162),JsonMember] public int addTimeBlockSpawnCountPerMove = 0;

		[ProtoMember(163),JsonMember] public NudgeData[][] nudge;
		
		public class NudgeData
		{
			public string data1;
			public int data2;
		}

		public LEVEL_TYPE Type {
			get {
				if (isClearShadedGame) {
					return LEVEL_TYPE.BLUE;
				} else if (isClearChocolateGame) {
					return LEVEL_TYPE.RED;
				} else if (isTreasureGame) {
					return LEVEL_TYPE.PURPLE;
				} else if (isGetTypesGame) {
					return LEVEL_TYPE.ORANGE;
				} else if (isSpecialJewelGame) {
					return LEVEL_TYPE.PURPLE2;
				} else if (isPenguinGame) {
					return LEVEL_TYPE.PENGUIN;
				} else if (isYetiGame) {
					return LEVEL_TYPE.YETI;
				} else if (isFairyGame) {
					return LEVEL_TYPE.FAIRY;
				} else if (isBossGame) {
					return LEVEL_TYPE.BOSS;
				} else if (isTimerGame) {
					return LEVEL_TYPE.YELLOW;
				} else if (isMaxMovesGame) {
					return LEVEL_TYPE.GREEN;
				} else {
					Debug.LogError(string.Format("Level {0} - WinningCondition Setting Is Wrong !!!", key));
					return default(LEVEL_TYPE);
				}
			}
		}

		public void InitLevel(int index, Root _root) 
		{
			Index = index;
			root = _root;
		}
	}
	
	[ProtoContract]
	public class Tutorial
	{
		[ProtoMember(1)] public string strID;
		[ProtoMember(2)] public int nStatus;
	}

    [ProtoContract]
    public class Purchaseinfo
    {
        [ProtoMember(1)] public string strStoreId = null;

        //[ProtoMember(2)] public string purchaseItemID = null;
        //[ProtoMember(3)] public string signature = null;
        //[ProtoMember(4)] public string purchaseData = null;
    }
     
    // XXX ( FB data...   )
	/*[ProtoContract]
	public class FBFriendInfo
	{
		[ProtoMember(1)] public string strFBUID = "";
		[ProtoMember(2)] public System.DateTime SendLifeDateTime = new System.DateTime();
		[ProtoMember(3)] public System.DateTime AskLifeDateTime  = new System.DateTime();
        //[ProtoMember(4)] public System.DateTime GiftReceivedDate  = new System.DateTime();
	}

	[ProtoContract]
	public class PuzzleItem 
	{
		[ProtoMember(1)] public int siUniqueNumber;
		[ProtoMember(2)] public System.Int16 ssCount;
	}*/
	
	[ProtoContract]
	public class LevelResultData 
	{
		[ProtoMember(1)] public System.Byte ucGrade = 0;
		[ProtoMember(2)] public System.Int64 sl64Score = 0;
		[ProtoMember(3)] public bool bCleared = false;
		//[ProtoMember(4)] public bool bReward = false;
		[ProtoMember(4)] public int siStraightFailCount = 0;	    // level을 clear하기 전까지 실패한 누적 횟수. clear하면 다시 reset 된다.	
	}

	[ProtoContract]
	public class PuzzlePlayerGameBaseData 
	{
		[ProtoMember(1)] public bool m_bRated = false;
        [ProtoMember(2)] public int m_slDailyRewardedCount = 0;		
        [ProtoMember(3)] public long m_DailyRewardDate = 0;
		[ProtoMember(4)] public long m_StaminaChargeTick = 0;
        [ProtoMember(5)] public long m_FreeGachaDate = 0;
        [ProtoMember(6)] public long m_AdsRewardedTick = 0;
        [ProtoMember(7)] public int m_siCountBuff             = 0;

        //[ProtoMember(3)] public System.DateTime m_DailyRewardDate = new System.DateTime();
		//[ProtoMember(4)] public System.DateTime m_StaminaChargeTick = new System.DateTime();
        //[ProtoMember(5)] public System.DateTime m_FreeGachaDate = new System.DateTime();
        //[ProtoMember(6)] public System.DateTime m_AdsRewaredTick = new System.DateTime();
        //[ProtoMember(2)] public int m_siTotalClearedLevelCount =0;		
		//[ProtoMember(2),JsonIgnore] public bool m_bLiked = false;
		//[ProtoMember(3),JsonIgnore] public int m_siBestCombo = 0;
		//[ProtoMember(5),JsonIgnore] public int m_siStraightWinCount = 0;
		//[ProtoMember(6),JsonIgnore] public bool m_bHammerUnlocked = false;
		//[ProtoMember(7),JsonIgnore] public bool m_bFirecrackerUnlocked = false;
		//[ProtoMember(8),JsonIgnore] public bool m_bMagicSwapUnlocked = false;
		//[ProtoMember(9),JsonIgnore] public bool m_bRainbowBustUnlocked = false;
		//[ProtoMember(10),JsonIgnore] public bool m_bMixedBoosterUnlocked = false;
		//[ProtoMember(11),JsonIgnore] public bool m_bTripleBoosterUnlocked = false;
		//[ProtoMember(12),JsonIgnore] public bool m_bSpecialBoosterUnlocked = false;

		//[ProtoMember(16),JsonIgnore] public string m_strDataSet = "A";
		//[ProtoMember(17),JsonIgnore] public string m_strFacebookUID = "";
        //[ProtoMember(5),JsonIgnore] public int m_siTotalClearedStarCount = 0;
        //[ProtoMember(14),JsonIgnore] public int m_siFanLetterExposeCount = 0;
		//[ProtoMember(15),JsonIgnore] public bool m_bSentFanLetter = false;
		//[ProtoMember(16),JsonIgnore] public bool m_bIsFirstPlayTutorial = false;

		//[ProtoMember(17),JsonIgnore] public int m_siCurrentTreasureIndex = 0;		
		//public bool Liked { get{ return m_bLiked;} set{m_bLiked = value;} }
		//public int BestCombo { get{ return m_siBestCombo;} set{m_siBestCombo = value;} }
		//public int TotalClearedLevelCount { get{ return m_siTotalClearedLevelCount;} set{m_siTotalClearedLevelCount = value;} }
		//public int TotalClearedStarCount { get{ return m_siTotalClearedStarCount;} set{m_siTotalClearedStarCount = value;} }
		//public int StraightWinCount { get{ return m_siStraightWinCount;} set{m_siStraightWinCount = value;} }

		//public bool HammerUnlocked {  get{ return m_bHammerUnlocked;}  set{m_bHammerUnlocked = value;}  }
		//public bool FirecrackerUnlocked { get{ return m_bFirecrackerUnlocked;} set{m_bFirecrackerUnlocked = value;} }
		//public bool MagicSwapUnlocked { get{ return m_bMagicSwapUnlocked;} set{m_bMagicSwapUnlocked = value;} }
		//public bool RainbowBustUnlocked { get{ return m_bRainbowBustUnlocked;} set{m_bRainbowBustUnlocked = value;} }
		//public bool MixedBoosterUnlocked { get{ return m_bMixedBoosterUnlocked;} set{m_bMixedBoosterUnlocked = value;} }
		//public bool TripleBoosterUnlocked { get{ return m_bTripleBoosterUnlocked;} set{m_bTripleBoosterUnlocked = value;} }
		//public bool SpecialBoosterUnlocked { get{ return m_bSpecialBoosterUnlocked;} set{m_bSpecialBoosterUnlocked = value;} }

		//public int FanLetterExposeCount { get{ return m_siFanLetterExposeCount;} set{m_siFanLetterExposeCount = value;} }
		//public bool SentFanLetter { get{ return m_bSentFanLetter;} set{m_bSentFanLetter = value;} }
		//public bool IsFirstPlayTutorial { get{ return m_bIsFirstPlayTutorial;} set{m_bIsFirstPlayTutorial = value;} }
		//public int CurrentTreasureIndex { get{ return m_siCurrentTreasureIndex;} set{m_siCurrentTreasureIndex = value;} }

        public bool Rated { get{ return m_bRated;} set{m_bRated = value;} }
		public int DailyRewardedCount       { get{ return m_slDailyRewardedCount;} set{m_slDailyRewardedCount = value;} }
		public long DailyRewardDate         { get{ return m_DailyRewardDate;} set{m_DailyRewardDate = value;} }
        public long FreeGachaDate           { get{ return m_FreeGachaDate;} set{m_FreeGachaDate = value;} }
		public long StaminaChargeTick       { get{ return m_StaminaChargeTick;} set{m_StaminaChargeTick = value;} }
        public long AdsRewardedTick         { get{ return m_AdsRewardedTick;} set{m_AdsRewardedTick = value;} }
        
		//public string DataSet { get{ return m_strDataSet;} set{m_strDataSet = value;} }
		//public string FacebookUID { get{ return m_strFacebookUID;} set{m_strFacebookUID = value;} }
		
		
		public void Clear()
		{
			m_bRated = false;
			//m_bLiked = false;
			//m_siBestCombo = 0;
			//m_siTotalClearedLevelCount =0;
            m_siCountBuff            = 0;
			//m_siTotalClearedStarCount = 0;
			//m_siStraightWinCount = 0;
			//m_bHammerUnlocked = false;
			//m_bFirecrackerUnlocked = false;
			//m_bMagicSwapUnlocked = false;
			//m_bRainbowBustUnlocked = false;
			//m_bMixedBoosterUnlocked = false;
			//m_bTripleBoosterUnlocked = false;
			//m_bSpecialBoosterUnlocked = false;

			//m_siFanLetterExposeCount = 0;
			//m_bSentFanLetter = false;
			//m_bIsFirstPlayTutorial = false;
			//m_siCurrentTreasureIndex = 0;
			//m_slDailyRewardedCount = 0;

			m_DailyRewardDate = 0;
			m_StaminaChargeTick = 0;
            m_FreeGachaDate = 0;
            m_AdsRewardedTick = 0;
			//m_strDataSet = "A";
			//m_strFacebookUID = "";
		}
		
		public void Reset()
		{
			//m_siTotalClearedLevelCount =0;
            m_siCountBuff                = 0;
			//m_siTotalClearedStarCount = 0;
			//m_bHammerUnlocked = false;
			//m_bFirecrackerUnlocked = false;
			//m_bMagicSwapUnlocked = false;
			//m_bRainbowBustUnlocked = false;
			//m_bMixedBoosterUnlocked = false;
			//m_bTripleBoosterUnlocked = false;
			//m_bSpecialBoosterUnlocked = false;
			//m_siCurrentTreasureIndex = 0;
			//m_strDataSet = "A";
		}
	}
	
	[ProtoContract]
	public class PuzzlePlayerWalletInventoryData
	{
		[ProtoMember(1)] public int Coin = 0;
		[ProtoMember(2)] public int Life = GameData.CHARGEABLE_MAX_STAMINA;
		[ProtoMember(3)] public int Hammer = 0;
		[ProtoMember(4)] public int FireCracker =0;
		[ProtoMember(5)] public int MagicSwap = 0;
        [ProtoMember(6)] public int MagicShuffle = 0;

        [ProtoMember(7)] public int _5MoreTurnBooster = 0;
		[ProtoMember(8)] public int HBombBooster = 0;
		[ProtoMember(9)] public int VBombBooster = 0;
		[ProtoMember(10)] public int BombBooster = 0;
		[ProtoMember(11)] public int RainbowBooster = 0;
		
		public void Reset()
		{
			Coin = 0;
			Life = GameData.CHARGEABLE_MAX_STAMINA;
			Hammer = 0;
			FireCracker =0;
			MagicSwap = 0;
            MagicShuffle = 0;
            _5MoreTurnBooster = 0;
            HBombBooster = 0;
            VBombBooster = 0;
            BombBooster = 0;
            RainbowBooster = 0;
		}
	}
	
    // XXX ( ingrediant data... )
    /*
	[ProtoContract]
	public class PuzzlePlayerWalletIngrediantData
	{
		[ProtoMember(1)] public int Ing_Sw01 = 0;
		[ProtoMember(2)] public int Ing_Sw02 = 0;
		[ProtoMember(3)] public int Ing_Sw03 = 0;
		[ProtoMember(4)] public int Ing_Sw04 =0;
		[ProtoMember(5)] public int Ing_Fr01 = 0;
		[ProtoMember(6)] public int Ing_Fr02 = 0;
		[ProtoMember(7)] public int Ing_Fr03 = 0;
		[ProtoMember(8)] public int Ing_Fr04 = 0;
		[ProtoMember(9)] public int Ing_So01 = 0;
		[ProtoMember(10)] public int Ing_So02 = 0;
		[ProtoMember(11)] public int Ing_So03 = 0;
		[ProtoMember(12)] public int Ing_So04 = 0;
		[ProtoMember(13)] public int Ing_Fa01 = 0;
		[ProtoMember(14)] public int Ing_Fa02 = 0;
		[ProtoMember(15)] public int Ing_Fa03 = 0;
		[ProtoMember(16)] public int Ing_Fa04 = 0;
		
		public void Reset()
		{
			Ing_Sw01 = 0;
			Ing_Sw02 = 0;
			Ing_Sw03 = 0;
			Ing_Sw04 = 0;
			Ing_Fr01 = 0;
			Ing_Fr02 = 0;
			Ing_Fr03 = 0;
			Ing_Fr04 = 0;
			Ing_So01 = 0;
			Ing_So02 = 0;
			Ing_So03 = 0;
			Ing_So04 = 0;
			Ing_Fa01 = 0;
			Ing_Fa02 = 0;
			Ing_Fa03 = 0;
			Ing_Fa04 = 0;
		}
	}
	*/

	[ProtoContract]
    public class GameData : ContextHolder<GameData.Record> 
	{
        [ProtoMember(1), JsonMember] public string  key;
        //[ProtoMember(2), JsonMember] public int     total_level;
        public const long       TICK_DETAIL     = 10000000;
        public const int        MAX_STAMINA     = 15;           // 소유 가능한 최대 라이프 개수.
        public const int        CHARGEABLE_MAX_STAMINA   = 5;   // 자동 충전되는 최대 라이프 개수. (이 개수 까지만 시간으로 충전된다.)
        public const int        CHARGE_MIN      = 15;           // 자동 충전되는 시간 = 분.
        public const long       CHARGE_TIME     = CHARGE_MIN * 60 * TICK_DETAIL;    // 자동 충전되는 시간 - UNITY 시간 포맷.

        public class Record
		{
            // record data.
			public PuzzlePlayerGameBaseData     playerData = new PuzzlePlayerGameBaseData();

            // 'LVs{0}' - for firebase key.
            public LevelResultData[]            LevelResultDataList= null;             // 레벨간 누적 clear 정보 보관 list. => level clear 여부 !!!        
			public PuzzlePlayerWalletInventoryData WalletInventoryData = new PuzzlePlayerWalletInventoryData();
            public Tutorial[]   TutorialInfo    = null;

            // 'RMs{0}' - for firebase key.
            public int[] RewardedMissionIds     = null;

            // 'PCs{0}' - for firebase key.
            public Purchaseinfo[]               PurchaseInfoList = null;




            //public byte ucReciveDailyGiftNum    = 0;
            //public byte ucSendDailyGiftNum      = 0;
            //public byte ucAskDailyGiftNum       = 0;

            //public string strKey                = "";
            //public int ucCountBuff              = 0;

            //public int CurrentThemeIndex;
            //public PuzzlePlayerWalletIngrediantData WalletIngrediantData = new PuzzlePlayerWalletIngrediantData();
            //public FBFriendInfo[] FacebookFriendInfo = null;
            //public PuzzlePlayerGameResultData PlayNowGameResultData = null; // 현재 level play 상태를 보관하는 버퍼.
            //public PuzzlePlayerGameResultData[] GameResultDataList= null;   // 게임 결과를 서버로 전송하기 위해 잠시 쌓아놓는 버퍼.    
        }
		
		public void Reset()
		{
			this.record.WalletInventoryData.Reset();
            this.record.LevelResultDataList = null;
            this.record.RewardedMissionIds  = null;
            this.record.playerData.Clear();
			this.record.TutorialInfo = null;
            this.record.PurchaseInfoList = null;
        }

        #region => PurchaseInfo.
        public void AddPurchaseInfo(Purchaseinfo data, bool bAutoSave = true)
        {
            if (data == null)
                return;

            Purchaseinfo[] result;
            if(this.record.PurchaseInfoList != null)
            {
                result = new Purchaseinfo[this.record.PurchaseInfoList.Length + 1];
                this.record.PurchaseInfoList.CopyTo(result, 0);
                result[record.PurchaseInfoList.Length] = data;
            }
            else
            {
                result = new Purchaseinfo[1];
                result[0] = data;
            }

            this.record.PurchaseInfoList = result;      

            if(bAutoSave)
                SaveContext();
        }

        public int GetPurchaseInfoCount()
        {
            if (this.record.PurchaseInfoList == null)
                return 0;

            return this.record.PurchaseInfoList.Length; 
        }

        public Purchaseinfo GetPurchaseInfoByIndex(int index)
        {
            if (this.record.PurchaseInfoList == null || this.record.PurchaseInfoList.Length < 1 || this.record.PurchaseInfoList.Length < index)
                return null;
            
            return this.record.PurchaseInfoList[index]; 
        }

        public Purchaseinfo PopPurchaseInfo()
        {
            if (this.record.PurchaseInfoList == null || this.record.PurchaseInfoList.Length < 1 )
                return null;

            return this.record.PurchaseInfoList[0]; 
        }

        public void ConsumePurchas(Purchaseinfo data, bool bAutoSave = true)
        {
            if (this.record.PurchaseInfoList == null || this.record.PurchaseInfoList.Length < 1)
                return;

            for (int i = 0; i < this.record.PurchaseInfoList.Length; ++i)
            {
                if (this.record.PurchaseInfoList[i] == data)
                {
                    if (this.record.PurchaseInfoList.Length > 1)
                    {
                        Purchaseinfo[] result = new Purchaseinfo[this.record.PurchaseInfoList.Length - 1];
                        for (int n = 0; n < i; ++n)
                        {
                            result[n] = this.record.PurchaseInfoList[n];
                        }

                        for (int n = i + 1; n < this.record.PurchaseInfoList.Length; ++n)
                        {
                            result[n - (i + 1)] = this.record.PurchaseInfoList[n];
                        }

                        this.record.PurchaseInfoList = result;

                        if (bAutoSave)
                            SaveContext();

                        return;
                    }
                    else
                    {
                        this.record.PurchaseInfoList = null;

                        if(bAutoSave)
                            SaveContext();

                        return;
                    }
                }
            }                
        }
        #endregion

        #region => Animal Buffs
        public int getCountBuff()
        {
            return record.playerData.m_siCountBuff;
        }
        public void addBuffCount()
        {
            int nCount          = record.playerData.m_siCountBuff;
            ++nCount;
            setBuffCount( nCount );
        }
        public void setBuffCount(int count)
        {
            record.playerData.m_siCountBuff = count;
            SaveContext();

    //        FireBaseHandler.update("playerData/m_siCountBuff", count);
        }
        #endregion

        #region => Mission Rewarded List.
        public void addRewardedMission(int idMission)
        {
            if(isMissionRewarded(idMission))
                return;

            if(null == record.RewardedMissionIds)
            {
                record.RewardedMissionIds   = new int[1];
                record.RewardedMissionIds[0]= idMission;
            }
            else
            {
                int[] arrTemp   = new int[record.RewardedMissionIds.Length+1];
                record.RewardedMissionIds.CopyTo(arrTemp, 0);
                arrTemp[ record.RewardedMissionIds.Length ] = idMission;

                record.RewardedMissionIds   = arrTemp;
            }
            SaveContext();
        }
        public bool isMissionRewarded(int idMission)
        {
            if(null == record.RewardedMissionIds)
                return false;

            for(int z = 0; z < record.RewardedMissionIds.Length; ++z)
            {
                if(record.RewardedMissionIds[z] == idMission)
                    return true;
            }
            return false;
        }
        #endregion

        /*
        public void AddGameUsedLPuzzleItem(PuzzleItem data, bool bAutoSave = true)
        {
            if (data == null || this.record.PlayNowGameResultData == null)
                return;

            for (int i = 0; i < this.record.PlayNowGameResultData.stGameUsedLPuzzleItemList.Length; ++i)
            {
                if (this.record.PlayNowGameResultData.stGameUsedLPuzzleItemList[i].siUniqueNumber == data.siUniqueNumber)
                {
                    this.record.PlayNowGameResultData.stGameUsedLPuzzleItemList[i].ssCount += data.ssCount;
                    this.record.PlayNowGameResultData.stTotalUsedLPuzzleItemList[i].ssCount += data.ssCount;
                    if (bAutoSave)
                        SaveContext();

                    break;
                }
            }
        }

        public void AddTotalUsedPlayPuzzleItem(PuzzleItem data, bool bAutoSave = true)
        {
            if (data == null || this.record.PlayNowGameResultData == null)
                return;

            for (int i = 0; i < this.record.PlayNowGameResultData.stTotalUsedLPuzzleItemList.Length; ++i)
            {
                if (this.record.PlayNowGameResultData.stTotalUsedLPuzzleItemList[i].siUniqueNumber == data.siUniqueNumber)
                {
                    this.record.PlayNowGameResultData.stTotalUsedLPuzzleItemList[i].ssCount += data.ssCount;
                    if (bAutoSave)
                        SaveContext();
                    
                    break;
                }
            }
        }

        public PuzzlePlayerGameResultData GetPlayNowGameResultData()
        {
            return this.record.PlayNowGameResultData;
        }

        public void SetPlayNowGameResultData(PuzzlePlayerGameResultData data)
        {
            this.record.PlayNowGameResultData = data;
        }
            
       
		
		
		
		
		
        /*
		public string GetFacebookUID()
		{
			return this.record.playerData.FacebookUID;
		}
		
		public void SetFacebookUID(string ID,bool bAutoSave = true)
		{
			this.record.playerData.FacebookUID = ID;
			if(bAutoSave)
				SaveContext();
		}
		
		public void SetDataType(System.Byte type)
		{
			if(type == 0)
				this.record.playerData.DataSet = "A";
			else if(type == 1)
				this.record.playerData.DataSet = "B";
			else if(type == 2)
				this.record.playerData.DataSet = "C";
		}*/
		
        public int getTutorialStatusByID(string id)
		{
			if(this.record.TutorialInfo != null)
			{
				for(uint i = 0; i < this.record.TutorialInfo.Length; ++i)
				{
					if(this.record.TutorialInfo[i].strID == id)
						return this.record.TutorialInfo[i].nStatus;
				}	
			}
			return 0;
		}
        public Tutorial GetTutorialInfoByID(string id)
		{
			if(this.record.TutorialInfo != null)
			{
				for(uint i = 0; i < this.record.TutorialInfo.Length; ++i)
				{
					if(this.record.TutorialInfo[i].strID == id)
						return this.record.TutorialInfo[i];
				}	
			}
			
			return null;
		}
        public bool HasTutorialInfo(string id)
		{
			if(this.record.TutorialInfo != null)
			{
				for(uint i = 0; i < this.record.TutorialInfo.Length; ++i)
				{
					if(this.record.TutorialInfo[i].strID == id)
						return true;
				}	
			}
			return false;
		}
        public void AddTutorialInfo(Tutorial info, bool bAutoSave = true)
		{
			if(HasTutorialInfo(info.strID))
			{
				Tutorial temp = GetTutorialInfoByID(info.strID);
				temp.nStatus = info.nStatus;
			}
			else
			{
				Tutorial[] result;
				if(this.record.TutorialInfo != null)
				{
					result = new Tutorial[this.record.TutorialInfo.Length + 1];
					this.record.TutorialInfo.CopyTo(result, 0);
					result[record.TutorialInfo.Length] = info;
				}
				else
				{
					result = new Tutorial[1];
					result[0] = info;
				}

				this.record.TutorialInfo = result;		
			}
			
			if(bAutoSave)
				SaveContext();
		}

        // this value represents servers's stamina last use time.
		void SetStaminaChargeTick(long dt)
		{
            if(this.record.playerData.StaminaChargeTick != dt)
            {
                this.record.playerData.StaminaChargeTick = dt;

       //         FireBaseHandler.update("playerData/m_StaminaChargeTick", dt);
            }
		}

        public long GetStaminaChargeTick()
		{
			return this.record.playerData.StaminaChargeTick;
		}

		public long GetAdsRewardedTick()
		{
			return this.record.playerData.AdsRewardedTick;
		}

        public void SetAdsRewardedTick(long dt)
		{
			this.record.playerData.AdsRewardedTick = dt;
		}

        public void SetLastFreeGachaDate(long dt)
		{
			this.record.playerData.FreeGachaDate = dt;
		}

		public long GetLastFreeGachaDate()
		{
			return this.record.playerData.FreeGachaDate;
		}
		
		public void SetDailyRewardDate(long dt)
		{
            this.record.playerData.DailyRewardDate = dt;
		}
		
		public void SetDailyRewardedCount(int count)
		{
			this.record.playerData.DailyRewardedCount = count;
		}
	
		public int GetDailyRewardedCount()
		{
			return this.record.playerData.DailyRewardedCount;
		}
		
		public long GetDailyRewardDate()
		{
            return this.record.playerData.DailyRewardDate;
		}
		
		public bool setWalletItemDataByID(string ID, int count, bool bAutoSave = true)
		{
			bool val = false;
			ITEM_TYPE itemType = (ITEM_TYPE)System.Enum.Parse(typeof(ITEM_TYPE), ID.ToUpper());
			switch(itemType)
			{
				case ITEM_TYPE.COIN:
					//val = this.record.WalletInventoryData.Coin < count;
					//if(val)
						this.record.WalletInventoryData.Coin = count;
				break;
				case ITEM_TYPE.LIFE:
					//val = this.record.WalletInventoryData.Life < count;
					//if(val)
						this.record.WalletInventoryData.Life = count;
				break;
				case ITEM_TYPE.HAMMER:
					//val =this.record.WalletInventoryData.Hammer < count;
					//if(val)
						this.record.WalletInventoryData.Hammer = count;
				break;
				case ITEM_TYPE.FIRECRACKER:
					//val = this.record.WalletInventoryData.FireCracker < count;
					//if(val)
						this.record.WalletInventoryData.FireCracker = count;
				break;
				case ITEM_TYPE.MAGICSWAP:
					//val = this.record.WalletInventoryData.MagicSwap < count;
					//if(val)
						this.record.WalletInventoryData.MagicSwap = count;
				break;
				case ITEM_TYPE.MAGICSHUFFLE:
					//val = this.record.WalletInventoryDa//ta.RainbowBust < count;
					//if(val)
						this.record.WalletInventoryData.MagicShuffle = count;
				break;
				case ITEM_TYPE.MORETURNBOOSTER:
					//val = this.record.WalletInventoryData.MixedBooster < count;
					//if(val)
						this.record.WalletInventoryData._5MoreTurnBooster = count;
				break;
				case ITEM_TYPE.HBOMBBOOSTER:
					//val = this.record.WalletInventoryData.TripleBooster < count;
					//if(val)
						this.record.WalletInventoryData.HBombBooster = count;
				break;
				case ITEM_TYPE.VBOMBBOOSTER:
					//val = this.record.WalletInventoryData.SpecialBooster < count;
					//if(val)
						this.record.WalletInventoryData.VBombBooster = count;
				break;
                case ITEM_TYPE.BOMBBOOSTER:
						this.record.WalletInventoryData.BombBooster = count;
				break;
                case ITEM_TYPE.RAINBOWBOOSTER:
						this.record.WalletInventoryData.RainbowBooster = count;
				break;
			}
			if(bAutoSave)
				SaveContext();

			return val;
		}
		
        public void unlockItem(string ID,bool bAutoSave = true)
        {
            /*
            if (ID.Contains("excoupon_set") == true)
                return; 
                
            ITEM_TYPE itemType = (ITEM_TYPE)System.Enum.Parse(typeof(ITEM_TYPE), ID.ToUpper());
			switch(itemType)
			{
            case ITEM_TYPE.FIRECRACKER:     record.playerData.FirecrackerUnlocked = true;     break;
            case ITEM_TYPE.HAMMER:          record.playerData.HammerUnlocked  = true;         break;
            case ITEM_TYPE.MAGICSWAP:       record.playerData.MagicSwapUnlocked = true;       break;
            case ITEM_TYPE.RAINBOWBUST:     record.playerData.RainbowBustUnlocked = true;     break;
            case ITEM_TYPE.MIXEDBOOSTER:    record.playerData.MixedBoosterUnlocked=true;      break;
            case ITEM_TYPE.TRIPLEBOOSTER:   record.playerData.TripleBoosterUnlocked= true;    break;
            case ITEM_TYPE.SPECIALBOOSTER:  record.playerData.SpecialBoosterUnlocked = true;  break;
            }
            */
            //if(bAutoSave)
            //    SaveContext();
        }

		public void AddWalletItemDataByID(string ID, int count, bool bAutoSave = true)
		{
            if(null==ID || ID=="")
                return;

            string strKey       = "";
            int dataCount       = 0;

			ITEM_TYPE itemType = (ITEM_TYPE)System.Enum.Parse(typeof(ITEM_TYPE), ID.ToUpper());
			switch(itemType)
			{
				case ITEM_TYPE.COIN:
					this.record.WalletInventoryData.Coin += count;
					if(this.record.WalletInventoryData.Coin < 0)
						this.record.WalletInventoryData.Coin = 0;
                    strKey      = "Coin";
                    dataCount   = this.record.WalletInventoryData.Coin;
				break;
				case ITEM_TYPE.LIFE:
					this.record.WalletInventoryData.Life += count;
					if(this.record.WalletInventoryData.Life < 0)
						this.record.WalletInventoryData.Life = 0;

                    if(count<0 && this.record.WalletInventoryData.Life==CHARGEABLE_MAX_STAMINA-1)
                        _setStamina(this.record.WalletInventoryData.Life, DateTime.UtcNow.Ticks);

                    //updateStamina( DateTime.UtcNow.Ticks );
                    strKey      = "Life";
                    dataCount   = this.record.WalletInventoryData.Life;
				break;
				case ITEM_TYPE.HAMMER:
					this.record.WalletInventoryData.Hammer += count;
					if(this.record.WalletInventoryData.Hammer < 0)
						this.record.WalletInventoryData.Hammer = 0;
                    strKey      = "Hammer";
                    dataCount   = this.record.WalletInventoryData.Hammer;
				break;
				case ITEM_TYPE.FIRECRACKER:
					this.record.WalletInventoryData.FireCracker += count;
					if(this.record.WalletInventoryData.FireCracker < 0)
						this.record.WalletInventoryData.FireCracker = 0;
                    strKey      = "FireCracker";
                    dataCount   = this.record.WalletInventoryData.FireCracker;
				break;
				case ITEM_TYPE.MAGICSWAP:
					this.record.WalletInventoryData.MagicSwap += count;
					if(this.record.WalletInventoryData.MagicSwap < 0)
						this.record.WalletInventoryData.MagicSwap = 0;
                    strKey      = "MagicSwap";
                    dataCount   = this.record.WalletInventoryData.MagicSwap;
				break;
				case ITEM_TYPE.MAGICSHUFFLE:
					this.record.WalletInventoryData.MagicShuffle += count;
					if(this.record.WalletInventoryData.MagicShuffle < 0)
						this.record.WalletInventoryData.MagicShuffle = 0;
                    strKey      = "MagicShuffle";
                    dataCount   = this.record.WalletInventoryData.MagicShuffle;
				break;
                case ITEM_TYPE.MORETURNBOOSTER:
					this.record.WalletInventoryData._5MoreTurnBooster += count;
					if(this.record.WalletInventoryData._5MoreTurnBooster < 0)
						this.record.WalletInventoryData._5MoreTurnBooster = 0;
                    strKey      = "_5MoreTurnBooster";
                    dataCount   = this.record.WalletInventoryData._5MoreTurnBooster;
				break;
				case ITEM_TYPE.HBOMBBOOSTER:
					this.record.WalletInventoryData.HBombBooster += count;
					if(this.record.WalletInventoryData.HBombBooster < 0)
						this.record.WalletInventoryData.HBombBooster = 0;
                    strKey      = "HBombBooster";
                    dataCount   = this.record.WalletInventoryData.HBombBooster;
				break;
				case ITEM_TYPE.VBOMBBOOSTER:
					this.record.WalletInventoryData.VBombBooster += count;
					if(this.record.WalletInventoryData.VBombBooster < 0)
						this.record.WalletInventoryData.VBombBooster = 0;
                    strKey      = "VBombBooster";
                    dataCount   = this.record.WalletInventoryData.VBombBooster;
				break;
                case ITEM_TYPE.BOMBBOOSTER:
					this.record.WalletInventoryData.BombBooster += count;
					if(this.record.WalletInventoryData.BombBooster < 0)
						this.record.WalletInventoryData.BombBooster = 0;
                    strKey      = "BombBooster";
                    dataCount   = this.record.WalletInventoryData.BombBooster;
				break;
                case ITEM_TYPE.RAINBOWBOOSTER:
					this.record.WalletInventoryData.RainbowBooster += count;
					if(this.record.WalletInventoryData.RainbowBooster < 0)
						this.record.WalletInventoryData.RainbowBooster = 0;
                    strKey      = "RainbowBooster";
                    dataCount   = this.record.WalletInventoryData.RainbowBooster;
				break;
			}
			if(bAutoSave && strKey.Length>0)
            {
                SaveContext();
          //      FireBaseHandler.update("WalletInventoryData/"+strKey, dataCount);
            }
		}
		
		public void ChangeWalletItemDataByID(string ID, int count, bool bAutoSave = true)
		{			
			if(count < 0)
				return;
			
			ITEM_TYPE itemType = (ITEM_TYPE)System.Enum.Parse(typeof(ITEM_TYPE), ID.ToUpper());
			switch(itemType)
			{
				case ITEM_TYPE.COIN:
					this.record.WalletInventoryData.Coin = count;
				break;
				case ITEM_TYPE.LIFE:
					this.record.WalletInventoryData.Life = count;
				break;
				case ITEM_TYPE.HAMMER:
					this.record.WalletInventoryData.Hammer = count;
				break;
				case ITEM_TYPE.FIRECRACKER:
					this.record.WalletInventoryData.FireCracker = count;
				break;
				case ITEM_TYPE.MAGICSWAP:
					this.record.WalletInventoryData.MagicSwap = count;
				break;
				case ITEM_TYPE.MAGICSHUFFLE:
					this.record.WalletInventoryData.MagicShuffle = count;
				break;
				case ITEM_TYPE.MORETURNBOOSTER:
					this.record.WalletInventoryData._5MoreTurnBooster = count;
				break;
				case ITEM_TYPE.HBOMBBOOSTER:
					this.record.WalletInventoryData.HBombBooster = count;
				break;
				case ITEM_TYPE.VBOMBBOOSTER:
					this.record.WalletInventoryData.VBombBooster = count;
				break;
                case ITEM_TYPE.RAINBOWBOOSTER:
					this.record.WalletInventoryData.RainbowBooster = count;
				break;
			}
			if(bAutoSave)
				SaveContext();
		}
		
        void _setStamina(int count, long usedTick)
        {
            int nStamina        = Math.Min(MAX_STAMINA, count);
            if(record.WalletInventoryData.Life != nStamina)
                record.WalletInventoryData.Life = nStamina;
            //record.playerData.StaminaChargeTick = usedTick;
            //record.playerData.StaminaChargeTick
            this.SetStaminaChargeTick( usedTick );
        }

        void updateStamina(long ticks)
        {
            // 충전할 필요 없으면 return.
            int nCurStamina         = this.record.WalletInventoryData.Life;
            if(nCurStamina >= CHARGEABLE_MAX_STAMINA)
            {
                if(nCurStamina >= MAX_STAMINA)
                    _setStamina(MAX_STAMINA, 0);
                return;
            }
        
            DateTime defaultTime    = new DateTime();
            DateTime lastUsedTime   = new DateTime( this.record.playerData.StaminaChargeTick );
            // 의미없는 시간 갱신이면 return.
            if(lastUsedTime.Ticks<1 || lastUsedTime.Equals(defaultTime))
            {
                AddWalletItemDataByID("life", CHARGEABLE_MAX_STAMINA-this.record.WalletInventoryData.Life);
                Debug.Log("updateStamina() ; life charged as full....");
                return;
            }


            long timeGap            = ticks - lastUsedTime.Ticks;
            long increaseStamina    = timeGap / CHARGE_TIME;

            // 충전 가능한 량보다 많아졌다.
            if((increaseStamina+nCurStamina) >= CHARGEABLE_MAX_STAMINA)
            {
                // 이미 더 많이 가지고 있었으면, 그냥 그 상태로 set.
                if(nCurStamina >= CHARGEABLE_MAX_STAMINA)
                    _setStamina(nCurStamina, 0);
                // 그것보다 적었으면, 최대 충전 가능량으로 set.
                else 
                    _setStamina(CHARGEABLE_MAX_STAMINA, 0);
            }
            // 충전 가능량 보다 적다.
            else
            {
                // 증가량이 없으면 return.
			    if(1 > increaseStamina)
				    return;

                // 시간 갱신.
                long tick           = lastUsedTime.Ticks + ((increaseStamina) * CHARGE_TIME);
                _setStamina(nCurStamina+(int)increaseStamina, tick);

                //record.WalletInventoryData.Life += (int)increaseStamina;
                //record.playerData.StaminaChargeTick  = new DateTime(tick);
            }
        }

		public int GetCountWalletItemDataByID(string ID)
		{			
			ITEM_TYPE itemType = (ITEM_TYPE)System.Enum.Parse(typeof(ITEM_TYPE), ID.ToUpper());
			switch(itemType)
			{
				case ITEM_TYPE.COIN:
					return this.record.WalletInventoryData.Coin;
				case ITEM_TYPE.LIFE:
                {
                    updateStamina( DateTime.UtcNow.Ticks );
                    return this.record.WalletInventoryData.Life;
                }
				case ITEM_TYPE.HAMMER:
					return this.record.WalletInventoryData.Hammer;
				case ITEM_TYPE.FIRECRACKER:
					return this.record.WalletInventoryData.FireCracker;
				case ITEM_TYPE.MAGICSWAP:
					return this.record.WalletInventoryData.MagicSwap;
				case ITEM_TYPE.MAGICSHUFFLE:
					return this.record.WalletInventoryData.MagicShuffle;
				case ITEM_TYPE.MORETURNBOOSTER:
					return this.record.WalletInventoryData._5MoreTurnBooster;
				case ITEM_TYPE.HBOMBBOOSTER:
					return this.record.WalletInventoryData.HBombBooster;
				case ITEM_TYPE.VBOMBBOOSTER:
					return this.record.WalletInventoryData.VBombBooster;
                case ITEM_TYPE.BOMBBOOSTER:
					return this.record.WalletInventoryData.BombBooster;
                case ITEM_TYPE.RAINBOWBOOSTER:
					return this.record.WalletInventoryData.RainbowBooster;
			}

			return -1;
		}
		
        /*
        public void clearFBFriendInfoData()
        {
            record.FacebookFriendInfo   = null;
        }

		public void AddFBFriendInfoData(FBFriendInfo data, bool bAutoSave = true)
		{
			FBFriendInfo[] result;
			if(this.record.FacebookFriendInfo != null)
			{
				result = new FBFriendInfo[this.record.FacebookFriendInfo.Length + 1];
				this.record.FacebookFriendInfo.CopyTo(result, 0);
				result[record.FacebookFriendInfo.Length] = data;
			}
			else
			{
				result = new FBFriendInfo[1];
				result[0] = data;
			}

			this.record.FacebookFriendInfo = result;
			if(bAutoSave)
				SaveContext();
		}
		
		public int CountFBFriendInfoData()
		{
			if(this.record.FacebookFriendInfo != null)
				return this.record.FacebookFriendInfo.Length;
			
			return 0;
		}
		
		public void RemoveFBFriendInfoDataByID(string FBID, bool bAutoSave = true)
		{
			if(this.record.FacebookFriendInfo != null)
			{
				if(this.record.FacebookFriendInfo.Length > 1)
				{
					FBFriendInfo[] result = new FBFriendInfo[this.record.FacebookFriendInfo.Length - 1];
					int index = 0;
					for(int i = 0; i < this.record.FacebookFriendInfo.Length; ++i)
					{
						if(this.record.FacebookFriendInfo[i].strFBUID != FBID)
						{
							result[index] = this.record.FacebookFriendInfo[i];
							++index;
						}
					}
					
					this.record.FacebookFriendInfo = result;
					if(bAutoSave)
						SaveContext();
				}
				else
				{
					if(this.record.FacebookFriendInfo[0].strFBUID == FBID)
					{
						this.record.FacebookFriendInfo = null;
						if(bAutoSave)
							SaveContext();
					}
				}
			}
		}
		
		public void RemoveFBFriendInfoDataByIndex(int index, bool bAutoSave = true)
		{
			if(this.record.FacebookFriendInfo != null)
			{
				if(this.record.FacebookFriendInfo.Length > 1)
				{
					FBFriendInfo[] result = new FBFriendInfo[this.record.FacebookFriendInfo.Length - 1];
					int n = 0;
					for(int i = 0; i < this.record.FacebookFriendInfo.Length; ++i)
					{
						if(i != index)
						{
							result[n] = this.record.FacebookFriendInfo[i];
							++n;
						}
					}

					this.record.FacebookFriendInfo = result;
					if(bAutoSave)
						SaveContext();
				}
				else
				{
                    if(index != 0) return;

                    this.record.FacebookFriendInfo = null;
                    if(bAutoSave)
                        SaveContext();

                    return;
				}
			}
		}
		
		public FBFriendInfo GetFBFriendInfoByIndex(int index)
		{
			if(this.record.FacebookFriendInfo != null && this.record.FacebookFriendInfo.Length > index && index > -1)
				return this.record.FacebookFriendInfo[index];

			return null;
		}
		
		public FBFriendInfo GetFBFriendInfoByID(string FBID)
		{
			if(this.record.FacebookFriendInfo != null)
			{
				for(int i = 0; i < this.record.FacebookFriendInfo.Length; ++i)
				{
					if(this.record.FacebookFriendInfo[i].strFBUID == FBID)
						return this.record.FacebookFriendInfo[i];
				}	
			}
			
			return null;
		}*/
		
        public void ClearLevelResultData()
        {
            record.LevelResultDataList  = null;
        }

		public void AddLevelResultData(LevelResultData data, bool bAutoSave = true)
		{
			LevelResultData[] result;
			if(this.record.LevelResultDataList != null)
			{
				result = new LevelResultData[this.record.LevelResultDataList.Length + 1];
				this.record.LevelResultDataList.CopyTo(result, 0);
				result[record.LevelResultDataList.Length] = data;
			}
			else
			{
				result = new LevelResultData[1];
				result[0] = data;
			}

			this.record.LevelResultDataList = result;
			if(bAutoSave)
				SaveContext();
		}
		
        // Just Add Fucntion.
        public void AddLevelResultDataFromFireBase(string strData)
        {
            string[] list       = strData.Split(',');
            for(int z = 0; z < list.Length; ++z)
            {
                LevelResultData result = new LevelResultData();
                result.bCleared = true;
                result.siStraightFailCount = 0;
                result.ucGrade  = byte.Parse( list[z] );
                result.sl64Score= 1000;
                
                AddLevelResultData(result);
            }
        }

        // Just Add Fucntion.
        public void SetRewardedMissionDataFromFireBase(string strData)
        {
            string[] list       = strData.Split(',');
            for(int z = 0; z < list.Length; ++z)
            {
                addRewardedMission( int.Parse( list[z] ));
            }
        }

		public bool GetClearLevelByIndex(int index)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
				return this.record.LevelResultDataList[index].bCleared;
			
			return false;
		}
		
		public System.Byte GetGradeLevelByIndex(int index)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
				return this.record.LevelResultDataList[index].ucGrade;

			return 0;
		}
		
		public System.Int64 GetScoreLevelByIndex(int index)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
				return this.record.LevelResultDataList[index].sl64Score;

			return 0;
		}
		
		public int GetStraightFailCountLevelByIndex(int index)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
				return this.record.LevelResultDataList[index].siStraightFailCount;

			return 0;
		}
		
		public LevelResultData GetLevelResultDataByIndex(int index)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
				return this.record.LevelResultDataList[index];
			
			return null;
		}
		
		public void SetLevelResultDataByIndex(int index, LevelResultData data, bool bAutoSave = true)
		{
			if(this.record.LevelResultDataList != null && this.record.LevelResultDataList.Length > index && index > -1)
			{
				this.record.LevelResultDataList[index].ucGrade = data.ucGrade;
				this.record.LevelResultDataList[index].sl64Score = data.sl64Score;
				this.record.LevelResultDataList[index].bCleared = data.bCleared;
				//this.record.LevelResultDataList[index].bReward = data.bReward;
				this.record.LevelResultDataList[index].siStraightFailCount = data.siStraightFailCount;
				if(bAutoSave)
					SaveContext();
			}
		}

        public void ClearGameResultData()
        {
            //this.record.GameResultDataList  = null;
            SaveContext();
        }

        /*public int GetGameResultDataCount()
		{
			if(this.record.GameResultDataList != null)
				return this.record.GameResultDataList.Length;
			
			return 0;
		}
		
		public PuzzlePlayerGameResultData GetGameResultData(int index)
		{
			if(this.record.GameResultDataList != null && this.record.GameResultDataList.Length > index && -1 < index)
				return this.record.GameResultDataList[index];
			
			return null;
		}
		
		public void RemoveGameResultDataByLastDateTime(System.DateTime dateTime)
		{
			if(this.record.GameResultDataList == null)
				return;
			
			List<PuzzlePlayerGameResultData> tmp = new List<PuzzlePlayerGameResultData>();
			
			for(int i = 0; i < this.record.GameResultDataList.Length; ++i)
			{
				if(dateTime < this.record.GameResultDataList[i].stLastGameResultUpdateDateTimeT)
					tmp.Add(this.record.GameResultDataList[i]);
			}
			
			if(tmp.Count > 0)
				this.record.GameResultDataList = tmp.ToArray();
			else
				this.record.GameResultDataList = null;
			
			SaveContext();
		}

        
        // PlayNowGameResultData를 사용하여 잠시동안 서버에 보낼 결과 데이터를 버퍼에 쌓는다.
        public void AddGameResultData( bool bAutoSave = true)
		{
            if (this.record.PlayNowGameResultData == null)
                return;
            
			PuzzlePlayerGameResultData[] result;
			if(this.record.GameResultDataList != null)
			{
				result = new PuzzlePlayerGameResultData[this.record.GameResultDataList.Length + 1];
				this.record.GameResultDataList.CopyTo(result, 0);
                result[this.record.GameResultDataList.Length] = this.record.PlayNowGameResultData;
			}
			else
			{
				result = new PuzzlePlayerGameResultData[1];
                result[0] = this.record.PlayNowGameResultData;
			}
			
			this.record.GameResultDataList = result;
            if(bAutoSave)
			    SaveContext();
		}
		*/

        public System.Int64 GetPlayerUniqueNumber()
		{
			//if(key != null)
			//	return System.Int64.Parse(key);
			
			return 0;
		}
		
		/*[JsonIgnore]
		public int CurrentTreasureIndex
		{
			get { return this.record.playerData.CurrentTreasureIndex; }

			set { 
				this.record.playerData.CurrentTreasureIndex = value; 
				SaveContext();
			}
		}*/
    }

	[ProtoContract]	
    public class Root 
	{
        [ProtoMember(1)] public GameData gameData;
		//[ProtoMember(2)] public Level[] levels;
		//[ProtoMember(2)] public int countClearedRescueMission;// treasureIndex;
        
		[JsonIgnore]
		public Level currentLevel;      // the level which im playing right now.
        [JsonIgnore]
        public int idxMaxClearedLevel   = -1; 
		//[JsonIgnore]
		//public int CurrentThemeIndex
		//{
		//	set{ gameData.record.CurrentThemeIndex = value;}
		//	get{ return gameData.record.CurrentThemeIndex;}
		//}
		
        public void reset()
        {
            gameData.Reset();
            currentLevel        = default(Level);
            idxMaxClearedLevel  = -1;
        }

		public bool GetTreasure()
		{
			//if(treasureIndex[gameData.CurrentTreasureIndex] == currentLevel.Index)
			//	return true;
			return false;
		}
		
		public int TotalLevelCount 
		{
			get {   return (int)BASIC_INFOS.COUNT_CHAPTER*(int)BASIC_INFOS.COUNT_LEVEL_PER_CHAPTER;    }
		}

		public Level GetLevelFromIndex(int idx) 
		{
            if(0>idx || TotalLevelCount<=idx )
                return null;

            //string fullpath     = string.Format("{0}/{1}.txt", Director.GetTextPath(), idx+1);
            //= Data.Level aLevel   = JsonFx.Json.JsonReader.Deserialize<Data.Level>(System.IO.File.ReadAllText(fullpath));
            string fullpath     = string.Format("{0}/{1}", Director.GetTextPath(), idx+1);
            TextAsset txtData   = Resources.Load(fullpath) as TextAsset;
            if(null==txtData)   return null;

            Data.Level aLevel   = JsonFx.Json.JsonReader.Deserialize<Data.Level>( txtData.text );
            aLevel.InitLevel(idx, this);
            return aLevel;
		}
		
		[JsonIgnore]
		public int TotalClearedLevelCount   // 0 ~ 
		{
			get {
                //if(null != currentLevel)
                //    return currentLevel.Index;
                return  idxMaxClearedLevel+1;
            }
		}
		
		//[JsonIgnore]
		//public int ClearedMaxLevelID        // 1 ~ , ( 0>=x , invalid.)
		//{
		//	get
         //   {   return idxMaxClearedLevel+1;   }
	//	}

        /*
        public void InitializeRoot()
        {
            // Reload-anyway.
            //-gameData.key        = strKey;    // load 하려는 context의 key setting.
            gameData.LoadContext();             // context re-load with key.

            // To Local time !!!
            //gameData.SetDailyRewardDate( gameData.GetDailyRewardDate().ToLocalTime() );
            //gameData.SetStaminaChargeTick( gameData.GetStaminaChargeTick().ToLocalTime() );
            //gameData.SetLastFreeGachaDate( gameData.GetLastFreeGachaDate().ToLocalTime() );

            this.initCurrentLevel();
        }*/

        public Level GetLevel (int index) 
		{
            return GetLevelFromIndex(index);
        }

        // todo remove
        public void initCurrentLevel()
        {
            int maxLevel        = TotalLevelCount;
            
            int current         = -1;
            for (int i = 0; i < maxLevel; i++) 
            {
                if(gameData.GetClearLevelByIndex(i))    
                    current     = i;
                else            break;
            }
            idxMaxClearedLevel  = current;
            //gameData.record.playerData.TotalClearedLevelCount = current+1;

            //gameData.CurrentTreasureIndex   = 0;
            current         = -1==current ? 0 : Math.Min(current+1, maxLevel);
            currentLevel    = GetLevelFromIndex(current);
        }

		/*public void SetCurrentLevel() 
		{
			if(levels == null)
				return;
			
			if (gameData.record.LevelResultDataList != null)
            {
                for (int i = 0; i < levels.Length; i++)
                {
                    if (!gameData.GetClearLevelByIndex(i))
                    {
                        if(i > 0 && levels[i].Index == 0)
                        {
                            gameData.CurrentTreasureIndex = treasureIndex.Length - 1;
                            currentLevel = levels[i - 1];
                        }
                        else
                        {
                            currentLevel = levels[i];            
                            int index = treasureIndex[gameData.CurrentTreasureIndex] - 1;
                            if (index < currentLevel.Index)
                            {
                                for (i = gameData.CurrentTreasureIndex; i < treasureIndex.Length; ++i)
                                {
                                    if (treasureIndex[i] - 1 >= currentLevel.Index)
                                    {
                                        gameData.CurrentTreasureIndex = i;
                                        return;
                                    }
                                }
                            }
                        }
                        return;
                    }
                }

                gameData.CurrentTreasureIndex = treasureIndex.Length - 1;
                currentLevel = levels[levels.Length - 1];
            }
            else
            {
                currentLevel = levels[0];
                gameData.CurrentTreasureIndex = 0;
            }
		}*/
    }
}

//#endif
