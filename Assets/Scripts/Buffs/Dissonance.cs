using UnityEngine;

public record OnDissonanceDeath(EntityClass Victim) : IEvent;

public class Dissonance: StatusEffect
{
    public const string buffName = "Dissonance";

    public override void GainStacks(int stacks)
    {
        base.GainStacks(stacks);
        CheckLethality();

    }
    private void CheckLethality()
    {
        if (Stacks > Host.Health)
        {
            new OnDissonanceDeath(Host).Invoke();
            Host.StartCoroutine(Host.Die());
        }
    }

    public override bool ShouldRenderBuffIcon() => Stacks >= 0;
    public override void ApplyStacks(ActionClass.RolledStats dup)
    {
        dup.CeilingBuffs -= this.buffStacks;
    }

    public override Sprite GetIcon()
    {
        Sprite buffSprite = new GetStatusIcons().Query()?.dissonance;
        if (!buffSprite) Debug.LogWarning("Dissonance Buff Sprite is missing");
        return buffSprite;
    }
}
