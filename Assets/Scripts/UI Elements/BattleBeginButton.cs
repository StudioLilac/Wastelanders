using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using static BattleQueue;

public class BattleBeginButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image duringSelectionSprite;
    [SerializeField] private Image duringCombatSprite;
    [SerializeField] private Sprite defaultState;
    [SerializeField] private Sprite activeState;
    [SerializeField] private Sprite activeDownState;
    [SerializeField] private Sprite defaultDownState;
    private bool isActive;
    public bool CanStartCombat { private get; set; } = true;

    private void Awake()
    {
        this.Subscribe<OnQueueRendered>(HandlePlayerActionCount);
    }

    private void HandlePlayerActionCount(OnQueueRendered ev)
    {
        isActive = ev.Items.Count(aw => aw.HasPlayerAction()) > 0;
        duringSelectionSprite.sprite = GetUnhoveredStateSprite();
    }

    private void OnEnable()
    {
        CombatManager.OnGameStateChanged += GameStateChangeHandler;
    }

    private void OnDisable()
    {
        CombatManager.OnGameStateChanged -= GameStateChangeHandler;
    }

    void GameStateChangeHandler(GameState gs)
    {
        duringCombatSprite.enabled = gs == GameState.FIGHTING;
        duringSelectionSprite.enabled = gs == GameState.SELECTION;
    }

    private Sprite GetHoveredState() => isActive ? activeDownState: defaultDownState;
    private Sprite GetUnhoveredStateSprite() => isActive ? activeState : defaultState;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanStartCombat) BattleQueue.BattleQueueInstance.BeginDequeue();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        duringSelectionSprite.sprite = GetHoveredState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        duringSelectionSprite.sprite = GetUnhoveredStateSprite();
    }
}
