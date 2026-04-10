using System.Collections;
using UnityEngine;

#nullable enable
public class AttentionSeeker : MonoBehaviour
{
    public float cycleSpeed = 1f;
    [SerializeField] private MonoBehaviour attentionObj = null!;
    
    private Coroutine? attentionFlashRoutine = null;

    public void ConfigureAttention(bool criteriaMet)
    {
        if (criteriaMet && attentionFlashRoutine == null)
        {
            attentionFlashRoutine = StartCoroutine(FlashAttentionRoutine());
        }
        else if (!criteriaMet && attentionFlashRoutine != null)
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
