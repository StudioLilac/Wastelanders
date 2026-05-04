using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Particles;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    public class FinalScene : MonoBehaviour {
        [SerializeField] private TextAsset dialogueJson;

        [SerializeField] private Camera moonCamera;
        [SerializeField] private GameObject effectsParent;
        
        [SerializeField] private UIFadeHandler uiFadeHandler;
        
        [Serializable]
        private class CaptionNarration {
            [TextArea(1, 5)] public string content;
            
            // leave this as 0 to use the default timing.
            public int duration = 0;
            
            public String speaker = "Narration";
            
            // use this to trigger events
            public string signal;
        }

        [Serializable]
        private class CaptionNarrationList {
            public List<CaptionNarration> captions;
        }

        private Camera mainCamera;
        [SerializeField] private float timeScale = 5f;
        
        private List<CaptionNarration> narrations;
        
        [SerializeField] private POVBlizzard povBlizzard;
        [SerializeField] private FogVolume2D fogVolume2D;
        [SerializeField] private ParticleSystem clouds;
        
        [SerializeField] private TextMeshProUGUI captionTextMesh;
        
        [Header ("Sound")]
        [SerializeField] public EventReference blizzardOneShot;

        [Header("Flashback")]
        [SerializeField] private Image camFlashback;
        [SerializeField] private Image jayFlashback;

        [SerializeField] private Animator cinematicBarsAnimator;
        
        private EventInstance blizzardInstance;
        
        private void Start() {
            LoadDialogueFromJson();
            
            UIFadeScreenManager.Instance.SetDarkScreen();
            captionTextMesh.alpha = 0;
            mainCamera = Camera.main;
            moonCamera.enabled = false;
            
            uiFadeHandler.SetLightScreen();
            
            camFlashback.color = new Color(0, 0, 0, 0);
            jayFlashback.color = new Color(0, 0, 0, 0);
            
            blizzardInstance = RuntimeManager.CreateInstance(blizzardOneShot);
            RuntimeManager.AttachInstanceToGameObject(
                blizzardInstance,
                gameObject,
                GetComponent<Rigidbody>()
            );
            
            blizzardInstance.start();
            
            StartCoroutine(PlayScene());
        }

        private void LoadDialogueFromJson()
        {
            narrations = JsonUtility.FromJson<CaptionNarrationList>(dialogueJson.text).captions;   
        }

        private IEnumerator PlayScene() {
            // we'll fade in from black from the previous scene (boss fight). However, we will load this scene
            // and wait x amount of time, before fading in. The x amount of time will be determined by FMOD; we
            // want this scene to sync with music meaning that the fade in should only happen once the music is 
            // at the right point.
            // TODO: make this wait until fmod flips to the ending track
            yield return new WaitForSeconds(0.5f);
            // now the scene starts, with the music synced up
            
            
            StartCoroutine(MoveCamera(mainCamera, new Vector2(0, -2), 3f));
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(2f));
            
            
            foreach (CaptionNarration narration in narrations) {
                captionTextMesh.text = (narration.speaker != "Narration" ? narration.speaker + ": " : "") + narration.content;
                
                HandleCaptionTextMeshColor(narration.speaker);

                yield return StartCoroutine(FadeText(captionTextMesh, 1f, 1f));
                
                HandleSignal(narration.signal);

                if (narration.duration != 0) {
                    yield return new WaitForSeconds(narration.duration);
                } else {
                    int wordCount = narration.content
                        .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                        .Length;
                    yield return new WaitForSeconds(wordCount / timeScale);
                }
                
                yield return StartCoroutine(FadeText(captionTextMesh, 0f, 0.5f));
            }
            
            // Narration is finished, remove the black bars
            yield return new WaitForSeconds(0.5f);
            cinematicBarsAnimator.SetTrigger("RemoveBars");
            
        }

        void HandleCaptionTextMeshColor(string speaker)
        {
            switch (speaker)
            {
                case "Narration": captionTextMesh.color = new Color(1, 1, 1, 1); break;
                case "Jackie": captionTextMesh.color = new Color(0f, 0.8f, 1, 1); break;
                case "Ives": captionTextMesh.color = new Color(1, 0.3f, 0.1f, 1); break;
                case "Cam": captionTextMesh.color = new Color(1, 0.8f, 0f, 1); break;
                case "Jay": captionTextMesh.color = new Color(0.7f, 0f, 0.9f, 1); break;
            }
        }

        private void HandleSignal(string signal) {
            switch (signal) {
                case "ivescrashes":
                    StartCoroutine(MoveCamera(mainCamera, new Vector2(0, -1.5f), 2f));
                    StartCoroutine(ZoomCamera(mainCamera, -10, 2f));
                    break;
                case "jackiesorry":
                    StartCoroutine(MoveCamera(mainCamera, new Vector2(0, 3.5f), 9f));
                    StartCoroutine(ZoomCamera(mainCamera, 10, 6f));
                    break;
                case "camfb":
                    StartCoroutine(PlayCamFlashbackSequence());
                    break;
                case "endcamfb":
                    StartCoroutine(EndCamFlashbackSequence());
                    break;
                case "jayfb":
                    StartCoroutine(PlayJayFlashbackSequence());
                    break;
                case "endjayfb":
                    StartCoroutine(EndJayFlashbackSequence());
                    break;
                case "apologystops":
                    StartCoroutine(MoveCamera(moonCamera, new Vector2(-0.85f, -1.35f), 60f));
                    StartCoroutine(ZoomCamera(moonCamera, 1.6f, 60f));
                    break;
                case "end":
                    StartCoroutine(FadeFMODVolume(blizzardInstance, 1f, 0f, 3f));
                    break;
                default:
                    break;
            }
        }

        private IEnumerator PlayCamFlashbackSequence() {
            StartCoroutine(FadeFMODVolume(blizzardInstance, 1f, 0f, 2f));
            yield return StartCoroutine(uiFadeHandler.FadeInDarkScreen(2f));
            
            camFlashback.color = new Color(1, 1, 1, 1);
            yield return StartCoroutine(uiFadeHandler.FadeInLightScreen(1f));
        }

        private IEnumerator EndCamFlashbackSequence()
        {
            yield return StartCoroutine(uiFadeHandler.FadeInDarkScreen(1f));
            camFlashback.color = new Color(0,0,0,0);
        }

        private IEnumerator PlayJayFlashbackSequence()
        {
            jayFlashback.color = new Color(1, 1, 1, 1);
            yield return StartCoroutine(uiFadeHandler.FadeInLightScreen(1f));
        }

        private IEnumerator EndJayFlashbackSequence()
        {
            yield return StartCoroutine(uiFadeHandler.FadeInDarkScreen(1f));
            jayFlashback.color = new Color(0,0,0,0);

            mainCamera.enabled = false;
            moonCamera.enabled = true;
            DisableSnowEffects();
            
            // moon scene time
            StartCoroutine(FadeFMODVolume(blizzardInstance, 0f, 0.5f, 0.5f));
            yield return StartCoroutine(uiFadeHandler.FadeInLightScreen(0.5f));
        }
        
        private IEnumerator FadeText(TextMeshProUGUI textMesh, float targetAlpha, float duration)
        {
            if (textMesh == null)
                yield break;

            float startAlpha = textMesh.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                textMesh.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            textMesh.alpha = targetAlpha;
        }


        private IEnumerator MoveCamera(Camera cam, Vector2 deltaPosition, float duration)
        {
            Vector3 startPos = cam.transform.position;
            Vector3 targetPos = startPos + new Vector3(deltaPosition.x, deltaPosition.y, 0f);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float easedT = t * t * (3f - 2f * t);

                cam.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
                yield return null;
            }

            cam.transform.position = targetPos;
        }
        
        private IEnumerator ZoomCamera(Camera cam, float deltaFov, float duration)
        {
            float startPos;
            if (cam.orthographic)
            {
                startPos = cam.orthographicSize;
            }
            else
            {
                startPos = cam.fieldOfView;
            }
            
            float targetPos = startPos + deltaFov;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float easedT = t * t * (3f - 2f * t);

                if (cam.orthographic)
                {
                    cam.orthographicSize = Mathf.Lerp(startPos, targetPos, easedT);
                }
                else
                {
                    cam.fieldOfView = Mathf.Lerp(startPos, targetPos, easedT);
                }
                yield return null;
            }

            cam.orthographicSize = targetPos;
            cam.fieldOfView = targetPos;
        }
        
        private IEnumerator FadeFMODVolume(
            EventInstance instance,
            float from,
            float to,
            float duration
        )
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                instance.setVolume(Mathf.Lerp(from, to, t));
                yield return null;
            }

            instance.setVolume(to);
        }

        public static IEnumerator FadeImageAlpha(
            Image image,
            float targetAlpha,
            float duration,
            AnimationCurve easing
        )
        {
            if (image == null)
                yield break;

            Color color = image.color;
            float startAlpha = color.a;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float eased = easing.Evaluate(t);
                color.a = Mathf.Lerp(startAlpha, targetAlpha, eased);
                image.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            image.color = color;
        }

        private void DisableSnowEffects()
        {
            effectsParent.SetActive(false);
        }

    }
}