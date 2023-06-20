using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;

public class HandIndicator : MonoBehaviour {

    Vector3 originalPos;
    Vector3 targetPos;
    public bool Showing { get; private set; }

    void Awake ()
    {
        originalPos = transform.localPosition;
        targetPos = originalPos + Vector3.down;
    }

    void OnEnable ()
    {
        Showing = true;
        transform.localPosition = originalPos;
//        TweenParms parms = new TweenParms().Prop("localPosition", targetPos)
//                                           .IntId(transform.GetInstanceID())
//                                           .Ease(EaseType.EaseOutCirc)
//                                           .Loops(-1, LoopType.Yoyo);
//        HOTween.To(transform, .3f, parms);
		TweenParams parms = new TweenParams();
		parms.SetId(transform.GetInstanceID());
		parms.SetEase(Ease.OutCirc);
		parms.SetLoops(-1, LoopType.Yoyo);
		transform.DOLocalMove(targetPos,0.3f);
    }

    void OnDisable ()
    {
        //HOTween.Kill(transform.GetInstanceID());
		DOTween.Kill(transform.GetInstanceID());
        Showing = false;
    }
}
