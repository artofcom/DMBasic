using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE.Diagnostics;

public static class Sound
{
	static bool enableSFX;
	static bool enableBGM;
	
	static Sound() {
		enableSFX = (PlayerPrefs.GetInt("Sound.enableSFX", 1) == 1);
		enableBGM = (PlayerPrefs.GetInt("Sound.enableBGM", 1) == 1);
	}
	
	public static void Play (string soundName, float volume = 1.0f, float pitch = 1.0f, bool loop = false) {
		if(!enableSFX) return;
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : Play - SoundManager Instance is null");
		SoundManager.Instance.Play(soundName, volume, pitch, loop);
	}
	
	public static void PlayBGM (string soundName, float volume = 1.0f, int channel = 0, bool loop = true, bool useFade = true) {
		if(!enableBGM) return;
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : PlayBGM - SoundManager Instance is null");
		SoundManager.Instance.PlayBGM(soundName, volume, channel, loop, useFade);
	}
	
	public static void StopByName (string soundName) {
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : StopByName - SoundManager Instance is null");
		SoundManager.Instance.Stop(soundName);
	}
	
	public static void StopBGM () {
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : StopBGM - SoundManager Instance is null");
		SoundManager.Instance.StopBGM();
	}
	
	public static bool IsPlaySound (string soundName) {
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : IsPlaySound - SoundManager Instance is null");
		return SoundManager.Instance.IsPlaySound (soundName);
	}
	
	public static void Stop (string soundName) { 
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : Stop - SoundManager Instance is null");
		SoundManager.Instance.Stop(soundName);
	}
	
	public static void SetVolume (float vol) {
		AudioListener.volume = vol;
	}
	
	public static void SetBGMVolume (float volume) {
        Debugger.Assert(SoundManager.Instance != null, "SoundManager : SetBGMVolume - SoundManager Instance is null");
		SoundManager.Instance.SetBGMVolume(volume);
	}
	
	public static bool EnableBGM {
		get {
			return enableBGM;
		}
		set {
			enableBGM = value;
			if(enableBGM == false)
				StopBGM();
			PlayerPrefs.SetInt("Sound.enableBGM", enableBGM?1:0);
			PlayerPrefs.Save();
		}
	}
	
	public static bool EnableSFX {
		get {
			return enableSFX;
		}
		set {
			enableSFX = value;
			PlayerPrefs.SetInt("Sound.enableSFX", enableSFX?1:0);
			PlayerPrefs.Save();
		}
	}
}

public sealed class SoundManager : SingletonMonoBehaviour<SoundManager>
{
	public AudioClip[] clips;
	public int BGMChannelCount = 1;
	Hashtable audioClips = new Hashtable();
	List<AudioSource> soundPlayers = new List<AudioSource>();
	GameObject sounds;
	List<int> bgmChannels = new List<int>();
	
	protected override void Awake () {
		base.Awake();
		sounds = new GameObject("+SoundManager");
		
		if(clips == null) {
			Debugger.LogWarning("SoundManager.Awake : no clips");
			return;
		}
		foreach (AudioClip clip in clips) {
			if(clip != null)
				audioClips.Add(clip.name, clip);
		}
		
		//create BGM player
		for(int i=0; i<BGMChannelCount; ++i) {
			CreateAudioSource();
			bgmChannels.Add(i);
		}
	}
	
	public void Play (string soundName, float volume = 1.0f, float pitch = 1.0f, bool loop = false) {
		AudioClip clip = audioClips[soundName] as AudioClip;
		
		if (clip == null) {
			Debugger.LogWarning(string.Format("Can't found audio clip - {0}", soundName));
			return;
		}
		
		var soundPlayer = GetSoundPlayer();
		
		soundPlayer.clip = clip;
		soundPlayer.loop = loop;
		soundPlayer.volume = volume;
		soundPlayer.pitch = pitch;
		soundPlayer.Play();
	}
	
	public void PlayBGM (string soundName, float volume = 1.0f, int channel = 0, bool loop = true, bool useFade = true) {
		Debugger.Log("SoundManager.PlayBGM : "+soundName+" ch:"+channel);
//		if (NOVNINE.NativeInterface.IsMusicPlaying()) return;
		AudioClip clip = audioClips[soundName] as AudioClip;
		
		if (clip == null) {
			Debugger.LogWarning(string.Format("Can't found audio clip - {0}", soundName));
			return;
		}
		
//		HOTween.Complete("BGM_Fade"+channel);
		DOTween.Complete("BGM_Fade"+channel);
		var bgm = soundPlayers[channel];
        Debugger.Assert(bgm != null, "SoundManager : PlayBGM - bgm is null");
        
		if (!bgmChannels.Contains(channel)) bgmChannels.Add(channel);
		
		if (bgm.isPlaying) {
			if (bgm.clip == clip) return;
			
			if (useFade) {
//				Sequence seq = new Sequence(new SequenceParms().Id("BGM_Fade"+channel));
//				seq.Append( HOTween.To(bgm, 0.5f, "volume", 0) );
				Sequence seq = DOTween.Sequence();
				seq.SetId("BGM_Fade"+channel);
				seq.Append( DOTween.To(()=> bgm.volume, x=> bgm.volume = x, 0, 0.5f) );
				
				seq.AppendInterval( 0.3f );
//				seq.AppendCallback( (parm) => {
//					bgm.clip = clip;
//					bgm.loop = loop;
//					bgm.Play();
//				});
				seq.AppendCallback( () => {
					bgm.clip = clip;
					bgm.loop = loop;
					bgm.Play();
				});
//				seq.Append( HOTween.To(bgm, 0.5f, "volume", volume) );
				seq.Append( DOTween.To(()=> bgm.volume, x=> bgm.volume = x, volume, 0.5f) );
				seq.Play();
			} else {
				bgm.clip = clip;
				bgm.loop = loop;
				bgm.Play();
			}
		} else {
			bgm.clip = clip;
			bgm.loop = loop;
			
			if (useFade) {
				bgm.volume = 0;
//				Sequence seq = new Sequence(new SequenceParms().Id("BGM_Fade"+channel));
//				seq.Append( HOTween.To(bgm, 0.5f, "volume", volume) );
				Sequence seq = DOTween.Sequence();
				seq.SetId("BGM_Fade"+channel);
				seq.Append( DOTween.To(()=> bgm.volume, x=> bgm.volume = x, volume, 0.5f) );
				seq.Play();
			} else {
				bgm.volume = volume;
			}
			
			bgm.Play();
		}
	}
	
	public void StopBGM () {
		foreach(int ch in bgmChannels) {
			var bgm = soundPlayers[ch];
            Debugger.Assert(bgm != null, "SoundManager : StopBGM - bgm is null");
			if(bgm.isPlaying) {
//				HOTween.Complete("BGM_Fade"+ch);
				DOTween.Complete("BGM_Fade"+ch);
//				Sequence seq = new Sequence(new SequenceParms().Id("BGM_Fade"+ch));
//				seq.Append( HOTween.To(bgm, 0.5f, "volume", 0) );
				Sequence seq = DOTween.Sequence();
				seq.SetId("BGM_Fade"+ch);
				seq.Append( DOTween.To(()=> bgm.volume, x=> bgm.volume = x, 0, 0.5f) );
//				seq.AppendCallback( (parm) => {
//					bgm.Stop();
//				});
				seq.AppendCallback( () => {
					bgm.Stop();
				});
				seq.Play();
			}
		}
	}
	
	public void SetBGMVolume (float volume) {
        Debugger.Assert(bgmChannels != null, "SoundManager : SetBGMVolume - bgmChannel is null");
		foreach(int ch in bgmChannels) {
			soundPlayers[ch].volume = volume;
		}
	}
	
	public bool IsPlaySound(string soundName) {
		AudioClip clip = audioClips[soundName] as AudioClip;
		
		foreach (AudioSource soundPlayer in soundPlayers) {
			if (soundPlayer == null) continue;
			if (soundPlayer.isPlaying == true && soundPlayer.clip == clip) {
				return true;
			}
		}
		return false;
		
	}
	
	public void Stop (string soundName) {
		AudioClip clip = audioClips[soundName] as AudioClip;
		
		foreach (AudioSource soundPlayer in soundPlayers) {
			if (soundPlayer == null) continue;
			if (soundPlayer.isPlaying == true && soundPlayer.clip == clip) {
				soundPlayer.Stop();
			}
		}
	}
	
	public bool IsExistSound(string soundName){
		AudioClip clip = audioClips[soundName] as AudioClip;
		
		if (clip == null) {
			return false;
		}
		
		return true;
	}
	
	AudioSource CreateAudioSource() {
		GameObject sp = new GameObject("SoundPlayer");
        Debugger.Assert(sp != null, "SoundManager : CreateAudioSource - SoundPlayer Gameobject is null");
		var audio = sp.AddComponent<AudioSource>() as AudioSource;

        Debugger.Assert(sounds.transform != null, "SoundManager : CreateAudioSource - SoundPlayer Gameobject parent is null");
		sp.transform.parent = sounds.transform;

		soundPlayers.Add(audio);
		return audio;
	}
	
	AudioSource GetSoundPlayer () {
		for(int i=2; i<soundPlayers.Count; ++i) {
			//foreach (var soundPlayer in soundPlayers) {
			var sp = soundPlayers[i];
			if (sp.GetComponent<AudioSource>().isPlaying == false) {
				return sp;
			}
		}
		return CreateAudioSource();
	}
}

