using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NOVNINE.Diagnostics;
using NOVNINE;
//using Holoville.HOTween;
using DG.Tweening;

public class Boss : NNRecycler {
    public Animator boss;
    string bossName;
    public bool bossDead;
    //BossEffect deadEffect;
    public GameObject prison;
    public GameObject[] fairys;
    public GameObject Message;
    public GameObject fairyMessage;
    public TextMesh bossMessage;
    public tk2dTextMesh bossHealth;
    List<Vector3> fairyStartPosition = new List<Vector3>();

    public static Boss Current { get; private set; }

    public int PrevAttackMove { get; set; }


    void Awake () {
    }

    void OnEnable () {
        Current = this;
        string healthInfo = string.Format("{0} / {1}", JMFUtils.GM.BossHealth, JMFUtils.GM.CurrentLevel.bossHealth);
        bossHealth.text = healthInfo;
        bossHealth.Commit();
        Message.SetActive(false);
        fairyMessage.SetActive(false);
        SetBossName();
        fairyStartPosition.Clear();
        foreach (GameObject fairy in fairys) {
            fairyStartPosition.Add(fairy.transform.localPosition);
        }
        boss.gameObject.SetActive(false);
        JMFRelay.OnChangeBossHealth += OnChangeBossHealth;
        StartCoroutine(CoBossEntry());

    }

    void OnDisable () {
        Recycle();
        JMFRelay.OnChangeBossHealth -= OnChangeBossHealth;
    }

    void OnChangeBossHealth (int health, int damage) {
        if (bossDead) return;

        string healthInfo = string.Format("{0} / {1}", JMFUtils.GM.BossHealth, JMFUtils.GM.CurrentLevel.bossHealth);
        bossHealth.text = healthInfo;
        bossHealth.Commit();
        if (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) return;
        if (health > 0) {
            if (damage > 0) {
                if (JMFUtils.GM.CurrentLevel.bossType != 0) boss.Play(bossName+"_skill");
//				BossEffect effect = NNPool.GetItem<BossEffect>("boss_effect");
//                effect.gameObject.transform.position = this.gameObject.transform.position;
                NNSoundHelper.Play("boss_heal3");
                //effect.Play("heal");
                //StartCoroutine(AbandonEffect(effect.gameObject, 1F));
            } else {
                NNSoundHelper.Play("boss_damage2");
                boss.Play(bossName+"_damage");
            }
        } else {
            StartCoroutine(CoBossDeadEffect());
        }

    }

    IEnumerator CoBossEntry() {
        prison.SetActive(true);
        while (JMFUtils.GM.State != JMF_GAMESTATE.PLAY) yield return null;
        Message.SetActive(false);
        fairyMessage.SetActive(true);
        fairyMessage.transform.localScale = Vector3.zero;
//        HOTween.To(fairyMessage.transform, 0.2F, new TweenParms().Prop("localScale", Vector3.one*0.7F).Ease(EaseType.EaseOutBack));
		fairyMessage.transform.DOScale( Vector3.one*0.7F, 0.2F).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1F);
        fairyMessage.SetActive(false);
        //GameObject entryEffect = NNPool.GetItem("EntryEffect", transform);
        //Transform entryTransform = entryEffect.transform.Find("MagicPoof_stick");
        //ParticleSystem particle = entryTransform.gameObject.GetComponent<ParticleSystem>();
        //particle.GetComponent<Renderer>().sortingOrder = 10;
        //entryEffect.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(1F);
        NNSoundHelper.Play("monster_entry");
        boss.gameObject.SetActive(true);
        boss.Play(bossName+"_idle");
        yield return new WaitForSeconds(1F);
        StartCoroutine(CoBossEntrySound());
        //NNPool.Abandon(entryEffect.gameObject);
        Message.SetActive(true);
        bossMessage.text = "The fairy is all mine";
        Message.transform.localScale = Vector3.zero;
//        HOTween.To(Message.transform, 0.25F, new TweenParms().Prop("localScale", Vector3.one*0.7F).Ease(EaseType.EaseOutBack));
		Message.transform.DOScale(Vector3.one*0.7F, 0.25F).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1.5F);
        Message.SetActive(false);
        JMFUtils.GM.bossEntry = true;
    }

    IEnumerator CoBossDeadEffect() {
        JMFUtils.GM.isBossDeadEffect = true;
        bossDead = true;
        boss.Play(bossName+"_damage");
        JMFUtils.GM.IncreaseScore(10000, transform.localPosition, 6);
        yield return new WaitForSeconds(0.2F);
        while (!JMFUtils.GM.IsStable) yield return null;
        NNSoundHelper.Play("boss_death_bomb2");
        boss.Play(bossName+"_dead");
        Message.SetActive(true);
        bossMessage.text = "I'll beat you beat\n you next time!";
        Message.transform.localScale = Vector3.zero;
//        HOTween.To(Message.transform, 0.25F, new TweenParms().Prop("localScale", Vector3.one*0.7F).Ease(EaseType.EaseOutBack));
		Message.transform.DOScale(Vector3.one*0.7F, 0.25F).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1.2F);
        Message.SetActive(false);
        prison.SetActive(false);
        boss.gameObject.SetActive(false);
        foreach (Board bd in JMFUtils.GM.Boards) {
            if (bd.Panel.PND is BossPanel) { 
//                ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("FX_cage");
//                pp.transform.position = bd.Position;
//                pp.Play();
            }
        }
//        deadEffect = NNPool.GetItem<BossEffect>("boss_effect");
//        deadEffect.gameObject.transform.position = this.gameObject.transform.position;
//        deadEffect.Play("magicPoof");
        Invoke("BossBonusCrash", 0.1F);
        Invoke("AbandonDeadEffect", 0.5F);
        Current = null;
        yield return new WaitForSeconds(0.5F);
        foreach (GameObject fairy in fairys) {
            Transform t = fairy.transform.Find("effect");
            t.gameObject.SetActive(true);
            int x = NNTool.Rand(-5, 5);
            int y = NNTool.Rand(-5, 5);
            fairy.Slide(new Vector3(x, y, -10F), 2F);
        }
        yield return new WaitForSeconds(2F);
        foreach (GameObject fairy in fairys) {
            fairy.FadeOut(1F);
        }
        yield return new WaitForSeconds(1F);
        Recycle();
    }

    IEnumerator CoBossEntrySound () {
        yield return new WaitForSeconds(0.2F);
        NNSoundHelper.Play("boss_talk2");
        yield return new WaitForSeconds(0.2F);
        NNSoundHelper.Play("boss_talk2");
        yield return new WaitForSeconds(0.2F);
        NNSoundHelper.Play("boss_talk2");
    }

    void BossBonusCrash() {
        foreach (Board bd in JMFUtils.GM.Boards) {
            if (bd.Panel.PND is BossPanel) { 
		        //NNSoundHelper.Play("IFX_lineblock_bust");
				BlockLine effect_H = NNPool.GetItem<BlockLine>("BlockLine");
				
				effect_H.Play("horizontal_hit",bd.Position,bd.Piece.Scale, bd.ColorIndex, false);
				BlockLine effect_V = NNPool.GetItem<BlockLine>("BlockLine");
				effect_V.Play("vertical_hit",bd.Position,bd.Piece.Scale, bd.ColorIndex, false);
				
                DestroyDirection(bd, JMF_DIRECTION.LEFT, 0.1F);
                DestroyDirection(bd, JMF_DIRECTION.UP, 0.1F);
                DestroyDirection(bd, JMF_DIRECTION.RIGHT, 0.1F);
                DestroyDirection(bd, JMF_DIRECTION.DOWN, 0.1F);
            } else {
                continue;
            }
        }
    }

    void AbandonDeadEffect() {
        //if (deadEffect == null) return;
        //NNPool.Abandon(deadEffect.gameObject);
    }

    void DestroyDirection (Board bd, JMF_DIRECTION direction, float delayTime) {
        Debugger.Assert(bd != null, "PlayOverlayHandler.DestroyDirection : Board is null."); 

        Board targetBoard = bd[direction];

        float delay = delayTime;

        while (targetBoard != null) {
            targetBoard.Hit(delay);

            // note : cut 하지 않는 것으로 변경.
            //if (targetBoard.PD is RoundChocoPiece || targetBoard.PD is GreenBubblePiece)
            //    break;

            targetBoard = targetBoard[direction];
            delay += delayTime;
        }
    }

    void ShowBossMessage(string msg) {
    }

    void SetBossName() {
        if (JMFUtils.GM.CurrentLevel.bossType == 1) {
            bossName = "boss_heal";
        } else if (JMFUtils.GM.CurrentLevel.bossType == 2) {
            bossName = "boss_attack";
        } else {
            bossName = "boss";
        }
    }

    IEnumerator AbandonEffect(GameObject go, float t)  {
        yield return new WaitForSeconds(t);
        NNPool.Abandon(go);
    }

    public override void Reset () {
        base.Reset();
        bossDead = false;
        PrevAttackMove = 0;
    }

    public void Recycle () {
        Current = null;
        JMFUtils.GM.isBossDeadEffect = false;
        NNPool.Abandon(gameObject);

        if(JMFUtils.GM.BossHealth > 0) return;

        int i = 0;
        foreach (GameObject fairy in fairys) {
            fairy.FadeIn(0.1F);
            Transform t = fairy.transform.Find("effect");
            t.gameObject.SetActive(false);
            if (fairyStartPosition.Count-1 < i) continue;
            fairy.transform.localPosition = fairyStartPosition[i];
            i++;
        }
        fairyStartPosition.Clear();
    }

    public void BossAttack (System.Action callback) {
        boss.Play(bossName+"_skill");
        StartCoroutine(ShowBossAttackEffect(callback));
    }

    IEnumerator ShowBossAttackEffect(System.Action callback) {
//        BossEffect effect = NNPool.GetItem<BossEffect>("boss_effect");
//        effect.gameObject.transform.position = this.gameObject.transform.position;
//        effect.Play("aura");
        yield return new WaitForSeconds(1F);
        NNSoundHelper.Play("yeti_attack");
//        BossEffect effect2 = NNPool.GetItem<BossEffect>("boss_effect");
//        effect2.gameObject.transform.position = this.gameObject.transform.position;
//        effect2.Play("attack");
//        StartCoroutine(AbandonEffect(effect.gameObject, 1F));
//        StartCoroutine(AbandonEffect(effect2.gameObject, 1F));
        if (callback != null) callback();
    }
}
