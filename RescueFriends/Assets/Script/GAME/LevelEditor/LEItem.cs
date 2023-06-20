using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class LEItem : MonoBehaviour {
    // Layer    Names               - details. 
    // 0.       Helper.             - dropper. => not visible in game-plays.
    // 1.       Portal              - portals.
    // 2.       Over Pannel         - bubble, PieceCover(ice), Fence
    // 3.       Piece               - game piece, round choco, portions, etc
    // 4.       Under Pannel        - rect choco, dough, 
    // 5.       Shades.             - cursed, net, ai, color-changers.
    // 6.       Choco-Bar(Mission)  - chco bar.
    // 7.       Over River          - river waffle.
    // 8.       River               - river, moving velt
    //

    // note : 기존 data를 유지키위해 type은 추가만 한다.
    public enum TYPE { PIECE, PANEL, SHADE, ETC, PORTAL, AI_TAKEN, CHOCO_BAR, COLOR_CHANGER, HELPER, OVER_RIVER, RIVER, PIECE_COVER, FENCE };
	public enum COLOR { NORMAL_COUNT=VIOLET, NONE = -1, RANDOM,  RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE, SKYBULE, VIOLET, STRAWBERRY, SUGAR_CHERRY, ZELLATO };
    public enum STRENGTH { NONE = -1, s1 = 1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16, s17, s18, s19, s20, 
                                      s21 , s22, s23, s24, s25, s26, s27, s28, s29, s30, s31, s32, s33, s34, s35, s36, s37, s38, s39, s40, 
                                      s41 , s42, s43, s44, s45, s46, s47, s48, s49, s50 };

    public enum SHADED { NONE = -1, ONE, TWO, THREE, FOUR };            // [JAM_SHADE]
    public enum SHADE_TYPE { NONE = -1, CURSE, JAM, NET, MUD_COVER };   // [JAM_SHADE], [NET_SHADE]
    public enum PORTAL { NONE, DOWN, UP, ALL };
    public enum LOCATION { NONE = -1, FRONT, BACK };
    public enum CHOCO_BAR { NONE=-1, _1X1, _2X2, _1X3, _2X3, _2X4, _3X1, _3X2, _4X2, _1X2, _2X1, _3X3 };
    public enum HELPER_TYPE { NONE=-1, DROPPER };

    public TYPE type = TYPE.PIECE;
    public COLOR color = COLOR.NONE;
    public STRENGTH strength = STRENGTH.NONE;
    public SHADED shaded = SHADED.NONE;
    public SHADE_TYPE eShadeType = SHADE_TYPE.NONE;             // [JAM_SHADE]
    public CHOCO_BAR eBarType   = CHOCO_BAR.NONE;               // [NET_PANNEL]
    public PORTAL portal = PORTAL.NONE;
    public COLOR changerColor = COLOR.NONE;
    public HELPER_TYPE eHelperType = HELPER_TYPE.NONE;
    public GameObject variants;
    public int index = 0;
    public LOCATION location = LOCATION.NONE;
    public Image icon;
    public Image[] variantImages;

    public int Type { get { return (int)type; } }
    public int Color { get { return (int)color; } }
    public int Strength { get { return (int)strength; } }
    public int Shaded { get { return (int)shaded; } }
    public int Portal { get { return (int)portal; } }
    public int Index { get { return index; } }
    public LOCATION Location { get { return location; } }

	public Toggle Switch { get { return _toggle; } }
	public Image Icon { get { return _icon; } }
	
	Toggle _toggle;
	Image _icon;
	
    bool isMouseOver = false;
    float pressedSec = 0f;

	void Awake()
	{
		EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
		trigger.triggers.Add(entry);
		
		_toggle = GetComponent<Toggle>();
		if(_toggle != null)
			_icon = _toggle.targetGraphic as Image;
		
		if(variants != null)
		{
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
			trigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
			trigger.triggers.Add(entry);
		}
	}
	
    void Update ()
    {
        TrackingLongPress();
    }

	public void OnPointerEnter (PointerEventData _event)
    {	
		isMouseOver = true;
    }

	public void OnPointerDown (PointerEventData _event)
	{
#if UNITY_EDITOR
		LevelEditorSceneHandler.Instance.OnMousDown(this);
#endif
	}
	
	public void OnPointerExit (PointerEventData _event)
    {
        isMouseOver = false;
    }

    public void OnSelectedEmpty (int _index)
    {
        index = _index;
        strength = 0;
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;
    }

    public void OnSelectedColor (int _index)
    {
        color = (COLOR)_index;
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;
    }
    public void OnSelectedChangerColor (int _index)
    {
        changerColor = (COLOR)_index;
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;

        //LEHelper.CurrentItem.Type
    }

    public void OnSelectedStrength (int _index)
    {
        strength = (STRENGTH)_index;
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;
    }

    public void OnSelectedHelper(int index)
    {
        LEHelper.CurrentItem    = this;
    }

    public void OnSelectedOverRiver(int index)
    {
        LEHelper.CurrentItem    = this;
    }

    public void OnSelectedRiver(int index)
    {
        LEHelper.CurrentItem    = this;
    }

    public void OnSelectedPieceCover()
    {
        LEHelper.CurrentItem    = this;
    }

    public void OnSelctedFence()
    {
        LEHelper.CurrentItem    = this;
    }

    public void OnSelectedShade (int param)// int _index, SHADE_TYPE shadeType)
    {
        int _index              = param % 100;
        int shadeType           = param / 100;

        shaded                  = (SHADED)_index;
        eShadeType              = (SHADE_TYPE)shadeType;    // [JAM_SHADE]
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;
    }

    public void OnSelectedCreator (int _index)
    {
        index = _index;
        strength = 0;
        if (variants != null) variants.SetActive(false);
        LEHelper.CurrentItem = this;
    }

    public void OnSelectedETC (int _index)
    {
        index = _index;
        LEHelper.CurrentItem = this;
    }

    // [AI_MISSION]
    public void OnSelected_AI_TAKEN (int _index)
    {
        index                   = _index;
        if (variants != null)   variants.SetActive(false);
        LEHelper.CurrentItem    = this;
    }

    // [NET_PANNEL]
    public void OnSelectedChoco(int bar_type)
    {
        eBarType                = (CHOCO_BAR)bar_type;
        if (variants != null)
        {
            variants.SetActive(false);   
        }
        LEHelper.CurrentItem    = this;
        string strContent       = "";

        switch(eBarType)
        {
        case CHOCO_BAR._1X1:    strContent  = "1x1";    break;
        case CHOCO_BAR._1X3:    strContent  = "1x3";    break;
        case CHOCO_BAR._2X2:    strContent  = "2x2";    break;
        case CHOCO_BAR._2X3:    strContent  = "2x3";    break;
        case CHOCO_BAR._2X4:    strContent  = "2x4";    break;
        case CHOCO_BAR._3X1:    strContent  = "3x1";    break;
        case CHOCO_BAR._3X2:    strContent  = "3x2";    break;
        case CHOCO_BAR._4X2:    strContent  = "4x2";    break;
        case CHOCO_BAR._1X2:    strContent  = "1x2";    break;
        case CHOCO_BAR._2X1:    strContent  = "2x1";    break;
        case CHOCO_BAR._3X3:    strContent  = "3x3";    break;
        default:                return;
        }

        Icon.sprite             = variantImages[(int)eBarType].sprite;
        Icon.color              = variantImages[(int)eBarType].color;

        Text txtContent         = transform.Find("background/Text").GetComponent<Text>();
        if(null != txtContent)
        {
            txtContent.text     = strContent;
        }
    }

    public void SetVariantsActive (bool active)
    {
		if(variants != null)
        	variants.SetActive(active);
    }

    public void UpdateImage (Image img)
    {
        Icon.sprite = img.sprite;
        Icon.color = img.color;
    }

    void TrackingLongPress()
    {
		if(variants == null)
			return;
		
		if(variants.activeSelf == false)
		{
			if((Input.GetKey(KeyCode.Mouse0) == true) && (isMouseOver == true) && pressedSec == 0f)
			{
				pressedSec = Time.time;
			}

			if((Input.GetKey(KeyCode.Mouse0) == true) && (isMouseOver == true))
			{
				if(Time.time - pressedSec > .5f)
				{
					if(variants != null)
						variants.SetActive(true);
					pressedSec = 0f;                    
				}
			}
			else
			{
				//pressedSec = 0f;
				//SetVariantsActive(false);
			}
			
		}
		//else if(isMouseOver == false)
		//	SetVariantsActive(isMouseOver);
    }
}
