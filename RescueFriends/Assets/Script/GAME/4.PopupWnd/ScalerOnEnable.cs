using UnityEngine;
using System.Collections;
using NOVNINE;
using NOVNINE.Store;
using DG.Tweening;

public class ScalerOnEnable : MonoBehaviour
{
    public float                scaleInit   = 0.001f;
    public float                scaleTo     = 1.0f;
    public float                duration    = 0.3f;
    public float                delay       = .0f;
    public Ease                 ease        = Ease.OutBack;

    void OnEnable ()
    {
        transform.localScale    = Vector3.one * scaleInit;

        Sequence doSeq      = DOTween.Sequence();
        doSeq.PrependInterval(delay);
        doSeq.Append( transform.DOScale(scaleTo, duration).SetEase(ease) );
    }

}
