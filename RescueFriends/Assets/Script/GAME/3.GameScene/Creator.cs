using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Creator : Panel {
    public GameObject handle;

    public void Spin () {
        if (handle == null) return;
        if (DOTween.IsTweening(GetInstanceID())) return;

        NNSoundHelper.Play("dispenser_release");
        handle.transform.DOLocalRotate(new Vector3(0F, 0F, 360F), 0.5F, RotateMode.FastBeyond360)
            .SetId(GetInstanceID())
            .SetEase(Ease.InOutQuint);
    }

    public override void Reset () {
        base.Reset();
        if (handle != null) {
            handle.transform.localRotation = Quaternion.identity;
        }
    }
}
