using UnityEngine;
using System.Collections;
using NOVNINE.Diagnostics;

public class Frog : Block {
    public GameObject focusEffect;
    public Animator frog;

    int eatingCount;
    public int EatingCount {
        get { return eatingCount; }
        set {
            if (eatingCount == value) return;
            eatingCount = value;

            if (Owner != null) {
                Debugger.Assert(Owner.PD is FrogPiece, "Frog.EatingCount : PieceDefinition is not FrogPiece.");

                if (IsReadyToJump) {
                    focusEffect.SetActive(true);
                } else {
                    focusEffect.SetActive(false);
                }
            }
        }
    }

    public bool IsReadyToJump {
        get { 
            Debugger.Assert(Owner != null, "Frog.IsReadyToJump : GamePiece is null.");
            Debugger.Assert(Owner.PD is FrogPiece, "Frog.EatingCount : PieceDefinition is not FrogPiece.");

            return EatingCount >= (Owner.PD as FrogPiece).fillingCount;
        }
    }

    public override Bounds GetBounds () {
        return JMFUtils.GetBoundsRecursive(gameObject);
    }

    void OnEnable () {
        focusEffect.SetActive(false);;
        eatingCount = 0;
    }

    public void ShowEatAnimation () {
        frog.Play("slime_eat");
        NNSoundHelper.Play("slime_eat");
    }
}
