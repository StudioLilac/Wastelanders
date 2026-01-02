using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement tooltipBox;
        private bool isTooltipVisible;

        private bool disableTooltip;
        
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
            var root = uiDocument.rootVisualElement;
            tooltipBox = root.Q<VisualElement>("tooltip-box");

            if (tooltipBox == null)
            {
                Debug.LogError("Tooltip box not found in UI Document!");
                return;
            }

            HideTooltip();
        }

        private void Update()
        {
            if (isTooltipVisible && tooltipBox != null)
            {
                Vector2 mousePos = Input.mousePosition;
                Vector2 panelPos = UICoordinateHelper.ToPanelPoint(mousePos, tooltipBox.panel);

                tooltipBox.style.left = panelPos.x + 15;
                tooltipBox.style.top = panelPos.y + 15;
            }
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
    }
}