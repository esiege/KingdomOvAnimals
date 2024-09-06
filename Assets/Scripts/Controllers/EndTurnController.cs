using UnityEngine;

public class EndTurnController : MonoBehaviour
{
    public EncounterController encounterController;  // Reference to the EncounterController

    // This is called when the mouse clicks on the sprite
    private void OnMouseDown()
    {
        // Call the endTurn method in the EncounterController
        encounterController.endTurn();
    }
}
