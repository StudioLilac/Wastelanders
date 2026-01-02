using System;
using System.Linq;
using UI_Toolkit.UI_Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class HUDV2 : MonoBehaviour
    {
        public static HUDV2 Instance { get; private set; }
        
        public VisualTreeAsset cardTemplate;
        public UIDocument rootDocument;

        private VisualElement rootElem;
        private VisualElement handElem;
        private VisualElement infoElem;

        public void Awake()
        {
            Instance = this;
            rootElem = rootDocument?.rootVisualElement ?? throw new Exception($"{nameof(rootDocument)} unset");
            handElem = rootElem.Q<VisualElement>("layout-hand-container");
            infoElem = rootElem.Q<VisualElement>("layout-info-container");
            rootDocument.panelSettings.sortingOrder = UISortOrder.Hudv2.GetOrder();

            RegisterCallbacks();
            LoadInitialValues();
        }
        
        public void SpawnCardPlayEffect(ActionClass actionClass, Vector3 worldPosition)
        {
            UI_Elements.CardPlayEffect.SpawnAt(rootElem, cardTemplate, actionClass, worldPosition);
        }

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
        }

        private void OnHideCardInfo(ActionClass ac)
        {
            infoElem.style.display = DisplayStyle.None;
        }

        private static void OnConfirmClicked()
        {
            BattleQueue.BattleQueueInstance.BeginDequeue();
        }

        private void RegisterCallbacks()
        {
            rootElem.Q<Button>("btn-start").clicked += OnConfirmClicked;
        }

        private void LoadInitialValues()
        {
            handElem.Clear();
            infoElem.style.display = DisplayStyle.None;
        }

        private void OnUpdateHand(PlayerClass player)
        {
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
    }
}