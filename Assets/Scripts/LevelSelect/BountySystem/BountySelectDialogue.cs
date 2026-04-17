using DialogueScripts;
using System.Collections;
using UnityEngine;


public class BountySelectDialogue : MonoBehaviour
{
    [SerializeField] private DialogueEntryWrapper bountyIntroduction;
    [SerializeField] private ScreenCutoutScrim screenCutoutScrim;
    [SerializeField] private MaterialTintFadeHandler materialTintFadeHandler;

    IEnumerator Start()
    {
        if (true || GameStateManager.Instance.PreviousScene == SceneData.Get<SceneData.Epilogue_3>())
        {
            screenCutoutScrim.SetBlocking(true);
            yield return materialTintFadeHandler.FadeToAlpha(160f/255f, 1f);
            yield return DialogueBoxV2.Instance.Play(bountyIntroduction);
            yield return materialTintFadeHandler.FadeInLightScreen(1f);
            screenCutoutScrim.SetBlocking(false);
        }
    }
}
