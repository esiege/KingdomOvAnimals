using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Represents a connected player in the network.
/// This is spawned for each player that connects.
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    /// <summary>
    /// The player's connection ID (synced to all clients).
    /// </summary>
    public readonly SyncVar<int> PlayerId = new SyncVar<int>();

    /// <summary>
    /// Player's display name (synced to all clients).
    /// </summary>
    public readonly SyncVar<string> PlayerName = new SyncVar<string>();

    /// <summary>
    /// Is this player ready to start the game?
    /// </summary>
    public readonly SyncVar<bool> IsReady = new SyncVar<bool>();

    /// <summary>
    /// Called by server to initialize this player.
    /// </summary>
    public void Initialize(int playerId)
    {
        PlayerId.Value = playerId;
        PlayerName.Value = $"Player {playerId}";
        
        Debug.Log($"[NetworkPlayer] Initialized: {PlayerName.Value} (ID: {PlayerId.Value})");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (IsOwner)
        {
            Debug.Log($"[NetworkPlayer] You are: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
        else
        {
            Debug.Log($"[NetworkPlayer] Other player joined: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        if (!IsOwner)
        {
            Debug.Log($"[NetworkPlayer] Player left: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
    }

    /// <summary>
    /// Called by owning client to set ready state.
    /// </summary>
    [ServerRpc]
    public void SetReady(bool ready)
    {
        IsReady.Value = ready;
        Debug.Log($"[NetworkPlayer] {PlayerName.Value} ready: {IsReady.Value}");
    }
}
