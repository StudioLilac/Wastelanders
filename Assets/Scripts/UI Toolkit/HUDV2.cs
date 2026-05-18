using System;
using System.Linq;
using UI_Toolkit.UI_Elements;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UtilClass;

namespace UI_Toolkit
{
    public class HUDV2 : MonoBehaviour {
        private const float FADED_OPACITY = 0.1f;
        
        [SerializeField] private Sprite deckInfoSprite;
        public static HUDV2 Instance { get; private set; }
        
        public VisualTreeAsset cardTemplate;
        public VisualTreeAsset glossaryNodeTemplate;
        public UIDocument rootDocument;

        private VisualElement rootElem;
        private VisualElement handElem;
        private VisualElement infoElem;
        private ScrollView childrenElem;
        private Label deckInfoLabel;

        private bool childrenFaded;

#nullable enable
        public void Awake()
        {
            Instance = this;
            rootElem = rootDocument?.rootVisualElement ?? throw new Exception($"{nameof(rootDocument)} unset");
            handElem = rootElem.Q<VisualElement>("layout-hand-container");
            infoElem = rootElem.Q<VisualElement>("layout-info-container");
            childrenElem = rootElem.Q<ScrollView>("children-container");
            rootDocument.panelSettings.sortingOrder = UISortOrder.Hudv2.GetOrder();
            deckInfoLabel = rootElem.Q<Label>("txt-deck-info");
            deckInfoLabel.RegisterCallback<MouseEnterEvent>(OnDeckHoverEnter);
            deckInfoLabel.RegisterCallback<MouseLeaveEvent>(OnDeckHoverExit);
            
            LoadInitialValues();
        }

        private void OnDeckHoverEnter(MouseEnterEvent ev) =>
            new TooltipEvent(TextTipDisplayStyle.Display, 
                Icon: deckInfoSprite, 
                Title: "CARDS REMAINING",
                Caption: GetCurrentDeckInfoText(), 
                Body: GetCurrentDeckBodyText()
               ).Invoke();

        private string GetCurrentDeckInfoText()
        {
            var player = new CurrentPlayer().Query();
            if (player) {
                if (player.Exhausted) {
                    return "No cards left";
                } else {
                    return $"{player.Pool.Count}/{player.DeckSize}";
                }
            }

            return "";
        }

        private string GetCurrentDeckBodyText() {
            var player = new CurrentPlayer().Query();
            if (player) {
                if (player.Exhausted) {
                    return "Your hand size is zero. All that's left to do is struggle.";
                }
            }

            return "Everytime the deck depletes and reshuffles, your hand size decreases by one!";
        }

        private void OnDeckHoverExit(MouseLeaveEvent ev) => new TooltipEvent(TextTipDisplayStyle.None).Invoke();

        public void OnEnable()
        {
            CombatManager.OnGameStateChanging += OnGameStateChanging;
            DisplayableClass.OnShowCard += OnShowCardInfo;
            DisplayableClass.OnHideCard += OnHideCardInfo;
            HighlightManager.OnUpdateHand += OnUpdateHand;

            ActionClass.CardHighlightedEvent += OnShowCardInfo;
            ActionClass.CardUnhighlightedEvent += OnHideCardInfo;
        }

        public void OnDisable()
        {
            CombatManager.OnGameStateChanging -= OnGameStateChanging;
            DisplayableClass.OnShowCard -= OnShowCardInfo;
            DisplayableClass.OnHideCard -= OnHideCardInfo;
            HighlightManager.OnUpdateHand -= OnUpdateHand;

            ActionClass.CardHighlightedEvent -= OnShowCardInfo;
            ActionClass.CardUnhighlightedEvent -= OnHideCardInfo;
        }

        private void OnGameStateChanging(GameState to)
        {
            rootElem.style.display = to switch
            {
                GameState.FIGHTING or GameState.OUT_OF_COMBAT => DisplayStyle.None,
                GameState.SELECTION => DisplayStyle.Flex,
                _ => rootElem.style.display
            };
        }

        private void OnShowCardInfo(ActionClass ac)
        {
            var card = infoElem.Q<TemplateContainer>("CardV2").Q<CardV2>();
            card.WithAttrsFromActionClass(ac);

            var desc = infoElem.Q<Label>("txt-blurb");
            desc.text = ac.GenerateCardDescription();
            desc.ClearClassList();
            desc.AddToClassList(ac.IsPlayedByPlayer() ? "blurb-player" : "blurb-enemy");

            infoElem.style.display = DisplayStyle.Flex;
            
            childrenElem.Clear();
            var children = GlossaryNode.GetAllSubchildren(ac.GlossaryNode);

            for (int i = 1; i < children.Count; i++)
            {
                GlossaryNode node = children[i];
                var entry = glossaryNodeTemplate.Instantiate();

                entry.Q<Label>("node-title").text = node.Title;
                entry.Q<Label>("node-tooltip").text = node.Tooltip;

                var icon = entry.Q<VisualElement>("node-icon");
                if (node.Icon != null)
                    icon.style.backgroundImage = new StyleBackground(node.Icon);
                else
                    icon.style.display = DisplayStyle.None;

                childrenElem.Add(entry);
                if (childrenFaded) entry.style.opacity = FADED_OPACITY;
            }

            childrenElem.style.display = children.Count > 1 
                ? DisplayStyle.Flex 
                : DisplayStyle.None;
            infoElem.style.display = DisplayStyle.Flex;
        }

        private void OnHideCardInfo(ActionClass ac)
        {
            childrenElem.Clear();
            childrenElem.style.display = DisplayStyle.None;
            infoElem.style.display = DisplayStyle.None;
        }

        private void LoadInitialValues()
        {
            handElem.Clear();
            infoElem.style.display = DisplayStyle.None;
        }

        private void OnUpdateHand(PlayerClass player)
        {
            deckInfoLabel.text = player.Pool.Count.ToString();
            handElem.Clear();

            foreach (var ac in player.Hand.Select(go => go.GetComponent<ActionClass>()).Where(ac => ac))
            {
                var cardLayout = cardTemplate.Instantiate();
                var card = cardLayout.Q<CardV2>();
                card.WithAttrsFromActionClass(ac);
                card.BindActionClassCardState(ac);
                card.BindActionClassCallbacks(ac);
                card.RegisterPointerEventCallbacks();

                handElem.Add(cardLayout);
                ac.SetCanPlay(ac.IsPlayableByPlayer(out _));
            }
        }

        public void SetDeckInfoVisibility(bool visible)
        {
            deckInfoLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void FadeChildren() {
            if (childrenFaded) return;
            foreach (VisualElement child in childrenElem.Children())
            {
                child.experimental.animation.Start(1f, FADED_OPACITY, 300, (el, val) => el.style.opacity = val);
            }
            childrenFaded = true;
        }

        public void UnfadeChildren()
        {
            if (!childrenFaded) return;
            foreach (VisualElement child in childrenElem.Children())
            {
                child.experimental.animation.Start(FADED_OPACITY, 1f, 300, (el, val) => el.style.opacity = val);
            }
            childrenFaded = false;
        }
    }
}