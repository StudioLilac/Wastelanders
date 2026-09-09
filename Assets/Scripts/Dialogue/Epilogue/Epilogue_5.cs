using DialogueScripts;
using LevelSelectInformation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    public class Epilogue_5 : MonoBehaviour {
        [SerializeField] private DialogueEntryInUnityEditor[] walkingDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] blackDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] labDialogue;

        [SerializeField] private GameObject labBg;
        [SerializeField] private SpriteFadeHandler blackFadeHandler;
        [SerializeField] private UIFadeHandler labFadeHandler;
        [SerializeField] private UIFadeHandler labForegroundFadeHandler;
        [SerializeField] private UIFadeHandler scrim;
        [SerializeField] private AudioClip officeBuzz;
        [SerializeField] private AudioClip footstepLoop;

        private IEnumerator Start()
        {
            scrim.SetDarkScreen();
            ControllableAudioChannel buzz = AudioManager.Instance.CreateChannel(officeBuzz, AudioCategory.Music, level: 1.2f);
            ControllableAudioChannel footsteps = AudioManager.Instance.CreateChannel(footstepLoop, AudioCategory.Music, level: 1f);
            buzz.Play(); footsteps.Play();
            yield return scrim.FadeInLightScreen(2f);

            yield return new WaitForSeconds(1f);
            StartCoroutine(footsteps.FadeTo(0.4f, 1.5f));
            StartCoroutine(blackFadeHandler.FadeToAlpha(0.7f, 1f));
            yield return DialogueBoxV2.Instance.Play(walkingDialogue.Into());
            StartCoroutine(footsteps.FadeTo(0f, 1.5f));
            yield return blackFadeHandler.FadeInDarkScreen(1f);
            scrim.SetDarkScreen();
            labBg.SetActive(true);
            ControllableAudioChannel eerie = AudioManager.Instance.CreateChannel(SoundID.VN_BGM_suspense_drone, AudioCategory.Music, level: 0f);
            
            eerie.Play();
            StartCoroutine(eerie.FadeTo(1f, 1.5f));

            yield return DialogueBoxV2.Instance.Play(blackDialogue.Into());
            yield return scrim.FadeInLightScreen(1.5f);
            yield return new WaitForSeconds(1.5f);
            yield return new DialogueAsCode().Line(DialogueCharacter.Cam, "These tanks... Are they creatures? Or people?").Play();
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(labForegroundFadeHandler.FadeToAlpha(0f, 1.5f));
            yield return new WaitForSeconds(0.5f);

            yield return DialogueBoxV2.Instance.Play(labDialogue.Into());
            StartCoroutine(eerie.FadeTo(0f, 2.5f));
            yield return scrim.FadeInDarkScreen(1.5f);
            yield return new WaitForSeconds(1f);

            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }
    }
}