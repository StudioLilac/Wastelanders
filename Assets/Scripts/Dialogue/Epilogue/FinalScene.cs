using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("You killed her")] 
        [SerializeField] private Image youKilledHerBg;
        [SerializeField] private TextMeshProUGUI youKilledHerText;
        
        private void Start() {
            int n = narrations.Count;
            UIFadeScreenManager.Instance.SetDarkScreen();
            captionTextMesh.alpha = 0;
            jackieTextMesh.alpha = 0;
            ivesTextMesh.alpha = 0;
            whiteOverlay.color = new Color(0, 0, 0, 0);
            mainCamera = Camera.main;
            
            youKilledHerBg.gameObject.SetActive(false);
            
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
                    StartCoroutine(MoveCamera(new Vector2(0, 2), 3f));
                    StartCoroutine(ZoomCamera(10, 2f));
                    StartCoroutine(PlayYouKilledHerSequence());
                    break;
                default:
                    break;
            }
        }

        private IEnumerator PlayYouKilledHerSequence() {
            yield return new WaitForSeconds(1.5f);
            
            youKilledHerBg.gameObject.SetActive(true);
            yield return StartCoroutine(FlashText(youKilledHerText, 1f));
            youKilledHerText.text = "YOU DID THIS TO HER";
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FlashText(youKilledHerText, 1f));
            youKilledHerText.text = "YOU KILLED HER";
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FlashText(youKilledHerText, 1f));
            
            
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
            float interval = 1f / flashesPerSecond;
            bool visible = false;

            while (elapsed < flashDuration)
            {
                visible = !visible;
                textMesh.alpha = visible ? 1f : 0f;

                yield return new WaitForSecondsRealtime(interval);
                elapsed += interval;
            }

            // Force invisible at the end
            textMesh.alpha = 0f;
        }

    }
}