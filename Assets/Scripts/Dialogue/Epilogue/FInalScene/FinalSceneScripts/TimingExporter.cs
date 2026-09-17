using System.IO;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Writes the computed caption timing to disk so it can be laid over the music
    /// in an audio or video editor. This is the fast iteration loop for sync work:
    /// drop the label track onto the waveform, see every caption against the music
    /// at once, and adjust lead and hold values without entering play mode.
    ///
    /// Right-click the component to export.
    /// </summary>
    public class TimingExporter : MonoBehaviour
    {
        [SerializeField] private CaptionPerformer performer;

        [Tooltip("Seconds into the music where beat 0 begins. Matches MusicClock's captionsStartAt.")]
        [SerializeField] private float captionsStartAt = 0f;

        [Tooltip("Written under the project folder, next to Assets.")]
        [SerializeField] private string fileName = "FinalSceneTiming";

        [ContextMenu("Export Timing")]
        public void Export()
        {
            if (performer == null)
            {
                Debug.LogError("[TimingExporter] No CaptionPerformer assigned.");
                return;
            }

            CinematicBeat[] beats = FinalSceneScript.Build();
            CinematicSchedule.Entry[] schedule = performer.BuildSchedule(beats);

            string root = Path.GetDirectoryName(Application.dataPath);
            string labels = Path.Combine(root, fileName + "_labels.txt");
            string csv = Path.Combine(root, fileName + ".csv");

            File.WriteAllText(labels, CinematicSchedule.ToLabelTrack(schedule, captionsStartAt));
            File.WriteAllText(csv, CinematicSchedule.ToCsv(schedule, captionsStartAt));

            float total = CinematicSchedule.TotalSeconds(schedule);

            Debug.Log(
                $"[TimingExporter] {beats.Length} beats, total {CinematicSchedule.Timecode(total)}.\n" +
                $"Labels: {labels}\nCSV: {csv}");

            WarnAboutLongCues(schedule, total);
        }

        [ContextMenu("Log Timing To Console")]
        public void LogTiming()
        {
            if (performer == null) return;

            CinematicBeat[] beats = FinalSceneScript.Build();
            CinematicSchedule.Entry[] schedule = performer.BuildSchedule(beats);

            var sb = new System.Text.StringBuilder();
            foreach (CinematicSchedule.Entry e in schedule)
            {
                string text = e.Text.Length > 52 ? e.Text.Substring(0, 52) + "..." : e.Text;
                sb.Append($"{e.Index,3}  {CinematicSchedule.Timecode(e.Start)}  " +
                          $"{e.Duration,5:F2}s  {e.Speaker,-9}  {text}\n");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// The moon camera move runs 130 seconds from its cue. If the script after
        /// that cue is shorter, the zoom is still travelling when the credits roll.
        /// </summary>
        private void WarnAboutLongCues(CinematicSchedule.Entry[] schedule, float total)
        {
            CinematicBeat[] beats = FinalSceneScript.Build();

            for (int i = 0; i < beats.Length; i++)
            {
                if (string.IsNullOrEmpty(beats[i].Cue)) continue;

                float remaining = total - schedule[i].Start;
                Debug.Log($"[TimingExporter] cue '{beats[i].Cue}' fires at " +
                          $"{CinematicSchedule.Timecode(schedule[i].Start)}, " +
                          $"{remaining:F1}s of script remaining.");
            }
        }
    }
}
