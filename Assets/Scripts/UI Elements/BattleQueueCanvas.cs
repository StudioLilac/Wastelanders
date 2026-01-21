using UnityEngine;

public class BattleQueueCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    void Awake()
    {
        canvas.worldCamera = Camera.main;
    }
}
