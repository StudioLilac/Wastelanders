using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    public Animator animator;
    public Image blackBg;
    public UIFadeHandler fadeHandler;
    public bool fastForward;
    public float speedNormal = 1f;
    public float speedFast = 10f;

    void Start()
    {
        StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f);
        StartCoroutine(FadeInBlackBg());
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
        yield return StartCoroutine(fadeHandler.FadeInDarkScreen(2f));
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.PreBounty0>().SceneName);
    }

    IEnumerator FadeInBlackBg()
    {
        float duration = 2f;
        float elapsed = 0f;

        Color startColor = blackBg.color;
        Color endColor = startColor;
        endColor.a = 128f / 255f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            blackBg.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        blackBg.color = endColor;
    }

    void Update() {
        fastForward = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.RightArrow);
        animator.speed = fastForward ? speedFast : speedNormal;
    }
}