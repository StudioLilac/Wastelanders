using System.Collections;
using UnityEngine;

namespace Dialogue.Epilogue
{
    public class Epilogue_11 : MonoBehaviour
    {
        [SerializeField] private UIFadeHandler blackBg;
        [SerializeField] private UIFadeHandler whiteBg;
        [SerializeField] private CanvasGroupFadeHandler teaserCanvas;
        [SerializeField] private GameObject bg1;
        [SerializeField] private GameObject bg2;
        [SerializeField] private GameObject bg3;
        [SerializeField] private AudioClip caveTown;

        IEnumerator Start()
        {
            var channel = AudioManager.Instance.CreateChannel(SoundID.VN_BGM_suspense_drone, AudioCategory.Music);
            channel.Play();
            blackBg.SetDarkScreen();
            yield return new WaitForEndOfFrame();
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(0f);
            yield return new WaitForSeconds(1.5f);
            SoundID.VN_Message_Tone.Play();
            yield return Epilogue_11_Dialogue.Cam_Talk.Play();
            SoundID.VN_video_call_hangup.Play();
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(ShakeUI(bg1, 3f, 20f, rampUp: true));
            yield return blackBg.FadeInLightScreen(1.5f);
            yield return new WaitForSeconds(0.25f);
            AudioManager.Instance.FadeInBackgroundTrack(1.5f, caveTown, false);
            yield return new WaitForSeconds(1.25f);

            StartCoroutine(ShakeUI(bg1, 0.5f, 100f));
            yield return whiteBg.FadeInDarkScreen(0.2f);
            bg1.SetActive(false); bg2.SetActive(true);
            yield return whiteBg.FadeInLightScreen(0.2f);
            yield return new WaitForSeconds(2f);
            SoundID.VN_Camera_Shutter.Play();

            yield return whiteBg.FadeInDarkScreen(0.2f);
            SoundID.VN_Camera_Shutter.Play();
            bg2.SetActive(false); bg3.SetActive(true);
            yield return whiteBg.FadeInLightScreen(0.2f);
            SoundID.VN_Camera_Shutter.Play();
            yield return new WaitForSeconds(0.3f);
            SoundID.VN_Camera_Shutter.Play();
            yield return new WaitForSeconds(1f);
            yield return Epilogue_11_Dialogue.EntityX.Play();
            yield return new WaitForSeconds(0.5f);
            yield return blackBg.FadeInDarkScreen(1.5f);
            yield return teaserCanvas.FadeInDarkScreen(1.5f);
            yield return new WaitForSeconds(2f);

            yield return Epilogue_11_Dialogue.Season2Pitch.Play();
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName);
        }

        private IEnumerator ShakeUI(GameObject target, float duration, float magnitude, bool rampUp = false)
        {
            if (target == null || target.transform.childCount == 0) yield break;

            int childCount = target.transform.childCount;
            Transform[] children = new Transform[childCount];
            Vector3[] originalPos = new Vector3[childCount];
            Quaternion[] originalRot = new Quaternion[childCount];
            Vector3[] originalScale = new Vector3[childCount];

            float maxMag = target.GetComponent<RectTransform>() != null ? magnitude : magnitude * 0.01f;

            for (int i = 0; i < childCount; i++)
            {
                children[i] = target.transform.GetChild(i);
                originalPos[i] = children[i].localPosition;
                originalRot[i] = children[i].localRotation;
                originalScale[i] = children[i].localScale;
                
                children[i].localScale = originalScale[i] * 1.05f;
            }

            float elapsed = 0.0f;
            WaitForEndOfFrame eof = new WaitForEndOfFrame();

            while (elapsed < duration)
            {
                float currentMag = rampUp ? maxMag * Mathf.Pow(elapsed / duration, 3f) : maxMag;
                
                float x = Random.Range(-1f, 1f) * currentMag;
                float y = Random.Range(-1f, 1f) * currentMag;
                float tilt = Random.Range(-1f, 1f) * (currentMag * 0.05f);

                for (int i = 0; i < childCount; i++)
                {
                    children[i].localPosition = originalPos[i] + new Vector3(x, y, 0);
                    children[i].localRotation = originalRot[i] * Quaternion.Euler(0, 0, tilt);
                }

                elapsed += Time.deltaTime;
                yield return eof;
            }

            for (int i = 0; i < childCount; i++)
            {
                children[i].localPosition = originalPos[i];
                children[i].localRotation = originalRot[i];
                children[i].localScale = originalScale[i];
            }
            
        }

        private static class Epilogue_11_Dialogue
        {
            public static DialogueAsCode Cam_Talk => new DialogueAsCode()
                .Line(DialogueCharacter.Cam, "Jackie? Ives? It's been a while since I've last heard from you two. I hope things are going alright on your end.")
                .Line(DialogueCharacter.Cam, "...I was just calling to share some exciting progress on my end.")
                .Line(DialogueCharacter.Cam, "The first stabilization on the second tone was a success!")
                .Line(DialogueCharacter.Cam, "With some fine personalized tuning, this should be able to help save people early on in their Waste infection.")
                .Line(DialogueCharacter.Cam, "Ives, that means you! So... come back soon once you hear this message alright?")
                .Line(DialogueCharacter.Cam, "Take care you two. I'll see ya.")
                ;
            public static DialogueAsCode Season2Pitch => new DialogueAsCode()
                .Line(DialogueCharacter.Lilac, "Hey there! That's all for Season 1!")
                .Line(DialogueCharacter.Lilac, "We hope you enjoyed! If you have the opportunity, please write us a review on Steam or help spread Wastelanders hype on social media!")
                .Line(DialogueCharacter.Lilac, "For us at Studio Lilac, we're entirely a student project. (NO ONE GETS PAID).")
                .Line(DialogueCharacter.Lilac, "Your support for our awesome musicians, artists, and game is literally what keeps us alive and going.")
                .Line(DialogueCharacter.Lilac, "That also means... if you want to see Cam's arc in Season 2: Where he's THE leading scientist in an increasingly urgent arms race...")
                .Line(DialogueCharacter.Lilac, "Please, show us some love.")
                .Line(DialogueCharacter.Lilac, "And we'll show you our's via the game.")
                .Line(DialogueCharacter.Lilac, "Love, Studio Lilac. <3")
                ;

            public static DialogueAsCode EntityX => new DialogueAsCode()
                .Line(DialogueCharacter.Unknown, "So Entity X... you've awoken.");
        }
    }
}