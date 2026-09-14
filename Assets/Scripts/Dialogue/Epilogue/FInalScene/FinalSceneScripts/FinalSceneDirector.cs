using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace Cinematics
{
    /// <summary>
    /// Replaces the JSON-driven FinalScene. The caption loop moves to
    /// <see cref="CaptionPerformer"/>; this class keeps the camera, audio and
    /// credits work and listens for cues.
    ///
    /// Behaviour changes worth knowing about:
    ///  - Cues now dispatch when a caption starts revealing, not one second later
    ///    after a fade completes. Camera moves keyed to a word are on the word.
    ///  - Cue names are validated at startup against the handled set, so a typo
    ///    logs an error instead of silently hitting default:.
    ///  - The unreachable flashback handlers are gone; add them back as cues if
    ///    the reworked ending needs them.
    /// </summary>
    public class FinalSceneDirector : MonoBehaviour
    {
        [SerializeField] private CaptionPerformer performer;

        [SerializeField] private Camera moonCamera;
        [SerializeField] private MoonGlowDriver moonGlow;
        [SerializeField] private GameObject effectsParent;
        [SerializeField] private UIFadeHandler uiFadeHandler;

        [Header("Sound")]
        [SerializeField] private EventReference blizzardOneShot;

        [Header("Tear shader")]
        [SerializeField] private TearFilm tearFilm;

        [Header("Credits")]
        [SerializeField] private Animator cinematicBarsAnimator;
        [SerializeField] private Animator creditsAnimator;

        private Camera mainCamera;
        private EventInstance blizzardInstance;
        private CinematicBeat[] script;

        private static readonly string[] HandledCues =
        {
            IVES_CRASHES, TIGHTEN_SHIRT, TEARS_BEGIN, TEARS_CLEAR, TEARS_END, MOON_ZOOM, END
        };
        public const string IVES_CRASHES = "ivescrashes";
        public const string TIGHTEN_SHIRT = "tightenshirt";
        public const string MOON_ZOOM = "moonzoom";
        public const string TEARS_BEGIN = "tearsbegin";
        public const string TEARS_CLEAR = "tearsclear";
        public const string TEARS_END = "tearsend";
        public const string END = "end";

        private void Start()
        {
            script = FinalSceneScript.Build();
            ValidateCues();

            UIFadeScreenManager.Instance.SetDarkScreen();
            mainCamera = Camera.main;
            moonCamera.enabled = false;
            uiFadeHandler.SetLightScreen();

            blizzardInstance = RuntimeManager.CreateInstance(blizzardOneShot);
            RuntimeManager.AttachInstanceToGameObject(blizzardInstance, gameObject, GetComponent<Rigidbody>());
            blizzardInstance.start();

            performer.OnCue += HandleCue;

            StartCoroutine(PlayScene());
        }

        private void OnDestroy()
        {
            if (performer != null) performer.OnCue -= HandleCue;
        }

        private void Update()
        {
            bool fastForward = Input.GetKey(KeyCode.Space)
                               || Input.GetKey(KeyCode.RightArrow)
                               || Input.GetKey(KeyCode.Mouse0);
            creditsAnimator.speed = fastForward ? CreditsManager.speedFast : CreditsManager.speedNormal;
        }

        private void ValidateCues()
        {
            foreach (CinematicBeat beat in script)
            {
                Check(beat.Cue, beat.Index);
                foreach (TimedAction action in PerformanceMarkup.Parse(beat.Raw).Actions)
                    if (action.Kind == ActionKind.Cue) Check(action.CueName, beat.Index);
            }

            void Check(string cue, int index)
            {
                if (string.IsNullOrEmpty(cue)) return;
                if (System.Array.IndexOf(HandledCues, cue) < 0)
                    Debug.LogError($"[FinalScene] beat {index} fires unhandled cue '{cue}'.");
            }
        }

        private IEnumerator PlayScene()
        {
            yield return new WaitForSeconds(1.5f);

            StartCoroutine(MoveCamera(mainCamera, new Vector2(0f, -2f), 3f));
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1.5f);

            yield return performer.Play(script);

            yield return new WaitForSeconds(0.5f);
            cinematicBarsAnimator.SetTrigger("RemoveBars");

            yield return new WaitForSeconds(1f);
            creditsAnimator.SetTrigger("RollCredits");

            yield return new WaitUntil(() =>
                creditsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Season1FinaleAnimation"));
            yield return new WaitUntil(() =>
                creditsAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);

            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName);
        }

        private void HandleCue(string cue)
        {
            switch (cue)
            {
                case IVES_CRASHES:
                    StartCoroutine(MoveCamera(mainCamera, new Vector2(0f, -1.5f), 3f));
                    StartCoroutine(ZoomCamera(mainCamera, -10f, 3f));
                    break;

                case TIGHTEN_SHIRT:
                    StartCoroutine(ToMoonScene());
                    break;

                case MOON_ZOOM:
                    StartCoroutine(MoveCamera(moonCamera, new Vector2(-0.85f, -1.35f), 130f));
                    StartCoroutine(ZoomCamera(moonCamera, 1.6f, 130f));
                    break;

                case TEARS_BEGIN: tearFilm.Begin(); break;
                case TEARS_CLEAR: tearFilm.Clear(); break;
                case TEARS_END: tearFilm.End(); break;

                case END:
                    StartCoroutine(FadeFMODVolume(blizzardInstance, 1f, 0f, 3f));
                    break;
            }
        }

        private IEnumerator ToMoonScene()
        {
            yield return uiFadeHandler.FadeInDarkScreen(1f);

            mainCamera.enabled = false;
            moonCamera.enabled = true;
            effectsParent.SetActive(false);

            yield return new WaitForSeconds(1.5f);
            StartCoroutine(FadeFMODVolume(blizzardInstance, 0f, 0.5f, 0.5f));
            yield return uiFadeHandler.FadeInLightScreen(1f);
            StartCoroutine(moonGlow.Ramp(1f, 2.5f));
        }


        private IEnumerator MoveCamera(Camera cam, Vector2 delta, float duration)
        {
            Vector3 start = cam.transform.position;
            Vector3 target = start + new Vector3(delta.x, delta.y, 0f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cam.transform.position = Vector3.Lerp(start, target, t * t * (3f - 2f * t));
                yield return null;
            }

            cam.transform.position = target;
        }

        private IEnumerator ZoomCamera(Camera cam, float delta, float duration)
        {
            float Get() => cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
            void Set(float v)
            {
                if (cam.orthographic) cam.orthographicSize = v;
                else cam.fieldOfView = v;
            }

            float start = Get();
            float target = start + delta;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Set(Mathf.Lerp(start, target, t * t * (3f - 2f * t)));
                yield return null;
            }

            Set(target);
        }

        private IEnumerator FadeFMODVolume(EventInstance instance, float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                instance.setVolume(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            instance.setVolume(to);
        }
    }
}
