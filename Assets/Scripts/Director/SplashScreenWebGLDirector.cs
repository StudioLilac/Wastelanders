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
            GameStateManager.Instance.LoadScene(Get<SceneData.MainMenu>().SceneName);
        }
    }
}