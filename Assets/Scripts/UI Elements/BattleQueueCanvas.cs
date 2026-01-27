using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static BattleQueue;

public class BattleQueueCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform bqContainer;
    [SerializeField] private BattleQueueIcons iconPrefab;
    [SerializeField] private BattleBeginButton battleBeginButton;
    [SerializeField] private ClashingBattleQueueIcon clashingPrefab;
    [SerializeField] private GameObject battleQueueParent;

#nullable enable
    private List<IBattleQueueDisplayable> battleQueueDisplayables = new();

    void Awake()
    {
        canvas.worldCamera = Camera.main;
        this.Subscribe<BattleBegin>(ResetScrollPosition);
        this.Subscribe<OnQueueChanged>(HandleQueueChanged);
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
        bool displayQueue = gs switch
        {
            GameState.SELECTION => true,
            GameState.FIGHTING => true,
            GameState.GAME_START => true,
            _ => false
        };
        battleBeginButton.GameStateChangeHandler(gs);
        battleQueueParent.SetActive(displayQueue);
    }

    private void ResetScrollPosition(BattleBegin bg)
    {
        scrollRect.velocity = Vector2.zero;
        scrollRect.horizontalNormalizedPosition = 0f;
    }


    private void HandleQueueChanged(OnQueueChanged ev)
    {
        var newData = ev.Items;

        for (int i = battleQueueDisplayables.Count - 1; i >= 0; i--)
        {
            var displayable = battleQueueDisplayables[i];
            bool isStillAlive = newData.Any(wrapper => wrapper.BattleIcon == displayable);

            if (!isStillAlive)
            {
                battleQueueDisplayables.RemoveAt(i);
                StartCoroutine(DeleteItem(displayable));
            }
        }

        for (int i = 0; i < newData.Count; i++)
        {
            ActionWrapper wrapper = newData[i];
            if (wrapper.BattleIcon == null || !battleQueueDisplayables.Contains(wrapper.BattleIcon))
            {
                RenderItemForWrapper(wrapper, i);
            }
        }
    }

    // Helper to Create Item (Extracted from your old RenderItem)
    private void RenderItemForWrapper(ActionWrapper battlingWrapper, int insertIndex)
    {
        GameObject createdObject;

        if (battlingWrapper.IsClashing())
        {
            ClashingBattleQueueIcon icon = Instantiate(clashingPrefab, new Vector3(100, 100, -10), Quaternion.identity);
            createdObject = icon.GameObject;

            ActionClass leftClashItem = battlingWrapper.PlayerAction!;
            ActionClass rightClashItem = battlingWrapper.EnemyAction!;

            icon.RenderClashingIcons(leftClashItem, rightClashItem);
            battlingWrapper.BattleIcon = icon;
        }
        else
        {
            BattleQueueIcons icon = Instantiate(iconPrefab, new Vector3(100, 100, -10), Quaternion.identity);
            createdObject = icon.GameObject;

            icon.RenderBQIcon(battlingWrapper.GetTheOnlyExistingAction());
            if (battlingWrapper.HasEnemyAction()) icon.RenderUnseenIndicator();

            battlingWrapper.BattleIcon = icon;
        }

        createdObject.transform.SetParent(bqContainer, false);
        if (insertIndex < battleQueueDisplayables.Count)
        {
            var neighbor = battleQueueDisplayables[insertIndex];
            createdObject.transform.SetSiblingIndex(neighbor.GameObject.transform.GetSiblingIndex());
        }
        else
        {
            createdObject.transform.SetAsLastSibling();
        }
        battleQueueDisplayables.Insert(insertIndex, battlingWrapper.BattleIcon);
        battlingWrapper.BattleIcon.DeEmphasize();
        battlingWrapper.BattleIcon.SetFullyTransparent();
        StartCoroutine(battlingWrapper.BattleIcon.FadeIn());
    }

    private IEnumerator DeleteItem(IBattleQueueDisplayable item)
    {        
        StartCoroutine(item.FadeOut());
        yield return new WaitForSeconds(IBattleQueueDisplayable.EXPAND_DURATION);
        Destroy(item.GameObject);
    }
}