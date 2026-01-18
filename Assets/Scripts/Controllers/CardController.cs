using UnityEngine;
using TMPro;
using System.Collections.Generic; 
using System;
using KOA.Data;
using KOA.Effects;

public class CardController : MonoBehaviour
{
    // === DATA SOURCE ===
    [Header("Card Data (New System)")]
    [Tooltip("Optional: If set, card initializes from this data asset")]
    public CardData cardData;

    // Card Properties (runtime values - can be modified during gameplay)
    [Header("Runtime Properties")]
    public string cardName;
    public int manaCost;
    public int health;
    private int maxHealth;
    public int attack;  // Derived from offensive ability damage
    public int defense; // Derived from defensive ability damage (for damage reduction)

    // Owner reference
    public PlayerController owningPlayer;

    // Unique ID for each card
    public string id;

    // Card status tracking
    public bool isInHand = true;
    public bool isActive = false;
    public bool isTapped = false;
    public bool isFlipped = false;
    public bool isInPlay = false;
    public bool isHighlighted = false;

    // Status Effects
    public bool hasSummoningSickness = true;
    public bool isFrozen;
    public bool isBuried;
    public bool isDefending;

    // Ability GameObjects (Legacy system - still works)
    [Header("Abilities (Legacy GameObject System)")]
    public GameObject offensiveAbility;
    public GameObject supportAbility; 

    // UI Elements
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;

    // Card Visuals
    [Header("Card Artwork")]
    [Tooltip("SpriteRenderer for the card artwork (assigned in Inspector)")]
    public SpriteRenderer artworkRenderer;
    
    public GameObject summoningSicknessIcon;
    public GameObject tappedIcon;
    public GameObject flippedIcon;

    // card frames (different states)
    public List<GameObject> standardFrames;
    public List<GameObject> highlightedFrames;

    // Initialization method
    public void Start()
    {
        // If CardData is assigned, initialize from it
        if (cardData != null)
        {
            InitializeFromData(cardData);
        }
        else
        {
            // Legacy: use inspector values
            maxHealth = health;
        }
        
        id = Guid.NewGuid().ToString();
        UpdateCardUI();
        UpdateVisualEffects(); // Update visuals on load
    }

    /// <summary>
    /// Initialize this card from a CardData asset.
    /// </summary>
    public void InitializeFromData(CardData data)
    {
        cardData = data;
        cardName = data.displayName;
        manaCost = data.manaCost;
        health = data.health;
        maxHealth = data.health;
        
        // Set attack from offensive ability damage
        attack = data.offensiveAbility != null ? data.offensiveAbility.damage : 0;
        
        // Set defense from defensive ability (could be heal or damage reduction)
        defense = data.defensiveAbility != null ? data.defensiveAbility.damage : 0;
        
        // Set artwork if available
        if (data.artwork != null && artworkRenderer != null)
        {
            artworkRenderer.sprite = data.artwork;
        }
        
        UpdateCardUI();
    }

    // Method to update the card's UI elements
    public void UpdateCardUI()
    {
        Debug.Log($"[CardController] UpdateCardUI called for '{cardName}' - Health: {health}, Mana: {manaCost}, Attack: {attack}, Defense: {defense}");
        Debug.Log($"[CardController] Text refs - Name: {(cardNameText != null ? "OK" : "NULL")}, Mana: {(manaCostText != null ? "OK" : "NULL")}, Health: {(healthText != null ? "OK" : "NULL")}, Attack: {(attackText != null ? "OK" : "NULL")}, Defense: {(defenseText != null ? "OK" : "NULL")}");
        
        if (cardNameText != null) cardNameText.text = cardName;
        if (manaCostText != null) manaCostText.text = manaCost.ToString();
        if (healthText != null) healthText.text = health.ToString();
        if (attackText != null) attackText.text = attack.ToString();
        if (defenseText != null) defenseText.text = defense.ToString();
        
        // Also update stats on the Condensed view if it exists (for in-play cards)
        Transform condensedView = transform.Find("Canvas/Condensed");
        if (condensedView != null)
        {
            Transform healthTransform = condensedView.Find("Health");
            if (healthTransform != null)
            {
                TextMeshProUGUI condensedHealthText = healthTransform.GetComponent<TextMeshProUGUI>();
                if (condensedHealthText != null)
                {
                    condensedHealthText.text = health.ToString();
                }
            }
            
            // Update attack on condensed view
            Transform attackTransform = condensedView.Find("Attack");
            if (attackTransform != null)
            {
                TextMeshProUGUI condensedAttackText = attackTransform.GetComponent<TextMeshProUGUI>();
                if (condensedAttackText != null)
                {
                    condensedAttackText.text = attack.ToString();
                }
            }
        }
    }

    // Update card's visual effects based on status
    // Update card's visual effects based on status
    public void UpdateVisualEffects()
    {
        // Determine which view is active and find the CardImage SpriteRenderer
        Transform currentView = isInPlay
            ? transform.Find("Canvas/Condensed")
            : transform.Find("Canvas/Full");

        if (currentView == null)
        {
            Debug.LogWarning("Active view (Condensed or Full) is missing.");
            return;
        }

        // Ensure the icons are assigned and toggle them based on status
        if (summoningSicknessIcon != null)
        {
            summoningSicknessIcon.SetActive(hasSummoningSickness);
        }
        else
        {
            Debug.LogWarning("Summoning Sickness Icon is not assigned.");
        }

        if (tappedIcon != null)
        {
            tappedIcon.SetActive(isTapped);
        }
        else
        {
            Debug.LogWarning("Tapped Icon is not assigned.");
        }

        if (flippedIcon != null)
        {
            flippedIcon.SetActive(isFlipped);
        }
        else
        {
            Debug.LogWarning("Flipped Icon is not assigned.");
        }

        // Toggle visibility of views
        Transform condensedView = transform.Find("Canvas/Condensed");
        Transform fullView = transform.Find("Canvas/Full");

        if (condensedView != null)
        {
            condensedView.gameObject.SetActive(isInPlay); // Show condensed view if in play
        }

        if (fullView != null)
        {
            fullView.gameObject.SetActive(!isInPlay); // Show full view if not in play
        }

        // Set active state for highlighted frames
        foreach (var highlightedFrame in highlightedFrames)
        {
            highlightedFrame.SetActive(isHighlighted);
        }

        // Set active state for standard frames
        foreach (var standardFrame in standardFrames)
        {
            standardFrame.SetActive(!isHighlighted);
        }


    }


    // Method to manage health (local/single-player)
    public void TakeDamage(int damage)
    {
        Debug.Log($"{cardName} takes {damage} damage. Current health: {health} -> {health - damage}");

        health -= damage;
        
        if (health <= 0)
        {
            owningPlayer.RemoveCardFromBoard(this);
        }
        else
        {
            UpdateCardUI();
        }
    }
    
    /// <summary>
    /// Takes damage with server-authoritative death determination.
    /// Used in networked games to ensure all clients agree on card death.
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    /// <param name="willDie">Server-determined flag indicating if this card should die</param>
    public void TakeDamageNetworked(int damage, bool willDie)
    {
        Debug.Log($"[Networked] {cardName} takes {damage} damage. Current health: {health} -> {health - damage}. Server says willDie={willDie}");

        health -= damage;
        
        if (willDie)
        {
            // Server says this card dies - remove it
            owningPlayer.RemoveCardFromBoard(this);
        }
        else
        {
            // Server says card survives - just update UI
            UpdateCardUI();
        }
    }

    public void Heal(int amount)
    {
        Debug.Log($"{cardName} heals {amount}. Current health: {health} -> {health + amount}");

        health += amount;
        UpdateCardUI();
    }

    // Methods to handle status effects
    public void SetSummoningSickness(bool status)
    {
        hasSummoningSickness = status;
        UpdateVisualEffects();
    }

    public void TapCard()
    {
        isTapped = true;
        UpdateVisualEffects();
    }

    public void UntapCard()
    {
        isTapped = false;
        UpdateVisualEffects();
    }

    public void FlipCard()
    {
        isFlipped = true;
        UpdateVisualEffects();
    }

    public void HighlightCard()
    {
        isHighlighted = true;
        UpdateVisualEffects();
    }

    public void UnHighlightCard()
    {
        isHighlighted = false;
        UpdateVisualEffects();
    }

    public void UnflipCard()
    {
        isFlipped = false;
        UpdateVisualEffects();
    }

    public void FreezeCard()
    {
        isFrozen = true;
    }

    public void UnfreezeCard()
    {
        isFrozen = false;
    }

    public void BuryCard()
    {
        isBuried = true;
    }

    public void UnburyCard()
    {
        isBuried = false;
    }

    public void EnterPlay()
    {
        isInPlay = true;
        UpdateVisualEffects();
    }
    public void SetDefending(bool status)
    {
        isDefending = status;
    }

    // Methods to activate abilities on a CardController target
    public void ActivateOffensiveAbility(CardController target)
    {
        // NEW SYSTEM: Use CardData + AbilityEffect
        if (cardData != null && cardData.offensiveAbility != null)
        {
            ExecuteAbilityEffect(cardData.offensiveAbility, target);
            return;
        }
        
        // LEGACY SYSTEM: Use GameObject with AbilityController
        if (offensiveAbility != null)
        {
            AbilityController abilityController = offensiveAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Offensive ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Offensive ability is not set.");
        }
    }

    public void ActivateDefensiveAbility(CardController target)
    {
        // NEW SYSTEM: Use CardData + AbilityEffect
        if (cardData != null && cardData.defensiveAbility != null)
        {
            ExecuteAbilityEffect(cardData.defensiveAbility, target);
            return;
        }
        
        // LEGACY SYSTEM: Use GameObject with AbilityController
        if (supportAbility != null)
        {
            AbilityController abilityController = supportAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Support ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Support ability is not set.");
        }
    }

    // Methods to activate abilities on a PlayerController target
    public void ActivateOffensiveAbility(PlayerController target)
    {
        // NEW SYSTEM: Use CardData + AbilityEffect
        if (cardData != null && cardData.offensiveAbility != null)
        {
            ExecuteAbilityEffectOnPlayer(cardData.offensiveAbility, target);
            return;
        }
        
        // LEGACY SYSTEM: Use GameObject with AbilityController
        if (offensiveAbility != null)
        {
            AbilityController abilityController = offensiveAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Offensive ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Offensive ability is not set.");
        }
    }

    public void ActivateDefensiveAbility(PlayerController target)
    {
        // NEW SYSTEM: Use CardData + AbilityEffect
        if (cardData != null && cardData.defensiveAbility != null)
        {
            ExecuteAbilityEffectOnPlayer(cardData.defensiveAbility, target);
            return;
        }
        
        // LEGACY SYSTEM: Use GameObject with AbilityController
        if (supportAbility != null)
        {
            AbilityController abilityController = supportAbility.GetComponentInChildren<AbilityController>();
            if (abilityController != null)
            {
                abilityController.Activate(target);
            }
            else
            {
                Debug.LogError("Support ability child does not have an AbilityController component.");
            }
        }
        else
        {
            Debug.LogError("Support ability is not set.");
        }
    }

    /// <summary>
    /// Execute an ability effect from the new data system.
    /// </summary>
    private void ExecuteAbilityEffect(AbilityData abilityData, CardController target)
    {
        if (abilityData.effectPrefab == null)
        {
            Debug.LogWarning($"Ability {abilityData.displayName} has no effect prefab assigned.");
            return;
        }

        AbilityEffect effect = abilityData.effectPrefab.GetComponent<AbilityEffect>();
        if (effect != null)
        {
            effect.Execute(abilityData, this, target);
        }
        else
        {
            Debug.LogError($"Effect prefab for {abilityData.displayName} does not have an AbilityEffect component.");
        }
    }

    /// <summary>
    /// Execute an ability effect targeting a player.
    /// </summary>
    private void ExecuteAbilityEffectOnPlayer(AbilityData abilityData, PlayerController target)
    {
        if (abilityData.effectPrefab == null)
        {
            Debug.LogWarning($"Ability {abilityData.displayName} has no effect prefab assigned.");
            return;
        }

        AbilityEffect effect = abilityData.effectPrefab.GetComponent<AbilityEffect>();
        if (effect != null)
        {
            effect.Execute(abilityData, this, target);
        }
        else
        {
            Debug.LogError($"Effect prefab for {abilityData.displayName} does not have an AbilityEffect component.");
        }
    }
}
