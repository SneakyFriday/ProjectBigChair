using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleSelectionUI : MonoBehaviour
{
    [Header("UI References - Diese Buttons/Texte musst du zuweisen")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    
    [Header("Vehicle Info Display")]
    [SerializeField] private TextMeshProUGUI vehicleNameText;
    [SerializeField] private TextMeshProUGUI vehicleDescriptionText;
    [SerializeField] private Image vehicleIconImage;
    [SerializeField] private TextMeshProUGUI indexText;
    
    [Header("Vehicle Stats")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Slider handlingSlider;
    [SerializeField] private Slider accelerationSlider;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI handlingText;
    [SerializeField] private TextMeshProUGUI accelerationText;
    
    [Header("Panel Management")]
    [SerializeField] private GameObject vehicleSelectionPanel;
    [SerializeField] private GameObject mainMenuPanel;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private int currentPreviewIndex = 0;
    private VehicleSelector vehicleSelector;
    
    void Start()
    {
        if (enableDebugLogs)
            Debug.Log("VehicleSelectionUI Start() called");
            
        vehicleSelector = VehicleSelector.Instance;
        if (!vehicleSelector)
        {
            Debug.LogError("VehicleSelector nicht gefunden! Stelle sicher dass ein VehicleSelector in der Szene ist.");
            return;
        }
        
        SetupButtons();
        ValidateReferences();
        
        currentPreviewIndex = vehicleSelector.GetSelectedVehicleIndex();
        UpdateVehicleDisplay();
    }
    
    void ValidateReferences()
    {
        if (!vehicleSelectionPanel)
            Debug.LogError("vehicleSelectionPanel is not assigned in Inspector!");
        if (!mainMenuPanel)
            Debug.LogWarning("mainMenuPanel is not assigned in Inspector!");
        if (!previousButton)
            Debug.LogWarning("previousButton is not assigned!");
        if (!nextButton)
            Debug.LogWarning("nextButton is not assigned!");
        if (!confirmButton)
            Debug.LogWarning("confirmButton is not assigned!");
        if (!backButton)
            Debug.LogWarning("backButton is not assigned!");
    }
    
    void SetupButtons()
    {
        if (previousButton)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(SelectPreviousVehicle);
            if (enableDebugLogs) Debug.Log("Previous button listener added");
        }
        
        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(SelectNextVehicle);
            if (enableDebugLogs) Debug.Log("Next button listener added");
        }
        
        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmVehicleSelection);
            if (enableDebugLogs) Debug.Log("Confirm button listener added");
        }
        
        if (backButton)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToMainMenu);
            if (enableDebugLogs) Debug.Log("Back button listener added");
        }
    }
    
    public void SelectPreviousVehicle()
    {
        if (!vehicleSelector) return;
        
        if (!vehicleSelector.IsVehicleSelectionAvailable())
        {
            Debug.LogWarning("Vehicle selection not available during gameplay!");
            return;
        }
        
        int vehicleCount = vehicleSelector.GetVehicleCount();
        if (vehicleCount > 1)
        {
            currentPreviewIndex = (currentPreviewIndex - 1 + vehicleCount) % vehicleCount;
            UpdateVehicleDisplay();
            PlayHoverSound();
        }
    }
    
    public void SelectNextVehicle()
    {
        if (!vehicleSelector) return;
        
        if (!vehicleSelector.IsVehicleSelectionAvailable())
        {
            Debug.LogWarning("Vehicle selection not available during gameplay!");
            return;
        }
        
        int vehicleCount = vehicleSelector.GetVehicleCount();
        if (vehicleCount > 1)
        {
            currentPreviewIndex = (currentPreviewIndex + 1) % vehicleCount;
            UpdateVehicleDisplay();
            
            PlayHoverSound();
        }
    }
    
    public void ConfirmVehicleSelection()
    {
        if (!vehicleSelector) return;
        vehicleSelector.SelectVehicle(currentPreviewIndex);
        PlayClickSound();
        BackToMainMenu();
        
        Debug.Log($"Vehicle selected: {vehicleSelector.GetCurrentVehicleData()?.vehicleName}");
    }
    
    public void BackToMainMenu()
    {
        if (vehicleSelectionPanel)
            vehicleSelectionPanel.SetActive(false);
        
        if (mainMenuPanel)
            mainMenuPanel.SetActive(true);
        
        Debug.Log("Returned to main menu - preview vehicle remains spawned");
    }
    
    void UpdateVehicleDisplay()
    {
        if (!vehicleSelector) return;
        
        var vehicles = vehicleSelector.GetAvailableVehicles();
        if (currentPreviewIndex >= 0 && currentPreviewIndex < vehicles.Count)
        {
            VehicleData currentVehicle = vehicles[currentPreviewIndex];
            
            if (vehicleNameText)
                vehicleNameText.text = currentVehicle.vehicleName;
            
            if (vehicleDescriptionText)
                vehicleDescriptionText.text = currentVehicle.description;
            
            if (vehicleIconImage && currentVehicle.vehicleIcon)
            {
                vehicleIconImage.sprite = currentVehicle.vehicleIcon;
                vehicleIconImage.color = Color.white;
            }
            else if (vehicleIconImage)
            {
                vehicleIconImage.color = Color.clear;
            }
            
            if (indexText)
            {
                indexText.text = $"{currentPreviewIndex + 1}/{vehicles.Count}";
            }
            
            UpdateVehicleStats(currentVehicle);
        }
        
        UpdateButtonStates();
    }
    
    void UpdateVehicleStats(VehicleData vehicle)
    {
        if (speedSlider)
        {
            speedSlider.value = vehicle.speed;
        }
        if (speedText)
        {
            speedText.text = $"{vehicle.speed}/5";
        }
        
        if (handlingSlider)
        {
            handlingSlider.value = vehicle.handling;
        }
        if (handlingText)
        {
            handlingText.text = $"{vehicle.handling}/5";
        }
        
        if (accelerationSlider)
        {
            accelerationSlider.value = vehicle.acceleration;
        }
        if (accelerationText)
        {
            accelerationText.text = $"{vehicle.acceleration}/5";
        }
    }
    
    void UpdateButtonStates()
    {
        if (!vehicleSelector) return;
        
        bool hasMultipleVehicles = vehicleSelector.GetVehicleCount() > 1;
        bool selectionAvailable = vehicleSelector.IsVehicleSelectionAvailable();
        
        if (previousButton)
            previousButton.interactable = hasMultipleVehicles && selectionAvailable;
        
        if (nextButton)
            nextButton.interactable = hasMultipleVehicles && selectionAvailable;
        
        if (confirmButton)
            confirmButton.interactable = selectionAvailable;
    }
    
    void PlayHoverSound()
    {
        if (MenuAudioManager.Instance)
        {
            MenuAudioManager.Instance.PlayHoverSound();
        }
    }
    
    void PlayClickSound()
    {
        if (MenuAudioManager.Instance)
        {
            MenuAudioManager.Instance.PlayClickSound();
        }
    }
    
    /// <summary>
    /// Öffnet das Vehicle Selection Panel (kann von MainMenu aufgerufen werden)
    /// </summary>
    public void OpenVehicleSelection()
    {
        Debug.Log("=== OpenVehicleSelection called! ===");
        
        // Check VehicleSelector
        if (!vehicleSelector)
        {
            vehicleSelector = VehicleSelector.Instance;
            if (!vehicleSelector)
            {
                Debug.LogError("VehicleSelector is null! Make sure VehicleSelector exists in the scene.");
                return;
            }
        }
        
        Debug.Log("VehicleSelector found!");
        
        if (!vehicleSelector.IsVehicleSelectionAvailable())
        {
            Debug.LogWarning("Vehicle selection not available! IsVehicleSelectionAvailable() returned false.");
            return;
        }
        
        Debug.Log("Vehicle selection is available!");
        
        currentPreviewIndex = vehicleSelector.GetSelectedVehicleIndex();
        Debug.Log($"Current preview index: {currentPreviewIndex}");
        
        if (mainMenuPanel)
        {
            Debug.Log($"Deactivating main menu panel (current state: {mainMenuPanel.activeSelf})");
            mainMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("mainMenuPanel reference is missing! Assign it in the Inspector.");
        }
        
        if (vehicleSelectionPanel)
        {
            Debug.Log($"Activating vehicle selection panel (current state: {vehicleSelectionPanel.activeSelf})");
            vehicleSelectionPanel.SetActive(true);
            Debug.Log($"Vehicle selection panel active state after activation: {vehicleSelectionPanel.activeSelf}");
            
            if (!vehicleSelectionPanel.activeInHierarchy)
            {
                Debug.LogError("Vehicle selection panel is not active in hierarchy! Check parent GameObjects.");
                Transform current = vehicleSelectionPanel.transform;
                while (current != null)
                {
                    Debug.Log($"Parent: {current.name} - Active: {current.gameObject.activeSelf}");
                    current = current.parent;
                }
            }
        }
        else
        {
            Debug.LogError("vehicleSelectionPanel reference is missing! Assign it in the Inspector.");
            return;
        }
        
        UpdateVehicleDisplay();
        Debug.Log("=== OpenVehicleSelection completed! ===");
    }
    
    /// <summary>
    /// Aktualisiert das Display (falls Fahrzeuge zur Laufzeit geändert werden)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateVehicleDisplay();
    }
    
    [ContextMenu("Test Show Vehicle Selection Panel")]
    public void TestShowPanel()
    {
        if (vehicleSelectionPanel)
        {
            vehicleSelectionPanel.SetActive(true);
            Debug.Log($"Test: Panel activated. Active in hierarchy: {vehicleSelectionPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("Test failed: vehicleSelectionPanel is not assigned!");
        }
    }
}