using System.Collections;
using DialogueScripts;
using UnityEngine;

public class Epilogue_9 : MonoBehaviour
{
    [SerializeField] private SpriteFadeHandler blackBg;
    [SerializeField] private SpriteFadeHandler purpleFlash;
    [SerializeField] private SpriteFadeHandler caveBg;
    [SerializeField] private SpriteFadeHandler tundraLab;

    [SerializeField] private DialogueEntryWrapper Preamble;
    [SerializeField] private DialogueEntryWrapper JayRadio;
    [SerializeField] private DialogueEntryWrapper PostRadio;
    [SerializeField] private DialogueEntryWrapper PreAscent;
    [SerializeField] private DialogueEntryWrapper PostAscent;
    [SerializeField] private DialogueEntryWrapper PostWallFlash;
    [SerializeField] private DialogueEntryWrapper Perimeter;
    [SerializeField] private DialogueEntryWrapper TundraWeiss;
    [SerializeField] private DialogueEntryWrapper PostRipple;

    void Start()
    {
        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene()
    {
        yield return blackBg.FadeInDarkScreen(0f);
        yield return DialogueBoxV2.Instance.Play(Preamble);
        yield return blackBg.FadeInLightScreen(2f);

        yield return DialogueBoxV2.Instance.Play(JayRadio);
        AudioManager.Instance.PlaySFX(SoundID.VN_radio_static);
        yield return new WaitForSeconds(3f);

        yield return DialogueBoxV2.Instance.Play(PostRadio);
        yield return blackBg.FadeInDarkScreen(2f);

        yield return DialogueBoxV2.Instance.Play(PreAscent);
        yield return blackBg.FadeInLightScreen(2f);

        yield return DialogueBoxV2.Instance.Play(PostAscent);
        yield return purpleFlash.FadeInDarkScreen(0.2f);
        // TODO: RINGING HERE
        yield return purpleFlash.FadeInLightScreen(3f);
        yield return DialogueBoxV2.Instance.Play(PostWallFlash);
        yield return blackBg.FadeInDarkScreen(2f);

        yield return DialogueBoxV2.Instance.Play(Perimeter);
        yield return tundraLab.FadeInDarkScreen(0f);
        yield return blackBg.FadeInLightScreen(2f);
        yield return DialogueBoxV2.Instance.Play(TundraWeiss);
        
        // TODO: Ping sound effect
        yield return purpleFlash.FadeInDarkScreen(0.5f);
        yield return purpleFlash.FadeInLightScreen(0.5f);

        yield return DialogueBoxV2.Instance.Play(PostRipple);
        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
    }
}
