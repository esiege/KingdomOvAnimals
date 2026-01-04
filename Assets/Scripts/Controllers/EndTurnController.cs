using UnityEngine;

public class EndTurnController : MonoBehaviour
{
    public EncounterController encounterController;  // Reference to the EncounterController

    // This is called when the mouse clicks on the sprite
    private void OnMouseDown()
    {
        // Check if it's the local player's turn before allowing end turn
        if (encounterController != null && !encounterController.IsLocalPlayerTurn())
        {
            Debug.Log("[EndTurnController] Cannot end turn - not your turn!");
            return;
        }
        
        // Call the endTurn method in the EncounterController
        encounterController.EndTurn();
    }
}
