using UnityEngine;
using KOA.Data;

namespace KOA.Tests
{
    /// <summary>
    /// Test script for Story 022: CardController Uses CardData.
    /// Attach to any GameObject in the scene to verify CardController + CardData integration.
    /// </summary>
    public class CardControllerDataTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("CardData asset to test with")]
        public CardData testCardData;
        
        [Tooltip("CardController to test (will create one if not assigned)")]
        public CardController testCard;
        
        [Tooltip("Test target card for ability testing")]
        public CardController targetCard;
        
        private void Start()
        {
            Debug.Log("=== CardController CardData Integration Test ===");
            
            if (testCardData == null)
            {
                Debug.LogError("[CardControllerDataTest] No testCardData assigned! Assign a CardData asset.");
                return;
            }
            
            if (testCard == null)
            {
                Debug.LogError("[CardControllerDataTest] No testCard assigned! Assign a CardController in the scene.");
                return;
            }
            
            RunTests();
        }
        
        private void RunTests()
        {
            Debug.Log($"Testing with CardData: {testCardData.displayName} (ID: {testCardData.id})");
            Debug.Log("");
            
            // Test 1: Inspector Assignment
            TestInspectorAssignment();
            
            // Test 2: Initialize from Data
            TestInitializeFromData();
            
            // Test 3: Display Name
            TestDisplayName();
            
            // Test 4: Display Health
            TestDisplayHealth();
            
            // Test 5: Display Artwork
            TestDisplayArtwork();
            
            // Test 6: Offensive Ability
            TestOffensiveAbility();
            
            // Test 7: Defensive Ability
            TestDefensiveAbility();
            
            // Summary
            Debug.Log("");
            Debug.Log("=== Test Complete ===");
            Debug.Log("Check the Console for PASS/FAIL results.");
            Debug.Log("For multiplayer test: Run as Host + Client and verify both see same stats.");
        }
        
        private void TestInspectorAssignment()
        {
            Debug.Log("[Test 1] Inspector Assignment...");
            
            // Assign via code (simulates Inspector)
            testCard.cardData = testCardData;
            
            bool pass = testCard.cardData == testCardData;
            Debug.Log($"  CardData assignable in Inspector: {(pass ? "PASS" : "FAIL")}");
        }
        
        private void TestInitializeFromData()
        {
            Debug.Log("[Test 2] Initialize from CardData...");
            
            testCard.InitializeFromData(testCardData);
            
            bool pass = testCard.cardData == testCardData;
            Debug.Log($"  InitializeFromData works: {(pass ? "PASS" : "FAIL")}");
        }
        
        private void TestDisplayName()
        {
            Debug.Log("[Test 3] Display Name from CardData...");
            
            bool pass = testCard.cardName == testCardData.displayName;
            Debug.Log($"  Expected: '{testCardData.displayName}', Got: '{testCard.cardName}'");
            Debug.Log($"  Name matches CardData: {(pass ? "PASS" : "FAIL")}");
        }
        
        private void TestDisplayHealth()
        {
            Debug.Log("[Test 4] Display Health from CardData...");
            
            bool pass = testCard.health == testCardData.health;
            Debug.Log($"  Expected: {testCardData.health}, Got: {testCard.health}");
            Debug.Log($"  Health matches CardData: {(pass ? "PASS" : "FAIL")}");
        }
        
        private void TestDisplayArtwork()
        {
            Debug.Log("[Test 5] Display Artwork from CardData...");
            
            if (testCardData.artwork == null)
            {
                Debug.Log("  (CardData has no artwork - skipping)");
                return;
            }
            
            if (testCard.artworkRenderer == null)
            {
                Debug.Log("  FAIL: CardController has no artworkRenderer assigned");
                return;
            }
            
            bool pass = testCard.artworkRenderer.sprite == testCardData.artwork;
            Debug.Log($"  Artwork matches CardData: {(pass ? "PASS" : "FAIL")}");
        }
        
        private void TestOffensiveAbility()
        {
            Debug.Log("[Test 6] Offensive Ability from CardData...");
            
            if (testCardData.offensiveAbility == null)
            {
                Debug.Log("  (CardData has no offensive ability - skipping)");
                return;
            }
            
            Debug.Log($"  Offensive ability assigned: {testCardData.offensiveAbility.displayName}");
            
            if (targetCard != null)
            {
                Debug.Log($"  Triggering offensive ability on target...");
                testCard.ActivateOffensiveAbility(targetCard);
                Debug.Log($"  Ability triggered: PASS (check target for effect)");
            }
            else
            {
                Debug.Log("  (No target card assigned - cannot test execution)");
            }
        }
        
        private void TestDefensiveAbility()
        {
            Debug.Log("[Test 7] Defensive Ability from CardData...");
            
            if (testCardData.defensiveAbility == null)
            {
                Debug.Log("  (CardData has no defensive ability - skipping)");
                return;
            }
            
            Debug.Log($"  Defensive ability assigned: {testCardData.defensiveAbility.displayName}");
            
            if (targetCard != null)
            {
                Debug.Log($"  Triggering defensive ability on target...");
                testCard.ActivateDefensiveAbility(targetCard);
                Debug.Log($"  Ability triggered: PASS (check target for effect)");
            }
            else
            {
                Debug.Log("  (No target card assigned - cannot test execution)");
            }
        }
    }
}
