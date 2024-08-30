using UnityEngine;
using TMPro; // Required for TextMeshPro components
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Player Properties
    public int maxHealth;
    public int currentLives;
    public int maxLives = 3; // Default maximum lives
    public List<CardController> deck; // Player's deck of cards

    // Overworld Progression
    public int currentStage;

    // UI Elements
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI currentLivesText;
    public TextMeshProUGUI currentStageText;
    public GameObject deckUIContainer; // UI container for displaying the player's deck

    // Card Positions in Hand
    public List<GameObject> cardPositions; // Positions where cards will be placed in hand

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

    // Method to add a card to the player's hand and position it
    public void AddCardToHand(CardController newCard, int handPositionIndex)
    {
        if (handPositionIndex < cardPositions.Count)
        {
            // Instantiate a new instance of the card prefab
            GameObject cardObject = Instantiate(newCard.gameObject, cardPositions[handPositionIndex].transform.position, Quaternion.identity);

            // Set the instantiated card's parent to the hand UI container (optional)
            cardObject.transform.SetParent(cardPositions[handPositionIndex].transform);

            // Add the card to the hand and update its UI
            deck.Add(newCard);
            newCard.UpdateCardUI();

            // Enable interaction on the card
            EncounterController encounterController = FindObjectOfType<EncounterController>();
            encounterController.MakeCardInteractable(newCard, this);
        }
        else
        {
            Debug.LogError("Hand position index is out of range.");
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
