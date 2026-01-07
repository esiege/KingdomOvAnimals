using UnityEditor;
using UnityEngine;
using KOA.Data;
using KOA.Abilities.Behaviors;

namespace KOA.Editor
{
    /// <summary>
    /// Editor script to create initial card and ability data assets.
    /// Run from Tools > KingdomOvAnimals > Create Initial Data
    /// </summary>
    public static class InitialDataCreator
    {
        [MenuItem("Tools/KingdomOvAnimals/Create Initial Data")]
        public static void CreateInitialData()
        {
            // Ensure directories exist
            EnsureDirectoryExists("Assets/Resources/Abilities");
            EnsureDirectoryExists("Assets/Resources/Cards");
            EnsureDirectoryExists("Assets/Resources/Decks");
            
            // Create abilities first
            CreateAbilities();
            
            // Create cards (which reference abilities)
            CreateCards();
            
            // Create starter deck
            CreateStarterDeck();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[InitialDataCreator] Initial data creation complete!");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        #region Abilities

        private static void CreateAbilities()
        {
            // Offensive Abilities
            CreateAbility("claw_attack", "Claw Attack", "Deal {damage} damage", "DamageAbility", 2, 0, 0, TargetType.SingleEnemy);
            CreateAbility("stomp", "Stomp", "Deal {damage} damage", "DamageAbility", 3, 0, 0, TargetType.SingleEnemy);
            CreateAbility("poison_bite", "Poison Bite", "Deal {damage} damage and poison for {duration} turns", "PoisonAbility", 1, 0, 3, TargetType.SingleEnemy);
            CreateAbility("dive_attack", "Dive Attack", "Deal {damage} damage", "DamageAbility", 2, 0, 0, TargetType.SingleEnemy);
            CreateAbility("maul", "Maul", "Deal {damage} damage", "DamageAbility", 3, 0, 0, TargetType.SingleEnemy);
            
            // Defensive Abilities
            CreateAbility("roar", "Roar", "Buff attack by {damage} for {duration} turns", "BuffAttackAbility", 1, 0, 2, TargetType.SingleAlly);
            CreateAbility("thick_skin", "Thick Skin", "Heal {heal} health", "HealAbility", 0, 2, 0, TargetType.Self);
            CreateAbility("shed_skin", "Shed Skin", "Heal {heal} health", "HealAbility", 0, 2, 0, TargetType.Self);
            CreateAbility("evasion", "Evasion", "Avoid the next attack (placeholder)", "ReturnToHandAbility", 0, 0, 0, TargetType.Self);
            CreateAbility("hibernate", "Hibernate", "Heal {heal} health", "HealAbility", 0, 3, 0, TargetType.Self);
            
            Debug.Log("[InitialDataCreator] Created 10 abilities");
        }

        private static AbilityData CreateAbility(string id, string name, string desc, string behaviorType, int damage, int heal, int duration, TargetType targetType)
        {
            string path = $"Assets/Resources/Abilities/{id}.asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<AbilityData>(path);
            if (existing != null)
            {
                Debug.Log($"[InitialDataCreator] Ability already exists: {id}");
                return existing;
            }
            
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.id = id;
            ability.displayName = name;
            ability.description = desc;
            ability.behaviorType = behaviorType;
            ability.damage = damage;
            ability.healAmount = heal;
            ability.duration = duration;
            ability.targetType = targetType;
            ability.animationType = AnimationType.None;
            
            AssetDatabase.CreateAsset(ability, path);
            Debug.Log($"[InitialDataCreator] Created ability: {name}");
            
            return ability;
        }

        #endregion

        #region Cards

        private static void CreateCards()
        {
            // Load abilities
            var clawAttack = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/claw_attack.asset");
            var roar = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/roar.asset");
            var stomp = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/stomp.asset");
            var thickSkin = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/thick_skin.asset");
            var poisonBite = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/poison_bite.asset");
            var shedSkin = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/shed_skin.asset");
            var diveAttack = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/dive_attack.asset");
            var evasion = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/evasion.asset");
            var maul = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/maul.asset");
            var hibernate = AssetDatabase.LoadAssetAtPath<AbilityData>("Assets/Resources/Abilities/hibernate.asset");
            
            // Create cards
            CreateCard("lion", "Lion", "King of the jungle", 3, 2, clawAttack, roar);
            CreateCard("elephant", "Elephant", "Massive and sturdy", 5, 4, stomp, thickSkin);
            CreateCard("snake", "Snake", "Venomous and sneaky", 2, 1, poisonBite, shedSkin);
            CreateCard("eagle", "Eagle", "Swift aerial hunter", 2, 2, diveAttack, evasion);
            CreateCard("bear", "Bear", "Powerful forest dweller", 4, 3, maul, hibernate);
            
            Debug.Log("[InitialDataCreator] Created 5 cards");
        }

        private static CardData CreateCard(string id, string name, string desc, int health, int manaCost, AbilityData offensive, AbilityData defensive)
        {
            string path = $"Assets/Resources/Cards/{id}.asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (existing != null)
            {
                Debug.Log($"[InitialDataCreator] Card already exists: {id}");
                return existing;
            }
            
            var card = ScriptableObject.CreateInstance<CardData>();
            card.id = id;
            card.displayName = name;
            card.description = desc;
            card.health = health;
            card.manaCost = manaCost;
            card.offensiveAbility = offensive;
            card.defensiveAbility = defensive;
            card.cardType = CardType.Creature;
            card.faction = "Beast";
            
            AssetDatabase.CreateAsset(card, path);
            Debug.Log($"[InitialDataCreator] Created card: {name}");
            
            return card;
        }

        #endregion

        #region Decks

        private static void CreateStarterDeck()
        {
            string path = "Assets/Resources/Decks/starter_deck.asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<DeckData>(path);
            if (existing != null)
            {
                Debug.Log("[InitialDataCreator] Starter deck already exists");
                return;
            }
            
            // Load all cards
            var lion = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Cards/lion.asset");
            var elephant = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Cards/elephant.asset");
            var snake = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Cards/snake.asset");
            var eagle = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Cards/eagle.asset");
            var bear = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Resources/Cards/bear.asset");
            
            var deck = ScriptableObject.CreateInstance<DeckData>();
            deck.id = "starter_deck";
            deck.deckName = "Starter Deck";
            deck.description = "A balanced deck for new players";
            
            // Add 2 of each card (10 total)
            deck.cards = new System.Collections.Generic.List<CardData>
            {
                lion, lion,
                elephant, elephant,
                snake, snake,
                eagle, eagle,
                bear, bear
            };
            
            AssetDatabase.CreateAsset(deck, path);
            Debug.Log("[InitialDataCreator] Created starter deck with 10 cards");
        }

        #endregion
    }
}
