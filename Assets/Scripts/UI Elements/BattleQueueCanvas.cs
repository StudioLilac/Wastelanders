using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BattleQueue;

public class BattleQueueCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform bqContainer;
    [SerializeField] private BattleQueueIcons iconPrefab;
    [SerializeField] private ClashingBattleQueueIcon clashingPrefab;
    [SerializeField] private GameObject battleQueueParent;

    private List<IBattleQueueDisplayable> battleQueueDisplayables = new();

    void Awake()
    {
        canvas.worldCamera = Camera.main;
        this.Subscribe<BattleBegin>(ResetScrollPosition);
        this.Subscribe<ItemAdded>(RenderItem);
        this.Subscribe<ItemRemoved>(DequeueItem);
    }

    private void OnEnable()
    {
        CombatManager.OnGameStateChanged += GameStateChangeHandler;
    }

    private void OnDisable()
    {
        CombatManager.OnGameStateChanged -= GameStateChangeHandler;
    }

    private void GameStateChangeHandler(GameState gs)
    {
        bool displayQueue = gs switch { 
            GameState.SELECTION => true,
            GameState.FIGHTING => true,
            GameState.GAME_START => true,
           _ => false 
        };
        battleQueueParent.SetActive(displayQueue);
    }

    private void ResetScrollPosition(BattleBegin bg)
    {
        scrollRect.velocity = Vector2.zero;
        scrollRect.horizontalNormalizedPosition = 0f;
    }

    private void RenderItem(ItemAdded item)
    {
        ActionWrapper battlingWrapper = item.Item;
        GameObject createdObject;

        if (battlingWrapper.IsClashing())
        {
            ClashingBattleQueueIcon icon = Instantiate(clashingPrefab, new Vector3(100, 100, -10), Quaternion.identity);
            createdObject = icon.GameObject;

            ActionClass leftClashItem = battlingWrapper.PlayerAction!;
            ActionClass rightClashItem = battlingWrapper.EnemyAction!;

            icon.RenderClashingIcons(leftClashItem, rightClashItem);
            battlingWrapper.BindIcon(icon);
        }
        else
        {
            BattleQueueIcons icon = Instantiate(iconPrefab, new Vector3(100, 100, -10), Quaternion.identity);
            createdObject = icon.GameObject;

            icon.RenderBQIcon(battlingWrapper.GetTheOnlyExistingAction());
            if (battlingWrapper.HasEnemyAction()) icon.RenderUnseenIndicator();

            battlingWrapper.BindIcon(icon);
        }

        createdObject.transform.SetParent(bqContainer, false);
        int insertIndex = item.Location;
        if (insertIndex < battleQueueDisplayables.Count)
        {
            var neighbor = battleQueueDisplayables[insertIndex];
            createdObject.transform.SetSiblingIndex(neighbor.GameObject.transform.GetSiblingIndex());
        }
        else
        {
            createdObject.transform.SetAsLastSibling();
        }
        battleQueueDisplayables.Insert(insertIndex, battlingWrapper.BattleIcon!);
        battlingWrapper.BattleIcon!.DeEmphasize();
        StartCoroutine(battlingWrapper.BattleIcon!.FadeIn());
    }

    private void DequeueItem(ItemRemoved item) => StartCoroutine(DeleteItem(item.Item.BattleIcon!));

    private IEnumerator DeleteItem(IBattleQueueDisplayable item)
    {
        battleQueueDisplayables.Remove(item);
        yield return StartCoroutine(item.FadeOut());
        Destroy(item.GameObject);
    }
}
