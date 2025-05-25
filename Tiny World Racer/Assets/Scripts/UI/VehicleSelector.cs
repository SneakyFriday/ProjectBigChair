using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VehicleData
{
    public string vehicleName = "Vehicle";
    public GameObject vehiclePrefab;
    public Sprite vehicleIcon;
    [TextArea(2, 4)]
    public string description = "Vehicle description";
    
    [Header("Stats (1-5)")]
    [Range(1, 5)] public int speed = 3;
    [Range(1, 5)] public int handling = 3;
    [Range(1, 5)] public int acceleration = 3;
    
    [Header("Camera Settings")]
    public CameraManager.VehicleType vehicleType = CameraManager.VehicleType.Standard;
}

public class VehicleSelector : MonoBehaviour
{
    private static VehicleSelector instance;
    public static VehicleSelector Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<VehicleSelector>();
            return instance;
        }
    }
    
    [Header("Vehicle Configuration")]
    [SerializeField] private List<VehicleData> availableVehicles = new List<VehicleData>();
    [SerializeField] private int defaultVehicleIndex = 0;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool autoSpawnOnStart = false;
    [SerializeField] private bool keepPreviewAsPlayer = true;
    
    [Header("Current State")]
    [SerializeField] private int selectedVehicleIndex = 0;
    [SerializeField] private GameObject currentVehicleInstance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        selectedVehicleIndex = defaultVehicleIndex;
    }
    
    private void Start()
    {
        // Subscribe to GameManager events if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.AddListener(OnGameStart);
            
            // If autoSpawn is enabled and game is already started, spawn now
            if (autoSpawnOnStart && GameManager.Instance.IsGameStarted)
            {
                SpawnSelectedVehicle();
            }
        }
        else if (autoSpawnOnStart)
        {
            // If no GameManager, spawn immediately if autoSpawn is enabled
            SpawnSelectedVehicle();
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.RemoveListener(OnGameStart);
        }
    }
    
    private void OnGameStart()
    {
        SpawnSelectedVehicle();
    }
    
    /// <summary>
    /// Spawns the currently selected vehicle at the spawn point
    /// </summary>
    public void SpawnSelectedVehicle()
    {
        // If we already have a vehicle (from preview) and keepPreviewAsPlayer is enabled, don't spawn a new one
        if (currentVehicleInstance != null && keepPreviewAsPlayer)
        {
            // Just rename it from preview to player
            if (currentVehicleInstance.name.Contains("(Preview)"))
            {
                VehicleData currentVehicle = availableVehicles[selectedVehicleIndex];
                currentVehicleInstance.name = currentVehicle.vehicleName + " (Player)";
                Debug.Log($"Converted preview vehicle to player vehicle: {currentVehicle.vehicleName}");
                return;
            }
            else
            {
                Debug.Log("Vehicle already spawned, skipping spawn");
                return;
            }
        }
        
        // Remove existing vehicle if any
        if (currentVehicleInstance != null)
        {
            Destroy(currentVehicleInstance);
        }
        
        // Check if we have vehicles configured
        if (availableVehicles.Count == 0)
        {
            Debug.LogError("No vehicles configured in VehicleSelector!");
            return;
        }
        
        // Validate index
        if (selectedVehicleIndex < 0 || selectedVehicleIndex >= availableVehicles.Count)
        {
            Debug.LogWarning($"Invalid vehicle index {selectedVehicleIndex}, using default");
            selectedVehicleIndex = 0;
        }
        
        // Get selected vehicle data
        VehicleData selectedVehicle = availableVehicles[selectedVehicleIndex];
        
        if (selectedVehicle.vehiclePrefab == null)
        {
            Debug.LogError($"Vehicle {selectedVehicle.vehicleName} has no prefab assigned!");
            return;
        }
        
        // Determine spawn position and rotation
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        // Spawn the vehicle
        currentVehicleInstance = Instantiate(selectedVehicle.vehiclePrefab, spawnPosition, spawnRotation);
        currentVehicleInstance.name = selectedVehicle.vehicleName + " (Player)";
        
        // Update CameraManager to follow the new vehicle
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetFollowTarget(currentVehicleInstance.transform);
            CameraManager.Instance.SetCockpitPreset(selectedVehicle.vehicleType);
            Debug.Log($"CameraManager updated to follow {selectedVehicle.vehicleName} with {selectedVehicle.vehicleType} preset");
        }
        
        Debug.Log($"Spawned vehicle: {selectedVehicle.vehicleName} at {spawnPosition}");
    }
    
    /// <summary>
    /// Spawns a vehicle for preview (without starting the game)
    /// </summary>
    public void SpawnPreviewVehicle(int index)
    {
        // Validate index
        if (index < 0 || index >= availableVehicles.Count)
        {
            Debug.LogWarning($"Invalid preview vehicle index: {index}");
            return;
        }
        
        // Remove existing vehicle if any
        if (currentVehicleInstance != null)
        {
            Destroy(currentVehicleInstance);
        }
        
        VehicleData vehicleToPreview = availableVehicles[index];
        
        if (vehicleToPreview.vehiclePrefab == null)
        {
            Debug.LogError($"Vehicle {vehicleToPreview.vehicleName} has no prefab assigned!");
            return;
        }
        
        // Determine spawn position and rotation
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        // Spawn the vehicle
        currentVehicleInstance = Instantiate(vehicleToPreview.vehiclePrefab, spawnPosition, spawnRotation);
        currentVehicleInstance.name = vehicleToPreview.vehicleName + " (Preview)";
        
        // Update CameraManager to follow the preview vehicle
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetFollowTarget(currentVehicleInstance.transform);
            CameraManager.Instance.SetCockpitPreset(vehicleToPreview.vehicleType);
        }
        
        Debug.Log($"Spawned preview vehicle: {vehicleToPreview.vehicleName}");
    }
    
    /// <summary>
    /// Select a vehicle by index
    /// </summary>
    public void SelectVehicle(int index)
    {
        if (index >= 0 && index < availableVehicles.Count)
        {
            selectedVehicleIndex = index;
            Debug.Log($"Selected vehicle: {availableVehicles[index].vehicleName}");
            
            // If we already have a preview vehicle and it's the selected one, just rename it
            if (currentVehicleInstance != null && currentVehicleInstance.name.Contains("(Preview)"))
            {
                currentVehicleInstance.name = availableVehicles[index].vehicleName + " (Player)";
            }
        }
        else
        {
            Debug.LogWarning($"Invalid vehicle index: {index}");
        }
    }
    
    /// <summary>
    /// Get the currently selected vehicle data
    /// </summary>
    public VehicleData GetCurrentVehicleData()
    {
        if (selectedVehicleIndex >= 0 && selectedVehicleIndex < availableVehicles.Count)
        {
            return availableVehicles[selectedVehicleIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Get all available vehicles
    /// </summary>
    public List<VehicleData> GetAvailableVehicles()
    {
        return availableVehicles;
    }
    
    /// <summary>
    /// Get the number of available vehicles
    /// </summary>
    public int GetVehicleCount()
    {
        return availableVehicles.Count;
    }
    
    /// <summary>
    /// Get the currently selected vehicle index
    /// </summary>
    public int GetSelectedVehicleIndex()
    {
        return selectedVehicleIndex;
    }
    
    /// <summary>
    /// Check if vehicle selection is available (not during gameplay)
    /// </summary>
    public bool IsVehicleSelectionAvailable()
    {
        // Only allow vehicle selection when game hasn't started
        if (GameManager.Instance != null)
        {
            return !GameManager.Instance.IsGameStarted;
        }
        return true;
    }
    
    /// <summary>
    /// Get the current vehicle instance
    /// </summary>
    public GameObject GetCurrentVehicleInstance()
    {
        return currentVehicleInstance;
    }
    
    /// <summary>
    /// Get the current vehicle GameObject (for CameraManager compatibility)
    /// </summary>
    public GameObject GetCurrentVehicle()
    {
        return currentVehicleInstance;
    }
    
    /// <summary>
    /// Manually respawn the vehicle at spawn point
    /// </summary>
    public void RespawnVehicle()
    {
        if (currentVehicleInstance != null)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            
            currentVehicleInstance.transform.position = spawnPosition;
            currentVehicleInstance.transform.rotation = spawnRotation;
            
            // Reset velocity if vehicle has MovementController
            MovementController movement = currentVehicleInstance.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.ResetVelocity();
            }
            
            // Update camera to make sure it's still following
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SetFollowTarget(currentVehicleInstance.transform);
            }
        }
    }
    
    // Draw spawn point in editor
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnPoint.position, Vector3.one);
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);
        }
    }
}