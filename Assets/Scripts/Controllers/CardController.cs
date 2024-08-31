using UnityEngine;
using TMPro; // Required for TextMeshPro components
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    // Card Properties
    public string cardName;
    public int manaCost;
    public int health;

    // Owner reference
    public PlayerController owningPlayer;

    // Status Effects
    public bool hasSummoningSickness;
    public bool isFrozen;
    public bool isBuried;
    public bool isDefending;

    // List of active abilities
    private List<IAbility> offensiveAbilities;
    private List<IAbility> defensiveAbilities;

    // UI Elements
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI healthText;

    // Initialization method
    public void InitializeCard(string name, int cost, int initialHealth)
    {
        cardName = name;
        manaCost = cost;
        health = initialHealth;
        hasSummoningSickness = true;
        isFrozen = false;
        isBuried = false;
        isDefending = false;

        offensiveAbilities = new List<IAbility>();
        defensiveAbilities = new List<IAbility>();

        UpdateCardUI(); // Update the UI after initialization
    }

    // Method to update the card's UI elements
    public void UpdateCardUI()
    {
        cardNameText.text = cardName;
        manaCostText.text = manaCost.ToString();
        healthText.text = health.ToString();
    }

    // Method to manage health
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyCard();
        }
        UpdateCardUI(); // Update UI after taking damage
    }

    public void Heal(int amount)
    {
        health += amount;
        UpdateCardUI(); // Update UI after healing
    }

    // Methods to handle status effects
    public void SetSummoningSickness(bool status)
    {
        hasSummoningSickness = status;
    }

    public void FreezeCard()
    {
        isFrozen = true;
    }

    public void UnfreezeCard()
    {
        isFrozen = false;
    }

    public void BuryCard()
    {
        isBuried = true;
    }

    public void UnburyCard()
    {
        isBuried = false;
    }

    public void SetDefending(bool status)
    {
        isDefending = status;
    }

    // Method to activate offensive abilities
    public void ActivateOffensiveAbility(int index, CardController target)
    {
        if (index >= 0 && index < offensiveAbilities.Count)
        {
            offensiveAbilities[index].Activate(target);
        }
    }

    // Method to activate defensive abilities
    public void ActivateDefensiveAbility(int index, CardController target)
    {
        if (index >= 0 && index < defensiveAbilities.Count)
        {
            defensiveAbilities[index].Activate(target);
        }
    }

    // Method to add abilities
    public void AddOffensiveAbility(IAbility ability)
    {
        offensiveAbilities.Add(ability);
    }

    public void AddDefensiveAbility(IAbility ability)
    {
        defensiveAbilities.Add(ability);
    }

    // Method to destroy the card
    private void DestroyCard()
    {
        // Perform any additional cleanup here
        Destroy(gameObject);
    }
}

// Interface for abilities
public interface IAbility
{
    void Activate(CardController target);
}
