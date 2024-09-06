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
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI currentLivesText;
    public TextMeshProUGUI currentStageText;
    public GameObject deckUIContainer; // UI container for displaying the player's deck

    // Initialization method
    public void InitializePlayer(int health)
    {
        maxHealth = health;
        currentLives = maxLives;
        currentStage = 1; // Start at the first stage
        deck = new List<CardController>();

        UpdatePlayerUI();
    }

    // Method to update the player's UI elements
    public void UpdatePlayerUI()
    {
        maxHealthText.text = $"Max Health: {maxHealth}";
        currentLivesText.text = $"Lives: {currentLives}";
        currentStageText.text = $"Stage: {currentStage}";

        UpdateDeckUI();
    }

    // Method to update the deck UI
    private void UpdateDeckUI()
    {
        foreach (Transform child in deckUIContainer.transform)
        {
            Destroy(child.gameObject); // Clear existing UI
        }

        foreach (CardController card in deck)
        {
            GameObject cardUI = Instantiate(card.gameObject, deckUIContainer.transform);
            cardUI.GetComponent<CardController>().UpdateCardUI(); // Update UI for each card
        }
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

    // Method to increase max health
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        UpdatePlayerUI();
    }
}
