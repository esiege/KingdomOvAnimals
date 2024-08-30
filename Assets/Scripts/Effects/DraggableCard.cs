using FishNet.Example.Scened;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public EncounterController encounterController;
    public PlayerController owningPlayer;

    private Vector3 startPosition;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false; // Disable raycast blocking so other objects can detect the drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Check if the card is dropped in a valid area to play
        if (IsValidDropArea())
        {
            encounterController.PlayCard(GetComponent<CardController>(), owningPlayer);
        }
        else
        {
            // Return to original position if not placed in a valid area
            transform.position = startPosition;
        }

        canvasGroup.blocksRaycasts = true;
    }

    private bool IsValidDropArea()
    {
        // Implement logic to check if the card is dropped in a valid area
        // For simplicity, return true for now, but this would typically check the drop target
        return true;
    }
}
