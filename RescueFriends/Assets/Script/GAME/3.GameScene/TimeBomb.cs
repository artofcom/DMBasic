using UnityEngine;
using System.Collections;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE.Diagnostics;

public class TimeBomb : Block {
    public TextMesh timeText;
    public SpriteRenderer warningSprite;

    bool isWarning = false;
    int fallBackTime = 9;

    public bool MarkedForDestroy { get; set; }
    public static int TotalCount { get; private set; }

    public int FallBackTime {
        get { return fallBackTime; }
        set { 
            Debugger.Assert(value > -1, "Timebomb.FallBackTime : ");
            fallBackTime = value;
            timeText.text = fallBackTime.ToString();
            AnimateWarning(fallBackTime <= 3);
        }
    }

    void OnEnable () {
        JMFRelay.OnPlayerMove += OnPlayerMove;
        TotalCount++;
    }

    void OnDisable () {
        JMFRelay.OnPlayerMove -= OnPlayerMove;
        AnimateWarning(false);
        TotalCount--;
    }

    public override void Reset () {
        base.Reset();
        isWarning = false;
        MarkedForDestroy = false;
        FallBackTime = JMFUtils.GM.CurrentLevel.defaultFallBackTime;
    }

    public void SmallExplosion () {
//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("BombExplosion_S");
//        Vector3 v = transform.position;
//        v.z -= 1f;
//        pp.transform.position = v;
//        pp.Play();
    }

    public void LargeExplosion () {
//        ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("BombExplosion_L");
//        Vector3 v = transform.position;
//        v.z -= 1f;
//        pp.transform.position = v;
//        pp.Play();
//        //JMFUtils.WC.ForceGameOver(1F);
        NNSoundHelper.Play("Timebomb_explode");
    }

    void OnPlayerMove () {
        FallBackTime--;
        timeText.text = fallBackTime.ToString();
    }

    void AnimateWarning (bool enable)
    {
        warningSprite.gameObject.SetActive(enable);
        if (enable == true) {
            if (isWarning == true) return;

            isWarning = true;
            warningSprite.color = new Color(1,1,1,0);
//            TweenParms parms = new TweenParms().Prop("color", Color.white)
//                                               .Loops(-1, LoopType.Yoyo)
//                                               .IntId(warningSprite.GetInstanceID())
//                                               .Ease(EaseType.EaseOutQuad);
//            HOTween.To(warningSprite, .5f, parms);
			TweenParams parms = new TweenParams();
			parms.SetLoops(-1, LoopType.Yoyo);
			parms.SetId(warningSprite.GetInstanceID());
			parms.SetEase(Ease.OutQuad);
//			warningSprite.DOColor(Color.white, 5f).SetAs(parms);
			DOTween.To(() => warningSprite.color, x => warningSprite.color = x, Color.white,5f).SetAs(parms);
        } else {
            isWarning = false;
//            HOTween.Kill(warningSprite.GetInstanceID());
			DOTween.Kill(warningSprite.GetInstanceID());
        }
    }
}
