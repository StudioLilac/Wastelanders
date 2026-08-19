using System;
using System.Collections;
using System.Collections.Generic;
using DialogueScripts;
using Entities;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleIntroEnum;

namespace Dialogue.Epilogue
{
    public class Epilogue_10 : DialogueClasses
    {
        private static Color PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 1f);
        private static Color TRANSPARENT_PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 0f);

        [SerializeField] private bool jumpToCombat;
        [SerializeField] private bool instakill;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject world;

        [SerializeField] private Image purpleFlash;
        [SerializeField] private Image tundraWithNoise;
        [SerializeField] private Image caveFlickerLayer;
        private bool shouldFlicker = false;

        [SerializeField] private Jackie jackie;
        [SerializeField] private EnemyIves ives;
        [SerializeField] private PrincessFrog princess;

        [SerializeField] private DialogueEntryInUnityEditor[] jayOpeningDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] preBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postVisionDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] momStoryFlashbackDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] weiseDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] spottedPFrogDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] battleStartDialogue;

        [SerializeField] private Transform jackieReturnPosition;
        [SerializeField] private Transform ivesReturnPosition;
        [SerializeField] private Transform princessReturnPosition;
        
        [SerializeField] private AudioClip campfireBg;
        [SerializeField] private AudioClip tundraBg;
        
        public StudioEventEmitter bossfightTrackEmitter;
        
        [SerializeField] private List<GameObject> ivesActions;
        private void Update()
        {
            if (!shouldFlicker) { return; }
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Mathf.Sin(Time.time));
        }

        private IEnumerator PurpleFlash()
        {
            float elapsedTime = 0f;
            float fadeInDuration = 0.2f;
            float fadeOutDuration = 2f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float fraction = elapsedTime / fadeInDuration;
                fraction = Mathf.Clamp01(fraction);
                // Using Lerp is fine here as we calculate fraction every frame
                purpleFlash.color = Color.Lerp(TRANSPARENT_PURPLE, PURPLE, fraction);
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float fraction = elapsedTime / fadeOutDuration;
                fraction = Mathf.Clamp01(fraction);
                purpleFlash.color = Color.Lerp(PURPLE, TRANSPARENT_PURPLE, fraction);
                yield return null;
            }
            // Deals with floating point errors
            purpleFlash.color = TRANSPARENT_PURPLE;
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

        private IEnumerator FadeOutBG(Image bgImage, float fadeTime = 0.8f)
        {
            for (var t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                bgImage.color = new Color(1f, 1f, 1f, fadeTime - t / fadeTime);
                yield return null;
            }
        }

        protected override void GameStateChange(GameState gameState)
        {
            if (gameState == GameState.GAME_START)
            {
                StartCoroutine(
                    ExecuteSceneStart());
            }
        }

        void Setup() {
            jackie.OutOfCombat();
            ives.OutOfCombat();
            princess.OutOfCombat();
        }

        private IEnumerator ExecuteSceneStart()
        {
            Setup();
            CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            purpleFlash.color = Color.black;
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0f);
            jackie.SetReturnPosition(jackieReturnPosition.position);
            ives.SetReturnPosition(ivesReturnPosition.position);
            princess.SetReturnPosition(princessReturnPosition.position);

            if (!GameStateManager.Instance.JumpToCombat && !jumpToCombat) {
                yield return DialogueBoxV2.Instance.Play(jayOpeningDialogue.Into());
                AudioManager.Instance.FadeInBackgroundTrack(5f, tundraBg, true);
                UIFadeScreenManager.Instance.SetDarkScreen();
                purpleFlash.color = TRANSPARENT_PURPLE;

                yield return UIFadeScreenManager.Instance.FadeInLightScreen(1.5f);
                yield return DialogueBoxV2.Instance.Play(preBonfireDialogue.Into());

                yield return PurpleFlash();
                yield return DialogueBoxV2.Instance.Play(postVisionDialogue.Into());
                shouldFlicker = true;
                AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
                yield return new WaitForSeconds(1);
                AudioManager.Instance.FadeInBackgroundTrack(2f, campfireBg, true);
                yield return new WaitForSeconds(5);
                yield return DialogueBoxV2.Instance.Play(postBonfireDialogue.Into());

                AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
                yield return new WaitForSeconds(1);
                background2.SetActive(true);
                AudioManager.Instance.FadeInBackgroundTrack(2f, tundraBg, true);
                yield return FadeInBG(tundraWithNoise);

                yield return DialogueBoxV2.Instance.Play(momStoryFlashbackDialogue.Into());
                AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
                yield return FadeOutBG(tundraWithNoise, 1f);
                AudioManager.Instance.FadeInBackgroundTrack(1f, campfireBg, true);
                background2.SetActive(false);

                yield return DialogueBoxV2.Instance.Play(weiseDialogue.Into());
                AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
                yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);

                background1.SetActive(false);
                purpleFlash.color = TRANSPARENT_PURPLE;
                AudioManager.Instance.FadeInBackgroundTrack(2f, tundraBg, true);
                ives.FaceLeft();
                princess.FaceLeft();
                yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
                yield return DialogueBoxV2.Instance.Play(spottedPFrogDialogue.Into());
                StartCoroutine(jackie.MoveToPosition(new Vector3(2.75f, 0.4f, jackie.transform.position.z), 0f, 2f));

                yield return new WaitForSeconds(2);
                ives.AttackAnimation("IsPunching");
                AudioManager.Instance.PlaySFX(SoundID.CB_fist_hit);
                yield return StartCoroutine(jackie.StaggerEntities(ives, jackie, 0.4f));
                yield return DialogueBoxV2.Instance.Play(battleStartDialogue.Into());
            } else {
                ives.FaceLeft();
                princess.FaceLeft();
                background2.SetActive(false);
                background1.SetActive(false);
                purpleFlash.color = TRANSPARENT_PURPLE;
                GameStateManager.Instance.JumpToCombat = false;
                yield return UIFadeScreenManager.Instance.FadeInLightScreen(0.5f);
            }

            new BattleIntroEvent(Get<ClashIntro>()).Invoke();
            
            CombatManager.PlayersWinEvent += PlayersWin;
            CombatManager.EnemiesWinEvent += EnemiesWin;

            princess.OwnedMinions.Add(ives);
            ives.InjectDeck(ivesActions);
            bossfightTrackEmitter.Play();

            if (instakill) {
                jackie.AddStacks(Accuracy.buffName, 999);
                jackie.AddStacks(Resonate.buffName, 999);
            }
            
            CombatManager.Instance.BeginCombat();
            
            yield return new WaitUntil(() => new GetGameState().Query() == GameState.GAME_WIN);
            EventInstance instance = bossfightTrackEmitter.EventInstance;
            instance.setParameterByNameWithLabel("BossState", "Defeated");
            
            CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }

        private void PlayersWin()
        {
            CombatManager.EnemiesWinEvent -= EnemiesWin;
            CombatManager.PlayersWinEvent -= PlayersWin;
            CombatManager.Instance.GameState = GameState.GAME_WIN;
        }

        private void EnemiesWin()
        {
            CombatManager.EnemiesWinEvent -= EnemiesWin;
            CombatManager.PlayersWinEvent -= PlayersWin;
            CombatManager.Instance.GameState = GameState.GAME_LOSE;
        }
    }
}