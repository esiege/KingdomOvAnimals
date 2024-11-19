using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    // Card Properties
    public string cardName;
    public int manaCost;
    public int health;

    // Owner reference
    public PlayerController owningPlayer;

    // Card status tracking
    public bool isInHand = true;
    public bool isActive = false;
    public bool isTapped = false;
    public bool isFlipped = false;
    public bool isInPlay = false;

    // Status Effects
    public bool hasSummoningSickness = true;
    public bool isFrozen;
    public bool isBuried;
    public bool isDefending;

    // Ability GameObjects
    public GameObject offensiveAbility;
    public GameObject supportAbility; 

    // UI Elements
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI healthText;

    // Card Visuals
    public GameObject summoningSicknessIcon;
    public GameObject tappedIcon;
    public GameObject flippedIcon;


    // Initialization method
    public void Start()
    {
        UpdateCardUI();
        UpdateVisualEffects(); // Update visuals on load
    }

    // Method to update the card's UI elements
    public void UpdateCardUI()
    {
        if (cardNameText != null) cardNameText.text = cardName;
        if (manaCostText != null) manaCostText.text = manaCost.ToString();
        if (healthText != null) healthText.text = health.ToString();
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
    }


    // Method to manage health
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
}
