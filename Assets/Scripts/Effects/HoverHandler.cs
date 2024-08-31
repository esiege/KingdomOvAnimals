using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HandController handController;
    private CardController cardController;
    private Transform parentTransform;
    private Vector3 originalPosition;
    private Vector3 targetHoverPosition;
    private float transitionSpeed;
    private bool isMovingToHoverPosition;
    private bool isMovingBack;

    public void Initialize(HandController controller, CardController card, Vector3 originalPos, Vector3 hoverOffset, float speed)
    {
        handController = controller;
        cardController = card;
        parentTransform = transform.parent; // Use the parent transform for movement
        originalPosition = originalPos;
        targetHoverPosition = originalPosition + hoverOffset;
        transitionSpeed = speed;
    }

    void Update()
    {
        // Move towards the hover position
        if (isMovingToHoverPosition)
        {
            parentTransform.localPosition = Vector3.Lerp(parentTransform.localPosition, targetHoverPosition, Time.deltaTime * transitionSpeed);
            if (Vector3.Distance(parentTransform.localPosition, targetHoverPosition) < 0.01f)
            {
                parentTransform.localPosition = targetHoverPosition;
                isMovingToHoverPosition = false;
            }
        }

        // Move back to the original position
        if (isMovingBack)
        {
            parentTransform.localPosition = Vector3.Lerp(parentTransform.localPosition, originalPosition, Time.deltaTime * transitionSpeed);
            if (Vector3.Distance(parentTransform.localPosition, originalPosition) < 0.01f)
            {
                parentTransform.localPosition = originalPosition;
                isMovingBack = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Set this card as the focused card
        handController.SetFocusedCard(this);
        StartHovering();
    }

    // We must implement OnPointerExit even if we don't use it
    public void OnPointerExit(PointerEventData eventData)
    {
        // No action needed here as the focus logic is handled in SetFocusedCard
    }

    public void StartHovering()
    {
        isMovingBack = false; // Stop moving back if it was
        isMovingToHoverPosition = true; // Start moving to hover position
    }

    public void ReturnToOriginalPosition()
    {
        isMovingToHoverPosition = false; // Stop moving to hover position
        isMovingBack = true; // Start moving back to original position
    }
}
