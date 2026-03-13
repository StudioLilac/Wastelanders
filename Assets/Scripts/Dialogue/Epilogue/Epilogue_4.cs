using System;
using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue
{
    public class Epilogue_4 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue0;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue3;
        [SerializeField] private DialogueEntryWrapper wasteInfectionSymptoms;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue4;


        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject vignette;

        [SerializeField] private UIFadeHandler scrimAlpha;
        [SerializeField] private UIFadeHandler vignetteAlpha;
        [SerializeField] private ScalingLerpHandler vignetteScale;


        private IEnumerator Start()
        {
            scrimAlpha.SetDarkScreen();
            yield return DialogueBoxV2.Instance.Play(postDialogue0.Into());
            yield return scrimAlpha.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue1.Into());
            yield return new WaitForSeconds(1f);
            yield return scrimAlpha.FadeInDarkScreen(1f);
            background1.SetActive(false);
            background2.SetActive(true);
            vignette.SetActive(true);
            yield return new WaitForSeconds(1f);
            yield return scrimAlpha.FadeInLightScreen(1f);
            var breathingCoroutine = StartCoroutine(PlayWakeUpSequence());
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue2.Into());
            StopCoroutine(breathingCoroutine);
            StartCoroutine(vignetteScale.FadeToAlpha(0f, 1f));
            yield return vignetteAlpha.FadeToAlpha(0.5f, 1f);
            yield return new WaitForSeconds(1f);

            this.Subscribe<CustomEvent>(CustomEventHandler);
            void CustomEventHandler(CustomEvent e)
            {
                this.UnSubscribe<CustomEvent>(CustomEventHandler);
                breathingCoroutine = StartCoroutine(PanicSequence());
            }

            yield return DialogueBoxV2.Instance.Play(dialogue3.Into());
            yield return new WaitForSeconds(1.5f); //TODO: Play louder heartbeat sfx here. 


            yield return DialogueBoxV2.Instance.Play(postDialogue3.Into());
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue4.Into());
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);

        }

        
        IEnumerator PanicSequence()
        {

            StartCoroutine(vignetteScale.FadeToAlpha(.1f, 1f));
            yield return StartCoroutine(vignetteAlpha.FadeToAlpha(1f, 1f));


            float inhaleDuration = 2.0f;
            float exhaleDuration = 2.5f;
            while (true)
            {
                yield return vignetteScale.FadeToAlpha(0.1f, inhaleDuration);
                yield return new WaitForSeconds(0.2f);
                yield return vignetteScale.FadeToAlpha(0.2f, exhaleDuration);
                yield return new WaitForSeconds(0.4f);

            }
        }


        public IEnumerator PlayWakeUpSequence()
        {
            float inhaleDuration = 2.0f;
            float exhaleDuration = 2.5f;

            vignetteScale.SetDarkScreen();

            yield return vignetteScale.FadeToAlpha(0.8f, inhaleDuration);
            yield return new WaitForSeconds(0.2f);
            yield return vignetteScale.FadeToAlpha(0.9f, exhaleDuration);
            yield return new WaitForSeconds(0.4f);

            yield return vignetteScale.FadeToAlpha(0.65f, inhaleDuration);
            yield return new WaitForSeconds(0.2f);
            yield return vignetteScale.FadeToAlpha(0.75f, exhaleDuration);
            yield return new WaitForSeconds(0.4f);

            yield return vignetteScale.FadeToAlpha(0.5f, inhaleDuration);
            yield return new WaitForSeconds(0.2f);
            yield return vignetteScale.FadeToAlpha(0.6f, exhaleDuration);
            yield return new WaitForSeconds(0.4f);

            yield return vignetteScale.FadeToAlpha(0.35f, inhaleDuration);
            yield return new WaitForSeconds(0.2f);
            yield return vignetteScale.FadeToAlpha(0.45f, exhaleDuration);
            yield return new WaitForSeconds(0.4f);


            while (true)
            {
                yield return vignetteScale.FadeToAlpha(0.25f, inhaleDuration);
                yield return new WaitForSeconds(0.2f);
                yield return vignetteScale.FadeToAlpha(0.35f, exhaleDuration);
                yield return new WaitForSeconds(0.4f);

            }
        }

    }
}