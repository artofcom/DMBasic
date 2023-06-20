using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;


public class ButtonAnimation : MonoBehaviour 
{
	public Transform Target     = null;

    tk2dUIItem _btnIItem        = null;
    Vector3 _vInitScale         = Vector3.zero;

	void Start()
    {
        //_init();
    }

    void _init()
    {
        _btnIItem               = GetComponent<tk2dUIItem>();        
        if(null != _btnIItem)
        {
            _btnIItem.OnReleaseUIItem -= CLICK;
            _btnIItem.OnReleaseUIItem += CLICK;
        }

        if(null != Target)
        {
            if(_vInitScale.Equals(Vector3.zero))
                _vInitScale     = Target.localScale;
            Target.localScale   = _vInitScale;
        }
        else 
            Debug.Log("Invalid Button Animation Target !!!");
    }

    private void OnEnable()
    {
        _init();
    }

    private void OnDisable()
    {
        if(null != _btnIItem)   _btnIItem.OnReleaseUIItem -= CLICK;
        _btnIItem               = null;
    }

    public void CLICK()
    {
        this.CLICK(null);
    }
    public void CLICK(tk2dUIItem item)
    {
        if(null == Target)      return;

        Target.DOScale(Target.localScale.x*1.1f, 0.05f).SetLoops(2, LoopType.Yoyo);
    }
	
}
