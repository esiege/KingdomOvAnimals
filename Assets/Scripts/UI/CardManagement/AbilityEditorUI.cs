using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Data;
using KOA.Abilities;

namespace KOA.UI
{
    /// <summary>
    /// UI for editing AbilityData assets in the Card Management scene.
    /// Provides list view, search, and editing capabilities.
    /// Shows conditional fields based on the selected behavior's RequiredFields.
    /// </summary>
    public class AbilityEditorUI : MonoBehaviour
    {
        [Header("List Panel")]
        [SerializeField] private Transform abilityListContent;
        [SerializeField] private GameObject abilityListItemPrefab;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button newAbilityButton;

        [Header("Editor Panel")]
        [SerializeField] private GameObject editorPanel;
        [SerializeField] private TMP_InputField idField;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField descriptionField;
        [SerializeField] private TMP_Dropdown behaviorDropdown;
        
        [Header("Conditional Fields")]
        [SerializeField] private GameObject damageFieldContainer;
        [SerializeField] private TMP_InputField damageField;
        [SerializeField] private GameObject healAmountFieldContainer;
        [SerializeField] private TMP_InputField healAmountField;
        [SerializeField] private GameObject durationFieldContainer;
        [SerializeField] private TMP_InputField durationField;
        
        [Header("Other Fields")]
        [SerializeField] private TMP_Dropdown targetTypeDropdown;
        [SerializeField] private TMP_Dropdown animationTypeDropdown;
        [SerializeField] private TMP_Dropdown effectPrefabDropdown;

        [Header("Editor Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private AbilityData selectedAbility;
        private List<AbilityData> allAbilities = new List<AbilityData>();
        private List<AbilityListItem> listItems = new List<AbilityListItem>();
        private List<GameObject> effectPrefabs = new List<GameObject>();
        private List<string> behaviorTypeNames = new List<string>();

        private void Start()
        {
            SetupButtonListeners();
            SetupDropdowns();
            LoadAllData();
            RefreshAbilityList();
            
            // Hide editor panel until an ability is selected
            if (editorPanel != null)
                editorPanel.SetActive(false);
            
            // Hide confirmation dialog
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void SetupButtonListeners()
        {
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            if (newAbilityButton != null)
                newAbilityButton.onClick.AddListener(CreateNewAbility);
            
            if (saveButton != null)
                saveButton.onClick.AddListener(SaveAbility);
            
            if (deleteButton != null)
                deleteButton.onClick.AddListener(PromptDeleteAbility);
            
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmDelete);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelDelete);
            
            // Listen for behavior dropdown changes to update conditional fields
            if (behaviorDropdown != null)
                behaviorDropdown.onValueChanged.AddListener(OnBehaviorChanged);
        }

        private void SetupDropdowns()
        {
            // Setup behavior dropdown
            if (behaviorDropdown != null)
            {
                behaviorDropdown.ClearOptions();
                behaviorTypeNames = new List<string> { "" }; // Empty option for "None"
                behaviorTypeNames.AddRange(AbilityBehaviorRegistry.BehaviorTypeNames);
                
                var displayNames = new List<string> { "(None)" };
                displayNames.AddRange(AbilityBehaviorRegistry.BehaviorDisplayNames);
                behaviorDropdown.AddOptions(displayNames);
            }
            
            // Setup target type dropdown
            if (targetTypeDropdown != null)
            {
                targetTypeDropdown.ClearOptions();
                var targetTypes = System.Enum.GetNames(typeof(TargetType)).ToList();
                targetTypeDropdown.AddOptions(targetTypes);
            }
            
            // Setup animation type dropdown
            if (animationTypeDropdown != null)
            {
                animationTypeDropdown.ClearOptions();
                var animTypes = System.Enum.GetNames(typeof(AnimationType)).ToList();
                animationTypeDropdown.AddOptions(animTypes);
            }
        }

        private void LoadAllData()
        {
            // Load all abilities from Resources
            allAbilities = Resources.LoadAll<AbilityData>("Abilities").ToList();
            Debug.Log($"[AbilityEditor] Loaded {allAbilities.Count} abilities");
            
            // Load effect prefabs for dropdown
            LoadEffectPrefabs();
        }

        private void LoadEffectPrefabs()
        {
            effectPrefabs.Clear();
            
            // NOTE: AbilityEffect system deprecated in Story 036
            // Effect prefabs are no longer used - abilities use AbilityBehavior instead
            var loadedPrefabs = Resources.LoadAll<GameObject>("Effects");
            foreach (var prefab in loadedPrefabs)
            {
                // Just add all prefabs from Effects folder
                effectPrefabs.Add(prefab);
            }
            
            Debug.Log($"[AbilityEditor] Loaded {effectPrefabs.Count} effect prefabs");
            
            // Populate dropdown
            if (effectPrefabDropdown != null)
            {
                effectPrefabDropdown.ClearOptions();
                var options = new List<string> { "(None)" };
                options.AddRange(effectPrefabs.Select(p => p.name));
                effectPrefabDropdown.AddOptions(options);
            }
        }

        #region Ability List

        public void RefreshAbilityList()
        {
            // Clear existing items
            foreach (var item in listItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            listItems.Clear();
            
            // Reload abilities in case new ones were created
            allAbilities = Resources.LoadAll<AbilityData>("Abilities").ToList();
            
            // Filter by search
            string filter = searchField != null ? searchField.text.ToLower() : "";
            var filteredAbilities = string.IsNullOrEmpty(filter) 
                ? allAbilities 
                : allAbilities.Where(a => a.displayName.ToLower().Contains(filter) || a.id.ToLower().Contains(filter)).ToList();
            
            // Create list items
            foreach (var ability in filteredAbilities)
            {
                CreateListItem(ability);
            }
            
            Debug.Log($"[AbilityEditor] Displaying {filteredAbilities.Count} abilities");
        }

        private void CreateListItem(AbilityData ability)
        {
            if (abilityListItemPrefab == null)
            {
                Debug.LogError("[AbilityEditor] abilityListItemPrefab is null! Trying to load from Resources...");
                abilityListItemPrefab = Resources.Load<GameObject>("UI/AbilityListItem");
                if (abilityListItemPrefab == null)
                {
                    Debug.LogError("[AbilityEditor] Failed to load AbilityListItem prefab from Resources/UI/");
                    return;
                }
            }
            
            if (abilityListContent == null)
            {
                Debug.LogError("[AbilityEditor] abilityListContent is null!");
                return;
            }
            
            var itemObj = Instantiate(abilityListItemPrefab, abilityListContent);
            var listItem = itemObj.GetComponent<AbilityListItem>();
            
            if (listItem != null)
            {
                listItem.Initialize(ability, OnAbilitySelected);
                listItems.Add(listItem);
            }
        }

        private void OnSearchChanged(string value)
        {
            RefreshAbilityList();
        }

        private void OnAbilitySelected(AbilityData ability)
        {
            SelectAbility(ability);
        }

        private void OnBehaviorChanged(int index)
        {
            UpdateConditionalFieldVisibility();
        }

        private void UpdateConditionalFieldVisibility()
        {
            // Get the selected behavior's required fields
            string[] requiredFields = new string[0];
            
            if (behaviorDropdown != null && behaviorDropdown.value > 0)
            {
                string typeName = behaviorTypeNames[behaviorDropdown.value];
                requiredFields = AbilityBehaviorRegistry.GetRequiredFields(typeName);
            }
            
            // Show/hide conditional field containers
            bool showDamage = requiredFields.Contains("damage");
            bool showHeal = requiredFields.Contains("healAmount");
            bool showDuration = requiredFields.Contains("duration");
            
            if (damageFieldContainer != null) damageFieldContainer.SetActive(showDamage);
            if (healAmountFieldContainer != null) healAmountFieldContainer.SetActive(showHeal);
            if (durationFieldContainer != null) durationFieldContainer.SetActive(showDuration);
            
            Debug.Log($"[AbilityEditor] Conditional fields - Damage:{showDamage} Heal:{showHeal} Duration:{showDuration}");
        }

        #endregion

        #region Ability Selection & Editing

        public void SelectAbility(AbilityData ability)
        {
            selectedAbility = ability;
            
            if (editorPanel != null)
                editorPanel.SetActive(true);
            
            // Populate fields
            if (idField != null) idField.text = ability.id;
            if (nameField != null) nameField.text = ability.displayName;
            if (descriptionField != null) descriptionField.text = ability.description ?? "";
            if (damageField != null) damageField.text = ability.damage.ToString();
            if (healAmountField != null) healAmountField.text = ability.healAmount.ToString();
            if (durationField != null) durationField.text = ability.duration.ToString();
            
            // Set behavior dropdown
            if (behaviorDropdown != null)
            {
                int behaviorIndex = behaviorTypeNames.IndexOf(ability.behaviorType ?? "");
                behaviorDropdown.value = behaviorIndex >= 0 ? behaviorIndex : 0;
            }
            
            // Update conditional field visibility based on selected behavior
            UpdateConditionalFieldVisibility();
            
            // Set target type dropdown
            if (targetTypeDropdown != null)
                targetTypeDropdown.value = (int)ability.targetType;
            
            // Set animation type dropdown
            if (animationTypeDropdown != null)
                animationTypeDropdown.value = (int)ability.animationType;
            
            // Set effect prefab dropdown
            SetEffectPrefabDropdown(ability.effectPrefab);
            
            // Update list selection visual
            foreach (var item in listItems)
            {
                item.SetSelected(item.Ability == ability);
            }
            
            Debug.Log($"[AbilityEditor] Selected ability: {ability.displayName}");
        }

        private void SetEffectPrefabDropdown(GameObject effectPrefab)
        {
            if (effectPrefabDropdown == null) return;
            
            if (effectPrefab == null)
            {
                effectPrefabDropdown.value = 0; // (None)
            }
            else
            {
                int index = effectPrefabs.FindIndex(p => p == effectPrefab);
                effectPrefabDropdown.value = index + 1; // +1 because of (None) option
            }
        }

        private GameObject GetEffectPrefabFromDropdown()
        {
            if (effectPrefabDropdown == null || effectPrefabDropdown.value == 0)
                return null;
            
            int prefabIndex = effectPrefabDropdown.value - 1; // -1 because of (None) option
            if (prefabIndex >= 0 && prefabIndex < effectPrefabs.Count)
                return effectPrefabs[prefabIndex];
            
            return null;
        }

        #endregion

        #region Save/Create/Delete

        public void SaveAbility()
        {
            if (selectedAbility == null) return;
            
            // Update ability data
            selectedAbility.id = idField != null ? idField.text : selectedAbility.id;
            selectedAbility.displayName = nameField != null ? nameField.text : selectedAbility.displayName;
            selectedAbility.description = descriptionField != null ? descriptionField.text : selectedAbility.description;
            
            // Save behavior type
            if (behaviorDropdown != null && behaviorDropdown.value > 0)
                selectedAbility.behaviorType = behaviorTypeNames[behaviorDropdown.value];
            else
                selectedAbility.behaviorType = "";
            
            // Save conditional fields (only if visible/relevant)
            selectedAbility.damage = damageField != null ? int.Parse(damageField.text) : selectedAbility.damage;
            selectedAbility.healAmount = healAmountField != null ? int.Parse(healAmountField.text) : selectedAbility.healAmount;
            selectedAbility.duration = durationField != null ? int.Parse(durationField.text) : selectedAbility.duration;
            
            if (targetTypeDropdown != null)
                selectedAbility.targetType = (TargetType)targetTypeDropdown.value;
            
            if (animationTypeDropdown != null)
                selectedAbility.animationType = (AnimationType)animationTypeDropdown.value;
            
            selectedAbility.effectPrefab = GetEffectPrefabFromDropdown();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(selectedAbility);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[AbilityEditor] Saved ability: {selectedAbility.displayName} (Behavior: {selectedAbility.behaviorType})");
            #endif
            
            RefreshAbilityList();
            SelectAbility(selectedAbility);
        }

        public void CreateNewAbility()
        {
            #if UNITY_EDITOR
            var newAbility = ScriptableObject.CreateInstance<AbilityData>();
            newAbility.id = "new_ability_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            newAbility.displayName = "New Ability";
            newAbility.behaviorType = "";
            newAbility.damage = 0;
            newAbility.healAmount = 0;
            newAbility.duration = 0;
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists("Assets/Resources/Abilities"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/Abilities");
            }
            
            string path = "Assets/Resources/Abilities/" + newAbility.id + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(newAbility, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[AbilityEditor] Created new ability: {path}");
            
            RefreshAbilityList();
            SelectAbility(newAbility);
            #else
            Debug.LogWarning("[AbilityEditor] Cannot create abilities outside of Editor");
            #endif
        }

        public void PromptDeleteAbility()
        {
            if (selectedAbility == null) return;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
                if (confirmText != null)
                    confirmText.text = $"Delete '{selectedAbility.displayName}'?\nThis cannot be undone.";
            }
        }

        private void ConfirmDelete()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
            
            DeleteSelectedAbility();
        }

        private void CancelDelete()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void DeleteSelectedAbility()
        {
            if (selectedAbility == null) return;
            
            #if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(selectedAbility);
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log($"[AbilityEditor] Deleting ability: {path}");
                UnityEditor.AssetDatabase.DeleteAsset(path);
                UnityEditor.AssetDatabase.Refresh();
            }
            
            selectedAbility = null;
            if (editorPanel != null)
                editorPanel.SetActive(false);
            
            RefreshAbilityList();
            #else
            Debug.LogWarning("[AbilityEditor] Cannot delete abilities outside of Editor");
            #endif
        }

        #endregion
    }
}
