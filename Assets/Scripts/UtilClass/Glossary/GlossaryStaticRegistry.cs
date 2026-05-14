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
        public static readonly GlossaryNode OnHitNode = new(
            "Accuracy",
            BuffExplainer.WeaponExplanation.PISTOL_EXPLANATION.ExplanationText,
            Resources.Load<Sprite>("StatusIcon/Accuracy"),
            Keywords.OnHit
        );
    }
}