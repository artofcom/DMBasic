using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Diagnostics;
using Data;
using Spine.Unity;
using Spine;
//using Facebook.Unity;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

public enum GAME_ITEM { NONE = 2 , HAMMER, FIRECRACKER, MAGICSWAP, MAGICSHUFFLE }

public enum E_PUZZLE_GAMERESULT_FAIL_REASON 
      {
          E_PUZZLE_GAMERESULT_FAIL_REASON_NONE,
          E_PUZZLE_GAMERESULT_FAIL_REASON_FAIL,     // 그냥 실패
          E_PUZZLE_GAMERESULT_FAIL_REASON_GIVEUP,
          E_PUZZLE_GAMERESULT_FAIL_REASON_OUTOFMOVE,
          E_PUZZLE_GAMERESULT_FAIL_REASON_OUTOFMOVE_GIVEUP,
          E_PUZZLE_GAMERESULT_FAIL_REASON_ERROR,
      }

public class PlaySceneHandler : MonoBehaviour, IGameScene
{
	readonly string[] BGMList = {"bgm_play","bgm_play","bgm_play","bgm_play", "bgm_play", "bgm_play"};

    const float fCHAR_POP_IN_DELAY  = 2.6f;
	const int shadowAreaOrder       = 25;
	
   // public GameObject shadowArea;
	//public GameObject topMask;
    //public tk2dSprite background;
    //public SpriteRenderer _sprBGFilter;
    public tk2dSprite _sprDarkBG;
    public SpriteRenderer[] _arrMainBG  = null;
    public SpriteRenderer[] _arrMainCollectBG  = null;

    //public tk2dBaseSprite buttomBG;

    static Data.Level currentLevel;
    //int remainTimeOrMove;
    bool skipNetworkErrPopupBtnOk = false;
    //int currentSeed;
    //bool isWorldMove = false;
    long _curScore              = 0;

    Dictionary<string, List<int>> usedSeedDict = new Dictionary<string, List<int>>();

    GameManager GM { get { return JMFUtils.GM; } }

    public void DoDataExchange() { }

	public GameObject GetGameObject()
	{
		return gameObject;
	}
	
    public void ScreenShot() 
    {
        /*Scene.ShowOverlay("PlayOverlay");
        JMFRelay.FireOnGameReady();

        Vector3 borderPos = JMFUtils.GM[0,0].LocalPosition;

        borderPos.z = shadowAreaOrder;
       // shadowArea.transform.localPosition = borderPos;

        borderPos = topMask.transform.position;
        // note : 큰 size panel이나, warf effect 등을 포함하여 여유있게 masking을 건다.
        borderPos.y = JMFUtils.GM[0,GM.getTopBoardIndex()].Position.y +( 0.5f * JMFUtils.GM.Size) + 0.5f; // - 0.15f;
        topMask.transform.position = borderPos;
        */
    }

	public void OnEnterScene (object param) 
	{
        Debugger.Assert(param is Data.Level, "PlaySceneHandler.OnEnterScene : Param is wrong.");

        JMFRelay.OnGameOver += OnGameOver;
        //JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.ON_PACKET_RES_GAME_RESULT += OnPacketResGameResult;
        JMFRelay.ON_NETWORK_ERR_POPUP_BTN_OK += OnNetworkErrPopupBtnOk;
        JMFRelay.REFRESH_VIEW += refreshUI;

//        JMFRelay.OnStartBonus += OnStartBonus;
		
        ReadyGame(param as Data.Level);
		Scene.ShowOverlay("PlayOverlay");
		JMFRelay.FireOnGameReady();
		
        //float fDelay            = fCHAR_POP_IN_DELAY;
        //string tutorialID       = TutorialOverlayHandler.GetAvailable(currentLevel.Index+1);
        //if(null!=tutorialID)    fDelay -= 0.5f;
        //StartCoroutine( GM.popCharacter(fDelay, true) );

        DOVirtual.DelayedCall(0.5f, () =>
        {
            Scene.ShowPopup("MissionPopup", currentLevel, (param2) => {
                StartGame();
            });
        });
		
        for(int zz = 0; zz < _arrMainBG.Length; ++zz)
            _arrMainBG[zz].gameObject.SetActive( false );
        for(int zz = 0; zz < _arrMainCollectBG.Length; ++zz)
            _arrMainCollectBG[zz].gameObject.SetActive( false );
        int idxBG               = Mathf.Min(_arrMainBG.Length-1, (int)currentLevel.missionType);
        idxBG                   = Mathf.Max(0, idxBG);
        if(0 == idxBG)
            _arrMainCollectBG[ Random.Range(0, _arrMainCollectBG.Length) ].gameObject.SetActive( true );
        else
            _arrMainBG[idxBG-1].gameObject.SetActive( true );
        
        /*
        Vector3 borderPos = JMFUtils.GM[0,0].LocalPosition;        
		borderPos.z = shadowAreaOrder;
      //  shadowArea.transform.localPosition = borderPos;
		borderPos = topMask.transform.position;
        // note : 큰 size panel이나, warf effect 등을 포함하여 여유있게 masking을 건다.
		borderPos.y = JMFUtils.GM[0,GM.getTopBoardIndex()].Position.y +( 0.5f * JMFUtils.GM.Size) + 0.5f; // - 0.15f;
		topMask.transform.position = borderPos;
        */
        _curScore               = 0;

		if(NNSoundHelper.EnableBGM && NNSoundHelper.IsPlaying(BGMList[currentLevel.missionType]) == false)
			NNSoundHelper.PlayBGM(BGMList[currentLevel.missionType]);
    }

    public void SetOnNetworkErrPopupBtnOk(bool _on)
    {
        skipNetworkErrPopupBtnOk = _on;
    }

    public void OnLeaveScene () 
	{
        Scene.CloseOverlay("PlayOverlay");
        JMFRelay.OnGameOver -= OnGameOver;
        //JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.ON_PACKET_RES_GAME_RESULT -= OnPacketResGameResult;
        JMFRelay.ON_NETWORK_ERR_POPUP_BTN_OK -= OnNetworkErrPopupBtnOk;
        JMFRelay.REFRESH_VIEW -= refreshUI;

        //JMFRelay.OnStartBonus -= OnStartBonus;
        
        NNSoundHelper.StopBGM(0, false);
        
        JMFUtils.GM.CountinueCount = 0;

        TutorialOverlayHandler.Deactivate();
        if (Yeti.Current != null) Yeti.Current.Recycle();
    }
        
    void refreshUI()
    {
        // item buttons update. 
        JMFUtils.GM.playOverlayHandler.UpdateItems();

        // and ???
    }

	public void OnEscape () 
	{
        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return;

        // test.
        //this.ShowClearPopup(3, 3, 0, 5000, false);
        //return;
        //

        JMFUtils.GM.Pause();
        
        Scene.ShowPopup("PausePopup", null, (param) => {
            PAUSE_RESULT result = (PAUSE_RESULT)param;

            if (result == PAUSE_RESULT.RESUME)
			{
                JMFUtils.GM.Resume();
            }
            else if(result == PAUSE_RESULT.QUIT)    // to lobby.
            {
                if(JMFUtils.GM.Moves > 0)
                    Wallet.Use("life");
                WorldSceneHandler._paramInfo info = new WorldSceneHandler._paramInfo();
                info.idxCurrent     = currentLevel.Index;
                info.isFromInGame   = true;
                ChangeScene("WorldScene", (object)info);// "NONE");//currentLevel.Index);
            } 
            else if (result == PAUSE_RESULT.RESTART) 
			{
                Wallet.Use("life");
                if(Wallet.GetItemCount("life") <= 0)
                {
                    //MessagePopupHandler.Data data2 = new MessagePopupHandler.Data();
                    //data2.isOkOnly      = true;
                    //data2.strMessage    = "You don't have \nenough Hearts.";
                    //Scene.ShowPopup("MessagePopup", data2);

                    Scene.ShowPopup("BuyItemPopup", "life", (param2) =>
                    {
                        if(Wallet.GetItemCount("life") > 0)
                        {
                            //JMFUtils.GM.CountinueCount = 0;
                            //GM.EndGame(GAMEOVER_REASON.RESTART);
                            //IncreaseFailCount();
                           // ResetGame(currentLevel);
                           sRetryLevel(false);
                        }
                        else
                           JMFUtils.GM.Resume();                
                    });                                     
                }
                else
                    sRetryLevel(false);              
            }
        });
    }

    // 결과 패킷 만들때 필요한 play now data build.
    void _buildNowPlayingData(long score, byte grade)
    {
        /*Data.PuzzlePlayerGameResultData ResultData = Root.Data.gameData.GetPlayNowGameResultData();

        ResultData.siMapUniqueNumber = currentLevel.Index + 1;
        ResultData.sl64Score = score;
        ResultData.ucStar = (System.Byte)grade;
        ResultData.ucIsCleared = 1;
        string MapDropItem = InfoLoader.GetMapDropItemNameByID(currentLevel.Index);
        ResultData.siRewardItemUniqueNumber = NOVNINE.Wallet.ChangeItemUniqueNumberByID(MapDropItem);
        ResultData.ucRewardItemCount = (System.Byte)InfoLoader.GetMapDropItemCountByID(currentLevel.Index);
		
        //ResultData.stLastGameResultUpdateDateTimeT = LGameData.GetInstance().GetCurrentServerTime();
        ResultData.ucPuzzleGameResult_Fail_Reason = (byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_NONE;

        //ResultData.ssLeftMoveCounter = (System.Int16)(currentLevel.allowedMoves - JMFUtils.GM.Moves);                            // clear 시에는 clear 하고 남은 move 수 이고, fail 시에는 fail 시에 남은 move 수

        int currentTreasureIndex    = Root.Data.gameData.CurrentTreasureIndex;
        if(currentTreasureIndex<0 || currentTreasureIndex>=Root.Data.treasureIndex.Length)
            currentTreasureIndex    = 0;
        //

        // auto event blk-count.
        ResultData.siAutoEventBlockCount    = //10; // test 
                                              JMFUtils.GM.playOverlayHandler._countAutoEventBlk;

		if(Root.Data.treasureIndex[currentTreasureIndex] == currentLevel.Index +1)
		{
			_TREASURE_ITEM_INFO outInfo = new _TREASURE_ITEM_INFO();
			InfoLoader.GetTreasureByIndex( currentTreasureIndex, ref outInfo);
            			
            // 이 시점에 item은 무조건 수령하는 것으로 수령 처리 !
            if (outInfo.strItemName.Contains("excoupon_set") == false)
            {
                ResultData.siTreasureRewardItemUniqueNumber = NOVNINE.Wallet.ChangeItemUniqueNumberByID(outInfo.strItemName);
                ResultData.siTreasureRewardItemCount = outInfo.count;
            }
		}*/
    }

    int mResultPrevGrade        = -1;
    bool mResultIsHighScore     = false;
	int OnGameOver (bool isWin, System.Byte grade, System.Int64 score) 
	{
        _curScore               = score;
        
        StartCoroutine( _coOnGameOver(isWin, grade, score) );

        return 0;
    }

    IEnumerator _coOnGameOver(bool isWin, byte grade, long score)
    {
        while(JMFUtils.GM.playOverlayHandler.IsPlayingComboText())
            yield return null;

        if (isWin) 
		{
            Data.LevelResultData data = Root.Data.gameData.GetLevelResultDataByIndex(currentLevel.Index);
            mResultPrevGrade    = null==data ? 0 : (int)data.ucGrade;
            mResultIsHighScore  = null==data ? true : data.sl64Score<score;
            data                = _buildLevelResultData(currentLevel.Index, grade, score);

           // _buildNowPlayingData(score, grade);
#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent("level_clear", "sClearLvId", (currentLevel.Index+1).ToString());
#endif

           /* if(null != FireBaseHandler.getUserDB())
            {
                //FireBaseHandler.write( FireBaseHandler.getUserDB().Child("LVs"+currentLevel.Index), data, true, () =>
                int _100        = 100;
                int idxStart    = currentLevel.Index - currentLevel.Index%_100;
                string strLvKey = string.Format("LV{0}-{1}", idxStart+1, idxStart+_100);
                FireBaseHandler.update( strLvKey, InfoLoader.buildLVGradeData(idxStart), true, () =>
                {
                    ShowClearPopup(0, grade, null==data?_curScore:data.sl64Score, _curScore, true);
                });
            }
            else
                ShowClearPopup(0, grade, null==data?_curScore:data.sl64Score, _curScore, true);*/
        } 
		else 
		{
#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent("level_failed", "sFaildLvId", (currentLevel.Index+1).ToString());
#endif

//            if (JMFUtils.GM.CountinueCount < 3)
//				ShowOutOfMovePopup(score);
//            else
//			{
//                JMFUtils.GM.CountinueCount = 0;
//                ShowFailPopup(score,(byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_FAIL);
//            }

            
            // ==> Note : Do not write to the sever. since 2018.11.11
            //
            //LevelResultData data    = IncreaseFailCount();
            //if(null != FireBaseHandler.getUserDB())
            //{
            //    FireBaseHandler.write( FireBaseHandler.getUserDB().Child("LVs"+currentLevel.Index), data, true, () =>
            //    {
            //        ShowOutOfMovePopup(score);
            //    });
            //}
            //else 
            //    ShowOutOfMovePopup(score);

            IncreaseFailCount();
            ShowOutOfMovePopup(score);
        }
    }

    // 전체 진행도를 buffer에 쌓는다. 
    // note : 서버 데이터에 의해 매번 초기화 된다.
    LevelResultData _buildLevelResultData(int idxLevel, byte nowGrade, long nowScore)
    {
        // update data.
        LevelResultData         data   = Root.Data.gameData.GetLevelResultDataByIndex(idxLevel);
		bool bAdd               = (data == null);
		if(bAdd)                data    = new Data.LevelResultData();
			
		data.bCleared           = Root.Data.gameData.GetClearLevelByIndex(idxLevel);
		data.ucGrade            = Root.Data.gameData.GetGradeLevelByIndex(idxLevel);
		//data.bReward            = Root.Data.gameData.GetRewardLevelByIndex(idxLevel);
		data.sl64Score          = Root.Data.gameData.GetScoreLevelByIndex(idxLevel);

		//int prevGrade = data.ucGrade;
		data.bCleared           = true;
			
        //if (data.bReward == false && nowGrade == 3)
        //{
            //WorldSceneHandler.IsMapClearReward = true;
        //    data.bReward = true;
        //}
		if (nowGrade > data.ucGrade) 
			data.ucGrade        = nowGrade;
                
		//bool highScore = false;
		if (nowScore > data.sl64Score)
		{
			data.sl64Score      = nowScore;
			//highScore = true;
		}

		//if (Root.Data.TotalClearedLevelCount > Root.Data.idxMaxClearedLevel.gameData.record.playerData.TotalClearedLevelCount)
		//	Root.Data.gameData.record.playerData.TotalClearedLevelCount = Root.Data.TotalClearedLevelCount;

		data.siStraightFailCount = 0;

        //Root.Data.gameData.StraightWinCount++;

        if (bAdd)
            Root.Data.gameData.AddLevelResultData(data);
        else
            Root.Data.gameData.SetLevelResultDataByIndex(idxLevel, data);
        
        if(null != JMFUtils.GM)
            JMFUtils.GM.CountinueCount = 0;

        return data;
    }

//    void OnGameReady () 
//	{
//        //didUseItem = false;
//        //Repositioning();
//    }

//    void OnStartBonus () 
//	{
//        if (GM.CurrentLevel.isTimerGame)
//            remainTimeOrMove = (int)Mathf.Max(0, GM.CurrentLevel.givenTime - GM.PlayTime);
//        else
//            remainTimeOrMove = Mathf.Max(0, GM.CurrentLevel.allowedMoves - GM.Moves);
//    }

    void OnEnableFrogJump (bool enable)
	{
        Scene.SendMessage<PlayOverlayHandler>("ShowBoardMask", enable);
    }

	void ShowClearPopup (int prevGrade, int grade, long bestscore, long cur_score, bool bHighScore) 
	{
        ClearInfo info = new ClearInfo();
        info.level = currentLevel;
        info.prevGrade = prevGrade;
        info.currGrade = grade;
        info.score = bestscore;
		info.highScore = bHighScore;
        info.cur_score  = cur_score;
		
        //
        /*
        int currentTreasureIndex= 0;
#if DEV_MODE
        for(int z = 0; z < Root.Data.treasureIndex.Length; ++z)
        {
            if(Root.Data.treasureIndex[z] == currentLevel.Index+1)
            {
                currentTreasureIndex    = z;
                break;
            }
        }
#else
        //currentTreasureIndex    = Root.Data.gameData.CurrentTreasureIndex;
#endif
        //if(currentTreasureIndex<0 || currentTreasureIndex>=Root.Data.treasureIndex.Length)
        //    currentTreasureIndex    = 0;
        */

        //-Root.Data.gameData.record.PlayNowGameResultData = null;

        Scene.ShowPopup("ClearPopup", info, ClearCallback);
    }

    IEnumerator ShowRewardItemPopup(ClearInfo info, FBUser user)
	{
        yield break;
        /*
        //-Scene.ShowPopup("ClearPopup", info, ClearCallback);   
        //
		float durtion = JMFUtils.GM.playOverlayHandler.scoreProgress.PlayIngredientSprite("hide", false);
		
		yield return new WaitForSeconds(durtion);
		
		BasePopupHandler popup = Scene.GetPopup("GetItemPopup");
		if(popup != null)
		{
            //string tutorialID       = TutorialOverlayHandler.GetAvailable(currentLevel.Index+1, "GetItemPopup");

			GetItemInfo itemInfo = new GetItemInfo();
			itemInfo.Id = InfoLoader.GetMapDropItemNameByID(info.level.Index);
			itemInfo.Count = 1;
			itemInfo.IsRecip = true;
            itemInfo.isOnTutorial   = tutorialID!=null;
			
			Scene.ShowPopup("GetItemPopup", itemInfo);
			durtion = popup.GetAnimationDuration("show")*0.5f;
            yield return new WaitForSeconds(durtion);

            if(tutorialID != null)
            {
                TutorialOverlayHandler.Activate(tutorialID);
                GetItemPopupHandler itemPopupo = Scene.CurrentPopup() as GetItemPopupHandler;
                yield return StartCoroutine( itemPopupo.processOnTutorial() );
            }

            while(null != Scene.CurrentPopup())
                yield return null;
		}
		
        yield return new WaitForSeconds(0.05f);
		
        //StartCoroutine( GM.popCharacter(.0f, false) );
        //Ranking
        if (user != null)
        {
        /*    RankingInfo ranking = new RankingInfo();
            ranking.FriendID = user.ID;
            ranking.MeScore = info.score;
            Scene.ShowPopup("RankingPopup", ranking, (param) =>
                {
                    //-Scene.ShowPopup("ClearPopup", info, ClearCallback);
                });   
        }
        else
        {
            //-Scene.ShowPopup("ClearPopup", info, ClearCallback);   
        */
	}
	
    void ClearCallback (object param) 
	{
        CLEAR_RESULT result = (CLEAR_RESULT)param;

        switch (result) 
		{
            case CLEAR_RESULT.CLOSE:
                GoToWorldMapOnLevelClear();
                break;
			case CLEAR_RESULT.NEXT_LEVEL:
#if UNITY_EDITOR
               // if(true == LevelEditorSceneHandler.EditorMode)
               //     ResetGame(currentLevel);
               // else
#endif
                    GoToWorldMapOnLevelClear();
                break;
            case CLEAR_RESULT.REPLAY :
                ResetGame(currentLevel);
                break;
        }
    }
	
    void GoToWorldMapOnLevelClear() 
	{
        WorldSceneHandler._paramInfo info = new WorldSceneHandler._paramInfo();
        info.isFromInGame       = true; 

#if UNITY_EDITOR
        if(LevelEditorSceneHandler.EditorMode)
		{
			Scene.ClearAll();
			Director.CurrentSceneLevelData = currentLevel;
			SceneManager.LoadScene("Scene/Editor/Editor");
			return;
		}
#endif

        // just cleared old level.
        if (Root.Data.currentLevel.Index < Root.Data.idxMaxClearedLevel+1)
            info.idxCurrent     = currentLevel.Index; 
        else
        {
            int idxNextLevel    = Root.Data.currentLevel.Index+1;
            if(idxNextLevel < Root.Data.TotalLevelCount)
            {
                Root.Data.currentLevel= Root.Data.GetLevelFromIndex( idxNextLevel );
                Root.Data.idxMaxClearedLevel    = idxNextLevel-1;                
                info.idxCurrent = idxNextLevel; 
            }
            else info.idxCurrent= Root.Data.TotalLevelCount - 1;
        }
        
		ChangeScene("WorldScene", (object)info);
    }

	void ShowOutOfMovePopup (System.Int64 score) 
	{
		//OutOfMoveHandler.PARAM  data = new OutOfMoveHandler.PARAM();
        //data.level              = currentLevel;
        //data.countAutoBlk       = JMFUtils.GM.playOverlayHandler._countAutoEventBlk;
		Scene.ShowPopup("OutOfMovePopup", null, (param) => 
        {
            CONTINUE_RESULT result = (CONTINUE_RESULT)param;
            byte failReason = (byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_FAIL;
            if(JMFUtils.GM.CountinueCount > 0)
                failReason = (byte)E_PUZZLE_GAMERESULT_FAIL_REASON.E_PUZZLE_GAMERESULT_FAIL_REASON_OUTOFMOVE;
            
            switch(result)
            {
                case CONTINUE_RESULT.CONTINUE:
#if !UNITY_EDITOR
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("level_continue", "sContinueLvId", (currentLevel.Index+1).ToString());
#endif

    			    JMFUtils.GM.CountinueCount++;
                    StartCoroutine( ContinueGame() );
                    PlayOverlayHandler handler = Scene.GetOverlayHandler<PlayOverlayHandler>();
                    handler.UpdateItems();
                    break;
                case CONTINUE_RESULT.GIVE_UP:
                    ShowFailPopup(score,failReason);
                    JMFUtils.GM.CountinueCount = 0;
                    break;
                case CONTINUE_RESULT.CLOSE:
                    ShowFailPopup(score,failReason);
                    JMFUtils.GM.CountinueCount = 0;
                    break;
            }
        });
    }

    static public void ShowFailPopup( System.Int64 score, System.Byte Fail_Reason) 
	{	
        /*Data.PuzzlePlayerGameResultData ResultData = Root.Data.gameData.GetPlayNowGameResultData();
        ResultData.sl64Score = score;
        ResultData.ucStar = (System.Byte)JMFUtils.GM.GetStarGrade();
        ResultData.ucIsCleared = 0;

        //ResultData.stLastGameResultUpdateDateTimeT = LGameData.GetInstance().GetCurrentServerTime();
        ResultData.ucPuzzleGameResult_Fail_Reason = Fail_Reason;
        ResultData.ssLeftMoveCounter = 0;//(System.Int16)(currentLevel.allowedMoves - JMFUtils.GM.Moves);                            // clear 시에는 clear 하고 남은 move 수 이고, fail 시에는 fail 시에 남은 move 수
        ResultData.siAutoEventBlockCount    = JMFUtils.GM.playOverlayHandler._countAutoEventBlk;
        //System.Byte[] sendpacket = LPuzzlePacket.LPuzzlePlayer_PACKET_REQ_GAME_RESULT();
        //if (null == sendpacket)
        //	Debug.LogError("Error LPuzzlePlayer_PACKET_REQ_GAME_RESULT, null == sendpacket");
        //else
        */

        Scene.ShowPopup("FailPopup", JMFUtils.GM.CurrentLevel.Index, (param)=>
        {
            if(param != null)
            {
                Wallet.Use("life");

                FAIL_RESULT result = (FAIL_RESULT)param;
                if(result == FAIL_RESULT.REPLAY)
                {   
                    if(Wallet.GetItemCount("life") <= 0)
                    {
                        Scene.ShowPopup("BuyItemPopup", "life", (param2) =>
                        {
                            if(Wallet.GetItemCount("life") > 0)
                                sRetryLevel(true);                     
                            else
                                sExitToLobby();                           
                        });
                    }
                    else
                        sRetryLevel(true);                    
                }
                else // close -> to lobby.
                {
                    sExitToLobby();
                }               
            }
        });

        JMFUtils.GM.CountinueCount = 0;
    }

    static void sExitToLobby()
    {
        WorldSceneHandler._paramInfo info2 = new WorldSceneHandler._paramInfo();
        info2.idxCurrent        = currentLevel.Index;
        info2.isFromInGame      = true;
        ChangeScene("WorldScene", (object)info2);// "NONE
    }

    static void sRetryLevel(bool exitWhenClose)
    {
        PlayReadyInfo _info     = new PlayReadyInfo();
        _info.currentLevelIndex = JMFUtils.GM.CurrentLevel.Index;
        _info.InGame            = true;
        Scene.ShowPopup("PlayReadyPopup",_info,(_PP)=>
        {
            switch((READY_RESULT)_PP)
            {
            case READY_RESULT.PLAY:
                Scene.ChangeTo("PlayScene", JMFUtils.GM.CurrentLevel);
                break;
            default:
            case READY_RESULT.CLOSE:
                if(exitWhenClose)   sExitToLobby();
                else                JMFUtils.GM.Resume(); 
                break;
            }
        });
    }

    void OnCloseFailPopup (object param) 
	{
		if(param != null)
		{
			Debugger.Assert(param is FAIL_RESULT, "PlaySceneHandler.PlayReadyCallback : Param is wrong.");
			FAIL_RESULT result = (FAIL_RESULT)param;
			if(result == FAIL_RESULT.REPLAY)
			{	
				PlayReadyInfo info = new PlayReadyInfo();
				info.currentLevelIndex = currentLevel.Index;
				info.InGame = true;
				Scene.ShowPopup("PlayReadyPopup",info,OnCloseFailPopup);
				//ResetGame(currentLevel);
			}
			else
			{
				//WorldSceneChangeInfo info = new WorldSceneChangeInfo();
				//info.LevelIndex = currentLevel.Index;
				//info.Type = "NONE";

				//ChangeScene("WorldScene", info);
			}				
		}
    }

    void ReadyGame (Data.Level level) 
	{
        Debugger.Assert(level != null, "PlaySceneHandler.ReadyGame : Level is null.");

        currentLevel = level;
		//if(null==Root.Data.currentLevel || Root.Data.currentLevel.Index<=level.Index) 
        Root.Data.currentLevel = level;

        /*
        Data.PuzzlePlayerGameResultData ResultData = Root.Data.gameData.GetPlayNowGameResultData();
#if UNITY_EDITOR
        bool bEditorMode  = LevelEditorSceneHandler.EditorMode;//UnityEditor.EditorPrefs.GetBool("JMK_EditorMode");
        if (null == ResultData)
        {
            ResultData = new Data.PuzzlePlayerGameResultData();
            Root.Data.gameData.SetPlayNowGameResultData(ResultData);
        }
#endif
        */
       // ResultData.DataReset(level.Index +1, LGameData.GetInstance().GetCurrentServerTime(), InfoLoader.GetUsedItemUniqueNumberList());
        Root.Data.gameData.SaveContext();

        UpdateBackground();
        UpdateRandomSeed();
        GM.Reset(currentLevel);
    }

    void StartGame ()
	{
        GM.StartGame();
//        string eventName = string.Format("Stage_{0}", currentLevel.GlobalIndex);
//        DWATTHelper.startStageEvent(eventName.Replace(" ", string.Empty));
		//skip tutorialID
		//return;
		
        string tutorialID       = TutorialOverlayHandler.GetAvailable(currentLevel.Index+1);
        if(tutorialID != null)  TutorialOverlayHandler.Activate(tutorialID);
    }

    void ReadyAndStartGame (Data.Level level)
	{
        Debugger.Assert(level != null, "PlaySceneHandler.ReadyAndStartGame : Level is null.");

        StartGame();
    }

    void UpdateBackground() 
	{
        //if (Root.Data.CurrentThemeIndex > -1  && Root.Data.CurrentThemeIndex < 25)
        //{
        //    background.SetSprite(string.Format("bg_{0}", Root.Data.CurrentThemeIndex));
        //    buttomBG.SetSprite(string.Format("UI_ingame_Board_BG_{0}", Root.Data.CurrentThemeIndex));
        //}
    }

    IEnumerator ContinueGame () 
	{
        if(GM.CurrentLevel.isTimerGame)
        {
            int nExtraTimes     = InfoLoader.GetDefaultPlayOnAddTime(JMFUtils.GM.CountinueCount);
            for(int q = 0; q < nExtraTimes+1; ++q)
            {
                yield return new WaitForSeconds(0.05f);
                GM.AddExtraTimes(1);
            }
        }
        else
        {
            int nExtraTurns     = InfoLoader.GetDefaultPlayOnMoveCount(JMFUtils.GM.CountinueCount);
            for(int q = 0; q < nExtraTurns; ++q)
            {
                yield return new WaitForSeconds(0.1f);
                GM.AddExtraMoves(1);
            }
        }
        
        GM.ContinueGame();
    }

//    void RetryGame () 
//	{
//        ResetGame(currentLevel);
//    }

    void ResetGame (Data.Level level)
	{
#if UNITY_EDITOR
        if(true == LevelEditorSceneHandler.EditorMode)
        {
            PlayReadyInfo info  = new PlayReadyInfo();
			info.currentLevelIndex = level.Index;
			info.InGame         = true;
			Scene.ShowPopup("PlayReadyPopup",info,OnCloseFailPopup);
            return;
        }
#endif

        if(NNSoundHelper.EnableBGM && NNSoundHelper.IsPlaying(BGMList[currentLevel.missionType]) == false)
			NNSoundHelper.PlayBGM(BGMList[currentLevel.missionType]);
		
        Debugger.Assert(level != null, "PlaySceneHandler.ResetGame : Level is null.");
        currentLevel = level;
        ITransition _transition = new FadeTransition();
        TaskManager.StartCoroutine(_transition.ShowTransitionEffect(1F, () => {
            ReadyGame(currentLevel);
            ReadyRibbonPopup();
        }, () => {}));
    }

    void ReadyRibbonPopup () 
	{
        GM.AllocateStartItems();

        //float fDelay            = fCHAR_POP_IN_DELAY;
        //string tutorialID       = TutorialOverlayHandler.GetAvailable(currentLevel.Index+1);
        //if(null!=tutorialID)    fDelay -= 0.5f;
        //StartCoroutine( GM.popCharacter(fDelay, true) );
		Scene.ShowPopup("MissionPopup", currentLevel, (param2) => {
			StartGame();
		});
		
    }

    void Repositioning () 
	{
		Vector3 newCamPos = Camera.main.transform.localPosition;

        Scene.SendMessage<PlayOverlayHandler>("UpdateMaskPositionAndSize", -newCamPos.y);

        int leftEmptyCount = 0;
        int rightEmptyCount = 0;
        int topEmptyCount = 0;
        int bottomEmptyCount = 0;

        for (int x = 0; x < GameManager.WIDTH; x++)
		{
            bool isEmptyColumn = true;

            for (int y = 0; y < GameManager.HEIGHT; y++) 
			{
                if (GM[x,y].PND is EmptyPanel) continue;
                isEmptyColumn = false;
                break;
            }

            leftEmptyCount = x;
            if (isEmptyColumn == false) break;
        }

        for (int x = GameManager.WIDTH - 1; x >= 0; x--) 
		{
            bool isEmptyColumn = true;

            for (int y = 0; y < GameManager.HEIGHT; y++) 
			{
                if (GM[x,y].PND is EmptyPanel) continue;
                isEmptyColumn = false;
                break;
            }

            rightEmptyCount = (GameManager.WIDTH - 1) - x;
            if (isEmptyColumn == false) break;
        }

        for (int y = GameManager.HEIGHT - 1; y >= 0; y--) 
		{
            bool isEmptyLow = true;

            for (int x = 0; x < GameManager.WIDTH; x++) 
			{
                if (GM[x,y].PND is EmptyPanel) continue;
                isEmptyLow = false;
                break;
            }

            topEmptyCount = (GameManager.HEIGHT - 1) - y;
            if (isEmptyLow == false) break;
        }

        for (int y = 0; y < GameManager.HEIGHT; y++) 
		{
            bool isEmptyLow = true;

            for (int x = 0; x < GameManager.WIDTH; x++) 
			{
                if (GM[x,y].PND is EmptyPanel) continue;
                isEmptyLow = false;
                break;
            }

            bottomEmptyCount = y;
            if (isEmptyLow == false) break;
        }

        int emptyColumnCount = leftEmptyCount - rightEmptyCount;
        int emptyLowCount = bottomEmptyCount - topEmptyCount;
		
		newCamPos.x += emptyColumnCount * JMFUtils.GM.Size * 0.5F;
		newCamPos.y += emptyLowCount * JMFUtils.GM.Size * 0.5F;
		TutorialOverlayHandler.camPos = newCamPos;
    }

    LevelResultData IncreaseFailCount() 
	{
		//Root.Data.gameData.StraightWinCount = 0;
		Data.LevelResultData data = Root.Data.gameData.GetLevelResultDataByIndex(currentLevel.Index);
		bool bAdd = (data == null);
		if(data == null)
			data = new Data.LevelResultData();
		
		int count = Root.Data.gameData.GetStraightFailCountLevelByIndex(currentLevel.Index);
		data.siStraightFailCount = count + 1;
		if(bAdd)
			Root.Data.gameData.AddLevelResultData(data);
		else
			Root.Data.gameData.SetLevelResultDataByIndex(currentLevel.Index, data);
        
        return data;
    }


    void UpdateRandomSeed (bool useForceSeed = false) 
	{
        if (useForceSeed || CheckTutorialLevel()) 
		{
            NNTool.Seed = 0;
            return;
        }

		string key = currentLevel.Index.ToString();

        List<int> usedSeeds = null;
        NNTool.Seed = Random.Range(0, 100);

        if (usedSeedDict.ContainsKey(key)) 
		{
            usedSeeds = usedSeedDict[key];

            if (usedSeeds.Count >= 100) usedSeeds.Clear();

            while (usedSeeds.Contains(NNTool.Seed)) 
			{
                NNTool.Seed = Random.Range(0, 100);
            }
        } 
		else
		{
            usedSeeds = new List<int>();
            usedSeedDict.Add(key, usedSeeds);
        }

        //currentSeed = NNTool.Seed;
        usedSeeds.Add(NNTool.Seed);
    }

    bool CheckTutorialLevel() 
	{
//		if (Root.Data.currentLevel.Index > 0) return false;
//        if (currentLevel.Index == 0) return true;
//        if (currentLevel.Index == 2) return true;
//        if (currentLevel.Index == 3) return true;
//        if (currentLevel.Index == 5) return true;
        return false;
    }

    static void ChangeScene(string scene, object param) 
	{
        // test.
        //Scene.ChangeTo("WorldScene", null);
        //return;
		
#if UNITY_EDITOR
		
		if(scene == "WorldScene" && LevelEditorSceneHandler.EditorMode)
		{
			Scene.ClearAll();
            DOTween.KillAll();
			Director.CurrentSceneLevelData = currentLevel;
			SceneManager.LoadScene("Scene/Editor/Editor");
			return;
		}
		else
#endif
        {
            Scene.ChangeTo(scene, param);
        }
    }    

    public void triggerDarkBGEffect(bool turnOn)
    {
        _sprDarkBG.DOKill();
        if(true == turnOn)
        {
            _sprDarkBG.gameObject.SetActive( true );
            _sprDarkBG.color    = new Color(0, 0, 0, 0);
            _sprDarkBG.DOFade(0.4f, 1.0f);
        }
        else
        {
            _sprDarkBG.DOFade(0, 1.0f).OnComplete( () =>
            {
                _sprDarkBG.gameObject.SetActive(false);
            });
        }
    }

    void OnPacketResGameResult(bool pktSuccess, int Result)
    {
        /*
        // note : 게임 중 이 패킷을 1회 받으면 뒤론 무시한다.
        JMFRelay.ON_PACKET_RES_GAME_RESULT -= OnPacketResGameResult;

        if (true == pktSuccess)
        {
            Data.PuzzlePlayerGameResultData playData = Root.Data.gameData.GetPlayNowGameResultData();

            if (1 == playData.ucIsCleared)   // if win ?
            {
                Data.LevelResultData data = Root.Data.gameData.GetLevelResultDataByIndex(currentLevel.Index);
                if (null == data)
                {
                    // ForceLogout((int)E_GAME_RESULT_ERROR_CODE.E_GAME_RESULT_ERROR_CODE_RemovGameResultData);
                    return;
                }

                //Director.Instance.RefreshStamina();
                if(NOVNINE.Wallet.GetItemCount("life") < Data.GameData.FULL_STAMINA)
                    NOVNINE.Wallet.Gain("life",1);

                ShowClearPopup(mResultPrevGrade, playData.ucStar, data.sl64Score, _curScore, mResultIsHighScore);
            }
            else                            // if lose ?
            {
                Scene.ClosePopup();
                DOVirtual.DelayedCall(0.6f, () =>
                    {
                        FailInfo info = new FailInfo();
                        info.level = currentLevel;
                        info.score = playData.sl64Score;
                        //Scene.ShowPopup("FailPopup", info, OnCloseFailPopup);        
                    });
            }
            //
        }
        else
            ForceLogout(Result);
        
        
        mResultPrevGrade        = -1;
        mResultIsHighScore      = false;
        */
    }
    /*
    void ForceLogout(int Result)
    {
        // relese blocker.
        Scene.UnlockWithMsg();

        NoticePopupHandler popup = Scene.GetPopup("NoticePopup") as NoticePopupHandler;
#if LIVE_MODE
        popup.SetErrorCode("Note!", "Result data Processing was failed.");
#else
        popup.SetErrorCode("Note!", string.Format("Result data Processing was failed.\nERROR:{0}",Result));
#endif
        popup.InitButton(NoticePopupHandler.BUTTON_TYPE.OK);
        Scene.AddPopup("NoticePopup",true);

        // 재로긴 필요.
        popup.OK_Callback = (_param) =>
        {
            //Director.GoTitle("Home");
        };
    }*/

    // play scene에서 나가도록 한다.
    void OnNetworkErrPopupBtnOk()
    {
        
    }
        
}
