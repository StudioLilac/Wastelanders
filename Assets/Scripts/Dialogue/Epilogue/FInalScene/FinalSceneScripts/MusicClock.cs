using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Makes the music the clock instead of the frame timer.
    ///
    /// Seeking, not fast forwarding, is what makes sync work reviewable: jump both
    /// the track and the script to the same timestamp and play at normal speed. You
    /// cannot judge whether a caption lands on a chord at six times speed, and FMOD
    /// will not time stretch cleanly anyway.
    ///
    /// Exposes both an absolute position and a per-frame delta so CaptionPerformer
    /// can anchor beats to musical time rather than accumulate frame error.
    /// </summary>
    public class MusicClock : MonoBehaviour
    {
        [SerializeField] private EventReference musicEvent;

        [Tooltip("Seconds into the track where the caption sequence begins. Beat 0 starts here.")]
        [SerializeField] private float captionsStartAt = 0f;

        [Tooltip("Jump here on play. Leave at 0 for a normal run.")]
        [SerializeField] private float seekOnStartSeconds = 0f;

        [Tooltip("Falls back to unscaled time if FMOD is unavailable, so the scene still runs without audio.")]
        [SerializeField] private bool allowFallback = true;

        [SerializeField] private bool logSeeks = true;

        private EventInstance instance;
        private bool hasInstance;
        private float lastPosition;
        private float fallbackTime;

        /// <summary>Seconds into the music track.</summary>
        public float TrackSeconds { get; private set; }

        /// <summary>Seconds since the caption sequence's zero point.</summary>
        public float CaptionSeconds => TrackSeconds - captionsStartAt;

        /// <summary>Advance since last frame, from the music where possible.</summary>
        public float Delta { get; private set; }

        public bool UsingMusic => hasInstance;

        private void Awake()
        {
            if (!musicEvent.IsNull)
            {
                instance = RuntimeManager.CreateInstance(musicEvent);
                hasInstance = instance.isValid();
            }

            if (!hasInstance && !allowFallback)
                Debug.LogError("[MusicClock] No FMOD event and fallback disabled.");
        }

        private void Start()
        {
            if (hasInstance)
            {
                instance.start();
                if (seekOnStartSeconds > 0f) Seek(seekOnStartSeconds);
            }
            else
            {
                fallbackTime = seekOnStartSeconds;
            }

            Sample();
        }

        private void Update() => Sample();

        private void Sample()
        {
            float previous = TrackSeconds;

            if (hasInstance && instance.getTimelinePosition(out int ms) == FMOD.RESULT.OK)
            {
                TrackSeconds = ms / 1000f;
            }
            else
            {
                fallbackTime += Time.unscaledDeltaTime;
                TrackSeconds = fallbackTime;
            }

            Delta = TrackSeconds - previous;

            // FMOD reports in timeline ticks, so a fast frame can see no movement at
            // all, and a loop region or transition can send position backwards. Clamp
            // rather than letting either stall or rewind the performance.
            if (Delta < 0f)
            {
                if (logSeeks) Debug.LogWarning(
                    $"[MusicClock] Timeline went backwards ({previous:F2} to {TrackSeconds:F2}). " +
                    "A loop region or transition in the event will break script sync.");
                Delta = Time.unscaledDeltaTime;
            }
            else if (Delta == 0f)
            {
                Delta = Time.unscaledDeltaTime;
            }

            lastPosition = TrackSeconds;
        }

        /// <summary>Jump the music to an absolute position in the track.</summary>
        public void Seek(float trackSeconds)
        {
            if (hasInstance)
            {
                instance.setTimelinePosition(Mathf.Max(0, (int)(trackSeconds * 1000f)));
            }
            else
            {
                fallbackTime = trackSeconds;
            }

            Sample();

            if (logSeeks)
                Debug.Log($"[MusicClock] Seek to {CinematicSchedule.Timecode(trackSeconds)}" +
                          $" (captions at {CinematicSchedule.Timecode(Mathf.Max(0f, CaptionSeconds))})");
        }

        /// <summary>Jump so that the caption sequence is at the given offset.</summary>
        public void SeekCaptions(float captionSeconds) => Seek(captionsStartAt + captionSeconds);

        public void Stop()
        {
            if (!hasInstance) return;
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            hasInstance = false;
        }

        private void OnDestroy() => Stop();
    }
}
