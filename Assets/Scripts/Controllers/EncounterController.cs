using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    
    // Turn Indicator UI
    [Header("Turn Indicator")]
    public TextMeshProUGUI turnIndicatorText;
    
    [Header("Disconnect UI")]
    public GameObject disconnectPanel;
    public TextMeshProUGUI disconnectStatusText;
    
    // Network mode flag
    private bool isNetworkGame = false;
    private bool networkInitialized = false;
    private NetworkGameManager networkGameManager;
    private bool isGamePaused = false;

    // Initialization method
    void Start()
    {
        // Initialize CardLibrary early to capture all cards before they're drawn/played
        CardLibrary.EnsureInitialized();
        
        // Hide disconnect panel at start
        if (disconnectPanel != null)
            disconnectPanel.SetActive(false);
            
        // Check if we're in a network game
        networkGameManager = NetworkGameManager.Instance;
        isNetworkGame = networkGameManager != null;
        
        if (isNetworkGame)
        {
            Debug.Log("[EncounterController] Network game detected, waiting for network to start game...");
            // In network mode, wait for NetworkGameManager to call OnNetworkGameStarted
            // DON'T shuffle/draw here - wait for seed from server
        }
        else
        {
            // Single player mode - initialize normally
            InitializeEncounter();
        }
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
    public void OnNetworkGameStarted(bool isLocalPlayerFirst, int shuffleSeed)
    {
        Debug.Log($"[EncounterController] Network game started! Local player goes first: {isLocalPlayerFirst}, Seed: {shuffleSeed}");
        
        // Shuffle decks with synchronized seeds
        // IMPORTANT: First player (host) always uses seed, second player always uses seed+1
        // This ensures both clients see the same deck orders regardless of perspective
        if (isLocalPlayerFirst)
        {
            // We go first - our deck uses seed, opponent uses seed+1
            player.ShuffleDeckWithSeed(shuffleSeed);
            opponent.ShuffleDeckWithSeed(shuffleSeed + 1);
        }
        else
        {
            // Opponent goes first - they use seed, we use seed+1
            opponent.ShuffleDeckWithSeed(shuffleSeed);
            player.ShuffleDeckWithSeed(shuffleSeed + 1);
        }
        
        // Draw initial cards (both clients will draw same cards due to same seed)
        StartCoroutine(DrawInitialCards());
        
        // Set initial state - OnNetworkTurnChanged will handle starting the first turn
        isCurrentPlayerTurn = isLocalPlayerFirst;
        currentPlayer = isLocalPlayerFirst ? player : opponent;
        turnNumber = 1;
        networkInitialized = true;
        
        // Note: Don't call StartTurnNetwork() here - OnNetworkTurnChanged will handle it
    }
    
    /// <summary>
    /// Called by NetworkGameManager when turn changes over network.
    /// </summary>
    public void OnNetworkTurnChanged(bool isLocalPlayerTurn, int networkTurnNumber)
    {
        Debug.Log($"[EncounterController] Network turn changed. Local turn: {isLocalPlayerTurn}, Turn #: {networkTurnNumber}, initialized: {networkInitialized}");
        
        // Ignore turn changes before the encounter is initialized
        if (!networkInitialized)
        {
            Debug.Log("[EncounterController] Ignoring turn change - not initialized yet");
            return;
        }
        
        // Sync turn state
        isCurrentPlayerTurn = isLocalPlayerTurn;
        turnNumber = networkTurnNumber;
        currentPlayer = isLocalPlayerTurn ? player : opponent;
        
        // Turn 1 is handled by OnNetworkGameStarted (initial draw + mana)
        // Only call StartTurnNetwork for turn 2+
        if (networkTurnNumber > 1)
        {
            StartTurnNetwork();
        }
        else
        {
            // Turn 1 - just update playable UI for whoever goes first
            if (isLocalPlayerTurn)
            {
                playerHandController.VisualizePlayableHand();
                playerHandController.VisualizePlayableBoard();
                Debug.Log("[EncounterController] Turn 1 - your turn! Cards are playable.");
            }
            else
            {
                playerHandController.HidePlayableHand();
                playerHandController.HidePlayableBoard();
                Debug.Log("[EncounterController] Turn 1 - opponent's turn. Waiting...");
            }
        }
        
        // Update turn indicator UI
        UpdateTurnIndicator();
    }
    
    /// <summary>
    /// Restore turn state after reconnection (without triggering turn effects).
    /// </summary>
    public void RestoreTurnState(bool isLocalPlayerTurn, int networkTurnNumber)
    {
        Debug.Log($"[EncounterController] Restoring turn state: Local turn: {isLocalPlayerTurn}, Turn #: {networkTurnNumber}");
        
        // Set turn state without triggering new turn effects
        isCurrentPlayerTurn = isLocalPlayerTurn;
        turnNumber = networkTurnNumber;
        currentPlayer = isLocalPlayerTurn ? player : opponent;
        networkInitialized = true; // Enable turn processing
        
        // Update turn indicator UI
        UpdateTurnIndicator();
    }
    
    /// <summary>
    /// Updates the turn indicator UI to show whose turn it is.
    /// </summary>
    private void UpdateTurnIndicator()
    {
        if (turnIndicatorText == null)
        {
            // Try to find it dynamically if not assigned
            var indicatorObj = GameObject.Find("TurnIndicatorText");
            if (indicatorObj != null)
            {
                turnIndicatorText = indicatorObj.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (turnIndicatorText != null)
        {
            if (isCurrentPlayerTurn)
            {
                turnIndicatorText.text = $"YOUR TURN (Turn {turnNumber})";
                turnIndicatorText.color = Color.green;
            }
            else
            {
                turnIndicatorText.text = $"OPPONENT'S TURN (Turn {turnNumber})";
                turnIndicatorText.color = Color.red;
            }
        }
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
        else
        {
            // Network mode: Now that cards are drawn, visualize playable cards for Turn 1
            // This is needed because OnNetworkTurnChanged fires before cards are drawn
            if (isCurrentPlayerTurn)
            {
                playerHandController.VisualizePlayableHand();
                playerHandController.VisualizePlayableBoard();
            }
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
    /// Both clients execute draw for current player (decks are synced via seed).
    /// </summary>
    private void StartTurnNetwork()
    {
        Debug.Log($"[EncounterController] Starting network turn. isCurrentPlayerTurn: {isCurrentPlayerTurn}");
        
        // BOTH clients draw for the current player (decks are in same order due to seed)
        // This keeps hands in sync on both sides
        if (isCurrentPlayerTurn)
        {
            // It's local player's turn
            currentPlayer = player;
            
            // Use network commands for mana (so it syncs to opponent)
            if (player.networkPlayer != null)
            {
                Debug.Log($"[EncounterController] Calling CmdIncreaseMaxMana for {player.networkPlayer.PlayerName.Value}");
                player.networkPlayer.CmdIncreaseMaxMana(1);  // This also refills mana
            }
            else
            {
                Debug.LogWarning("[EncounterController] player.networkPlayer is NULL - using local fallback");
                player.maxMana += 1;
                player.RefillMana();
            }
            
            player.ResetBoard();
            DrawCard(player);  // Local player draws for themselves

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
            // It's opponent's turn - draw for opponent (keeps decks in sync)
            currentPlayer = opponent;
            
            // Reset opponent's board (clear summoning sickness, untap) so server state matches
            // This is important for server-side validation of abilities
            opponent.ResetBoard();
            
            // Draw for opponent so their hand stays in sync
            DrawCard(opponent);

            // Hide all playable indicators - can't play during opponent's turn
            playerHandController.HideBoardTargets();
            playerHandController.HidePlayableHand();
            playerHandController.HidePlayableBoard();
            
            Debug.Log("[EncounterController] Opponent's turn. Waiting...");
        }
        
        // Update turn indicator UI
        UpdateTurnIndicator();
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
    /// Returns false if game is paused (opponent disconnected).
    /// </summary>
    public bool IsLocalPlayerTurn()
    {
        // Can't take actions while game is paused
        if (isGamePaused)
        {
            return false;
        }
        
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
    
    #region Disconnect Handling
    
    /// <summary>
    /// Called when opponent disconnects from the game.
    /// </summary>
    public void OnOpponentDisconnected(float gracePeriod)
    {
        Debug.Log($"[EncounterController] OnOpponentDisconnected called! Waiting {gracePeriod}s for reconnect...");
        Debug.Log($"[EncounterController] disconnectPanel is null: {disconnectPanel == null}, disconnectStatusText is null: {disconnectStatusText == null}");
        isGamePaused = true;
        
        // Show disconnect UI
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
            Debug.Log("[EncounterController] DisconnectPanel activated");
        }
        else
        {
            Debug.LogWarning("[EncounterController] disconnectPanel is null! Run 'Tools > Kingdom Ov Animals > Add Disconnect UI' in Unity Editor.");
        }
        
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = $"Opponent disconnected!\nWaiting for reconnect... {gracePeriod:F0}s";
        }
        
        // Hide playable indicators
        playerHandController?.HidePlayableHand();
        playerHandController?.HidePlayableBoard();
        playerHandController?.HideBoardTargets();
    }
    
    /// <summary>
    /// Called every frame while waiting for reconnect to update timer display.
    /// </summary>
    public void UpdateDisconnectTimer(float remainingTime)
    {
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = $"Opponent disconnected!\nWaiting for reconnect... {remainingTime:F0}s";
        }
    }
    
    /// <summary>
    /// Called when opponent reconnects.
    /// </summary>
    public void OnOpponentReconnected()
    {
        Debug.Log("[EncounterController] Opponent reconnected! Resuming game...");
        isGamePaused = false;
        
        // Hide disconnect UI
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
        
        // Re-show playable indicators if it's our turn
        if (isCurrentPlayerTurn)
        {
            playerHandController?.VisualizePlayableHand();
            playerHandController?.VisualizePlayableBoard();
        }
    }
    
    /// <summary>
    /// Called when opponent forfeits (grace period expired).
    /// </summary>
    public void OnOpponentForfeited()
    {
        Debug.Log("[EncounterController] Opponent forfeited! You win!");
        
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = "Opponent forfeited!\n\nYOU WIN!\n\nReturning to menu...";
        }
        
        // Could trigger victory animation/sound here
    }
    
    /// <summary>
    /// Called when the host disconnects (client only). Shows waiting UI.
    /// </summary>
    public void OnHostDisconnected(float gracePeriod)
    {
        Debug.Log($"[EncounterController] Host disconnected! Waiting {gracePeriod}s for reconnect...");
        isGamePaused = true;
        
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
        }
        
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = $"Host disconnected!\nWaiting for reconnect... {gracePeriod:F0}s";
        }
        
        // Hide playable indicators
        playerHandController?.HidePlayableHand();
        playerHandController?.HidePlayableBoard();
        playerHandController?.HideBoardTargets();
    }
    
    /// <summary>
    /// Legacy overload for backward compatibility.
    /// </summary>
    public void OnHostDisconnected()
    {
        OnHostDisconnected(120f);
    }
    
    /// <summary>
    /// Update the host disconnect timer display.
    /// </summary>
    public void UpdateHostDisconnectTimer(float remainingTime)
    {
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = $"Host disconnected!\nWaiting for reconnect... {remainingTime:F0}s";
        }
    }
    
    /// <summary>
    /// Called when the host reconnects.
    /// </summary>
    public void OnHostReconnected()
    {
        Debug.Log("[EncounterController] Host reconnected! Resuming game...");
        isGamePaused = false;
        
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when host failed to reconnect within grace period.
    /// </summary>
    public void OnHostForfeited()
    {
        Debug.Log("[EncounterController] Host forfeited! You win!");
        
        if (disconnectStatusText != null)
        {
            disconnectStatusText.text = "Host forfeited!\n\nYOU WIN!\n\nReturning to menu...";
        }
    }
    
    /// <summary>
    /// Called when game state has been restored after reconnection.
    /// </summary>
    public void OnGameStateRestored()
    {
        Debug.Log("[EncounterController] Game state restored!");
        isGamePaused = false;
        networkInitialized = true; // Enable turn processing for reconnected client
        
        // Hide disconnect panel
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
        
        // Refresh UI
        player?.UpdatePlayerUI();
        opponent?.UpdatePlayerUI();
        
        // Update turn indicator
        UpdateTurnIndicator();
        
        // Update playable cards based on current turn
        if (isCurrentPlayerTurn)
        {
            playerHandController?.VisualizePlayableHand();
            playerHandController?.VisualizePlayableBoard();
        }
        else
        {
            playerHandController?.HidePlayableHand();
            playerHandController?.HidePlayableBoard();
        }
    }
    
    private System.Collections.IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Check if the game is currently paused (e.g., opponent disconnected).
    /// </summary>
    public bool IsGamePaused => isGamePaused;
    
    #endregion
}
