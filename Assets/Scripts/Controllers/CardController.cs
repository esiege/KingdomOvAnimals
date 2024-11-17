using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    // Card Properties
    public string cardName;
    public int manaCost;
    public int health;

    // Owner reference
    public PlayerController owningPlayer;

    // Card status tracking
    public bool isInHand = true;
    public bool isActive = false;
    public bool isInPlay = false;

    // Status Effects
    public bool hasSummoningSickness;
    public bool isFrozen;
    public bool isBuried;
    public bool isDefending;

    // Ability GameObjects
    public GameObject offensiveAbility; // GameObject with an AbilityController component
    public GameObject defensiveAbility; // GameObject with an AbilityController component

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

        UpdateCardUI();
    }

    // Method to update the card's UI elements
    public void UpdateCardUI()
    {
        if (cardNameText != null) cardNameText.text = cardName;
        if (manaCostText != null) manaCostText.text = manaCost.ToString();
        if (healthText != null) healthText.text = health.ToString();
    }

    // Method to manage health
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyCard();
        }
        UpdateCardUI();
    }

    public void Heal(int amount)
    {
        health += amount;
        UpdateCardUI();
    }

    // Methods to handle status effects
    public void SetSummoningSickness(bool status) => hasSummoningSickness = status;
    public void FreezeCard() => isFrozen = true;
    public void UnfreezeCard() => isFrozen = false;
    public void BuryCard() => isBuried = true;
    public void UnburyCard() => isBuried = false;
    public void SetDefending(bool status) => isDefending = status;

    // Methods to activate abilities
    public void ActivateOffensiveAbility(CardController target)
    {
        if (offensiveAbility != null && offensiveAbility.TryGetComponent(out AbilityController abilityController))
        {
            abilityController.Activate(target);
        }
        else
        {
            Debug.LogError("Offensive ability is not set or does not have an AbilityController.");
        }
    }

    public void ActivateDefensiveAbility(CardController target)
    {
        if (defensiveAbility != null && defensiveAbility.TryGetComponent(out AbilityController abilityController))
        {
            abilityController.Activate(target);
        }
        else
        {
            Debug.LogError("Defensive ability is not set or does not have an AbilityController.");
        }
    }

    private void DestroyCard()
    {
        // Perform any additional cleanup here
        Debug.Log($"{cardName} has been destroyed.");
        Destroy(gameObject);
    }
}
