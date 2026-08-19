namespace UtilClass
{
    public static class Keywords
    {
        public static readonly GlossaryNode OnHit = new(
            "On Hit",
            "An action triggers its On-hit effect when it deals at least 1 damage."
        );

        public static readonly GlossaryNode Crystals = new(
            "Crystals",
            "Crystals grant 1 resonance stack upon dropping to 10, 5, and 0 health.",
            null,
            null,
            StatusEffects.Resonance
        );
    }

    public static class StatusEffects
    {
        public static readonly GlossaryNode Accuracy = new(
            global::Accuracy.buffName,
            BuffExplainer.WeaponExplanation.PISTOL_EXPLANATION.ExplanationText,
            icons => icons.accuracy
        );

        public static readonly GlossaryNode Flow = new(
            global::Flow.buffName,
            BuffExplainer.WeaponExplanation.STAFF_EXPLANATION.ExplanationText,
            icons => icons.flow
        );

        public static readonly GlossaryNode Wound = new(
            global::Wound.buffName,
            BuffExplainer.WeaponExplanation.AXE_EXPLANATION.ExplanationText,
            icons => icons.wound
        );

        public static readonly GlossaryNode Resonance = new(
            Resonate.buffName,
            BuffExplainer.WeaponExplanation.RESONANCE_EXPLANATION.ExplanationText,
            icons => icons.resonance
        );
    }
}