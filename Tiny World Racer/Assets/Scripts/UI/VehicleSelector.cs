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
            if (!instance)
                instance = FindFirstObjectByType<VehicleSelector>();
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart.AddListener(OnGameStart);
            
            if (autoSpawnOnStart && GameManager.Instance.IsGameStarted)
            {
                SpawnSelectedVehicle();
            }
        }
        else if (autoSpawnOnStart)
        {
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
        if (currentVehicleInstance != null && keepPreviewAsPlayer)
        {
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
        
        if (currentVehicleInstance != null)
        {
            Destroy(currentVehicleInstance);
        }
        
        if (availableVehicles.Count == 0)
        {
            Debug.LogError("No vehicles configured in VehicleSelector!");
            return;
        }
        
        if (selectedVehicleIndex < 0 || selectedVehicleIndex >= availableVehicles.Count)
        {
            Debug.LogWarning($"Invalid vehicle index {selectedVehicleIndex}, using default");
            selectedVehicleIndex = 0;
        }
        
        VehicleData selectedVehicle = availableVehicles[selectedVehicleIndex];
        
        if (selectedVehicle.vehiclePrefab == null)
        {
            Debug.LogError($"Vehicle {selectedVehicle.vehicleName} has no prefab assigned!");
            return;
        }
        
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        currentVehicleInstance = Instantiate(selectedVehicle.vehiclePrefab, spawnPosition, spawnRotation);
        currentVehicleInstance.name = selectedVehicle.vehicleName + " (Player)";
        
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
        if (index < 0 || index >= availableVehicles.Count)
        {
            Debug.LogWarning($"Invalid preview vehicle index: {index}");
            return;
        }
        
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
        
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        currentVehicleInstance = Instantiate(vehicleToPreview.vehiclePrefab, spawnPosition, spawnRotation);
        currentVehicleInstance.name = vehicleToPreview.vehicleName + " (Preview)";
        
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
            
            MovementController movement = currentVehicleInstance.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.ResetVelocity();
            }
            
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SetFollowTarget(currentVehicleInstance.transform);
            }
        }
    }
    
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