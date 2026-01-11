using System;
using System.Collections;
using DialogueScripts;
using UnityEngine;

namespace Dialogue.Epilogue
{
    public class Epilogue_6 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue;

        private IEnumerator Start()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(dialogue.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }
    }
}