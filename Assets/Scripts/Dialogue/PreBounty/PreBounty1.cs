using System.Collections;
using DialogueScripts;
using UnityEngine;
using Utils;

public class PreBounty1 : MonoBehaviour
{
    [SerializeField] private GameObject jackie;
    [SerializeField] private GameObject ives;

    [SerializeField] private Transform ivesTarget;
    [SerializeField] private SpriteFadeHandler blackScreen;

    [SerializeField] private DialogueEntryWrapper Preamble;
    [SerializeField] private DialogueEntryWrapper JackieReminiscingDialogue;
    [SerializeField] private DialogueEntryWrapper BountyBoardDialogue;

    [SerializeField] private float ivesMoveSpeed = 6f;

    public void Start()
    {
        StartCoroutine(StartScene());
    }

    public IEnumerator StartScene()
    {
        UIFadeScreenManager.Instance.SetDarkScreen();
        yield return UIFadeScreenManager.Instance.FadeInLightScreen(1f);
        yield return DialogueBoxV2.Instance.Play(Preamble);
        yield return blackScreen.FadeToAlpha(0, 2f);
        yield return DialogueBoxV2.Instance.Play(JackieReminiscingDialogue);

        yield return DialogueSceneUtils.MoveCharacterToTarget(ives, ivesTarget, ivesMoveSpeed);
        ives.GetComponent<Animator>().speed = 0.3f;
        yield return new WaitForSeconds(1f);

        yield return DialogueBoxV2.Instance.Play(BountyBoardDialogue);

        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.PreBounty2>().SceneName);
    }
}
