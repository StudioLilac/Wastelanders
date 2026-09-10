using UnityEngine;

public class Decohering: StatusEffect
{
    public const string buffName = "Decohering";
    public override void OnHostAssigned(EntityClass host)
    {
        base.OnHostAssigned(host);
        Host.Subscribe<OnEntityTakeDamage>(HandleDamage);
    }

    private void HandleDamage(OnEntityTakeDamage e)
    {
        if (e.DamageTaker == Host && e.Damage > 0)
        {
            Host.AddStacks(Dissonance.buffName, 1);
            Host.UpdateBuffs();
        }
        else if (e.DamageDealer == Host && e.Damage > 0)
        {
            Host.ReduceStacks(Dissonance.buffName, 1);
            Host.UpdateBuffs();
        }
    }
    public override bool ShouldRenderStackCount() => false;
    public override Sprite GetIcon()
    {
        Sprite buffSprite = new GetStatusIcons().Query()?.corruptedIves;
        if (!buffSprite) Debug.LogWarning("Corrutped Ives Buff Sprite is missing");
        return buffSprite;
    }
}
