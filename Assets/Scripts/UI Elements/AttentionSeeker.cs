using System.Collections;
using UnityEngine;

#nullable enable
public class AttentionSeeker : MonoBehaviour
{
    public float cycleSpeed = 1f;
    [SerializeField] private MonoBehaviour attentionObj = null!;
    [SerializeField] private Condition condition;
    private Coroutine? attentionFlashRoutine = null;

    void OnEnable()
    {
        ConfigureAttention(condition);
    }

    private void ConfigureAttention(Condition criteriaMet)
    {
        if (criteriaMet.IsMet() && attentionFlashRoutine == null)
        {
            attentionFlashRoutine = StartCoroutine(FlashAttentionRoutine());
        }
        else if (!criteriaMet.IsMet() && attentionFlashRoutine != null)
        {
            StopCoroutine(attentionFlashRoutine);
            attentionFlashRoutine = null;
            attentionObj.enabled = false;
        }
    }

    private IEnumerator FlashAttentionRoutine()
    {
        var waitFor = new WaitForSeconds(cycleSpeed);

        while (true)
        {
            attentionObj.enabled = !attentionObj.enabled;
            yield return waitFor;
        }
    }
}
