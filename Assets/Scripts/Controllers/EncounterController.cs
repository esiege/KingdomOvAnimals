using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterController : MonoBehaviour
{
    // Player References
    public PlayerController player1;
    public PlayerController player2;

    // Hand Controllers for both players
    public HandController player1HandController;
    public HandController player2HandController;

    // Turn Management
    public PlayerController currentPlayer;
    private bool isPlayer1Turn;

    // Board State
    public List<CardController> player1Board;
    public List<CardController> player2Board;

    public int turnNumber = 1;

    // Maximum hand size
    public int maxHandSize = 5;

    // Initialization method
    void Start()
    {
        InitializeEncounter();
    }

    // Method to initialize the encounter
    public void InitializeEncounter()
    {

        // Initialize turn order
        isPlayer1Turn = true;
        currentPlayer = player1;

        // Clear the boards
        player1Board = new List<CardController>();
        player2Board = new List<CardController>();

        // Start with both players drawing 3 cards with a delay
        StartCoroutine(DrawInitialCards());
    }

    // Coroutine to draw 3 cards for each player with a 1-second delay
    private IEnumerator DrawInitialCards()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard(player1);
            yield return new WaitForSeconds(0.1f);

            DrawCard(player2);
            yield return new WaitForSeconds(0.1f);
        }

        StartTurn(); // Start the first turn after drawing initial cards
    }

    // Method to start a player's turn
    public void StartTurn()
    {
        if (isPlayer1Turn)
        {
            currentPlayer = player1;

            player1.maxMana += 1;
            player1.currentMana = player1.maxMana;
        }
        else
        {
            currentPlayer = player2;

            player2.maxMana += 1;
            player2.currentMana = player2.maxMana;
        }

        DrawCard(currentPlayer);


    }

    public void endTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        turnNumber++;

        Debug.Log("Turn ended. It is now " + (isPlayer1Turn ? "Player 1's" : "Player 2's") + " turn: " + turnNumber);

        StartTurn();
    }

    private void DrawCard(PlayerController player)
    {
        HandController handController = player == player1 ? player1HandController : player2HandController;

        if (handController.GetHand().Count >= maxHandSize)
        {
            Debug.Log($"{player.name} cannot draw more cards. Hand is full.");
            return; // Hand is full, cannot draw more cards
        }

        if (player.deck.Count > 0)
        {
            // Draw the top card from the deck
            CardController drawnCard = player.deck[0];
            player.deck.RemoveAt(0); // Remove the card from the deck

            // Set the owning player on the card
            drawnCard.owningPlayer = player;

            // Add the card to the player's hand using the HandController
            handController.AddCardToHand(drawnCard);

            Debug.Log($"{player.name} draws {drawnCard.cardName}.");
        }
        else
        {
            Debug.Log($"{player.name} has no more cards to draw.");
        }
    }

    // Method to play a card (called when a card is played)
    public void PlayCard(CardController card, PlayerController player)
    {
        List<CardController> playerBoard = player == player1 ? player1Board : player2Board;
        HandController handController = player == player1 ? player1HandController : player2HandController;

        if (currentPlayer.currentMana >= card.manaCost)
        {
            playerBoard.Add(card);
            handController.RemoveCardFromHand(card); // Remove the card from hand when played

            currentPlayer.currentMana -= card.manaCost;

            // Additional logic for placing the card on the board, applying effects, etc.
            card.SetSummoningSickness(true); // Card cannot act until the next turn
        }
        else
        {
            Debug.Log($"{player.name} does not have enough mana to play {card.cardName}.");
        }
    }

    // Method to end the current player's turn
    public void EndTurn()
    {
        ResolveBoardState();

        // Switch turn
        isPlayer1Turn = !isPlayer1Turn;

        StartTurn();
    }

    // Method to resolve the board state after each turn
    private void ResolveBoardState()
    {
        // Logic to resolve card interactions, such as combat
        // This could include attacking, triggering abilities, etc.

        // Example: Basic combat resolution
        if (player1Board.Count > 0 && player2Board.Count > 0)
        {
            CardController player1Card = player1Board[0]; // Assuming front-most card for simplicity
            CardController player2Card = player2Board[0]; // Assuming front-most card for simplicity

            // Resolve damage between the front cards
            player1Card.TakeDamage(player2Card.health);
            player2Card.TakeDamage(player1Card.health);

            // Check if any cards should be removed from the board
            if (player1Card.health <= 0)
            {
                player1Board.Remove(player1Card);
                Destroy(player1Card.gameObject); // Remove the card from the game
            }
            if (player2Card.health <= 0)
            {
                player2Board.Remove(player2Card);
                Destroy(player2Card.gameObject); // Remove the card from the game
            }
        }
    }

    // Method to check if the encounter is over
    private void CheckEncounterOutcome()
    {
        // Logic to determine if either player has lost all their cards or life, ending the encounter
        if (player1Board.Count == 0 || player2.currentLives <= 0)
        {
            Debug.Log("Player 2 wins the encounter!");
            // Handle Player 2 victory
        }
        else if (player2Board.Count == 0 || player1.currentLives <= 0)
        {
            Debug.Log("Player 1 wins the encounter!");
            // Handle Player 1 victory
        }
    }
}
