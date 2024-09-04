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

    public int CardIndex { get; private set; } // Index of the card in the hand

    public void Initialize(HandController controller, CardController card, int index, Vector3 originalPos, Vector3 hoverOffset, float speed)
    {
        handController = controller;
        cardController = card;
        CardIndex = index; // Store the index of the card
        parentTransform = transform.parent; // Use the parent transform for movement
        originalPosition = originalPos;
        targetHoverPosition = originalPosition + hoverOffset;
        transitionSpeed = speed;
        mainCamera = Camera.main; // Cache the main camera for raycasting
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
                    Debug.Log("Mouse entered the card");
                }
            }
            else if (isHovered) // Mouse exited
            {
                isHovered = false;
                ReturnToOriginalPosition();
                handController.RemoveHoveredCard(this);
                Debug.Log("Mouse exited the card");
            }
        }
        else if (isHovered) // Mouse exited
        {
            isHovered = false;
            ReturnToOriginalPosition();
            handController.RemoveHoveredCard(this);
            Debug.Log("Mouse exited the card");
        }
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
