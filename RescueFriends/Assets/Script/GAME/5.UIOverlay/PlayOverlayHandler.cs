using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NOVNINE;
using NOVNINE.Store;
using NOVNINE.Diagnostics;
using DG.Tweening;
using Spine.Unity;
using Spine;

public class PlayOverlayHandler : MonoBehaviour, IUIOverlay {

	public enum WORD_TYPE { NONE = -1, GOOD, WOW, COOL, GREAT, UNBELIEVABLE, SHUFFLE};
	readonly string[] WORDS = new string[] { "good", "wow", "cool", "great", "unbelievable", "shuffle"};
	
	public ScoreProgress scoreProgress;
    //public Mission mission;
	public CollectAnyMission collectAnyMission;
	public AIMission aiMission;
    public ScoreMission scoreMission;
    public Transform effects;
    //public tk2dUIMask mask;
   // public GameObject boardMask;
    
    //public tk2dTextMesh winningConditionLabel;
    //public tk2dTextMesh itemText;
    public ItemButton[] itemButtons;
    //public TextMesh levelNumber;

    //public GameObject _objGoalFood;

    // Buff Gauage Relatives.
    const int COUNT_BUFF_TARGET = 3;
    public GameObject           _objBuffBox;
    public tk2dSlicedSprite     _sprBuffGauge;
    public ParticleSystem       _effBuffGained;
    public ParticleSystem       _effBuffCharged;
    bool _isOnFoxBuff           = false;
    public chatBox _chatBox     = null;

	public tk2dSprite           _sprWordEffect;
	WORD_TYPE wordType = WORD_TYPE.NONE;
	
    //public SkeletonAnimation _skFxAiTurn;
    public GameObject _objMoveCountUI;          // for accessing position info.

    //public GameObject _objAiCurtain;

    public AnimationCurve _gainObjectCurve;
    public float _fGainObjectSpeed  = 6.0f;
    public float _fGainPotionSpeed  = 6.0f;

    public ParticleSystem       _effItemTrigger;

  //  public tk2dTextMesh _levelID;
   // public GameObject _autoMission;

    Vector3 orgMaskPos;
    Vector3 orgWarningMessagePosition;
    GAME_ITEM equipment;
    tk2dTextMesh winningConditionGainLabel;
//    PromotionPlanner.Plan plan;

    bool mOnItemUsing           = false;
    //
    bool _isPlayingComboText    = false;
    public bool IsPlayingComboText()    {  return _isPlayingComboText; }
    //
    public int _countAutoEventBlk { get; private set; }

    ItemButton _curItemBtn      = null;
    int buyItemIndex = -1;

    bool _isValidAutoEvent      = false;

    public GAME_ITEM Equipment 
	{
        get { return equipment; }
        set {
            if (equipment == value) return;

            equipment = value;

            //= 고치자... ShowBoardMask(equipment != GAME_ITEM.NONE);

//            Vector3 newPos;
            
            if(GAME_ITEM.NONE == equipment)
                NNSoundHelper.Stop("IFX_bonus_time_bg");
            else 
                NNSoundHelper.Play("IFX_bonus_time_bg", true);

            switch (equipment)
			{
                case GAME_ITEM.HAMMER :
//                    newPos = hammerItem.transform.localPosition;
//                    newPos.z = -10F;
//                    hammerItem.transform.localPosition = newPos;
				//	itemText.text = "HAMMER.";
                    break;
				case GAME_ITEM.FIRECRACKER :
//                    newPos = boomItem.transform.localPosition;
//                    newPos.z = -10F;
//                    boomItem.transform.localPosition = newPos;
				//	itemText.text = "FIRE CRACKER.";
                    break;
				case GAME_ITEM.MAGICSWAP :
//                    newPos = swapItem.transform.localPosition;
//                    newPos.z = -10F;
//                    swapItem.transform.localPosition = newPos;
				//	itemText.text = "MAGIC SWAP";
                    break;
				case GAME_ITEM.MAGICSHUFFLE :
//                    newPos = magicStickItem.transform.localPosition;
//                    newPos.z = -10F;
//                    magicStickItem.transform.localPosition = newPos;
			//		itemText.text = "RAINBOW BUST";
                    break;
                case GAME_ITEM.NONE:
                    if(null != _curItemBtn)
                    {
                        _curItemBtn.IsOn    = false;
                        _curItemBtn         = null;
                    }
                    break;
                default: break;
            }

            //
            //itemText.Commit();
        }
    }

    void Awake () 
	{
        //  orgMaskPos = boardMask.transform.localPosition;
        //	startDelegate = new Spine.AnimationState.TrackEntryDelegate(OnStart);

        // test.
        //Root.Data.gameData.setBuffCount(0);
        //

        JMFUtils.POH            = this;
	}
	
	void Start()
	{
		//if(WordEffect != null)
        //{
        //    WordEffect.GetComponent<MeshRenderer>().enabled = false;
        //    WordEffect.AnimationState.Start += startDelegate;
        //}
	}
	
	void OnStart (TrackEntry entry)
	{
		//=if (wordType == WORD_TYPE.SHUFFLE) 
		//	NNSoundHelper.Play("change_block");
		//else if (wordType == WORD_TYPE.DELICIOUS) 
		//	NNSoundHelper.Play("bonus_time");
		//else 
		//	NNSoundHelper.Play("compose_"+WORDS[(int)wordType]);
		
		wordType = WORD_TYPE.NONE;
        mOnItemUsing            = false;
	}

    public void DoDataExchange () {}
	
	public GameObject GetGameObject()
	{
		return gameObject;
	}

    public void OnEnterUIOveray (object param) 
	{
        buyItemIndex = -1;
        JMFRelay.OnGameReady += OnGameReady;
        JMFRelay.OnClickPanel += OnClickPanel;
        JMFRelay.OnClickPiece += OnClickPiece;
        JMFRelay.OnFinishCombo += OnFinishCombo;
        //JMFRelay.OnShuffle += OnShuffle;
        JMFRelay.OnGameOver += OnGameOver;
        JMFRelay.OnStartBonus += OnStartBonus;
        JMFRelay.OnPlayerMove += OnPlayerMove;
        GameManager.OnEnableFrogJump += OnEnableFrogJump;
     //   JMFRelay.PACKET_RES_ITEM_BUY += OnPacketResItemBuy;
        //JMFRelay.OnCollectPieceForAutoEvent += onCollectPieceForAutoEvent;
//		NOVNINE.Wallet.itemChangedCallback += OnChangedItem;

        _isOnFoxBuff        = false;

        _refreshBuffBox();
    }

    public void OnLeaveUIOveray () {
        JMFRelay.OnGameReady -= OnGameReady;
        JMFRelay.OnClickPanel -= OnClickPanel;
        JMFRelay.OnClickPiece -= OnClickPiece;
        JMFRelay.OnFinishCombo -= OnFinishCombo;
        //JMFRelay.OnShuffle -= OnShuffle;
        JMFRelay.OnGameOver -= OnGameOver;
        JMFRelay.OnStartBonus -= OnStartBonus;
        JMFRelay.OnPlayerMove -= OnPlayerMove;
        GameManager.OnEnableFrogJump -= OnEnableFrogJump;
//		NOVNINE.Wallet.itemChangedCallback -= OnChangedItem;
    //    JMFRelay.PACKET_RES_ITEM_BUY -= OnPacketResItemBuy;
        //JMFRelay.OnCollectPieceForAutoEvent -= onCollectPieceForAutoEvent;
        ResetToggleItem();

        // de-init.
        collectAnyMission.gameObject.SetActive(false);
		aiMission.gameObject.SetActive(false);
        scoreMission.gameObject.SetActive( false );


        Equipment               = GAME_ITEM.NONE;
        StopCoroutine( "_updateAutoEventTimer" );
    }

    void ResetToggleItem() {
        Equipment = GAME_ITEM.NONE;
//        hammerItem.IsOn = false;
//        boomItem.IsOn = false;
//        swapItem.IsOn = false;
//        magicStickItem.IsOn = false;
        ShowBoardMask(false);
    }

//    void OnChangedItem(NOVNINE.InventoryItem item, int count) {
//        UpdateItems();
//    }

    void OnGameReady () 
	{
        //levelNumber.text = string.Format("LEVEL {0}", JMFUtils.GM.CurrentLevel.GlobalIndex);
        UpdateMission();
		UpdateItems();
    }
	
	void UpdateMission () 
	{
		collectAnyMission.gameObject.SetActive(false);
		aiMission.gameObject.SetActive(false);
		scoreMission.gameObject.SetActive( false );

		// [AI_MISSION]
		if((EditWinningConditions.MISSION_TYPE)JMFUtils.GM.CurrentLevel.missionType == EditWinningConditions.MISSION_TYPE.DEFEAT)
			aiMission.gameObject.SetActive(true);
        else if((EditWinningConditions.MISSION_TYPE)JMFUtils.GM.CurrentLevel.missionType == EditWinningConditions.MISSION_TYPE.SCORE)
            scoreMission.gameObject.SetActive( true );
		else
		{
			collectAnyMission.gameObject.SetActive(true);
			if((EditWinningConditions.MISSION_TYPE)JMFUtils.GM.CurrentLevel.missionType == EditWinningConditions.MISSION_TYPE.CLEAR)
				collectAnyMission.InitClearMission();
		}
		
//		switch((EditWinningConditions.MISSION_TYPE)JMFUtils.GM.CurrentLevel.missionType)
//		{
//			case EditWinningConditions.MISSION_TYPE.DEFEAT:     // AI Mode.
//				aiMission.gameObject.SetActive( true );
//			break;
//			case EditWinningConditions.MISSION_TYPE.COLLECT:    // collect Any.
//			case EditWinningConditions.MISSION_TYPE.FILL:       // catch Potions... 
//			case EditWinningConditions.MISSION_TYPE.FIND:       // find ChocoBars.
//			case EditWinningConditions.MISSION_TYPE.CLEAR:      // [SHADE_CURSE]
//				collectAnyMission.gameObject.SetActive( true );
//			break;
//		}

		/*        else if (JMFUtils.GM.CurrentLevel.isClearShadedGame) {
            targets[0].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isClearChocolateGame) {
            targets[1].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isGetTypesGame) {
            targets[2].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isTreasureGame) {
            targets[3].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isSpecialJewelGame) {
            targets[4].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isPenguinGame) {
            targets[5].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isYetiGame) {
            targets[6].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isFairyGame) {
            targets[7].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isBossGame) {
            targets[8].SetActive(true);
        } else if (JMFUtils.GM.CurrentLevel.isTimerGame || JMFUtils.GM.CurrentLevel.isMaxMovesGame) {
            targets[9].SetActive(true);
        }*/
	}

    void OnClickPanel (Point pt) 
	{
		if (Equipment == GAME_ITEM.MAGICSWAP) return;
        if(false==JMFUtils.GM.IsAllBoardStable || JMFUtils.GM.Pump!=JMFUtils.GM.idlePump)
            return;
        if(JMFUtils.GM.isAIFightMode && JMFUtils.GM.isCurPlayerAI)
            return;

        switch (Equipment) 
		{
        case GAME_ITEM.HAMMER:            
            StartCoroutine(CoUseHammer(pt));
            break;
        case GAME_ITEM.FIRECRACKER:
            StartCoroutine(CoUseFireCracker(pt));
            break;
        case GAME_ITEM.MAGICSHUFFLE:    
            StartCoroutine(CoUseMagicShuffle());            
            break;
        default:
            break;
        }
    }

    // 두개를 swap 함.
    public IEnumerator UseSwapJewel (Board bdA, Board bdB)
    {
        if(true==mOnItemUsing)  yield break;
        if (Wallet.GetItemCount("magicswap")<=0)
            yield break;        // double check.

        mOnItemUsing            = true;

        //List<Board> listBoards  = new List<Board>();
        //listBoards.Add( bdA );
        //listBoards.Add( bdB );

        // 커튼 처리.
        //AiCurtainReady( ref listBoards, 0.8f );
        // process char magic fire.
        //yield return JMFUtils.GM.fireItemMagicAnimation(bdA);

        // 실제 swap은 GM caller func에서 처리.

        // 커튼 hide.
        yield return new WaitForSeconds(0.5F);
        //AiCurtainClose();

        // 소진 처리.
        Wallet.Use("magicswap");

        NNSoundHelper.Play("IFX_block_match_01");
        //Scene.SendMessage<PlaySceneHandler>("MarkOnUseItem", "Swap");
        UpdateItems();
        Equipment               = GAME_ITEM.NONE;

        Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", false);
        mOnItemUsing            = false;
    }

    // 특정 셀 하나를 터뜨림.
    IEnumerator CoUseHammer (Point pt)
    {
        if(true==mOnItemUsing)  yield break;
        if (Wallet.GetItemCount("hammer")<=0)
            yield break;        // double check.

        mOnItemUsing            = true;
        JMFUtils.GM.itemUsing   = true;

        Equipment               = GAME_ITEM.NONE;
        
        // process curtain.
        //List<Board> listBoards  = new List<Board>();
        //listBoards.Add(JMFUtils.GM[pt]);
        //AiCurtainReady( ref listBoards, 0.8f );
        // process char magic fire.
        _effItemTrigger.transform.localPosition = new Vector3(JMFUtils.GM[pt].LocalPosition.x, JMFUtils.GM[pt].LocalPosition.y, _effItemTrigger.transform.localPosition.z);
        _effItemTrigger.Play();
        yield return new WaitForSeconds(0.7f);
        //yield return JMFUtils.GM.fireItemMagicAnimation(JMFUtils.GM[pt]);//  new WaitForSeconds(0.8f);

        // process item action.
        Wallet.Use("hammer");
        JMFUtils.GM[pt].Hit();
        // JMFUtils.GM.SplashHit( JMFUtils.GM[pt], .0f );
        UpdateItems();

        yield return new WaitForSeconds(0.5f);
        //NNPool.Abandon(effect);
        //AiCurtainClose();

        mOnItemUsing            = false;        
        Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", false);
        //Scene.SendMessage<PlaySceneHandler>("MarkOnUseItem", "Hammer");
    }

    // 선택된 컬러 모두를 터뜨림. - rainbow power.
  /*  IEnumerator CoUseRainbowBust (Point pt) {
        
        Equipment = GAME_ITEM.NONE;
        // if ((JMFUtils.GM[pt].PD is NormalPiece) == false)
        // -> Match가 가능한 녀석들은 모두 작동 가능토록 변경.
        if(null==JMFUtils.GM[pt].PD || false==JMFUtils.GM[pt].PD.isMatchable)
        {
            UpdateItems();
            Equipment = GAME_ITEM.NONE;
            //return false;
			yield break;
        }
        if(true==mOnItemUsing)  yield break;
        if (Wallet.GetItemCount("rainbowbust")<=0)
            yield break;        // note : double check.

        mOnItemUsing            = true;
        JMFUtils.GM.itemUsing   = true;

        NNSoundHelper.Play("IFX_block_match_01");
        
        int targetColorIndex    = JMFUtils.GM[pt].ColorIndex;
		BlockCrash effect       = null;
        // 대상 특정.
        List<Board> listBoards  = new List<Board>();
        foreach (Board _bd in JMFUtils.GM.Boards) 
		{
			if (_bd.IsFilled == false) continue;
			if (_bd.Piece.IsMatchable() == false) continue;
			if (_bd.Piece.ColorIndex != targetColorIndex) continue;
			
            listBoards.Add( _bd );
        }

        // 커튼 처리.
        AiCurtainReady( ref listBoards, 0.8f );
        // process char magic fire.
        yield return JMFUtils.GM.fireItemMagicAnimation(JMFUtils.GM[pt]);
        
        // 떠뜨리자.
        for(int q = 0; q < listBoards.Count; ++q)
        {
			//effect = NNPool.GetItem<BlockCrash>("NormalPieceCrash");
            //effect.Play("play", listBoards[q].Position, listBoards[q].Piece.Scale, listBoards[q].ColorIndex, false, .0f);
			//listBoards[q].Hit(.0f);
		}
        //= JMFUtils.GM.SplashHit( listBoards );

        //  소진.
        if(_WAYNE_TESTOR._CNT_ITEM_RAINBOWBUST <= 0)
        {
            Wallet.Use("rainbowbust");

           // Data.PuzzleItem item = new Data.PuzzleItem();
           // item.siUniqueNumber = NOVNINE.Wallet.ChangeItemUniqueNumberByID("rainbowbust");
           // item.ssCount = 1;
            //Root.Data.gameData.AddGameUsedLPuzzleItem(item);
        }

        UpdateItems();
        yield return new WaitForSeconds(0.5F);
        AiCurtainClose();

        mOnItemUsing            = false;
        //NNPool.Abandon(effect);
        //Scene.SendMessage<PlaySceneHandler>("MarkOnUseItem", "MagicStick");
    }*/

    IEnumerator CoUseMagicShuffle()
    {
        // pre-check.
        if(true==mOnItemUsing)  yield break;

        if(Wallet.GetItemCount("magicshuffle")<=0)
            yield break;        // double check.

        mOnItemUsing            = true;
        JMFUtils.GM.itemUsing   = true;

        // Shuffle.
        JMFUtils.GM.isShuffleItemTriggered  = true;
        //JMFUtils.GM.Resume();
        yield return new WaitForSeconds(1.0f);
        do{
            yield return new WaitForSeconds(1.0f);
        }while( JMFUtils.GM.Pump != JMFUtils.GM.idlePump );
        
        // post check.
        Wallet.Use("magicshuffle");
        UpdateItems();

        Equipment               = GAME_ITEM.NONE;
        mOnItemUsing            = false;
        Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", false);
    }


    // 십자로 떠뜨림.
    IEnumerator CoUseFireCracker (Point pt, float delayTime = 0.1F)
    {                
        if(true==mOnItemUsing)  yield break;

        if(Wallet.GetItemCount("firecracker")<=0)
            yield break;        // double check.


        mOnItemUsing            = true;
        JMFUtils.GM.itemUsing   = true;

        Board targetBoard       = JMFUtils.GM[pt];

        _effItemTrigger.transform.localPosition = new Vector3(JMFUtils.GM[pt].LocalPosition.x, JMFUtils.GM[pt].LocalPosition.y, _effItemTrigger.transform.localPosition.z);
        _effItemTrigger.Play();
        yield return new WaitForSeconds(0.7f);

        // search target.
        List<Board> listBoards  = new List<Board>();
        listBoards.Add( targetBoard );
        
        searchTargetWithDirection(targetBoard, JMF_DIRECTION.LEFT, ref listBoards);
        searchTargetWithDirection(targetBoard, JMF_DIRECTION.RIGHT, ref listBoards);
        searchTargetWithDirection(targetBoard, JMF_DIRECTION.UP, ref listBoards);
        searchTargetWithDirection(targetBoard, JMF_DIRECTION.DOWN, ref listBoards);
        
        // 커튼 처리.
        //AiCurtainReady( ref listBoards, 0.8f );
        // process char magic fire.
        //yield return JMFUtils.GM.fireItemMagicAnimation(JMFUtils.GM[pt]);


        // 떠뜨림.
        // piece가 없거나(보드), 있으면 특수 piece 제외하고.
        if(null==targetBoard.PD || (null != targetBoard.PD && !(targetBoard.PD is SpecialFive) && !(targetBoard.PD is TMatch7Piece)) )
            targetBoard.Hit();
		BlockLine effect_H = NNPool.GetItem<BlockLine>("BlockLine");
        Vector3 vScale          = null!=targetBoard.Piece ? targetBoard.Piece.Scale : Vector3.one;
		effect_H.Play("horizontal_hit", targetBoard.Position, vScale, targetBoard.ColorIndex, false);
		BlockLine effect_V = NNPool.GetItem<BlockLine>("BlockLine");
		effect_V.Play("vertical_hit", targetBoard.Position, vScale, targetBoard.ColorIndex, false);

        DestroyDirection(targetBoard, JMF_DIRECTION.LEFT, delayTime);
        DestroyDirection(targetBoard, JMF_DIRECTION.UP, delayTime);
        DestroyDirection(targetBoard, JMF_DIRECTION.RIGHT, delayTime);
        DestroyDirection(targetBoard, JMF_DIRECTION.DOWN, delayTime);
        NNSoundHelper.Play("IFX_lineblock_bust");

        yield return new WaitForSeconds(0.5F);
        //AiCurtainClose();

        Wallet.Use("firecracker");
        UpdateItems();

        Equipment               = GAME_ITEM.NONE;
        mOnItemUsing            = false;
        Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", false);

        //Scene.SendMessage<PlaySceneHandler>("MarkOnUseItem", "CrossBomb");
    }

    void OnClickPiece (Point pt) {
        //= NNSoundHelper.Play("touch");
    }

    void OnFinishCombo (int comboCount) 
	{
        if (comboCount < 3)     return;
        if(_isOnFoxBuff)        return;

		//if (comboCount > Root.Data.gameData.record.playerData.BestCombo) 
		{
		//	Root.Data.gameData.record.playerData.BestCombo = comboCount;
            //string achieveKey = NOVNINE.Context.SocialPlatformBase.leaderboards[2].id;
            //SocialInfoReporter.ReportScore(Root.Data.gameData.record.BestCombo, achieveKey);
        }
		
		if (comboCount > 6) 
			wordType = WORD_TYPE.UNBELIEVABLE;
        else if (comboCount > 5)
			wordType = WORD_TYPE.GREAT;
        else if (comboCount > 4)
			wordType = WORD_TYPE.COOL;
        else if (comboCount > 3)
			wordType = WORD_TYPE.WOW;
        else if (comboCount > 2)
			wordType = WORD_TYPE.GOOD;
		
		float time              = ShowWordEffect( WORDS[(int)wordType] );
        if(time > .0f)
        {
            _isPlayingComboText = true;
            DOVirtual.DelayedCall(time, () => { _isPlayingComboText = false; });
        }
    }

	public float OnShuffle()
	{
		wordType = WORD_TYPE.SHUFFLE;
		return ShowWordEffect( WORDS[(int)wordType] );
    }

	float ShowWordEffect( string animationName, float delay = 0.0f ) 
	{
        _sprWordEffect.gameObject.SetActive( true );
        _sprWordEffect.spriteName   = animationName;

        _sprWordEffect.transform.localScale = Vector3.zero * 0.01f;
        _sprWordEffect.transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutExpo);

        DOVirtual.DelayedCall(1.0f, () =>
        {
            _sprWordEffect.transform.DOScale(0.01f, 0.05f).SetEase(Ease.OutExpo).OnComplete( () =>
            {
                _sprWordEffect.gameObject.SetActive( false );
            });
        });

        JMFUtils.GM.playComboVoiceFx();
        return 0.5f+0.5f+0.2f;
	}
	
	int OnGameOver (bool isWin, System.Byte grade, System.Int64 score) 
	{
        Equipment = GAME_ITEM.NONE;
        return 0;
    }

    void OnStartBonus () 
	{
        Equipment = GAME_ITEM.NONE;
        //WordEffect wordEffect = NNPool.GetItem<WordEffect>("WordEffect", effects);
        //wordEffect.ShowWord(WORD_TYPE.BONUS);
    }

    void OnPlayerMove ()
	{
        Equipment = GAME_ITEM.NONE;
    }

    void OnEnableFrogJump (bool enabled) 
	{
        //itemText.text = "PICK MY LANDING SQUARE.\n I'M ABOUT TO SMASH SOME JEWEL!";
        //itemText.Commit();
        //ShowBoardMask(enabled);
    }

    void OnClickTimeOrMove (tk2dUIItem item) 
	{
        /*if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return;

        if (Wallet.GetItemCount("timer_or_move") > 0) 
        {
            if (JMFUtils.GM.CurrentLevel.isMaxMovesGame) 
            {
                JMFUtils.GM.AddExtraMoves(5);
                AnimateGain(5);
            }
            else 
            {
                JMFUtils.GM.AddExtraTimes(15);
                AnimateGain(15);
            }

            Wallet.Use("timer_or_move");
            UpdateItems();
            //= NNSoundHelper.Play("coin2");

            //Scene.SendMessage<PlaySceneHandler>("MarkOnUseItem", "TimeOrMove");
        } 
        else 
        {
            JMFUtils.GM.Pause();
//            if (JMFUtils.GM.CurrentLevel.isTimerGame) {
//                Scene.ShowPopup("BuyPopup", BUY_ITEM_TYPE.TIME, BuyCallback);
//            } else {
//                Scene.ShowPopup("BuyPopup", BUY_ITEM_TYPE.MOVE, BuyCallback);
//            }
        }*/
    }

    void _showBuyItemPopup(string strId)
    {
        Scene.ShowPopup("BuyItemPopup", strId, (param) =>
        {
            UpdateItems();
            JMFUtils.GM.Resume();
        });

        /*
        Scene.ShowPopup("AddBoostPopup", info, (param ) =>
		{
            BUY_RESULT result = (BUY_RESULT)param;
            if(result == BUY_RESULT.CANCLE)
                JMFUtils.GM.Resume();
		});	*/
    }


    public void preSelectItemButton(string strItemId)
    {
        /*GAME_ITEM eTarget;
        switch(strItemId)
        {
            case "hammer":      eTarget = GAME_ITEM.HAMMER;        break;
            case "firecracker": eTarget = GAME_ITEM.FIRECRACKER;   break;
            case "magicswap":   eTarget = GAME_ITEM.MAGICSWAP;     break;
        default:            return;
        }

        for(int g = 0; g < itemButtons.Length; ++g)
        {
            if(eTarget == itemButtons[g].itemType)
            {
                switch(eTarget)
                {
                    case GAME_ITEM.HAMMER:
                    if(true==Root.Data.gameData.record.playerData.HammerUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM)
                        OnClickHammer(itemButtons[g].gameObject.GetComponent<tk2dUIItem>());
                    break;
                    case GAME_ITEM.MAGICSWAP:
                    if(true == Root.Data.gameData.record.playerData.MagicSwapUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM)
                        OnClickSwap(itemButtons[g].gameObject.GetComponent<tk2dUIItem>());
                    break;
                    case GAME_ITEM.FIRECRACKER:
                    if(true == Root.Data.gameData.record.playerData.FirecrackerUnlocked || _WAYNE_TESTOR._UNLOCK_ITEM)
                        OnClickFireCracker(itemButtons[g].gameObject.GetComponent<tk2dUIItem>());
                    break;
                }
                break;
            }
        }
        strItemId           = "";
        */
    }

    void OnClickTestClear(tk2dUIItem item)
    {
        // test code.
        JMFRelay.FireOnGameOver(true, 3, 10000);
    }

    void OnClickHammer (tk2dUIItem item)
    {
        Debugger.Assert(item != null, "PlayOverlayHandler.OnClickHammer : Toggle is null."); 

        if(false==JMFUtils.GM.IsAllBoardStable || true==mOnItemUsing)
            return;
        if(JMFUtils.GM.Pump!=JMFUtils.GM.idlePump && JMFUtils.GM.Pump!=JMFUtils.GM.failPump)
            return;

        // de-init view.
        if(GAME_ITEM.HAMMER != Equipment)
            Equipment           = GAME_ITEM.NONE;

        //if(!Root.Data.gameData.record.playerData.HammerUnlocked && !_WAYNE_TESTOR._UNLOCK_ITEM)
        //    return;

        ItemButton itemBtn      = item.GetComponent<ItemButton>();
        if(null == itemBtn)     return;
        _curItemBtn             = null;

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) 
        {
            itemBtn.IsOn        = false;
            return;
        }

        if (Wallet.GetItemCount("hammer")>0 || _WAYNE_TESTOR._CNT_ITEM_HAMMER>0) 
        {
            if (!itemBtn.IsOn)
            {
                Equipment = GAME_ITEM.HAMMER;
                _curItemBtn     = itemBtn;
            }
            else 
            {
                Equipment = GAME_ITEM.NONE;
            }

            itemBtn.IsOn        = !itemBtn.IsOn;
            Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", itemBtn.IsOn);
        }
        else
        {
            itemBtn.IsOn        = false;
            JMFUtils.GM.Pause();
            //=Scene.ShowPopup("BuyPopup", BUY_ITEM_TYPE.HAMMER, BuyCallback);
            _showBuyItemPopup("hammer");
        }

        //= NNSoundHelper.Play("touch");
    }

    void OnClickFireCracker (tk2dUIItem item) {

        if(false==JMFUtils.GM.IsAllBoardStable || true==mOnItemUsing)
            return;
        if(JMFUtils.GM.Pump!=JMFUtils.GM.idlePump && JMFUtils.GM.Pump!=JMFUtils.GM.failPump)
            return;

        Debugger.Assert(item != null, "PlayOverlayHandler.OnClickBoom : Toggle is null."); 

        // de-init view.
        if(GAME_ITEM.FIRECRACKER != Equipment)
            Equipment           = GAME_ITEM.NONE;

        //if(!Root.Data.gameData.record.playerData.FirecrackerUnlocked && !_WAYNE_TESTOR._UNLOCK_ITEM)
        //    return;

        ItemButton itemBtn      = item.GetComponent<ItemButton>();
        if(null == itemBtn)     return;
        _curItemBtn             = null;

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) {
            itemBtn.IsOn        = false;
            return;
        }

		if (Wallet.GetItemCount("firecracker")>0 || _WAYNE_TESTOR._CNT_ITEM_FIRECRACKER>0 ) {
            if(!itemBtn.IsOn) {
				Equipment       = GAME_ITEM.FIRECRACKER;
                _curItemBtn     = itemBtn;
            } else {
                Equipment       = GAME_ITEM.NONE;
            }
            itemBtn.IsOn        = !itemBtn.IsOn;
            Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", itemBtn.IsOn);
        } else {
            itemBtn.IsOn        = false;
            JMFUtils.GM.Pause();
            _showBuyItemPopup("firecracker");
        }

        //= NNSoundHelper.Play("touch");
    }

    void OnClickSwap (tk2dUIItem item) {

        if(false==JMFUtils.GM.IsAllBoardStable || true==mOnItemUsing)
            return;
        if(JMFUtils.GM.Pump!=JMFUtils.GM.idlePump && JMFUtils.GM.Pump!=JMFUtils.GM.failPump)
            return;

        Debugger.Assert(item != null, "PlayOverlayHandler.OnClickSwapJewel : Toggle is null.");

        // de-init view.
        if(GAME_ITEM.MAGICSWAP != Equipment)
            Equipment           = GAME_ITEM.NONE;

       // if(!Root.Data.gameData.record.playerData.MagicSwapUnlocked && !_WAYNE_TESTOR._UNLOCK_ITEM)
       //     return;

        ItemButton itemBtn      = item.GetComponent<ItemButton>();
        if(null == itemBtn)     return;
        _curItemBtn             = null;

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) {
            itemBtn.IsOn        = false;
            return;
        }

		if (Wallet.GetItemCount("magicswap")>0 || _WAYNE_TESTOR._CNT_ITEM_MAGICSWAP>0) {
            if(!itemBtn.IsOn) {
				Equipment = GAME_ITEM.MAGICSWAP;
                _curItemBtn     = itemBtn;
            } else {
                Equipment = GAME_ITEM.NONE;
            }
            itemBtn.IsOn        = !itemBtn.IsOn;
            Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", itemBtn.IsOn);
        } else {
            itemBtn.IsOn        = false;
            JMFUtils.GM.Pause();

			//Scene.ShowPopup("BuyPopup", BUY_ITEM_TYPE.MAGICSWAP, BuyCallback);
            _showBuyItemPopup("magicswap");
        }
    }

    void OnClickMagicShuffle (tk2dUIItem item) {

        if(false==JMFUtils.GM.IsAllBoardStable || true==mOnItemUsing)
            return;
        if(JMFUtils.GM.Pump != JMFUtils.GM.idlePump)
            return;

        Debugger.Assert(item != null, "PlayOverlayHandler.OnClickMagicShuffle : Toggle is null.");

        if(GAME_ITEM.MAGICSHUFFLE != Equipment)
            Equipment           = GAME_ITEM.NONE;

        //if(!Root.Data.gameData.record.playerData.RainbowBustUnlocked && !_WAYNE_TESTOR._UNLOCK_ITEM)
        //    return;

        ItemButton itemBtn      = item.GetComponent<ItemButton>();
        if(null == itemBtn)     return;
        _curItemBtn             = null;

        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) {
            itemBtn.IsOn        = false;
            return;
        }
		
		if (Wallet.GetItemCount("magicshuffle") > 0)// || _WAYNE_TESTOR._CNT_ITEM_RAINBOWBUST>0) 
        {
            if(!itemBtn.IsOn) {
				Equipment = GAME_ITEM.MAGICSHUFFLE;
                _curItemBtn     = itemBtn;
            } else {
                Equipment = GAME_ITEM.NONE;
            }
            itemBtn.IsOn        = !itemBtn.IsOn;
            Scene.SendMessage<PlaySceneHandler>("triggerDarkBGEffect", itemBtn.IsOn);
        } else {
            itemBtn.IsOn        = false;
            JMFUtils.GM.Pause();
			//Scene.ShowPopup("BuyPopup", BUY_ITEM_TYPE.RAINBOWBUST, BuyCallback);
            _showBuyItemPopup("magicshuffle");
        }
    }

    void OnClickPause (tk2dUIItem item) {
        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return; // todo 
        NNSoundHelper.Play("FX_btn_on");
        Scene.SendMessage<PlaySceneHandler>("OnEscape");
    }

    void ShowBoardMask (bool enable) {
    //    boardMask.SetActive(enable);
    }

    void BuyCallback (object param) {
        UpdateItems();
        JMFUtils.GM.Resume();
    }

    void searchTargetWithDirection(Board bd, JMF_DIRECTION direction, ref List<Board> outList)
    {
        if(null == outList)     return;

        Board targetBoard       = bd[direction];
        while (targetBoard != null) {
            if( false==outList.Contains(targetBoard) &&         // 중복 제거.
                true==JMFUtils.GM._listOnBoard.Contains(targetBoard) ) 
            {                                                   // 백 보드가 없는 영역 제거.
                outList.Add( targetBoard );
            }
            targetBoard         = targetBoard[direction];
        }
    }

    void DestroyDirection (Board bd, JMF_DIRECTION direction, float delayTime) {
        Debugger.Assert(bd != null, "PlayOverlayHandler.DestroyDirection : Board is null."); 

        Board targetBoard = bd[direction];
        if(null == targetBoard) return;

        float delay = delayTime;

        List<JMF_DIRECTION> listDirSkip = new List<JMF_DIRECTION>();
        switch(direction)
        {
        case JMF_DIRECTION.RIGHT:
        case JMF_DIRECTION.LEFT:
            listDirSkip.Add( JMF_DIRECTION.RIGHT );
            listDirSkip.Add( JMF_DIRECTION.LEFT );
            break;
        case JMF_DIRECTION.UP:
        case JMF_DIRECTION.DOWN:
            listDirSkip.Add( JMF_DIRECTION.RIGHT );
            listDirSkip.Add( JMF_DIRECTION.LEFT );
            break;
        }

        while (targetBoard != null) {

            // piece가 없거나(보드), 있으면 특수 piece 제외하고.
            if(null==targetBoard.PD || (null != targetBoard.PD && !(targetBoard.PD is SpecialFive) && !(targetBoard.PD is TMatch7Piece)) )
            {
                targetBoard.Hit(delay);
                // == JMFUtils.GM.SplashHit( targetBoard, delay, listDirSkip.Count>0 ? listDirSkip : null);
                // note : splash hit 제거. 170629
            }

            // note : cut 하지 않는 것으로 변경.
            // if (targetBoard.PD is RoundChocoPiece) break;

            targetBoard = targetBoard[direction];
            delay += delayTime;
        }
    }

    void UpdateMaskPositionAndSize (float height) {
        UpdateMaskPosition(height);
        UpdateMaskSize();
        UpdateItemTextPosition();
        UpdateWarningMesssgePosition(height);
    }

    void UpdateMaskPosition (float height) {
        Vector3 newMaskPos = orgMaskPos;
        newMaskPos.y += height;
     //   boardMask.transform.localPosition = newMaskPos;
    }

    void UpdateWarningMesssgePosition (float height) {
       // Vector3 newMaskPos = Vector3.zero;
       // newMaskPos.y = mask.transform.localPosition.y;
       // newMaskPos.y += (mask.size.y * 0.5F) + height;
       // newMaskPos.z = -1;
    }

    void UpdateMaskSize () {
        /*int maxH = 0;
        for (int x = 0; x < GameManager.WIDTH; x++) {
            List<bool> l = new List<bool>();
            for (int y = 0; y < GameManager.HEIGHT; y++) {
                l.Add(JMFUtils.GM[x,y].PND is EmptyPanel);
            }
            int minBlockIdx = l.FindIndex(empty => (empty == false));
            int maxBlockIdx = l.FindLastIndex(empty => (empty == false));
            int count = maxBlockIdx - minBlockIdx + 1;
            maxH = Mathf.Max(maxH, count);
        }

        int maxW = 0;
        for (int y = 0; y < GameManager.HEIGHT; y++) {
            List<bool> l = new List<bool>();
            for (int x = 0; x < GameManager.WIDTH; x++) {
                l.Add(JMFUtils.GM[x,y].PND is EmptyPanel);
            }
            int minBlockIdx = l.FindIndex(empty => (empty == false));
            int maxBlockIdx = l.FindLastIndex(empty => (empty == false));
            int count = maxBlockIdx - minBlockIdx + 1;
            maxW = Mathf.Max(maxW, count);
        }

        float w = maxW * JMFUtils.GM.Size;
        float h = maxH * JMFUtils.GM.Size;
        mask.size = new Vector2(w, h);
        mask.Build();*/
    }

    void UpdateItemTextPosition () {
        //Vector3 newMaskPos = Vector3.zero;
        //newMaskPos.y = mask.transform.localPosition.y;
        //newMaskPos.y += (mask.size.y * 0.5F);
        //newMaskPos.z = -1;
        ///itemText.transform.localPosition = newMaskPos;
    }

    /*
    Sequence AnimateGain (int amount) {
        if (winningConditionGainLabel == null) {
            winningConditionGainLabel = Instantiate(winningConditionLabel) as tk2dTextMesh;
            winningConditionGainLabel.transform.parent = winningConditionLabel.transform;
            winningConditionGainLabel.transform.localPosition = winningConditionLabel.transform.localPosition;
        }

//        HOTween.Complete(winningConditionGainLabel.GetInstanceID());
		DOTween.Complete(winningConditionGainLabel.GetInstanceID());
        winningConditionGainLabel.color = Color.white;
        winningConditionGainLabel.text = string.Format("+{0}", amount);
        winningConditionGainLabel.scale = Vector3.one;
        winningConditionGainLabel.Commit();
        winningConditionGainLabel.transform.localPosition = Vector3.down * 2f;

        Vector3 targetPos = winningConditionLabel.transform.localPosition;
        targetPos.y += .5f;
        float animateSec = 1f;

//        Sequence seq = new Sequence( new SequenceParms().IntId(winningConditionGainLabel.GetInstanceID()) );
//        TweenParms parms = new TweenParms().Prop("localPosition", targetPos)
//                                           .Ease(EaseType.EaseOutQuad);
//        seq.Insert(0f, HOTween.To(winningConditionGainLabel.transform, animateSec, parms));
//
//        parms = new TweenParms().Prop("color", new Color(1,1,1,0))
//                                           .Ease(EaseType.EaseInCirc);
//        seq.Insert(0f, HOTween.To(winningConditionGainLabel, animateSec, parms));
		
		Sequence seq = DOTween.Sequence();
		seq.SetId(winningConditionGainLabel.GetInstanceID());
		
		TweenParams parms = new TweenParams();
		parms.SetEase(Ease.OutQuad);
		seq.Insert(0f, winningConditionGainLabel.transform.DOLocalMove(targetPos, animateSec).SetAs(parms));

		parms = new TweenParams();
		parms.SetEase(Ease.InCirc);
		seq.Insert(0f, DOTween.To(() => winningConditionGainLabel.color, x => winningConditionGainLabel.color = x, new Color(1,1,1,0), animateSec).SetAs(parms));

        seq.Play();
        return seq;
    }*/

    void OnClickCoin (tk2dUIItem item) {
        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return;

        NNSoundHelper.Play("FX_btn_on");
        JMFUtils.GM.Pause();
        Scene.ShowPopup("StorePopup", null, (param) => { 
            UpdateItems(); 
            JMFUtils.GM.Resume();
        });
    }

    public void UpdateItems () {
        for (int i = 0; i < itemButtons.Length; i++)
            itemButtons[i].UpdateItem();
    }

    public void PlayeEffectItem(int index)
    {
        //if(index > -1 && index < itemButtons.Length)
        //    itemButtons[index].PlayEffect();
    }

	void OnClickEdit () {
#if UNITY_EDITOR
		Scene.SendMessage<PlaySceneHandler> ("CallEditScene");
#endif
	}

    void OnArrivedMissionPoint(GameObject go) {
        //= NNSoundHelper.Play("coin");
        if (go.transform.Find("Fairy") != null) {
            StartCoroutine(AbandonFairy(go, go.transform.Find("Fairy")));
        } else {
            NNPool.Abandon(go); 
        }
    }

    IEnumerator AbandonFairy(GameObject go, Transform fairy) {
        fairy.gameObject.SetActive(false);
        yield return new WaitForSeconds(1F);
        NNPool.Abandon(go); 
    }

    // Buff Gauage Relatives.
    public void onAddBuff()
    {        
        if(false == DOTween.IsTweening( _objBuffBox.transform ))
            _objBuffBox.transform.DOScale(1.1f, 0.1f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);

        _effBuffGained.Play();

        NNSoundHelper.Play("IFX_goal_earning");
        Root.Data.gameData.addBuffCount();
        
        _refreshBuffBox();
        if(Root.Data.gameData.getCountBuff() >= InfoLoader.MAX_FOX_BUFF)
        {
            StartCoroutine( _coEndLevelByBuff() );
            
            _chatBox.gameObject.SetActive( true );
            _chatBox.displayChatBox("!!!", 3.0f, true);
        }
        else
        {
            if(false == _chatBox.gameObject.activeSelf)
            {
                _chatBox.gameObject.SetActive( true );
                _chatBox.displayChatBox("...", 2.0f);
            }
        }
    }
    IEnumerator _coEndLevelByBuff()
    {
        if(_isOnFoxBuff)        yield break;

        _isOnFoxBuff            = true;
        _effBuffCharged.Play();

        float fTimeEffStarted   = Time.time;

        // wait till stable.
        while(false==JMFUtils.GM.IsAllBoardStable || IsPlayingComboText() || false==JMFUtils.GM.isDragable())
            yield return null;
        
        yield return new WaitForSeconds(0.1f);

        // is it already game cleared ??
        if(JMF_GAMESTATE.FINAL <= JMFUtils.GM.State)
        {
            _effBuffCharged.Stop();
            yield break;
        }
        
        // hold.
        while(Time.time-fTimeEffStarted < 0.5f)
        {
            yield return new WaitForSeconds(0.05f);
        }

        // 무언가 연출이 이 과정중이라는걸 알려줄 필요가 있다.
        // yield return StartCoroutine( _clearLevel() );
        _objBuffBox.transform.DOLocalMoveY(1.0f, 0.2f).SetRelative(true).SetLoops(2, LoopType.Yoyo);        
        yield return null;// WaitForSeconds(0.4f);
        _effBuffCharged.Stop();

        // rain bow effect로 replace !
        //if(fox) yield return StartCoroutine( _coRainbowBurst() );
        yield return StartCoroutine( _coProcessFoxBuff() );

        // 
        int buffCount       = Root.Data.gameData.getCountBuff() % InfoLoader.MAX_FOX_BUFF;
        Root.Data.gameData.setBuffCount( buffCount );
        _refreshBuffBox();
        _isOnFoxBuff        = false;        
    }

    void _getTargetBoards_normal(ref List<Board> outBoard)
    {
        if(null==outBoard)      return;

        List<Board> listBoards  = new List<Board>();
        foreach (Board _bd in JMFUtils.GM.Boards) 
		{
			if (_bd.IsFilled == false) continue;
			if (_bd.Piece.IsMatchable() == false) continue;
			if ((_bd.PD is NormalPiece) == false) continue;
            if (_bd.Panel.IsDestroyablePiece() == false) continue;
            if(null!=_bd.Piece && _bd.Piece.LifeCover>0)  continue;

            listBoards.Add( _bd );
        }

        int numTarget           = Mathf.Min(4, listBoards.Count);// UnityEngine.Random.Range(2, 4);
        for(int zq = 0; zq < numTarget; )
        {
            Board bbb           = listBoards[ UnityEngine.Random.Range(0, listBoards.Count) ];
            if(false == outBoard.Contains(bbb))
            {
                outBoard.Add( bbb );
                ++zq;
            }
        }
    }

    void _getTargetBoards_missoinTargets(ref List<Board> outBoard)
    {
        if(null==outBoard)      return;

        List<Board> listBoards  = new List<Board>();
        foreach (Board _bd in JMFUtils.GM.Boards) 
		{
            if(true == _bd.isLevelMissionTarget())
                listBoards.Add( _bd );
        }

        int numTarget           = Mathf.Min(COUNT_BUFF_TARGET, listBoards.Count);// UnityEngine.Random.Range(2, 4);
        for(int zq = 0; zq < numTarget; )
        {
            Board bbb           = listBoards[ UnityEngine.Random.Range(0, listBoards.Count) ];
            if(false == outBoard.Contains(bbb))
            {
                outBoard.Add( bbb );
                ++zq;
            }
        }
    }

    // 추후에 동물별 buff가 다르게 만듬. - 지금 여우는 2~3개의 랜덤 bomb.
    IEnumerator _coProcessFoxBuff()
    {
        //int numTarget           = UnityEngine.Random.Range(2, 4);

        List<Board> listActBoards  = new List<Board>();
        _getTargetBoards_missoinTargets( ref listActBoards );

        if(listActBoards.Count < COUNT_BUFF_TARGET)
            _getTargetBoards_normal( ref listActBoards );
        
        JMFUtils.GM.makingSpecialPiece   = true;

        // 떠뜨리자.
        //BlockCrash effect       = null;
        float fFullDelay        = .0f;
        //for(int q = 0; q < listBoards.Count; ++q)
        int q                   = 0;
        const float fEachFireD  = 0.1f;
        do
        {
            Board bdTarget      = listActBoards[ UnityEngine.Random.Range(0, listActBoards.Count) ];
            StartCoroutine( _coFoxFireStar(fEachFireD*(float)q, bdTarget) );
            fFullDelay          = Mathf.Max(fFullDelay, fEachFireD*(float)q+0.25f);
            ++q;
            listActBoards.Remove(bdTarget);

		}while(listActBoards.Count>0);
        //= JMFUtils.GM.SplashHit( listBoards );

        yield return new WaitForSeconds( fFullDelay + 0.5f);

        JMFUtils.GM.makingSpecialPiece   = false;
    }

    IEnumerator _coRainbowBurst()
    {
        JMFUtils.GM.itemUsing   = true;

        NNSoundHelper.Play("IFX_block_match_01");
        
        // target color search.
        List<int> listColor     = new List<int>();
        foreach (Board _bd in JMFUtils.GM.Boards) 
		{
			if (_bd.IsFilled == false) continue;
			if (_bd.Piece.IsMatchable() == false) continue;

            if(false == listColor.Contains(_bd.Piece.ColorIndex))
                listColor.Add( _bd.Piece.ColorIndex );
        }

        // random target setting.
        int targetColorIndex    = listColor[ UnityEngine.Random.Range(0, listColor.Count) ];
		
        // block 대상 특정.
        List<Board> listBoards  = new List<Board>();
        foreach (Board _bd in JMFUtils.GM.Boards) 
		{
			if (_bd.IsFilled == false) continue;
			if (_bd.Piece.IsMatchable() == false) continue;
			if (_bd.Piece.ColorIndex != targetColorIndex) continue;
			
            listBoards.Add( _bd );
        }

        // 커튼 처리.
        //AiCurtainReady( ref listBoards, 0.8f );
        // process char magic fire.
        //yield return JMFUtils.GM.fireItemMagicAnimation(JMFUtils.GM[pt]);
        JMFUtils.GM.makingSpecialPiece   = true;
        // 떠뜨리자.
        //BlockCrash effect       = null;
        float fFullDelay        = .0f;
        //for(int q = 0; q < listBoards.Count; ++q)
        int q                   = 0;
        const float fEachFireD  = 0.1f;
        do
        {
            Board bdTarget      = listBoards[ UnityEngine.Random.Range(0, listBoards.Count) ];
            StartCoroutine( _coFoxFireStar(fEachFireD*(float)q, bdTarget) );
            fFullDelay          = Mathf.Max(fFullDelay, fEachFireD*(float)q+0.25f);
            ++q;
            listBoards.Remove(bdTarget);

		}while(listBoards.Count>0);
        //= JMFUtils.GM.SplashHit( listBoards );

        yield return new WaitForSeconds( fFullDelay + 0.5f);
        //AiCurtainClose();
        //mOnItemUsing            = false;

        JMFUtils.GM.makingSpecialPiece   = false;
    }

    IEnumerator _coFoxFireStar(float fDelay, Board bd)
    {
        //FlyParticle FR          = NNPool.GetItem<FlyParticle>("FlyBallFox");
        //FR.Play(null, _objBuffBox.transform.position, bd.Position, 0.25f);
        yield return new  WaitForSeconds(fDelay);

        NNSoundHelper.Play("IFX_fire_and_fly");

        SpriteRenderer SR       = NNPool.GetItem<SpriteRenderer>("sprFlyingObj");
        SR.sprite               = SR.GetComponent<SpriteLibrary>().getSpriteByName("flyBird");
        SR.transform.position   = _objBuffBox.transform.position;
        SR.transform.localScale = Vector3.one * 1.2f;
        SR.GetComponent<SpriteRenderer>().sortingOrder    = 11;

        //SR.transform.DOMove( bd.Position, 0.25f);
        float d = JMFUtils.tween_move(SR.transform, SR.transform.position, bd.Position, 14.0f);
        yield return new WaitForSeconds(d);

		BlockCrash effect= NNPool.GetItem<BlockCrash>("NormalPieceCrash");
        effect.Play("play", bd.Position, Vector3.one, 1);// bd.Piece.Scale, bd.Piece.ColorIndex);
		bd.Hit(.0f);

        SR.transform.DOKill();
        SR.transform.localScale = Vector3.one;
        NNPool.Abandon(SR.gameObject);
    }

    IEnumerator _clearLevel()
    {
        do
        {
            for(int z = 0; z < (int)LEItem.COLOR.NORMAL_COUNT; ++z)
            {
                if(JMFUtils.GM.CurrentLevel.numToGet[z] > 0)
                {
                    JMFUtils.GM.JewelMatchCounts[z]++;
                    int remainCount     = JMFUtils.GM.CurrentLevel.numToGet[z] - JMFUtils.GM.JewelMatchCounts[z];
                    if(remainCount >= 0)
                        JMFRelay.FireOnCollectJewelForDisplay(z, remainCount);
                }
            }

            _fireGoalObject(JMFUtils.GM.CurrentLevel.countRoundChocho, ref JMFUtils.GM.countMatchRoundChocho, "RoundChoco" );
            
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countRectChocho, ref JMFUtils.GM.countMatchRectChocho, "RectChoco" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countCottonCandy, ref JMFUtils.GM.countMatchCottonCandy, "CottonCandy" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countSodaCan, ref JMFUtils.GM.countMatchSodaCan, "SodaCan" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countSugarBlock, ref JMFUtils.GM.countMatchSugarBlock, "SugarBlock" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countZellatto, ref JMFUtils.GM.countMatchZellatto, "Zellato" );            
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countChocoBar, ref JMFUtils.GM.countMatchChocoBar, "ChocoBar" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countCookieJelly, ref JMFUtils.GM.countMatchCookieJelly, "CookieJelly" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countColorBox, ref JMFUtils.GM.countMatchColorBox, "ColorBox" );
            _fireGoalObject(JMFUtils.GM.CurrentLevel.countWaffleCooker, ref JMFUtils.GM.countMatchWaffleCooker, "WaffleCooker" );

            _fireTreasureObject(JMFUtils.GM.CurrentLevel.countPotion1, ref JMFUtils.GM.countMatchPotion1, TREASURE_TYPE.POTION1);
            _fireTreasureObject(JMFUtils.GM.CurrentLevel.countPotion2, ref JMFUtils.GM.countMatchPotion2, TREASURE_TYPE.POTION2);
            _fireTreasureObject(JMFUtils.GM.CurrentLevel.countPotion3, ref JMFUtils.GM.countMatchPotion3, TREASURE_TYPE.POTION3);

            _fireShadeObject(JMFUtils.GM.CurrentLevel.countJamBottom, ref JMFUtils.GM.countMatchJamBottom, LEItem.SHADE_TYPE.JAM );
            _fireShadeObject(JMFUtils.GM.CurrentLevel.countCursedBottom, ref JMFUtils.GM.countMatchCursedBottom, LEItem.SHADE_TYPE.CURSE );
            _fireShadeObject(JMFUtils.GM.CurrentLevel.countMudShade, ref JMFUtils.GM.countMatchMudShade, LEItem.SHADE_TYPE.MUD_COVER );


            yield return new WaitForSeconds(0.1f);

        }while(false==JMFUtils.GM.IsClearedLevel(JMFUtils.GM.CurrentLevel));

        // 연출 좀 넣고...
        JMFUtils.GM.isLevelClearByBuff  = true;

        _refreshBuffBox();

        Debug.Log("Buff Full !!!");
     
    }
    void _fireGoalObject(int missionCount, ref int matchedCount, string strId)
    {
        if(missionCount > 0)
        {
            matchedCount++;
            int remainCount     = missionCount - matchedCount;
            if(remainCount >= 0)
				JMFRelay.FireOnCollectMissionObjectForDisplay(strId, remainCount);
        }
    }
    void _fireShadeObject(int missionCount, ref int matchedCount, LEItem.SHADE_TYPE eType)
    {
        if(missionCount > 0)
        {
            matchedCount++;
            int remainCount     = missionCount - matchedCount;
            if(remainCount >= 0)
				JMFRelay.FireOnChangeRemainShadeForDisplay(remainCount, (int)eType);
        }
    }
    void _fireTreasureObject(int missionCount, ref int matchedCount, TREASURE_TYPE eType)
    {
        if(missionCount > 0)
        {
            matchedCount++;
            int remainCount     = missionCount - matchedCount;
            if(remainCount >= 0)
				JMFRelay.FireOnCollectTreasureForDisplay(eType,  remainCount);
        }
    }
    void _refreshBuffBox()
    {
        int curCount            = Mathf.Min(Root.Data.gameData.getCountBuff(), InfoLoader.MAX_FOX_BUFF);
        // 80 ~ 247
        float fWidth            = (247.0f-80.0f) * ((float)curCount) / ((float)InfoLoader.MAX_FOX_BUFF);
        _sprBuffGauge.dimensions= new Vector2(80.0f + fWidth, _sprBuffGauge.dimensions.y);
    }

    public void AnimateGainBuff(object obj, System.Action onComplete)
    {
        Debugger.Assert(obj != null, "PlayOverlayHandler.AnimateGainBuff : Items are null.");

        object[] objs           = (object[])obj;
        Vector3 vPos            = (Vector3)objs[0];     // start position.
        GameObject objFlying    = (GameObject)objs[1];  // flying game-object.

        float yDest             = .0f;
        const float deepz       = -100.0f;
        float yOffset           = 0.6f;
        Vector3 targetPos       = _objBuffBox.transform.position;
        float fTargetX          = vPos.x;
        Vector3 v1              = new Vector3(fTargetX, targetPos.y+yOffset, deepz);
        Vector3 v2              = new Vector3(targetPos.x, targetPos.y+yDest-0.65f, deepz);
            
        float duration          = UnityEngine.Random.Range(1.2f, 1.6f);
        objFlying.transform.position    = new Vector3(vPos.x, vPos.y, -100.0f);
        objFlying.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(_gainObjectCurve);// Ease.InSine);//.OutCubic);
        //objFlying.transform.DOScale(objFlying.transform.localScale*0.8f, duration).SetEase(Ease.OutQuad);

        DOVirtual.DelayedCall(duration, () =>
        {
            //NNSoundHelper.Play("IFX_goal_earning");

            if(onComplete!=null) onComplete();
            objFlying.transform.localScale = Vector3.zero;
            NNPool.Abandon(objFlying);  
        });
    }

    public void AnimateGainMissionItem (object obj, System.Action onComplete) 
	{
        // temp !!!
        //if (onComplete != null) onComplete();
        //return;
        //

        Debugger.Assert(obj != null, "PlayOverlayHandler.AnimateGainMissionItem : Items are null.");

#region // spine ani로 대처됨. ...... old code => comments.
        /*float speed = 0.7F;
        float ySpeed = 0.3F;
        float toScale = 2F;

        if (JMFUtils.GM.CurrentLevel.isGetTypesGame) 
		{
            speed = (JMFUtils.GM.animationCount / 10F) + 0.5F;
            ySpeed = (JMFUtils.GM.animationCount / 10F) + 0.2F;
            if (speed >= 0.8F) speed = 0.8F;
            if (ySpeed >= 0.5F) ySpeed = 0.5F;
            toScale = 1.3F;
        }

        object[] objs = (object[])obj;
        GameObject go = (GameObject)objs[0];
        Vector3 rawV = Scene.SceneCamera.WorldToScreenPoint(go.transform.position);
		Vector3 departurePos = Camera.main.ScreenToWorldPoint(rawV);

        Vector3 targetPos = Vector3.zero;
        string missionName = (string)objs[1];
		
//        switch (missionName) 
//		{
////        case "Jewel0" :
////            targetPos = mission.targets[2].GetComponent<JewelMission>().iconA.transform.position;
////            break;
////        case "Jewel1" :
////            targetPos = mission.targets[2].GetComponent<JewelMission>().iconB.transform.position;
////            break;
////        case "Jewel2" :
////            targetPos = mission.targets[2].GetComponent<JewelMission>().iconC.transform.position;
////            break;
////        case "Penguin" :
////            targetPos = mission.targets[5].GetComponent<PenguinMission>().icon.transform.position;
////            break;
////        case "Potion1" :    targetPos   = mission.targets[3].GetComponent<CollectMission>().iconA.transform.position;   break;
////        case "Potion2" :    targetPos   = mission.targets[3].GetComponent<CollectMission>().iconB.transform.position;   break;
////        case "Potion3" :    targetPos   = mission.targets[3].GetComponent<CollectMission>().iconC.transform.position;   break;
////        case "Fairy" :
////            targetPos = mission.targets[7].GetComponent<FairyMission>().icon.transform.position;
////            break;
////        case "Shaded" :
////            targetPos = mission.targets[0].GetComponent<ShadeMission>().icon.transform.position;
////            break;
//        case "RoundChoco":  // [ROUND_CHOCO]
//        case "JamShade":    // [JAM_SHADE]
//        case "CurseShade":  // [CURSE_SHADE]
//        case "RectChoco":   // [RECT_CHOCO]
//        case "ChocoBar":    // [CHOCO_BAR], [NET_SHADE]
//        case "CottonCandy": // 
//        case "SodaCan":     // [SODA_CAN]
//        case "SugarBlock":  // [SUGAR_BLOCK]
//        case "Zellato":     // [ZELLATO]
//        {
//            GameObject objIcon  = mission.targets[0].GetComponent<CollectAnyMission>().getIcon(missionName);
//            if(null == objIcon) break;
//            targetPos           = objIcon.transform.position;
//            break;
//        }
//        default:                break;
//        }   // end of switch.

		
        //GameObject objIcon  = collectAnyMission.getIcon(missionName);
		//if(null == objIcon)
        //{
        //    if(null != onComplete)
        //        onComplete();
        //    return;
        //}
		//targetPos = objIcon.transform.position;
        // note : 가운데 조리 재료로 가는 것으로 목표 위치 변경.
        targetPos               = _objGoalFood.transform.position + Vector3.back;
        departurePos.z          = targetPos.z - 1.0f;
        go.transform.position   = departurePos;
        Vector3 mYPos = go.transform.position;
        mYPos.y -= 1.2F;

        if (missionName.Equals("Fairy") || missionName.Equals("Shaded")) 
		{
            go.SetActive(true);
            go.transform.Find("effect").gameObject.SetActive(true);
            float startTime = UnityEngine.Random.Range(.0f,.2f);
            float fairyTime = UnityEngine.Random.Range(1f, 1.5f);
            float time1 = UnityEngine.Random.Range(0.7f, 0.9f);

            Vector3 originalScale = go.transform.localScale;
			Sequence seq = DOTween.Sequence();

            Vector3 position1 = go.transform.position;

            Vector3[] fairyPath = new Vector3[4];
            fairyPath[0] = position1;
            if (position1.x < 0) 
			{
                fairyPath[1] = new Vector3(position1.x+2f,position1.y-2f, position1.z);
                fairyPath[2] = new Vector3(fairyPath[1].x+1f,fairyPath[1].y+1, fairyPath[1].z);
                fairyPath[3] = new Vector3(fairyPath[2].x-1f,fairyPath[2].y+1, fairyPath[2].z);
            } 
			else 
			{
                fairyPath[1] = new Vector3(position1.x-2f,position1.y-2f, position1.z);
                fairyPath[2] = new Vector3(fairyPath[1].x-1f,fairyPath[1].y+1, fairyPath[1].z);
                fairyPath[3] = new Vector3(fairyPath[2].x+1f,fairyPath[2].y+1, fairyPath[2].z);
            }

			TweenParams parms = new TweenParams();
			parms.SetEase(Ease.OutQuad);
			parms.OnComplete( () => { NNSoundHelper.Play("match_butterfly");});
			seq.Insert(startTime, go.transform.DOScale( originalScale * 1.2f, .3f).SetAs(parms));
			
            go.transform.localScale = originalScale * 1.7F;
			
			parms = new TweenParams();
			parms.SetEase(Ease.InOutSine);
			seq.Insert(startTime, go.transform.DOPath( fairyPath,fairyTime).SetAs(parms));
			parms = new TweenParams();
			parms.SetEase(Ease.InQuad);
			seq.Insert(fairyTime+startTime, go.transform.DOScale(originalScale, fairyTime+time1).SetAs(parms));
			parms = new TweenParams();
			parms.SetEase(Ease.InOutSine);
			seq.Insert(fairyTime+startTime, DOTween.To(() => go.transform.position, x => go.transform.position = x, targetPos, time1).SetAs(parms));
			
            seq.Play();

			Sequence seq2 = DOTween.Sequence();
			seq2.OnComplete(() => { 
                OnArrivedMissionPoint(go); 
                if (onComplete != null) onComplete(); 
            });
			
            seq2.AppendInterval(startTime+fairyTime+time1);
            //seq2.Play();        
        }
		else 
		{
            Vector3 originalScale = go.transform.localScale;
			
			Sequence seq = DOTween.Sequence();
			seq.OnComplete(() => { 
				if (onComplete != null) onComplete(); 
			});
			seq.Insert(0f, go.transform.DOScale( originalScale * toScale, .3f).SetEase(Ease.OutQuad));

			if (JMFUtils.GM.CurrentLevel.isGetTypesGame) 
				seq.Insert(0f, DOTween.To(() => go.transform.position, x => go.transform.position = x, mYPos, ySpeed).SetEase(Ease.InQuad));
			
            seq.Insert(0f, DOTween.To(() => go.transform.position, x => go.transform.position = x, mYPos, ySpeed).SetEase(Ease.InQuad));

            seq.Insert(ySpeed, DOTween.To(() => go.transform.position, x => go.transform.position = x, targetPos, speed).SetEase(Ease.InQuad).OnComplete(() => { OnArrivedMissionPoint(go); }));
			seq.Insert(ySpeed, go.transform.DOScale(originalScale * 0.7F, .7f).SetEase(Ease.InQuad));
            //seq.Play();
        }*/
#endregion

        //
        object[] objs           = (object[])obj;
        Vector3 vPos            = (Vector3)objs[0];     // start position.
        string strSpriteName    = (string)objs[1];      // image name.
        GameObject objFlying    = (GameObject)objs[2];  // flying game-object.
        GameObject objMission   = (GameObject)objs[3];  // UI Goal Mission object.
        float fDistFromCenter   = Camera.main.WorldToScreenPoint(vPos).x - Camera.main.pixelWidth/2;
        Vector3 vScale          = (Vector3)objs[4];

		bool dirLeft            = fDistFromCenter < .0f; 
        Vector3 startPos        = vPos;
        Vector3 targetPos       = objMission ? objMission.transform.position : Vector3.zero;
                                  //_objGoalFood.transform.position + Vector3.up*1.0f;

        float duration          = 1.0f;
        const float deepz       = -100.0f;
            
        Vector3 vBubble         = Vector2.zero;
        if(null != objFlying)
        {
            objFlying.transform.localScale  = vScale;
            MeshRenderer mr     = objFlying.GetComponent<MeshRenderer>();
            if(null != mr)      mr.material.color   = Color.white;
        }
        
       /* if(strSpriteName.Contains("Panel_chocobar") || strSpriteName.Contains("potion_"))
        {
            float fSpeed        = _fGainPotionSpeed;    
            const float XXX     = 2.5f;     const float YYY     = 1.35f;

            vBubble             = dirLeft ? targetPos + new Vector3(-XXX, YYY, deepz) : targetPos + new Vector3(XXX, YYY, deepz);
            objFlying.transform.position    = new Vector3(startPos.x, startPos.y, -100.0f);
            duration            = Vector2.Distance(vBubble, startPos) / fSpeed;

            Vector3 v1          = dirLeft ? startPos + new Vector3(-0.5f, -0.5f, deepz) : startPos + new Vector3(0.5f, -0.5f, deepz);
            
            float fScaleTime    = 0.15f;
            Sequence seq        = DOTween.Sequence();
            // 해당지점에서 살짝 커졌다 작아지며, 아래 방향으로 완만한 곡선을 그리며 목표점으로 올라간다.
		    seq.Append( objFlying.transform.DOScale(objFlying.transform.localScale*1.1f, fScaleTime).SetLoops(2, LoopType.Yoyo) );
            seq.Append( objFlying.transform.DOPath(new Vector3[]{ v1, vBubble}, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.OutQuad) );
            seq.Join( objFlying.transform.DOScale(objFlying.transform.localScale*0.8f, duration));
            duration += fScaleTime*2;

            if(strSpriteName.Contains("potion_1"))      strSpriteName   = "bubble_potion1";
            else if(strSpriteName.Contains("potion_2")) strSpriteName   = "bubble_potion2";
            else if(strSpriteName.Contains("potion_3")) strSpriteName   = "bubble_potion3";
            else strSpriteName  = "bubble_chocobar";
        }
        else*/
        {
            float fSpeed        = _fGainObjectSpeed;
            float yOffset       = 0.6f;
            float yDest         = .0f;

            // 해당 포지션의 x 값으로 특정 비율내에 위치토록 목표점을 재조정한다.
            //float fRate         = (vPos.x-JMFUtils.GM[0].Position.x) / (JMFUtils.GM[8].Position.x-JMFUtils.GM[0].Position.x);
            //const float fWidth  = 5.0f;
            //float fTargetX      = -fWidth*0.5f + fWidth*fRate;
            // == 그냥 원래 위치에서 내려오는 것으로 수정.
            float fTargetX      = vPos.x;

            /*if(strSpriteName.Contains("bubble_")) 
            {
                NNSoundHelper.Play("IFX_bubble_earning");

                //yDest += 0.7f;
                //BubbleParticle  pc = NNPool.GetItem<BubbleParticle>("BubbleParticle");
                //if(null != objFlying)   NNPool.Abandon( objFlying.gameObject );
                //pc.play( strSpriteName );
                //objFlying       = pc.gameObject;
            }*/

            Vector3 v1          = new Vector3(fTargetX, targetPos.y+yOffset, deepz);
            Vector3 v2          = new Vector3(targetPos.x, targetPos.y+yDest, deepz);
            duration            = Vector2.Distance(targetPos, startPos) / fSpeed;
            objFlying.transform.position    = new Vector3(startPos.x, startPos.y, -100.0f);
            objFlying.transform.DOPath(new Vector3[]{ v1, v2 }, duration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(_gainObjectCurve);// Ease.InSine);//.OutCubic);
            objFlying.transform.DOScale(objFlying.transform.localScale*0.8f, duration).SetEase(Ease.OutQuad);
        }

#region -> old code. commented.
        /* if(strSpriteName.Contains("bubble_"))    // odacan" || strSpriteName=="bubble_Strawberry")
         {
             GainMissionParticleEff  effect  = NNPool.GetItem<GainMissionParticleEff>("GainMissionParticleEffect");
             duration            = effect.Play(strSpriteName, startPos, targetPos, .0f);
         }
         else if(strSpriteName.Contains("Panel_chocobar"))
         {
             GainMissionChocobarEff effect  = NNPool.GetItem<GainMissionChocobarEff>("GainMissionChocobarEffect");
             duration            = effect.Play(strSpriteName, true, startPos, targetPos, false, .0f);
         }
         else if(strSpriteName.Contains("potion_"))
         {
             // 새로운 spine이 들어오면 그때 처리.
             duration            = 1.0f;
         }
         else
         {
             GainMissionEff      effect   = NNPool.GetItem<GainMissionEff>("GainMissionEffect");
             duration            = effect.Play(strSpriteName, dirLeft, startPos, targetPos, false, .0f);            
         }*/
#endregion

        StartCoroutine( _coFinishGainMission(duration, objFlying, strSpriteName, onComplete, vBubble, targetPos) );
    }

    IEnumerator _coFinishGainMission(float duration, GameObject objFlying, string strPicName, System.Action onComplete, Vector2 vMidPos, Vector2 targetPos)
    {
        yield return new WaitForSeconds(duration);
        
        float fFadeOutDelay     = 0.0f;
        if(vMidPos.Equals( Vector2.zero))
        {
            /*SpineEffect eff     = NNPool.GetItem<SpineEffect>("get_hit_effect");
            eff.play("play", .0f);
            eff.GetComponent<MeshRenderer>().sortingOrder    = 11;
            eff.transform.position  = new Vector3(targetPos.x, targetPos.y, -100.0f);
            eff.transform.localEulerAngles  = new Vector3(.0f, .0f, UnityEngine.Random.Range(0, 360));
              */  
            if (onComplete != null) onComplete();

            // 타이밍 문제로 소멸 별도 처리.
            if(null != objFlying)
            {
                ParticleSystem  pc  = objFlying.GetComponent<ParticleSystem>();
                if(null != pc)
                {
                    yield return new WaitForSeconds(0.5f);  // 다 도달 할때까지 기다린다.
                    pc.Stop();
                    yield return new WaitForSeconds(0.5f);  // 서서히 소멸할때까지 기다린다.
                    NNPool.Abandon(objFlying);  
                    objFlying       = null;
                }
            }
        }
        else
        {
            if(null != objFlying)
            {
                // 쭈그러 들음.
         /*       objFlying.transform.DOScale(objFlying.transform.localScale*0.8f, 0.2f);
                objFlying.transform.DOShakePosition(0.3f, 0.1f);
                yield return new WaitForSeconds(0.2f);

                NNSoundHelper.Play("IFX_syrup_eff");

                // 확 펴짐.
                objFlying.transform.DOScale(objFlying.transform.localScale*1.4f, 0.1f);
                fFadeOutDelay       = 0.18f;
            
                // 쏟아붓는 파티클 터짐.
                SpineEffect eff     = NNPool.GetItem<SpineEffect>("chocobar_potion_move");
                Slot _slot;
                // update texture.
                const int countBubble   = 50;
                for(int q = 0; q < countBubble; ++q)
                {
                    string strSlot      = string.Format("bubble{0}", q+1);
                  //  _slot               = eff.getSA().Skeleton.FindSlot( strSlot );
                  //  if(null == _slot)   continue;
                  //  Attachment att      = eff.getSA().Skeleton.GetAttachment(eff.getSA().Skeleton.Data.FindSlotIndex(strSlot), strPicName );
                  //  if(null!= att)      _slot.attachment    = att;
                }
                //

                if (vMidPos.x < .0f)    eff.play("play_L", .0f);
                else                    eff.play("play_R", .0f);
                eff.GetComponent<MeshRenderer>().sortingOrder    = 11;
                eff.transform.position  = new Vector3(vMidPos.x, vMidPos.y, -100.0f);
                */
                //
            }
            //

            DOVirtual.DelayedCall(1.0f, () =>
            {
                /*SpineEffect eff2= NNPool.GetItem<SpineEffect>("get_hit_effect");
                eff2.play("play", .0f);
                eff2.GetComponent<MeshRenderer>().sortingOrder    = 11;
                eff2.transform.position         = new Vector3(targetPos.x, targetPos.y, -100.0f);
                eff2.transform.localEulerAngles = new Vector3(.0f, .0f, UnityEngine.Random.Range(0, 360));
                */
                if (onComplete != null) onComplete();
            });
        }

        //yield return new WaitForSeconds(0.2f);

        if(null != objFlying)
        {
            //objFlying.GetComponent<MeshRenderer>().material.color.DOFade(.0f, fFadeOutDelay).OnComplete( () => 
            DOVirtual.DelayedCall(fFadeOutDelay, () => 
            {
                MeshRenderer mr = objFlying.GetComponent<MeshRenderer>();
                if(null != mr)  mr.sortingOrder    = 0;
                SpriteRenderer  sr= objFlying.GetComponent<SpriteRenderer>();
                if(null!= sr)   sr.sortingOrder     = 0;
                objFlying.transform.position        = Vector3.zero;
                NNPool.Abandon(objFlying);                
            });
        }
	    // 
    }

    public void fire_AiTurn(bool myTurn, bool rainbowBonus)
    {
        if(false == JMFUtils.GM.isAIFightMode)
            return;

        StartCoroutine( _coFireAiTurn(myTurn, rainbowBonus) );
    }

    IEnumerator _coFireAiTurn(bool myTurn, bool rainbowBonus)
    {
        yield break;
        /*
        Debug.Assert(null != _skFxAiTurn);

        yield return new WaitForSeconds(0.2f);

        while(this.IsPlayingComboText())
            yield return null;
        while(false == JMFUtils.GM.IsCharPopFinished())
            yield return null;

        _skFxAiTurn.gameObject.SetActive( true );
        Director.Instance.showMeshNextFrame( _skFxAiTurn.GetComponent<MeshRenderer>() );

        string mapName;
        if(myTurn)
        {
            if(rainbowBonus)    mapName = "your_bonus";
            else                mapName = "your_turn";
        }
        else
        {
            if(rainbowBonus)    mapName = "enby_bonus";
            else                mapName = "envy_turn";
        }

        NNSoundHelper.Play("PFX_warning");

        // update texture.
        //_skFxAiTurn.Initialize( true );
        Slot slot               = _skFxAiTurn.Skeleton.FindSlot("turn_bonus");
        Attachment att          = _skFxAiTurn.Skeleton.GetAttachment(_skFxAiTurn.Skeleton.Data.FindSlotIndex("turn_bonus"), mapName );
        if(null!= att)          slot.attachment     = att;
        Spine.Animation ani     = _skFxAiTurn.skeleton.Data.FindAnimation("show");
		_skFxAiTurn.AnimationState.SetAnimation(0, ani, false);
        DOVirtual.DelayedCall( ani.duration, () => _skFxAiTurn.gameObject.SetActive( false ) );
        */
    }

    List<Transform>             _listMaskingBlk = new List<Transform>();
    public void AiCurtainReady(ref List<Board> listBoards, float fDuration)
    {
        //_objAiCurtain.SetActive(true);
        //_objAiCurtain.GetComponent<MeshRenderer>().material.color   = new Color(0, 0, 0, 0);
        //_objAiCurtain.GetComponent<MeshRenderer>().material.DOFade(0.7f, fDuration);

        _listMaskingBlk.Clear();

        for(int k = 0; k < listBoards.Count; ++k)
        {
            Transform trMask    = NNPool.GetItem<Transform>("maskBlock");
          //  trMask.SetParent(_objAiCurtain.transform.parent);
          //  trMask.position     = new Vector3(listBoards[k].Position.x, listBoards[k].Position.y, _objAiCurtain.transform.position.z+0.1f);
            _listMaskingBlk.Add( trMask );
        }
        //trMask                  = NNPool.GetItem<Transform>("maskBlock");
        //trMask.SetParent(_objAiCurtain.transform.parent);
        //trMask.position         = new Vector3(to.Position.x, to.Position.y, _objAiCurtain.transform.position.z+0.1f);
        //_listMaskingBlk.Add( trMask );       
    }
    public void AiCurtainClose()
    {
        for(int k = 0; k < _listMaskingBlk.Count; ++k)
            NNPool.Abandon( _listMaskingBlk[k].gameObject );

        //_listMaskingBlk.Clear();
        //_objAiCurtain.SetActive(false);
    }
}
