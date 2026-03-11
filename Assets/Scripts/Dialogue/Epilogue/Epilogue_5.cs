using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    public class Epilogue_5 : MonoBehaviour {
        [SerializeField] private DialogueEntryInUnityEditor[] walkingDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] blackDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] labDialogue;

        [SerializeField] private Image labBg;
        [SerializeField] private SpriteFadeHandler blackFadeHandler;
        [SerializeField] private UIFadeHandler labFadeHandler;
        private IEnumerator Start()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            labBg.gameObject.SetActive(false);
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1f);
            StartCoroutine(blackFadeHandler.FadeToAlpha(0.7f, 1f));
            yield return DialogueBoxV2.Instance.Play(walkingDialogue.Into());
            yield return blackFadeHandler.FadeInDarkScreen(1f);
            yield return DialogueBoxV2.Instance.Play(blackDialogue.Into());
            labBg.gameObject.SetActive(true);
            yield return labFadeHandler.FadeInDarkScreen(2f);
            yield return DialogueBoxV2.Instance.Play(labDialogue.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1.5f);
        }
    }
}