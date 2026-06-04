
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static CardDatabase;

public class BuffExplainer : MonoBehaviour
{
    [SerializeField] SpriteRenderer buffIcon;
    [SerializeField] TextMeshPro explanationTitleField;
    [SerializeField] TextMeshPro explanationTextField;
    List<WeaponExplanation> explanationText = WeaponExplanation.Values;
#nullable enable
    private StatusEffect? currentEffect;

    public void RenderExplanationForBuff(CardDatabase.WeaponType weaponType)
    {
        explanationTextField.text = explanationText.FirstOrDefault(tuple => tuple.WeaponType == weaponType)?.ExplanationText;
        explanationTitleField.text = explanationText.FirstOrDefault(tuple => tuple.WeaponType == weaponType)?.ExplanationTitle;
        currentEffect = weaponType switch
        {
            CardDatabase.WeaponType.STAFF => new Flow(),
            CardDatabase.WeaponType.PISTOL => new Accuracy(),
            CardDatabase.WeaponType.FIST => null,
            CardDatabase.WeaponType.AXE => new Wound(),
            CardDatabase.WeaponType.ENEMY => new Resonate(),
            _ => null
        };
        
        buffIcon.sprite = currentEffect?.GetIcon();
    }


    public class WeaponExplanation
    {
        [field: SerializeField] public CardDatabase.WeaponType WeaponType { get; private set; }
        [field: SerializeField] public string ExplanationTitle { get; private set; } = "";
        [field: SerializeField] public string ExplanationText { get; private set; } = "";

        public static readonly WeaponExplanation STAFF_EXPLANATION = new(weaponType: WeaponType.STAFF, explanationTitle: Flow.buffName.ToUpper(), explanationText: "The next Action's Power gains +1 to its upper and lower bound for every stack of Flow. Then consume all Flow.");
        public static readonly WeaponExplanation PISTOL_EXPLANATION = new(weaponType: WeaponType.PISTOL, explanationTitle: Accuracy.buffName.ToUpper(), explanationText: "Lower bound of an Action's Power increases by 1 per stack. On taking damage, stacks are halved (rounded down).");
        public static readonly WeaponExplanation AXE_EXPLANATION = new(weaponType: WeaponType.AXE, explanationTitle: Wound.buffName.ToUpper(), explanationText: "Damage taken by an Action increases by 1 per stack. Stacks are halved after each round (rounded down).");
        public static readonly WeaponExplanation FIST_EXPLANATION = new(weaponType: WeaponType.FIST, explanationTitle: "FIST", explanationText: "Makes several small but impactful attacks.");
        public static readonly WeaponExplanation RESONANCE_EXPLANATION = new(weaponType: WeaponType.ENEMY, explanationTitle: Resonate.buffName.ToUpper(), explanationText: "Upper bound of an Action's power increases by 1 per stack.");
        public static readonly List<WeaponExplanation> Values = new() { STAFF_EXPLANATION, PISTOL_EXPLANATION, AXE_EXPLANATION, FIST_EXPLANATION, RESONANCE_EXPLANATION };
        WeaponExplanation(WeaponType weaponType, string explanationTitle, string explanationText)
        {
            WeaponType = weaponType;
            ExplanationTitle = explanationTitle;
            ExplanationText = explanationText;
        }
    }
}
