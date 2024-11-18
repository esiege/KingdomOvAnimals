using UnityEngine;
using TMPro; // Required for TextMeshPro components
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Player Properties
    public int currentHealth;
    public int maxHealth;

    public int currentMana;
    public int maxMana;

    public int currentLives;
    public int maxLives = 3;

    public List<CardController> deck;

    // Overworld Progression
    public int currentStage;

    // UI Elements
    public TextMeshProUGUI currentHealthText;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI currentManaText;
    public TextMeshProUGUI maxManaText;
    public TextMeshProUGUI currentLivesText;
    public TextMeshProUGUI currentStageText;
    public GameObject deckUIContainer; // UI container for displaying the player's deck

    // Initialization method
    public void InitializePlayer(int health, int mana)
    {
        maxHealth = health;
        currentHealth = health;
        maxMana = mana;
        currentMana = mana;
        currentLives = maxLives;
        currentStage = 1; // Start at the first stage
        deck = new List<CardController>();

        UpdatePlayerUI();
    }

    // Method to update the player's UI elements
    public void UpdatePlayerUI()
    {
        if (currentHealthText != null) currentHealthText.text = $"{currentHealth}";
        if (maxHealthText != null) maxHealthText.text = $"Max Health: {maxHealth}";
        if (currentManaText != null) currentManaText.text = $"{currentMana}";
        if (maxManaText != null) maxManaText.text = $"Max Mana: {maxMana}";
        if (currentLivesText != null) currentLivesText.text = $"Lives: {currentLives}/{maxLives}";
        if (currentStageText != null) currentStageText.text = $"Stage: {currentStage}";

        UpdateDeckUI();
    }

    // Method to update the deck UI
    private void UpdateDeckUI()
    {
    }

    // Method to manage health
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            LoseLife();
        }

        UpdatePlayerUI();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdatePlayerUI();
    }

    // Method to manage mana
    public void SpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
        }
        else
        {
            Debug.LogError("Not enough mana!");
        }

        UpdatePlayerUI();
    }
    // Method to manage mana
    public void RefillMana()
    {
        currentMana = maxMana;
        UpdatePlayerUI();
    }

    public void RegainMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        UpdatePlayerUI();
    }

    public void ResetMana()
    {
        currentMana = maxMana;
        UpdatePlayerUI();
    }

    // Method to manage player lives
    public void LoseLife()
    {
        currentLives--;
        UpdatePlayerUI();

        if (currentLives <= 0)
        {
            // Handle player defeat (e.g., end the game, restart, etc.)
            HandlePlayerDefeat();
        }
        else
        {
            Debug.Log("Player lost a life! Reviving...");
            currentHealth = maxHealth; // Reset health for the next life
        }
    }

    public void GainLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            UpdatePlayerUI();
        }
    }

    // Method to handle player defeat
    private void HandlePlayerDefeat()
    {
        Debug.Log("Player defeated! Game over.");
        // Implement game over logic here
    }

    // Method to progress to the next stage
    public void ProgressToNextStage()
    {
        currentStage++;
        UpdatePlayerUI();
        // Implement logic for transitioning to the next stage
    }

    // Method to add a card to the player's deck
    public void AddCardToDeck(CardController newCard)
    {
        deck.Add(newCard);
        UpdatePlayerUI();
    }

    // Method to remove a card from the player's deck
    public void RemoveCardFromDeck(CardController card)
    {
        if (deck.Contains(card))
        {
            deck.Remove(card);
            UpdatePlayerUI();
        }
        else
        {
            Debug.LogWarning("Attempted to remove a card that is not in the deck.");
        }
    }

    // Method to increase max health
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Increase current health proportionally
        UpdatePlayerUI();
    }

    // Method to increase max mana
    public void IncreaseMaxMana(int amount)
    {
        maxMana += amount;
        currentMana = maxMana; // Reset current mana to the new maximum
        UpdatePlayerUI();
    }
}
