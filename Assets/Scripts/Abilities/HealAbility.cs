using UnityEngine;

public class HealAbility : AbilityController
{
    public int healAmount = 3; // Amount of health this ability restores

    public override void Activate(CardController target)
    {
        if (target == null)
        {
            Debug.LogError("No target provided for HealAbility.");
            return;
        }

        Debug.Log($"{gameObject.name} heals {healAmount} health for {target.cardName}.");
        target.Heal(healAmount);
    }
}
