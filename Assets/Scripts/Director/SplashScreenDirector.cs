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
        private bool sequenceEnded = false;

        private void OnDisable()
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
        }

        private void Start()
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.errorReceived += OnVideoError;
            StartCoroutine(StartSequence());
        }

        private void Update()
        {
            if (!sequenceEnded && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                StartCoroutine(EndSequence());
            }
        }

        private IEnumerator StartSequence()
        {
            yield return new WaitForSeconds(1f);
            if (!sequenceEnded)
            {
                videoPlayer.Play();
            }
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogWarning($"VideoPlayer error: {message}");
            StartCoroutine(EndSequence());
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (!sequenceEnded) StartCoroutine(EndSequence());
        }

        private IEnumerator EndSequence()
        {
            if (sequenceEnded) yield break;
            sequenceEnded = true;
            
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.Stop(); // Ensure video stops if skipped

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