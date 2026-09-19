using DialogueScripts;
using LevelSelectInformation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue
{
    public class Epilogue_4 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue0;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue1; // Authored in Code. 
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue3;
        [SerializeField] private DialogueEntryWrapper wasteInfectionSymptoms;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue4; // Authored in Code. 

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

            yield return DialogueBoxV2.Instance.Play(Epilogue4.AilinIvesDiscussion.PartA);
            yield return new WaitForSeconds(1f);
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return scrimAlpha.FadeInDarkScreen(1f);
            background1.SetActive(false);
            background2.SetActive(true);
            vignette.SetActive(true);
            deception.gameObject.SetActive(false);
            worthless.gameObject.SetActive(false);
            pathetic.gameObject.SetActive(false);
            AudioManager.Instance.FadeInBackgroundTrack(2f, backgroundBeeping, true);

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
            yield return DialogueBoxV2.Instance.Play(Epilogue4.IvesCamDiscussion.PartA);

            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return new WaitForSeconds(2f);
            AudioManager.Instance.FadeInBackgroundTrack(2f, labAmbiance, true);
            yield return new WaitForSeconds(1f); // [Wait 1 sec]

            yield return DialogueBoxV2.Instance.Play(Epilogue4.IvesCamDiscussion.PartB);

            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f); // [Fade to black, 2 secs]

            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }

        private Coroutine _panicCoroutine;
        private Coroutine _infiniteBreath;
        public const string WEAK = "weak";
        public const string WORTHLESS = "worthless";
        public const string PATHETIC = "pathetic";
        public const string RESOLVE = "resolve";
        void IvesInternalTextDisplay(CustomEvent e)
        {
            if (e.EventName == WEAK) deception.Play();
            else if (e.EventName == WORTHLESS) worthless.Play();
            else if (e.EventName == PATHETIC) pathetic.Play();
            else if (e.EventName == "panic") _panicCoroutine = StartCoroutine(PanicSequence()); // I'm afraid the infection is permanent line. 
            else if (e.EventName == "unpanic") UnPanic(); // I intend to keep it.
            else if (e.EventName == RESOLVE) Resolve(); // Ailin says, that makes Ives her friend.
        }

        void UnPanic()
        {
            StartCoroutine(vignetteScale.FadeToAlpha(0f, 5f));
            StartCoroutine(vignetteAlpha.FadeToAlpha(0.5f, 5f));
        }

        void Resolve()
        {
            if (_panicCoroutine != null) StopCoroutine(_panicCoroutine);
            if (_infiniteBreath != null) StopCoroutine(_infiniteBreath);

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