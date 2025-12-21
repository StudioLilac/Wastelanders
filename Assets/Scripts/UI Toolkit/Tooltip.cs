using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement tooltipBox;
        private bool isTooltipVisible;

        public void OnEnable()
        {
            BuffIcons.OnBuffIconHovered += HandleOnBuffIconHovered;
        }

        public void OnDisable()
        {
            BuffIcons.OnBuffIconHovered -= HandleOnBuffIconHovered;
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

        private void HandleOnBuffIconHovered(string buffName, int stacks, bool hovered)
        {
            if (hovered)
            {
                ShowTooltip(buffName, stacks);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(string buffName, int stacks)
        {
            if (tooltipBox == null) return;

            var nameLabel = tooltipBox.Q<Label>("buff-name");
            var stacksLabel = tooltipBox.Q<Label>("buff-stacks");

            if (nameLabel != null)
                nameLabel.text = buffName;

            if (stacksLabel != null)
                stacksLabel.text = $"Stacks: {stacks}";

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