using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class TooltipManager : PersistentSingleton<TooltipManager>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement tooltipBox;
        private bool isTooltipVisible = false;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            tooltipBox = root.Q<VisualElement>("tooltip-box");

            if (tooltipBox == null)
            {
                Debug.LogError("Tooltip box not found in UI Document!");
                return;
            }

            // Start hidden
            HideTooltip();
        }

        private void Update()
        {
            if (isTooltipVisible && tooltipBox != null)
            {
                // Update position to follow mouse
                Vector2 mousePos = Input.mousePosition;

                // Convert screen coordinates to UI Toolkit coordinates
                // (bottom-left origin to top-left origin)
                float adjustedY = Screen.height - mousePos.y;

                // Offset so tooltip doesn't cover the cursor
                tooltipBox.style.left = mousePos.x + 15;
                tooltipBox.style.top = adjustedY + 15;
            }
        }

        public void ShowTooltip(string buffName, int stacks)
        {
            if (tooltipBox == null) return;

            // Update tooltip content
            var nameLabel = tooltipBox.Q<Label>("buff-name");
            var stacksLabel = tooltipBox.Q<Label>("buff-stacks");

            if (nameLabel != null)
                nameLabel.text = buffName;

            if (stacksLabel != null)
                stacksLabel.text = $"Stacks: {stacks}";

            // Show the tooltip
            tooltipBox.style.display = DisplayStyle.Flex;
            isTooltipVisible = true;
        }

        public void HideTooltip()
        {
            if (tooltipBox == null) return;

            tooltipBox.style.display = DisplayStyle.None;
            isTooltipVisible = false;
        }
    }
}