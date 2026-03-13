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

        [SerializeField] private TypewriterHelper deception;
        [SerializeField] private TypewriterHelper worthless;
        [SerializeField] private TypewriterHelper pathetic;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject vignette;

        [SerializeField] private UIFadeHandler scrimAlpha;
        [SerializeField] private UIFadeHandler vignetteAlpha;
        [SerializeField] private ScalingLerpHandler vignetteScale;

        [SerializeField] private AudioClip backgroundBeeping;
        [SerializeField] private AudioClip labAmbiance;

        private void Awake()
        {
            this.Subscribe<CustomEvent>(IvesInternalTextDisplay);
            scrimAlpha.SetDarkScreen();
        }

        private IEnumerator Start()
        {
            yield return DialogueBoxV2.Instance.Play(postDialogue0.Into());
            yield return scrimAlpha.FadeInLightScreen(2f);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue1.Into());
            yield return new WaitForSeconds(1f);
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return scrimAlpha.FadeInDarkScreen(1f);
            background1.SetActive(false);
            background2.SetActive(true);
            vignette.SetActive(true);
            deception.gameObject.SetActive(false);
            worthless.gameObject.SetActive(false);
            pathetic.gameObject.SetActive(false);
            AudioManager.Instance.PlayBackgroundMusic(backgroundBeeping, true);

            yield return new WaitForSeconds(1f);
            yield return scrimAlpha.FadeInLightScreen(1f);
            var breathingCoroutine = StartCoroutine(PlayWakeUpSequence());
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue2.Into());
            StopCoroutine(breathingCoroutine);
            StartCoroutine(vignetteScale.FadeToAlpha(0f, 1f));
            yield return vignetteAlpha.FadeToAlpha(0.5f, 1f);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(dialogue3.Into());
            yield return _panicCoroutine; //TODO: Play louder heartbeat sfx here. 
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(wasteInfectionSymptoms);


            yield return DialogueBoxV2.Instance.Play(postDialogue3.Into());
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return new WaitForSeconds(2f);
            AudioManager.Instance.PlayBackgroundMusic(labAmbiance, true);
            yield return DialogueBoxV2.Instance.Play(dialogue4.Into());
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);

        }

        private Coroutine _panicCoroutine;
        private Coroutine _infiniteBreath;
        void IvesInternalTextDisplay(CustomEvent e)
        {
            if (e.EventName == "deception") deception.Play();
            else if (e.EventName == "worthless") worthless.Play();
            else if (e.EventName == "pathetic") pathetic.Play();
            else if (e.EventName == "panic") _panicCoroutine = StartCoroutine(PanicSequence()); // I'm afraid the infection is permanent line. 
            else if (e.EventName == "resolve") Resolve();
        }


        void Resolve()
        {
            StopCoroutine(_panicCoroutine);
            StopCoroutine(_infiniteBreath);
            StartCoroutine(vignetteScale.FadeToAlpha(0f, 5f));
            StartCoroutine(vignetteAlpha.FadeToAlpha(0.5f, 5f));

            deception.eraseCharsPerSecond = 30f;
            worthless.eraseCharsPerSecond = 30f;
            pathetic.eraseCharsPerSecond = 30f;

            deception.Erase();
            worthless.Erase();
            pathetic.Erase();
        }

        IEnumerator PanicSequence()
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(vignetteScale.FadeToAlpha(.15f, 1f));
            yield return StartCoroutine(vignetteAlpha.FadeToAlpha(1f, 1f));


            deception.gameObject.SetActive(true);
            worthless.gameObject.SetActive(true);
            pathetic.gameObject.SetActive(true);

            deception.Play();
            yield return new WaitForSeconds(.2f);
            worthless.Play();
            yield return new WaitForSeconds(.2f);
            pathetic.Play();

            _infiniteBreath = StartCoroutine(InfiniteBreath());
        }

        IEnumerator InfiniteBreath()
        {
            float inhaleDuration = 0.8f;
            float exhaleDuration = 1.2f;
            while (true)
            {
                yield return vignetteScale.FadeToAlpha(0.05f, inhaleDuration);
                yield return new WaitForSeconds(0.2f);
                yield return vignetteScale.FadeToAlpha(0.15f, exhaleDuration);
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