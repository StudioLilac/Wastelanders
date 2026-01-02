using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using DialogueScripts;
using Systems.Persistence;
using Director;

public class PostQueenBeetle : DialogueClasses
{
    [SerializeField] private Jackie jackie;

    [SerializeField] private Transform jackieListensToBroadcast;
    [SerializeField] private Transform jackieIvesTalk;

    [SerializeField] private EnemyIves ives;

    [SerializeField] private CinemachineVirtualCamera ivesCamera;
    [SerializeField] private Transform mainCameraIvesTalk;

    [SerializeField] Sprite jackieSmileImage;
    [SerializeField] Image ivesImage;
    [SerializeField] Image jackieImage;
    [SerializeField] private UIFadeHandler hugBackground;
    [SerializeField] private UIFadeHandler backgroundScrim;

    [SerializeField] private DialogueEntryWrapper jackieOpening;
    [SerializeField] private DialogueEntryWrapper soldierChat;
    [SerializeField] private DialogueEntryWrapper resultBroadcast;
    [SerializeField] private DialogueEntryWrapper jackieConfused;
    [SerializeField] private DialogueEntryWrapper ivesFinal;
    [SerializeField] private DialogueEntryWrapper jackieFinal;

    protected override void GameStateChange(GameState gameState)
    {
        if (gameState == GameState.GAME_START)
        {
            StartCoroutine(ExecuteGameStart());
        }
    }

    private IEnumerator ExecuteGameStart()
    {
        CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
        CombatFadeScreenHandler.Instance.SetDarkScreen();
        ives.FaceLeft();
        yield return new WaitForSeconds(0.8f);

        jackie.OutOfCombat(); ives.OutOfCombat();
        yield return StartCoroutine(CombatFadeScreenHandler.Instance.FadeInLightScreen(1f));
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(jackieOpening));
        yield return StartCoroutine(jackie.MoveToPosition(jackieListensToBroadcast.position, 0f, 1.2f));
        var originalSpeed = jackie.animator.speed;
        StartCoroutine(LerpSpeed(jackie.animator, 0.0f, 2f));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(backgroundScrim.FadeToAlpha(0.7f, 1.0f));

        yield return StartCoroutine(DialogueBoxV2.Instance.Play(soldierChat));

        yield return StartCoroutine(DialogueBoxV2.Instance.Play(resultBroadcast));
        yield return StartCoroutine(backgroundScrim.FadeInLightScreen(1f));

        CombatManager.Instance.ActivateDynamicCamera();
        jackie.FaceLeft();
        yield return new WaitForSeconds(0.8f);
        jackie.animator.speed = originalSpeed;
        yield return StartCoroutine(jackie.MoveToPosition(jackieIvesTalk.position, 0f, 2f));
        yield return new WaitForSeconds(0.8f);
        jackie.animator.enabled = false;

        ivesCamera.transform.position = mainCameraIvesTalk.position;
        ivesCamera.Priority = 2;
        CombatManager.Instance.ActivateBaseCamera();
        yield return StartCoroutine(backgroundScrim.FadeToAlpha(0.7f, 1.0f));
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(jackieConfused));
        yield return new WaitForSeconds(0.5f);

        ives.FaceRight(); ives.animator.enabled = false; yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(DialogueBoxV2.Instance.Play(ivesFinal));

        yield return StartCoroutine(backgroundScrim.FadeToAlpha(1.0f, 1.0f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(hugBackground.FadeInDarkScreen(1.5f));

        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(jackieFinal));

        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.Credits>().SceneName);    
    }

    private IEnumerator LerpSpeed(Animator animator, float target, float duration)
    {
        float startSpeed = animator.speed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            animator.speed = Mathf.Lerp(startSpeed, target, elapsed / duration);
            yield return null;
        }

        animator.speed = target;
    }
}
