using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using static SceneData;

namespace Director
{
    public class SplashScreenDirector : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        private void OnDisable()
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        private void Start()
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            StartCoroutine(StartSequence());
        }

        private IEnumerator StartSequence()
        {
            yield return new WaitForSeconds(1f);
            videoPlayer.Play();
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            StartCoroutine(EndSequence());
        }

        private IEnumerator EndSequence()
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            yield return new WaitForSeconds(1f);
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(0.5f);
            GameStateManager.Instance.LoadScene(Get<SceneData.MainMenu>().SceneName);
        }
    }
}