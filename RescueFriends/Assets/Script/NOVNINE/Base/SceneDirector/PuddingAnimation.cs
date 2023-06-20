using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
//using Holoville.HOTween.Plugins;
using DG.Tweening;


public class PuddingAnimation : MonoBehaviour {
    
    public Vector2 targetScale = new Vector2(1.02f, 0.97f);
    public float duration = 1.2f;
    public bool playOnce=false; 
    float yDelay = 0.1f;

    Transform transformCache;
    Vector3 originalScale;
//    EaseType ease = EaseType.EaseInOutCubic;
	Ease ease = Ease.InOutCubic;
    
    void Awake () {
        transformCache = transform;
        originalScale = transformCache.localScale;
    }

    void OnEnable ()
    {
        Do(gameObject);
    }

    void OnDisable ()
    {
        Reset();
    }

    public IEnumerator coAnimate() {
//        HOTween.Kill(transformCache);
		DOTween.Kill(transformCache);
//        TweenParms parmsX = new TweenParms().Prop("localScale", new PlugVector3X(targetScale.x, ease, false));
		TweenParams parmsX = new TweenParams();
		parmsX.SetEase(ease);
		
//        if (playOnce == false) parmsX.Loops(-1, LoopType.Yoyo);
//        HOTween.To(transformCache, duration * 0.5f, parmsX);
		if (playOnce == false) parmsX.SetLoops(-1, LoopType.Yoyo);
		transformCache.DOScaleX(targetScale.x, duration * 0.5f).SetAs(parmsX);

        yield return new WaitForSeconds(yDelay);

//        TweenParms parmsY = new TweenParms().Prop("localScale", new PlugVector3Y(targetScale.y, ease, false));
		TweenParams parmsY = new TweenParams();
        if (playOnce == false) {
			parmsY.SetLoops(-1, LoopType.Yoyo);//parmsY.Loops(-1, LoopType.Yoyo);
        } else {
            parmsY.OnComplete(TweenScaleToOriginal);
        }

        //HOTween.To(transformCache, duration * 0.5f, parmsY);
		transformCache.DOScaleX(targetScale.y, duration * 0.5f).SetAs(parmsY);
    }

    public static void Do (GameObject target)
    {
        PuddingAnimation inst = target.GetComponent<PuddingAnimation>();
        if(inst == null) 
            inst = target.AddComponent<PuddingAnimation>();

        inst.StartCoroutine(inst.coAnimate());
    }
    
    void TweenScaleToOriginal(){
//        HOTween.To(transformCache, duration * 0.5f, "localScale", originalScale);
		transformCache.DOScale(originalScale, duration * 0.5f);	
    }

    void Reset ()
    {
//        HOTween.Kill(transformCache);
		DOTween.Kill(transformCache);
        transformCache.localScale = originalScale;
    }
}

