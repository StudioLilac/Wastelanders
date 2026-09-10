using UnityEngine;

public record OnDissonanceDeath(EntityClass Victim) : IEvent;

public class Dissonance: StatusEffect
{
    public const string buffName = "Dissonance";

    public override void OnHostAssigned(EntityClass host)
    {
        base.OnHostAssigned(host);
        Host.Subscribe<OnEntityTakeDamage>(HandleDamage);
    }

    private void HandleDamage(OnEntityTakeDamage e)
    {
        if (e.DamageTaker == Host && e.Damage > 0)
        {
            GainStacks(1);
            CheckLethality();
            Host.UpdateBuffs();
        }
        else if (e.DamageDealer == Host && e.Damage > 0)
        {
            LoseStacks(1);
            Host.UpdateBuffs();
        }
    }

    private void CheckLethality()
    {
        if (Stacks > Host.Health)
        {
            new OnDissonanceDeath(Host).Invoke();
            Host.StartCoroutine(Host.Die());
        }
    }

    public override bool ShouldRenderBuffIcon() => Stacks > 0;
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
