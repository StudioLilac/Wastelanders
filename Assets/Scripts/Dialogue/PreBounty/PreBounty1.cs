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


    void Awake()
    {
        if (GameStateManager.Instance.PreviousScene == SceneData.Get<SceneData.LevelSelect>())
            this.Answer<GetGameState, GameState?>(_ => GameState.GAME_START); // Mock this scene as a combat scene to allow jump into combat.
    }

    public void Start()
    {
        if (GameStateManager.Instance.JumpToCombat)
        {
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.Epilogue_3>().SceneName, payload: new CombatPayload(SceneData.Get<SceneData.Epilogue_3>(), JumpToCombat: true));
        }
        else
        {
            StartCoroutine(StartScene());
        }
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
