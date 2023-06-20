using UnityEngine;
using System.Collections;

public abstract class PanelDefinition : MonoBehaviour {
	public bool isFront;
	public bool hasNoSkin;
	//public bool hasDefaultPanel;
	public int destroyScore;
	public GameObject[] skin;

	public GameManager GM { get { return JMFUtils.GM; } }
    public string ClassName { get { return GetType().Name; } }

	public void FireOnPanelCreate (BoardPanel bp) {
        OnPanelCreate(bp);
    }

	public void FireOnPanelDestroy (BoardPanel bp) {
        if(false == JMFUtils.GM._isRemovedByConveyor)
            OnPanelDestroy(bp);
    }

	public void FireOnPanelClick (BoardPanel bp) {
        OnPanelClicked(bp);
    }

	public void FireOnHit (BoardPanel bp, bool isSpecialAttack) { 
        OnHit(bp, isSpecialAttack); 
    }

	public abstract bool IsSolid (BoardPanel bp);

	public abstract bool IsFallable (BoardPanel bp);

	public abstract bool IsFillable (BoardPanel bp);

	public abstract bool IsMatchable (BoardPanel bp);

	public abstract bool IsStealable (BoardPanel bp);

	public abstract bool IsSwitchable (BoardPanel bp);

	public abstract bool IsDestroyable (BoardPanel bp);

	public abstract bool IsSplashHitable (BoardPanel bp);

	public abstract bool IsDestroyablePiece (BoardPanel bp);

    public abstract bool IsShufflable (BoardPanel bp);
	
	protected virtual void OnPanelCreate (BoardPanel bp) {
    }

	protected virtual void OnPanelDestroy (BoardPanel bp) {
    }

	protected virtual void OnPanelClicked (BoardPanel bp) {
    }

	protected virtual void OnHit (BoardPanel bp, bool isSpecialAttack) { 
    }
	
    public virtual float ShowHitEffect (BoardPanel bp) { 
        return 0F; 
    }

    public virtual object ConvertToPanelInfo (string data) { 
        return null; 
    }
	
	public virtual string GetImageName (int type)
	{
		return null;
	}
    public virtual bool IsApplyShockWave()
	{
		return true;
	}
    public virtual bool IsDamageable(BoardPanel bp, int matchedColor)
    {
        return true;
    }
    public virtual bool IsNeedGuage()
	{
		return false;
	}
    public virtual bool IsNeedNumber()
	{
		return false;
	}
    public virtual bool isLevelMissionTarget()
    {
        return false;
    }
}
