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
    
    // Network mode flag
    private bool isNetworkGame = false;
    private NetworkGameManager networkGameManager;

    // Initialization method
    void Start()
    {
        // Check if we're in a network game
        networkGameManager = NetworkGameManager.Instance;
        isNetworkGame = networkGameManager != null;
        
        if (isNetworkGame)
        {
            Debug.Log("[EncounterController] Network game detected, waiting for network to start game...");
            // In network mode, wait for NetworkGameManager to call OnNetworkGameStarted
            // But still do initial setup
            InitializeEncounterLocal();
        }
        else
        {
            // Single player mode - initialize normally
            InitializeEncounter();
        }
    }
    
    /// <summary>
    /// Local initialization (deck shuffle, draw cards) - no turn logic
    /// </summary>
    private void InitializeEncounterLocal()
    {
        player.ShuffleDeck();
        opponent.ShuffleDeck();
        StartCoroutine(DrawInitialCards());
    }

    // Method to initialize the encounter (single player mode)
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
    
    /// <summary>
    /// Called by NetworkGameManager when the networked game starts.
    /// </summary>
    public void OnNetworkGameStarted(bool isLocalPlayerFirst)
    {
        Debug.Log($"[EncounterController] Network game started! Local player goes first: {isLocalPlayerFirst}");
        
        // Just set initial state - OnNetworkTurnChanged will handle starting the first turn
        isCurrentPlayerTurn = isLocalPlayerFirst;
        currentPlayer = isLocalPlayerFirst ? player : opponent;
        turnNumber = 1;
        
        // Note: Don't call StartTurnNetwork() here - OnNetworkTurnChanged will handle it
    }
    
    /// <summary>
    /// Called by NetworkGameManager when turn changes over network.
    /// </summary>
    public void OnNetworkTurnChanged(bool isLocalPlayerTurn, int networkTurnNumber)
    {
        Debug.Log($"[EncounterController] Network turn changed. Local turn: {isLocalPlayerTurn}, Turn #: {networkTurnNumber}");
        
        // Sync turn state
        isCurrentPlayerTurn = isLocalPlayerTurn;
        turnNumber = networkTurnNumber;
        currentPlayer = isLocalPlayerTurn ? player : opponent;
        
        // Start the new turn
        StartTurnNetwork();
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

        // In network mode, the turn is started by OnNetworkTurnChanged callback
        // In single-player mode, start the first turn here
        if (!isNetworkGame)
        {
            StartTurn();
        }
    }

    // Method to start a player's turn
    public void StartTurn()
    {
        if (isNetworkGame)
        {
            StartTurnNetwork();
            return;
        }
        
        StartTurnLocal();
    }
    
    /// <summary>
    /// Start turn logic for single player / local mode.
    /// </summary>
    private void StartTurnLocal()
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
    
    /// <summary>
    /// Start turn logic for network mode.
    /// Only the active player performs turn start actions (mana refill, draw).
    /// The opponent's client just updates UI state.
    /// </summary>
    private void StartTurnNetwork()
    {
        Debug.Log($"[EncounterController] Starting network turn. isCurrentPlayerTurn: {isCurrentPlayerTurn}");
        
        if (isCurrentPlayerTurn)
        {
            // It's local player's turn - perform turn start actions
            currentPlayer = player;

            player.maxMana += 1;
            player.RefillMana();
            player.ResetBoard();
            DrawCard(player);

            // Show playable cards
            playerHandController.HideBoardTargets();
            playerHandController.HidePlayableHand();
            playerHandController.HidePlayableBoard();
            playerHandController.VisualizePlayableHand();
            playerHandController.VisualizePlayableBoard();
            
            Debug.Log("[EncounterController] Your turn! Cards are playable.");
        }
        else
        {
            // It's opponent's turn - just update local state, don't perform actions
            // The opponent's client will handle their own mana/draw
            currentPlayer = opponent;

            // Hide all playable indicators - can't play during opponent's turn
            playerHandController.HideBoardTargets();
            playerHandController.HidePlayableHand();
            playerHandController.HidePlayableBoard();
            
            Debug.Log("[EncounterController] Opponent's turn. Waiting...");
        }
    }

    public void EndTurn()
    {
        if (isNetworkGame)
        {
            // In network mode, request end turn through NetworkGameManager
            if (networkGameManager != null)
            {
                networkGameManager.RequestEndTurn();
            }
            return;
        }
        
        // Single player mode
        isCurrentPlayerTurn = !isCurrentPlayerTurn;
        turnNumber++;

        Debug.Log("Turn ended. It is now " + (isCurrentPlayerTurn ? "Player 1's" : "Player 2's") + " turn: " + turnNumber);

        StartTurn();
    }
    
    /// <summary>
    /// Check if it's currently the local player's turn.
    /// </summary>
    public bool IsLocalPlayerTurn()
    {
        if (isNetworkGame && networkGameManager != null)
        {
            return networkGameManager.IsLocalPlayerTurn();
        }
        return isCurrentPlayerTurn;
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
