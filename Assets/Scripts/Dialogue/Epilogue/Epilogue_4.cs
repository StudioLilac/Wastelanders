using System;
using System.Collections;
using DialogueScripts;
using UnityEngine;

namespace Dialogue.Epilogue
{
    public class Epilogue_4 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue0;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue4;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;

        private IEnumerator Start()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            yield return DialogueBoxV2.Instance.Play(postDialogue0.Into());
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue1.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
            yield return DialogueBoxV2.Instance.Play(postDialogue1.Into());
            background1.SetActive(false);
            background2.SetActive(true);
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue2.Into());
            yield return new WaitForSeconds(3f);
            yield return DialogueBoxV2.Instance.Play(dialogue3.Into());
            yield return DialogueBoxV2.Instance.Play(postDialogue3.Into());
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue4.Into());
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }
    }
}