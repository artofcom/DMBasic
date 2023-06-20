using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace NOVNINE
{

public static class TextMeshEx {
    readonly static string NUMBERS = "0123456789";

    public static void IncreaseNumberWithAnimation (this TextMesh tm, int goal, 
        float duration = 0.5F, System.Func<string,string> preProcessing = null) {

        DOTween.Kill(tm.gameObject.GetInstanceID());

        int targetNumber = tm.TrimToNumber();

        DOTween.To(() => targetNumber, (x) => targetNumber = x, goal, duration).SetEase(Ease.Linear)
            .SetId(tm.gameObject.GetInstanceID()).OnUpdate( () => {
            string text = targetNumber.ToString("N0");
            if (preProcessing != null) text = preProcessing(text);
            tm.text = text;
        });
    }

    public static int TrimToNumber (this TextMesh tm) {
        int number;
        string numberString = "";

        for (int i = 0; i < tm.text.Length; i++) {
            if (NUMBERS.IndexOf(tm.text[i]) > -1) numberString += tm.text[i];
        }

        if (System.Int32.TryParse(numberString, out number)) {
            return number;
        } else {
            Debug.LogError("Error : TrimToNumber");
            return 0;
        }
    }
}

}
