using UnityEngine;
using System.Collections;

public class AudioObserver : MonoBehaviour
{

    static public System.Action onChangeVolumeRate;

    static public System.Action<bool> onChangeSFX;
    static public System.Action<bool> onChangeBGM;

    static float _volumeRate = 1f;
    static bool _sfx = true;
    static bool _bgm = true;

    float _volume = 1f;
    public bool sfx = true;

    static public void SetVolume(float volumeRate = 1f)
    {
        if (AudioObserver._volumeRate == volumeRate) return;

        AudioObserver._volumeRate = volumeRate;
        if (null != AudioObserver.onChangeVolumeRate)
            AudioObserver.onChangeVolumeRate();
    }

    static public void SetSFX(bool on)
    {
        //if (AudioObserver._sfx == on) return;

        AudioObserver._sfx = on;
        if (null != AudioObserver.onChangeSFX)
            AudioObserver.onChangeSFX(on);
    }

    static public void SetBGM(bool on)
    {
        //if (AudioObserver._bgm == on) return;

        AudioObserver._bgm = on;
        if (null != AudioObserver.onChangeBGM)
            AudioObserver.onChangeBGM(on);
    }

    void Awake()
    {
        AudioObserver.onChangeVolumeRate += OnMsgChangeVolumeRate;
        AudioObserver.onChangeSFX += OnMsgChangeSFX;
        AudioObserver.onChangeBGM += OnMsgChangeBGM;

        if (null != this.GetComponent<AudioSource>())
            _volume = this.GetComponent<AudioSource>().volume;

        OnMsgChangeVolumeRate();
    }

    void OnDestroy()
    {
        AudioObserver.onChangeVolumeRate -= OnMsgChangeVolumeRate;
        AudioObserver.onChangeSFX -= OnMsgChangeSFX;
        AudioObserver.onChangeBGM -= OnMsgChangeBGM;
    }

    void OnMsgChangeVolumeRate()
    {
        if (null == this.GetComponent<AudioSource>()) return;

        if (this.sfx == true && false == _sfx) {
            this.GetComponent<AudioSource>().volume = 0f;
            this.GetComponent<AudioSource>().mute = true;
        } else if (this.sfx == false && false == _bgm) {
            this.GetComponent<AudioSource>().volume = 0f;
            this.GetComponent<AudioSource>().mute = true;
        } else {
            this.GetComponent<AudioSource>().volume = _volume * _volumeRate / 1f;
            this.GetComponent<AudioSource>().mute = false;
        }
    }

    void OnMsgChangeSFX(bool on)
    {
        _sfx = on;
        OnMsgChangeVolumeRate();
    }

    void OnMsgChangeBGM(bool on)
    {
        _bgm = on;
        OnMsgChangeVolumeRate();
    }
}

public static class AudioSourceExtensionsForAudioObserver
{
    public static AudioObserver RegisterAudioObserver(this AudioSource source)
    {
        AudioObserver item = source.gameObject.GetComponent<AudioObserver>();
        if (null == item) {
            item = source.gameObject.AddComponent<AudioObserver>();
        }

        return item;
    }
}

public static class tk2dButtonExtensionsForAudioObserver
{
    public static AudioObserver RegisterAudioObserver(this tk2dButton button, AudioClip buttonPressedSound)
    {
        if (null == button.buttonPressedSound && null != buttonPressedSound)
            button.buttonPressedSound = buttonPressedSound;

        if (null == button.GetComponent<AudioSource>())
            button.gameObject.AddComponent<AudioSource>();

        AudioObserver item = button.GetComponent<AudioObserver>();
        if (null == item) {
            item = button.gameObject.AddComponent<AudioObserver>();
        }

        return item;
    }

    /*
    public static void Temp() {
        tk2dButton[] buttons = Resources.FindObjectsOfTypeAll(typeof(tk2dButton)) as tk2dButton[];
        for (int i = 0; i < buttons.Length; i++) {

    		if (null != buttons[i].buttonUpSound ||
    			null != buttons[i].buttonDownSound ||
    			null != buttons[i].buttonPressedSound)
    		{
    			Debug.Log(NGUITools.GetHierarchy(buttons[i].gameObject));
    		}

    		buttons[i].buttonUpSound = null;
    					buttons[i].buttonDownSound = null;
    					buttons[i].buttonPressedSound = null;
            // buttons[i].RegisterAudioObserver(this.buttonDownSound);
        }
    } */
}

/*
public static class UIButtonSoundExtensionsForAudioObserver
{
    public static AudioObserver RegisterAudioObserver(this UIButtonSound button, AudioClip buttonDownSound)
    {
		if (null == button.audioClip && null != buttonDownSound)
		    button.audioClip = buttonDownSound;

		if (null == button.audio)
		    button.gameObject.AddComponent("AudioSource");

		AudioObserver item = button.GetComponent<AudioObserver>();
		if (null == item)
		{
			item = button.gameObject.AddComponent<AudioObserver>();
		}

		return item;
	}
}
*/

