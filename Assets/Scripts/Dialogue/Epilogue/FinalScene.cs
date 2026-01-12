using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Particles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    enum CaptionType {
        Narration,
        Jackie,
        Ives
    }
    
    public class FinalScene : MonoBehaviour {
        [Serializable]
        private class CaptionNarration {
            [SerializeField] [TextArea(1, 5)] public string content;
            
            // leave this as 0 to use the default timing.
            [SerializeField] public int manualDuration = 0;
            
            [SerializeField] public CaptionType speaker = CaptionType.Narration;
            
            // use this to trigger events
            [SerializeField] public string signal;
        }

        private Camera mainCamera;
        [SerializeField] private float timeScale = 5f;
        
        [SerializeField] private List<CaptionNarration> narrations;
        
        [SerializeField] private Image whiteOverlay;
        [SerializeField] private POVBlizzard povBlizzard;
        [SerializeField] private FogVolume2D fogVolume2D;
        [SerializeField] private ParticleSystem clouds;
        
        [SerializeField] private TextMeshProUGUI captionTextMesh;
        [SerializeField] private TextMeshProUGUI jackieTextMesh;
        [SerializeField] private TextMeshProUGUI ivesTextMesh;
        
        [Header ("Sound")]
        [SerializeField] public EventReference blizzardOneShot;

        [Header("You killed her")] 
        [SerializeField] private float jitterAmount;
        [SerializeField] private GameObject youKilledHerObj;
        [SerializeField] private TextMeshProUGUI itsYourFaultText;
        [SerializeField] private TextMeshProUGUI youDidThisToHerText;
        [SerializeField] private TextMeshProUGUI youKilledHerText;
        
        [Header("Flashback")]
        [SerializeField] private Image camFlashback;
        [SerializeField] private Image jayFlashback;
        
        private EventInstance blizzardInstance;
        
        private void Start() {
            int n = narrations.Count;
            UIFadeScreenManager.Instance.SetDarkScreen();
            captionTextMesh.alpha = 0;
            jackieTextMesh.alpha = 0;
            ivesTextMesh.alpha = 0;
            whiteOverlay.color = new Color(0, 0, 0, 0);
            mainCamera = Camera.main;

            itsYourFaultText.alpha = 0;
            youDidThisToHerText.alpha = 0;
            youKilledHerText.alpha = 0;
            
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

        private IEnumerator PlayScene() {
            // we'll fade in from black from the previous scene (boss fight). However, we will load this scene
            // and wait x amount of time, before fading in. The x amount of time will be determined by FMOD; we
            // want this scene to sync with music meaning that the fade in should only happen once the music is 
            // at the right point.
            // TODO: make this wait until fmod flips to the ending track
            yield return new WaitForSeconds(0.5f);
            // now the scene starts, with the music synced up
            
            
            StartCoroutine(MoveCamera(new Vector2(0, -2), 3f));
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(2f));
            
            
            foreach (CaptionNarration narration in narrations) {
                TextMeshProUGUI activeText = GetSpeakerTextMesh(narration.speaker);
                activeText.text = narration.content;

                yield return StartCoroutine(FadeText(activeText, 1f, 1f));
                
                HandleSignal(narration.signal);

                if (narration.manualDuration != 0) {
                    yield return new WaitForSeconds(narration.manualDuration);
                } else {
                    int wordCount = narration.content
                        .Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries)
                        .Length;
                    yield return new WaitForSeconds(wordCount / timeScale);
                }
                
                yield return StartCoroutine(FadeText(activeText, 0f, 0.5f));
            }
        }

        private void HandleSignal(string signal) {
            switch (signal) {
                case "ivescrashes":
                    StartCoroutine(MoveCamera(new Vector2(0, -1), 2f));
                    StartCoroutine(ZoomCamera(-10, 2f));
                    break;
                case "jackiesorry":
                    StartCoroutine(MoveCamera(new Vector2(0, 2), 9f));
                    StartCoroutine(ZoomCamera(10, 6f));
                    StartCoroutine(PlayYouKilledHerSequence());
                    break;
                default:
                    break;
            }
        }

        private IEnumerator PlayYouKilledHerSequence() {
            yield return new WaitForSeconds(1.5f);
            
            StartCoroutine(FloatThoughtText(itsYourFaultText, 4f));
            yield return new WaitForSeconds(2f);
            StartCoroutine(FloatThoughtText(youDidThisToHerText, 2f));
            yield return new WaitForSeconds(3f);
            
            yield return StartCoroutine(FloatThoughtText(youKilledHerText, 3f));
            youKilledHerObj.SetActive(false);
            
            StartCoroutine(FadeFMODVolume(blizzardInstance, 1f, 0f, 2f));
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(2f));
            
            camFlashback.color = new Color(1, 1, 1, 1);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1f));
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(2f));
            
            camFlashback.color = new Color(0,0,0,0);
            jayFlashback.color = new Color(1, 1, 1, 1);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1f));
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(2f));
            jayFlashback.color = new Color(0,0,0,0);
            
            // quickly, now back to the real world.
            StartCoroutine(FadeFMODVolume(blizzardInstance, 0f, 1f, 0.5f));
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(0.5f));
        }
        
        private IEnumerator FadeText(TextMeshProUGUI textMesh, float targetAlpha, float duration)
        {
            if (textMesh == null)
                yield break;

            float startAlpha = textMesh.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                textMesh.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            textMesh.alpha = targetAlpha;
        }


        private IEnumerator MoveCamera(Vector2 deltaPosition, float duration)
        {
            Vector3 startPos = mainCamera.transform.position;
            Vector3 targetPos = startPos + new Vector3(deltaPosition.x, deltaPosition.y, 0f);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float easedT = t * t * (3f - 2f * t);

                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
                yield return null;
            }

            mainCamera.transform.position = targetPos;
        }
        
        private IEnumerator ZoomCamera(float deltaFov, float duration) {
            float startPos = mainCamera.fieldOfView;
            float targetPos = startPos + deltaFov;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float easedT = t * t * (3f - 2f * t);

                mainCamera.fieldOfView = Mathf.Lerp(startPos, targetPos, easedT);
                yield return null;
            }

            mainCamera.fieldOfView = targetPos;
        }
        
        private TextMeshProUGUI GetSpeakerTextMesh(CaptionType speaker) {
            switch (speaker) {
                case CaptionType.Jackie:
                    return jackieTextMesh;
                case CaptionType.Ives:
                    return ivesTextMesh;
                case CaptionType.Narration:
                default:
                    return captionTextMesh;
            }
        }

        private IEnumerator FlashText(
            TextMeshProUGUI textMesh,
            float flashDuration,
            float flashesPerSecond = 12f
        )
        {
            if (textMesh == null)
                yield break;

            float elapsed = 0f;
            bool visible = false;
            
            float baseInterval = 1f / flashesPerSecond;
            float jitter = baseInterval * 0.4f; 
            
            // also make the text move around slightly
            Vector3 basePos = textMesh.rectTransform.anchoredPosition;
            
            while (elapsed < flashDuration)
            {
                visible = !visible;
                float targetAlpha = visible
                    ? UnityEngine.Random.Range(0.85f, 1f)
                    : UnityEngine.Random.Range(0f, 0.15f);

                textMesh.alpha = targetAlpha;

                textMesh.rectTransform.anchoredPosition =
                    (Vector2)basePos + UnityEngine.Random.insideUnitCircle * jitterAmount;

                float actualInterval = baseInterval + UnityEngine.Random.Range(-jitter, jitter);
                actualInterval = Mathf.Max(0.02f, actualInterval);

                yield return new WaitForSecondsRealtime(actualInterval);
                elapsed += actualInterval;
            }

            textMesh.alpha = 0f;
        }
        
        private IEnumerator FloatThoughtText(
            TextMeshProUGUI textMesh,
            float duration,
            float minAlpha = 0.5f,
            float maxAlpha = 0.9f,
            float orbitRadius = 30f,
            float orbitSpeed = 0.25f,
            float bobAmplitude = 6f,
            float bobSpeed = 0.6f,
            float fadeOutDuration = 0.8f
        )
        {
            if (textMesh == null)
                yield break;

            RectTransform rect = textMesh.rectTransform;
            Vector2 basePos = rect.anchoredPosition;

            float elapsed = 0f;
            textMesh.alpha = 0f;

            float orbitPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float bobPhase   = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            float floatDuration = Mathf.Max(0f, duration - fadeOutDuration);

            // ---------- Phase A: normal floating ----------
            while (elapsed < floatDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                UpdateFloatingMotion(
                    textMesh,
                    rect,
                    basePos,
                    elapsed,
                    minAlpha,
                    maxAlpha,
                    orbitRadius,
                    orbitSpeed,
                    bobAmplitude,
                    bobSpeed,
                    orbitPhase,
                    bobPhase,
                    alphaMultiplier: 1f
                );

                yield return null;
            }

            // ---------- Phase B: fade out while floating ----------
            float fadeElapsed = 0f;

            while (fadeElapsed < fadeOutDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float fadeT = Mathf.Clamp01(fadeElapsed / fadeOutDuration);

                UpdateFloatingMotion(
                    textMesh,
                    rect,
                    basePos,
                    elapsed + fadeElapsed,
                    minAlpha,
                    maxAlpha,
                    orbitRadius,
                    orbitSpeed,
                    bobAmplitude,
                    bobSpeed,
                    orbitPhase,
                    bobPhase,
                    alphaMultiplier: 1f - fadeT
                );

                yield return null;
            }

            // Final cleanup
            textMesh.alpha = 0f;
            rect.anchoredPosition = basePos;
        }

        private void UpdateFloatingMotion(
            TextMeshProUGUI textMesh,
            RectTransform rect,
            Vector2 basePos,
            float time,
            float minAlpha,
            float maxAlpha,
            float orbitRadius,
            float orbitSpeed,
            float bobAmplitude,
            float bobSpeed,
            float orbitPhase,
            float bobPhase,
            float alphaMultiplier
        )
        {
            // Alpha breathing
            float alphaT =
                (Mathf.Sin(time * 1.3f + orbitPhase) + 1f) * 0.5f;

            float baseAlpha = Mathf.Lerp(minAlpha, maxAlpha, alphaT);
            textMesh.alpha = baseAlpha * alphaMultiplier;

            // Orbital motion
            float orbitAngle = time * orbitSpeed + orbitPhase;

            Vector2 orbitOffset = new Vector2(
                Mathf.Cos(orbitAngle),
                Mathf.Sin(orbitAngle * 0.9f)
            ) * orbitRadius;

            // Vertical bob
            float bobOffset =
                Mathf.Sin(time * bobSpeed + bobPhase) * bobAmplitude;

            rect.anchoredPosition =
                basePos + orbitOffset + Vector2.up * bobOffset;
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

    }
}