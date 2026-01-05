using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private List<CardController> playerHand = new List<CardController>();
    private List<HoverHandler> hoveredCards = new List<HoverHandler>();
    private HoverHandler focusedCardHandler;
    private CardController activeCard;
    private Vector3 activeCardPosition;
    private LineRenderer lineRenderer;

    public List<GameObject> cardPositions;
    public TargetingController targetingController;
    public EncounterController encounterController;
    public float transitionSpeed = 5f;
    public float zIncrement = 0.2f;
    public Material lineMaterial;

    // Owner reference
    public PlayerController owningPlayer;


    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
        lineRenderer.material = lineMaterial;
    }



    void Start()
    {
        StartCoroutine(CheckFocusedCard());
        VisualizePlayableHand();
        HideBoardTargets();

    }


    void Update()
    {
        if (owningPlayer != encounterController.currentPlayer) return;

        HandleMouseInput();
    }


    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            HandleMouseDown();
        if (Input.GetMouseButton(0))
            OnMouseDrag();
        if (Input.GetMouseButtonUp(0))
            OnMouseUp();
    }

    private void HandleMouseDown()
    {
        // In network mode, check if it's our turn before allowing interaction
        if (encounterController != null && !encounterController.IsLocalPlayerTurn())
        {
            return; // Not our turn, ignore input
        }
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the position stays in the 2D plane

        // Get all colliders at the mouse position
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);

        // Check each collider for a CardController
        foreach (var hit in hits)
        {
            var clickedCard = hit.GetComponent<CardController>();
            if (clickedCard != null && clickedCard.owningPlayer == encounterController.currentPlayer)
            {
                OnCardMouseDown(clickedCard);
                return; // Stop once the correct CardController is found
            }
        }
    }

    /// <summary>
    /// Add an already-instantiated card to the hand. Used for game state restoration.
    /// Unlike AddCardToHand, this does NOT instantiate a new copy.
    /// </summary>
    public void AddExistingCardToHand(CardController card)
    {
        if (playerHand.Count >= cardPositions.Count)
        {
            Debug.LogError("Hand is full. Cannot add more cards.");
            Destroy(card.gameObject);
            return;
        }

        int positionIndex = playerHand.Count;

        // Position the card correctly in the hand
        card.transform.position = cardPositions[positionIndex].transform.position;
        card.transform.rotation = Quaternion.identity;
        card.transform.SetParent(cardPositions[positionIndex].transform);

        playerHand.Add(card);

        if (card.GetComponent<BoxCollider2D>() == null)
            card.gameObject.AddComponent<BoxCollider2D>();

        // Re-arrange the cards in hand to ensure proper positioning
        ArrangeCardsInHand();
    }

    public void AddCardToHand(CardController card)
    {
        if (playerHand.Count >= cardPositions.Count)
        {
            Debug.LogError("Hand is full. Cannot add more cards.");
            return;
        }

        int positionIndex = playerHand.Count;

        // Instantiate the card and position it correctly in the hand
        GameObject cardObject = Instantiate(card.gameObject, cardPositions[positionIndex].transform.position, Quaternion.identity);
        cardObject.transform.SetParent(cardPositions[positionIndex].transform);

        CardController instantiatedCard = cardObject.GetComponent<CardController>();
        playerHand.Add(instantiatedCard);

        if (cardObject.GetComponent<BoxCollider2D>() == null)
            cardObject.AddComponent<BoxCollider2D>();

        //HoverHandler hoverHandler = cardObject.AddComponent<HoverHandler>();
        //hoverHandler.Initialize(this, instantiatedCard, positionIndex, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);

        // Re-arrange the cards in hand to ensure proper positioning
        ArrangeCardsInHand();
    }

    public void RemoveCardFromHand(string cardId)
    {
        // Find the card in the hand by its ID
        int removedIndex = playerHand.FindIndex(card => card.id == cardId);

        if (removedIndex >= 0)
        {
            // Remove the card from the hand
            playerHand.RemoveAt(removedIndex);

            // Re-arrange the cards to ensure the gaps are filled
            ArrangeCardsInHand();
        }
        else
        {
            Debug.LogWarning($"Attempted to remove a card with ID {cardId} that is not in the hand.");
        }
    }


    // Method to arrange the cards in hand and update their positions
    private void ArrangeCardsInHand()
    {
        for (int i = 0; i < playerHand.Count; i++)
        {
            GameObject cardObject = playerHand[i].gameObject;

            // Move the card to its new position based on the current index
            cardObject.transform.SetParent(cardPositions[i].transform);
            cardObject.transform.localPosition = Vector3.zero;  // Reset the local position within the slot

            // Adjust Z position so the cards appear stacked correctly
            Vector3 cardPosition = cardObject.transform.localPosition;
            cardPosition.z = zIncrement * i;
            cardObject.transform.localPosition = cardPosition;

            // Set the sorting order for layering
            SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = playerHand.Count - i; // Higher sortingOrder for leftmost card
            }

            // Update any other logic associated with the card (like hover behavior)
            HoverHandler hoverHandler = cardObject.GetComponent<HoverHandler>();
            if (hoverHandler != null)
            {
                hoverHandler.CardIndex = i;  // Ensure the hover handler has the correct index
            }
        }
    }


    public List<GameObject> GetAllBoardTargets()
    {
        List<GameObject> targets = new List<GameObject>();

        GameObject slot;
        
        slot = GameObject.Find("OpponentSlot-1");
        if (slot != null) targets.Add(slot);
        
        slot = GameObject.Find("OpponentSlot-2");
        if (slot != null) targets.Add(slot);
        
        slot = GameObject.Find("OpponentSlot-3");
        if (slot != null) targets.Add(slot);
        
        slot = GameObject.Find("PlayerSlot-1");
        if (slot != null) targets.Add(slot);
        
        slot = GameObject.Find("PlayerSlot-2");
        if (slot != null) targets.Add(slot);
        
        slot = GameObject.Find("PlayerSlot-3");
        if (slot != null) targets.Add(slot);

        //targets.Add(GameObject.Find("Player"));
        //targets.Add(GameObject.Find("Opponent"));

        return targets;
    }
    public List<GameObject> GetPlayerPlayableBoard()
    {
        List<GameObject> targets = new List<GameObject>();

        targets.Add(GameObject.Find("PlayerSlot-1"));
        targets.Add(GameObject.Find("PlayerSlot-2"));
        targets.Add(GameObject.Find("PlayerSlot-3"));

        //targets.Add(GameObject.Find("Player"));
        //targets.Add(GameObject.Find("Opponent"));

        return targets;
    }
    public bool AllPlayerSlotsFull()
    {
        int cnt = 0;

        if (GameObject.Find("PlayerSlot-1").GetComponentInChildren<CardController>() != null)
            cnt++;
        if (GameObject.Find("PlayerSlot-2").GetComponentInChildren<CardController>() != null)
            cnt++;
        if (GameObject.Find("PlayerSlot-3").GetComponentInChildren<CardController>() != null)
            cnt++;

        return cnt == 3;
    }

    private void OnCardMouseDown(CardController card)
    {
        activeCard = card;
        activeCardPosition = card.transform.position;
        //card.GetComponent<HoverHandler>().enabled = false;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, activeCardPosition);
        activeCard.isActive = true;
        HidePlayableHand();
        VisualizeBoardTargets(card);

    }


    public void VisualizeBoardTargets(CardController card)
    {
        if (!targetingController)
            return;


        foreach (var t in encounterController.currentPlayer.board)
        {
            t.UnHighlightCard();
        }


        List<GameObject> supportTargets = targetingController.GetSupportTargets(card);
        foreach (var t in supportTargets)
        {
            CardController c = t.GetComponentInChildren<CardController>();

            if (c != null)
                c.HighlightCard();

            PlayerController p = t.GetComponentInChildren<PlayerController>();
        }

        List<GameObject> offensiveTargets = targetingController.GetOffensiveTargets(card);
        foreach (var t in offensiveTargets)
        {
            CardController c = t.GetComponentInChildren<CardController>();

            if (c != null)
                c.HighlightCard();

            PlayerController p = t.GetComponentInChildren<PlayerController>();
        }
        
        
    }

    public void HideBoardTargets()
    {
        foreach (var t in GetAllBoardTargets())
        {
            if (t == null) continue;
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }
    }

    public void HidePlayableBoard()
    {
        foreach (var t in GetPlayerPlayableBoard())
        {
            if (t == null) continue;
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }
    }
    public void VisualizePlayableBoard()
    {

        foreach (var t in GetPlayerPlayableBoard())
        {
            if (t == null) continue;
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null && !c.isTapped && !c.hasSummoningSickness)
                c.HighlightCard();
        }

    }

    public void VisualizePlayableHand()
    {
        if (!targetingController)
            return;


        List<CardController> targets = targetingController.GetPlayableCardsInHand();

        foreach (var t in targets)
        {
            t.HighlightCard();
        }


        //foreach (var t in cardPositions)
        //{
        //    CardController c = t.GetComponentInChildren<CardController>();
        //    if (c != null && c.manaCost <= owningPlayer.currentMana && owningPlayer.name == "PlayerController")
        //    {
        //        if (!c.isFlipped || !AllPlayerSlotsFull())
        //            c.HighlightCard();

        //    }

        //}

    }
    public void HidePlayableHand()
    {

        foreach (var t in cardPositions)
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }

    }



    private void OnMouseDrag()
    {
        if (activeCard == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        lineRenderer.SetPosition(1, mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.white);
        if (hit.collider == null)
        {
            return;
        }

        GameObject hitObject = hit.collider.gameObject;


        string slotName = "PlayerSlot";
        if (owningPlayer.name == "Opponent")
            slotName = "OpponentSlot";


        if (owningPlayer != encounterController.currentPlayer)
            return;

        if (hitObject.name.StartsWith(slotName))
        {
            show_addCardToEncounter(activeCard);
        }
        else if (hitObject.TryGetComponent(out CardController targetCard))
        {

            if (targetCard.owningPlayer == this.owningPlayer)
                show_useCardAbilityDefensive(targetCard);
            else
                show_useCardAbilityOffensive(targetCard);
        }
    }


    private void show_addCardToEncounter(CardController card)
    {
        if (activeCard.isInPlay) return;


        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.yellow);
    }

    private void addCardToEncounter(CardController card, GameObject hitObject)
    {
        if (activeCard.isInPlay) return;
        if (owningPlayer != encounterController.currentPlayer) return;

        if (hitObject.GetComponentInChildren<CardController>() != null)
        {
            Debug.Log($"Cannot add {card.cardName} to {hitObject.name}, slot is already occupied.");
            return;
        }

        if (encounterController.currentPlayer.currentMana < card.manaCost)
        {
            Debug.Log($"Card {card.cardName} not added, not enough mana.");
            return;
        }

        // Check if we're in a network game
        if (owningPlayer.networkPlayer != null)
        {
            // Network play - send request to server
            int cardIndex = playerHand.IndexOf(card);
            int slotIndex = GetSlotIndex(hitObject);
            
            if (cardIndex >= 0 && slotIndex >= 0)
            {
                Debug.Log($"[HandController] Sending network card play: card {cardIndex}, slot {slotIndex}");
                owningPlayer.networkPlayer.CmdPlayCard(cardIndex, slotIndex);
            }
            else
            {
                Debug.LogError($"[HandController] Invalid card index {cardIndex} or slot index {slotIndex}");
            }
            return;
        }

        // Local play (single player or fallback)
        ExecuteLocalCardPlay(card, hitObject);
    }
    
    /// <summary>
    /// Gets the slot index (0, 1, 2) from a slot GameObject.
    /// </summary>
    private int GetSlotIndex(GameObject slot)
    {
        string slotName = slot.name;
        // Handles both "PlayerSlot-1" and "OpponentSlot-1" etc.
        if (slotName.Contains("-1")) return 0;
        if (slotName.Contains("-2")) return 1;
        if (slotName.Contains("-3")) return 2;
        return -1;
    }
    
    /// <summary>
    /// Executes a card play locally (for single player or called by network).
    /// </summary>
    private void ExecuteLocalCardPlay(CardController card, GameObject hitObject)
    {
        encounterController.currentPlayer.SpendMana(card.manaCost);

        card.transform.SetParent(hitObject.transform);
        card.transform.position = hitObject.transform.position;
        card.transform.localPosition = Vector3.zero;

        card.isActive = false;
        card.isInHand = false;
        card.SetSummoningSickness(true);
        card.UnflipCard();
        card.EnterPlay();

        RemoveCardFromHand(card.id);
        encounterController.currentPlayer.AddCardToBoard(card);
    }

    // Defensive: CardController target
    private void show_useCardAbilityDefensive(CardController targetCard)
    {
        if (targetCard == activeCard || !targetCard.isInPlay) return;
        SetLineColor(activeCard.isInPlay ? Color.cyan : Color.green);
    }

    // Defensive: PlayerController target
    private void show_useCardAbilityDefensive(PlayerController targetPlayer)
    {
        SetLineColor(Color.green);
    }







    // Activate Defensive Ability: CardController target
    private void useCardAbilityDefensive(CardController targetCard)
    {
        if (!CanActivateAbility(targetCard)) return;

        activeCard.ActivateDefensiveAbility(targetCard);
        Debug.Log("Defense action triggered on card!");

    }

    // Activate Defensive Ability: PlayerController target
    private void useCardAbilityDefensive(PlayerController targetPlayer)
    {
        if (!CanActivateAbility(targetPlayer)) return;

        activeCard.ActivateDefensiveAbility(targetPlayer);
        Debug.Log("Defense action triggered on player!");
    }

    // Activate Offensive Ability: CardController target
    private void useCardAbilityOffensive(CardController targetCard)
    {
        if (!ValidateAbilityTarget(targetCard)) return;

        // Check if we're in a network game
        if (owningPlayer.networkPlayer != null)
        {
            if (activeCard.isInPlay)
            {
                // Card is on board - send request to server using slot names
                string attackerSlotName = GetCardSlotName(activeCard);
                string targetSlotName = GetCardSlotName(targetCard);
                
                if (attackerSlotName != null && targetSlotName != null)
                {
                    Debug.Log($"[HandController] Sending network ability use: {activeCard.cardName} ({attackerSlotName}) -> {targetCard.cardName} ({targetSlotName})");
                    owningPlayer.networkPlayer.CmdUseAbilityOnCard(attackerSlotName, targetSlotName, true);
                    return;
                }
                else
                {
                    Debug.LogError($"[HandController] Could not find slot for attacker or target card");
                    return;
                }
            }
            else
            {
                // Card is in hand (flip ability) - use hand index and target slot
                int handIndex = playerHand.IndexOf(activeCard);
                string targetSlotName = GetCardSlotName(targetCard);
                
                if (handIndex >= 0 && targetSlotName != null)
                {
                    Debug.Log($"[HandController] Sending network flip ability: {activeCard.cardName} (hand index {handIndex}) -> {targetCard.cardName} ({targetSlotName})");
                    owningPlayer.networkPlayer.CmdUseFlipAbilityOnCard(handIndex, targetSlotName, true);
                    return;
                }
                else
                {
                    Debug.LogError($"[HandController] Could not find hand index or target slot for flip ability");
                    return;
                }
            }
        }
        
        // Local play - apply effects
        ApplyAbilityEffects();
        activeCard.ActivateOffensiveAbility(targetCard);
        Debug.Log("Offense action triggered on card!");
    }

    // Activate Offensive Ability: PlayerController target
    private void useCardAbilityOffensive(PlayerController targetPlayer)
    {
        if (!ValidateAbilityTarget(targetPlayer)) return;

        // Check if we're in a network game
        if (owningPlayer.networkPlayer != null)
        {
            if (activeCard.isInPlay)
            {
                // Card is on board - send request to server using slot name
                string attackerSlotName = GetCardSlotName(activeCard);
                int targetPlayerId = targetPlayer.networkPlayer?.PlayerId.Value ?? -1;
                
                if (attackerSlotName != null && targetPlayerId >= 0)
                {
                    Debug.Log($"[HandController] Sending network ability use on player: {activeCard.cardName} ({attackerSlotName}) -> Player {targetPlayerId}");
                    owningPlayer.networkPlayer.CmdUseAbilityOnPlayer(attackerSlotName, targetPlayerId, true);
                    return;
                }
            }
            else
            {
                // Card is in hand (flip ability) - use hand index
                int handIndex = playerHand.IndexOf(activeCard);
                int targetPlayerId = targetPlayer.networkPlayer?.PlayerId.Value ?? -1;
                
                if (handIndex >= 0 && targetPlayerId >= 0)
                {
                    Debug.Log($"[HandController] Sending network flip ability on player: {activeCard.cardName} (hand index {handIndex}) -> Player {targetPlayerId}");
                    owningPlayer.networkPlayer.CmdUseFlipAbilityOnPlayer(handIndex, targetPlayerId, true);
                    return;
                }
            }
        }

        // Local play - apply effects
        ApplyAbilityEffects();
        activeCard.ActivateOffensiveAbility(targetPlayer);
        Debug.Log("Offense action triggered on player!");
    }
    
    /// <summary>
    /// Gets the slot name for a card that is in play.
    /// </summary>
    private string GetCardSlotName(CardController card)
    {
        if (card == null || !card.isInPlay) return null;
        
        // The card's parent should be the slot
        Transform parent = card.transform.parent;
        if (parent != null && parent.name.Contains("Slot"))
        {
            return parent.name;
        }
        
        return null;
    }

    // Helper method to validate ability target (without applying effects)
    private bool ValidateAbilityTarget(object target)
    {
        if (target == activeCard)
        {
            Debug.LogWarning("Cannot target the active card itself.");
            return false;
        }

        if (target is CardController targetCard && !targetCard.isInPlay)
        {
            Debug.LogWarning("Cannot target a card that is not in play.");
            return false;
        }

        if (activeCard.isInPlay && activeCard.hasSummoningSickness)
        {
            Debug.LogWarning("Cannot activate ability: Card has summoning sickness.");
            return false;
        }

        if (activeCard.isInPlay && activeCard.isTapped)
        {
            Debug.LogWarning("Cannot activate ability: Card is already tapped.");
            return false;
        }

        if (!activeCard.isInPlay && activeCard.isFlipped)
        {
            Debug.LogWarning("Cannot activate ability: Card is already flipped.");
            return false;
        }

        if (!activeCard.isInPlay && encounterController.currentPlayer.currentMana < activeCard.manaCost)
        {
            Debug.LogWarning($"Not enough mana to activate ability. Current Mana: {encounterController.currentPlayer.currentMana}, Required: {activeCard.manaCost}");
            return false;
        }

        return true;
    }
    
    // Apply side effects of using an ability (tap/flip, spend mana)
    private void ApplyAbilityEffects()
    {
        if (activeCard.isInPlay)
        {
            activeCard.TapCard();
        }
        else
        {
            activeCard.FlipCard();
            encounterController.currentPlayer.SpendMana(activeCard.manaCost);
        }
    }

    // Legacy helper method for defensive abilities (not yet networked)
    private bool CanActivateAbility(object target)
    {
        if (!ValidateAbilityTarget(target)) return false;

        // Apply effects for local play
        ApplyAbilityEffects();
        return true;
    }

    // Offensive: CardController target
    private void show_useCardAbilityOffensive(CardController targetCard)
    {
        if (targetCard == activeCard || !targetCard.isInPlay) return;
        SetLineColor(activeCard.isInPlay ? new Color(1.0f, 0.5f, 0.0f) : Color.red);
    }

    // Offensive: PlayerController target
    private void show_useCardAbilityOffensive(PlayerController targetPlayer)
    {
        SetLineColor(Color.red);
    }


    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = lineRenderer.endColor = color;
    }
    private void OnMouseUp()
    {
        if (activeCard == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // Get all colliders at the mouse position
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        GameObject hitObject = null;
        CardController targetCard = null;
        PlayerController targetPlayer = null;

        // Iterate through hits to find the first relevant object (CardController or PlayerController)
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out CardController card))
            {
                targetCard = card;
                hitObject = hit.collider.gameObject;
                break;
            }
            else if (hit.collider.TryGetComponent(out PlayerController player))
            {
                targetPlayer = player;
                hitObject = hit.collider.gameObject;
                break;
            }
            else
            {
                hitObject = hit.collider.gameObject; // Fallback to generic hit (e.g., slot)
            }
        }

        if (hitObject != null)
        {
            string slotName = "PlayerSlot";
            if (owningPlayer.name == "Opponent")
                slotName = "OpponentSlot";

            // Drag card to empty slot
            if (hitObject.name.StartsWith(slotName))
            {
                addCardToEncounter(activeCard, hitObject);
            }
            // Drag card onto another card
            else if (targetCard != null)
            {
                if (targetCard.owningPlayer == this.owningPlayer)
                    useCardAbilityDefensive(targetCard);
                else
                    useCardAbilityOffensive(targetCard);
            }
            // Drag card onto a player
            else if (targetPlayer != null)
            {
                if (targetPlayer == owningPlayer)
                    useCardAbilityDefensive(targetPlayer);
                else
                    useCardAbilityOffensive(targetPlayer);
            }
        }

        activeCard = null;
        lineRenderer.enabled = false;

        HideBoardTargets();
        VisualizePlayableHand();

        HidePlayableBoard();
        VisualizePlayableBoard();
    }

    public List<CardController> GetHand() => playerHand;

    public void AddHoveredCard(HoverHandler hoverHandler)
    {
        if (!hoveredCards.Contains(hoverHandler))
            hoveredCards.Add(hoverHandler);
    }

    public void RemoveHoveredCard(HoverHandler hoverHandler)
    {
        hoveredCards.Remove(hoverHandler);
    }

    private IEnumerator CheckFocusedCard()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            HoverHandler cardToFocus = null;
            int highestIndex = -1;

            foreach (var hoverHandler in hoveredCards)
            {
                if (hoverHandler.CardIndex > highestIndex)
                {
                    highestIndex = hoverHandler.CardIndex;
                    cardToFocus = hoverHandler;
                }
            }

            if (cardToFocus != focusedCardHandler)
            {
                focusedCardHandler?.ReturnToOriginalPosition();
                focusedCardHandler = cardToFocus;
                focusedCardHandler?.StartHovering();
            }
        }
    }

}