using System.Collections;
using DialogueScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue
{
    public class Epilogue_10 : MonoBehaviour
    {
        private static Color PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 1f);
        private static Color TRANSPARENT_PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 0f);

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;

        [SerializeField] private Image purpleFlash;
        [SerializeField] private Image tundraWithNoise;
        [SerializeField] private Image caveFlickerLayer;
        private bool shouldFlicker = false;

        [SerializeField] private DialogueEntryInUnityEditor[] jayOpeningDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] preBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postVisionDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] momStoryFlashbackDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] weiseDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] preBattleDialogue;


        private IEnumerator Start()
        {
            purpleFlash.color = Color.black;
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0f);
            yield return DialogueBoxV2.Instance.Play(jayOpeningDialogue.Into());
            UIFadeScreenManager.Instance.SetDarkScreen();
            purpleFlash.color = TRANSPARENT_PURPLE;

            yield return UIFadeScreenManager.Instance.FadeInLightScreen(1.5f);
            yield return DialogueBoxV2.Instance.Play(preBonfireDialogue.Into());

            yield return PurpleFlash();
            yield return DialogueBoxV2.Instance.Play(postVisionDialogue.Into());

            shouldFlicker = true;
            yield return DialogueBoxV2.Instance.Play(postBonfireDialogue.Into());

            background2.SetActive(true);
            yield return FadeInBG(tundraWithNoise);

            yield return DialogueBoxV2.Instance.Play(momStoryFlashbackDialogue.Into());
            yield return FadeOutBG(tundraWithNoise);
            background2.SetActive(false);

            yield return DialogueBoxV2.Instance.Play(weiseDialogue.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
            background1.SetActive(false);

            
        }

        private void Update()
        {
            if (!shouldFlicker) { return; }
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Mathf.Sin(Time.time));
        }

        private IEnumerator PurpleFlash()
        {
            float elapsedTime = 0f;
            float duration = 0.5f;
            // Lerps from transparent to alpha 1 in [0, duration/2] and the opposite in [duration/2, duration]
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float fraction = 1f - (2f / duration) * Mathf.Abs(elapsedTime - duration / 2f);
                fraction = Mathf.Clamp01(fraction);
                // Using Lerp is fine here as we calculate fraction every frame
                purpleFlash.color = Color.Lerp(TRANSPARENT_PURPLE, PURPLE, fraction);
                yield return null;
            }
            // Deals with floating point errors
            purpleFlash.color = TRANSPARENT_PURPLE;
        }

        private void DisableBonfireFlicker()
        {
            shouldFlicker = false;
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0f);
        }

        private IEnumerator FadeInBG(Image bgImage)
        {
            float fadeTime = 0.8f;
            for (var t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                bgImage.color = new Color(1f, 1f, 1f, t / fadeTime);
                yield return null;
            }
        }

        private IEnumerator FadeOutBG(Image bgImage)
        {
            float fadeTime = 0.8f;
            for (var t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                bgImage.color = new Color(1f, 1f, 1f, fadeTime - t / fadeTime);
                yield return null;
            }
        }
    }
}