using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using JsonFx.Json;
using System.Collections.Generic;

public class LECell : MonoBehaviour {
    public Image defaultImg;
    public Image shadeImg;
    public Image portalImg;
    public Image backPanelImg;
    public Image frontPanelImg;
    public Image pieceImg;
    public Text fallbackTimeLabel;
    public Text portalUIndexLabel, portalDIndexLabel, conveyorUIndexLabel;
    public Image treasureGoalImg;
    public Image penguinSpawnImg;
    public Image selectionImg;
    public Image ai_taken_Img;  // [AI_MISSION], [CHOCO_BAR]
    public Image imgBars;       // [NET_SHADE]
    public Image _imgHelper;
    public Image _imgRiver, _imgOverRiver;

    Image _imgFence;
    JMF_DIRECTION _dirFence     = JMF_DIRECTION.NONE;
    public JMF_DIRECTION DirFence
    {
        get { return _dirFence; }
        set { _dirFence = value; }
    }

    Image _imgPieceCover;
    int _nLifePieceCover        = 0;
    public int LifePieceCover
    {
        get { return _nLifePieceCover; }
        set { _nLifePieceCover = value; }
    }

    public int Index { get; private set; }
	public Point PT { get; private set; }
    public int X { get { return PT.X; } }
    public int Y { get { return PT.Y; } }

    public bool hasRiver        = false;
    public int _idxOverRiver    = -1;
	public int panelIndex = 0;
	public int pStrength = -1;
	public int shaded = -1;
    public int eShadeType = -1;         // [JAM_SHADE]
	public int pieceIndex = 0;
	public int colorIndex = 0;
    public int changerColorIndex = -1;   // [CHANGER_COLOR]
	public string panelInfo = "";
    public string riverInfo = "";
	public bool startPiece = true;
    public bool hasTreaserGoal = false;
    public bool hasPenguinSpawn = false;
    public int portalType = 0;
    public int ai_taken_index   = -1;       // [AI_MISSION]
    public int eBarType         = -1;       // [NET_SHADE]
    public int indexBar         = -1;       // [NET_SHADE]
    public Text _txtShadeCount, _txtInfo, _txtBlockMissionCount;
    // only for choco-bar, only 0 index has this.
    public List<int>            _listIdxChocoBar  = new List<int>();
    public int _nHelperType     = -1;       // LEItem.HELPER_TYPE
    public int panelColor       = -1;

    bool isMouseOver = false;

    int portalDIndex = 0;
    public int PortalDIndex {
        get { return portalDIndex; }
        set {
            portalDIndex = value;
            portalDIndexLabel.gameObject.SetActive(portalDIndex > 0);
            portalDIndexLabel.text = (portalDIndex > 0) ? portalDIndex.ToString() : "";
        }
    }
    int portalUIndex = 0;
    public int PortalUIndex {
        get { return portalUIndex; }
        set {
            portalUIndex = value;
            portalUIndexLabel.gameObject.SetActive(portalUIndex > 0);
            portalUIndexLabel.text = (portalUIndex > 0) ? portalUIndex.ToString() : "";
        }
    }

    int conveyorUIndex = 0;
    public int ConveyorUIndex {
        get { return conveyorUIndex; }
        set {
            conveyorUIndex      = value;
            conveyorUIndexLabel.gameObject.SetActive(conveyorUIndex > 0);
            conveyorUIndexLabel.text = (conveyorUIndex > 0) ? conveyorUIndex.ToString() : "";
        }
    }

    int fallbackTime = 0;
    public int FallbackTime {
        get { return fallbackTime; }
        set {
            fallbackTime = value;
            fallbackTimeLabel.gameObject.SetActive(fallbackTime > 0);
            fallbackTimeLabel.text = (fallbackTime > 0) ? fallbackTime.ToString() : "";
        }
    }

    void Awake()
	{
        _imgPieceCover          = transform.Find("PieceCover").GetComponent<Image>();
        _imgFence               = transform.Find("imgFence").GetComponent<Image>();
    }

    void Update ()
    {
        TrackingMouseWheel ();
        TrackingConveyorDrawing();
        TrackingEraser();
    }

    public void OnPointerEnter ()
    {
        isMouseOver = true;
        if (Input.GetKey(KeyCode.Mouse0) == false) return;

        UpdateCell();

        //if ((LEHelper.CurrentItem.Type == (int)LEItem.TYPE.PANEL) && (LEHelper.CurrentItem.Index == 22)) {
        //    LEHelper.conveyorList.Add(this);
        //}
        if(null != LEHelper.CurrentItem)
        {
            if(LEItem.TYPE.RIVER == (LEItem.TYPE)LEHelper.CurrentItem.Type)
                LEHelper.conveyorList.Add( this );

            if (null != LEHelper.CurrentItem.variants)
                LEHelper.CurrentItem.variants.SetActive( false );
        }
    }

    public void OnPointerExit ()
    {
        isMouseOver = false;
    }


#region load from level data file
    public void UpdateForLoadedLevelData ()
    {
        LevelEditorSceneHandler leScene = LevelEditorSceneHandler.Instance;

        //Draw Piece
        bool isEmptyPiece = ((CanPlacePiece() == false) || (startPiece == false));

        if(pieceIndex<0 || pieceIndex>=leScene.pieces.Length || true==isEmptyPiece ||null==leScene.pieces[pieceIndex]) {
            pieceImg.sprite = null;
        }
        else
        {
            if (null!=leScene.pieces[pieceIndex].variantImages && leScene.pieces[pieceIndex].variantImages.Length>0)
            {
                // [ROUND_CHOCO]
                if(0!=pieceIndex && pStrength>=1 && pStrength<leScene.pieces[pieceIndex].variantImages.Length+1 && 0==panelIndex)
                {
                    if(24==pieceIndex && 2==colorIndex)
                    {
                        pieceImg.sprite = leScene._cookieJellyPiece2.variantImages[pStrength-1].sprite;
                        pieceImg.color  = Color.white;
                        pieceImg.transform.rotation = leScene._cookieJellyPiece2.variantImages[pStrength-1].transform.rotation;
                    }
                    else
                    {
                        colorIndex      = Mathf.Max(0, colorIndex);
                        pieceImg.sprite = leScene.pieces[pieceIndex].variantImages[pStrength-1].sprite;
                        pieceImg.color  = Color.white;
                        pieceImg.transform.rotation = leScene.pieces[pieceIndex].variantImages[pStrength-1].transform.rotation;
                    }
                }
                else
                {
                    colorIndex = Mathf.Max(0, colorIndex);
                    pieceImg.sprite = leScene.pieces[pieceIndex].variantImages[colorIndex].sprite;
                    pieceImg.color = leScene.pieces[pieceIndex].variantImages[colorIndex].color;
                    pieceImg.transform.rotation = leScene.pieces[pieceIndex].variantImages[colorIndex].transform.rotation;
                }
            } else {
                pieceImg.sprite = leScene.pieces[pieceIndex].icon.sprite;
                pieceImg.color = leScene.pieces[pieceIndex].icon.color;
                pieceImg.transform.rotation = leScene.pieces[pieceIndex].icon.transform.rotation;
            }
        }

        //Draw default panel 
        defaultImg.gameObject.SetActive((panelIndex != 1) && (panelIndex != 17) && (panelIndex != 23));

        //Draw panel 
        if(panelIndex>=0 && panelIndex<leScene.panels.Length)
        {
            Image panelImage = (leScene.panels[panelIndex].Location == LEItem.LOCATION.FRONT) ? frontPanelImg : backPanelImg;
            if ((panelIndex != 0) && (panelIndex != 1) && (panelIndex != 23))
            {
                if (leScene.panels[panelIndex].variantImages.Length > 0)
                {
                    if (IsCreatorPanel() == true) {
                        panelImage.sprite = leScene.panels[panelIndex].variantImages[panelIndex - 11].sprite;
                        panelImage.color = leScene.panels[panelIndex].variantImages[panelIndex - 11].color;
                    } else if (IsCakePanel() == true) {
                        panelImage.sprite = leScene.panels[panelIndex].variantImages[panelIndex - 18].sprite;
                        panelImage.color = leScene.panels[panelIndex].variantImages[panelIndex - 18].color;
                    }
                    // Block Mission Pannel ?
                    else if(true == IsBlockMissionPanel())
                    {
                        panelImage.sprite = leScene.panels[panelIndex].variantImages[Mathf.Max(0, panelColor - 1)].sprite;
                        //panelImage.color = leScene.panels[panelIndex].variantImages[Mathf.Max(0, panelColor - 1)].color;
                        _txtBlockMissionCount.text = (pStrength).ToString();
                    }
                    else 
                    {
                        if(panelColor > 0)
                        {
                            if(1 == pStrength)
                                panelImage.sprite = leScene.panels[panelIndex].variantImages[Mathf.Max(0, panelColor - 1)].sprite;
                            else if(2 == pStrength)
                                panelImage.sprite = leScene.panelColorBoxS2.variantImages[Mathf.Max(0, panelColor - 1)].sprite;
                            else if(3 == pStrength)
                                panelImage.sprite = leScene.panelColorBoxS3.variantImages[Mathf.Max(0, panelColor - 1)].sprite;
                        }
                        else 
                            panelImage.sprite = leScene.panels[panelIndex].variantImages[Mathf.Max(0, pStrength - 1)].sprite;

                        panelImage.color = leScene.panels[panelIndex].variantImages[Mathf.Max(0, pStrength - 1)].color;
                    }
                } else {
                    panelImage.sprite = leScene.panels[panelIndex].icon.sprite;
                    panelImage.color = leScene.panels[panelIndex].icon.color;
                }
            } else {
                frontPanelImg.sprite = null;

                if (panelIndex == 23) {
                    backPanelImg.sprite = leScene.panels[panelIndex].variantImages[1].sprite;
                    backPanelImg.color = leScene.panels[panelIndex].variantImages[1].color;
                } else {
                    backPanelImg.sprite = null;
                }
            }        

            //Draw panel by  panelInfo
            bool hasPanelInfo = (string.IsNullOrEmpty(panelInfo) == false);
            if (hasPanelInfo == true) {
                switch (panelIndex) {
                    case 10 :
                    case 35:
                    {
                        CavePanel.Info caveInfo = JsonReader.Deserialize<CavePanel.Info>(panelInfo);
                        if(35==panelIndex && 2==pStrength)
                        {
                            panelImage.sprite = leScene._2x2Color6Pnl.variantImages[caveInfo.Index].sprite;
                            panelImage.color = leScene._2x2Color6Pnl.variantImages[caveInfo.Index].color;
                        }
                        else
                        {
                            panelImage.sprite = leScene.panels[panelIndex].variantImages[caveInfo.Index].sprite;
                            panelImage.color = leScene.panels[panelIndex].variantImages[caveInfo.Index].color;
                        }
                        break;
                    }
                    case 22 :
                        //ConveyorPanel.Info ConveyorInfo = JsonReader.Deserialize<ConveyorPanel.Info>(panelInfo);
                        //DrawConveyor(ConveyorInfo);
                        break;
                }
            }

            if(true == hasRiver)
            {
                ConveyorPanel.Info ConveyorInfo = JsonReader.Deserialize<ConveyorPanel.Info>(this.riverInfo);
                DrawConveyor(ConveyorInfo);

                if(_idxOverRiver >= 0)
                {
                    //LEItem item             = LevelEditorSceneHandler.Instance._riverWaffle;
                    //_imgOverRiver.sprite    = item.icon.sprite;
                }
            }            
        }

         
        //Draw shade
        if (shaded!=-1 && eShadeType!=-1) { // [JAM_SHADE]
            shadeImg.sprite     = leScene.shade[eShadeType].variantImages[shaded].sprite;
            _txtShadeCount.text = (shaded+1).ToString();        
        } else {

            // draw changer-color
            if(changerColorIndex >= 0)
            {
                shadeImg.sprite = leScene.colorChanger.variantImages[changerColorIndex].sprite;
                shadeImg.color  = Color.white;
            }
            else 
                shadeImg.sprite = null;
        }

        //Draw portal
        if (portalType > 0) {
            RectTransform rt = portalImg.GetComponent<RectTransform>();

            switch((PORTAL_TYPE)portalType)
            {
            case PORTAL_TYPE.UP:
                rt.rotation     = Quaternion.Euler(0, 0, 180);
                portalImg.sprite= leScene.portal[portalType-1].icon.sprite;
                break;
            case PORTAL_TYPE.DOWN:
            case PORTAL_TYPE.ALL:
                rt.rotation     = Quaternion.identity;
                portalImg.sprite= leScene.portal[portalType-1].icon.sprite;
                break;
            }
        } else {
            portalImg.sprite = null;
        }

        // [AI_MISSION] - Draw Ai Taken.
        if(0==ai_taken_index || 1==ai_taken_index)
            ai_taken_Img.sprite = leScene.ai_taken_items.variantImages[ ai_taken_index ].sprite;
        //

        // [CHOCO_BAR]
        if(indexBar>=0 && eBarType>=0)
        {
            // imgBars.sprite  = leScene.bar_choco.icon.sprite;
            LEItem.CHOCO_BAR    eType = (LEItem.CHOCO_BAR)eBarType;
            eBarType            = -1;
            _buildChocoBar(eType, LevelEditorSceneHandler.Instance.bar_choco, this.indexBar);
        }

        switch((LEItem.HELPER_TYPE)_nHelperType)
        {
        case LEItem.HELPER_TYPE.DROPPER:
            _imgHelper.sprite   = leScene._itemHelper.icon.sprite;
            break;
        default:                break;
        }

        //Draw treasureGoal & penguinSpawn
        imgBars.gameObject.SetActive(indexBar>=0);
        treasureGoalImg.gameObject.SetActive(hasTreaserGoal);
        penguinSpawnImg.gameObject.SetActive(hasPenguinSpawn);
        
        // piece over ice.
        if(1==_nLifePieceCover || 2==_nLifePieceCover)
            _imgPieceCover.sprite   = leScene._pieceOverIce.variantImages[ _nLifePieceCover-1 ].sprite;
        //

        if(DirFence != JMF_DIRECTION.NONE)
        {
            _imgFence.sprite        = leScene._fence.icon.sprite;
            _refreshFenceView();
        }
        else _imgFence.sprite       = null;

        //if(hasTreaserGoal || hasPenguinSpawn) - by wayne. pannel과 공유하도록 변경.
        //    panelIndex          = 0;
        UpdateImageActive();
    }
#endregion

#region Control Show/Hide objects
    public void ActiveSelection (bool active)
    {
        selectionImg.gameObject.SetActive(active);
    }

    void ActiveConveyorPortal (int index)
    {
        if (index < 0) {
            foreach (Transform t in transform.Find("ConveyorPortal")) {
                t.gameObject.SetActive(false);
            }
            return;
        }

        int i = 0;
        foreach (Transform t in transform.Find("ConveyorPortal")) {
            t.gameObject.SetActive(i == index);
            i++;
        }
    }
#endregion

#region Update data with appearnce (Piece, Panel and so on)
    public void UpdateCell ()
    {
        if (LEHelper.CurrentItem == null) return;

        switch ((LEItem.TYPE)LEHelper.CurrentItem.Type)
        {
        case LEItem.TYPE.PIECE :    UpdatePiece(LEHelper.CurrentItem);      break;
        case LEItem.TYPE.PANEL :    UpdatePanel(LEHelper.CurrentItem);      break;
        case LEItem.TYPE.SHADE :    UpdateShade(LEHelper.CurrentItem);      break;
        case LEItem.TYPE.ETC :      UpdateETC(LEHelper.CurrentItem);        break;
        case LEItem.TYPE.PORTAL :   UpdatePortal(LEHelper.CurrentItem);     break;
        case LEItem.TYPE.AI_TAKEN:  Update_Ai_Taken(LEHelper.CurrentItem);  break;
        case LEItem.TYPE.CHOCO_BAR: UpdateChocoBar(LEHelper.CurrentItem);   break;      // [NET_PANNEL]
        case LEItem.TYPE.COLOR_CHANGER: UpdateColorChanger(LEHelper.CurrentItem);       break;
        case LEItem.TYPE.HELPER:    UpdateHelper(LEHelper.CurrentItem);     break;
        case LEItem.TYPE.OVER_RIVER: UpdateOverRiver(LEHelper.CurrentItem); break;
        case LEItem.TYPE.RIVER:     UpdateRiver(LEHelper.CurrentItem);      break;
        case LEItem.TYPE.PIECE_COVER: UpdatePieceCover(LEHelper.CurrentItem); break;
        case LEItem.TYPE.FENCE:     UpdateFence(LEHelper.CurrentItem);      break;
        }

        LevelEditorSceneHandler.Instance.refreshGoalObjText();
    }

    public void Init (int index)
    {
        Index = index;
        PT = new Point(index % 9, index / 9);
        ResetAllImages();
    }

    void UpdatePiece (LEItem item)
    {
        if (CanPlacePiece() == false) return;

        startPiece = true;
        pieceIndex = item.Index;
        colorIndex = item.Color;
        // [ROUND_CHOCO]
        if(item.Strength >= 0)
            pStrength   = item.Strength;
        //
        pieceImg.sprite = item.Icon.sprite;
        pieceImg.transform.rotation = item.Icon.transform.rotation;
        pieceImg.color = item.Icon.color;
        MakeFrogAsUnique();
        UpdateImageActive();
    }

    void UpdatePanel (LEItem item)
    {
        backPanelImg.transform.rotation = Quaternion.identity;
        backPanelImg.transform.localScale = Vector3.one;
        panelInfo = "";

        if (IsTimeBomb() == false) FallbackTime = 0;

        Image panelImage = (item.Location == LEItem.LOCATION.FRONT) ? frontPanelImg : backPanelImg;
        switch (item.Index) {
            case 0 : //basic
                startPiece = true;
                panelIndex = item.Index;
                pStrength = item.Strength;
                defaultImg.gameObject.SetActive(true);
                pieceImg.sprite = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].sprite;
                pieceImg.color = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].color;
                break;
            case 1 : //empty
                shadeImg.sprite = null;
                shaded = -1;
                ResetPanael();
                break;
            case 10 : //cave
                RemovePanel();
                if ((X < 8) && (Y < 8)) MakeWholeCave(item); 
                break;
            case 17 : //creator
                defaultImg.gameObject.SetActive(false);
                panelIndex = item.Index;
                pStrength = item.Strength;
                backPanelImg.sprite = item.Icon.sprite;
                backPanelImg.color = item.Icon.color;
                frontPanelImg.sprite = null;
                frontPanelImg.color = Color.white;
                break;
            case 18 : //cake
                RemovePanel();
                if ((X < 8) && (Y < 8)) MakeWholeCake(item);
                break;
            case 23 :
                shadeImg.sprite = null;
                shaded = -1;
                ResetPanael(23);

                backPanelImg.sprite = item.Icon.sprite;
                backPanelImg.color = item.Icon.color;
                break;
            case 24 :
                RemovePanel();
                if ((X < 8) && (Y < 8)) MakeWholeCave(item); 
                break;
            case 35:            // 2x2 color bomb
                RemovePanel();
                if ((X < 8) && (Y < 8)) MakeWholeCave(item); 
                break;
            case 32:            // color box.
            case 40:            // block mission.
                RemovePanel();
                defaultImg.gameObject.SetActive(true);
                panelColor          = item.Color;
                panelIndex          = item.Index;
                panelImage.sprite   = item.icon.sprite; // LevelEditorSceneHandler.Instance.panels[panelIndex].variantImages[panelColor-1].sprite;
                panelImage.color    = item.Icon.color;
                pStrength           = item.Strength;
                if(item.Index==40) 
                    _txtBlockMissionCount.text = pStrength.ToString();
                break;
            default :
                RemovePanel();
                defaultImg.gameObject.SetActive(true);
                panelIndex = item.Index;
                pStrength = item.Strength;
                panelImage.sprite = item.Icon.sprite;
                panelImage.color = item.Icon.color;
                break;
        }

        if (CanPlacePiece() == false) ResetPiece();

        UpdateImageActive();
    }

    void UpdateShade (LEItem item)
    {
        RemoveETC();
        RemoveColorChanger();

        shaded                  = item.Shaded;
        eShadeType              = (int)item.eShadeType; // [JAM_SHADE]
        shadeImg.sprite         = item.Icon.sprite;
        UpdateImageActive();

        Debug.Assert(null != _txtShadeCount);
        _txtShadeCount.text     = (shaded+1).ToString();
    }

    void UpdateETC (LEItem item)
    {
        RemoveShade();
        RemoveColorChanger();

        switch (item.Index)
        {
            case 0 : 
                hasTreaserGoal  = true;
                treasureGoalImg.gameObject.SetActive(true);
                //panelIndex      = 0;
                break;
            case 1 : 
                hasPenguinSpawn = true;
                penguinSpawnImg.gameObject.SetActive(true);
                //panelIndex      = 0;
                break;
        }
    }

    void UpdateColorChanger(LEItem item)
    {
        RemoveShade();
        RemoveETC();

        shadeImg.sprite         = item.Icon.sprite;
        changerColorIndex       = item.Color;

        UpdateImageActive();
    }

    void UpdateHelper(LEItem item)
    {
        // note : 이 시점에는 dropper type만 존재.
        switch(item.eHelperType)
        {
        case LEItem.HELPER_TYPE.DROPPER:
            _imgHelper.sprite   = item.icon.sprite;
            _nHelperType        = (int)item.eHelperType;
            UpdateImageActive();
            break;
        }
    }

    void UpdateOverRiver(LEItem item)
    {
        // river가 없으면 하지 않는다.
        if(!hasRiver)           return;

        _idxOverRiver           = 0;
        _imgOverRiver.sprite    = item.icon.sprite;

        UpdateImageActive();
    }

    void UpdateRiver(LEItem item)
    {
        hasRiver                = true;
        _imgRiver.sprite        = item.icon.sprite;
        riverInfo               = "";

        UpdateImageActive();
    }

    // piece cover - ice.
    void UpdatePieceCover(LEItem item)
    {
        _nLifePieceCover        = (int)item.strength;
        _imgPieceCover.sprite   = item.icon.sprite;

        UpdateImageActive();
    }

    void UpdateFence(LEItem item)
    {
        DirFence                = JMF_DIRECTION.LEFT;
        _imgFence.sprite        = item.icon.sprite;

        UpdateImageActive();
        _refreshFenceView();
    }

    // [AI_MISSIOIN]
    void Update_Ai_Taken (LEItem item)
    {
        ai_taken_Img.gameObject.SetActive(true);
        ai_taken_Img.sprite = item.icon.sprite;
        ai_taken_index      = item.index;
        //switch (item.Index)
    }

    // [NET_PANNEL]
    void UpdateChocoBar(LEItem item)
    {
        // this is actually some kind of pannel.
        _updateChocoBar(item.eBarType, item);
    }

    void _buildChocoBar(LEItem.CHOCO_BAR eType, LEItem item, int indexBarComponent)
    {
        string strPicName       = "";
        Vector2 vScale          = Vector2.one;
        
        switch(eType)
        {
        case LEItem.CHOCO_BAR._1X1: // x
            strPicName          = "pngPics/imgSingle";
            vScale              = Vector2.one;
            break;
         case LEItem.CHOCO_BAR._2X1:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgSingleL"; vScale  = Vector2.one; }
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgSingleL"; vScale  = new Vector2(-1, 1); }
            break;
        case LEItem.CHOCO_BAR._3X1:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgSingleL"; vScale  = Vector2.one; }
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgSingleH"; vScale  = Vector2.one; }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgSingleL"; vScale  = new Vector2(-1, 1); }
            break;
        case LEItem.CHOCO_BAR._2X2:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(-1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(1, 1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(-1, 1); }
            break;
         case LEItem.CHOCO_BAR._3X3:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";  vScale  = new Vector2(1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(-1, -1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartL"; vScale  = new Vector2(1, 1); }
            else if(4 == indexBarComponent) {   strPicName  = "pngPics/imgSingle"; vScale  = new Vector2(1, 1); }
            else if(5 == indexBarComponent) {   strPicName  = "pngPics/imgPartL"; vScale  = new Vector2(-1, 1); }
            else if(6 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(1, 1); }
            else if(7 == indexBarComponent) {   strPicName  = "pngPics/imgPartU"; vScale  = new Vector2(1, 1); }
            else if(8 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT"; vScale  = new Vector2(-1, 1); }
            break;
        case LEItem.CHOCO_BAR._2X3:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(1, 1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(-1, 1); }
            else if(4 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, 1); }
            else if(5 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, 1); }
            break;
        case LEItem.CHOCO_BAR._2X4:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(1, 1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(-1, 1); }
            else if(4 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(1, 1); }
            else if(5 == indexBarComponent) {   strPicName  = "pngPics/imgPartL";   vScale  = new Vector2(-1, 1); }
            else if(6 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, 1); }
            else if(7 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, 1); }
            break;
         case LEItem.CHOCO_BAR._1X2:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgSingleU";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgSingleU";  vScale  = new Vector2(1, 1); }
            break;
        case LEItem.CHOCO_BAR._1X3:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgSingleU";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgSingleV";  vScale  = new Vector2(1, 1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgSingleU";  vScale  = new Vector2(1, 1); }
            break;
        case LEItem.CHOCO_BAR._3X2:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, -1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, 1); }
            else if(4 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, 1); }
            else if(5 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, 1); }
            break;
        case LEItem.CHOCO_BAR._4X2:
            if(0 == indexBarComponent)      {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, -1);}
            else if(1 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, -1); }
            else if(2 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, -1); }
            else if(3 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, -1); }
            else if(4 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(1, 1); }
            else if(5 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, 1); }
            else if(6 == indexBarComponent) {   strPicName  = "pngPics/imgPartU";   vScale  = new Vector2(1, 1); }
            else if(7 == indexBarComponent) {   strPicName  = "pngPics/imgPartLT";  vScale  = new Vector2(-1, 1); }
            break;
        }

        Image imgPic            = item.transform.Find( strPicName ).GetComponent<Image>();
        SetChocoBarData(imgPic, eType, indexBarComponent, vScale);
    }

    void _updateChocoBar(LEItem.CHOCO_BAR eType, LEItem item)
    {
        List<int> listTargets   = new List<int>();
        List<Vector2> listScale = new List<Vector2>();
        List<Image> listPngImg  = new List<Image>();

        switch(eType)
        {
        case LEItem.CHOCO_BAR._1X1: // x
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgSingle", Vector2.one);
            break;
        case LEItem.CHOCO_BAR._2X1: // x x      // 0 1
        {
            if (Index/GameManager.WIDTH != (Index+1)/GameManager.WIDTH)
                break;         
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgSingleL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgSingleL", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._3X1: // x x x    // 0 1 2
        {
            if (Index/GameManager.WIDTH != (Index+2)/GameManager.WIDTH)
                break;         
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgSingleL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgSingleH", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2, "pngPics/imgSingleL", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._2X2: // x x          2 3
        {                           // x x          0 1
            if((Index/GameManager.WIDTH != (Index+1)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 1 >= GameManager.HEIGHT))
                break;
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartLT", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._3X3: // x x x        6 7 8
        {                           // x x x        3 4 5
                                    // x x x        0 1 2
            if((Index/GameManager.WIDTH != (Index+2)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 2 >= GameManager.HEIGHT))
                break;
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgSingle", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+2, "pngPics/imgPartL", new Vector2(-1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH, "pngPics/imgPartLT", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH+1, "pngPics/imgPartU", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH+2, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._2X3: // x x      4 5
        {                           // x x      2 3 
                                    // x x      0 1
            if((Index/GameManager.WIDTH != (Index+1)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 2 >= GameManager.HEIGHT))
                break;          // 오류 filter.

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgPartL", new Vector2(-1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH, "pngPics/imgPartLT", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH+1, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._2X4: // x x      6 7
        {                           // x x      4 5
                                    // x x      2 3 
                                    // x x      0 1
            if ((Index/GameManager.WIDTH != (Index+1)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 3 >= GameManager.HEIGHT))
                break;          // 오류 filter.

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgPartL", new Vector2(-1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH, "pngPics/imgPartL", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH+1, "pngPics/imgPartL", new Vector2(-1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+3*GameManager.WIDTH, "pngPics/imgPartLT", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+3*GameManager.WIDTH+1, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._1X2: // x    1
        {                           // x    0
            if(Index/GameManager.WIDTH + 1 >= GameManager.HEIGHT)
                break;

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgSingleU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgSingleU", Vector2.one); 
            break;
        }
        case LEItem.CHOCO_BAR._1X3: // x    2
        {                           // x    1
                                    // x    0
            if(Index/GameManager.WIDTH + 2 >= GameManager.HEIGHT)
                break;

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgSingleU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgSingleV", Vector2.one); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2*GameManager.WIDTH, "pngPics/imgSingleU", Vector2.one); 
            break;
        }
        case LEItem.CHOCO_BAR._3X2: // x x x        3 4 5
        {                           // x x x        0 1 2 
            if((Index/GameManager.WIDTH != (Index+2)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 1 >= GameManager.HEIGHT))
                break;

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartLT", new Vector2(1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgPartU", new Vector2(1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+2, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        case LEItem.CHOCO_BAR._4X2: // x x x x      4 5 6 7
        {                           // x x x x      0 1 2 3 
            if((Index/GameManager.WIDTH != (Index+3)/GameManager.WIDTH) ||
               (Index/GameManager.WIDTH + 1 >= GameManager.HEIGHT))
                break;

            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index, "pngPics/imgPartLT", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+1, "pngPics/imgPartU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+2, "pngPics/imgPartU", new Vector2(1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+3, "pngPics/imgPartLT", new Vector2(-1, -1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH, "pngPics/imgPartLT", new Vector2(1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+1, "pngPics/imgPartU", new Vector2(1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+2, "pngPics/imgPartU", new Vector2(1, 1)); 
            _bindChocoBarList(ref listTargets, ref listPngImg, ref listScale, item, Index+GameManager.WIDTH+3, "pngPics/imgPartLT", new Vector2(-1, 1)); 
            break;
        }
        default:    return;
        }

        // 다른 패널이 이 공간내에 있다면, 이 action은 무시된다.
        for(int zz = 0; zz < listTargets.Count; ++zz)
        {            
            if (false == LEHelper.Cells[ listTargets[zz] ].panelInfo.Equals(""))
                return;         // !!!
            if(LEHelper.Cells[ listTargets[zz] ].eBarType>=0 && LEHelper.Cells[ listTargets[zz] ].indexBar>=0)
                return;
        }

        //ai_taken_Img.transform.rotation     = Quaternion.identity;
        //ai_taken_Img.transform.localScale   = Vector3.one;
        panelInfo               = "";

        LEHelper.Cells[ listTargets[0] ].SetChocoBarList( listTargets );

        for(int aa = 0; aa < listTargets.Count; ++aa)
            LEHelper.Cells[ listTargets[aa] ].SetChocoBarData(listPngImg[aa], eType, aa, listScale[aa]);
    }

    void UpdatePortal (LEItem item) {
        portalType = item.Portal;
        portalImg.sprite = item.Icon.sprite;
        RectTransform rt = portalImg.GetComponent<RectTransform>();

        PortalUIndex            = 0;
        PortalDIndex            = 0;
        _txtInfo.gameObject.SetActive( false );
        switch((PORTAL_TYPE)portalType)
        {
        case PORTAL_TYPE.UP:
            rt.rotation     = Quaternion.Euler(0, 0, 180);
            break;
        case PORTAL_TYPE.DOWN:
            rt.rotation     = Quaternion.identity;
            break;
        case PORTAL_TYPE.ALL:
            rt.rotation     = Quaternion.identity;
            _txtInfo.gameObject.SetActive( true );
            _txtInfo.text   = "Shift+";
            break;
        }

        UpdateImageActive();
    }
#endregion

#region Tracking input on this cell
    public void ChangeFallbackTime (int _amount) {
        if (IsTimeBomb() == false) return;
        FallbackTime = Mathf.Max(0, FallbackTime + _amount);
    }

    void TrackingMouseWheel ()
    {
        if (isMouseOver == false) return;

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount == 0) return;

        if (Input.GetKey(KeyCode.LeftAlt)) {
            if (IsTimeBomb() == false) return;

            if (scrollAmount < 0) {
                FallbackTime++;
            } else {
                FallbackTime = Mathf.Max(0, FallbackTime - 1);
            }
        }
        else if(true == IsPortal())
        {
            switch((PORTAL_TYPE)portalType)
            {
            case PORTAL_TYPE.UP:
                if (scrollAmount < 0)   PortalUIndex++;
                else                    PortalUIndex = Mathf.Max(0, PortalUIndex - 1);
                break;
            case PORTAL_TYPE.DOWN:
                if (scrollAmount < 0)   PortalDIndex++;
                else                    PortalDIndex = Mathf.Max(0, PortalDIndex - 1);
                break;
            case PORTAL_TYPE.ALL:
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Z))
                {
                    if (scrollAmount < 0)   PortalUIndex++;
                    else                    PortalUIndex = Mathf.Max(0, PortalUIndex - 1);
                }
                else
                {
                    if (scrollAmount < 0)   PortalDIndex++;
                    else                    PortalDIndex = Mathf.Max(0, PortalDIndex - 1);
                }
                break;
            default:    break;
            }
        }
        else if(true == IsBlockMissionPanel())
        {
            if (scrollAmount < 0)   pStrength++;
            else                    pStrength = Mathf.Max(0, pStrength - 1);

            _txtBlockMissionCount.text = pStrength.ToString();
        }
        else if(null != _imgFence.sprite)
        {
            switch(DirFence)
            {
            case JMF_DIRECTION.LEFT:    DirFence    = JMF_DIRECTION.UP;     break;
            case JMF_DIRECTION.RIGHT:   DirFence    = JMF_DIRECTION.DOWN;   break;
            case JMF_DIRECTION.UP:      DirFence    = JMF_DIRECTION.RIGHT;  break;
            case JMF_DIRECTION.DOWN:    DirFence    = JMF_DIRECTION.LEFT;   break;
            default: return;
            }
            _refreshFenceView();
        }
    }

    void _refreshFenceView()
    {
        if(null==_imgFence || null==_imgFence.sprite)
            return;

        Vector3 vEuler          = Vector3.zero;
        Vector3 vLocalPos       = Vector3.zero;
        const float half_size   = LevelEditorSceneHandler.CELL_GAP_DISTANCE*0.5f;
            
        //
        const float             fViewAdjust = 5.0f;
        switch(DirFence)
        {
        case JMF_DIRECTION.LEFT:
            vEuler              = new Vector3(0, 0, 90);
            vLocalPos           = new Vector3(-half_size+fViewAdjust, 0, 0);
            break;
        case JMF_DIRECTION.RIGHT:
            vEuler              = new Vector3(0, 0, 90);
            vLocalPos           = new Vector3(half_size-fViewAdjust, 0, 0);
            break;
        case JMF_DIRECTION.UP:
            vLocalPos           = new Vector3(0, half_size-fViewAdjust, 0);
            break;
        case JMF_DIRECTION.DOWN:
            vLocalPos           = new Vector3(0, -half_size+fViewAdjust, 0);
            break;
        default: return;
        }

        vLocalPos += new Vector3(half_size, half_size, .0f);
        _imgFence.transform.localEulerAngles    = vEuler;
        _imgFence.transform.localPosition       = vLocalPos;
    }

    void TrackingConveyorDrawing()
    {
        if(null == LEHelper.CurrentItem)    return;
        if(LEItem.TYPE.RIVER != (LEItem.TYPE)LEHelper.CurrentItem.Type)
            return;
        // if (panelIndex != 22) return;
        if (isMouseOver == false)           return;
        if (Input.GetKey(KeyCode.LeftAlt))  return;
        if (Input.GetKeyUp(KeyCode.Mouse0)) LEHelper.EndConveyorDrawing();

        float scrollAmount      = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount != 0f)
        {
            ConveyorPanel.Info  info = JsonReader.Deserialize<ConveyorPanel.Info>(this.riverInfo);

            if((Input.GetKey(KeyCode.Mouse0) == false) && (false==Input.GetKey(KeyCode.LeftShift) && false==Input.GetKey(KeyCode.Z)))
            {   
                if (info.Type == ConveyorPanel.TYPE.BEGIN) {
                    info.In = (ConveyorPanel.DIRECTION)Mathf.Repeat((int)info.In + 1, 4);
                    DrawConveyor(info);
                    CheckConnectedConveyor(info, true);
                } else if (info.Type == ConveyorPanel.TYPE.END) {
                    info.Out = (ConveyorPanel.DIRECTION)Mathf.Repeat((int)info.Out + 1, 4);
                    DrawConveyor(info);
                    CheckConnectedConveyor(info, false);
                }
            }
            else if((true==Input.GetKey(KeyCode.LeftShift) || true==Input.GetKey(KeyCode.Z)) &&        // adjust link.
                   (ConveyorPanel.TYPE.BEGIN==info.Type || ConveyorPanel.TYPE.END==info.Type))   
            {
                if (scrollAmount < 0)   ConveyorUIndex++;
                else                    ConveyorUIndex = Mathf.Max(0, ConveyorUIndex - 1);
            }
        }
    }

    void TrackingEraser ()
    {
        if (isMouseOver == false) return;
        if (Input.GetKey(KeyCode.Mouse1) == false) return;
        Remove();
    }
    
    public void Remove ()
    {
        if (LEHelper.CurrentItem == null) return;

        switch ((LEItem.TYPE)LEHelper.CurrentItem.Type) {
            case LEItem.TYPE.PIECE :
                RemovePiece();
                break;
            case LEItem.TYPE.PANEL :
                if (panelIndex == 0 || panelIndex == 1) break;
                if (panelIndex == 17) {
                    ResetPanael();
                    break;
                }
                RemovePanel();
                break;
            case LEItem.TYPE.SHADE :
                if (shaded == -1) break;
                RemoveShade();
                break;
            case LEItem.TYPE.COLOR_CHANGER:
                RemoveColorChanger();
                break;
            case LEItem.TYPE.ETC :
                RemoveETC();
                break;
            case LEItem.TYPE.AI_TAKEN:
                Remove_AiTaken();
                break;
            case LEItem.TYPE.PORTAL:
                RemovePortal();
                break;
            case LEItem.TYPE.HELPER:
                RemoveHelper();
                break;
            case LEItem.TYPE.CHOCO_BAR:
            {
                #region => remove choco bar.
                // 이녀석과 연결된 모든 data를 unlink 한다.
                for (int tt=0; tt<LEHelper.Cells.Count; ++tt)
                {
                    int idxTarget           = -1;
                    if(0==LEHelper.Cells[tt].indexBar && LEHelper.Cells[tt]._listIdxChocoBar.Count>0)
                    {
                        for(int qq=0; qq<LEHelper.Cells[tt]._listIdxChocoBar.Count; ++qq)
                        {
                            if(LEHelper.Cells[tt]._listIdxChocoBar[qq] == Index)
                            {
                                idxTarget   = tt;
                                break;
                            }
                        }
                    }

                    if(idxTarget >= 0)
                    {
                        for(int qq=0; qq<LEHelper.Cells[idxTarget]._listIdxChocoBar.Count; ++qq)
                        {
                            int idxOthers   = LEHelper.Cells[idxTarget]._listIdxChocoBar[qq];
                            LEHelper.Cells[idxOthers].eBarType  = -1;
                            LEHelper.Cells[idxOthers].indexBar  = -1;
                            LEHelper.Cells[idxOthers].imgBars.sprite    = null;
                            LEHelper.Cells[idxOthers].imgBars.gameObject.SetActive( false );
                        }
                        LEHelper.Cells[idxTarget]._listIdxChocoBar.Clear();
                    }
                }
                #endregion
                break;
            }
            case LEItem.TYPE.OVER_RIVER:
                RemoveOverRiver();
                break;
            case LEItem.TYPE.RIVER:
                RemoveRiver();
                break;
            case LEItem.TYPE.PIECE_COVER:
                RemovePieceCover();
                break;
            case LEItem.TYPE.FENCE:
                RemoveFence();
                break;
        }

        LevelEditorSceneHandler.Instance.refreshGoalObjText();

        UpdateImageActive();
    }
#endregion

#region Reset
    public void ResetAll ()
    {
        ResetAllData();
        ResetAllImages();
    }

    void ResetPanael (int resetIndex = 1)
    {
        panelIndex = resetIndex;
        pStrength = -1;
        panelInfo = "";
        panelColor = -1;
        frontPanelImg.sprite = null;
        frontPanelImg.color = Color.white;
        frontPanelImg.transform.rotation = Quaternion.identity;
        frontPanelImg.transform.localScale = Vector3.one;
        backPanelImg.sprite = null;
        backPanelImg.color = Color.white;
        backPanelImg.transform.rotation = Quaternion.identity;
        backPanelImg.transform.localScale = Vector3.one;
        defaultImg.gameObject.SetActive(false);        
        if (IsTimeBomb() == false) FallbackTime = 0;
    }

    void ResetPiece ()
    {
        //startPiece = false;
        pieceIndex = 0;
        colorIndex = 0;
        pieceImg.sprite = null;
        pieceImg.color = Color.white;
        pieceImg.transform.rotation = Quaternion.identity;
        if (IsTimeBomb() == false) FallbackTime = 0;
    }

    void ResetAllData ()
    {
        startPiece = true;
        panelIndex = 0;
        pStrength = -1;
        shaded = -1;
        panelColor = -1;
        portalType = 0;
        PortalUIndex = 0;
        PortalDIndex = 0;
        ConveyorUIndex = 0;
        pieceIndex = 0;
        colorIndex = 0;
        fallbackTime = 0;
        panelInfo = "";
        riverInfo = "";
        FallbackTime = 0;
        hasTreaserGoal = false;
        hasPenguinSpawn = false;
        eBarType                = -1;
        indexBar                = -1;
        _nHelperType            = -1;
        hasRiver                = false;
        _idxOverRiver           = -1;
        changerColorIndex       = -1;
        _nLifePieceCover        = 0;
        DirFence                = JMF_DIRECTION.NONE;
        _listIdxChocoBar.Clear();
    }

    void ResetAllImages ()
    {
        defaultImg.color = Index % 2 == 0 ? new Color32(0,55,138,255) : new Color32(0,47,107,255);
        shadeImg.sprite = null;
        backPanelImg.sprite = null;
        backPanelImg.transform.rotation = Quaternion.identity;
        backPanelImg.transform.localScale = Vector3.one;
        frontPanelImg.sprite = null;
        pieceImg.sprite = LevelEditorSceneHandler.Instance.pieces[0].variantImages[0].sprite;
        pieceImg.color = LevelEditorSceneHandler.Instance.pieces[0].variantImages[0].color;
        pieceImg.transform.rotation = Quaternion.identity;
        _imgHelper.sprite       = null;
        portalImg.sprite        = null;
        _imgRiver.sprite        = null;
        _imgOverRiver.sprite    = null;
        _imgPieceCover.sprite   = null;
        _imgFence.sprite        = null;

        defaultImg.gameObject.SetActive(true);
        shadeImg.gameObject.SetActive(false);
        portalImg.gameObject.SetActive(false);
        backPanelImg.gameObject.SetActive(false);
        frontPanelImg.gameObject.SetActive(false);
        pieceImg.gameObject.SetActive(true);
        treasureGoalImg.gameObject.SetActive(false);
        penguinSpawnImg.gameObject.SetActive(false);
        ai_taken_Img.gameObject.SetActive(false);   // [AI_MISSION]
        imgBars.gameObject.SetActive( false );      // [NET_SHADE]
        _imgRiver.gameObject.SetActive( false );
        _imgOverRiver.gameObject.SetActive( false );
        _imgFence.gameObject.SetActive( false );

        ActiveConveyorPortal(-1);

        UpdateImageActive ();
    }
#endregion

#region Remove something
    public void RemoveAll ()
    {
        RemovePiece();
        ResetPanael();
        RemoveShade();
        RemoveETC();
        RemoveColorChanger();
        Remove_AiTaken();
        RemovePortal();
        RemoveHelper();
        RemoveRiver();
        RemoveOverRiver();

        UpdateImageActive();
    }

    public void RemovePiece ()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            startPiece = false;
        } else {
            startPiece = true;
        }

        pieceIndex = 0;
        colorIndex = 0;
        pieceImg.sprite = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].sprite;
        pieceImg.color = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].color;
        pieceImg.transform.rotation = Quaternion.identity;
        if (IsTimeBomb() == false) FallbackTime = 0;
    }

    void RemovePanel ()
    {
        panelIndex = 0;
        pStrength = -1;
        panelInfo = "";
        panelColor = -1;
        frontPanelImg.sprite = null;
        backPanelImg.sprite = null;
        ActiveConveyorPortal(-1);
        if (IsTimeBomb() == false) FallbackTime = 0;
        if (CanPlacePiece() && startPiece) {
            if (LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages.Length > 0) {
                pieceImg.sprite = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].sprite;
                pieceImg.color = LevelEditorSceneHandler.Instance.pieces[pieceIndex].variantImages[colorIndex].color;
            } else {
                pieceImg.sprite = LevelEditorSceneHandler.Instance.pieces[pieceIndex].icon.sprite;
                pieceImg.color = LevelEditorSceneHandler.Instance.pieces[pieceIndex].icon.color;
            }
        }
    }

    void RemoveShade ()
    {
        shaded = -1;
        shadeImg.sprite = null;        
    }

    void RemoveColorChanger()
    {
        changerColorIndex   = -1;
        shadeImg.sprite = null;
    }

    void RemoveETC ()
    {
        hasTreaserGoal = false;
        hasPenguinSpawn = false;
        treasureGoalImg.gameObject.SetActive(false);
        penguinSpawnImg.gameObject.SetActive(false);
    }
    // [AI_MISSION]
    void Remove_AiTaken()
    {
        ai_taken_Img.gameObject.SetActive(false);
        ai_taken_index          = -1;
    }
    void RemovePortal () 
    {
        portalType = 0;
        PortalDIndex = 0;
        PortalUIndex = 0;
        portalImg.sprite = null;
        FallbackTime = 0;
    }
    void RemoveHelper()
    {
        _imgHelper.sprite       = null;
        _nHelperType            = -1;
    }
    void RemoveRiver()
    {
        _imgRiver.sprite        = null;
        hasRiver                = false;
        riverInfo               = "";
        ConveyorUIndex          = 0;

        ActiveConveyorPortal(-1);
    }
    void RemovePieceCover()
    {
        _imgPieceCover.sprite   = null;
        _nLifePieceCover        = 0;
    }
    void RemoveFence()
    {
        DirFence                = JMF_DIRECTION.NONE;
        _imgFence.sprite        = null;
    }
    void RemoveOverRiver()
    {
        _imgOverRiver.sprite    = null;
        _idxOverRiver           = -1;
    }
#endregion

#region Utility Function
    void UpdateImageActive ()
    {
        shadeImg.gameObject.SetActive(shadeImg.sprite != null);
        _txtShadeCount.gameObject.SetActive(shadeImg.sprite != null);
        portalImg.gameObject.SetActive(portalImg.sprite != null);
        backPanelImg.gameObject.SetActive(backPanelImg.sprite != null);
        frontPanelImg.gameObject.SetActive(frontPanelImg.sprite != null);
        _imgHelper.gameObject.SetActive(_imgHelper.sprite != null);
        _imgRiver.gameObject.SetActive(_imgRiver.sprite != null );
        _imgOverRiver.gameObject.SetActive(_imgOverRiver.sprite != null);
        _imgPieceCover.gameObject.SetActive(_nLifePieceCover>=1);
        _imgFence.gameObject.SetActive( _imgFence.sprite != null );

        if (CanPlacePiece() && startPiece) {
            pieceImg.gameObject.SetActive(true);
        } else {
            pieceImg.gameObject.SetActive(false);
        }

        _txtBlockMissionCount.gameObject.SetActive( IsBlockMissionPanel() );

        // [AI_MISSION]
        ai_taken_Img.gameObject.SetActive( ai_taken_index!=-1 );
    }

    bool IsTimeBomb ()
    {
        return (pieceIndex == 7);
    }

    bool CanPlacePiece ()
    {
        switch (panelIndex) {
        case 0 :    // none.
        case 2:     // frost
        case 8:     // cage
            return true;
        default :
            return false;
        }
    }

    bool IsCakePanel ()
    {
        return ((panelIndex >= 18) && (panelIndex <= 21));
    }

    bool IsCreatorPanel ()
    {
        return ((panelIndex >= 11) && (panelIndex <= 17));
    }

    bool IsBlockMissionPanel()
    {
        return panelIndex==40;
    }

    bool IsPortal ()
    {
        return (portalType != 0);
    }

    bool IsFrog (LECell cell)
    {
        return cell.pieceIndex == 14;
    }

    void MakeFrogAsUnique ()
    {
        if (IsFrog(this) == false) return;

        for (int i = 0; i < LEHelper.Cells.Count; i++) {
            if (i == Index) continue;
            if (IsFrog(LEHelper.Cells[i]) == true) LEHelper.Cells[i].RemovePiece();
        }
    }
#endregion

#region For odd panel
    public void SetChocoBarList(List<int> listChocobars)
    {
        if(this.Index != listChocobars[0])
            return;

        _listIdxChocoBar.Clear();
        for(int q = 0; q < listChocobars.Count; ++q)
            _listIdxChocoBar.Add( q );
    }
    void _bindChocoBarList(ref List<int> idxBoard, ref List<Image> listPngImg, ref List<Vector2> listAngles, LEItem item, int idx, string strTrName, Vector2 v2Scale)
    {
        Debug.Assert(null!=item && null!=item.transform.Find(strTrName));
        Debug.Assert(idx>=0 && idx<(GameManager.WIDTH*GameManager.HEIGHT));

        idxBoard.Add( idx );
        listPngImg.Add( item.transform.Find( strTrName ).GetComponent<Image>() ); 
        listAngles.Add( v2Scale );
    }

    // [NET_SHADE]
    public void SetChocoBarData(Image img, LEItem.CHOCO_BAR eChocoBarType, int idxBar, Vector2 v2Scale)
    {
        imgBars.gameObject.SetActive( true );
        imgBars.sprite          = img.sprite;
        imgBars.color           = img.color;
        // imgBars.transform.localRotation = Quaternion.Euler(0, 0, angle);
        imgBars.transform.localScale    = new Vector3(v2Scale.x, v2Scale.y, 1);
        eBarType                = (int)eChocoBarType;
        indexBar                = idxBar;
        _listIdxChocoBar.Clear();
        if(0 != indexBar)       return;

        #region // add index by chocobar type.
        switch (eChocoBarType)
        {
        case LEItem.CHOCO_BAR._1X1: // x
            _listIdxChocoBar.Add( Index );
            break;
        case LEItem.CHOCO_BAR._2X1: // x x      // 0 1
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            break;
        case LEItem.CHOCO_BAR._3X1: // x x x    // 0 1 2
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index + 2 );
            break;
        case LEItem.CHOCO_BAR._2X2: // x x          2 3
        {                           // x x          0 1
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            break;
        }
        case LEItem.CHOCO_BAR._3X3: // x x x        6 7 8
        {                           // x x x        3 4 5
                                    // x x x        0 1 2
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index + 2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 + 2 );
            break;
        }
        case LEItem.CHOCO_BAR._2X3: // x x      4 5
        {                           // x x      2 3 
                                    // x x      0 1
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 + 1 );
            break;
        }
        case LEItem.CHOCO_BAR._2X4: // x x      6 7
        {                           // x x      4 5
                                    // x x      2 3 
                                    // x x      0 1
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*3 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*3 + 1 );
            break;
        }
        case LEItem.CHOCO_BAR._1X2: 
        {                           // x    1
                                    // x    0
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            break;
        }
        case LEItem.CHOCO_BAR._1X3: // x    2
        {                           // x    1
                                    // x    0
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH*2 );
            break;
        }
        case LEItem.CHOCO_BAR._3X2: // x x x        3 4 5
        {                           // x x x        0 1 2 
            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index + 2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 2 );
            break;
        }
        case LEItem.CHOCO_BAR._4X2: // x x x x      4 5 6 7
        {                           // x x x x      0 1 2 3 

            _listIdxChocoBar.Add( Index );
            _listIdxChocoBar.Add( Index + 1 );
            _listIdxChocoBar.Add( Index + 2 );
            _listIdxChocoBar.Add( Index + 3 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 1 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 2 );
            _listIdxChocoBar.Add( Index+GameManager.WIDTH + 3 );
            break;
        }
        default:    break;
        }
        #endregion

        // check error.
        for (int g = 0; g < _listIdxChocoBar.Count; ++g)
        {
            Debug.Assert(_listIdxChocoBar[g]>=0 && _listIdxChocoBar[g]<GameManager.WIDTH*GameManager.HEIGHT);
        }
    }
    public void Set2X2PanelData (Image image, int index, int strength, string infoString = "")
    {
        RemovePanel();
        backPanelImg.sprite = image.sprite;
        backPanelImg.color = image.color;
        panelIndex = index;
        pStrength = strength;
        panelInfo = infoString;
        panelColor = -1;
        UpdateImageActive();
    }

    public void DrawConveyor (ConveyorPanel.Info info)
    {
        if(null == info)        return;
        riverInfo = JsonWriter.Serialize(info);
        LEItem item = LevelEditorSceneHandler.Instance._river;//.panels[22];

        int indexIn = (int)info.In;
        int indexOut = (int)info.Out;
        bool isStraight = (Mathf.Abs(indexOut - indexIn) == 2);

        _imgRiver.gameObject.SetActive( true );

        if (isStraight) {            
            _imgRiver.sprite    = item.variantImages[0].sprite;
            _imgRiver.transform.rotation = Quaternion.Euler(0, 0, indexIn * 90);
        } else {
            _imgRiver.sprite    = item.variantImages[1].sprite;

            if ((indexIn == 3) && (indexOut == 0)) {
                _imgRiver.transform.localScale = Vector3.one;
            } else if ((indexIn == 0) && (indexOut == 3)) {
                _imgRiver.transform.localScale = new Vector3(1F, -1F, 1F);
            } else if (indexIn < indexOut) {
                _imgRiver.transform.localScale = Vector3.one;
            } else {
                _imgRiver.transform.localScale = new Vector3(1F, -1F, 1F);
            }

            _imgRiver.transform.rotation = Quaternion.Euler(0, 0, indexIn * 90);
        }

        if (info.Type == ConveyorPanel.TYPE.BEGIN) {
            ActiveConveyorPortal((int)info.In);
        } else if (info.Type == ConveyorPanel.TYPE.END) {
            ActiveConveyorPortal((int)info.Out);
        } else {
            ActiveConveyorPortal(-1);
        }
    }

    void MakeWholeCave (LEItem item)
    {
        Image[] images = item.Icon.transform.parent.GetComponentsInChildren<Image>();
        backPanelImg.sprite = images[0].sprite;
        panelIndex = item.Index;
        pStrength = item.Strength;
        CavePanel.Info info = new CavePanel.Info();
        info.Index = 0;
        panelInfo = JsonWriter.Serialize(info);

        info.Index = 1;
        LEHelper.Cells[Index + 1].Set2X2PanelData(images[1], item.Index, item.Strength, JsonWriter.Serialize(info));
        info.Index = 2;
        LEHelper.Cells[Index + 9].Set2X2PanelData(images[2], item.Index, item.Strength, JsonWriter.Serialize(info));
        info.Index = 3;
        LEHelper.Cells[Index + 10].Set2X2PanelData(images[3], item.Index, item.Strength, JsonWriter.Serialize(info));
    }

    void MakeWholeCake (LEItem item)
    {
        Image[] images = item.Icon.transform.parent.GetComponentsInChildren<Image>();
        backPanelImg.sprite = images[0].sprite;
        panelIndex = item.Index;
        pStrength = item.Strength;

        LEHelper.Cells[Index + 1].Set2X2PanelData(images[1], (item.Index + 1), item.Strength);
        LEHelper.Cells[Index + 9].Set2X2PanelData(images[2], (item.Index + 2), item.Strength);
        LEHelper.Cells[Index + 10].Set2X2PanelData(images[3], (item.Index + 3), item.Strength);
    }

    void CheckConnectedConveyor (ConveyorPanel.Info info, bool isBegin)
    {
        Point p = PT;
        p.x += LEHelper.direction[isBegin ? (int)info.In : (int)info.Out].x;
        p.y += LEHelper.direction[isBegin ? (int)info.In : (int)info.Out].y;
        int cellIndex = p.x + (p.y * 9);
        if ((cellIndex >= LEHelper.Cells.Count) || (cellIndex < 0)) return;

        LECell targetCell = LEHelper.Cells[cellIndex];
        if (targetCell == null) return;
        if(null == targetCell._imgRiver.sprite) return;
        // if (targetCell.panelIndex != 22)  return;

        ConveyorPanel.Info targetInfo = JsonReader.Deserialize<ConveyorPanel.Info>(targetCell.riverInfo);
        if ((targetInfo.Type == (isBegin ? ConveyorPanel.TYPE.END : ConveyorPanel.TYPE.BEGIN))
                && ((isBegin ? (int)info.In : (int)info.Out) == ((isBegin ? (int)targetInfo.Out : (int)targetInfo.In) + 2) % 4)) {
            info.Type = ConveyorPanel.TYPE.MIDDLE;
            targetInfo.Type = ConveyorPanel.TYPE.MIDDLE;
            DrawConveyor(info);
            targetCell.DrawConveyor(targetInfo);
        }
    }
#endregion
}
