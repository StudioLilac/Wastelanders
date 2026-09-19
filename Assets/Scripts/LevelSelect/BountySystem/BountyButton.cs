using BountySystem;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public record BountyOnClickEvent(IBounties Bounty) : IEvent;
// A Specific Bounty Button Component
public class BountyButton : MonoBehaviour
{
    [SerializeField] private TMP_Text bountyTitle;
    [SerializeField] private SpriteRenderer bountyBackRenderer;
    [SerializeField] private SpriteRenderer rewardIconRenderer;
    [SerializeField] private SpriteRenderer selectedRenderer;
    [SerializeField] private Sprite completed;
    [SerializeField] private Sprite incomplete;

    [Range(0, 1)]
    [SerializeField] private float hoverAlpha; // transparency level of selectedSprite when mouse is over

    // late init !!
    private IBounties bounty;
    private BountyAssetDatabase bountyAssetDatabase;
#nullable enable
    private bool selected = false;
    private bool mouseOver = false;

    public delegate void BountyButtonDelegate(IBounties bounty);
    public static event BountyButtonDelegate? BountyOnHoverEvent; // Needed for updating popup board
    public static event BountyButtonDelegate? BountyOnHoverEndEvent;  // Needed for updating popup board

    private void Awake()
    {
        this.Subscribe<BountyOnClickEvent>(ev => HandleOtherBountySelectedEvent(ev.Bounty));
    }

    void OnMouseOver()
    {
        mouseOver = true;
        if (Input.GetMouseButtonDown(0))
        {
            OnPress();
        }
    }

    void OnMouseEnter()
    {
        BountyOnHoverEvent?.Invoke(bounty);
        if (selected) return;
        selectedRenderer.gameObject.SetActive(true);
        UpdateSelectedAlpha(hoverAlpha);
    }

    void OnMouseExit()
    {
        BountyOnHoverEndEvent?.Invoke(bounty);
        mouseOver = false;
        if (selected) return;
        selectedRenderer.gameObject.SetActive(false);
        UpdateSelectedAlpha(1f);
    }

    private void HandleOtherBountySelectedEvent(IBounties bounty)
    {
        if (bounty != this.bounty) Deselected();
    }

    public void OnPress()
    {
        selected = !selected;
        new BountyOnClickEvent(bounty).Invoke();
        if (selected)
        {
            Selected();
        }
        else
        {
            Deselected();
        }
    }

    private void Selected()
    {
        selected = true;
        selectedRenderer.gameObject.SetActive(true);
        UpdateSelectedAlpha(1f);
    }

    private void Deselected()
    {
        selected = false;
        if (!mouseOver) selectedRenderer.gameObject.SetActive(false);
        else UpdateSelectedAlpha(hoverAlpha);
    }

    public void Initialize(IBounties bounty, BountyAssetDatabase database)
    {
        this.bounty = bounty;
        this.bountyAssetDatabase = database;
        Redraw();
    }

    private void Redraw()
    {
        rewardIconRenderer.sprite = bounty?.GetBountyAssets(bountyAssetDatabase).Sprite;
        bountyTitle.text = bounty?.BountyName;
        bountyBackRenderer.sprite = BountyManager.Instance.IsBountyCompleted(bounty) ? completed : incomplete;
    }

    private void UpdateSelectedAlpha(float alpha)
    {
        Color c = selectedRenderer.color;
        c.a = alpha;
        selectedRenderer.color = c;
    }
}