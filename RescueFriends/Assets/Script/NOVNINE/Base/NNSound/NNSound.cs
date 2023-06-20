using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
//using Holoville.HOTween;
using DG.Tweening;
using NOVNINE.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class NNSoundHelper { 
    static bool enableSFX;
    static bool enableBGM;
    static float volumeMaster;
    static float volumeSFX;
    static float volumeBGM;

	static NNSoundHelper () {
        enableSFX = (PlayerPrefs.GetInt("Sound.enableSFX", 1) == 1);
        enableBGM = (PlayerPrefs.GetInt("Sound.enableBGM", 1) == 1);
        volumeMaster = PlayerPrefs.GetFloat("Sound.volumeMaster", 1F);
        volumeSFX = PlayerPrefs.GetFloat("Sound.volumeSFX", 1F);
        volumeBGM = PlayerPrefs.GetFloat("Sound.volumeBGM", 1F);

//		NNSound.Instance.Initialize();
		NNSound.Instance.SetVolumeSFX(volumeSFX);
		NNSound.Instance.SetVolumeBGM(volumeBGM);
        AudioListener.volume = volumeMaster;
    }

    public static bool EnableSFX {
        get { return enableSFX; }
        set {
            if (enableSFX == value) return;
            enableSFX = value;
            PlayerPrefs.SetInt("Sound.enableSFX", enableSFX ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool EnableBGM {
        get { return enableBGM; }
        set {
            if (enableBGM == value) return;
            enableBGM = value;
            PlayerPrefs.SetInt("Sound.enableBGM", enableBGM ? 1 : 0);
            PlayerPrefs.Save();
            if (enableBGM == false) StopBGMAll();
        }
    }

    public static float VolumeMaster {
        get { return volumeMaster; }
        set {
            if (volumeMaster == value) return;
            volumeMaster = value;
            PlayerPrefs.SetFloat("Sound.volumeMaster", volumeMaster);
            PlayerPrefs.Save();
            AudioListener.volume = volumeMaster;
        }
    }

    public static float VolumeSFX {
        get { return volumeSFX; }
        set {
            if (volumeSFX == value) return;
            volumeSFX = value;
            PlayerPrefs.SetFloat("Sound.volumeSFX", volumeSFX);
            PlayerPrefs.Save();
            NNSound.Instance.SetVolumeSFX(volumeSFX);
        }
    }

    public static float VolumeBGM {
        get { return volumeBGM; }
        set {
            if (volumeBGM == value) return;
            volumeBGM = value;
            PlayerPrefs.SetFloat("Sound.masterVolume", volumeBGM);
            PlayerPrefs.Save();
			NNSound.Instance.SetVolumeBGM(volumeBGM);
        }
    }
	
	public static void PlayBGM (string clipName, int channel = 0, bool loop = true, bool useFade = true) {
		if (enableBGM) NNSound.Instance.PlayBGM(clipName, VolumeBGM, channel, loop, useFade);
    }

    public static void StopBGM (int channel = 0, bool useFade = true, float fadeTime = 0.5f) {
		NNSound.Instance.StopBGM(channel, useFade, fadeTime);
    }

    public static void StopBGMAll (bool useFade = true) {
		NNSound.Instance.StopBGMAll(useFade);
    }

    public static void Play (string clipName, bool loop = false,float pitch = 1f) {
		if (enableSFX) NNSound.Instance.Play(clipName, VolumeSFX, loop, pitch);
    }

    public static void Stop (string clipName) {
		NNSound.Instance.Stop(clipName);
    }

    public static void StopAll () {
        NNSound.Instance.StopAll();
    }

    public static bool IsPlaying (string clipName) {
        return NNSound.Instance.IsPlaying (clipName);
    }

    public static void SetMaxChannelForClip(string clipName, int maxChn) {
        NNSound.Instance.SetMaxChannelForClip (clipName, maxChn);
    }
}

public class NNSound : ScriptableObject {
	const string themeSettingsAssetName = "NNSoundSettings";
    const string themeSettingsPath = "Resources";
    const string themeSettingsAssetExtension = ".asset";

	static NNSound instance;

    [NonSerialized] public static Transform soundParent;
	
	[NonSerialized] Dictionary<string, ClipInfo> clipDic = new Dictionary<string, ClipInfo>();
	List<ClipInfo> clipsForBGM = new List<ClipInfo>();

	public AudioClip[] audioClips;
	public bool warnOnSkip = true;
	
    class ClipInfo {
        public int maxCnt;
        public List<AudioSource> sources = new List<AudioSource>();
        public AudioClip clip;

        public ClipInfo(AudioClip _clip, int max = 0) {
            clip = _clip;
            SetMaxChannel(max);
        }

        public bool IsPlaying {
            get {
                for(int i=0; i<sources.Count; ++i) {
                    if(sources[i].isPlaying)
                        return true;
                }
                return false;
            }
        }

        public void SetMaxChannel(int max) {
            maxCnt = max;

            if(maxCnt > 0) {
                for(int i=sources.Count; i<max; ++i) 
                    sources.Add(CreateAudioSource(clip.name+"_"+sources.Count));
            }
        }

        public AudioSource GetPlayer(int idx=0) {
            if(sources.Count <= idx) {
                for(int i=sources.Count; i<(idx+1); ++i) 
                    sources.Add(CreateAudioSource(clip.name+"_"+sources.Count));
            }
            return sources[idx];
        }

        public void SetVolume(float vol) {
            for(int i=0; i<sources.Count; ++i) 
                sources[i].volume = vol;
        }

        void PlayClip(AudioSource s, float vol, bool loop, float pitch = 1f) {
            s.clip = clip;
            s.loop = loop;
            s.volume = vol;
            s.pitch = pitch;
            s.Play();
        }

        public bool Play(float volume, bool loop, float pitch = 1f) {
            AudioSource slot = null;
            for(int i=0; i<sources.Count; ++i) {
                if(!sources[i].isPlaying) {
                    slot = sources[i];
                    break;
                }
            }

            if(slot == null) {
                if(maxCnt <= 0) {
                    slot = CreateAudioSource(clip.name+"_"+sources.Count);
                    sources.Add(slot);
                } else {
                    if(instance.warnOnSkip)
                        Debug.LogWarning("NNSoundHelper : ChannelCount("+maxCnt+") for clip["+clip.name+"] exceeded. skipping..");
                    return false;
                }
            }

            PlayClip(slot, volume, loop, pitch);
            return true;
        }

        public void Stop() {
            for(int i=0; i<sources.Count; ++i) {
                if(sources[i].isPlaying) {
                    sources[i].Stop();
                }
            }
        }

        public void SetPitch (float amount) {
            for(int i=0; i<sources.Count; ++i) {
                sources[i].pitch = amount;
            }
        }

        AudioSource CreateAudioSource (string name = null) {
            GameObject soundPlayer = new GameObject(name);
            AudioSource audioSource = soundPlayer.AddComponent<AudioSource>() as AudioSource;
            soundPlayer.transform.parent = NNSound.soundParent;
            return audioSource;
        }

    }
    
	public static NNSound Instance {
        get {
            if (instance == null) {
				instance = Resources.Load(themeSettingsAssetName) as NNSound;

                if (instance == null) {
					instance = CreateInstance<NNSound>();
#if UNITY_EDITOR
                    string properPath = Path.Combine(Application.dataPath, themeSettingsPath);

                    if (!Directory.Exists(properPath)) {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    string fullPath = Path.Combine(Path.Combine("Assets", themeSettingsPath),
                                                   themeSettingsAssetName + themeSettingsAssetExtension
                                                  );
                    AssetDatabase.CreateAsset(instance, fullPath);
#endif
                }
            }
            return instance;
        }
    }

    public float VolumeOfBGM {
        get { return clipsForBGM[0].GetPlayer().volume; }
        set { 
            for (int i = 0; i < clipsForBGM.Count; i++) {
                clipsForBGM[i].GetPlayer().volume = value;
            }
        }
    }

#if UNITY_EDITOR
	[MenuItem("NOVNINE/Sound Settings")]
    public static void Edit () {
        Selection.activeObject = Instance;
    }
#endif

    void OnEnable () {
        DontDestroyOnLoad(this);
    }

	public void Reset()
	{
		soundParent = null;
		
		clipDic.Clear();
		clipsForBGM.Clear();
		Initialize();
	}
	
    public void Initialize()
	{
        if (audioClips == null) return;

        if (soundParent == null)
        {
            soundParent = new GameObject("+NNSound").transform;
            DontDestroyOnLoad(soundParent.gameObject);
        }
        
        foreach (AudioClip audioClip in audioClips) 
		{
            if (audioClip != null) 
                clipDic.Add(audioClip.name, new ClipInfo(audioClip));
        }
    }

    public void SetVolumeSFX (float volumeSFX) {
        foreach(var clp in clipDic.Values) {
            if(clipsForBGM.IndexOf(clp) == -1) {
                clp.SetVolume(volumeSFX);
            }
        }
    }

    public void SetVolumeBGM (float volumeBGM) {
        for (int i = 0; i < clipsForBGM.Count; i++) {
            if (clipsForBGM[i] == null) continue;
            clipsForBGM[i].GetPlayer().volume = volumeBGM;
        }
    }

    public void PlayBGM (string clipName, float volume = 1F, int channel = 0, bool loop = true, bool useFade = true) {
//		if (NOVNINE.NativeInterface.IsMusicPlaying()) return;
		// iOS 작업
//#if UNITY_IOS
//		if(Application.platform == RuntimePlatform.IPhonePlayer) {
//			return _IsMusicPlaying();
//		} else {
//			return false;
//		}
//#else
//		
//#endif
       
        while(clipsForBGM.Count <= channel) 
            clipsForBGM.Add(null);

        ClipInfo prevClip = clipsForBGM[channel];
        ClipInfo curClip = FindClipInfoByName(clipName);
        if(curClip == null) {
            Debugger.LogError("NNSound.PlayBGM fail : clip not found "+clipName);
            return;
        }
        clipsForBGM[channel] = curClip;
	
        //HOTween.Complete("BGM_Fade_" + channel);
		DOTween.Complete("BGM_Fade_" + channel);

        if (useFade) {
			Sequence seq = DOTween.Sequence();
			seq.id = "BGM_Fade_" + channel;
            //Sequence seq = new Sequence(new SequenceParms().Id("BGM_Fade_"+channel));

            if (prevClip != null && prevClip.IsPlaying) 
			{
                //seq.Append(HOTween.To(prevClip.GetPlayer(), 0.5F, "volume", 0F));
				seq.Append(prevClip.GetPlayer().DOFade( 0.5F, 0F));
                seq.AppendInterval(0.3F);
            } 
			
            seq.AppendCallback(() => { curClip.Play(volume, loop); });
            //seq.Append(HOTween.To(curClip.GetPlayer(), 0.5F, "volume", volume));
			seq.Append(curClip.GetPlayer().DOFade(0.5F, volume));

            if (prevClip != null && prevClip.IsPlaying) {
                seq.AppendCallback(() => { prevClip.Stop(); });
            }

            seq.Play();
        } else {
            if(prevClip != null)
                prevClip.Stop();
            curClip.Play(volume, loop);
        }
    }

    public void StopBGM (int channel = 0, bool useFade = true, float fadeTime = 0.5f) {
        if(clipsForBGM.Count <= channel) return;

        ClipInfo clip = clipsForBGM[channel];
        Debugger.Assert(clip != null);
        if (clip.IsPlaying == false) return;

        //HOTween.Complete("BGM_Fade_" + channel);
		DOTween.Complete("BGM_Fade_" + channel);

        if (useFade) {
            //Sequence seq = new Sequence(new SequenceParms().Id("BGM_Fade_" + channel));
			Sequence seq = DOTween.Sequence();
			seq.id = "BGM_Fade_" + channel;
            //seq.Append(HOTween.To(clip.GetPlayer(), fadeTime, "volume", 0F));
			seq.Append(clip.GetPlayer().DOFade( fadeTime, 0F));
            seq.AppendCallback(() => { clip.Stop(); });
            seq.Play();
        } else {
            clip.Stop();
        }
    }

    public void StopBGMAll (bool useFade = true) {
        for (int i = 0; i < clipsForBGM.Count; i++) {
            StopBGM(i, useFade);
        }
    }

    public void Play (string clipName, float volume = 1F, bool loop = false, float pitch = 1f) {
        var clip = FindClipInfoByName(clipName);
        if(clip == null) return;
        clip.Play(volume, loop, pitch);
    }

    public void Stop (string clipName) {
        var clip = FindClipInfoByName(clipName);
        if(clip == null) return;
        clip.Stop();
    }

    public void StopAll () {
        foreach(var e in clipDic)
            e.Value.Stop();
    }

    public bool IsPlayingBGM (int channel = 0)
    {
        if(0 == clipsForBGM.Count)
            return false;
        ClipInfo clip = clipsForBGM[channel];
        if(clip == null) return false;
        return clip.IsPlaying;
    }

    public bool IsPlaying (string clipName) {
        ClipInfo clip = FindClipInfoByName(clipName);
        if(clip == null) return false;
        return clip.IsPlaying;
    }

    public void SetMaxChannelForClip (string clipName, int maxChn) {
        ClipInfo clip = FindClipInfoByName(clipName);
        if(clip == null) return;
        clip.SetMaxChannel(maxChn);
    }

    public void SetPitch (string clipName, float amount, bool useFade = true) {
        ClipInfo clip = FindClipInfoByName(clipName);
        if(clip == null) return;

        //HOTween.Complete("SetPitch_" + clipName);
		DOTween.Complete("SetPitch_" + clipName);
		
        if (useFade) {
            //Sequence seq = new Sequence(new SequenceParms().Id("SetPitch_" + clipName));
			Sequence seq = DOTween.Sequence();
			seq.id = "SetPitch_" + clipName;
            //seq.Append(HOTween.To(clip.GetPlayer(), 0.5F, "pitch", amount));
			seq.Append(clip.GetPlayer().DOPitch( 0.5F, amount));
            seq.Play();
        } else {
            clip.SetPitch(amount);
        }
    }

    ClipInfo FindClipInfoByName(string clipName) {
        if(clipDic.ContainsKey(clipName))
            return clipDic[clipName];
		
        Debugger.Assert(false, "Can't found audio clip - "+clipName);
        return null;
    }

    //public int getClipDicCount()
    //{
    //    return clipDic.Count;
    //}

}
