using System.Collections;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
    }
}