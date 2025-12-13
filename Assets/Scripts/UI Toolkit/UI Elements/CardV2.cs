using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI_Toolkit.UI_Elements
{
    [UxmlElement]
    public partial class CardV2 : VisualElement
    {
        private bool dragging;
        private Vector2 startPos;
        
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
            dragging = true;
            startPos = layout.position;
            
            this.CapturePointer(eventData.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!dragging) return;

            Debug.Log("OnPointerMove");
        }

        private void OnPointerUp(PointerUpEvent eventData) {
            if (!dragging) return;
            dragging = false;

            Debug.Log("Playing card");
            actionClass.ToggleUnSelected();
            if (this.HasPointerCapture(eventData.pointerId))
                this.ReleasePointer(eventData.pointerId);

            TryClickEntity(eventData.position);
        }
        
        private void TryClickEntity(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("TryClickEntity: Camera.main is null");
                return;
            }

            Ray ray = cam.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            Debug.Log($"Raycasting from screenPos {screenPos}");

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                Debug.Log("Raycast hit NOTHING");
                return;
            }

            Debug.Log($"Raycast hit: {hit.collider.name}");
            Debug.Log($"Hit collider type: {hit.collider.GetType()}");
            Debug.Log($"Hit collider layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            EntityClass entity = hit.collider.GetComponentInParent<EntityClass>();

            if (entity == null)
            {
                Debug.Log("Collider found, but NO EntityClass in parents");
                Debug.Log("Components on hit object:");

                foreach (var c in hit.collider.GetComponents<Component>())
                    Debug.Log($" - {c.GetType().Name}");

                return;
            }

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
