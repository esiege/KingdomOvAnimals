using UnityEngine;

public class HealAbility : AbilityController
{
    public int healAmount = 5; // The amount of health to restore

    // Activate ability on a CardController
    public override void Activate(CardController target)
    {
        if (target != null)
        {
            Debug.Log($"Healing {target.cardName} for {healAmount} health.");
            target.Heal(healAmount);
        }
        else
        {
            Debug.LogError("No target CardController provided for HealAbility.");
        }
    }

    // Activate ability on a PlayerController
    public override void Activate(PlayerController target)
    {
        if (target != null)
        {
            Debug.Log($"Healing player {target.name} for {healAmount} health.");
            target.Heal(healAmount);
        }
        else
        {
            Debug.LogError("No target PlayerController provided for HealAbility.");
        }
    }
}
