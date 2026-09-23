using System;
using UI_Toolkit;
using UnityEngine;

#nullable enable
public class HoverIndicatorManager : MonoBehaviour 
{
    [Header("Hover Indicator Assets")]
    public Sprite? redirectIcon;
    public Sprite? unopposedAttackIcon;
    public Sprite? unopposedDefenseIcon;
    public Sprite? clashIcon;
    public TMPro.TMP_FontAsset? floatingFont;

    private GameObject? hoverIndicatorObj;
    private SpriteRenderer? hoverIndicatorMainIcon;
    private SpriteRenderer? hoverIndicatorSecondaryIcon;
    private TMPro.TextMeshPro? hoverIndicatorText;
    private EntityClass? hoveredEntity = null;
    private ActionClass? hoveredActionIcon = null;

    private void Start()
    {
        this.Subscribe<EntityHovered>(e => OnEntityHovered(e.Entity));
        this.Subscribe<EntityUnhovered>(e => OnEntityUnhovered(e.Entity));
        this.Subscribe<DisplayableHoveredEvent>(e => OnIconHovered(e.ActionClass));
        this.Subscribe<DisplayableUnhoveredEvent>(e => OnIconUnhovered(e.ActionClass));
    }

    private void SetupHoverIndicator()
    {
        hoverIndicatorObj = new GameObject("HoverIndicator");
        hoverIndicatorObj.transform.SetParent(this.transform);
        
        GameObject mainIconObj = new GameObject("MainIcon");
        mainIconObj.transform.SetParent(hoverIndicatorObj.transform);
        mainIconObj.transform.localPosition = Vector3.zero;
        hoverIndicatorMainIcon = mainIconObj.AddComponent<SpriteRenderer>();
        hoverIndicatorMainIcon.sortingLayerName = UISortName.CardUILayer.GetSortName();
        hoverIndicatorMainIcon.sortingOrder = 0;
        
        GameObject secondaryIconObj = new GameObject("SecondaryIcon");
        secondaryIconObj.transform.SetParent(hoverIndicatorObj.transform);
        secondaryIconObj.transform.localPosition = new Vector3(0.5f, -0.5f, 0);
        hoverIndicatorSecondaryIcon = secondaryIconObj.AddComponent<SpriteRenderer>();
        hoverIndicatorSecondaryIcon.sortingLayerName = UISortName.CardUILayer.GetSortName();
        hoverIndicatorSecondaryIcon.sortingOrder = 1;
        
        GameObject textObj = new GameObject("HoverText");
        textObj.transform.SetParent(hoverIndicatorObj.transform);
        textObj.transform.localPosition = new Vector3(1f, 0, 0);
        hoverIndicatorText = textObj.AddComponent<TMPro.TextMeshPro>();
        hoverIndicatorText.font = floatingFont;
        hoverIndicatorText.fontSize = 4;
        hoverIndicatorText.sortingLayerID = hoverIndicatorMainIcon.sortingLayerID;
        hoverIndicatorText.sortingOrder = 2;
        hoverIndicatorText.alignment = TMPro.TextAlignmentOptions.Left;
        
        SetIndicatorActive(false);
    }

    private void OnEntityHovered(EntityClass entity)
    {
        hoveredEntity = entity;
        UpdateHoverIndicator();
    }
    
    private void OnEntityUnhovered(EntityClass entity)
    {
        if (hoveredEntity == entity) 
        {
            hoveredEntity = null;
            UpdateHoverIndicator();
        }
    }

    private void OnIconHovered(ActionClass icon)
    {
        hoveredActionIcon = icon;
        UpdateHoverIndicator();
    }
    
    private void OnIconUnhovered(ActionClass icon)
    {
        if (hoveredActionIcon == icon)
        {
            hoveredActionIcon = null;
            UpdateHoverIndicator();
        }
    }

    private void Update()
    {
        if (hoverIndicatorObj != null && hoverIndicatorObj.activeSelf)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            hoverIndicatorObj.transform.position = mousePos;
        }
    }

    private void SetIndicatorActive(bool active)
    {
        if (hoverIndicatorObj != null) 
        {
            hoverIndicatorObj.SetActive(active);
        }
        Cursor.visible = !active;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void UpdateHoverIndicator()
    {
        if (hoverIndicatorObj == null) SetupHoverIndicator();

        ActionClass? currentHighlightedAction = new GetCurrentHighlightedAction().Query();

        if (currentHighlightedAction == null || (hoveredEntity == null && hoveredActionIcon == null))
        {
            SetIndicatorActive(false);
            return;
        }

        SetIndicatorActive(true);
        hoverIndicatorSecondaryIcon!.sprite = null;

        if (hoveredActionIcon != null)
        {
            ActionClass incomingAction = currentHighlightedAction;
            ActionClass existingAction = hoveredActionIcon;

            if (existingAction.Origin is PlayerClass || !existingAction.Clashable || !incomingAction.Clashable)
            {
                SetIndicatorActive(false);
                return;
            }
            
            if (existingAction.Target != incomingAction.Origin)
            {
                if (incomingAction.Speed >= existingAction.Speed)
                {
                    hoverIndicatorMainIcon!.sprite = redirectIcon;
                    hoverIndicatorText!.text = "Redirect!";
                }
                else
                {
                    hoverIndicatorMainIcon!.sprite = null;
                    hoverIndicatorText!.text = "Too slow!";
                }
            }
            else
            {
                hoverIndicatorMainIcon!.sprite = clashIcon;
                hoverIndicatorText!.text = "Clash";
                if (incomingAction.CardType == CardType.Defense)
                {
                    hoverIndicatorSecondaryIcon!.sprite = unopposedDefenseIcon;
                    hoverIndicatorText!.text = "Clash & Defend";
                }
            }
        }
        else if (hoveredEntity != null && hoveredEntity is EnemyClass clickedEnemy)
        {
            ActionClass incomingAction = currentHighlightedAction;
            ActionClass mockAction = incomingAction;
            EntityClass originalTarget = mockAction.Target;
            mockAction.Target = clickedEnemy;

            bool willClash = false;
            foreach (BattleQueue.ActionWrapper wrapper in BattleQueue.BattleQueueInstance.ProvideArray())
            {
                if (wrapper.CanClashWithAction(mockAction, allowRedirect: false))
                {
                    willClash = true;
                    break;
                }
            }
            mockAction.Target = originalTarget;

            if (willClash)
            {
                hoverIndicatorMainIcon!.sprite = clashIcon;
                hoverIndicatorText!.text = "Clash";
                if (incomingAction.CardType == CardType.Defense)
                {
                    hoverIndicatorSecondaryIcon!.sprite = unopposedDefenseIcon;
                    hoverIndicatorText!.text = "Clash & Defend";
                }
            }
            else
            {
                if (incomingAction.CardType == CardType.Defense)
                {
                    hoverIndicatorMainIcon!.sprite = unopposedDefenseIcon;
                    hoverIndicatorText!.text = "Unopposed Defend";
                }
                else
                {
                    hoverIndicatorMainIcon!.sprite = unopposedAttackIcon;
                    hoverIndicatorText!.text = "Unopposed Attack";
                }
            }
        }
    }
}


