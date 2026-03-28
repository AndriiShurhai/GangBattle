using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Music")]
    [SerializeField] public List<Sound> musicTracks;
    [SerializeField] private float musicFadeDuration = 1f;

    [Header("Sound Effects")]
    [SerializeField] public List<Sound> soundEffects;
    [SerializeField] private int sfxPoolSize = 10;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private HashSet<AudioSource> activeSfxSources = new HashSet<AudioSource>();

    private AudioSource currentMusicSource;
    private AudioSource fadingMusicSource;
    private string currentMusicName;

    private GameObject audioSourcesContainer;

    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSfxVolumeChanged;
    public static event Action<bool> OnMusicMuteChanged;
    public static event Action<bool> OnSfxMuteChanged;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;
    public bool IsMusicMuted => isMusicMuted;
    public bool IsSfxMuted => isSfxMuted;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateAudioSourcesContainer();
        InitializeAudio();
        LoadVolumeSettings();
        PlayMusicOnAwake();
    }

    private void CreateAudioSourcesContainer()
    {
        audioSourcesContainer = new GameObject("AudioSources");
        audioSourcesContainer.transform.SetParent(transform);
    }

    private void InitializeAudio()
    {
        foreach (Sound music in musicTracks)
        {
            music.source = audioSourcesContainer.AddComponent<AudioSource>();
            music.source.clip = music.clip;
            music.source.volume = music.volume;
            music.source.pitch = music.pitch;
            music.source.loop = music.loop;
            music.source.playOnAwake = false; // We control playback manually
            music.source.outputAudioMixerGroup = musicMixerGroup;
        }

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfxSource = audioSourcesContainer.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            sfxSource.playOnAwake = false;
            sfxPool.Enqueue(sfxSource);
        }
    }

    private void PlayMusicOnAwake()
    {
        Sound playOnAwakeMusic = musicTracks.Find(m => m.playOnAwake);
        if (playOnAwakeMusic != null && !isMusicMuted)
        {
            PlayMusic(playOnAwakeMusic.name);
        }
    }

    // ─────────────────────────────────────────────
    //  Music Playback
    // ─────────────────────────────────────────────

    public void PlayMusic(string name)
    {
        Sound music = musicTracks.Find(s => s.name == name);
        if (music == null)
        {
            Debug.LogWarning($"[AudioManager] Music track '{name}' not found.");
            return;
        }

        if (currentMusicName == name && currentMusicSource != null && currentMusicSource.isPlaying)
        {
            if (enableDebugLogs) Debug.Log($"[AudioManager] '{name}' is already playing.");
            return;
        }

        StartCoroutine(PlayMusicWithFade(music));
    }

    private IEnumerator PlayMusicWithFade(Sound newMusic)
    {
        // Fade out old track
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            fadingMusicSource = currentMusicSource;
            yield return StartCoroutine(FadeAudioSource(fadingMusicSource, 0f, musicFadeDuration));
            fadingMusicSource.Stop();
            fadingMusicSource = null;
        }

        currentMusicSource = newMusic.source;
        currentMusicName = newMusic.name;

        currentMusicSource.volume = 0f;
        currentMusicSource.Play();
        yield return StartCoroutine(FadeAudioSource(currentMusicSource, GetEffectiveMusicVolume(newMusic), musicFadeDuration));
    }

    public void StopMusic()
    {
        if (currentMusicSource != null)
            StartCoroutine(StopMusicWithFade());
    }

    private IEnumerator StopMusicWithFade()
    {
        yield return StartCoroutine(FadeAudioSource(currentMusicSource, 0f, musicFadeDuration));
        currentMusicSource.Stop();
        currentMusicSource = null;
        currentMusicName = null;
    }

    public void PauseMusic()
    {
        currentMusicSource?.Pause();
    }

    public void ResumeMusic()
    {
        currentMusicSource?.UnPause();
    }

    public bool IsMusicPlaying() => currentMusicSource != null && currentMusicSource.isPlaying;

    public string GetCurrentMusicName() => currentMusicName;

    // ─────────────────────────────────────────────
    //  Crossfade
    // ─────────────────────────────────────────────

    public void CrossfadeMusic(string newMusicName, float crossfadeDuration = -1f)
    {
        if (crossfadeDuration < 0f)
            crossfadeDuration = musicFadeDuration;

        Sound newMusic = musicTracks.Find(s => s.name == newMusicName);
        if (newMusic == null)
        {
            Debug.LogWarning($"[AudioManager] Music track '{newMusicName}' not found for crossfade.");
            return;
        }

        StartCoroutine(CrossfadeMusicCoroutine(newMusic, crossfadeDuration));
    }

    private IEnumerator CrossfadeMusicCoroutine(Sound newMusic, float duration)
    {
        AudioSource newMusicSource = newMusic.source;
        AudioSource oldMusicSource = currentMusicSource;

        float startOldVolume = oldMusicSource != null ? oldMusicSource.volume : 0f;
        float targetNewVolume = GetEffectiveMusicVolume(newMusic);

        newMusicSource.volume = 0f;
        newMusicSource.Play();

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime; 
            float t = time / duration;

            if (oldMusicSource != null)
                oldMusicSource.volume = Mathf.Lerp(startOldVolume, 0f, t);

            newMusicSource.volume = Mathf.Lerp(0f, targetNewVolume, t);

            yield return null;
        }

        if (oldMusicSource != null)
        {
            oldMusicSource.Stop();
            oldMusicSource.volume = startOldVolume; // Restore for next use
        }

        newMusicSource.volume = targetNewVolume;
        currentMusicSource = newMusicSource;
        currentMusicName = newMusic.name;
    }

    // ─────────────────────────────────────────────
    //  SFX Playback
    // ─────────────────────────────────────────────

    public void PlaySFX(string name)
    {
        if (isSfxMuted) return;

        Sound sfx = soundEffects.Find(s => s.name == name);
        if (sfx == null)
        {
            Debug.LogWarning($"[AudioManager] Sound effect '{name}' not found.");
            return;
        }

        PlaySFX(sfx.clip, sfx.volume, sfx.pitch, sfx.loop);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        if (clip == null || isSfxMuted) return;

        AudioSource sfxSource = GetPooledSFXSource();
        if (sfxSource == null) return;

        sfxSource.clip = clip;
        sfxSource.volume = volume * sfxVolume * masterVolume;
        sfxSource.pitch = pitch;
        sfxSource.loop = loop; // FIX 2: Actually tell the AudioSource to loop!
        sfxSource.Play();

        if (enableDebugLogs) Debug.Log($"[AudioManager] SFX played: {clip.name}");

        activeSfxSources.Add(sfxSource);

        if (!loop)
        {
            StartCoroutine(ReturnSFXToPool(sfxSource, clip.length / Mathf.Max(pitch, 0.01f)));
        }
    }

    public void PlaySFXAtPosition(string name, Vector3 position)
    {
        if (isSfxMuted) return;

        Sound sfx = soundEffects.Find(s => s.name == name);
        if (sfx == null)
        {
            Debug.LogWarning($"[AudioManager] Sound effect '{name}' not found.");
            return;
        }

        PlaySFXAtPosition(sfx.clip, position, sfx.volume);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null || isSfxMuted) return;

        GameObject tempGO = new GameObject("TempSFX_" + clip.name);
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * sfxVolume * masterVolume;
        source.spatialBlend = 1f; // full 3D
        source.outputAudioMixerGroup = sfxMixerGroup; // 👈 the fix
        source.Play();

        Destroy(tempGO, clip.length);
    }

    // ─────────────────────────────────────────────
    //  Random SFX
    // ─────────────────────────────────────────────

    public void PlayRandomSFX(AudioClip[] sfxArray)
    {
        // FIX: null guard + skip null entries in array
        if (sfxArray == null || sfxArray.Length == 0 || isSfxMuted) return;

        AudioClip[] valid = Array.FindAll(sfxArray, c => c != null);
        if (valid.Length == 0) return;

        PlaySFX(valid[UnityEngine.Random.Range(0, valid.Length)]);
    }

    public void PlayRandomSFXAtPosition(AudioClip[] sfxArray, Vector3 position)
    {
        if (sfxArray == null || sfxArray.Length == 0 || isSfxMuted) return;

        AudioClip[] valid = Array.FindAll(sfxArray, c => c != null);
        if (valid.Length == 0) return;

        PlaySFXAtPosition(valid[UnityEngine.Random.Range(0, valid.Length)], position);
    }

    public void PlayRandomSFX(string[] sfxNames)
    {
        if (sfxNames == null || sfxNames.Length == 0 || isSfxMuted) return; // FIX: null guard

        PlaySFX(sfxNames[UnityEngine.Random.Range(0, sfxNames.Length)]);
    }

    public void PlayRandomSFX(List<string> sfxNames)
    {
        if (sfxNames == null || sfxNames.Count == 0 || isSfxMuted) return; // FIX: null guard

        PlaySFX(sfxNames[UnityEngine.Random.Range(0, sfxNames.Count)]);
    }

    // ─────────────────────────────────────────────
    //  SFX Utilities
    // ─────────────────────────────────────────────

    public void StopAllSFX()
    {
        // FIX: stop sources AND return them to pool, preventing the ReturnSFXToPool
        // coroutine from trying to double-enqueue them (Contains check handles that).
        foreach (AudioSource source in activeSfxSources)
        {
            source.Stop();
            sfxPool.Enqueue(source);
        }
        activeSfxSources.Clear();
    }

    public void StopSFX(string name)
    {
        // 1. Find the Sound object by your custom Inspector name
        Sound sfx = soundEffects.Find(s => s.name == name);

        if (sfx == null)
        {
            Debug.LogWarning($"[AudioManager] Could not stop SFX. '{name}' not found.");
            return;
        }

        // 2. Check if the active source is playing THIS specific clip
        foreach (AudioSource source in activeSfxSources)
        {
            if (source.isPlaying && source.clip == sfx.clip)
            {
                source.Stop();
                sfxPool.Enqueue(source);
                break;
            }
        }

        // 3. Clean up the active list
        activeSfxSources.RemoveWhere(s => !s.isPlaying);
    }

    public bool IsSFXPlaying(string name)
    {
        // Apply the exact same logic here so this function actually works too!
        Sound sfx = soundEffects.Find(s => s.name == name);
        if (sfx == null) return false;

        foreach (AudioSource source in activeSfxSources)
        {
            if (source.isPlaying && source.clip == sfx.clip)
                return true;
        }
        return false;
    }

    public int GetActiveSFXCount() => activeSfxSources.Count;
    public int GetAvailableSFXCount() => sfxPool.Count;

    // ─────────────────────────────────────────────
    //  SFX Pool
    // ─────────────────────────────────────────────

    private AudioSource GetPooledSFXSource()
    {
        if (sfxPool.Count > 0)
            return sfxPool.Dequeue();

        // Reclaim a finished source from active set
        AudioSource reclaimed = null;
        foreach (AudioSource source in activeSfxSources)
        {
            if (!source.isPlaying)
            {
                reclaimed = source;
                break;
            }
        }

        if (reclaimed != null)
        {
            activeSfxSources.Remove(reclaimed);
            return reclaimed;
        }

        Debug.LogWarning("[AudioManager] SFX pool exhausted! Consider increasing pool size.");
        return null;
    }

    private IEnumerator ReturnSFXToPool(AudioSource source, float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use realtime so it works when paused

        if (activeSfxSources.Contains(source))
        {
            activeSfxSources.Remove(source);
            sfxPool.Enqueue(source);
        }
    }

    // ─────────────────────────────────────────────
    //  Volume & Mute
    // ─────────────────────────────────────────────

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnMasterVolumeChanged?.Invoke(masterVolume);
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
        OnMusicVolumeChanged?.Invoke(musicVolume);
        SaveVolumeSettings();

        if (enableDebugLogs) Debug.Log($"[AudioManager] Music volume set to: {musicVolume}");
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        OnSfxVolumeChanged?.Invoke(sfxVolume);
        SaveVolumeSettings();
    }

    public void SetMusicMuted(bool muted)
    {
        isMusicMuted = muted;

        if (muted)
        {
            UpdateMusicVolume();
        }
        else
        {
            if (currentMusicSource == null)
            {
                Sound playOnAwakeMusic = musicTracks.Find(m => m.playOnAwake);
                if (playOnAwakeMusic != null)
                    PlayMusic(playOnAwakeMusic.name);
            }
            else
            {
                UpdateMusicVolume();
            }
        }

        OnMusicMuteChanged?.Invoke(isMusicMuted);
        SaveVolumeSettings();

        if (enableDebugLogs) Debug.Log($"[AudioManager] Music muted: {muted}");
    }

    public void SetSfxMuted(bool muted)
    {
        isSfxMuted = muted;
        if (muted) StopAllSFX();

        OnSfxMuteChanged?.Invoke(isSfxMuted);
        SaveVolumeSettings();

        if (enableDebugLogs) Debug.Log($"[AudioManager] SFX muted: {muted}");
    }

    public void SetMusicPitch(float pitch)
    {
        if (currentMusicSource != null)
            currentMusicSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    }

    public void ResetVolumeSettings()
    {
        SetMasterVolume(1f);
        SetMusicVolume(1f);
        SetSfxVolume(1f);
        SetMusicMuted(false);
        SetSfxMuted(false);
    }

    // ─────────────────────────────────────────────
    //  Internal Volume Helpers
    // ─────────────────────────────────────────────

    private float GetEffectiveMusicVolume(Sound music)
    {
        return isMusicMuted ? 0f : music.volume * musicVolume * masterVolume;

    }

    private void UpdateAllVolumes()
    {
        UpdateMusicVolume();
        // Active SFX volumes are intentionally NOT updated mid-play to avoid pops.
        // New SFX will automatically use the updated masterVolume.
    }

    private void UpdateMusicVolume()
    {
        if (currentMusicSource == null) return;

        Sound currentMusic = musicTracks.Find(s => s.source == currentMusicSource);
        if (currentMusic == null) return;

        float effectiveVolume = GetEffectiveMusicVolume(currentMusic);
        currentMusicSource.volume = effectiveVolume;

        if (enableDebugLogs)
            Debug.Log($"[AudioManager] Music volume updated to: {effectiveVolume}");
    }

    // ─────────────────────────────────────────────
    //  Persistence
    // ─────────────────────────────────────────────

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt("SfxMuted", isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1;

        UpdateAllVolumes();
    }

    // ─────────────────────────────────────────────
    //  Fade Utility
    // ─────────────────────────────────────────────

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            if (source == null) yield break;
            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        if (source != null)
            source.volume = targetVolume;
    }

    // ─────────────────────────────────────────────
    //  Cleanup
    // ─────────────────────────────────────────────

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}