using DialogueScripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit.UI_Elements
{
#nullable enable
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
        
        private Vector2 dragStartMousePos;
        private Vector2 lastMousePos;
        private float currentRotation;
        private float targetRotation;
        private int activePointerId = -1;
        
        private IVisualElementScheduledItem? rotationSchedule;
        
        // How strongly velocity affects rotation (degrees per pixel of velocity)
        private const float ROTATION_SENSITIVITY = 0.6f;
        
        // How quickly the rotation returns to neutral (0 = instant, 1 = never)
        private const float ROTATION_DAMPING = 0.85f;
        
        private const float MAX_ROTATION = 35f;
        
        // TODO: is it bad to hold a reference to the actionclass associated with this card?
        private ActionClass actionClass = null!;
        
        public void WithAttrsFromActionClass(ActionClass ac)
        {
            var stats = ac.GetRolledStats();
            WithAttrs(ac.GetIcon(), ac.GetName(), ac.Speed.ToString(), stats.RollFloor.ToString(), stats.RollCeiling.ToString());

            var floorLabel = this.Q<Label>("txt-stat-floor-outline");
            ApplyStatStyle(floorLabel, stats.FloorBuffs);

            var ceilingLabel = this.Q<Label>("txt-stat-ceiling-outline");
            ApplyStatStyle(ceilingLabel, stats.CeilingBuffs);

            var icon = this.Q<VisualElement>("img-stat-icon");
            icon.ClearClassList();
            icon.AddToClassList(ac.CardType == CardType.Defense ? "stat-icon-def" : "stat-icon-atk");

            var back = this.Q<VisualElement>("img-card-back");
            back.ClearClassList();
            back.AddToClassList($"card-back-{ac switch { AxeCards => "a", FistCards => "f", PistolCards => "p", StaffCards => "s", ClasslessCards => "c", _ => "e" }}");
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
            
            actionClass = ac;
        }

        // These callbacks hook into pointer events. All three are required to properly detect drag and drop movements.
        public void RegisterPointerEventCallbacks() {
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent eventData) {
            if (actionClass.IsPlayableByPlayer(out PopupType popupType) == false)
            {
                PopUpNotificationManager.Instance.DisplayWarning(popupType);
                return;
            }
            if (clicked) return; 
            
            clicked = true;
            activePointerId = eventData.pointerId;
            dragStartMousePos = eventData.position;
            lastMousePos = eventData.position;
            currentRotation = 0f;
            targetRotation = 0f;
            
            this.CapturePointer(eventData.pointerId);
            BringToFront();
            
            rotationSchedule = schedule.Execute(UpdateRotation).Every(16); // 60fps
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!clicked) return;
            dragging = true;
            
            HighlightManager.Instance.SetSelectedAction(actionClass);
            
            // code that moves the card
            Vector2 delta = (Vector2)evt.position - dragStartMousePos;
            float velocityX = evt.position.x - lastMousePos.x;
            lastMousePos = evt.position;
            style.translate = new Translate(delta.x, delta.y, 0);
            
            // code that rotates the card
            targetRotation = Mathf.Clamp(-velocityX * ROTATION_SENSITIVITY, -MAX_ROTATION, MAX_ROTATION);
        }
        
        // Called every 16ms; handles the rotation. @Anrui let me know if we have a standardized framerate for UI 
        //      (I don't think we do).
        private void UpdateRotation()
        {
            currentRotation = Mathf.Lerp(currentRotation, targetRotation, 1f - ROTATION_DAMPING);
            targetRotation *= ROTATION_DAMPING;
            style.rotate = new Rotate(Angle.Degrees(currentRotation));
        }

        private void OnPointerUp(PointerUpEvent eventData) {
            if (!clicked) return;
            clicked = false;
            
            rotationSchedule?.Pause();
            rotationSchedule = null;
            
            if (this.HasPointerCapture(eventData.pointerId))
                this.ReleasePointer(eventData.pointerId);
            
            if (!dragging)
            {
                style.translate = StyleKeyword.Null;
                style.rotate = StyleKeyword.Null;
                return;
            }
            dragging = false;
            
            actionClass.ToggleSelected();
            TryClickEntity(eventData.position);
            
            style.translate = StyleKeyword.Null;
            style.rotate = StyleKeyword.Null;
        }
        
        // Helper method for the raycast in the below method. Since our HUDV2 panel is scaled with screen size,
        // we need to convert coordinates accordingly.
        Vector2 ToScreenPoint(Vector2 panelPos)
        {
            return UICoordinateHelper.ToScreenPoint(panelPos, panel);
        }

        // Raycasts from the screen point to world space, looking for both 3D enemies and 2D UI cards.
        private void TryClickEntity(Vector2 screenPos)
        {
            Camera cam = Camera.main;

            if (cam == null)
            {
                return;
            }

            screenPos = ToScreenPoint(screenPos);

            if (TryClickCard(cam.ScreenToWorldPoint(screenPos)))
            {
                return;
            }

            TryClickEnemy(cam.ScreenPointToRay(screenPos));
        }

        private bool TryClickCard(Vector3 worldPos)
        {
            Debug.DrawRay(worldPos, Vector3.forward * 10f, Color.red, 2f);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector3.forward, 1000f);
            if (!hit.collider)
            {
                return false;
            }

            CombatCardUI? cardIcon = hit.collider.GetComponent<CombatCardUI>();
            BattleQueueIcons? bqIcon = hit.collider.GetComponent<BattleQueueIcons>();


            if (cardIcon != null)
            {
                actionClass.OnMouseExit();
                TryClash(cardIcon.ActionClass);
                return true;
            }

            if (bqIcon != null)
            {
                actionClass.OnMouseExit();
                TryClash(bqIcon.ActionClass);
                return true;
            }

            return false;
        }

        private void TryClickEnemy(Ray ray)
        {
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return;
            }
            EnemyClass? enemy = hit.collider.GetComponent<EnemyClass>();

            if (enemy != null)
            {
                actionClass.OnMouseExit();
                HighlightManager.Instance.OnEntityClicked(enemy);
            }
        }

        void TryClash(ActionClass? clashingAction)
        {
            if (clashingAction == null) return;
            HighlightManager.Instance.OnIconClicked(clashingAction, actionClass);
        }


        private void WithAttrs(
            Sprite? fg = null,
            string? tt = null,
            string? sp = null,
            string? sf = null,
            string? sc = null)
        {
            if (fg != null) this.Q<VisualElement>("img-card-icon").style.backgroundImage = new StyleBackground(fg);
            if (tt != null) this.Q<Label>("txt-title").text = tt;
            if (sp != null) this.Q<Label>("txt-speed").text = sp;
            if (sf != null) this.Q<Label>("txt-stat-floor").text = sf;
            if (sf != null) this.Q<Label>("txt-stat-floor-outline").text = sf;
            if (sc != null) this.Q<Label>("txt-stat-ceiling").text = sc;
            if (sc != null) this.Q<Label>("txt-stat-ceiling-outline").text = sc;
        }

        private void WithState(ActionClass.CardState state)
        {
            ClearClassList();
            AddToClassList($"card-state-{state switch { ActionClass.CardState.CANT_PLAY => "1", ActionClass.CardState.CLICKED_STATE => "2", _ => "0" }}");
        }

        private static void ApplyStatStyle(Label label,int buffValue)
        {
            var (targetColor, fontStyle, blur) = buffValue switch
            {
                > 0 => (Color.green, FontStyle.Bold, 2f),
                < 0 => (Color.red, FontStyle.Bold, 2f),
                _ => (Color.black, FontStyle.Normal, 0f) 
            };

            label.style.color = targetColor;
            label.style.unityFontStyleAndWeight = fontStyle;
            label.style.textShadow = new TextShadow
            {
                offset = Vector2.zero,
                blurRadius = blur,
                color = targetColor
            };
        }
    }
}
