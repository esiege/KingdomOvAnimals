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

    public List<CardController> deck = new List<CardController>();
    public List<CardController> board = new List<CardController>();

    // Network Reference (optional - for multiplayer mode)
    [Header("Network")]
    [Tooltip("Set at runtime by EncounterController when NetworkPlayer links.")]
    [System.NonSerialized]
    public NetworkPlayer networkPlayer;

    // UI Elements
    public TextMeshProUGUI currentHealthText;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI currentManaText;
    public TextMeshProUGUI maxManaText;
    public TextMeshProUGUI currentLivesText;
    public TextMeshProUGUI currentStageText;
    public GameObject deckUIContainer; // UI container for displaying the player's deck

    // Initialization method
    public void Awake()
    {
        UpdatePlayerUI();
    }

    // Method to update the player's UI elements
    public void UpdatePlayerUI()
    {
        Debug.Log($"[PlayerController] {gameObject.name} UpdatePlayerUI: health={currentHealth}, mana={currentMana}, " +
                  $"healthText={(currentHealthText != null ? "assigned" : "NULL")}, " +
                  $"manaText={(currentManaText != null ? "assigned" : "NULL")}");
        
        if (currentHealthText != null) currentHealthText.text = $"{currentHealth}";
        if (currentManaText != null) currentManaText.text = $"{currentMana}";

        UpdateDeckUI();
    }

    public void ResetBoard()
    {
        // Reset summoning sickness and tapped status for cards on the board
        foreach (CardController card in board)
        {
            card.SetSummoningSickness(false);
            card.UntapCard();
        }
    }


    // Method to add a card to the board state
    public void AddCardToBoard(CardController card)
    {
        if (card == null)
        {
            Debug.LogError("Attempted to add a null card to the board.");
            return;
        }

        if (board.Contains(card))
        {
            Debug.LogWarning($"{card.cardName} is already on the board.");
            return;
        }

        board.Add(card);
        Debug.Log($"{card.cardName} has been added to the board.");
    }

    // Method to remove a card from the board state (e.g., when it dies)
    public void RemoveCardFromBoard(CardController card)
    {
        if (card == null)
        {
            Debug.LogError("Attempted to remove a null card from the board.");
            return;
        }

        if (!board.Contains(card))
        {
            Debug.LogWarning($"{card.cardName} is not on the board.");
            return;
        }

        board.Remove(card);
        Debug.Log($"{card.cardName} has been removed from the board.");

        Destroy(card.gameObject);
    }


    // Method to update the deck UI
    private void UpdateDeckUI()
    {
    }

    // Method to manage health
    public void TakeDamage(int damage)
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdTakeDamage(damage);
            return; // Network callback will update local values
        }
        
        // Single-player fallback
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // game over!
        }

        UpdatePlayerUI();
    }

    public void Heal(int amount)
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdHeal(amount);
            return;
        }
        
        // Single-player fallback
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
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdSpendMana(amount);
            return;
        }
        
        // Single-player fallback
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

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            // Swap current card with a card at a random index
            CardController temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        Debug.Log("Deck shuffled!");
    }

    // Method to manage mana
    public void RefillMana()
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdRefillMana();
            return;
        }
        
        // Single-player fallback
        currentMana = maxMana;
        UpdatePlayerUI();
    }

    public void RegainMana(int amount)
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            // NetworkPlayer doesn't have RegainMana, use SpendMana with negative? No, just update locally for now
            // TODO: Add CmdRegainMana to NetworkPlayer if needed
        }
        
        // Single-player fallback
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        UpdatePlayerUI();
    }

    public void ResetMana()
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdRefillMana();
            return;
        }
        
        // Single-player fallback
        currentMana = maxMana;
        UpdatePlayerUI();
    }

    

    public void AddCardToDeck(CardController newCard)
    {
        deck.Add(newCard);
        UpdatePlayerUI();
    }

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

   



    public void IncreaseMaxMana(int amount)
    {
        // Route through network if available
        if (networkPlayer != null)
        {
            networkPlayer.CmdIncreaseMaxMana(amount);
            return;
        }
        
        // Single-player fallback
        maxMana += amount;
        currentMana = maxMana; // Reset current mana to the new maximum
        UpdatePlayerUI();
    }
    
    /// <summary>
    /// Check if player has enough mana (works in both single and multiplayer)
    /// </summary>
    public bool HasEnoughMana(int cost)
    {
        if (networkPlayer != null)
        {
            return networkPlayer.HasEnoughMana(cost);
        }
        return currentMana >= cost;
    }
}
