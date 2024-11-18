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
    public bool isUsed = false;
    public bool isFlipped = false;
    public bool isInPlay = false;

    // Status Effects
    public bool hasSummoningSickness = true;
    public bool isFrozen;
    public bool isBuried;
    public bool isDefending;

    // Ability GameObjects
    public GameObject offensiveAbility; // GameObject with an AbilityController on a child
    public GameObject supportAbility; // GameObject with an AbilityController on a child

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
        Debug.Log($"{cardName} takes {damage} damage. Current health: {health} -> {health - damage}");

        health -= damage;
        if (health <= 0)
        {
            DestroyCard();
        }
        else
        {
            UpdateCardUI();
        }
    }

    public void Heal(int amount)
    {
        Debug.Log($"{cardName} heals {amount}. Current health: {health} -> {health + amount}");

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

    // Methods to activate abilities on a CardController target
    public void ActivateOffensiveAbility(CardController target)
    {
        if (offensiveAbility != null)
        {
            AbilityController abilityController = offensiveAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Offensive ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Offensive ability is not set.");
        }
    }

    public void ActivateDefensiveAbility(CardController target)
    {
        if (supportAbility != null)
        {
            AbilityController abilityController = supportAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Support ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Support ability is not set.");
        }
    }

    // Methods to activate abilities on a PlayerController target
    public void ActivateOffensiveAbility(PlayerController target)
    {
        if (offensiveAbility != null)
        {
            AbilityController abilityController = offensiveAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Offensive ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Offensive ability is not set.");
        }
    }

    public void ActivateDefensiveAbility(PlayerController target)
    {
        if (supportAbility != null)
        {
            AbilityController abilityController = supportAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Support ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Support ability is not set.");
        }
    }

    private void DestroyCard()
    {
        Debug.Log($"{cardName} has been destroyed.");
        Destroy(gameObject);
    }
}
