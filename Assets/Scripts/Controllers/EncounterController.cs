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
    public PlayerController otherPlayer;
    private bool isPlayer1Turn;

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

        player1.ShuffleDeck();
        player2.ShuffleDeck();

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
            otherPlayer = player2;

            player1.maxMana += 1;
            player1.RefillMana();
            player1.ResetBoard();
            DrawCard(currentPlayer);

            player1HandController.HideBoardTargets();
            player1HandController.HidePlayableHand();
            player1HandController.HidePlayableBoard();
            player1HandController.VisualizePlayableHand();
            player1HandController.VisualizePlayableBoard();

        }
        else
        {
            currentPlayer = player2;
            otherPlayer = player1;

            player2.maxMana += 1;
            player2.RefillMana();
            player2.ResetBoard();
            DrawCard(currentPlayer);

            player1HandController.HideBoardTargets();
            player1HandController.HidePlayableHand();
            player1HandController.HidePlayableBoard();
        }
    }

    public void EndTurn()
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

}
