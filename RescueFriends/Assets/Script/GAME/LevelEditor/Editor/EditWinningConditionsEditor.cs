using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonFx.Json;
using NOVNINE.Diagnostics;

[CustomEditor (typeof(EditWinningConditions))]
public class EditWinningConditionsEditor : Editor {
    string[] mode = new string[] { "MOVE", "TIMER" };
	string[] mission_type = new string[] { "COLLECT", "FILL", "DEFEAT (AI)", "FIND", "CLEAR", "SCORE", "OLD_TYPE" };
	
    public string[] colors = new string[] { "Red", "Yellow", "Green", "Blue", "Purple", "Orange", "SkyBlue", "Violet" };
        
    string[] mysteryType = new string[] { "Line", "Bomb", "Rainbow", "Line@Ice", "Bomb@Ice", "Rainbow@Ice", "Snow1", "Snow2", "Snow3", "Snow4", "Snow5", "SnowMan", "SnowManGenerator", "Penguin", "SpiralSnow", "TimeBomb", "Camelon", "XCage"};
    public string[] specialJewelCombineStr = new string[] { "Line", "Line+Line", "Line+Bomb", "Line+Rainbow", "Bomb", "Bomb+Bomb", "Bomb+Rainbow", "Rainbow", "Rainbow+Rainbow"};
    string[] yetiAttackType = new string[] { "Snow1", "Snow2", "Snow3", "Snow4", "Snow5", "SnowMan", "Penguin", "SpiralSnow", "TimeBomb" };
    enum PLAY_LIMITATION { TIME, MOVE, COUNT };
    const int BLOCK_PROBABILITY_INTERVAL = 60;

    class ProbabilityPreset {
        public class Entry {
            public string name;
            public int[] prob;
        }
        public Entry[] mysteryPieceConvertProbability;
        public Entry[] attackConvertProbability;
        public Entry[] bossAttackConvertProbability;
        public string[] mysteryPieceConvertProbabilityNames;
        public string[] attackConvertProbabilityNames;
        public string[] bossAttackConvertProbabilityNames;
        public int mysteryPieceConvertProbabilityIdx;
        public int attackConvertProbabilityIdx;
        public int bossAttackConvertProbabilityIdx;
        public void Init() {
            mysteryPieceConvertProbabilityNames = mysteryPieceConvertProbability.Select(x=> x.name).ToArray();
            attackConvertProbabilityNames = attackConvertProbability.Select(x=> x.name).ToArray();
            bossAttackConvertProbabilityNames = bossAttackConvertProbability.Select(x=> x.name).ToArray();
        }

        public void UpdateIdx(EditWinningConditions target) {
            mysteryPieceConvertProbabilityIdx = 0;

            for (int i = 0; i < mysteryPieceConvertProbability.Length; ++i) {
                if(Enumerable.SequenceEqual(mysteryPieceConvertProbability[i].prob, target.mysteryPieceConvertProbability)) {
                    mysteryPieceConvertProbabilityIdx = i;
                    break;
                }
            }

            attackConvertProbabilityIdx = 0;
            for (int i = 0; i < attackConvertProbability.Length; ++i) {
                if(Enumerable.SequenceEqual(attackConvertProbability[i].prob, target.attackConvertProbability)) {
                    attackConvertProbabilityIdx = i;
                    break;
                }
            }

            bossAttackConvertProbabilityIdx = 0;
            for (int i = 0; i < bossAttackConvertProbability.Length; ++i) {
                if(Enumerable.SequenceEqual(bossAttackConvertProbability[i].prob, target.bossAttackConvertProbability)) {
                    bossAttackConvertProbabilityIdx = i;
                    break;
                }
            }
        }
    }

    ProbabilityPreset preset;
    ProbabilityPreset Preset {
        get {
            if(preset == null) {
                string buf = System.IO.File.ReadAllText(Application.dataPath+"/Script/GAME/LevelEditor/Editor/winningConditionPreset.txt");
			    preset = JsonReader.Deserialize<ProbabilityPreset>(buf);
                Debugger.Assert(preset != null);
                preset.Init();
            }
            return preset;
        }
    }

    int gameModeIndex = 0;
    public override void OnInspectorGUI() {
        EditWinningConditions _target = (EditWinningConditions)target;
        Preset.UpdateIdx(_target);

        DrawGameMode(_target);
		DrawTargetScore(_target);
		EditorGUILayout.Space();
		DrawNormalProbability(_target);
		EditorGUILayout.Space();
		DrawRoundChoco(_target);    // [ROUND_CHOCO]
		EditorGUILayout.Space();
		DrawGreenBubble(_target);   // [GREEN_BUBBLE]
		EditorGUILayout.Space();
		DrawStrawberry(_target);
		EditorGUILayout.Space();
		DrawSugarCherry(_target);   // [SUGAR_CHERRY]
		EditorGUILayout.Space();
		DrawZellato(_target);       // [ZELLATO]
		EditorGUILayout.Space();
        DrawAddTimeBlock(_target); // [Add Time Block]
		EditorGUILayout.Space();
		//DrawTimeBomb(_target);
		//EditorGUILayout.Space();
		//DrawPenguinSpawn(_target);
		//EditorGUILayout.Space();
		//DrawFairy(_target);
		//EditorGUILayout.Space();

		//DrawMystery(_target);
		//EditorGUILayout.Space();
		//DrawChameleon(_target);
		//EditorGUILayout.Space();
		//DrawCurrentInfo(_target);	
    }

    void _addColorIntField(LEItem.COLOR eColor, ref EditWinningConditions wc)
    {
        int idx                 = (int)eColor - 1;
        if(idx<0 || idx>=colors.Length)
            return;

        // note : display view 용 조금 다른 이름을 쓰자.
        string strName                  = colors[idx];
        if(colors[idx]=="Violet")       strName = "Purple";
        else if(colors[idx]=="Purple")  strName = "Pink";
        else if(colors[idx]=="SkyBlue") strName = "Mint";

        wc.numToGet[idx]        = EditorGUILayout.IntField(strName, wc.numToGet[idx]);
    }

    void _addColorSlider(LEItem.COLOR eColor, ref EditWinningConditions wc)
    {
        int idx                 = (int)eColor - 1;
        if(idx<0 || idx>=colors.Length)
            return;

        // note : display view 용 조금 다른 이름을 쓰자.
        string strName                  = colors[idx];
        if(colors[idx]=="Violet")       strName = "Purple";
        else if(colors[idx]=="Purple")  strName = "Pink";
        else if(colors[idx]=="SkyBlue") strName = "Mint";

        wc.normalProbability[idx] = EditorGUILayout.IntSlider(strName, wc.normalProbability[idx], 0, 100);
    }

    void DrawNormalProbability (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.PrefixLabel("Probability");

        EditorGUI.indentLevel++;
        //for (int i = 0; i < colors.Length; i++) {
        //    if(_target.normalProbability.Count <= i)
        //        _target.normalProbability.Add( 0 );
            // 여기서만 지정 순서를 사용한다. not index order.
            //_target.normalProbability[i] = EditorGUILayout.IntSlider(colors[i], _target.normalProbability[i], 0, 100);
        //}

        // extend list.
        do{
            if(_target.normalProbability.Count < colors.Length)
                _target.normalProbability.Add(100);
            else break;
        }while(true);

        _addColorSlider(LEItem.COLOR.RED, ref _target);
        _addColorSlider(LEItem.COLOR.ORANGE, ref _target);
        _addColorSlider(LEItem.COLOR.YELLOW, ref _target);
        _addColorSlider(LEItem.COLOR.GREEN, ref _target);
        _addColorSlider(LEItem.COLOR.BLUE, ref _target);
        _addColorSlider(LEItem.COLOR.VIOLET, ref _target);
        _addColorSlider(LEItem.COLOR.PURPLE, ref _target);  // pink
        _addColorSlider(LEItem.COLOR.SKYBULE, ref _target);
        //

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Save/Load");
        GUILayout.FlexibleSpace();
        int arraySize = 0;
        if (_target.wcEditorSettings != null) arraySize = _target.wcEditorSettings.typeProbabilities.Length;
        string[] displays = new string[arraySize];
        for (int i = 0; i < displays.Length; i++) displays[i] = (i+1).ToString();

        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
    }

    void DrawTargetScore (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Target Score");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Auto ", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
            GenerateAutoStarScore();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        _target.scoreToReach = EditorGUILayout.IntField("Star 1", _target.scoreToReach);
        _target.scoreMilestone2 = EditorGUILayout.IntField("Star 2", _target.scoreMilestone2);
        _target.scoreMilestone3 = EditorGUILayout.IntField("Star 3", _target.scoreMilestone3);

        EditorGUI.indentLevel--;
        EditorGUILayout.PrefixLabel("Magnification");
        EditorGUI.indentLevel++;
        GUILayout.FlexibleSpace();
        _target.magnification2 = EditorGUILayout.FloatField("Magnification 2", _target.magnification2);
        _target.magnification3 = EditorGUILayout.FloatField("Magnification 3", _target.magnification3);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawGameMode (EditWinningConditions _target)
    {
		// [FIX_MISSION] : This is always false. !!!
		gameModeIndex          = _target.isTimerGame ? 1 : 0;
		gameModeIndex         = EditorGUILayout.Popup("Game Mode", gameModeIndex, mode);
		_target.isTimerGame     = (gameModeIndex == 1);   
		
        _target.isHardLevelGame  = EditorGUILayout.Toggle("Hard Level", _target.isHardLevelGame);
        //EditorGUILayout.("Allowed moves", _target.allowedMoves);
		// [FIX_MISSION]
		EditWinningConditions.MISSION_TYPE oldMission = _target.missionType;
		_target.missionType     = (EditWinningConditions.MISSION_TYPE)EditorGUILayout.Popup("Misstion Type", (int)_target.missionType, mission_type);
		//int _allowedMoves = _target.allowedMoves;
		//_target.isTreasure = _target.missionType == EditWinningConditions.MISSION_TYPE.TREASURE;
		
        // de-init old types.
        _target.isClearChocolateGame    = false;
        _target.isPenguinGame           = false;
        _target.isYetiGame              = false;
        _target.isBossGame              = false;
        _target.isFairyGame             = false;
        //

        if (gameModeIndex == 0) 
		{
			_target.allowedMoves = EditorGUILayout.IntField("Allowed moves", _target.allowedMoves);
        }
		else 
		{
            _target.TimeGiven = EditorGUILayout.FloatField("Time given", _target.TimeGiven);
        }

        if(oldMission != _target.missionType)
            _clearDatas(_target, EditWinningConditions.MISSION_TYPE.MAX_TYPE);

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.textField);

        // [FIX_MISSION]
        _target.enableTreasure      = false;
        _target.isTreasureGame      = false;
        //_target.isGetTypesGame      = false;
        //_target.isSpecialJewelGame  = false;
        switch(_target.missionType)
        {
        case EditWinningConditions.MISSION_TYPE.COLLECT:    // collect targets.
        {
            _updateCollectMission(_target);
            
            break;
        }
        case EditWinningConditions.MISSION_TYPE.FILL:       // drop or locate to target bottoms.
        {
            _target.enableTreasure      = true;
            _target.isTreasureGame      = true;
            //_target.countPotion1        = EditorGUILayout.IntField("Potion 1", _target.countPotion1);
            //_target.countPotion2        = EditorGUILayout.IntField("Potion 2", _target.countPotion2);

            //_target.isTreasureGame = EditorGUILayout.Toggle("Collect Ingredient", _target.isTreasureGame);
            //if (_target.isTreasureGame) 
            {
                EditorGUI.indentLevel++;
                // _target.enableTreasure = EditorGUILayout.Toggle("Enable treasure", _target.enableTreasure);
                _target.countPotion1    = EditorGUILayout.IntField("Potion1", _target.countPotion1);
                _target.countPotion2    = EditorGUILayout.IntField("Potion2", _target.countPotion2);
                _target.countPotion3    = EditorGUILayout.IntField("Potion3", _target.countPotion3);
                _target.maxOnScreen     = EditorGUILayout.IntField("Max On Screen", _target.maxOnScreen);
                _target.treasureSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.treasureSpawnCountPerMove);
                _target.movePerTreasure = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerTreasure);
                EditorGUI.indentLevel--;
            }
            
            _updateCollectMission(_target);

            break;
        }
        case EditWinningConditions.MISSION_TYPE.FIND:       // find hiddent objects.
            _target.countChocoBar           = EditorGUILayout.IntField("Choco Bar", _target.countChocoBar);
            _updateCollectMission(_target);
            break;
        case EditWinningConditions.MISSION_TYPE.CLEAR:      // clear bottoms.
            _target.countCursedBottom       = EditorGUILayout.IntField("Cursed Bottom", _target.countCursedBottom);
            _updateCollectMission(_target);
            break;
        case EditWinningConditions.MISSION_TYPE.DEFEAT:     // AI fight.
            _target.countAiWinBoard         = EditorGUILayout.IntField("Ai Win Count", _target.countAiWinBoard);
            _target.ai_intelligence         = EditorGUILayout.IntSlider("Ai Intel(0-3)", _target.ai_intelligence, 0, 3);
            break;
        case EditWinningConditions.MISSION_TYPE.SCORE:      // has goal score type.
            _target.goalScore   = EditorGUILayout.IntField("Goal Score", _target.goalScore);
            break;
//			case EditWinningConditions.MISSION_TYPE.TREASURE:
//#if USE_UncleBill
//				treasureKeyindex = FindAssetsByTreasureItem(_target.TreasureItemKey);
//#endif
//				treasureKeyindex = EditorGUILayout.Popup("Treasure Item Key", treasureKeyindex, treasureKeylist.ToArray<string>());
//				_target.TreasureItemKey  = treasureKeylist[treasureKeyindex];
//				_target.TreasureItemCount         = EditorGUILayout.IntField("Treasure Item Count", _target.TreasureItemCount);
//		break;
        case EditWinningConditions.MISSION_TYPE.OLD_TYPE:        
        {
            #region => olds.. no use anylonger.
            _target.isClearChocolateGame = EditorGUILayout.Toggle("Eliminate Snowman", false);// _target.isClearChocolateGame);

            _target.isPenguinGame = EditorGUILayout.Toggle("Eliminate Penguin", false);// _target.isPenguinGame);

            if (_target.isPenguinGame) {
                EditorGUI.indentLevel++;
                _target.numberOfPenguin = EditorGUILayout.IntField("Number of penguin", _target.numberOfPenguin);
                EditorGUI.indentLevel--;
            }

            _target.isYetiGame = EditorGUILayout.Toggle("Eliminate Yeti", false);// _target.isYetiGame);

            if (_target.isYetiGame) {
                EditorGUI.indentLevel++;
                _target.yetiHealth = EditorGUILayout.IntField("Yeti Health", _target.yetiHealth);
                _target.attackPerMove = EditorGUILayout.IntField("Attack Per Move", _target.attackPerMove);
                _target.yetiBallDropProbability = EditorGUILayout.IntSlider("YetiBall Probability", _target.yetiBallDropProbability, 0, 100);
                _target.minYetiBallPieceCount = EditorGUILayout.IntSlider("Min YetiBall", _target.minYetiBallPieceCount, 0, 81);
                _target.maxYetiBallPieceCount = EditorGUILayout.IntSlider("Max YetiBall", _target.maxYetiBallPieceCount, 0, 81);
                _target.minYetiAttackCount = EditorGUILayout.IntSlider("Min Yeti Attack", _target.minYetiAttackCount, 0, 81);
                _target.maxYetiAttackCount = EditorGUILayout.IntSlider("Max Yeti Attack", _target.maxYetiAttackCount, 0, 81);
                EditorGUILayout.Space();
                EditorGUILayout.PrefixLabel("Attack Probability");

                int nval = EditorGUILayout.Popup(Preset.attackConvertProbabilityIdx, Preset.attackConvertProbabilityNames);
                if(nval != Preset.attackConvertProbabilityIdx) {
                    if(Preset.attackConvertProbability[nval].prob.Length != 0) {
                        Preset.attackConvertProbabilityIdx = nval;
                        for (int i = 0; i < _target.attackConvertProbability.Length; ++i) {
                            _target.attackConvertProbability[i] = Preset.attackConvertProbability[nval].prob[i];
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

             _target.isBossGame = EditorGUILayout.Toggle("Eliminate Boss", false);// _target.isBossGame);

            if (_target.isBossGame) {
                EditorGUI.indentLevel++;
                _target.bossHealth = EditorGUILayout.IntField("Boss Health", _target.bossHealth);
                EditorGUILayout.LabelField("Boss Type ex)", "0:Normal/1:Heal/2:Attack");

                _target.bossType = EditorGUILayout.IntSlider("Boss Type", _target.bossType, 0, 2);
                //_target.bossType = EditorGUILayout.IntField("Boss Type", _target.bossType);
                _target.bossActionPerMove = EditorGUILayout.IntField("Boss Action Per Move", _target.bossActionPerMove);
                _target.bossHealCount = EditorGUILayout.IntField("Boss Heal Count", _target.bossHealCount);
                _target.minBossAttackCount = EditorGUILayout.IntSlider("Min Boss Attack", _target.minBossAttackCount, 0, 81);
                _target.maxBossAttackCount = EditorGUILayout.IntSlider("Max Boss Attack", _target.maxBossAttackCount, 0, 81);
                EditorGUILayout.Space();
                EditorGUILayout.PrefixLabel("Boss Attack Probability");
                int bossNval = EditorGUILayout.Popup(Preset.bossAttackConvertProbabilityIdx, Preset.bossAttackConvertProbabilityNames);
                if(bossNval != Preset.bossAttackConvertProbabilityIdx) {
                    if(Preset.bossAttackConvertProbability[bossNval].prob.Length != 0) {
                        Preset.bossAttackConvertProbabilityIdx = bossNval;
                        for (int i = 0; i < _target.bossAttackConvertProbability.Length; ++i) {
                            _target.bossAttackConvertProbability[i] = Preset.bossAttackConvertProbability[bossNval].prob[i];
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            
             

            _target.isFairyGame = EditorGUILayout.Toggle("Rescue Fairy", false);// _target.isFairyGame);

            if (_target.isFairyGame) {
                EditorGUI.indentLevel++;
                _target.numberOfFairy = EditorGUILayout.IntField("Number of Fairy", _target.numberOfFairy);
                EditorGUI.indentLevel--;
            }

            #endregion
            break;
        }
        default:
            break;  // end of default.

        }   // end of switch.
        EditorGUILayout.EndVertical();
    }

    void _updateCollectMission(EditWinningConditions _target)
    {   
        if(_target.numToGet.Length < colors.Length)
            _extenArray(ref _target.numToGet, colors.Length);

        _target.isGetTypesGame  = EditorGUILayout.Toggle("Collect Jelly", _target.isGetTypesGame);
        if (_target.isGetTypesGame)
        {
            EditorGUI.indentLevel++;
            //for (int i = 0; i < colors.Length; i++) {                                        
            //    _target.numToGet[i] = EditorGUILayout.IntField(colors[i], _target.numToGet[i]);
            //}
            _addColorIntField(LEItem.COLOR.RED, ref _target);
            _addColorIntField(LEItem.COLOR.ORANGE, ref _target);
            _addColorIntField(LEItem.COLOR.YELLOW, ref _target);
            _addColorIntField(LEItem.COLOR.GREEN, ref _target);
            _addColorIntField(LEItem.COLOR.BLUE, ref _target);
            _addColorIntField(LEItem.COLOR.VIOLET, ref _target);
            _addColorIntField(LEItem.COLOR.PURPLE, ref _target);
            _addColorIntField(LEItem.COLOR.SKYBULE, ref _target);

            EditorGUI.indentLevel--;
            //_target.UpdateGameModeState( EditWinningConditions.GAME_MODE.JEWEL );
        }
        else
        {
            for (int i = 0; i < colors.Length; i++) 
                _target.numToGet[i] = 0;
        }
            
        _target.isMixMission    = EditorGUILayout.Toggle("Collect objects", _target.isMixMission);
        if(true == _target.isMixMission)
        {
            EditorGUI.indentLevel++;
            _target.countRoundChocho    = EditorGUILayout.IntField("Round Choco(수박)", _target.countRoundChocho);
            _target.countRectChocho     = EditorGUILayout.IntField("Rect Choco(케익)", _target.countRectChocho);
            _target.countJamBottom      = EditorGUILayout.IntField("Jam Bottom(후라이)", _target.countJamBottom);
            _target.countCottonCandy    = EditorGUILayout.IntField("Cotton Candy(초코네모)", _target.countCottonCandy);
            _target.countSodaCan        = EditorGUILayout.IntField("Soda Can(맥주잔)", _target.countSodaCan);
            _target.countSugarBlock     = EditorGUILayout.IntField("Sugar Block(모자)", _target.countSugarBlock);
            _target.countZellatto       = EditorGUILayout.IntField("Zellatto(아이스크림)", _target.countZellatto);
            _target.countCookieJelly    = EditorGUILayout.IntField("Cookie Jelly(X)", _target.countCookieJelly);
            _target.countColorBox       = EditorGUILayout.IntField("Color box(X)", _target.countColorBox);
            _target.countWaffleCooker   = EditorGUILayout.IntField("Waffle Cooker(X)", _target.countWaffleCooker);
            _target.countMudShade       = EditorGUILayout.IntField("Mud Shade(X)", _target.countMudShade);
            EditorGUI.indentLevel--;
        }
        else
        {
            _target.countRoundChocho    = 0;
            _target.countRectChocho     = 0;
            _target.countJamBottom      = 0;
            _target.countCottonCandy    = 0;
            _target.countSodaCan        = 0;
            _target.countSugarBlock     = 0;
            _target.countZellatto       = 0;
            _target.countCookieJelly    = 0;
            _target.countColorBox       = 0;
            _target.countWaffleCooker   = 0;
            _target.countMudShade       = 0;
        }

        _target.isSpecialJewelGame  = EditorGUILayout.Toggle("Collect Specials", _target.isSpecialJewelGame);
        if (_target.isSpecialJewelGame) {
            EditorGUI.indentLevel++;
            for (int i = 0; i < specialJewelCombineStr.Length; i++) {
                _target.specialJewels[i] = EditorGUILayout.IntField(specialJewelCombineStr[i], _target.specialJewels[i]);
            }
            EditorGUI.indentLevel--;
            // _target.UpdateGameModeState( EditWinningConditions.GAME_MODE.SPECIAL_JEWEL );
        }
        else
        {
            for (int i = 0; i < specialJewelCombineStr.Length; i++) 
                _target.specialJewels[i] = 0;
        }
    }

    // data clear helper.
    void _clearDatas(EditWinningConditions data, EditWinningConditions.MISSION_TYPE clearType)
    {
        switch(clearType)
        {
        case EditWinningConditions.MISSION_TYPE.COLLECT:
            // reset collect.
            data.isGetTypesGame     = false;
            for(int i = 0; i < colors.Length; i++)
                data.numToGet[i]    = 0;
            data.isSpecialJewelGame = false;
            for(int i = 0; i < specialJewelCombineStr.Length; i++)
                data.specialJewels[i]= 0;
            data.isMixMission       = false;
            data.countRoundChocho   = 0;
            data.countRectChocho    = 0;
            data.countJamBottom     = 0;
            data.countCottonCandy   = 0;
            data.countSodaCan       = 0;
            data.countSugarBlock    = 0;
            data.countZellatto      = 0;
            data.countCookieJelly   = 0;
            data.countColorBox      = 0;
            data.countWaffleCooker  = 0;
            data.countMudShade      = 0;
            break;

        case EditWinningConditions.MISSION_TYPE.FILL:
            data.enableTreasure     = false;
            data.isTreasureGame     = false;
            data.countPotion1       = 0;
            data.countPotion2       = 0;
            data.countPotion3       = 0;
            data.maxOnScreen        = 0;
            data.treasureSpawnCountPerMove = 0;
            data.movePerTreasure    = 0;
            break;

        case EditWinningConditions.MISSION_TYPE.DEFEAT:
            data.countAiWinBoard    = 0;
            data.ai_intelligence    = 0;
            break;

        case EditWinningConditions.MISSION_TYPE.FIND:
            data.countChocoBar      = 0;
            break;

        case EditWinningConditions.MISSION_TYPE.CLEAR:
            data.countCursedBottom  = 0;
            break;

        case EditWinningConditions.MISSION_TYPE.SCORE:
            data.goalScore          = 0;
            break;

        default:
            _clearDatas(data, EditWinningConditions.MISSION_TYPE.COLLECT);
            _clearDatas(data, EditWinningConditions.MISSION_TYPE.FILL);
            _clearDatas(data, EditWinningConditions.MISSION_TYPE.DEFEAT);
            _clearDatas(data, EditWinningConditions.MISSION_TYPE.FIND);
            _clearDatas(data, EditWinningConditions.MISSION_TYPE.CLEAR);
            break;
        }
    }

    int infoFilledPannelCount = 0;
    int infoIceCount = 0;
    int infoSnowmanCount = 0;

    void DrawCurrentInfo (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Info");
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Update info", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
            List<LECell> cells = LEHelper.Cells;
            infoFilledPannelCount = 0;
            infoIceCount = 0;
            infoSnowmanCount = 0;
            for (int i = 0; i < cells.Count; i++) {
                if (cells[i].shaded != -1) {
                    infoIceCount += (cells[i].shaded + 1);
                } else if (cells[i].panelIndex == 7) {
                    infoSnowmanCount++;
                } else if (((cells[i].panelIndex >= 2) && (cells[i].panelIndex <= 4)) || (cells[i].panelIndex == 8)) {
                    infoIceCount += (cells[i].shaded + 1);
                }

                if (cells[i].panelIndex != 1) infoFilledPannelCount++;
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Ice count", infoIceCount.ToString());
        EditorGUILayout.LabelField("Snowman count", infoSnowmanCount.ToString());
        EditorGUILayout.LabelField("Filled pannel count", infoFilledPannelCount.ToString());
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawStrawberry (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableStrawberry    = EditorGUILayout.Toggle("Enable StrawBerry", _target.enableStrawberry);
        EditorGUI.indentLevel++;
        _target.movePerStrawberry   = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerStrawberry);
        _target.minStrawberryCount  = EditorGUILayout.IntSlider("Minimum count", _target.minStrawberryCount, 1, 81);
        _target.maxStrawberryCount  = EditorGUILayout.IntSlider("Maximum count", _target.maxStrawberryCount, 1, 81);
        _target.strawberrySpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.strawberrySpawnCountPerMove);
        //_target.defaultFallBackTime = EditorGUILayout.IntField("default FallBackTime", _target.defaultFallBackTime);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    // [SUGAR_CHERRY]
    void DrawSugarCherry (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableSugarCherry   = EditorGUILayout.Toggle("Enable SugarCherry", _target.enableSugarCherry);
        EditorGUI.indentLevel++;
        _target.movePerSugarCherry  = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerSugarCherry);
        _target.minSugarCherryCount = EditorGUILayout.IntSlider("Minimum count", _target.minSugarCherryCount, 1, 81);
        _target.maxSugarCherryCount = EditorGUILayout.IntSlider("Maximum count", _target.maxSugarCherryCount, 1, 81);
        _target.sugarCherrySpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.sugarCherrySpawnCountPerMove);
        //_target.defaultFallBackTime = EditorGUILayout.IntField("default FallBackTime", _target.defaultFallBackTime);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    // [ZELLATO]
    void DrawZellato (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableZellato   = EditorGUILayout.Toggle("Enable Zellato", _target.enableZellato);
        EditorGUI.indentLevel++;
        _target.movePerZellato  = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerZellato);
        _target.minZellatoCount = EditorGUILayout.IntSlider("Minimum count", _target.minZellatoCount, 1, 81);
        _target.maxZellatoCount = EditorGUILayout.IntSlider("Maximum count", _target.maxZellatoCount, 1, 81);
        _target.zellatoSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.zellatoSpawnCountPerMove);
        //_target.defaultFallBackTime = EditorGUILayout.IntField("default FallBackTime", _target.defaultFallBackTime);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    // [Add Time Block]
    void DrawAddTimeBlock (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableAddTimeBlock   = EditorGUILayout.Toggle("Enable addTimeBlock", _target.enableAddTimeBlock);
        EditorGUI.indentLevel++;
        _target.movePerAddTimeBlock  = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerAddTimeBlock);
        _target.minAddTimeBlockCount = EditorGUILayout.IntSlider("Minimum count", _target.minAddTimeBlockCount, 1, 81);
        _target.maxAddTimeBlockCount = EditorGUILayout.IntSlider("Maximum count", _target.maxAddTimeBlockCount, 1, 81);
        _target.addTimeBlockSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.addTimeBlockSpawnCountPerMove);
        //_target.defaultFallBackTime = EditorGUILayout.IntField("default FallBackTime", _target.defaultFallBackTime);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawTimeBomb (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableTimeBomb = EditorGUILayout.Toggle("Enable time bomb", _target.enableTimeBomb);
        EditorGUI.indentLevel++;
        _target.movePerTimeBomb = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerTimeBomb);
        _target.minTimeBombCount = EditorGUILayout.IntSlider("Minimum count", _target.minTimeBombCount, 1, 81);
        _target.maxTimeBombCount = EditorGUILayout.IntSlider("Maximum count", _target.maxTimeBombCount, 1, 81);
        _target.timeBombSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.timeBombSpawnCountPerMove);
        _target.defaultFallBackTime = EditorGUILayout.IntField("default FallBackTime", _target.defaultFallBackTime);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawPenguinSpawn (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        EditorGUILayout.PrefixLabel("Penguin");
        EditorGUI.indentLevel++;
        _target.maxPenguinOnScreen = EditorGUILayout.IntField("Max penguin", _target.maxPenguinOnScreen);
        _target.chanceToPenguinSpawn = EditorGUILayout.IntSlider("Chance to spwan", _target.chanceToPenguinSpawn, 0, 100);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawFairy (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableFairyPiece = EditorGUILayout.Toggle("Enable Fairy", _target.enableFairyPiece);
        EditorGUI.indentLevel++;
        _target.minFairyPieceCount = EditorGUILayout.IntSlider("Minimum count", _target.minFairyPieceCount, 1, 81);
        _target.maxFairyPieceCount = EditorGUILayout.IntSlider("Maximum count", _target.maxFairyPieceCount, 1, 81);
        _target.fairyPieceSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.fairyPieceSpawnCountPerMove);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawRoundChoco (EditWinningConditions _target)
    {
        // [ROUND_CHOCO]
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableSpiralSnow = EditorGUILayout.Toggle("Enable Round Choco", _target.enableSpiralSnow);
        EditorGUI.indentLevel++;
        _target.movePerSpiralSnow = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerSpiralSnow);
        _target.lifeNewRoundChoco   = EditorGUILayout.IntSlider("Default Life", _target.lifeNewRoundChoco, 1, 3);
        _target.minSpiralSnowCount = EditorGUILayout.IntSlider("Minimum count", _target.minSpiralSnowCount, 1, 81);
        _target.maxSpiralSnowCount = EditorGUILayout.IntSlider("Maximum count", _target.maxSpiralSnowCount, 1, 81);
        _target.spiralSnowSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.spiralSnowSpawnCountPerMove);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawGreenBubble (EditWinningConditions _target)
    {
        // [GREEN_BUBBLE]
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableGreenBubble = EditorGUILayout.Toggle("Enable Green Bubble", _target.enableGreenBubble);
        EditorGUI.indentLevel++;
        _target.movePerGreenBubble = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerGreenBubble);
        _target.lifeNewGreenBubble   = EditorGUILayout.IntSlider("Default Life", _target.lifeNewGreenBubble, 1, 3);
        _target.minGreenBubble = EditorGUILayout.IntSlider("Minimum count", _target.minGreenBubble, 1, 81);
        _target.maxGreenBubble = EditorGUILayout.IntSlider("Maximum count", _target.maxGreenBubble, 1, 81);
        _target.greenBubbleSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.greenBubbleSpawnCountPerMove);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawMystery (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableMysteryPiece = EditorGUILayout.ToggleLeft("Use Mystery", _target.enableMysteryPiece);
        EditorGUI.indentLevel++;

        _target.movePerMysteryPiece = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerMysteryPiece);

        _target.minMysteryPieceCount = EditorGUILayout.IntSlider("Minimum count", _target.minMysteryPieceCount, 1, 81);
        _target.maxMysteryPieceCount = EditorGUILayout.IntSlider("Maximum count", _target.maxMysteryPieceCount, 1, 81);
        _target.mysteryPieceSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.mysteryPieceSpawnCountPerMove);

        EditorGUI.indentLevel++;


        int nval = EditorGUILayout.Popup(Preset.mysteryPieceConvertProbabilityIdx, Preset.mysteryPieceConvertProbabilityNames);
        if(nval != Preset.mysteryPieceConvertProbabilityIdx) {
            if(Preset.mysteryPieceConvertProbability[nval].prob.Length != 0) {
                Preset.mysteryPieceConvertProbabilityIdx = nval;
                for (int i = 0; i < _target.mysteryPieceConvertProbability.Length; ++i) {
                    _target.mysteryPieceConvertProbability[i] = Preset.mysteryPieceConvertProbability[nval].prob[i];
                }
            }
        }
/*
        int totalPercent = 0;
        for (int i = 0; i < mysteryType.Length; i++) {
            _target.mysteryPieceConvertProbability[i] = EditorGUILayout.IntField(mysteryType[i], _target.mysteryPieceConvertProbability[i]);
            totalPercent += _target.mysteryPieceConvertProbability[i];
        }
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Total Percent", totalPercent.ToString());
*/

        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void DrawChameleon (EditWinningConditions _target)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        _target.enableChameleon = EditorGUILayout.ToggleLeft("Use Chameleon", _target.enableChameleon);
        EditorGUI.indentLevel++;
        _target.movePerChameleon = EditorGUILayout.IntField("Generate interval(drop)", _target.movePerChameleon);
        _target.minChameleonPieceCount = EditorGUILayout.IntSlider("Minimum count", _target.minChameleonPieceCount, 0, 81);
        _target.maxChameleonPieceCount = EditorGUILayout.IntSlider("Maximum count", _target.maxChameleonPieceCount, 0, 81);
        _target.chameleonSpawnCountPerMove = EditorGUILayout.IntField("Spawn Count", _target.chameleonSpawnCountPerMove);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }


    void GenerateAutoStarScore ()
    {
        EditWinningConditions _target = (EditWinningConditions)target;
        _target.scoreToReach    = _target.allowedMoves * 150;  // 1000;
        _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
        _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
        return;

        /* old codes !!!
        List<LECell> cells = LEHelper.Cells;
        EditWinningConditions.GAME_MODE mode = _target.GetGameMode();
        switch (mode) {
            case EditWinningConditions.GAME_MODE.TIMER :
                _target.scoreToReach = _target.allowedMoves * 150;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.MOVE :
                _target.scoreToReach = _target.allowedMoves * 150;  //1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.ICE:
                int iceCount = 0;
                for (int i = 0; i < cells.Count; i++) {
                    iceCount += (cells[i].shaded != -1) ? cells[i].shaded + 1 : 0;
                }

                _target.scoreToReach = iceCount * 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.SNOWMAN :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.JEWEL :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.INGREDIENT :
                int ingredientsTotalCount = _target.countPotion1 + _target.countPotion2 + _target.countPotion3;
                _target.scoreToReach = (ingredientsTotalCount * 10000) + (_target.allowedMoves * 100);
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.PENGUIN :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.FAIRY :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.YETI :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            case EditWinningConditions.GAME_MODE.BOSS :
                _target.scoreToReach = 1000;
                _target.scoreMilestone2 = (int)(_target.scoreToReach * _target.magnification2);
                _target.scoreMilestone3 = (int)(_target.scoreToReach * _target.magnification3);
                break;
            default :
                Debug.LogError("Error, Check Game Mode !!");
                break;
        }*/
    }

    void _extenArray(ref int[] srcArray, int destSize)
    {
        if(null==srcArray || srcArray.Length>destSize)
            return;

        int[] _temp             = srcArray.ToArray();
        srcArray                = new int[ destSize ];
        for(int qq = 0; qq < _temp.Length; ++qq)
            srcArray[qq]        = _temp[qq];
    }
	
	
}
