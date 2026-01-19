using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit.UI_Elements
{
    /// <summary>
    /// A floating card effect that plays when a card is successfully used on a target.
    /// Shows a smaller version of the card that floats upward while fluttering and fading out.
    /// </summary>
    public class CardPlayEffect
    {
        private readonly VisualElement effectRoot;
        private readonly CardV2 cardVisual;
        private IVisualElementScheduledItem animationSchedule;

        private const float CARD_WIDTH = 246f;
        private const float CARD_HEIGHT = 283f;
        
        // Animation parameters
        private const float DURATION = 0.8f;           // Total animation duration in seconds
        private const float FLOAT_DISTANCE = 120f;     // How far the card floats upward
        private const float FLUTTER_AMPLITUDE = 10f;   // Maximum rotation angle for flutter
        private const float FLUTTER_FREQUENCY = 1f;   // How fast the card flutters (oscillations per second)
        private const float SCALE = 0.5f;              // Size of the effect card relative to original
        
        private float elapsedTime;
        private Vector2 startPosition;
        
        public CardPlayEffect(VisualElement root, VisualTreeAsset cardTemplate, ActionClass actionClass, Vector2 screenPosition)
        {
            effectRoot = new VisualElement
            {
                name = "card-play-effect",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    right = 0,
                    bottom = 0
                }
            };
            
            var cardInstance = cardTemplate.Instantiate();
            cardVisual = cardInstance.Q<CardV2>();
            
            if (cardVisual != null)
            {
                cardVisual.WithAttrsFromActionClass(actionClass);
                cardVisual.pickingMode = PickingMode.Ignore;
            }
            
            SetPickingModeRecursive(cardInstance, PickingMode.Ignore);
            
            float scale = root.panel.scaledPixelsPerPoint;
            startPosition = new Vector2(
                screenPosition.x / scale,
                screenPosition.y / scale
            );
            
            cardInstance.style.position = Position.Absolute;
            
            effectRoot.Add(cardInstance);
            root.Add(effectRoot);
            
            cardInstance.style.left = startPosition.x - (CARD_WIDTH * SCALE);
            cardInstance.style.top = startPosition.y - (CARD_HEIGHT * SCALE);
            cardInstance.style.scale = new Scale(new Vector2(SCALE, SCALE));
            cardInstance.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));
            
            elapsedTime = 0f;
            animationSchedule = effectRoot.schedule.Execute(UpdateAnimation).Every(16);
        }
        
        private void SetPickingModeRecursive(VisualElement element, PickingMode mode)
        {
            element.pickingMode = mode;
            foreach (var child in element.Children())
            {
                SetPickingModeRecursive(child, mode);
            }
        }
        
        private void UpdateAnimation()
        {
            elapsedTime += 0.016f; // Again, called every 16ms.
            float t = elapsedTime / DURATION;
            
            if (t >= 1f)
            {
                animationSchedule?.Pause();
                effectRoot.RemoveFromHierarchy();
                return;
            }
            
            float easeOut = 1f - (1f - t) * (1f - t);
            float yOffset = -FLOAT_DISTANCE * easeOut;
            float flutter = Mathf.Sin(elapsedTime * FLUTTER_FREQUENCY * Mathf.PI * 2f) * FLUTTER_AMPLITUDE * (1f - t);
            
            float fadeStart = 0.4f;
            float opacity = t < fadeStart ? 1f : 1f - ((t - fadeStart) / (1f - fadeStart));
            
            var cardInstance = effectRoot.ElementAt(0);
            cardInstance.style.translate = new Translate(0, yOffset, 0);
            cardInstance.style.rotate = new Rotate(Angle.Degrees(flutter));
            cardInstance.style.opacity = opacity;
        }
        
        public static void SpawnAt(VisualElement root, VisualTreeAsset cardTemplate, ActionClass actionClass, Vector3 worldPosition)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            
            Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);
            
            float panelHeight = root.layout.height * root.panel.scaledPixelsPerPoint;
            screenPos.y = panelHeight - screenPos.y;
            
            new CardPlayEffect(root, cardTemplate, actionClass, new Vector2(screenPos.x, screenPos.y));
        }
    }
}
