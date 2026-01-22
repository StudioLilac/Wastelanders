using UnityEngine;

using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public static class VisualElementExtensions
    {
        public static void SetVisible(this VisualElement element, bool visible)
        {
            if (element == null) return;

            if (visible)
            {
                element.style.visibility = Visibility.Visible;
                element.pickingMode = PickingMode.Position;
            }
            else
            {
                element.style.visibility = Visibility.Hidden;
                element.pickingMode = PickingMode.Ignore;
            }
        }
    }
}