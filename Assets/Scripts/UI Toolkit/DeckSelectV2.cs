using System;
using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.UI_Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class DeckSelectV2 : MonoBehaviour
    {
        public VisualTreeAsset cardTemplate;
        public UIDocument rootDocument;

        private VisualElement rootElem;
        private VisualElement gridElem;

        public void Awake()
        {
            rootElem = rootDocument?.rootVisualElement ?? throw new Exception($"{nameof(rootDocument)} unset");
            gridElem = rootElem.Q<VisualElement>("layout-grid");
            rootDocument.panelSettings.sortingOrder = UISortOrder.Base.GetOrder();

            LoadInitialValues();
            this.Subscribe<DeckSelectStateChanged>(OnDeckSelectStateChanged);
        }

        public void OnEnable()
        {
            DeckSelectionManager.OnRenderDecks += OnRenderDecks;
        }

        public void OnDisable()
        {
            DeckSelectionManager.OnRenderDecks -= OnRenderDecks;
        }

        private void OnDeckSelectStateChanged(DeckSelectStateChanged ev)
        {
            rootElem.style.display = ev.State == DeckSelectionState.DeckSelection ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnRenderDecks(int cols, List<ActionClass> acs)
        {
            gridElem.Clear();
            foreach (var ac in acs)
            {
                var cardLayout = cardTemplate.Instantiate();
                var card = cardLayout.Q<DeckSelectCardV2>();
                card.WithAttrsFromActionClass(ac);
                card.BindActionClassCardState(ac);
                card.BindActionClassCallbacks(ac);

                gridElem.Add(cardLayout);
            }

            gridElem.ClearClassList();
            gridElem.AddToClassList($"grid-{cols}");
        }

        private void LoadInitialValues()
        {
            gridElem.Clear();
        }
    }
}