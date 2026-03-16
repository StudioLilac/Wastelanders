using System.Collections;
using DialogueScripts;
using UnityEngine;
using Utils;

public class PreBounty1 : MonoBehaviour
{
    [SerializeField] private GameObject jackie;
    [SerializeField] private GameObject ives;

    [SerializeField] private Transform ivesTarget;
    [SerializeField] private SpriteRenderer blackScreen;

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
        yield return DialogueBoxV2.Instance.Play(Preamble);
        yield return FadeOutSpriteRenderer(2f, blackScreen);
        yield return DialogueBoxV2.Instance.Play(JackieReminiscingDialogue);

        yield return DialogueSceneUtils.MoveCharacterToTarget(ives, ivesTarget, ivesMoveSpeed);
        ives.GetComponent<Animator>().speed = 0.3f;
        yield return new WaitForSeconds(1f);

        yield return DialogueBoxV2.Instance.Play(BountyBoardDialogue);

        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(1f);
    }

    private IEnumerator FadeOutSpriteRenderer(float time, SpriteRenderer sr)
    {
        float curTime = 0;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1 - curTime / time);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
    }
}
