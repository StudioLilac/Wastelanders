using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit.UI_Elements
{
    [UxmlElement]
    public partial class DeckSelectCardV2 : VisualElement
    {
        public void WithAttrsFromActionClass(ActionClass ac)
        {
            WithAttrs(ac.GetIcon(), ac.GetName(), ac.Speed.ToString(), ac.CostToAddToDeck.ToString(), FormatStats(ac.GetRolledStats()));

            var icon = this.Q<VisualElement>("img-stat-icon");
            var statusIcons = new GetStatusIcons().Query();
            if (statusIcons != null)
                icon.style.backgroundImage = new StyleBackground(
                    ac.CardType == CardType.Defense ? statusIcons.defense : statusIcons.damage);

            var back = this.Q<VisualElement>("img-card-back");
            back.ClearClassList();
            back.AddToClassList($"card-back-{ac switch { AxeCards => "a", FistCards => "f", PistolCards => "p", StaffCards => "s", _ => "e" }}");
        }

        public void BindActionClassCardState(ActionClass ac)
        {
            ac.OnIsSelectedForDeckChanged += WithState;
            WithState(ac.IsSelectedForDeck);
        }

        public void BindActionClassCallbacks(ActionClass ac)
        {
            RegisterCallback<MouseDownEvent>(_ => ac.OnMouseDown());
            RegisterCallback<MouseEnterEvent>(_ => ac.OnMouseEnter());
            RegisterCallback<MouseLeaveEvent>(_ => ac.OnMouseExit());
        }

        private void WithAttrs(
            Sprite fg = null,
            string tt = null,
            string sp = null,
            string pr = null,
            string st = null)
        {
            if (fg) this.Q<VisualElement>("img-card-icon").style.backgroundImage = new StyleBackground(fg);
            if (tt != null) this.Q<Label>("txt-title").text = tt;
            if (sp != null) this.Q<Label>("txt-speed").text = sp;
            if (pr != null) this.Q<Label>("txt-price").text = pr;
            if (st != null) this.Q<Label>("txt-stats").text = st;
        }

        private void WithState(bool isSelectedForDeck)
        {
            ClearClassList();
            AddToClassList($"card-state-{(isSelectedForDeck ? "1" : "0")}");
        }

        private static string FormatStats(ActionClass.RolledStats stats) => $"{stats.RollFloor} - {stats.RollCeiling}";
    }
}