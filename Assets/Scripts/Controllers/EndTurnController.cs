using UnityEngine;

/// <summary>
/// UI button to end the current turn.
/// Works with NetworkGameManager for turn management.
/// Story 036: Simplified - no longer references deprecated EncounterController.
/// </summary>
public class EndTurnController : MonoBehaviour
{
    // This is called when the mouse clicks on the sprite
    private void OnMouseDown()
    {
        // Use NetworkGameManager for turn checks
        if (NetworkGameManager.Instance == null)
        {
            Debug.LogWarning("[EndTurnController] NetworkGameManager not found!");
            return;
        }
        
        // Check if it's the local player's turn before allowing end turn
        if (!NetworkGameManager.Instance.IsLocalPlayerTurn())
        {
            Debug.Log("[EndTurnController] Cannot end turn - not your turn!");
            return;
        }
        
        // Request end turn via network
        NetworkGameManager.Instance.RequestEndTurn();
    }
}
