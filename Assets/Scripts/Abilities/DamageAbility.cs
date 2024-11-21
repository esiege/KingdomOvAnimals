using UnityEngine;
using TMPro;

public class DamageAbility : AbilityController
{
    public int damageAmount = 5; // Amount of damage this ability deals

    // UI Elements
    public TextMeshProUGUI damageAmountText;

    void Start()
    {
        UpdateUI(); // Initialize the UI with the damageAmount value
    }

    // Update the UI with the current damageAmount
    public void UpdateUI()
    {
        if (damageAmountText != null)
        {
            damageAmountText.text = $"{damageAmount}";
        }
        else
        {
            Debug.LogWarning("DamageAmountText is not assigned in DamageAbility.");
        }
    }

    // Activate ability on a CardController
    public override void Activate(CardController target)
    {
        if (target != null)
        {
            Debug.Log($"Dealing {damageAmount} damage to card: {target.cardName}.");
            target.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogError("No target CardController provided for DamageAbility.");
        }
    }

    // Activate ability on a PlayerController
    public override void Activate(PlayerController target)
    {
        if (target != null)
        {
            Debug.Log($"Dealing {damageAmount} damage to player: {target.name}.");
            target.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogError("No target PlayerController provided for DamageAbility.");
        }
    }
}
