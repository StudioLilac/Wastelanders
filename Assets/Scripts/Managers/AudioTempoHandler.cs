using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioTempoHandler : FadeHandlerBase
{
    private AudioSource source;
    private AudioSource Source => source != null ? source : source = GetComponent<AudioSource>();

    protected override float CurrentAlpha => Source.pitch;
    protected override void SetAlpha(float pitch) => Source.pitch = pitch;

    public void SlowTempo(float tempo, float duration) => StartFadeToAlpha(tempo, duration);
    public void RestoreTempo(float duration) => StartFadeToAlpha(1f, duration);
}
