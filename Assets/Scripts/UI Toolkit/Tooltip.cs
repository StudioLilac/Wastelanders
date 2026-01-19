using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance { get; private set; }

        public UIDocument uiDocument;

        private const float CURSOR_OFFSET = 15f;
        
        private VisualElement tooltipBox;
        private Label textTip;
        private bool isTooltipVisible;
        private bool disableTooltip;
        private TextTipDisplayStyle currentTextStyle = TextTipDisplayStyle.BottomRight;

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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            this.Subscribe<TooltipText>(TooltipTextHandler);
            var root = uiDocument.rootVisualElement;
            tooltipBox = root.Q<VisualElement>("tooltip-box");
            textTip = root.Q<Label>("textip-text");
            if (textTip != null) textTip.pickingMode = PickingMode.Ignore;
            if (tooltipBox != null) tooltipBox.pickingMode = PickingMode.Ignore;

            HideTooltip();
            HideTextTip();
        }

        private void Update()
        {
            if (isTooltipVisible && tooltipBox != null)
            {
                PositionTooltip(tooltipBox);
            }

            if (currentTextStyle != TextTipDisplayStyle.None && textTip != null)
            {
                Vector2 mousePos = Input.mousePosition;
                const float offset = 15f;
                Vector2 panelPos = UICoordinateHelper.ToPanelPoint(mousePos, textTip.panel);

                float targetX = panelPos.x + offset;
                float targetY = currentTextStyle switch
                {
                    TextTipDisplayStyle.TopRight => panelPos.y - textTip.layout.height - offset,
                    _ => panelPos.y + offset
                };

                textTip.style.left = targetX;
                textTip.style.top = targetY;
            } else
            {
                HideTextTip();
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
                HideTextTip();
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

            var nameLabel = tooltipBox.Q<Label>("buff-name");
            var stacksLabel = tooltipBox.Q<Label>("buff-stacks");
            var iconLabel = tooltipBox.Q<VisualElement>("buff-icon");
            var descriptionLabel = tooltipBox.Q<Label>("buff-description");
            
            var buffDescription = BuffExplainer.WeaponExplanation.Values.FirstOrDefault(exp => exp.ExplanationTitle == buffName)?.ExplanationText;

            if (nameLabel != null)
                nameLabel.text = buffName;

            if (stacksLabel != null) {
                stacksLabel.text = $"{stacks} stack{(stacks != 1 ? "s" : "")}";
                stacksLabel.style.color = TooltipColors[buffName];
            }
                
            
            if (iconLabel != null)
                iconLabel.style.backgroundImage = new StyleBackground(buffIcon);
            
            if (descriptionLabel != null)
                descriptionLabel.text = buffDescription;

            tooltipBox.style.display = DisplayStyle.Flex;
            isTooltipVisible = true;
        }

        private void HideTooltip()
        {
            if (tooltipBox == null) return;

            tooltipBox.style.display = DisplayStyle.None;
            isTooltipVisible = false;
        }

        private void TooltipTextHandler(TooltipText text)
        {
            ShowGenericTooltip(text.Content, text.Style);
        }

        private void ShowGenericTooltip(string text, TextTipDisplayStyle style)
        {
            if (textTip == null || disableTooltip) return;

            currentTextStyle = style;
            textTip.text = text;
            textTip.style.display = DisplayStyle.Flex;
        }

        private void HideTextTip()
        {
            if (textTip == null) return;

            textTip.style.display = DisplayStyle.None;
            currentTextStyle = TextTipDisplayStyle.None;
        }
    }
}

public record TooltipText(string Content, TextTipDisplayStyle Style) : IEvent {}

public enum TextTipDisplayStyle
{
    BottomRight,
    TopRight,
    None,
}