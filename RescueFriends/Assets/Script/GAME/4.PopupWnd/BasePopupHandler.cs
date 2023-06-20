using UnityEngine;
using System;
using Spine.Unity;
using Spine;
using System.Collections.Generic;
using DG.Tweening;

public abstract class BasePopupHandler : MonoBehaviour, IPopupWnd 
{
    const float IN_TIME         = 0.25f;
    const float OUT_TIME        = 0.15f;
    
    public Transform            Target;

    [HideInInspector]
    public bool AddPopup        = false;

	List<object> ColorObject    = new List<object>();
    Transform _trBackGround     = null;

    protected virtual void OnEnter( object param )
	{
        _init();
        
        // swallow 0.1f sec for load something..
        DOVirtual.DelayedCall(0.1f, () => _showUp() );
    }

    void _init()
    {
        // Play("show", false);
        if(null != _trBackGround)
        {
            tk2dSlicedSprite    spr = _trBackGround.GetComponent<tk2dSlicedSprite>();
            if(null != spr)
                spr.color       = new Color(0, 0, 0, 0);
        }
        if(null != Target)
            Target.localScale   = Vector3.one * 0.0001f;
    }

    void _showUp()
    { 
		// Play("show", false);
        if(null != _trBackGround)
        {
            const float tagetOpa= 0.7f;
            tk2dSlicedSprite    spr = _trBackGround.GetComponent<tk2dSlicedSprite>();
            if(null != spr)
                DOTween.To( () => spr.color, x => spr.color=x, new Color(0, 0, 0, tagetOpa), IN_TIME);
        }
        if(null != Target)
            Target.DOScale(1.0f, IN_TIME).SetEase(Ease.OutExpo);// .OutBack);
        
        DOVirtual.DelayedCall(IN_TIME, OnEnterFinished);
	}

    
	
	protected virtual void  OnLeave()
	{
		//Play("hide",false);
        if(null != _trBackGround)
        {
            tk2dSlicedSprite    spr = _trBackGround.GetComponent<tk2dSlicedSprite>();
            if(null != spr)
                DOTween.To( () => spr.color, x => spr.color=x, new Color(0, 0, 0, 0), OUT_TIME);
        }
        if(null != Target)      Target.DOScale(0.01f, OUT_TIME).SetEase(Ease.OutExpo);//.InBack);
        
        DOVirtual.DelayedCall(OUT_TIME, OnLeaveFinished);
	}

    protected virtual void OnEnterFinished()
	{
	}

    protected virtual void OnLeaveFinished()
	{
        gameObject.SetActive( false );
	}


	protected virtual void Awake()
	{
        FindColorObject();

        _trBackGround           = transform.Find("Background");        
    }
	
	protected virtual void Start()
	{
	}
	
    void SetAlpha(float _alpha)
    {
        for(int i = 0; i < ColorObject.Count; ++i)
        {
            Color col = JMFUtils.GetPropValue<Color>(ColorObject[i], "color");
            JMFUtils.SetPropValue<Color>(ColorObject[i], "color", new Color(col.r, col.g, col.b, _alpha));
        }
    }

	private void FindColoredObject(GameObject go, ref List<object> list)
	{
	    Renderer render = go.GetComponent<Renderer>();
	    if(render != null && render.sharedMaterial != null)
	    {
		    var spr = go.GetComponent<tk2dBaseSprite>();
		    if(spr != null)
			    list.Add(spr);
		    else
		    {
			    var textMesh = go.GetComponent<tk2dTextMesh>();
			    if(textMesh != null)
				    list.Add(textMesh);
			    else
			    {
				    if(render.sharedMaterial.HasProperty("_Color"))
					    list.Add(go.GetComponent<Renderer>().material);
			    }
		    }
	    }

		for(int i = 0; i < go.transform.childCount; ++i)
		{
			FindColoredObject(go.transform.GetChild(i).gameObject, ref list);	
		}
	}

    protected void FindColorObject()
    {
        ColorObject.Clear();
        if(Target != null)
        {
            for(int i = 0; i < Target.childCount; ++i)
            {
                FindColoredObject(Target.GetChild(i).gameObject, ref ColorObject);  
            }
        }
    }
	
	
	
	protected virtual void  DataExchange(){}
	
	public virtual void CLICK (tk2dUIItem item) 
	{
		NNSoundHelper.Play("FX_btn_on");
		ButtonAnimation ani = item.GetComponent<ButtonAnimation>();
		if(ani != null)
			ani.CLICK();
	}
	
	public virtual void OVER (tk2dUIItem item) 
	{
		//ButtonAnimation ani = item.GetComponent<ButtonAnimation>();
		//if(ani != null)
		//	ani.OVER();
	}

	public virtual void LEAVE (tk2dUIItem item) 
	{
		//ButtonAnimation ani = item.GetComponent<ButtonAnimation>();
		//if(ani != null)
		//	ani.LEAVE();
	}
	
	public void OnEnterPopup(object param)
	{
		OnEnter(param);
	}
	
	public void OnLeavePopup()
	{
        AddPopup = false;
		OnLeave();
	}
	
	public void DoDataExchange () 
	{
		DataExchange();
	}

	protected virtual void OnEvent (TrackEntry entry, Spine.Event e)
	{

	}
	
	protected virtual void OnStart (TrackEntry entry)
	{
	}

	protected virtual void OnComplete (TrackEntry entry)
	{
	}
	
	public virtual void OnEscape() 
	{
        //i/f (AddPopup)
        //    Scene.RemovePopup(this.gameObject.name);
        //else
		    Scene.ClosePopup();
	}
	
	protected void HandleRebuildRenderer (SkeletonRenderer skeletonRenderer) 
	{
		Initialize();
	}

	protected void Initialize() 
	{	
#if UNITY_EDITOR
		LateUpdate();
#endif
	}

	protected virtual void OnDestroy() 
	{
	}
	
	public float GetAnimationDuration(string name)
	{
        return 0.01f;// DO_TIME;
	}

	public void LateUpdate()
	{
	}
}
