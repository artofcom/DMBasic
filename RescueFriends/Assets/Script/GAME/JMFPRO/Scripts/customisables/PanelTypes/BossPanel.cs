using UnityEngine;
using System.Collections;
using JsonFx.Json;
using NOVNINE.Diagnostics;

public class BossPanel : PanelDefinition {
    public static bool HitThisTurn { get; set; }
    public static int StraightNotHitCount { get; set; }

    public class Info {
        public int Index { get; set; }
    }

    void OnEnable () {
        JMFRelay.OnBoardStable += OnBoardStable;
    }

    void OnDisable () {
        JMFRelay.OnBoardStable -= OnBoardStable;
    }

	public override bool IsSolid (BoardPanel bp) { return true; }
	public override bool IsFallable (BoardPanel bp) { return false; }
	public override bool IsFillable (BoardPanel bp) { return false; }
	public override bool IsMatchable (BoardPanel bp) { return false; }
	public override bool IsStealable (BoardPanel bp) { return false; }
	public override bool IsSwitchable (BoardPanel bp) { return false; }
	public override bool IsDestroyable (BoardPanel bp) { return false; }
	public override bool IsSplashHitable (BoardPanel bp) { return true; }
	public override bool IsDestroyablePiece (BoardPanel bp) { return false; }
    public override bool IsShufflable (BoardPanel bp) { return false; }

    public override object ConvertToPanelInfo (string data) {
        Debugger.Assert(string.IsNullOrEmpty(data) == false);
        return JsonReader.Deserialize<Info>(data);
    }

	protected override void OnHit (BoardPanel bp, bool isSpecialAttack) {
        int damage = 1;
        if (isSpecialAttack) {
            damage = JMFUtils.GM.bossSpecialAttackDamage;
        } else {
            damage = 1;
        }
        JMFUtils.GM.BossHealth -= damage;
        StraightNotHitCount = 0;
        HitThisTurn = true;

        if (JMFUtils.GM.State == JMF_GAMESTATE.PLAY) {
//            BossHitEffect effect = NNPool.GetItem<BossHitEffect>("BossHitEffect");
//            effect.transform.position = bp.Owner.Position;
//            effect.ShowHitEffect(damage);
        }
	}

    void OnBoardStable () {
        if (HitThisTurn == false) {
            StraightNotHitCount++;

            if (JMFUtils.GM.CurrentLevel.bossType == 1 && 
                (StraightNotHitCount+1 > JMFUtils.GM.CurrentLevel.bossActionPerMove))  {
                int GainHealCount = JMFUtils.GM.BossHealth + JMFUtils.GM.CurrentLevel.bossHealCount;
                if (GainHealCount >= JMFUtils.GM.CurrentLevel.bossHealth) {
                    JMFUtils.GM.BossHealth = JMFUtils.GM.CurrentLevel.bossHealth;
                } else {
                    JMFUtils.GM.BossHealth = GainHealCount;
                }
                StraightNotHitCount = 0;
            }
        }
        HitThisTurn = false;
    }
}
