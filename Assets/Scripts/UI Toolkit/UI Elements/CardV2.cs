using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit.UI_Elements
{
    [UxmlElement]
    public partial class CardV2 : VisualElement
    {
        // these two booleans track the current "state" of the pointer's action.
        // clicked refers to whether an OnMouseDown event has happened on this card.
        // dragging refers to if the card is actively being dragged.
        // Both of these are necessary to allow the existing behaviour of clicking a card to select it, plus the new
        // dragging behaviour.
        private bool clicked;
        private bool dragging;
        
        private Vector2 startPos;
        private Vector2 pointerDownPos;
        
        private VisualElement dragLayer;
        private VisualElement originalParent;
        
        public void WithAttrsFromActionClass(ActionClass ac)
        {
            WithAttrs(ac.GetIcon(), ac.GetName(), ac.Speed.ToString(), FormatStats(ac.GetRolledStats()));

            var icon = this.Q<VisualElement>("img-stat-icon");
            icon.ClearClassList();
            icon.AddToClassList(ac.CardType == CardType.Defense ? "stat-icon-def" : "stat-icon-atk");

            var back = this.Q<VisualElement>("img-card-back");
            back.ClearClassList();
            back.AddToClassList($"card-back-{ac switch { AxeCards => "a", FistCards => "f", PistolCards => "p", StaffCards => "s", _ => "e" }}");
        }

        public void BindActionClassCardState(ActionClass ac)
        {
            ac.OnCardStateChanged += WithState;
            WithState(ac.cardState);
            RegisterCallback<DetachFromPanelEvent>(_ => ac.OnCardStateChanged -= WithState);
        }

        public void BindActionClassCallbacks(ActionClass ac)
        {
            RegisterCallback<MouseDownEvent>(_ => ac.OnMouseDown());
            RegisterCallback<MouseEnterEvent>(_ => ac.OnMouseEnter());
            RegisterCallback<MouseLeaveEvent>(_ => ac.OnMouseExit());
            
            // TODO: move these to a non-AC function call
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                dragLayer = panel.visualTree.Q<VisualElement>("drag-layer");
            });
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            clicked = true;
            dragging = false;

            originalParent = parent;

            originalParent.Remove(this);
            dragLayer.Add(this);

            style.position = Position.Absolute;

            startPos = evt.position;
            pointerDownPos = evt.position;

            BringToFront();
            this.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!clicked || !this.HasPointerCapture(evt.pointerId))
                return;

            dragging = true;

            Vector2 delta = (Vector2)evt.position - pointerDownPos;
            Vector2 newPos = startPos + delta;

            style.left = newPos.x;
            style.top = newPos.y;

            evt.StopPropagation();
        }


        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!clicked)
                return;

            clicked = false;

            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);

            if (dragging)
            {
                dragLayer.Remove(this);
                originalParent.Add(this);

                style.position = Position.Relative;
                style.left = StyleKeyword.Auto;
                style.top = StyleKeyword.Auto;
            }

            dragging = false;
        }
        
        // Helper method for the raycast in the below method. Since our HUDV2 panel is scaled with screen size,
        // we need to convert coordinates accordingly.
        Vector2 ToScreenPoint(Vector2 panelPos)
        {
            float scale = panel.scaledPixelsPerPoint;
            return new Vector2(
                panelPos.x * scale,
                (panel.visualTree.layout.height - panelPos.y) * scale
            );
        }
        
        // Raycasts from the screen point to world space, looking for EntityClasses. If it finds one, it uses the
        // existing behaviour in HighlightManager.cs.
        
        // If I open a PR and forget to remove all these debug logs I hope you catch this @anrui
        private void TryClickEntity(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            
            if (cam == null)
            {
                Debug.LogWarning("[TryClickEntity] Camera.main is null");
                return;
            }
            
            screenPos = ToScreenPoint(screenPos);

            Ray ray = cam.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            Debug.Log($"Raycasting from screenPos {screenPos}");

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Debug.Log("Raycast hit NOTHING");
                HighlightManager.Instance.ResetCurrentHighlightedAction();
                return;
            }

            EntityClass entity = hit.collider.GetComponentInParent<EntityClass>();

            Debug.Log($"Entity FOUND: {entity.name} ({entity.GetType().Name})");
            HighlightManager.Instance.OnEntityClicked(entity);
        }


        private void WithAttrs(
            Sprite fg = null,
            string tt = null,
            string sp = null,
            string st = null)
        {
            if (fg) this.Q<VisualElement>("img-card-icon").style.backgroundImage = new StyleBackground(fg);
            if (tt != null) this.Q<Label>("txt-title").text = tt;
            if (sp != null) this.Q<Label>("txt-speed").text = sp;
            if (st != null) this.Q<Label>("txt-stats").text = st;
        }

        private void WithState(ActionClass.CardState state)
        {
            ClearClassList();
            AddToClassList($"card-state-{state switch { ActionClass.CardState.CANT_PLAY => "1", ActionClass.CardState.CLICKED_STATE => "2", _ => "0" }}");
        }

        private static string FormatStats(ActionClass.RolledStats stats) => $"<color=#{stats.FloorBuffs switch { > 0 => "00FF", < 0 => "FF00", _ => "0000" }}00>{stats.RollFloor}</color> - <color=#{stats.CeilingBuffs switch { > 0 => "00FF", < 0 => "FF00", _ => "0000" }}00>{stats.RollCeiling}</color>";
    }
}
