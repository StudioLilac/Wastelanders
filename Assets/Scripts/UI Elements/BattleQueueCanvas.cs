using UnityEngine;
using UnityEngine.UI;

public class BattleQueueCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect scrollRect;
    void Awake()
    {
        canvas.worldCamera = Camera.main;
        this.Subscribe<BattleBegin>(ResetScrollPosition);
    }

    public void ResetScrollPosition(BattleBegin bg)
    {
        scrollRect.velocity = Vector2.zero;
        scrollRect.horizontalNormalizedPosition = 0f;
    }
}
