using UnityEngine;
using System.Collections;
using DG.Tweening;

public class YetiAttackBall : NNRecycler {
    public Sprite[] sprites;

    SpriteRenderer spriteRenderer;

    void Awake () {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play (Vector3 scale, int index) {
        if (Yeti.Current == null) return;

        if (JMFUtils.GM.YetiHealth > 0) JMFUtils.GM.YetiHealth--;

        transform.localScale = scale;
        Vector3 targetPos = Yeti.Current.transform.position;
        targetPos.z -= 1F;
        spriteRenderer.sprite = sprites[index];

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(scale * 1.5F, 0.2F).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(scale, 0.3F).SetEase(Ease.OutQuad));
        seq.Insert(0F, transform.DOMove(targetPos, 0.5F).SetEase(Ease.InQuad));

        seq.OnComplete(() => {
            NNPool.Abandon(gameObject);
              
//            ParticlePlayer pp = NNPool.GetItem<ParticlePlayer>("AttackParticle", JMFUtils.GM.transform);
//            pp.transform.position = targetPos;
//            pp.transform.Translate(0F, -0.7F, 0F);
//            pp.Play();

            JMFRelay.FireOnChangeYetiHealthForDisplay(JMFUtils.GM.YetiHealth);
        });
    }
}
