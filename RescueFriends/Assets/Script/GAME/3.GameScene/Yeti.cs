using UnityEngine;
using System.Collections;

public class Yeti : NNRecycler {
    Animator yeti;

    public static Yeti Current { get; private set; }

    public int PrevAttackMove { get; set; }

    public bool Dead {
        get { return yeti.GetBool("Dead"); }
        set { yeti.SetBool("Dead", value); }
    }

    void Awake () {
        yeti = GetComponent<Animator>();
    }

    void OnEnable () {
        Current = this;
        JMFRelay.OnChangeYetiHealthForDisplay += OnChangeYetiHealthForDisplay;
    }

    void OnDisable () {
        Current = null;
        JMFRelay.OnChangeYetiHealthForDisplay -= OnChangeYetiHealthForDisplay;
    }

    void OnChangeYetiHealthForDisplay (int health) {
        if (Dead) return;

        if (health > 0) {
            yeti.Play("yeti_attacked");
            NNSoundHelper.Play("yeti_damage");
        } else {
            Dead = true;
            NNSoundHelper.Play("yeti_death");
            JMFUtils.GM.IncreaseScore(10000, transform.localPosition, 6);
            Invoke("Recycle", 1F);
        }
    }

    public override void Reset () {
        base.Reset();
        Dead = false;
        PrevAttackMove = 0;
    }

    public void Recycle () {
        Current = null;
        NNPool.Abandon(gameObject);
    }

    public void ShowAnimation (string clipName) {
        yeti.Play(clipName);
    }
}
