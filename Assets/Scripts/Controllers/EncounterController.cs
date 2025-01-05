using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterController : MonoBehaviour
{
    // Player References
    public PlayerController player;
    public PlayerController opponent;

    // Hand Controllers for both players
    public HandController playerHandController;
    public HandController opponentHandController;

    // Turn Management
    public PlayerController currentPlayer;

    private bool isCurrentPlayerTurn;

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
        isCurrentPlayerTurn = true;
        currentPlayer = player;

        player.ShuffleDeck();
        opponent.ShuffleDeck();

        // Start with both players drawing 3 cards with a delay
        StartCoroutine(DrawInitialCards());
    }

    // Coroutine to draw 3 cards for each player with a 1-second delay
    private IEnumerator DrawInitialCards()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard(player);
            yield return new WaitForSeconds(0.1f);

            DrawCard(opponent);
            yield return new WaitForSeconds(0.1f);
        }

        StartTurn(); // Start the first turn after drawing initial cards
    }

    // Method to start a player's turn
    public void StartTurn()
    {
        if (isCurrentPlayerTurn)
        {
            currentPlayer = player;

            player.maxMana += 1;
            player.RefillMana();
            player.ResetBoard();
            DrawCard(currentPlayer);

            playerHandController.HideBoardTargets();
            playerHandController.HidePlayableHand();
            playerHandController.HidePlayableBoard();
            playerHandController.VisualizePlayableHand();
            playerHandController.VisualizePlayableBoard();

        }
        else
        {
            currentPlayer = opponent;

            opponent.maxMana += 1;
            opponent.RefillMana();
            opponent.ResetBoard();
            DrawCard(currentPlayer);

            playerHandController.HideBoardTargets();
            playerHandController.HidePlayableHand();
            playerHandController.HidePlayableBoard();
        }
    }

    public void EndTurn()
    {
        isCurrentPlayerTurn = !isCurrentPlayerTurn;
        turnNumber++;

        Debug.Log("Turn ended. It is now " + (isCurrentPlayerTurn ? "Player 1's" : "Player 2's") + " turn: " + turnNumber);

        StartTurn();
    }

    private void DrawCard(PlayerController player)
    {
        HandController handController = player == this.player ? playerHandController : opponentHandController;

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

}
