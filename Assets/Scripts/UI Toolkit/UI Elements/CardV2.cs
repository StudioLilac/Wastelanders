using UnityEngine;
using UnityEngine.EventSystems;
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
        
        // Stores the starting mouse position when drag begins, used to calculate drag delta
        private Vector2 dragStartMousePos;
        
        // TODO: is it bad to hold a reference to the actionclass associated with this card?
        private ActionClass actionClass;
        
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
            
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            actionClass = ac;
        }

        private void OnPointerDown(PointerDownEvent eventData) {
            clicked = true;
            dragStartMousePos = eventData.position;
            
            this.CapturePointer(eventData.pointerId);
            BringToFront();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!clicked) return;
            dragging = true;
            
            // Calculate the delta from the drag start position and apply as transform
            Vector2 delta = (Vector2)evt.position - dragStartMousePos;
            style.translate = new Translate(delta.x, delta.y, 0);
        }

        private void OnPointerUp(PointerUpEvent eventData) {
            if (!clicked) return;
            clicked = false;
            
            if (!dragging)
            {
                // Reset any transform offset if we didn't actually drag
                style.translate = StyleKeyword.Null;
                return;
            }
            dragging = false;
            
            actionClass.ToggleUnSelected();
            if (this.HasPointerCapture(eventData.pointerId))
                this.ReleasePointer(eventData.pointerId);
            
            bool entityFound = TryClickEntity(eventData.position);
            
            // If no valid entity was found, animate the card back to its original position
            if (!entityFound)
            {
                style.translate = StyleKeyword.Null;
            }
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
        // Returns true if an entity was found and clicked, false otherwise.
        
        // If I open a PR and forget to remove all these debug logs I hope you catch this @anrui
        private bool TryClickEntity(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            
            if (cam == null)
            {
                Debug.LogWarning("[TryClickEntity] Camera.main is null");
                return false;
            }
            
            screenPos = ToScreenPoint(screenPos);

            Ray ray = cam.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            Debug.Log($"Raycasting from screenPos {screenPos}");

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Debug.Log("Raycast hit NOTHING");
                HighlightManager.Instance.ResetCurrentHighlightedAction();
                return false;
            }

            EntityClass entity = hit.collider.GetComponentInParent<EntityClass>();
            
            if (entity == null)
            {
                Debug.Log("Raycast hit something but no EntityClass found");
                HighlightManager.Instance.ResetCurrentHighlightedAction();
                return false;
            }

            Debug.Log($"Entity FOUND: {entity.name} ({entity.GetType().Name})");
            HighlightManager.Instance.OnEntityClicked(entity);
            return true;
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
