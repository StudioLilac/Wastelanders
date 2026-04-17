using DialogueScripts;
using System.Collections;
using UnityEngine;


public class BountySelectDialogue : MonoBehaviour
{
    [SerializeField] private DialogueEntryWrapper bountyIntroduction;
    [SerializeField] private DialogueEntryWrapper pressStartWhenReady;
    [SerializeField] private ScreenCutoutScrim screenCutoutScrim;
    [SerializeField] private MaterialTintFadeHandler materialTintFadeHandler;
    [SerializeField] private CutoutFadeHandler cutoutFadeHandler;
    [SerializeField] private SpriteRenderer interactionBlocker;
    [SerializeField] private RectTransform startBlocker;
    [SerializeField] private Canvas startCanvas;

    IEnumerator Start()
    {
        var buttonClicked = false;
        this.Subscribe<BountyOnClickEvent>(_ => buttonClicked = true);

        if (true || GameStateManager.Instance.PreviousScene == SceneData.Get<SceneData.Epilogue_3>())
        {
            screenCutoutScrim.SetBlocking(true);
            yield return materialTintFadeHandler.FadeToAlpha(200f/255f, 1f);
            yield return DialogueBoxV2.Instance.Play(bountyIntroduction);

            { // Show focus on bounty items
                interactionBlocker.gameObject.SetActive(false);
                cutoutFadeHandler.SetDarkScreen();
                screenCutoutScrim.SetTarget(new SpriteTarget(interactionBlocker));
                yield return cutoutFadeHandler.FadeInLightScreen(1f);
                yield return new WaitUntil(() => buttonClicked);
                yield return cutoutFadeHandler.FadeInDarkScreen(0.5f);
            }

            { // Show focus on start button
                screenCutoutScrim.SetTarget(new UITarget(startBlocker, startCanvas, paddingPercent: 0.15f));
                StartCoroutine(cutoutFadeHandler.FadeInLightScreen(0.5f));
                VerticalLayoutChange.MoveBoxV2ToTop();
                yield return DialogueBoxV2.Instance.Play(pressStartWhenReady);
            }

            yield return materialTintFadeHandler.FadeInLightScreen(1f);
            screenCutoutScrim.SetBlocking(false);
        }
    }
}
