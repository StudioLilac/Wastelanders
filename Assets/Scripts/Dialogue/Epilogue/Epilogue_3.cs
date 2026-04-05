using System.Collections;
using DialogueScripts;
using UnityEngine;

public class Epilogue_3 : MonoBehaviour
{
    // BGs
    [SerializeField] private SpriteFadeHandler black;
    [SerializeField] private SpriteFadeHandler car;
    [SerializeField] private SpriteFadeHandler marsh;

    // Dialogue
    [SerializeField] private DialogueEntryWrapper Driving;
    [SerializeField] private DialogueEntryWrapper WesternMarsh;

    void Start()
    {
        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene()
    {
        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(0f);
        yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);

        yield return DialogueBoxV2.Instance.Play(Driving);
        yield return black.FadeInDarkScreen(2f);
        yield return car.FadeInLightScreen(0f);
        yield return marsh.FadeInDarkScreen(0f);
        // TODO: Fade out vehicle SFX
        yield return black.FadeInLightScreen(2f);
        
        yield return DialogueBoxV2.Instance.Play(WesternMarsh);

        // TODO: Purple flash
        // TODO: [Beetle sprite slides in]

        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);   
    }
}
