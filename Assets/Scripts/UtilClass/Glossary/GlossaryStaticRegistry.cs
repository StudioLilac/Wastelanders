using UnityEngine;

namespace UtilClass
{
    public static class Keywords
    {
        public static readonly GlossaryNode OnHit = new(
            "On Hit",
            "An action triggers its On-hit effect when it deals at least 1 damage."
        );
    }

    public static class StatusEffects
    {
        public static readonly GlossaryNode Accuracy = new(
            "Accuracy",
            BuffExplainer.WeaponExplanation.PISTOL_EXPLANATION.ExplanationText,
            Resources.Load<Sprite>("StatusIcon/Accuracy")
        );
        
        public static readonly GlossaryNode Flow = new(
            "Flow",
            BuffExplainer.WeaponExplanation.STAFF_EXPLANATION.ExplanationText,
            Resources.Load<Sprite>("StatusIcon/Flow")
        );
        
        public static readonly GlossaryNode Wound = new(
            "Wound",
            BuffExplainer.WeaponExplanation.AXE_EXPLANATION.ExplanationText,
            Resources.Load<Sprite>("StatusIcon/Wound")
        );
        
        public static readonly GlossaryNode Resonance = new(
            "Resonance",
            BuffExplainer.WeaponExplanation.RESONANCE_EXPLANATION.ExplanationText,
            Resources.Load<Sprite>("StatusIcon/resonance")
        );
    }
}