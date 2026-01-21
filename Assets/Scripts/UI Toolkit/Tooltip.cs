using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.UI_Elements;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable
namespace UI_Toolkit
{
    public class Tooltip : MonoBehaviour
    {

        [SerializeField] UIDocument uiDocument = null!;

        private const float CURSOR_OFFSET = 15f;
        
        private VisualElement tooltipBox = null!;
        private bool isTooltipVisible;
        private bool disableTooltip;
        private Label nameLabel = null!;
        private Label stacksLabel = null!;
        private Label descriptionLabel = null!;
        private VisualElement iconLabel = null!;


        public static readonly Dictionary<string, Color> TooltipColors = new Dictionary<string, Color>
        {
            ["ACCURACY"] = Color.blue,
            ["FLOW"] = new Color(1f, 143f/255f, 143f/255f, 1f),
            ["WOUND"] = Color.red,
            ["RESONATE"] = Color.magenta
        };

        public void OnEnable()
        {
            BuffIcons.OnBuffIconHovered += HandleOnBuffIconHovered;
            CombatManager.OnGameStateChanged += HandleOnGameStateChanged;
        }

        public void OnDisable()
        {
            BuffIcons.OnBuffIconHovered -= HandleOnBuffIconHovered;
            CombatManager.OnGameStateChanged -= HandleOnGameStateChanged;
        }
        private void Start()
        {
            this.Subscribe<CardInserted>(CardInsertedEffectHandler);
            this.Subscribe<TooltipEvent>(TooltipTextHandler);
            var root = uiDocument.rootVisualElement;
            tooltipBox = root.Q<VisualElement>("tooltip-box");
            if (tooltipBox != null) tooltipBox.pickingMode = PickingMode.Ignore;
            nameLabel = tooltipBox.Q<Label>("buff-name");
            stacksLabel = tooltipBox.Q<Label>("buff-stacks");
            descriptionLabel = tooltipBox.Q<Label>("buff-description");
            iconLabel = tooltipBox.Q<VisualElement>("buff-icon");

            HideTooltip();
        }

        private void Update()
        {
            if (isTooltipVisible && tooltipBox != null)
            {
                PositionTooltip(tooltipBox);
            }
        }

        private void PositionTooltip(VisualElement tooltip)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 panelPos = UICoordinateHelper.ToPanelPoint(mousePos, tooltip.panel);

            var root = tooltip.panel.visualTree;

            float panelWidth = root.resolvedStyle.width;
            float panelHeight = root.resolvedStyle.height;

            float tooltipWidth = tooltip.resolvedStyle.width;
            float tooltipHeight = tooltip.resolvedStyle.height;

            // by default we'll show the tooltip bottom-right
            float x = panelPos.x + CURSOR_OFFSET;
            float y = panelPos.y + CURSOR_OFFSET;

            // if it would overflow right, flip to left
            if (x + tooltipWidth > panelWidth)
            {
                x = panelPos.x - tooltipWidth - CURSOR_OFFSET;
            }

            // if it would overflow bottom, flip to top
            if (y + tooltipHeight > panelHeight)
            {
                y = panelPos.y - tooltipHeight - CURSOR_OFFSET;
            }

            tooltip.style.left = x;
            tooltip.style.top = y;
        }

        private void HandleOnGameStateChanged(GameState state) {
            if (state == GameState.FIGHTING) {
                disableTooltip = true;
                HideTooltip();
            }
            else {
                disableTooltip = false;
            }
        }

        private void HandleOnBuffIconHovered(string buffName, int stacks, Sprite buffIcon, bool hovered)
        {
            if (hovered)
            {
                ShowTooltip(buffName, stacks, buffIcon);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(string buffName, int stacks, Sprite buffIcon)
        {
            if (tooltipBox == null || disableTooltip) return;

            buffName = buffName.ToUpper();
            
            var buffDescription = BuffExplainer.WeaponExplanation.Values.FirstOrDefault(exp => exp.ExplanationTitle == buffName)?.ExplanationText;

            if (nameLabel != null)
            {
                nameLabel.text = buffName;
                nameLabel.style.display = DisplayStyle.Flex;
            }

            if (stacksLabel != null) {
                stacksLabel.text = $"{stacks} stack{(stacks != 1 ? "s" : "")}";
                stacksLabel.style.color = TooltipColors[buffName];
                stacksLabel.style.display = DisplayStyle.Flex;
            }
                
            if (iconLabel != null)
            {
                iconLabel.style.backgroundImage = new StyleBackground(buffIcon);
                iconLabel.style.display = DisplayStyle.Flex;
            }
            
            if (descriptionLabel != null)
            {
                descriptionLabel.text = buffDescription;
                descriptionLabel.style.display = DisplayStyle.Flex;
            }

            tooltipBox.style.display = DisplayStyle.Flex;
            isTooltipVisible = true;
        }

        private void HideTooltip()
        {
            if (tooltipBox == null) return;

            tooltipBox.style.display = DisplayStyle.None;
            isTooltipVisible = false;
        }

        private void TooltipTextHandler(TooltipEvent toolTip)
        {
            if (toolTip.Style == TextTipDisplayStyle.Display)
                ShowTextTip(toolTip);
            else
                HideTooltip();
        }

        private void ShowTextTip(TooltipEvent text)
        {
            UpdateLabel(nameLabel, text.Title);
            UpdateLabel(stacksLabel, text.Caption);
            UpdateLabel(descriptionLabel, text.Body);
            UpdateIcon(iconLabel, text.Icon);

            stacksLabel.style.color = Color.blue;
            tooltipBox.style.display = DisplayStyle.Flex;
            isTooltipVisible = true;
        }

        private void UpdateLabel(Label label, string? content)
        {
            if (label == null) return;

            bool hasContent = !string.IsNullOrEmpty(content);
            label.style.display = hasContent ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasContent) label.text = content;
        }

        private void UpdateIcon(VisualElement iconElement, Sprite? icon)
        {
            if (iconElement == null) return;

            bool hasIcon = icon != null;
            iconElement.style.display = hasIcon ? DisplayStyle.Flex : DisplayStyle.None;

            if (hasIcon) iconElement.style.backgroundImage = new StyleBackground(icon);
        }

        private void CardInsertedEffectHandler(CardInserted effect)
        {
            if (effect.ActionClass.IsPlayedByPlayer())
            {
                CardPlayEffect.SpawnAt(
                uiDocument.rootVisualElement,
                HUDV2.Instance.cardTemplate,
                effect.ActionClass,
                effect.ActionClass.Target.transform.position);
            }
        }
    }
}

public record TooltipEvent(TextTipDisplayStyle Style, string Title = "", string Body = "", string Caption = "", Sprite? Icon = null) : IEvent { }
public record TooltipText(string Content, TextTipDisplayStyle Style) : IEvent {}

public enum TextTipDisplayStyle
{
    Display,
    None,
}