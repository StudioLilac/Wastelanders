using LevelSelectInformation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using static SceneData;

namespace Director
{
    public class SplashScreenWebGLDirector : MonoBehaviour
    {
        [SerializeField] private Animator splashAnimator;

        private void Start()
        {
            StartCoroutine(StartSequence());
        }

        private IEnumerator StartSequence()
        {
            yield return new WaitUntil(() => splashAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
        }
        
        public void OnSplashAnimationEnd()
        {
            StartCoroutine(EndSequence());
        }
        
        private IEnumerator EndSequence()
        {
            yield return new WaitForSeconds(1f);
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(0.5f);
            LoadSpecialScene();
        }

        void LoadSpecialScene()
        {
            SceneData load = true switch
            {
                var _ when StageInformation.Get<StageInformation.PrincessFrogFight>().UnlockCriteriaMet()
                                && GameStateManager.Instance.RecordFirstTimeEvent(OneTimeEvents.ShowSeason1Intro) => Get<PreBounty0>(),
                _ => Get<SceneData.MainMenu>()
            };

            GameStateManager.Instance.LoadScene(load.SceneName);
        }
    }
}