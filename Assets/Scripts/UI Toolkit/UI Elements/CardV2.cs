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
        
        private Vector2 dragStartMousePos;
        private Vector2 lastMousePos;
        private float currentRotation;
        private float targetRotation;
        private int activePointerId = -1;
        
        private IVisualElementScheduledItem rotationSchedule;
        
        // How strongly velocity affects rotation (degrees per pixel of velocity)
        private const float ROTATION_SENSITIVITY = 0.6f;
        
        // How quickly the rotation returns to neutral (0 = instant, 1 = never)
        private const float ROTATION_DAMPING = 0.85f;
        
        private const float MAX_ROTATION = 35f;
        
        // TODO: is it bad to hold a reference to the actionclass associated with this card?
        private ActionClass actionClass;
        
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
        
        // Raycasts from the screen point to world space, looking for EntityClasses. If it finds one, it uses the
        // existing behaviour in HighlightManager.cs.
        // Returns true if an entity was found and clicked, false otherwise.
        private void TryClickEntity(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            
            if (cam == null)
            {
                return;
            }
            
            screenPos = ToScreenPoint(screenPos);

            Ray ray = cam.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return;
            }

            // Right now, I'm only looking for enemies. This is because there aren't ANY cards in the game that
            // target a player. If we allowed this to look for EntityClass, cards would get "stuck" on a player.
            //
            // If we ever make cards that target players, you'd need to change this to EntityClass. Along with that,
            // you'd need to make HighlightManager.Instance.OnEntityClicked somehow indicate if that card assignment
            // is valid, i.e. are you playing a player-targeting card on an enemy? Technically you'd need this
            // anyways since the old click to select method would also require this knowledge.
            EnemyClass enemy = hit.collider.GetComponentInParent<EnemyClass>();
            
            if (enemy == null)
            {
                return;
            }

            HighlightManager.Instance.OnEntityClicked(enemy);
        }


        private void WithAttrs(
            Sprite fg = null,
            string tt = null,
            string sp = null,
            string sf = null,
            string sc = null)
        {
            if (fg) this.Q<VisualElement>("img-card-icon").style.backgroundImage = new StyleBackground(fg);
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
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            if (buffValue > 0)
            {
                label.style.color = Color.green;
            }
            else if (buffValue < 0)
            {
                label.style.color = Color.red;
            }
            else
            {
                label.style.color = Color.black;
                label.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
        }
    }
}
