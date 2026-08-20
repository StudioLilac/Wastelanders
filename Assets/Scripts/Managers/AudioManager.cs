using System.Collections;
using FMOD.Studio;
using FMODUnity;
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

[System.Serializable]
public class AudioPreferences
{
    [field: SerializeField] public float BackgroundMusicVolume { get; set; } = 0.7f;
    [field: SerializeField] public float SFXVolume { get; set; } = 0.7f;
    [field: SerializeField] public bool MusicMuted { get; set; } = false;
    [field: SerializeField] public bool SFXMuted { get; set; } = false;
}