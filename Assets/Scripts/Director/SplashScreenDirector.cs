using LevelSelectInformation;
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
            Debug.Log($"[Splash] Start. timeScale={Time.timeScale}, prepared={videoPlayer.isPrepared}");
            videoPlayer.errorReceived += (vp, msg) => Debug.LogError($"[Splash] Video error: {msg}");
            videoPlayer.loopPointReached += OnVideoEnd;
            StartCoroutine(StartSequence());
        }

        private IEnumerator StartSequence()
        {
            yield return new WaitForSecondsRealtime(1f);
            Debug.Log($"[Splash] Playing video. timeScale={Time.timeScale}");
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
            LoadSpecialScene();

        }

        void LoadSpecialScene()
        {
            SceneData load = true switch
            {
                var _ when StageInformation.Get<StageInformation.PrincessFrogFight>().UnlockCriteriaMet() 
                                && GameStateManager.Instance.RecordFirstTimeEvent(OneTimeEvents.ShowSeason1Intro)=> Get<PreBounty0>(),
                _ => Get<SceneData.MainMenu>()
            };

            GameStateManager.Instance.LoadScene(load.SceneName);

        }
    }
}