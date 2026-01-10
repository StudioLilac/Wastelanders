using System;
using System.Collections;
using System.Collections.Generic;
using Particles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    public class FinalScene : MonoBehaviour {
        [Serializable]
        private class CaptionNarration {
            [SerializeField] [TextArea(1, 5)] public string content;
            
            // leave this as 0 to use the default timing.
            [SerializeField] public int manualDuration = 0;
        }
        
        [SerializeField] private List<CaptionNarration> narrations;
        [SerializeField] private TextMeshProUGUI captionTextMesh;
        [SerializeField] private Image whiteOverlay;
        [SerializeField] private POVBlizzard povBlizzard;
        [SerializeField] private FogVolume2D fogVolume2D;
        [SerializeField] private ParticleSystem clouds;

        private void Start() {
            int n = narrations.Count;
            UIFadeScreenManager.Instance.SetDarkScreen();
            captionTextMesh.alpha = 0;
            whiteOverlay.color = new Color(0, 0, 0, 0);
            
            StartCoroutine(PlayScene());
        }

        private IEnumerator PlayScene() {
            // we'll fade in from black from the previous scene (boss fight). However, we will load this scene
            // and wait x amount of time, before fading in. The x amount of time will be determined by FMOD; we
            // want this scene to sync with music meaning that the fade in should only happen once the music is 
            // at the right point.
            // TODO: make this wait until fmod flips to the ending track
            yield return new WaitForSeconds(0.5f);
            
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(2f));
            
            // now the scene starts, with the music synced up
            foreach (CaptionNarration narration in narrations) {
                captionTextMesh.text = narration.content;
                
                yield return StartCoroutine(FadeText(1f, 1f));

                if (narration.manualDuration != 0) {
                    yield return new WaitForSeconds(narration.manualDuration);
                } else {
                    int wordCount = narration.content
                        .Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries)
                        .Length;
                    yield return new WaitForSeconds(wordCount / 3f);
                }
                
                yield return StartCoroutine(FadeText(0f, 0.5f));
            }
        }
        
        private IEnumerator FadeText(float targetAlpha, float duration) {
            if (captionTextMesh == null)
                yield break;

            float startAlpha = captionTextMesh.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                captionTextMesh.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            captionTextMesh.alpha = targetAlpha;
        }
    }
}