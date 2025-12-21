using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public static class UICoordinateHelper
    {
        /// <summary>
        /// Converts UI Toolkit panel coordinates to screen-space coordinates,
        /// accounting for panel scaling.
        /// </summary>
        public static Vector2 ToScreenPoint(Vector2 panelPos, IPanel panel)
        {
            float scale = panel.scaledPixelsPerPoint;
            return new Vector2(
                panelPos.x * scale,
                (panel.visualTree.layout.height - panelPos.y) * scale
            );
        }

        /// <summary>
        /// Converts screen-space coordinates to UI Toolkit panel coordinates,
        /// accounting for panel scaling.
        /// </summary>
        public static Vector2 ToPanelPoint(Vector2 screenPos, IPanel panel)
        {
            float scale = panel.scaledPixelsPerPoint;
            return new Vector2(
                screenPos.x / scale,
                panel.visualTree.layout.height - (screenPos.y / scale)
            );
        }
    }
}