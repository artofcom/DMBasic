using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonFx.Json;
using NOVNINE.Diagnostics;
using UnityEngine.SceneManagement;
using System.Linq;
using AssetBundles;
//using DiffSharp;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelEditorSceneHandler : SingletonMonoBehaviour<LevelEditorSceneHandler>//, IGameScene 
{    
#if UNITY_EDITOR
	static public bool EditorMode = false;
#endif
	
    readonly Point boardSize = new Point(9, 9);
    public const float CELL_GAP_DISTANCE = 66.6f;
    const float KEYBOARD_INPUT_DELAY = .4f;

    public Transform cellContainer;
    public LECell cellMold;
    public Toggle[] defaultSpawnToggles;
    public Toggle[] roundChocoSpawnToggles;
    public Toggle[] greenBubbleSpawnToggles;
    public Toggle[] treasureSpawnToggles;
    public Toggle[] strawBerrySpawnToggles;
    public Toggle[] sugarCherrySpawnToggles;
    public Toggle[] zellattoSpawnToggles;
    public Toggle[] addTimeBlockSpawnToggles;
    public LEItem[] pieces;
    public LEItem[] panels;
    public LEItem[] shade;
    public LEItem etc;
    public LEItem[] portal;
    public LEItem colorChanger;
	public EditLevelConditions conditions;
	public EditWinningConditions winningConditions;
	public LEItem _river;   //, _riverWaffle;
    public LEItem _pieceOverIce;
    public LEItem _fence;
    public LEItem panelColorBoxS2, panelColorBoxS3;
    public LEItem _2x2Color6Pnl, _cookieJellyPiece2;

	public WorldMapBuilder worldMapBuilder;
	
    public Text levelInfoLabel;
    public Text modifedLabel;
	
    // [AI_MISSION]
    public LEItem ai_taken_items;
    public LEItem bar_choco;

    public GameObject _shadeGroup;
    public LEItem _itemHelper;


    List<LECell> cells = new List<LECell>();
    List<LEItem> tools = new List<LEItem>();
	Data.Level level;
    int levelIdx;
    int LevelIdx {
        get { return levelIdx; }
        set {
            levelIdx = value;
            conditions.levelIndex = levelIdx;
#if UNITY_EDITOR
            conditions.Repaint();
#endif
        }
    }
    bool needContinuousKey = false;
    float inputHoldSec = 0f;

    /*
    public bool Modified {
        get { return LEHelper.didModify; }
        set { 
            LEHelper.didModify = value;
            modifedLabel.color = value ? Color.red : Color.clear;
        }
    }
    */

	
	protected override void Awake()
	{
		base.Awake();
	
		//Root.SetPostfix("A");
		//Root.Data.Initialize();
	
#if UNITY_EDITOR
		LevelEditorSceneHandler.EditorMode = true;
#endif
		
	}
	
    public void DoDataExchange () {
    }

    public void OnEnterScene (object param) 
	{
        
#if UNITY_EDITOR
//#if !USE_DLLDATACLASS 
//		DisplayDialog( "에러", "C# Data 모드입니다. DLL 모드로 변경해주세요, 그냥쓰셔도 됩니다만 속도가 느릴겁니다.", "Ok");
//#endif
        string fullpath = string.Format("Assets/Resources/{0}/{1}.txt", GetTextPath(), 1);
        if(!System.IO.File.Exists(fullpath)) 
		{
            DisplayDialog( "에러", "split 폴더가 없네요. 자동으로 Split합니다.", "Ok");
            SplitThemeToLevels();
        }

        UnityEditor.Selection.activeGameObject = gameObject;
        var lv = param as Data.Level;
        if(lv == null) {
            int lvidx = UnityEditor.EditorPrefs.GetInt("JMK_EditorLevel", 1);
            LoadLevelFromFile(lvidx);
        } else {
			LoadLevelFromFile(lv.Index);
        }
#endif
    }

    public void OnLeaveScene () {
    }

#if UNITY_EDITOR
    public override void Init ()
    {
        // for tool.
       if(Root.Data == null)
		{			
			Root.Data           = Root.Load();
            //Root.Data.InitializeRoot();     //WorldSceneHandler.ID_MAX_LEVEL, "0");
            Root.Data.initCurrentLevel();

			Director.PatchComplete = true;
            NOVNINE.Platform.Init();
        }

        GenerateBoardCell();
        GatherToolsForTrackingCombinationKey();
		
		OnEnterScene(Director.CurrentSceneLevelData);		
    }

    void OnEnable ()
    {
		conditions.OnLevelLoad += LoadLevelFromFile;
		conditions.OnSaveLevel += SaveLevelToFile;
    }

    void OnDisable ()
    {
		conditions.OnLevelLoad -= LoadLevelFromFile;
		conditions.OnSaveLevel -= SaveLevelToFile;
    }

    void Update ()
    {
        CheckKeyboardCanInputCoutinuously();
        TrackingKeyCombinationsAtToolArea(needContinuousKey);
        TrackingKeyCombinationsAtBoardArea(needContinuousKey);
        TrackingKeyCombinationsAtBottomBar(needContinuousKey);
    }

#region tk2dButton Handlers
#endregion //tk2dButton Handlers

    public void OnMousDown (LEItem item)
    {
        // turn off old.
        if(null!=LEHelper.CurrentItem && null!=LEHelper.CurrentItem.variants)
            LEHelper.CurrentItem.variants.SetActive( false );

        item.Switch.isOn = true;
        switch ((LEItem.TYPE)item.Type) {
            case LEItem.TYPE.PIECE :
            {
                if(10 == item.index)    // [ROUND_CHOCO]
                    item.OnSelectedStrength(item.Strength);
                else
                    item.OnSelectedColor(item.Color);
                break;
            }
            case LEItem.TYPE.PANEL : 
                int str = ((item.Index >= 18) && (item.Index <= 21)) ? 3 : item.Strength;
                item.OnSelectedStrength(str);
                break;
            case LEItem.TYPE.SHADE :
            {
                int param       = item.Shaded + 100 * (int)item.eShadeType;
                item.OnSelectedShade(param);    // item.Shaded, item.eShadeType);
                break;
            }
            case LEItem.TYPE.ETC :      item.OnSelectedETC(item.Index);         break;
            case LEItem.TYPE.PORTAL :   item.OnSelectedStrength(item.Strength); break;
            case LEItem.TYPE.AI_TAKEN:  item.OnSelected_AI_TAKEN(item.index);   break;
            case LEItem.TYPE.CHOCO_BAR:
            {
                if(false == Input.GetKey(KeyCode.Mouse1))
                    item.OnSelectedChoco((int)item.eBarType);
                break;
            }
            case LEItem.TYPE.HELPER:    item.OnSelectedHelper(0);   break;
            case LEItem.TYPE.COLOR_CHANGER:
                item.OnSelectedChangerColor(item.Color); 
                break;
            case LEItem.TYPE.OVER_RIVER:    item.OnSelectedOverRiver(0);break;
            case LEItem.TYPE.RIVER:         item.OnSelectedRiver(0);    break;
            case LEItem.TYPE.PIECE_COVER:   item.OnSelectedPieceCover(); break;
            case LEItem.TYPE.FENCE:         item.OnSelctedFence();      break;
            default:    break;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Mouse1)) {
            if (item.variants != null) item.SetVariantsActive(item.Switch.isOn);
        }
    }

    void InitLevel(int idx, Data.Level lv)
	{
		var thm = Root.Data.GetLevelFromIndex(idx);
        if(thm == null) 
		{
		//-	thm = Root.Data.levels[0];
		//-	lv.InitializeRoot(0, Root.Data);
        }
		else 
		{
			lv.InitLevel(idx, Root.Data);
        }
		
		//-Debugger.Assert(lv.Index  < Root.Data.levels.Length);
		Root.Data.currentLevel = lv;
    }

    static bool DisplayDialog(string title, string msg, string ok, string cancel = null) 
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.DisplayDialog( title, msg, ok, cancel);
#else
        Debug.Log(string.Format("DisplayDialog Title({0}) Msg({1}) Ok({2}) Cancel({3})", 
            title, msg, ok, cancel));
        return false;
#endif
    }

    bool mNeedFix               = false;

	public void OnClickPlay ()
    {
        mNeedFix                = false;
        if(CheckModified()) 
		{
            if(DisplayDialog( "확인", string.Format("현재 수정중인 레벨 {0}을 저장하시렵니까?", LevelIdx+1), "저장", "취소")) 
			{
                SaveLevelToFile(LevelIdx);
                InitLevel(LevelIdx, level);
            } 
            else 
                return;
        }

        // error filter.
        if (true == mNeedFix)    return;

		EventSystem.current.SetSelectedGameObject(null);
		
		
#if UNITY_EDITOR
		Director.CurrentSceneLevelData = level;
		SceneManager.LoadScene("Play");
#endif
		
	}
	
    public void GenerateNewBoard ()
    {
        foreach (LECell c in cells) c.ResetAll();
        EventSystem.current.SetSelectedGameObject(null);

        refreshGoalObjText();
    }

    public void GenerateEmptyBoard ()
    {
        foreach (LECell c in cells) c.RemoveAll();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShiftLevelBy (int amount)
    {
        LoadLevelFromFile(LevelIdx + amount);
    }

    public Image GetPieceImage (int pieceIndex, int colorIndex)
    {
        Image img = null;
        return img;
    }

    public Image GetPanelImage (int pieceIndex, int colorIndex)
    {
        Image img = null;
        return img;
    }

    void GenerateBoardCell()
    {
        cells.Clear();
        for (int y = 0; y < boardSize.y; y++) 
		{
            for (int x = 0; x < boardSize.x; x++) 
			{
        		LECell cell = Instantiate(cellMold) as LECell;
                cell.transform.SetParent(cellContainer);
                cell.transform.localScale = Vector3.one;
 				Vector3 targetPosition = new Vector3(x * CELL_GAP_DISTANCE, y * CELL_GAP_DISTANCE, 0.0f);
				cell.GetComponent<Transform>().localPosition = targetPosition;
                 int cellIndex = x + (y * 9);
                 cell.Init(cellIndex);
                 cells.Add(cell);
            }
        }

        LEHelper.Cells = cells;
    }

    void GatherToolsForTrackingCombinationKey ()
    {
        for (int i = 0; i < pieces.Length; i++) tools.Add(pieces[i]);
        for (int i = 0; i < panels.Length; i++) {
            if (i >= 12 && i <= 17) continue; //need only one Creator tool
            if (i >= 19 && i <= 21) continue; //need only one cake tool
            tools.Add(panels[i]);
        }
        for(int q = 0; q < shade.Length; ++q)
            tools.Add(shade[q]);
        tools.Add(etc);

        tools[0].Switch.isOn = true;
        LEHelper.CurrentItem = tools[0];
    }

    int ConvertFlipYIndex (int index) {
        Point p = new Point(index % 9, index / 9);
        return (p.x + 9 * (8 - p.y));
    }

    bool bypassModifiedCheck = false;

    void LoadLevelFromFile (int index)
    {
        if(CheckModified()) {
            if(DisplayDialog( "확인", string.Format("현재 수정중인 레벨 {0}을 저장하시렵니까?", LevelIdx+1), "저장", "무시")) {
                SaveLevelToFile(LevelIdx);
            } 
        }

        //if(index == 0)
        //    index = 1;

        //string fullpath = string.Format("Assets/Resources/{0}/{1}.txt", GetTextPath(), index+1);
        //if(!System.IO.File.Exists(fullpath)) {
       //     DisplayDialog( "에러", string.Format("{0}.txt 없는 레벨입니다. 레벨파일을 만들어주세요", index+1), "Ok");
       //     return;
       // }

        string fullpath = string.Format("Assets/Resources/test.txt");
        

        LevelIdx = index;
        UnityEditor.EditorPrefs.SetInt("JMK_EditorLevel", index);
        
		Data.Level lv   = JsonReader.Deserialize<Data.Level>(System.IO.File.ReadAllText(fullpath));
        lv.Index        = index;
        Debugger.Assert(lv != null);
		LoadLevel(lv);
    }

    public static bool PublicInstancePropertiesEqual<T>(T self, T to, params string[] ignore) where T : class 
    {
        if (self != null && to != null)
        {
            System.Type type = typeof(T);
            List<string> ignoreList = new List<string>(ignore);
            foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!ignoreList.Contains(pi.Name))
                {
                    object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                    object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return self == to;
    }
    
    bool CheckModified() 
    {
        if(null == level)       return false;
        if(bypassModifiedCheck) return false;
        string fullpath = string.Format("Assets/Resources/{0}/{1}.txt", GetTextPath(), LevelIdx+1);
        if(!File.Exists(fullpath)) {
            return false;
        }
        
        Data.Level orgLv        = JsonReader.Deserialize<Data.Level>(System.IO.File.ReadAllText(fullpath));
        var strOrg              = JsonWriter.Serialize(orgLv);
        Data.Level newLv        = SaveLevel();
        var strNew              = JsonWriter.Serialize(newLv);
        if(string.Compare(strOrg, strNew, false) != 0) {
            return true;
        }
        
        return false;
    }

    void LoadLevel (Data.Level _level)
    {
        Debugger.Assert(_level != null);

        level = _level;
		
		winningConditions.SetConditionsInspector(level);

        //conditions.levelLength = Root.Data.TotalLevelCount;
        //conditions.levelIndex = level.GlobalIndex;

        foreach (LECell c in cells) c.ResetAll();

        const int max_bd_size   = GameManager.WIDTH * GameManager.HEIGHT;   // 9 x 9

        // [ AI_MISSION ]
        if(null==level.ai_taken_index)
        {
            level.ai_taken_index    = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.ai_taken_index[i] = -1;
        }
        // [JAM_SHADE]
        if(null==level.eShadeType)
        {
            level.eShadeType        = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.eShadeType[i] = -1;
        }
        // [NET_SHADE]
        if(null==level.eBarType)
        {
            level.eBarType          = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.eBarType[i] = -1;
        }
        if(null==level.indexBar)
        {
            level.indexBar          = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.indexBar[i] = -1;
        }
        if(null==level.changerColorIndex)
        {
            level.changerColorIndex = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.changerColorIndex[i] = -1;
        }
        if(null==level.eHelperType)
        {
            level.eHelperType   = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.eHelperType[i] = -1;
        }
        if(null==level.hasRiver)
        {
            level.hasRiver      = new bool[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.hasRiver[i]= false;
        }
        if(null==level.idxOverRiver)
        {
            level.idxOverRiver  = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.idxOverRiver[i]= -1;
        }
        if(null==level.lifePieceCover)
        {
            level.lifePieceCover = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.lifePieceCover[i]= 0;
        }
        if(null==level.fenceDirection)
        {
            level.fenceDirection = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.fenceDirection[i]= 0;
        }
        if(null==level.panelColors)
        {
            level.panelColors = new int[max_bd_size];
            for (int i = 0; i < max_bd_size; i++) 
                level.panelColors[i] = -1;
        }
        //

        for (int i = 0; i < max_bd_size; i++) {

            cells[i].hasRiver       = level.hasRiver[ConvertFlipYIndex(i)];
            cells[i]._idxOverRiver  = level.idxOverRiver[ConvertFlipYIndex(i)];

            cells[i].panelIndex = level.panels[ConvertFlipYIndex(i)];
            cells[i].pStrength = level.strengths[ConvertFlipYIndex(i)];
            cells[i].shaded = level.shaded[ConvertFlipYIndex(i)];
            cells[i].eShadeType = level.eShadeType[ConvertFlipYIndex(i)];
            cells[i].panelColor = level.panelColors[ConvertFlipYIndex(i)];

            // [JAM_SHADE] ; 둘다 valid해야만 한다.
            if (cells[i].shaded<0 || cells[i].eShadeType<0)
            {
                cells[i].shaded     = -1;
                cells[i].eShadeType = -1;
            }
            cells[i].pieceIndex = level.pieces[ConvertFlipYIndex(i)];
            cells[i].colorIndex = level.colors[ConvertFlipYIndex(i)];
            cells[i].changerColorIndex = level.changerColorIndex[ConvertFlipYIndex(i)];

            // [ AI_MISSION ]       - 시작시 ai 차지 보드 정보 setting.
            cells[i].ai_taken_index = level.ai_taken_index[ ConvertFlipYIndex(i) ];

            // [NET_SHADE]
            cells[i].eBarType       = level.eBarType[ ConvertFlipYIndex(i) ];
            cells[i].indexBar       = level.indexBar[ ConvertFlipYIndex(i) ];
            //

            cells[i]._nHelperType   = level.eHelperType[ ConvertFlipYIndex(i) ];

            //if (level.fallBackTime != null) cells[i].FallbackTime = level.fallBackTime[ConvertFlipYIndex(i)];
            if (level.panelInfos != null) cells[i].panelInfo = level.panelInfos[ConvertFlipYIndex(i)];
            if (level.riverInfos != null) cells[i].riverInfo = level.riverInfos[ConvertFlipYIndex(i)];
            if (level.portalTypes != null) cells[i].portalType = level.portalTypes[ConvertFlipYIndex(i)];
            if (level.portalUIndices != null) cells[i].PortalUIndex = level.portalUIndices[ConvertFlipYIndex(i)];
            if (level.portalDIndices != null) cells[i].PortalDIndex = level.portalDIndices[ConvertFlipYIndex(i)];

            if (level.conveyorIndices != null) cells[i].ConveyorUIndex = level.conveyorIndices[ConvertFlipYIndex(i)];

            if (level.lifePieceCover != null)   cells[i].LifePieceCover = level.lifePieceCover[ConvertFlipYIndex(i)];

            if (level.fenceDirection != null)   cells[i].DirFence       = (JMF_DIRECTION)level.fenceDirection[ConvertFlipYIndex(i)];

            if (level.startPieces == null) {
                cells[i].startPiece = true;
            } else {
                cells[i].startPiece = level.startPieces[ConvertFlipYIndex(i)];
            }
        }

        if (level.treasureGoal != null) {
            for (int i = 0; i < level.treasureGoal.Length; i += 2) {
                if (level.treasureGoal.Length <= (i + 1)) break;

                Point p = new Point(level.treasureGoal[i], level.treasureGoal[i + 1]);
                cells[p.x + (p.y * 9)].hasTreaserGoal = true;
            }
        }

        if (level.penguinSpawn != null) {
            for (int i = 0; i < level.penguinSpawn.Length; i += 2) {
                if (level.penguinSpawn.Length <= (i + 1)) break;

                Point p = new Point(level.penguinSpawn[i], level.penguinSpawn[i + 1]);
                cells[p.x + (p.y * 9)].hasPenguinSpawn = true;
            }
        }

        if (level.defaultSpawnColumn != null || level.defaultSpawnColumn.Length==0) {
            for (int i = 0; i < defaultSpawnToggles.Length; i++) {
                defaultSpawnToggles[i].isOn = false;
            }

            for (int i = 0; i < level.defaultSpawnColumn.Length; i++) {
                if (level.defaultSpawnColumn[i] < 0) continue;
                if (level.defaultSpawnColumn[i] >= defaultSpawnToggles.Length) continue;
                defaultSpawnToggles[level.defaultSpawnColumn[i]].isOn = true;
            }
        }

        if (level.roundChocoSpawnColumn != null || level.roundChocoSpawnColumn.Length==0) {
            for (int i = 0; i < roundChocoSpawnToggles.Length; i++) {
                roundChocoSpawnToggles[i].isOn = false;
            }

            for (int i = 0; i < level.roundChocoSpawnColumn.Length; i++) {
                if (level.roundChocoSpawnColumn[i] < 0) continue;
                if (level.roundChocoSpawnColumn[i] >= roundChocoSpawnToggles.Length) continue;
                roundChocoSpawnToggles[level.roundChocoSpawnColumn[i]].isOn = true;
            }
        }

        if (level.greenBubbleSpawnColumn!=null || level.greenBubbleSpawnColumn.Length==0) {
            for (int i = 0; i < greenBubbleSpawnToggles.Length; i++) {
                greenBubbleSpawnToggles[i].isOn = false;
            }

            for (int i = 0; i < level.greenBubbleSpawnColumn.Length; i++) {
                if (level.greenBubbleSpawnColumn[i] < 0) continue;
                if (level.greenBubbleSpawnColumn[i] >= greenBubbleSpawnToggles.Length) continue;
                greenBubbleSpawnToggles[level.greenBubbleSpawnColumn[i]].isOn = true;
            }
        }

        if (level.treasureSpawnColumn==null || level.treasureSpawnColumn.Length==0) {
            for (int i = 0; i < treasureSpawnToggles.Length; i++) {
                treasureSpawnToggles[i].isOn = false;
            }
        } else {
            for (int i = 0; i < level.treasureSpawnColumn.Length; i++) {
                if (level.treasureSpawnColumn[i] < 0) continue;
                if (level.treasureSpawnColumn[i] >= treasureSpawnToggles.Length) continue;
                treasureSpawnToggles[level.treasureSpawnColumn[i]].isOn = true;
            }
        }

        if (level.strawberrySpawnColumn == null || level.strawberrySpawnColumn.Length==0) {
            for (int i = 0; i < strawBerrySpawnToggles.Length; i++) {
                strawBerrySpawnToggles[i].isOn = false;
            }
        } else {
            for (int i = 0; i < level.strawberrySpawnColumn.Length; i++) {
                if (level.strawberrySpawnColumn[i] < 0) continue;
                if (level.strawberrySpawnColumn[i] >= strawBerrySpawnToggles.Length) continue;
                strawBerrySpawnToggles[level.strawberrySpawnColumn[i]].isOn = true;
            }
        }

        if (level.sugarCherrySpawnColumn == null || level.sugarCherrySpawnColumn.Length==0) {
            for (int i = 0; i < sugarCherrySpawnToggles.Length; i++) {
                sugarCherrySpawnToggles[i].isOn = false;
            }
        } else {
            for (int i = 0; i < level.sugarCherrySpawnColumn.Length; i++) {
                if (level.sugarCherrySpawnColumn[i] < 0) continue;
                if (level.sugarCherrySpawnColumn[i] >= sugarCherrySpawnToggles.Length) continue;
                sugarCherrySpawnToggles[level.sugarCherrySpawnColumn[i]].isOn = true;
            }
        }

        if (level.zellattoSpawnColumn == null || level.zellattoSpawnColumn.Length==0) {
            for (int i = 0; i < zellattoSpawnToggles.Length; i++) {
                zellattoSpawnToggles[i].isOn = false;
            }
        } else {
            for (int i = 0; i < level.zellattoSpawnColumn.Length; i++) {
                if (level.zellattoSpawnColumn[i] < 0) continue;
                if (level.zellattoSpawnColumn[i] >= zellattoSpawnToggles.Length) continue;
                zellattoSpawnToggles[level.zellattoSpawnColumn[i]].isOn = true;
            }
        }

        if (level.addTimeBlockSpawnColumn == null || level.addTimeBlockSpawnColumn.Length==0) {
            for (int i = 0; i < addTimeBlockSpawnToggles.Length; i++) {
                addTimeBlockSpawnToggles[i].isOn = false;
            }
        } else {
            for (int i = 0; i < level.addTimeBlockSpawnColumn.Length; i++) {
                if (level.addTimeBlockSpawnColumn[i] < 0) continue;
                if (level.addTimeBlockSpawnColumn[i] >= addTimeBlockSpawnToggles.Length) continue;
                addTimeBlockSpawnToggles[level.addTimeBlockSpawnColumn[i]].isOn = true;
            }
        }

        // note : 여기서 해당 정보를 바탕으로 editor view를 세팅.
        foreach (LECell c in cells) c.UpdateForLoadedLevelData();
        levelInfoLabel.text = GetLevelInfoText(level);
        //

        ActiveSelectionWithIndex(0);

        InitLevel(LevelIdx, level);

        this.refreshGoalObjText();
    }

    Data.Level SaveLevel()
    {
        Debugger.Assert(level != null);
		Data.Level lv = level;
		lv = winningConditions.GetConditionsInspector (lv);

        // error filter.
        if(false == _checkErrorFilter(ref lv))
        {
            mNeedFix            = true;
            // --- return lv;
        }

        const int max_bd_size   = GameManager.WIDTH * GameManager.HEIGHT;   // 9 x 9

		if (lv.shaded == null) {
			lv.shaded = new int[lv.panels.Length];
		}

        if(lv.eShadeType == null)   // [JAM_SHADE]
            lv.eShadeType       = new int[lv.panels.Length];

        List<int> listTreasureGoal = new List<int>();
        List<int> listPenguinSpawn = new List<int>();

        //if (lv.fallBackTime == null) lv.fallBackTime = new int[81];
		if (lv.panelInfos == null) lv.panelInfos = new string[max_bd_size];
        if( lv.riverInfos == null) lv.riverInfos = new string[max_bd_size];
		if (lv.portalTypes == null) lv.portalTypes = new int[max_bd_size];
		if (lv.portalUIndices == null) lv.portalUIndices = new int[max_bd_size];
        if (lv.portalDIndices == null) lv.portalDIndices = new int[max_bd_size];
		if (lv.startPieces == null) lv.startPieces = new bool[max_bd_size];
        if (lv.conveyorIndices == null) lv.conveyorIndices = new int[max_bd_size];
        if( lv.hasRiver == null )       lv.hasRiver = new bool[max_bd_size];
        if( lv.idxOverRiver == null )   lv.idxOverRiver = new int[max_bd_size];
        if(lv.lifePieceCover == null)   lv.lifePieceCover = new int[max_bd_size];
        if(lv.fenceDirection == null)   lv.fenceDirection = new int[max_bd_size];
        if(lv.panelColors == null)      lv.panelColors = new int[max_bd_size];

        bool isShadeMission = false;
		for (int i = 0; i < cells.Count; i++)
		{
            lv.hasRiver[ConvertFlipYIndex(i)]       = cells[i].hasRiver;
            lv.idxOverRiver[ConvertFlipYIndex(i)]   = cells[i]._idxOverRiver;
            lv.panels[ConvertFlipYIndex(i)] = cells[i].panelIndex;
			lv.strengths[ConvertFlipYIndex(i)] = cells[i].pStrength;
			lv.shaded[ConvertFlipYIndex(i)] = cells[i].shaded;
            lv.eShadeType[ConvertFlipYIndex(i)] = cells[i].eShadeType;
			lv.pieces[ConvertFlipYIndex(i)] = cells[i].pieceIndex;
			lv.colors[ConvertFlipYIndex(i)] = cells[i].colorIndex;
            lv.panelColors[ConvertFlipYIndex(i)] = cells[i].panelColor;
			//lv.fallBackTime[ConvertFlipYIndex(i)] = cells[i].FallbackTime;
			lv.panelInfos[ConvertFlipYIndex(i)] = cells[i].panelInfo;
            lv.riverInfos[ConvertFlipYIndex(i)] = cells[i].riverInfo;
			lv.portalTypes[ConvertFlipYIndex(i)] = cells[i].portalType;
			lv.portalUIndices[ConvertFlipYIndex(i)] = cells[i].PortalUIndex;
            lv.portalDIndices[ConvertFlipYIndex(i)] = cells[i].PortalDIndex;
			lv.startPieces[ConvertFlipYIndex(i)] = cells[i].startPiece;
            lv.changerColorIndex[ConvertFlipYIndex(i)] = cells[i].changerColorIndex;
            lv.conveyorIndices[ConvertFlipYIndex(i)] = cells[i].ConveyorUIndex;

            lv.lifePieceCover[ConvertFlipYIndex(i)] = cells[i].LifePieceCover;
            lv.fenceDirection[ConvertFlipYIndex(i)] = (int)cells[i].DirFence;

            // [AI_MISSION]
            lv.ai_taken_index[ConvertFlipYIndex(i)] = cells[i].ai_taken_index;

            // [NET_SHADE]
            lv.eBarType[ ConvertFlipYIndex(i) ] = cells[i].eBarType;
            lv.indexBar[ ConvertFlipYIndex(i) ] = cells[i].indexBar;
            //

            lv.eHelperType[ ConvertFlipYIndex(i) ]  = cells[i]._nHelperType;

            if (cells[i].hasTreaserGoal) {
                listTreasureGoal.Add(cells[i].X);
                listTreasureGoal.Add(cells[i].Y);
            }
            if (cells[i].hasPenguinSpawn) {
                listPenguinSpawn.Add(cells[i].X);
                listPenguinSpawn.Add(cells[i].Y);
            }

            if (cells[i].shaded >= 0) {
                isShadeMission = true;
            }
        }


        if (isShadeMission) 
		    lv.isClearShadedGame = true;
        else
            lv.isClearShadedGame = false;
        
        lv.treasureGoal = listTreasureGoal.ToArray();
        lv.penguinSpawn = listPenguinSpawn.ToArray();

        List<int> listSpawn     = new List<int>();
        for (int i = 0; i < defaultSpawnToggles.Length; i++) {
            if (defaultSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.defaultSpawnColumn   = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < roundChocoSpawnToggles.Length; i++) {
            if (roundChocoSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.roundChocoSpawnColumn= listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < greenBubbleSpawnToggles.Length; i++) {
            if (greenBubbleSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.greenBubbleSpawnColumn = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < treasureSpawnToggles.Length; i++) {
            if (treasureSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.treasureSpawnColumn = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < strawBerrySpawnToggles.Length; i++) {
            if (strawBerrySpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.strawberrySpawnColumn = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < sugarCherrySpawnToggles.Length; i++) {
            if (sugarCherrySpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.sugarCherrySpawnColumn = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < zellattoSpawnToggles.Length; i++) {
            if (zellattoSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.zellattoSpawnColumn = listSpawn.ToArray();

        listSpawn.Clear();
        for (int i = 0; i < addTimeBlockSpawnToggles.Length; i++) {
            if (addTimeBlockSpawnToggles[i].isOn) {
                listSpawn.Add(i);
            }
        }
        lv.addTimeBlockSpawnColumn = listSpawn.ToArray();

        /*
		string text = JsonWriter.Serialize (lv);
        string fullpath = string.Format("{0}/{1}.txt", GetTextPath(), idx);
		System.IO.File.WriteAllText(fullpath, text);
        Debug.Log("LevelEditorSceneHandler.saveLevel : "+fullpath);
        */

        //Modified = false;
        return lv;
    }

    // note : 각 type당 error 조건 check.
    bool _checkErrorFilter(ref Data.Level lv)
    {
        switch( (EditWinningConditions.MISSION_TYPE)lv.missionType)
        {
        case EditWinningConditions.MISSION_TYPE.COLLECT:
        {
            // 총 goal 수가 5개를 넘으면 안된다.
            const int LIMIT     = 5;
            int totalGoal       = 0;
            if(lv.isGetTypesGame)
            {
                for(int g = 0; g < lv.numToGet.Length; ++g)
                {
                    if(lv.numToGet[g] > 0)
                        ++totalGoal;
                }
            }
            if(lv.isSpecialJewelGame)
            {
                for(int g = 0; g < lv.specialJewels.Length; ++g)
                {
                    if(lv.specialJewels[g] > 0)
                        ++totalGoal;
                }
            }
            if(lv.isMixMission)
            {
                if(lv.countRoundChocho > 0) ++totalGoal;
                if(lv.countRectChocho > 0)  ++totalGoal;
                if(lv.countJamBottom > 0)   ++totalGoal;
                if(lv.countCottonCandy > 0) ++totalGoal;
                if(lv.countSodaCan > 0)     ++totalGoal;
                if(lv.countSugarBlock > 0)  ++totalGoal;
                if(lv.countZellatto > 0)    ++totalGoal;
                if(lv.countCookieJelly > 0) ++totalGoal;
            }
            if(totalGoal > LIMIT)
            {
                Debug.LogError(string.Format("target condition count must be under {0} !", LIMIT));
                return false;
            }
            if(totalGoal <= 0)
            {
                Debug.LogError("target condition count must be more than 0 !");
                return false;
            }
            if(!lv.isGetTypesGame && !lv.isSpecialJewelGame && !lv.isMixMission)
            {
                Debug.LogError("should check collect type at lease 1.");
                return false;
            }
            break;
        }
        case EditWinningConditions.MISSION_TYPE.DEFEAT:
            if(lv.countAiWinBoard <= 0)
            {
                Debug.LogError("goal ai win board count must be more than 1.");
                return false;
            }
            break;
        case EditWinningConditions.MISSION_TYPE.FILL:
            if(lv.countPotion1<=0 && lv.countPotion2<=0 && lv.countPotion3<=0)
            {
                Debug.LogError("goal potion count must be more than 1 at least 1 potion.");
                return false;
            }
            break;
        case EditWinningConditions.MISSION_TYPE.FIND:
            if(lv.countChocoBar <= 0)
            {
                Debug.LogError("goal count choco bar must be more than 1.");
                return false;
            }
            break;
        case EditWinningConditions.MISSION_TYPE.CLEAR:
            if(lv.countCursedBottom <= 0)
            {
                Debug.LogError("goal count cursed bottom(shade) must be more than 1.");
                return false;
            }
            break;
        case EditWinningConditions.MISSION_TYPE.SCORE:
            if(lv.goalScore <= 0)
            {
                Debug.LogError("goal score must be more than 1.");
                return false;
            }
            break;
        default:
            Debug.LogError("Unsupported missioin Type.!!!");
            return false;
        }

        return true;
    }

    void SaveLevelToFile(int idx) 
	{
        StringBuilder output = new StringBuilder();
		JsonWriterSettings writerSetting = new JsonWriterSettings();
		writerSetting.PrettyPrint = true;

		using (JsonWriter writer = new JsonWriter(output, writerSetting))
		{
			var lv = SaveLevel();
			writer.Write(lv);
			//string text = JsonWriter.Serialize (lv);            
			string fullpath = string.Format("Assets/Resources/{0}/{1}.txt", GetTextPath(), idx+1);
			System.IO.File.WriteAllText(fullpath, output.ToString());
			Debug.Log("LevelEditorSceneHandler.SaveLevelToFile : "+fullpath);
		}
    }

    string GetTextPath () 
	{
		return Director.GetTextPath();  // "Assets/Data/text/"+Root.GetPostfix()+"/split";
    }

    delegate bool GetKeyDelegate (KeyCode keyCode);

    void TrackingKeyCombinationsAtToolArea (bool isContinuous)
    {
        GetKeyDelegate getKey;
        if (isContinuous) getKey = Input.GetKey;
        else getKey = Input.GetKeyDown;

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift)) {
            if (getKey(KeyCode.LeftArrow)) {
                ChangeCurrentItemToOtherVariants(false);
            } else if (getKey(KeyCode.RightArrow)) {
                ChangeCurrentItemToOtherVariants(true);
            } else if (getKey(KeyCode.UpArrow)) {
            } else if (getKey(KeyCode.DownArrow)) {
            }
        } else if (Input.GetKey(KeyCode.LeftAlt)) {
            if (getKey(KeyCode.LeftArrow)) {
                ChangeTool(KeyCode.LeftArrow);
            } else if (getKey(KeyCode.RightArrow)) {
                ChangeTool(KeyCode.RightArrow);
            } else if (getKey(KeyCode.UpArrow)) {
                ChangeTool(KeyCode.UpArrow);
            } else if (getKey(KeyCode.DownArrow)) {
                ChangeTool(KeyCode.DownArrow);
            }
        } 
    }

    void TrackingKeyCombinationsAtBoardArea (bool isContinuous)
    {
        if (IsTrackingToolArea()) return;

        GetKeyDelegate getKey;
        if (isContinuous) getKey = Input.GetKey;
        else getKey = Input.GetKeyDown;

        /*
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (getKey(KeyCode.LeftArrow)) {
            } else if (getKey(KeyCode.RightArrow)) {
            } else if (getKey(KeyCode.UpArrow)) {
                LEHelper.SelectedCell.ChangeFallbackTime(1);
            } else if (getKey(KeyCode.DownArrow)) {
                LEHelper.SelectedCell.ChangeFallbackTime(-1);
            }
        } else */
        {
            if (getKey(KeyCode.LeftArrow)) {
                MoveBoardSelection(KeyCode.LeftArrow);
            } else if (getKey(KeyCode.RightArrow)) {
                MoveBoardSelection(KeyCode.RightArrow);
            } else if (getKey(KeyCode.UpArrow)) {
                MoveBoardSelection(KeyCode.UpArrow);
            } else if (getKey(KeyCode.DownArrow)) {
                MoveBoardSelection(KeyCode.DownArrow);
            }
        }

        if (Input.GetKey(KeyCode.Space)) LEHelper.SelectedCell.UpdateCell();
        if (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete)) LEHelper.SelectedCell.Remove();
    }

    void TrackingKeyCombinationsAtBottomBar (bool isContinuous)
    {
        if (Input.GetKey(KeyCode.LeftControl)) {
            if (Input.GetKeyDown(KeyCode.R)) {
                OnClickPlay();
            }
            if (Input.GetKeyDown(KeyCode.S)) {
                SaveLevelToFile(LevelIdx);
            }
        }

        GetKeyDelegate getKey;
        if (isContinuous) getKey = Input.GetKey;
        else getKey = Input.GetKeyDown;

        if (getKey(KeyCode.LeftBracket)) {
            ShiftLevelBy(-1);
        }
        if (getKey(KeyCode.RightBracket)) {
            ShiftLevelBy(1);
        }
    }

    void CheckKeyboardCanInputCoutinuously ()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)
            || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)
            || Input.GetKey(KeyCode.LeftBracket) || Input.GetKey(KeyCode.RightBracket)) {
            if (((Time.time - inputHoldSec) > KEYBOARD_INPUT_DELAY) && (needContinuousKey == false)) {
                needContinuousKey = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket)) {
            inputHoldSec = Time.time;
            needContinuousKey = false;
        }
    }

    void MoveBoardSelection (KeyCode _keyCode)
    {
        Point boardSize = new Point(9, 9);
        int idx = LEHelper.SelectedCell.Index;
        switch (_keyCode) {
            case KeyCode.LeftArrow :
                if ((idx > 0) && (idx % 9 != 0)) idx--;
                break;
            case KeyCode.RightArrow :
                if ((idx < 80) && (idx % 9 != 8)) idx++;
                break;
            case KeyCode.UpArrow :
                if (idx <= (80 - boardSize.y)) idx += boardSize.y;
                break;
            case KeyCode.DownArrow :
                if (idx >= (0 + boardSize.y)) idx -= boardSize.y;
                break;
        }

        ActiveSelectionWithIndex(idx);
    }

    void ChangeTool (KeyCode _keyCode)
    {
        int columLength = pieces.Length;
        int idx = tools.IndexOf(LEHelper.CurrentItem);
        switch (_keyCode) {
            case KeyCode.LeftArrow :
                idx -= columLength;
                if (idx < 0) idx = tools.IndexOf(LEHelper.CurrentItem);
                break;
            case KeyCode.RightArrow :
                idx += columLength;
                if (idx >= tools.Count) idx = tools.IndexOf(LEHelper.CurrentItem);
                break;
            case KeyCode.UpArrow :
                idx--;
                if (idx < 0) idx = tools.IndexOf(LEHelper.CurrentItem);
                break;
            case KeyCode.DownArrow :
                idx++;
                if (idx >= tools.Count) idx = tools.IndexOf(LEHelper.CurrentItem);
                break;
        }

        tools[idx].Switch.isOn = true;
        LEHelper.CurrentItem = tools[idx];
    }

    void ChangeCurrentItemToOtherVariants (bool isNext)
    {
        if (LEHelper.CurrentItem == null) return;

        LEItem item = LEHelper.CurrentItem;
        if ((item.Type == (int)LEItem.TYPE.PANEL) && (item.Index == 22)) return;
        if ((item.variants == null) || (item.variantImages.Length <= 0)) return;

        int index = 0;
        switch ((LEItem.TYPE)item.Type) {
            case LEItem.TYPE.PIECE :
                index = item.Color;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                item.OnSelectedColor(index);
                item.UpdateImage(item.variantImages[index]);
                break;
            case LEItem.TYPE.PANEL :
                index = item.Strength;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                item.OnSelectedStrength(index);
                item.UpdateImage(item.variantImages[index]);
                break;
            case LEItem.TYPE.SHADE :    // [JAM_SHADE]
            {
                index = item.Shaded;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                int param       = index + 100 * (int)item.eShadeType;
                item.OnSelectedShade(param);    // index, item.eShadeType);
                item.UpdateImage(item.variantImages[index]);
                break;
            }
            case LEItem.TYPE.ETC :
                index = item.Index;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                item.OnSelectedETC(index);
                item.UpdateImage(item.variantImages[index]);
                break;
            case LEItem.TYPE.COLOR_CHANGER:
                index = item.Index;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                item.OnSelectedChangerColor(item.Color);
                item.UpdateImage(item.variantImages[index]);
                break;
            case LEItem.TYPE.AI_TAKEN :  // [AI_MISSION]
                index = item.Index;
                index = (int)Mathf.Repeat((isNext ? ++index : --index), item.variantImages.Length);
                item.OnSelected_AI_TAKEN(index);
                item.UpdateImage(item.variantImages[index]);
                break;
            case LEItem.TYPE.PORTAL :
                // FIXME
                break;
        }
    }

    void ActiveSelectionWithIndex (int _index)
    {
        for (int i = 0; i < cells.Count; i++) {
            if (_index == i) {
                cells[i].ActiveSelection(true);
                LEHelper.SelectedCell = cells[i];
            } else {
                cells[i].ActiveSelection(false);
            }
        }
    }

    bool IsTrackingToolArea ()
    {
        return Input.GetKey(KeyCode.LeftAlt);
    }

    string GetLevelInfoText (Data.Level _level)
    {
		// Level info 
        string[] colors = { "Red", "Yellow", "Green", "Blue", "Purple", "Orange", "SkyBlue", "Violet" };
        string[] specialJewelCombineStr = new string[] { "Line", "Line+Line", "Line+Bomb", "Line+Rainbow", "Bomb", "Bomb+Bomb", "Bomb+Rainbow", "Rainbow", "Rainbow+Rainbow"};
        string modeString = "";
        string missionString = "";
        if (_level.isTimerGame == true) {
            modeString = "Timer";
            missionString = string.Format("GivenTime : {0}", _level.givenTime);
        } else if ((_level.isClearShadedGame == true)) {
            modeString = "Ice";
            missionString = string.Format("AllowedMoves : {0}", _level.allowedMoves);
        } else if ((_level.isClearChocolateGame == true)) {
            modeString = "Snowman";
            missionString = string.Format("AllowedMoves : {0}", _level.allowedMoves);
        } else if ((_level.isGetTypesGame == true)) {
            modeString = "Jewel";
            missionString = string.Format("AllowedMoves : {0} /", _level.allowedMoves);

            // exten array.
            if (_level.numToGet.Length < colors.Length)
                _extenArray(ref _level.numToGet, colors.Length);

            for (int i = 0; i < colors.Length; i++) {
                if (_level.numToGet[i] == 0) continue;
                missionString += string.Format(" {0} : {1} / ", colors[i], _level.numToGet[i]);
            }
        } else if ((_level.isTreasureGame == true)) {
            modeString = "Ingredient";
            missionString = string.Format("AllowedMoves : {0} /", _level.allowedMoves);
            if (_level.countPotion1 != 0) missionString += string.Format(" countPotion1 : {0} / ", _level.countPotion1);
            if (_level.countPotion2 != 0) missionString += string.Format(" countPotion2 : {0} / ", _level.countPotion2);
            if (_level.countPotion3 != 0) missionString += string.Format(" countPotion3 : {0} / ", _level.countPotion3);
        } else if (_level.isSpecialJewelGame == true) {
            modeString = "Special jewel";
            missionString = string.Format("AllowedMoves : {0} /", _level.allowedMoves);
            for (int i = 0; i < specialJewelCombineStr.Length; i++) {
                if (_level.specialJewels[i] == 0) continue;
                missionString += string.Format(" {0} : {1} /", specialJewelCombineStr[i], _level.specialJewels[i]);
            }
        } else if (_level.isPenguinGame == true) {
            modeString = "Penguin";
            missionString = string.Format("AllowedMoves : {0} /", _level.allowedMoves);
            missionString += string.Format(" numberOfPenguin : {0}", _level.numberOfPenguin);
        } else if (_level.isYetiGame == true) {
            modeString = "Yeti";
            missionString = string.Format("AllowedMoves : {0} /", _level.allowedMoves);
            missionString += string.Format(" YetiHealth : {0}", _level.yetiHealth);
        } else if (_level.isMaxMovesGame == true) {
            modeString = "Move";
            missionString = string.Format("AllowedMoves : {0}", _level.allowedMoves);
        } else {
            modeString = "Unknown";
        }

        return string.Format("{0} Level [DataSet:{1}] Hard Level:{4}\n{2} mode\n{3}", (LevelIdx+1), Root.GetPostfix(), modeString, missionString,_level.hardLevel.ToString());
    }

	void OnEscape () 
	{
		Scene.ChangeTo("WorldScene");
	}
	
	private int GetFileCount( string path)
	{
		int nReturn = 0;
		if(Directory.Exists(path))
		{
			string [] pszfileEntries = Directory.GetFiles(path);
			
			for(int i = 0; i < pszfileEntries.Length; ++i)
			{
				string[] temp = pszfileEntries[i].Split('.');

				if(temp[temp.Length - 1] == "txt")
					nReturn = nReturn + 1;					
			}
		}

		return nReturn;
	}

    public void MergeLevelsToTheme ( bool skipDLG = false, bool skipTreasure =  false) 
	{
        /* -
		if(worldMapBuilder == null)
		{
			if(DisplayDialog("주의", "WorldMapBuilder = null", "예", "아니오") == false) return;
		}
		
        if (!skipDLG)
        {
            if (DisplayDialog("주의", 
                "split폴더의 모든 레벨로 merge된 레벨파일을 대치합니다. 계속할까요?", 
                "예", "아니오") == false) return;
        }

        var dataSet = PlayerPrefs.GetString("DataSet");
		Root.SetPostfix(dataSet);
		var rootData = Root.Load();
		//Root.Data.Initialize();
		if(rootData.gameData == null)
		{
			rootData.gameData = new Data.GameData();
			rootData.gameData.key = "0";
		}
		
		//rootData.gameData.key = "gameData";
		
        string path = "Assets/Data/text/"+dataSet+"/";
        string dirPath = path + "split";
		int count = GetFileCount(dirPath);
		if(count <= 0)
		{
			Debug.LogError("Can't find directory : " + dirPath);
			return;
		}
		
		List<Data.Level> lvs = new List<Data.Level>();
		for(int i = 0; i < count; ++i)
		{
			string fname = string.Format("{0}/{1}.txt", dirPath, (i + 1));
			if(!File.Exists(fname)) 
			{
				Debug.LogError("file not found:"+fname);
				continue;
			}

			Debug.Log(" reading file:"+fname);
			StreamReader streamReader = new StreamReader(fname);
			string data = streamReader.ReadToEnd();
			streamReader.Close();
			var lv = JsonReader.Deserialize<Data.Level>(data); 
			lv.Index = i;
			lv.key = string.Format("levels.{0}", i);
			lvs.Add(lv);
        }
		
		rootData.levels = lvs.ToArray();
        if (!skipTreasure)
        {
            worldMapBuilder.PaserData();
            rootData.treasureIndex = worldMapBuilder.TreasureList.ToArray();    
        }

        Root.Save(rootData);
        AssetDatabase.Refresh();
        */
    }

    IEnumerator CoNormalizeAllLevels() {
#if UNITY_EDITOR
		
		// todo check list
//		if(LevelEditorSceneHandler.EditorMode == false)
//		{
//			var dataSet = PlayerPrefs.GetString("DataSet");
//			Root.SetPostfix(dataSet);
//			var rootData = Root.Load();			
//		}

		string path = GetDataPath();
        var files = System.IO.Directory.GetFiles(path+"split", "*.txt");
        int idx = 0;
        EditorUtility.DisplayProgressBar(
            "Normalize All Levels", 
            string.Format("{0}/{1} {2:0.00}%", idx, files.Length, idx/(float)files.Length*100), 
            idx/(float)files.Length);

        bypassModifiedCheck = true;
        foreach(var f in files) {
            if(f.EndsWith(".txt")) {
                string fn = System.IO.Path.GetFileNameWithoutExtension(f);
                int lv = 1;
                EditorUtility.DisplayProgressBar(
                    "Normalize All Levels", 
                    string.Format("{0}/{1} {2:0.00}%", idx, files.Length, idx/(float)files.Length*100), 
                    idx/(float)files.Length);
                if(System.Int32.TryParse(fn, out lv)) {
                    LoadLevelFromFile(lv);
                    //FIXME for repaint
                    int am = winningConditions.allowedMoves;
                    winningConditions.allowedMoves = am;
                    yield return null;
                    yield return null;
                    SaveLevelToFile(lv);
                    Debug.Log(f);
                }
                else {
                    Debug.LogError("NormalizeAllLevels fail : "+f);
                }
            }
            idx++;
        }
        bypassModifiedCheck = false;
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
#endif
    }

    public void NormalizeAllLevels() {
        StartCoroutine(CoNormalizeAllLevels());
    }
    
    public void SplitThemeToLevels ()
	{
        /* -
        if (DisplayDialog("주의", 
            "merge된 레벨파일로 split폴더의 모든 파일을 대치합니다. 계속할까요?", 
            "예", "아니오") == false) return;

        var dataSet = PlayerPrefs.GetString("DataSet");
        Root.SetPostfix(dataSet);
        var rootData = Root.Load();

        string path = GetDataPath();

        string dirPath = path + "split";
        if (Directory.Exists(dirPath) == false) Directory.CreateDirectory(dirPath);

		JsonWriterSettings writerSetting = new JsonWriterSettings();
		writerSetting.PrettyPrint = true;
		
		for (int i = 0; i < rootData.levels.Length; i++) 
		{
			StringBuilder output = new StringBuilder();
			using (JsonWriter writer = new JsonWriter(output, writerSetting))
			{
				writer.Write(rootData.levels[i]);
				string filePath = dirPath + "/" + i.ToString() + ".txt";
				StreamWriter streamWriter = new StreamWriter(filePath);
				streamWriter.Write(output.ToString());
				streamWriter.Close();
			}
		}
		
        NormalizeAllLevels();
        */
    }

    static string GetDataPath () {
        return "Assets/Data/text/"+Root.GetPostfix()+"/";
    }

//    static int GetEpisodeLevelCount(Data.Theme thm) {
//        int cnt = 0;
//        foreach(var e in thm.episodes) {
//            int nc = 0;
//            if(System.Int32.TryParse(e.count, out nc))
//                cnt += nc;
//        }
//        return cnt;
//    }
#endif
    
    void _extenArray(ref int[] srcArray, int destSize)
    {
        if(null==srcArray || srcArray.Length>destSize)
            return;

        int[] _temp             = srcArray.ToArray();
        srcArray                = new int[ destSize ];
        for(int qq = 0; qq < _temp.Length; ++qq)
            srcArray[qq]        = _temp[qq];
    }

    #region // MISSION GOAL OBJECT DISPLAY & APPLY.

    // round choco, rect-choco, jam bottom, cotton candy, sugar block, zellato
    Dictionary<string, int> calculateObjectGoalCount()
    {
        Dictionary<string, int> dicNameCount = new Dictionary<string, int>();
        for(int q = 0; q < cells.Count; ++q)
        {
            int count           = 1;
            switch( (LEItem.SHADE_TYPE) cells[q].eShadeType)
            {
            case LEItem.SHADE_TYPE.CURSE:
                count           = (cells[q].shaded + 1);
                _addCount(ref dicNameCount, "cursed bottom", count);
                break;
            case LEItem.SHADE_TYPE.JAM:
                count           = (cells[q].shaded + 1);
                _addCount(ref dicNameCount, "jam bottom", count);
                break;
            default:
                break;
            }

            if(10 == cells[q].pieceIndex)       _addCount(ref dicNameCount, "round choco", 1);
            else if(23 == cells[q].pieceIndex)  _addCount(ref dicNameCount, "zellatto", 1);
            else if(24 == cells[q].pieceIndex)  _addCount(ref dicNameCount, "cookie jelly", 1);

            if(4 == cells[q].panelIndex)        _addCount(ref dicNameCount, "rect choco", 1);
            else if(7 == cells[q].panelIndex)   _addCount(ref dicNameCount, "cotton", 1);
            
            if(cells[q]._listIdxChocoBar.Count > 0)
                _addCount(ref dicNameCount, "cheese bar", 1);
        }
        return dicNameCount;
    }

    void _addCount(ref Dictionary<string, int> dicNameCount, string strKey, int addCount)
    {
        if(true == dicNameCount.ContainsKey( strKey ))
            dicNameCount[ strKey ] = dicNameCount[ strKey ] + addCount;
        else 
            dicNameCount[ strKey ] = addCount;
    }

    public void refreshGoalObjText()
    {
        Debug.Assert(null != _shadeGroup);

        Dictionary<string, int> dicNameCount = calculateObjectGoalCount();
        IEnumerator<string> itr = dicNameCount.Keys.GetEnumerator();
        int MAX_UI          = 5;// Mathf.Min(dicNameCount.Count, 5);
        for (int z = 0; z < MAX_UI; ++z)
        {
            itr.MoveNext();

            Text txt        = _shadeGroup.transform.Find("txtGoalObj"+z).GetComponent<Text>();
            if(null== txt)  continue;
            if(z < dicNameCount.Count)
                txt.text    = string.Format("{0} count : {1}", itr.Current, dicNameCount[ itr.Current ]);
            else 
                txt.text    = "";

            _shadeGroup.transform.Find("btnApplyGoal"+ z).gameObject.SetActive( z < dicNameCount.Count );
        }   
    }
    // 현재 editor에 있는 shade 수 대로 goal setting을 한다.
    public void onApplyObjectToGoal0() { _applyGoal(0); }
    public void onApplyObjectToGoal1() { _applyGoal(1); }
    public void onApplyObjectToGoal2() { _applyGoal(2); }
    public void onApplyObjectToGoal3() { _applyGoal(3); }
    public void onApplyObjectToGoal4() { _applyGoal(4); }
    void _applyGoal(int idx)
    {
        string strKey = "";

        Dictionary<string, int> dicNameCount = calculateObjectGoalCount();
        IEnumerator<string> itr = dicNameCount.Keys.GetEnumerator();
        int MAX_UI          = 5;
        for (int z = 0; z < MAX_UI; ++z)
        {
            itr.MoveNext();
            if(idx == z)    {  strKey  = itr.Current; break; }
        }

        switch(strKey)
        {
        case "cursed bottom":
            if(EditWinningConditions.MISSION_TYPE.CLEAR == winningConditions.missionType)
                winningConditions.countCursedBottom = dicNameCount[ strKey ];
            break;
        case "jam bottom":
            //winningConditions.missionType       = EditWinningConditions.MISSION_TYPE.COLLECT;
            winningConditions.countJamBottom    = dicNameCount[ strKey ];
            break;
        case "round choco":
            //winningConditions.missionType       = EditWinningConditions.MISSION_TYPE.COLLECT;
            winningConditions.countRoundChocho  = dicNameCount[ strKey ];
            break;
        case "zellatto":
            //winningConditions.missionType       = EditWinningConditions.MISSION_TYPE.COLLECT;
            winningConditions.countZellatto     = dicNameCount[ strKey ];
            break;
        case "rect choco":
            //winningConditions.missionType       = EditWinningConditions.MISSION_TYPE.COLLECT;
            winningConditions.countRectChocho   = dicNameCount[ strKey ];
            break;
        case "cotton":
            //winningConditions.missionType       = EditWinningConditions.MISSION_TYPE.COLLECT;
            winningConditions.countCottonCandy  = dicNameCount[ strKey ];
            break;
        case "cheese bar":
            if(EditWinningConditions.MISSION_TYPE.FIND == winningConditions.missionType)
                winningConditions.countChocoBar = dicNameCount[ strKey ];
            break;
        case "cookie jelly":
            winningConditions.countCookieJelly      = dicNameCount[ strKey ];
            break;
        }
    }
    #endregion
}
