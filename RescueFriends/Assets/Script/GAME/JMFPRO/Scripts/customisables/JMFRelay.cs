using UnityEngine;
using System.Collections;

/// <summary>
/// JMF Relay static class. 
/// WARNING~! Do not call JMFRelay.onXXX(); explicitly... it is not meant to be called!
/// *** already called by fixed coding positions in GameManager. ***
/// </summary>


public static class JMFRelay {

	static GameManager GM { get { return JMFUtils.GM; } } // getter methods for gameManager reference

	public delegate void OnEvent ();
	public static OnEvent OnShuffle;
	public static OnEvent OnGameReady;
	public static OnEvent OnGameStart;
	public static OnEvent OnPlayerMove;
    public static OnEvent OnFinishDrop;
	public static OnEvent OnStartBonus;
	public static OnEvent OnBoardStable;
	public static OnEvent OnChangeRemainShade;

    public static OnEvent<bool> OnChangeAITurn;   // true - is mine. false - ai bot.

    public static OnEvent<string> OnChangeStaminaTime;
    public static OnEvent OnFinishChargingStaminaTime;
    public static OnEvent<bool> OnInfiniteStaminaTime;

	public static OnEvent<int> OnAlramRecip;
	public static OnEvent OnMessageBox;
	public static OnEvent<int> OnMessageBoxAlram;
    public static OnEvent<System.Int64> OnRemoveGiftMessage;
    public static OnEvent<byte> OnFriendBoxAlram;
	
	public delegate void OnEvent<T> (T param);
	public static OnEvent<int> OnCombo;
	public static OnEvent<int> OnFinishCombo;
	public static OnEvent<int> OnAchieveStar;
	public static OnEvent<long> OnChangeScore;
	public static OnEvent<Point> OnClickPanel;
	public static OnEvent<Point> OnClickPiece;
	public static OnEvent<int> OnChangeRemainMove;
	public static OnEvent<int, int> OnChangeRemainShadeForDisplay;
	public static OnEvent<float> OnChangeRemainTime;
	public static OnEvent<int> OnChangeRemainSnowman;
	public static OnEvent<int> OnChangeRemainPenguin;
	public static OnEvent<int> OnChangeRemainFairy;
	public static OnEvent<int> OnChangeRemainPenguinForDisplay;
	public static OnEvent<int> OnChangeRemainFairyForDisplay;
	public static OnEvent<Data.Level> OnClearedLevel;
	public static OnEvent<int> OnChangeYetiHealth;
	public static OnEvent<int> OnChangeYetiHealthForDisplay;
	public static OnEvent<int, int> OnChangeBossHealth;
	public static OnEvent<GamePiece> OnPieceDestroy;

	public delegate void OnEvent<T1, T2> (T1 x, T2 y);
	public static OnEvent<int, int> OnCollectJewel;
	public static OnEvent<int, int> OnCollectJewelForDisplay;
    public static OnEvent<int, int> OnCollectSugarJewelForDisplay;
	public static OnEvent<SJ_COMBINE_TYPE, int> OnCollectSpecialJewel;
	public static OnEvent<TREASURE_TYPE, int> OnCollectTreasure;
	public static OnEvent<TREASURE_TYPE, int> OnCollectTreasureForDisplay;
    public static OnEvent OnUpdateAiScoreUI;
    public static OnEvent REFRESH_VIEW;

    // network relay.
    //public static OnEvent ON_PACKET_WAIT_TIMEOUT;
    //public static OnEvent ON_PACKET_STOP_RETRY;

    public static OnEvent<bool> PACKET_RES_ITEM_BUY;
    public static OnEvent<bool> PACKET_RES_RECIPE;
    public static OnEvent<bool, int> ON_PACKET_RES_GAME_RESULT;
    public static OnEvent<bool> ON_PACKET_RES_LEVEL_ENTER_POPUP;
    public static OnEvent<bool> ON_PACKET_RES_OUT_OF_MOVE_POPUP;

    public static OnEvent ON_PACKET_RES_CANCEL;
    public static OnEvent ON_PACKET_RES_CLOSE_POPUP;
    public static OnEvent ON_NETWORK_ERR_POPUP_BTN_OK;
    
    // [ROUND_CHOCO]
    public static OnEvent<string, int> OnCollectMissionObjectForDisplay;

	public delegate int OnEvent<T1, T2, T3> (T1 x, T2 y, T3 z);
	public static OnEvent<bool, System.Byte, System.Int64> OnGameOver;
    public static OnEvent<int, int, Vector3> OnCollectPieceForAutoEvent;

	public static void FireOnGameStart () {
		if (OnGameStart != null) OnGameStart.Invoke();
	}

	public static void FireOnPlayerMove () {
		if (OnPlayerMove != null) OnPlayerMove.Invoke();
	}

    public static void FireOnFinishDrop () {
		if (OnFinishDrop != null) OnFinishDrop.Invoke();
	}

    public static void FireOnBoardStable () {
		if (OnBoardStable != null) OnBoardStable.Invoke();
	}

	public static void FireOnIncreaseCombo (int comboCount) {
		if (OnCombo != null) OnCombo.Invoke(comboCount);
	}

	public static void FireOnFinishCombo (int comboCount) {
		if (OnFinishCombo != null) OnFinishCombo.Invoke(comboCount);
	}

	public static void FireOnShuffle () {
		if(OnShuffle != null) OnShuffle.Invoke();
	}

	public static void FireOnClickPiece (Point pt) {
        if (GM[pt].IsFilled) GM[pt].PD.FireOnPieceClick(GM[pt].Piece);
        if (GM[pt].Panel != null) FireOnClickPanel(pt);
		if (OnClickPiece != null) OnClickPiece.Invoke(pt);
	}

	public static void FireOnClickPanel (Point pt) {
		GM[pt].Panel.PND.FireOnPanelClick(GM[pt].Panel);
		if (OnClickPanel != null) OnClickPanel.Invoke(pt);
	}

	public static void FireOnChangeScore (long score) {
	    if (OnChangeScore != null) 
			OnChangeScore.Invoke(score);
	}

    public static void FireOnChangeRemainShade () {
        if (OnChangeRemainShade != null) OnChangeRemainShade.Invoke();
    }

    public static void FireOnChangeAITurn (bool isMyTurn) {
        if (OnChangeAITurn != null) OnChangeAITurn.Invoke(isMyTurn);
    }

    public static void FireOnChangeRemainShadeForDisplay (int remainCount, int shadeType) {
        if (OnChangeRemainShadeForDisplay != null) OnChangeRemainShadeForDisplay.Invoke(remainCount, shadeType);
    }

    public static void onChangeRemainSnowman (int snowmanCount) {
        if (OnChangeRemainSnowman != null) OnChangeRemainSnowman.Invoke(snowmanCount);
    }

    public static void FireOnCollectJewel (int index, int remainCount) {
        if (OnCollectJewel != null) OnCollectJewel.Invoke(index, remainCount);
    }

    public static void FireOnCollectJewelForDisplay (int index, int remainCount) {
        if (OnCollectJewelForDisplay != null) OnCollectJewelForDisplay.Invoke(index, remainCount);
    }
    public static void FireOnCollectSugarJewelForDisplay (int index, int remainCount) {
        if (OnCollectSugarJewelForDisplay != null) OnCollectSugarJewelForDisplay.Invoke(index, remainCount);
    }

    // [ROUND_CHOCO]
    public static void FireOnCollectMissionObjectForDisplay (string strObjType, int remainCount) {
        if (OnCollectMissionObjectForDisplay != null) OnCollectMissionObjectForDisplay.Invoke(strObjType, remainCount);
    }

    public static void FireOnChangeRemainMove (int remainMove) {
        if (OnChangeRemainMove != null) OnChangeRemainMove.Invoke(remainMove);
    }

    public static void FireOnChangeRemainTime (float remainTime) {
	    if (OnChangeRemainTime != null) OnChangeRemainTime.Invoke(remainTime);
    }

    public static void FireOnCollectTreasure (TREASURE_TYPE type, int remainCount) {
        if (OnCollectTreasure != null) OnCollectTreasure.Invoke(type, remainCount);
    }

    public static void FireOnCollectTreasureForDisplay (TREASURE_TYPE type, int remainCount) {
        if (OnCollectTreasureForDisplay != null) OnCollectTreasureForDisplay.Invoke(type, remainCount);
    }

    public static void FireOnChangeRemainPenguin(int remainCount) {
        if (OnChangeRemainPenguin != null) OnChangeRemainPenguin.Invoke(remainCount);
    }

    public static void FireOnChangeRemainFairy(int remainCount) {
        if (OnChangeRemainFairy != null) OnChangeRemainFairy.Invoke(remainCount);
    }

    public static void FireOnChangeRemainPenguinForDisplay(int remainCount) {
        if (OnChangeRemainPenguinForDisplay != null) OnChangeRemainPenguinForDisplay.Invoke(remainCount);
    }

    public static void FireOnChangeRemainFairyForDisplay(int remainCount) {
        if (OnChangeRemainFairyForDisplay != null) OnChangeRemainFairyForDisplay.Invoke(remainCount);
    }

	public static void FireOnGameOver (bool isWin, System.Byte grade, int score) {
        if (OnGameOver != null) OnGameOver.Invoke(isWin, grade, score);
    }

    public static void FireOnGameReady () {
	    if (OnGameReady != null) OnGameReady.Invoke();
    }

    public static void FireOnStartBonus () {
	    if (OnStartBonus != null) OnStartBonus.Invoke();
    }

    public static void FireOnCollectSpecialJewel (SJ_COMBINE_TYPE combineType, int count) {
        if (OnCollectSpecialJewel != null) OnCollectSpecialJewel(combineType, count);
    }

	public static void FireOnClearedLevel (Data.Level level) {
		if (OnClearedLevel != null) 
			OnClearedLevel.Invoke(level);
	}

    public static void FireOnChangeYetiHealth (int health) {
	    if (OnChangeYetiHealth != null) OnChangeYetiHealth(health);
    }

    public static void FireOnChangeYetiHealthForDisplay (int health) {
	    if (OnChangeYetiHealthForDisplay != null) OnChangeYetiHealthForDisplay(health);
    }

    public static void FireOnChangeBossHealth (int health, int damage) {
	    if (OnChangeBossHealth != null) OnChangeBossHealth(health, damage);
    }

    public static void FireOnPieceDestroy (GamePiece gp) {
        if (OnPieceDestroy != null) OnPieceDestroy(gp);
    }

    public static int FireOnCollectPieceForAutoEvent (int colorIndex, int typeIndex, Vector3 vLocalPos) {
        if (OnCollectPieceForAutoEvent != null) return OnCollectPieceForAutoEvent(colorIndex, typeIndex, vLocalPos);

        return 0;
    }

    public static void FireOnChangeStaminaTime(string str) {
        if (OnChangeStaminaTime != null) OnChangeStaminaTime(str);
    }
    
	public static void FireOnFinishChargingStaminaTime() {
        if (OnFinishChargingStaminaTime != null) OnFinishChargingStaminaTime();
    }

    public static void FireOnInfiniteStaminaTime(bool active) {
        if (OnInfiniteStaminaTime != null) OnInfiniteStaminaTime(active);
    }
	
	public static void FireOnAlramRecip(int count)
	{
		if (OnAlramRecip != null) OnAlramRecip(count);
	}
	
	public static void FireOnMessageBox()
	{
		if (OnMessageBox != null) OnMessageBox();
	}
	
	public static void FireOnMessageBoxAlram(int count)
	{
		if (OnMessageBoxAlram != null) OnMessageBoxAlram(count);
	}

    public static void FireOnRemoveGiftMessage(System.Int64 id)
    {
        if (OnRemoveGiftMessage != null) OnRemoveGiftMessage(id);
    }

    public static void FireOnFriendBoxAlram(byte index)
    {
        if (OnFriendBoxAlram != null) OnFriendBoxAlram(index);
    }
        
}
