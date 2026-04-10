using System;
using System.Collections;
using LevelSelectInformation;
using UnityEngine;
using static BattleIntroEnum;

namespace Director
{
    public class PrincessFrogCombatDirector : MonoBehaviour
    {
        [SerializeField] private GameObject entityContainer;

        [SerializeField] private DialogueWrapper gameOverDialogue; // sucks...

        private void Start()
        {
            if (CombatManager.Instance.GameState != GameState.GAME_START) return;
            StartCoroutine(OnStart());
        }

        public void OnDisable()
        {
            CombatManager.PlayersWinEvent -= PlayersWin;
            CombatManager.EnemiesWinEvent -= EnemiesWin;
            new ClearBounty().Invoke();
        }

        private IEnumerator OnStart()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            CombatManager.PlayersWinEvent += PlayersWin;
            CombatManager.EnemiesWinEvent += EnemiesWin;

            yield return new WaitForEndOfFrame(); // Necessary for associated initialization code to run (to assign teams)

            CombatManager.Instance.BeginCombat();
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1.5f));
            new BattleIntroEvent(Get<ClashIntro>()).Invoke();
            yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.GAME_WIN);
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
            BountyManager.Instance.NotifyWin();
            GameStateManager.Instance.UpdateLevelProgress(StageInformation.Get<StageInformation.IvesFinale>());
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(CombatManager.Instance.FadeInDarkScreen(1.5f));
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
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
            GameLose();
            CombatManager.Instance.GameState = GameState.GAME_LOSE;
        }

        private void GameLose()
        {
            GameOver.Instance.FadeInWithDialogue(gameOverDialogue);
        }
    }
}