using System;
using FishNet.Serializing;

namespace KOA.Model
{
    /// <summary>
    /// Complete game state for the board.
    /// This is the single source of truth for all game data.
    /// </summary>
    [Serializable]
    public class BoardState
    {
        public const int NUM_PLAYERS = 2;
        
        /// <summary>
        /// State for each player (indexed by player ID: 0 or 1).
        /// </summary>
        public PlayerBoardState[] Players;
        
        /// <summary>
        /// Which player's turn it is (0 or 1).
        /// </summary>
        public int CurrentTurnPlayerId;
        
        /// <summary>
        /// Current turn number (1-indexed).
        /// </summary>
        public int TurnNumber;
        
        /// <summary>
        /// Is the game currently in progress?
        /// </summary>
        public bool IsGameActive;
        
        /// <summary>
        /// Winner player ID, or -1 if no winner yet.
        /// </summary>
        public int WinnerPlayerId;
        
        public BoardState()
        {
            Players = new PlayerBoardState[NUM_PLAYERS];
            for (int i = 0; i < NUM_PLAYERS; i++)
            {
                Players[i] = new PlayerBoardState(i);
            }
            CurrentTurnPlayerId = 0;
            TurnNumber = 1;
            IsGameActive = false;
            WinnerPlayerId = -1;
        }
        
        /// <summary>
        /// Get player state by ID.
        /// </summary>
        public PlayerBoardState GetPlayer(int playerId)
        {
            if (playerId < 0 || playerId >= NUM_PLAYERS) return null;
            return Players[playerId];
        }
        
        /// <summary>
        /// Get the opponent of a player.
        /// </summary>
        public PlayerBoardState GetOpponent(int playerId)
        {
            return GetPlayer(1 - playerId);
        }
        
        /// <summary>
        /// Check if it's a specific player's turn.
        /// </summary>
        public bool IsPlayerTurn(int playerId)
        {
            return IsGameActive && CurrentTurnPlayerId == playerId;
        }
        
        /// <summary>
        /// Find a card anywhere on the board by instance ID.
        /// Returns (playerId, slotIndex) or (-1, -1) if not found.
        /// </summary>
        public (int playerId, int slotIndex) FindCardOnBoard(int instanceId)
        {
            for (int p = 0; p < NUM_PLAYERS; p++)
            {
                int slot = Players[p].FindSlotByInstanceId(instanceId);
                if (slot >= 0)
                {
                    return (p, slot);
                }
            }
            return (-1, -1);
        }
        
        /// <summary>
        /// Get a card by player ID and slot index.
        /// </summary>
        public CardState GetCard(int playerId, int slotIndex)
        {
            var player = GetPlayer(playerId);
            return player?.GetCardInSlot(slotIndex);
        }
        
        /// <summary>
        /// End current turn and switch to next player.
        /// </summary>
        public void EndTurn()
        {
            CurrentTurnPlayerId = 1 - CurrentTurnPlayerId;
            
            // Increment turn number when it comes back to player 0
            if (CurrentTurnPlayerId == 0)
            {
                TurnNumber++;
            }
            
            // Notify the new current player's cards
            Players[CurrentTurnPlayerId].OnTurnStart(TurnNumber);
        }
        
        /// <summary>
        /// Check for game over condition.
        /// </summary>
        public void CheckGameOver()
        {
            if (!Players[0].IsAlive)
            {
                WinnerPlayerId = 1;
                IsGameActive = false;
            }
            else if (!Players[1].IsAlive)
            {
                WinnerPlayerId = 0;
                IsGameActive = false;
            }
        }
        
        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            IsGameActive = true;
            WinnerPlayerId = -1;
            TurnNumber = 1;
            CurrentTurnPlayerId = 0;
            
            // Initialize player states
            for (int i = 0; i < NUM_PLAYERS; i++)
            {
                Players[i].Health = Players[i].MaxHealth;
                Players[i].Mana = 1;
                Players[i].MaxMana = 1;
            }
        }
        
        public override string ToString()
        {
            return $"[BoardState Turn={TurnNumber}, CurrentPlayer={CurrentTurnPlayerId}, Active={IsGameActive}]";
        }
    }
    
    /// <summary>
    /// FishNet serializer for BoardState.
    /// </summary>
    public static class BoardStateSerializer
    {
        public static void WriteBoardState(this Writer writer, BoardState value)
        {
            writer.WriteInt32(value.CurrentTurnPlayerId);
            writer.WriteInt32(value.TurnNumber);
            writer.WriteBoolean(value.IsGameActive);
            writer.WriteInt32(value.WinnerPlayerId);
            
            for (int i = 0; i < BoardState.NUM_PLAYERS; i++)
            {
                writer.WritePlayerBoardState(value.Players[i]);
            }
        }
        
        public static BoardState ReadBoardState(this Reader reader)
        {
            var state = new BoardState();
            state.CurrentTurnPlayerId = reader.ReadInt32();
            state.TurnNumber = reader.ReadInt32();
            state.IsGameActive = reader.ReadBoolean();
            state.WinnerPlayerId = reader.ReadInt32();
            
            for (int i = 0; i < BoardState.NUM_PLAYERS; i++)
            {
                state.Players[i] = reader.ReadPlayerBoardState();
            }
            
            return state;
        }
    }
}
