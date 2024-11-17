using UnityEngine;

public class DamageAbility : AbilityController
{
    public int damageAmount = 5; // Amount of damage this ability deals

    public override void Activate(CardController target)
    {
        if (target == null)
        {
            Debug.LogError("No target provided for DamageAbility.");
            return;
        }

        Debug.Log($"{gameObject.name} deals {damageAmount} damage to {target.cardName}.");
        target.TakeDamage(damageAmount);
    }
}
