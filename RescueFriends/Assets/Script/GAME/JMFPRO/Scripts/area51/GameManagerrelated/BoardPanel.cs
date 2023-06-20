using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NOVNINE.Diagnostics;

public class BoardPanel {
    public enum TYPE { DEFAULT, BACK, FRONT }

	public object info;

    public bool IsImmortal { get; set; }

    Board owner;
	public Board Owner { 
        get { return owner; }
        set { 
            if (owner == value) return;
            owner = value;
            UpdateTracker();
        }
    }

    public PanelDefinition PND { get; set; }

	int durability = -1;
    public int Durability {
        get { return durability; }
        set {
            value = Mathf.Max(-1, value);
            if (durability == value) return;
            durability = value;

            if(true == IsNeedGuage())
            {
                Panel panel     = this[TYPE.BACK];
                if(panel)       panel.refreshGauge(durability, _maxGaugeValue);
            }
            else if(IsNeedNumber())
            {
                Panel panel     = this[TYPE.BACK];
                if(panel)       panel.refreshNumber(durability+1);
            }
        }
    }

    float _maxGaugeValue        = .0f;
    public void setMaxGaugeValue(float f)
    {
        _maxGaugeValue          = f;
        Panel panel             = this[TYPE.BACK];
        if(panel)               panel.refreshGauge(durability, _maxGaugeValue);
    }

    int _boardColor             = -1;
    public int getBoardColor()  { return _boardColor; }

    int _damagingColor          = -1;
    public void setDamagingColor(int color) {   _damagingColor = color; }
    public int getDamagingColor()           {   return _damagingColor; }

    // note : 특정 skin은 갱신해줄 필요가 없다.(연출 ani에 이미 포함되어 있는 경우) 이때 재갱신 불필요.
    bool _validateSkin          = true;

	Dictionary<TYPE, Panel> panelDict = new Dictionary<TYPE, Panel>();

	public Panel this [TYPE type] { 
        get { 
            if (panelDict.ContainsKey(type)) {
                return panelDict[type]; 
            } else {
                return null;
            }
        } 
        set {
            if (panelDict.ContainsKey(type)) {
                panelDict[type] = value; 
            } else {
                panelDict.Add(type, value);
            }
        }
    }

    public Dictionary<TYPE, Panel> PanelDict() { return panelDict; }
	
	public BoardPanel (Board _Owner) 
	{
		Owner = _Owner;
    }

    public BoardPanel (Board _Owner, PanelDefinition _pnd, int _durability, object _info = null, int color=-1) {
		Owner = _Owner;
        Reset(_pnd, _durability, _info, color);
    }

    public void Reset (PanelDefinition _pnd, int _durability, object _info = null, int color=-1)
	{
        //Debugger.Assert(_durability < _pnd.skin.Length, "BoardPanel.Reset : Durability is wrong.");
        
        //if ((_pnd.hasNoSkin == false) && ((_durability < 0) || (_durability >= _pnd.skin.Length))) 
		if ( _pnd.hasNoSkin == false && _durability < 0) 
		    _durability = 0; 
        
        if(PND!=null && false==JMFUtils.GM._isRemovedByConveyor)
            PND.FireOnPanelDestroy(this);

        PND = _pnd;
        durability = _durability;
        info = _info;
		_boardColor             = color;
        _validateSkin           = true;

        IsOnSkilling            = false;
        _isDestroyablePanel     = true;

        ChangePanel();

        if(PND!=null && false==JMFUtils.GM._isRemovedByConveyor)
            PND.FireOnPanelCreate(this);

        if(true == IsNeedGuage())
        {
            Panel panel         = this[TYPE.BACK];
            panel.refreshGauge(Durability, _maxGaugeValue);
        }
        else if(IsNeedNumber())
        {
            Panel panel         = this[TYPE.BACK];
            panel.refreshNumber(Durability+1);
        }
    }

    public void Remove () {
        Remove(0F, null);
    }

    public void Remove (float delay, System.Action onComplete) {
        Debugger.Assert(PND != null, "BoardPanel.Remove : PanelDefinition is null.");

        if (delay > 0F) {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() => { 
                if (PND != null) PND.FireOnPanelDestroy(this);
                RemovePanel(); 
                if (onComplete != null) onComplete();
            });
        } else {
            if (PND != null) PND.FireOnPanelDestroy(this);
            RemovePanel();
            if (onComplete != null) onComplete();
        }
    }

	public int Destroy (bool isByMatch, bool force, bool isSpecialAttack, System.Action<bool> onComplete = null) {
        Debugger.Assert(PND != null, "BoardPanel.Destroy : PanelDefinition is null.");

        if(false==PND.IsDamageable(this, _damagingColor) || ((force == false) && (IsDestroyable() == false)))
        {
            if (onComplete != null) onComplete(false);

            // just give some animation.
            if (PND is ColorBoxPanel && null!=this[TYPE.BACK])
                this[TYPE.BACK].Play( string.Format("colorbox_level{0}_nonbust", Durability+1),  false);
            return 0;
        }
        
        float delay = PND.ShowHitEffect(this);
        // PND.FireOnHit(this, isSpecialAttack); -> double called.

        PanelDefinition _pnd = PND;
        Durability--;

        bool remainSkins        = IsNeedGuage() || PND is ColorBoxPanel || PND is WaffleCookerPanel;

        _validateSkin           = !(remainSkins && Durability>=0);

        if (delay > 0F) {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() => {

                if (_validateSkin)
                    Invalidate();

                if ((isByMatch == false) && (_pnd.destroyScore > 0)) {
                    JMFUtils.GM.IncreaseScore(_pnd.destroyScore, Owner.PT, 6);
                }
                if (onComplete != null) onComplete(true);

                _validateSkin   = true;
            });
        } else {
            if(_validateSkin)
                Invalidate();
            
            if ((isByMatch == false) && (_pnd.destroyScore > 0)) {
                JMFUtils.GM.IncreaseScore(_pnd.destroyScore, Owner.PT, 6);
            }
            if (onComplete != null) onComplete(true);

            _validateSkin   = true;
        }

        _damagingColor          = -1;
        return _pnd.destroyScore;
	}

	void RemovePanel () 
	{
        foreach (TYPE type in panelDict.Keys) 
		{
			NNPool.Abandon(panelDict[type].gameObject);
        }
        panelDict.Clear();
	}

    public void Invalidate () {
        if (Durability < 0) {
            ConvertBasicPanel();
        } else {
            ChangePanel();
        }
    }
	
	void ChangePanel () 
	{
		RemovePanel();
        if (PND.hasNoSkin) return;

        Debugger.Assert(PND.skin.Length > 0, "BoardPanel.ChangePanel : PanelDefinition's skin is empty.");
		
		Panel panel = null;
		if(panelDict.ContainsKey(TYPE.FRONT))
			panel = this[TYPE.FRONT];
		else if (panelDict.ContainsKey(TYPE.BACK)) 	
			panel = this[TYPE.BACK];
		
		if(panel == null)
		{
			panel = NNPool.GetItem<Panel>(PND.skin[0].name);
			panel.GetComponent<PanelTracker>().PT = Owner.PT;
			
			panel.gameObject.transform.parent = JMFUtils.GM.gameObject.transform;
			Vector3 pos = Owner.LocalPosition;
			
			if(PND is EmptyPanel)
				panel.gameObject.transform.localScale = Vector3.one * JMFUtils.GM.Size;
			else			
				JMFUtils.SpineObjectAutoScalePadded(panel.gameObject);
			
			if (PND.isFront) 
			{
				pos.z = (Owner.PT.Y + (GameManager.WIDTH -1 - Owner.PT.X)) * 0.01f;
				pos.z -= 3F;
				this[TYPE.FRONT] = panel;
			}
			else 
			{
				pos.z = (Owner.PT.Y + (GameManager.WIDTH -1 - Owner.PT.X)) * 0.01f;
				
				if(PND is EmptyPanel)
					pos.z = 1F;	
				else
					pos.z += 2F;	
				
				this[TYPE.BACK] = panel;
			}
			
			panel.transform.localPosition = pos;
		}
		        
        IsOnSkilling            = false;
        if(PND is ColorBoxPanel)
        {
            panel.ChangePanelWithSlot((PND as ColorBoxPanel).GetImageName(_boardColor), "colorbox");

            panel.Play(string.Format("colorbox_level{0}_idle", Durability+1) , false);
            //Director.Instance.showMeshNextFrame( panel.GetComponent<MeshRenderer>() );
        }
        else if(PND is WaffleCookerPanel)
        {
            panel.ChangePanelWithSlot("", "waffle");
        }
        else if(PND is BlockMissionPanel)
        {
            panel.ChangePanel(PND.GetImageName( _boardColor ));
            panel.UpdatePanel(info);
        }
        else
        {
		    panel.ChangePanel(PND.GetImageName(Durability));
            panel.UpdatePanel(info);
        }

        // setup guage.
        bool isGaugeOn          = IsNeedGuage();// PND is FireStonePanel || PND is FireWebPanel || PND is FireIcePanel;
        panel.enableGauge( isGaugeOn );
        if(isGaugeOn)           panel.refreshGauge(Durability, _maxGaugeValue);

        panel.enableNumber( IsNeedNumber() );
        if(IsNeedNumber())      panel.refreshNumber(Durability+1);
        //

        UpdateTracker();

        // [ADJUST SCALE]
        // adjust order or scale.
        if(null != panel)
        {
            if(PND is JamGenPanel)
                panel.transform.localScale = Vector3.one * 0.9f;
            else if(PND is ZellatoBoxPanel)
                panel.transform.localScale = Vector3.one * 0.7f;
            else if(PND is ColorBoxPanel || PND is WaffleCookerPanel || PND is ColorBombPanel || PND is BlockMissionPanel)
                panel.transform.localScale = Vector3.one * 0.8f;
            else if(PND is SodaCanPanel)
                panel.transform.localScale = Vector3.one * 0.68f;
            else  if(PND is RectChocoPanel)
                panel.transform.localScale = Vector3.one * 1.2f;
            else if(PND is visibleObstructionPanel)
                panel.transform.localScale = Vector3.one * 1.2f;
            else 
                panel.transform.localScale = Vector3.one;
            
        }
    }

    void ConvertBasicPanel () 
	{
        if (PND != null) PND.FireOnPanelDestroy(this);
        PND = Owner.GM.GetPanelType<BasicPanel>();
        ChangePanel();
        if (PND != null) PND.FireOnPanelCreate(this);
    }

//	void CreateDefaultPanel () {
//		DefaultPanel dp = NNPool.GetItem<DefaultPanel>("DefaultPanel", JMFUtils.GM.transform);
//        dp.GetComponent<PanelTracker>().PT = Owner.PT;
//        dp.UpdatePanel(Owner.PT);
//        this[TYPE.DEFAULT] = dp.gameObject;
//        this[TYPE.DEFAULT].transform.localPosition = Owner.LocalPosition;
//        JMFUtils.autoScale(this[TYPE.DEFAULT]);
//        this[TYPE.DEFAULT].transform.localPosition += 
//            new Vector3(0,0,6*Owner.GM.Size*this[TYPE.DEFAULT].transform.localScale.z);
//	}
    
    void UpdateTracker () {
        foreach (TYPE type in panelDict.Keys) {
            PanelTracker tracker = panelDict[type].GetComponent<PanelTracker>();
            tracker.PT = Owner.PT;
        }
    }
	
    bool mIsOnSkilling          = false;
    public bool IsOnSkilling
    {
        get { return mIsOnSkilling; }
        set { mIsOnSkilling = value; }
    }

    // 0 - 대상 아님.
    // 1 - 처리 시작
    // 2 - 처리 중.
    public int checkSkill()
    {
        if(PND is JamGenPanel)
        {
            if(0 != Durability)     return 0;
            if(true==IsOnSkilling)  return 2;

            for(int q = 0; q < (int)TYPE.FRONT+1; ++q)
            {
                Panel pnl       = this[(TYPE)q];
                if(null != pnl) 
                {
                    pnl.StartCoroutine( pnl.coProcessJamGen(this) );
                    return 1;
                }
            }
        }
        else if(PND is ZellatoBoxPanel)
        {
            if(0 != Durability)     return 0;
            if(true==IsOnSkilling)  return 2;

            for(int q = 0; q < (int)TYPE.FRONT+1; ++q)
            {
                Panel pnl       = this[(TYPE)q];
                if(null != pnl) 
                {
                    pnl.StartCoroutine( pnl.coProcessZellatoGen(this) );
                    return 1;
                }
            }
        }

        return 0;
    }

	public bool IsSolid () {
		return PND.IsSolid(this);
	}
	public bool IsFallable () {
		return PND.IsFallable(this);
	}

	public bool IsFillable () {
		return PND.IsFillable(this);
	}

	public bool IsMatchable () {
		return PND.IsMatchable(this);
	}
	
	public bool IsStealable () {
		return PND.IsStealable(this);
	}
	
	public bool IsSwitchable () {
		return PND.IsSwitchable(this);
	}

	public bool IsSplashHitable () {
        return PND.IsSplashHitable(this);
    }

    public bool IsDestroyablePiece (){
		return PND.IsDestroyablePiece(this);
	}
    public bool IsNeedGuage()
    {
        return PND.IsNeedGuage();
    }
    public bool IsNeedNumber()
    {
        return PND.IsNeedNumber();
    }

    //
    bool _isDestroyablePanel    = true;
    public void setIsDestroyablePanel(bool isDestroyable)
    {
        _isDestroyablePanel     = isDestroyable;
    }
	public bool IsDestroyable ()
    {
        if(PND is WaffleCookerPanel)    // note : waffleCokkerPanel 일때는 동적으로 조절 필요.
            return _isDestroyablePanel;
		return PND.IsDestroyable(this);
	}
    //

    public float fireIce(Board bdTarget)
    {
        if(false == (PND is FireIcePanel))
            return .0f;
        
        for(int q = 0; q < (int)TYPE.FRONT+1; ++q)
        {
            Panel pnl       = this[(TYPE)q];
            if(null!=pnl)   return pnl.fireIce(bdTarget);
        }
        return .0f;
    }

    public float fireStone(Board bdTarget)
    {
        if(false == (PND is FireStonePanel))
            return .0f;
        
        for(int q = 0; q < (int)TYPE.FRONT+1; ++q)
        {
            Panel pnl       = this[(TYPE)q];
            if(null!=pnl)   return pnl.fireStone(bdTarget);
        }
        return .0f;
    }

    public float fireWeb(Board bdTarget)
    {
        if(false == (PND is FireWebPanel))
            return .0f;
        
        for(int q = 0; q < (int)TYPE.FRONT+1; ++q)
        {
            Panel pnl       = this[(TYPE)q];
            if(null!=pnl)   return pnl.fireWeb(bdTarget);
        }
        return .0f;
    }
}
