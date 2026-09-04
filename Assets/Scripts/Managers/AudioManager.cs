using System.Collections;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using Managers;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : PersistentSingleton<AudioManager>
{
    [SerializeField] private AudioSource SFXSoundsPlayer, BackgroundMusicPlayer, BackgroundMusicIntroPlayer;
    [SerializeField] private SoundEffectsDatabase soundEffectsDatabase;
    [SerializeField] private AudioDatabase sceneAudioDatabase;
#nullable enable
    private SceneAudio sceneAudio = null!;
    Coroutine? combatMusicCoroutine;
    
    private Bus fmodMusicBus;
    private Bus fmodSFXBus;

    // Scene-scoped ambient sources with their own volume control, kept in sync with preferences.
    private readonly List<ControllableAudioChannel> channels = new();

    protected override void Awake()
    {
        base.Awake();
        if (invalid) return;
        
        fmodMusicBus = RuntimeManager.GetBus("bus:/Music");
        fmodSFXBus = RuntimeManager.GetBus("bus:/SFX");
        
        this.Subscribe<AudioPreferencesChanged>(_ => ApplyAudioPreferences());
    }

    private void Start() => ApplyAudioPreferences();

    private void ApplyAudioPreferences()
    {
        PreferencesManager prefs = PreferencesManager.Instance;
        SetMusicVolume(prefs.GetMusicVolume());
        SetSFXVolume(prefs.GetSFXVolume());
        SetMusicMuted(prefs.GetMusicMuted());
        SetSFXMuted(prefs.GetSFXMuted());
        foreach (ControllableAudioChannel channel in channels) channel.ApplyPreferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisposeAllChannels(); // ambient channels are scene-scoped, never outlive their scene
        SceneAudio incomingAudio = SceneData.FromSceneName(scene.name).GetAudio(sceneAudioDatabase);

        // This check allows us to have certain audio traacks persist across scenes 
        // E.x. MainMenu -> Level Select while also allowing audio to play on cold start
        if (sceneAudio == incomingAudio && incomingAudio.isPersisting) return;
        sceneAudio = incomingAudio;
        StartBackgroundTrack();
    }

    public void StartBackgroundTrack()
    {
        StartCoroutine(PlayStartAudio());
    }

    public void StartCombatMusic()
    {
        combatMusicCoroutine = StartCoroutine(BeginCombatMusic());
    }



    private IEnumerator BeginCombatMusic()
    {
        yield return StartCoroutine(FadeAudioRoutine(BackgroundMusicPlayer, true, 1f));

        if (sceneAudio.combatMusicIntro != null)
        {
            BackgroundMusicPlayer.clip = sceneAudio.combatMusicIntro;
            BackgroundMusicPlayer.Play();
            BackgroundMusicPlayer.loop = false;
            yield return new WaitUntil(() => !BackgroundMusicPlayer.isPlaying);
        }
        BackgroundMusicPlayer.clip = sceneAudio.combatMusicPrimary;
        BackgroundMusicPlayer.Play();
        BackgroundMusicPlayer.loop = true;
    }



    protected IEnumerator FadeAudioRoutine(AudioSource audioSource, bool isFadingOut, float fadeTime)
    {
        float startVolume = audioSource.volume;

        float targetMaxVolume = (audioSource == SFXSoundsPlayer)
            ? PreferencesManager.Instance.GetSFXVolume()
            : PreferencesManager.Instance.GetMusicVolume();

        float endVolume = isFadingOut ? 0f : targetMaxVolume;
        float time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / fadeTime);
            yield return null;
        }

        if (isFadingOut)
        {
            audioSource.Stop();
            audioSource.volume = targetMaxVolume;
        }
        else
        {
            audioSource.volume = endVolume;
        }
    }

    public void FadeOutCurrentBackgroundTrack(float duration)
    {
        if (combatMusicCoroutine != null)
        {
            StopCoroutine(combatMusicCoroutine);
            combatMusicCoroutine = null;
        }
        StartCoroutine(FadeAudioRoutine(BackgroundMusicPlayer, true, duration));
    }

    public void FadeInBackgroundTrack(float duration, AudioClip? audioclip, bool loop)
    {
        BackgroundMusicPlayer.volume = 0f;
        PlayBackgroundMusic(audioclip, loop);
        StartCoroutine(FadeAudioRoutine(BackgroundMusicPlayer, false, duration));
    }

    protected IEnumerator PlayStartAudio()
    {
        PlayBackgroundMusic(sceneAudio.backgroundMusicIntro, false);
        yield return new WaitUntil(() => !BackgroundMusicPlayer.isPlaying);
        PlayBackgroundMusic(sceneAudio.backgroundMusicPrimary, true);
    }

    protected void PlayBackgroundMusic(AudioClip? audioclip, bool loop)
    {
        BackgroundMusicPlayer.clip = audioclip;
        BackgroundMusicPlayer.Play();
        BackgroundMusicPlayer.loop = loop;
    }

    public void PlaySFX(SoundID effect)
    {
        RandomizePitch();
        SFXSoundsPlayer.PlayOneShot(soundEffectsDatabase.GetClipByID(effect));
    }

    // Spawns a standalone audio source the caller can play, fade, and dispose independently
    // of the global music/SFX players. Useful for scenes needing several ambient loops at once.
    public ControllableAudioChannel CreateChannel(SoundID clip, AudioCategory category, bool loop = true, float level = 1f)
        => CreateChannel(soundEffectsDatabase.GetClipByID(clip), category, loop, level);

    public ControllableAudioChannel CreateChannel(AudioClip? clip, AudioCategory category, bool loop = true, float level = 1f)
    {
        GameObject host = new GameObject(clip != null ? $"AudioChannel_{clip.name}" : "AudioChannel");
        host.transform.SetParent(transform);
        AudioSource source = host.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        AudioTempoHandler tempoHandler = host.AddComponent<AudioTempoHandler>();

        ControllableAudioChannel channel = new(source, tempoHandler, category, level, ReleaseChannel);
        channels.Add(channel);
        return channel;
    }

    private void ReleaseChannel(ControllableAudioChannel channel) => channels.Remove(channel);

    private void DisposeAllChannels()
    {
        foreach (ControllableAudioChannel channel in channels.ToArray()) channel.Dispose();
    }

    private void RandomizePitch()
    {
        SFXSoundsPlayer.pitch = Random.Range(0.90f, 1.1f);
    }

    public void StopMusic()
    {
        BackgroundMusicPlayer.Stop();
    }

    public void PlayDeath()
    {
        StartCoroutine(PlayDeathMusic());
    }

    IEnumerator PlayDeathMusic()
    {
        yield return StartCoroutine(FadeAudioRoutine(BackgroundMusicPlayer, true, 2f));
        BackgroundMusicPlayer.clip = sceneAudio.backgroundMusicDeath;
        BackgroundMusicPlayer.Play();
    }

    public void SetSFXVolume(float volume)
    {
        SFXSoundsPlayer.volume = volume;
        fmodSFXBus.setVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        BackgroundMusicPlayer.volume = volume;
        BackgroundMusicIntroPlayer.volume = volume;
        fmodMusicBus.setVolume(volume);
    }

    public void SetSFXMuted(bool state)
    {
        SFXSoundsPlayer.mute = state;
        fmodSFXBus.setMute(state);
    }

    public void SetMusicMuted(bool state)
    {
        BackgroundMusicPlayer.mute = BackgroundMusicIntroPlayer.mute = state;
        fmodMusicBus.setMute(state);
    }
}

public enum AudioCategory { SFX, Music }

// A portable, preference-aware audio source. The author sets a 'level' (0..1 mix), and the
// final output volume is that level scaled by the player's live volume preference for its
// category. Fades lerp the level while continuously re-reading preferences, so a volume or
// mute change mid-fade stays respected. Created and owned by AudioManager.CreateChannel.
public class ControllableAudioChannel
{
    private readonly AudioSource source;
    private readonly AudioTempoHandler tempoHandler;
    private readonly AudioCategory category;
    private readonly System.Action<ControllableAudioChannel> release;
    private float level;

    internal ControllableAudioChannel(AudioSource source, AudioTempoHandler tempoHandler, AudioCategory category, float level, System.Action<ControllableAudioChannel> release)
    {
        this.source = source;
        this.tempoHandler = tempoHandler;
        this.category = category;
        this.level = level;
        this.release = release;
        ApplyPreferences();
    }

    public void Play() => source.Play();
    public void Stop() => source.Stop();

    // Eases the loop's playback tempo (via pitch) down to the given fraction of normal speed,
    // e.g. SlowTempo(0.7f, 2f) winds the tracker's beeping down to 70% over two seconds.
    public void SlowTempo(float tempo, float duration) => tempoHandler.SlowTempo(tempo, duration);
    public void RestoreTempo(float duration) => tempoHandler.RestoreTempo(duration);

    public void SetLevel(float value)
    {
        level = value;
        ApplyPreferences();
    }

    public IEnumerator FadeTo(float targetLevel, float duration)
    {
        float startLevel = level;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            level = Mathf.Lerp(startLevel, targetLevel, time / duration);
            ApplyPreferences();
            yield return null;
        }
        SetLevel(targetLevel);
    }

    public void ApplyPreferences()
    {
        if (source == null) return;
        PreferencesManager prefs = PreferencesManager.Instance;
        bool isMusic = category == AudioCategory.Music;
        source.volume = level * (isMusic ? prefs.GetMusicVolume() : prefs.GetSFXVolume());
        source.mute = isMusic ? prefs.GetMusicMuted() : prefs.GetSFXMuted();
    }

    public void Dispose()
    {
        release?.Invoke(this);
        if (source != null) Object.Destroy(source.gameObject);
    }
}

[System.Serializable]
public class AudioPreferences
{
    [field: SerializeField] public float BackgroundMusicVolume { get; set; } = 0.7f;
    [field: SerializeField] public float SFXVolume { get; set; } = 0.7f;
    [field: SerializeField] public bool MusicMuted { get; set; } = false;
    [field: SerializeField] public bool SFXMuted { get; set; } = false;
}