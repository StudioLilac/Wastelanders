using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dialogue.PreBounty
{
    public class PreBounty8 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue0;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue4;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue4;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue5;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue6;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue7;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue8;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject background3;

        [SerializeField] private Image caveFlickerLayer;
        [SerializeField] private Image splitScreenLayer;
        [SerializeField] private Image ailinRevealLayer;

        private IEnumerator Start()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            yield return DialogueBoxV2.Instance.Play(postDialogue0.Into());
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue1.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
            yield return DialogueBoxV2.Instance.Play(postDialogue1.Into());
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(1f);
            yield return DialogueBoxV2.Instance.Play(dialogue2.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(postDialogue2.Into());
            background1.SetActive(false);
            background2.SetActive(true);
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(1f);
            yield return DialogueBoxV2.Instance.Play(dialogue3.Into());
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue4.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
            yield return DialogueBoxV2.Instance.Play(postDialogue4.Into());
            yield return new WaitForSeconds(2f);
            background2.SetActive(false);
            background3.SetActive(true);
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(1f);
            yield return DialogueBoxV2.Instance.Play(dialogue5.Into());

            for (var t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                splitScreenLayer.color = new Color(1f, 1f, 1f, t / 0.5f);
                yield return null;
            }

            splitScreenLayer.color = new Color(1f, 1f, 1f, 1f);
            yield return DialogueBoxV2.Instance.Play(dialogue6.Into());
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue7.Into());
            yield return new WaitForSeconds(0.5f);
            AudioManager.Instance.PlaySFX(SoundID.VN_video_call_hangup);

            for (var t = 0.5f; t > 0f; t -= Time.deltaTime)
            {
                splitScreenLayer.color = new Color(1f, 1f, 1f, t / 0.5f);
                yield return null;
            }

            splitScreenLayer.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(dialogue8.Into());
            yield return new WaitForSeconds(0.5f);

            for (var t = 0f; t < 4f; t += Time.deltaTime)
            {
                ailinRevealLayer.color = new Color(1f, 1f, 1f, t / 4f);
                yield return null;
            }

            ailinRevealLayer.color = new Color(1f, 1f, 1f, 1f);
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }

        private void Update()
        {
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Mathf.Sin(Time.time));
        }
    }
}