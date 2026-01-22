using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using static BattleQueue;

public record BattleBegin() : IEvent { }
public class BattleBeginButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image duringSelectionSprite;
    [SerializeField] private Image duringCombatSprite;
    [SerializeField] private Sprite defaultState;
    [SerializeField] private Sprite activeState;
    [SerializeField] private Sprite activeDownState;
    [SerializeField] private Sprite defaultDownState;
    [SerializeField] private float flashingSpeed = 5.0f;
    private bool isHovering;
    private bool isActive;

    public bool CanStartCombat { private get; set; } = true;

    private void Awake()
    {
        this.Subscribe<OnQueueRendered>(HandlePlayerActionCount);
    }

    private void HandlePlayerActionCount(OnQueueRendered ev)
    {
        isActive = ev.Items.Count(aw => aw.HasPlayerAction()) > 0;
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
        isActive = isActive && gs == GameState.SELECTION;
    }

    private void Update()
    {
        if (!duringSelectionSprite.enabled) return;
        if (isHovering) return;
        if (isActive)
        {
            if (CanStartCombat)
            {
                float cycle = Time.time % flashingSpeed;
                duringSelectionSprite.sprite = (cycle < flashingSpeed / 2f) ? activeState : defaultState;
            } else
            {
                if (duringSelectionSprite.sprite != activeState) duringSelectionSprite.sprite = activeState;
            }
        }
        else 
        {
            if (duringSelectionSprite.sprite != defaultState) duringSelectionSprite.sprite = defaultState;
        }
    }

    private Sprite GetHoveredState() => isActive ? activeDownState: defaultDownState;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanStartCombat) new BattleBegin().Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        duringSelectionSprite.sprite = GetHoveredState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public Image SelectionSprite => duringSelectionSprite;
}
