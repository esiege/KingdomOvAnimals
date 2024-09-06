using System;
using UnityEngine;

public class HoverHandler : MonoBehaviour
{
    private HandController handController;
    private CardController cardController;
    private Transform parentTransform;
    private Vector3 originalPosition;
    private Vector3 targetHoverPosition;
    private float transitionSpeed;
    private bool isMovingToHoverPosition;
    private bool isMovingBack;
    private bool isHovered; // Flag to track if currently hovered
    private Camera mainCamera; // Reference to the main camera
    private float hoverZOffset = -0.1f; // Offset to bring the card closer to the camera when hovered
    private float originalZPosition; // Store the original Z position
    private Canvas cardCanvas; // Reference to the Canvas component
    private int originalCanvasOrder; // Store the original Canvas order
    private SpriteRenderer[] spriteRenderers; // Array to hold all child SpriteRenderers
    private int[] originalSpriteSortingOrders; // Array to store original sorting orders of each SpriteRenderer


    public int CardIndex { get; private set; } // Index of the card in the hand

    public void Initialize(HandController controller, CardController card, int index, Vector3 originalPos, Vector3 hoverOffset, float speed)
    {
        handController = controller;
        cardController = card;
        CardIndex = index; // Store the index of the card
        parentTransform = transform.parent; // Use the parent transform for movement
        originalPosition = originalPos;
        transitionSpeed = speed;
        mainCamera = Camera.main; // Cache the main camera for raycasting

        if (card.owningPlayer.name == "Opponent")
            hoverOffset = new Vector3(hoverOffset.x, hoverOffset.y * -1, hoverOffset.z);

        targetHoverPosition = originalPosition + hoverOffset;

        // Store the original Z position of the card
        originalZPosition = parentTransform.localPosition.z;

        // Find and cache the Canvas component from the child object named "Canvas"
        cardCanvas = transform.Find("Canvas").GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            originalCanvasOrder = cardCanvas.sortingOrder; // Store the original canvas order in layer

            // Find all SpriteRenderer components that are children of the Canvas
            spriteRenderers = cardCanvas.GetComponentsInChildren<SpriteRenderer>();

            // Store their original sorting orders
            originalSpriteSortingOrders = new int[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalSpriteSortingOrders[i] = spriteRenderers[i].sortingOrder;
            }
        }


    }

    void Update()
    {
        HandleHoverDetection();

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

    // Handle hover detection using Raycasting for 2D
    void HandleHoverDetection()
    {
        Vector3 screenPos = Input.mousePosition;
        if (screenPos.x < 0 || screenPos.y < 0 || screenPos.x > Screen.width || screenPos.y > Screen.height)
        {
            return;
        }

        // Convert mouse position to world space
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.transform == transform)
            {
                if (!isHovered) // Mouse just entered
                {
                    isHovered = true;
                    StartHovering();
                    handController.AddHoveredCard(this);
                }
            }
            else if (isHovered) // Mouse exited
            {
                isHovered = false;
                ReturnToOriginalPosition();
                handController.RemoveHoveredCard(this);
            }
        }
        else if (isHovered) // Mouse exited
        {
            isHovered = false;
            ReturnToOriginalPosition();
            handController.RemoveHoveredCard(this);
        }
    }

    public void StartHovering()
    {
        isMovingBack = false; // Stop moving back if it was
        isMovingToHoverPosition = true; // Start moving to hover position

        // Bring the card closer to the camera by adjusting the Z position
        Vector3 newPosition = parentTransform.localPosition;
        newPosition.z = hoverZOffset; // Move closer to the camera
        parentTransform.localPosition = newPosition;

        // Change Canvas sorting order when hovered
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder += 20; // Set to a higher value when hovered

            // Increase sorting order of all child SpriteRenderers by 20
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingOrder = originalSpriteSortingOrders[i] + 20;
            }
        }
    }

    public void ReturnToOriginalPosition()
    {
        isMovingToHoverPosition = false; // Stop moving to hover position
        isMovingBack = true; // Start moving back to original position

        // Reset the Z position to the original value
        Vector3 resetPosition = parentTransform.localPosition;
        resetPosition.z = originalZPosition; // Reset to the original Z position
        parentTransform.localPosition = resetPosition;

        // Reset Canvas sorting order to its original value when unhovered
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = originalCanvasOrder; // Reset to original value

            // Reset the sorting orders of all child SpriteRenderers
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingOrder = originalSpriteSortingOrders[i];
            }
        }
    }
}
