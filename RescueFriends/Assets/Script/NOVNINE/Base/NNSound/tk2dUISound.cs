using UnityEngine;
using System.Collections;

public class tk2dUISound : tk2dUIBaseItemControl {
 	[HideInInspector][SerializeField] public string[] buttonSounds = new string[] {null, null, null, null};

    void OnEnable () {
        if (uiItem == null) return;
        if (string.IsNullOrEmpty(buttonSounds[0]) == false) uiItem.OnDown += PlayDownSound;
        if (string.IsNullOrEmpty(buttonSounds[1]) == false) uiItem.OnUp += PlayUpSound;
        if (string.IsNullOrEmpty(buttonSounds[2]) == false) uiItem.OnClick += PlayClickSound;
        if (string.IsNullOrEmpty(buttonSounds[3]) == false) uiItem.OnRelease += PlayReleaseSound;
    }

    void OnDisable () {
        if (uiItem == null) return;
        if (string.IsNullOrEmpty(buttonSounds[0]) == false) uiItem.OnDown -= PlayDownSound;
        if (string.IsNullOrEmpty(buttonSounds[1]) == false) uiItem.OnUp -= PlayUpSound;
        if (string.IsNullOrEmpty(buttonSounds[2]) == false) uiItem.OnClick -= PlayClickSound;
        if (string.IsNullOrEmpty(buttonSounds[3]) == false) uiItem.OnRelease -= PlayReleaseSound;
    }

    void PlayDownSound () {
        PlaySound(buttonSounds[0]);
    }

    void PlayUpSound () {
        PlaySound(buttonSounds[1]);
    }

    void PlayClickSound () {
        PlaySound(buttonSounds[2]);
    }

    void PlayReleaseSound () {
        PlaySound(buttonSounds[3]);
    }

    void PlaySound (string clipName) {
        if (string.IsNullOrEmpty(clipName)) return;
        NNSoundHelper.Play(clipName);
    }
}
