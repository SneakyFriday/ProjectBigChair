using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VehicleData
{
    [Header("Vehicle Info")]
    public string vehicleName = "Vehicle";
    public GameObject vehiclePrefab;
    public Sprite vehicleIcon;
    
    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "Ein tolles Fahrzeug";
    
    [Header("Stats (nur für Anzeige)")]
    [Range(1, 5)] public int speed = 3;
    [Range(1, 5)] public int handling = 3;
    [Range(1, 5)] public int acceleration = 3;
    
    [Header("Camera Settings")]
    public CameraManager.VehicleType vehicleType = CameraManager.VehicleType.Standard;
    public Vector3 cockpitOffset = Vector3.zero;
}

public class VehicleSelector : MonoBehaviour
{
    public static VehicleSelector Instance { get; private set; }
    
    [Header("Available Vehicles")]
    [SerializeField] private List<VehicleData> availableVehicles = new List<VehicleData>();
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool useCurrentPositionAsSpawn = true;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera vehicleCamera;
    [SerializeField] private bool autoFindCamera = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5, -10);
    [SerializeField] private bool cameraFollowsVehicle = true;
    
    [Header("Game Control")]
    [SerializeField] private bool lockSelectionDuringGame = true;
    
    private GameObject currentVehicle;
    private bool gameIsActive = false;
    private MovementController currentMovementController;
    private int selectedVehicleIndex = 0;
    
    // Events
    public System.Action<VehicleData> OnVehicleChanged;
    public System.Action<GameObject> OnVehicleSpawned;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (autoFindCamera && !vehicleCamera)
        {
            vehicleCamera = Camera.main;
            if (!vehicleCamera)
            {
                vehicleCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Spawn Point setzen falls nicht definiert
        if (spawnPoint == null && useCurrentPositionAsSpawn)
        {
            GameObject spawnObj = new GameObject("VehicleSpawnPoint");
            spawnPoint = spawnObj.transform;
            spawnPoint.position = transform.position;
            spawnPoint.rotation = transform.rotation;
        }
    }
    
    private void Start()
    {
        // Erstes Fahrzeug spawnen falls noch keins existiert
        if (currentVehicle == null && availableVehicles.Count > 0)
        {
            SpawnVehicle(selectedVehicleIndex, false);
        }
    }
    
    /// <summary>
    /// Spawnt das ausgewählte Fahrzeug
    /// </summary>
    public void SpawnVehicle(int vehicleIndex, bool preservePosition = true)
    {
        // Prüfen ob Fahrzeugwechsel während des Spiels gesperrt ist
        if (lockSelectionDuringGame && gameIsActive)
        {
            Debug.LogWarning("Vehicle selection is locked during gameplay!");
            return;
        }
        
        if (vehicleIndex < 0 || vehicleIndex >= availableVehicles.Count)
        {
            Debug.LogError($"Invalid vehicle index: {vehicleIndex}");
            return;
        }
        
        VehicleData selectedVehicle = availableVehicles[vehicleIndex];
        if (selectedVehicle.vehiclePrefab == null)
        {
            Debug.LogError($"Vehicle prefab is null for {selectedVehicle.vehicleName}");
            return;
        }
        
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;
        
        // Position vom aktuellen Fahrzeug übernehmen falls gewünscht
        if (preservePosition && currentVehicle != null)
        {
            spawnPosition = currentVehicle.transform.position;
            spawnRotation = currentVehicle.transform.rotation;
        }
        
        // Altes Fahrzeug entfernen
        if (currentVehicle != null)
        {
            DestroyImmediate(currentVehicle);
        }
        
        // Neues Fahrzeug spawnen
        currentVehicle = Instantiate(selectedVehicle.vehiclePrefab, spawnPosition, spawnRotation);
        currentVehicle.name = selectedVehicle.vehicleName;
        
        // Stelle sicher, dass das Fahrzeug den Player-Tag hat
        if (!currentVehicle.CompareTag("Player"))
        {
            currentVehicle.tag = "Player";
            Debug.Log($"Added Player tag to {selectedVehicle.vehicleName}");
        }
        
        // MovementController referenzieren
        currentMovementController = currentVehicle.GetComponent<MovementController>();
        if (currentMovementController == null)
        {
            Debug.LogError($"No MovementController found on {selectedVehicle.vehicleName}!");
        }
        
        // Kamera-Setup
        SetupCamera();
        
        // Index aktualisieren
        selectedVehicleIndex = vehicleIndex;
        
        // Kamera-Manager über Fahrzeugwechsel informieren
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetFollowTarget(currentVehicle.transform);
            
            // Cockpit-Einstellungen für dieses Fahrzeug anwenden
            if (selectedVehicle.vehicleType != CameraManager.VehicleType.Standard)
            {
                CameraManager.Instance.SetCockpitPreset(selectedVehicle.vehicleType);
            }
            else
            {
                CameraManager.Instance.SetCockpitOffset(selectedVehicle.cockpitOffset, true);
            }
        }
        
        // Events auslösen
        OnVehicleChanged?.Invoke(selectedVehicle);
        OnVehicleSpawned?.Invoke(currentVehicle);
        
        Debug.Log($"Spawned vehicle: {selectedVehicle.vehicleName}");
    }
    
    private void SetupCamera()
    {
        if (vehicleCamera == null || currentVehicle == null) return;
        
        if (cameraFollowsVehicle)
        {
            // Kamera als Child des Fahrzeugs setzen
            vehicleCamera.transform.SetParent(currentVehicle.transform);
            vehicleCamera.transform.localPosition = cameraOffset;
            vehicleCamera.transform.localRotation = Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Wählt das nächste Fahrzeug in der Liste (nur wenn Spiel nicht läuft)
    /// </summary>
    public void SelectNextVehicle()
    {
        if (lockSelectionDuringGame && gameIsActive)
        {
            Debug.LogWarning("Cannot change vehicle during gameplay!");
            return;
        }
        
        int nextIndex = (selectedVehicleIndex + 1) % availableVehicles.Count;
        SpawnVehicle(nextIndex);
    }
    
    /// <summary>
    /// Wählt das vorherige Fahrzeug in der Liste (nur wenn Spiel nicht läuft)
    /// </summary>
    public void SelectPreviousVehicle()
    {
        if (lockSelectionDuringGame && gameIsActive)
        {
            Debug.LogWarning("Cannot change vehicle during gameplay!");
            return;
        }
        
        int prevIndex = (selectedVehicleIndex - 1 + availableVehicles.Count) % availableVehicles.Count;
        SpawnVehicle(prevIndex);
    }
    
    /// <summary>
    /// Wählt ein spezifisches Fahrzeug (nur wenn Spiel nicht läuft)
    /// </summary>
    public void SelectVehicle(int index)
    {
        if (lockSelectionDuringGame && gameIsActive)
        {
            Debug.LogWarning("Cannot change vehicle during gameplay!");
            return;
        }
        
        SpawnVehicle(index);
    }
    
    /// <summary>
    /// Setzt die Spawn-Position auf die aktuelle Fahrzeug-Position
    /// </summary>
    public void UpdateSpawnPoint()
    {
        if (currentVehicle != null && spawnPoint != null)
        {
            spawnPoint.position = currentVehicle.transform.position;
            spawnPoint.rotation = currentVehicle.transform.rotation;
        }
    }
    
    // GETTER METHODEN
    
    public List<VehicleData> GetAvailableVehicles() => availableVehicles;
    public VehicleData GetCurrentVehicleData() => selectedVehicleIndex < availableVehicles.Count ? availableVehicles[selectedVehicleIndex] : null;
    public GameObject GetCurrentVehicle() => currentVehicle;
    public MovementController GetCurrentMovementController() => currentMovementController;
    public int GetSelectedVehicleIndex() => selectedVehicleIndex;
    public int GetVehicleCount() => availableVehicles.Count;
    
    // GAME STATE MANAGEMENT METHODS
    
    /// <summary>
    /// Startet das Spiel und sperrt die Fahrzeugauswahl
    /// </summary>
    public void StartGame()
    {
        gameIsActive = true;
        Debug.Log("Game started - Vehicle selection locked");
    }
    
    /// <summary>
    /// Stoppt das Spiel und gibt die Fahrzeugauswahl wieder frei
    /// </summary>
    public void StopGame()
    {
        gameIsActive = false;
        Debug.Log("Game stopped - Vehicle selection unlocked");
    }
    
    /// <summary>
    /// Prüft ob das Spiel gerade läuft
    /// </summary>
    public bool IsGameActive() => gameIsActive;
    
    /// <summary>
    /// Prüft ob Fahrzeugauswahl aktuell verfügbar ist
    /// </summary>
    public bool IsVehicleSelectionAvailable()
    {
        return !lockSelectionDuringGame || !gameIsActive;
    }
    
    /// <summary>
    /// Fügt ein neues Fahrzeug zur Liste hinzu
    /// </summary>
    public void AddVehicle(VehicleData vehicleData)
    {
        if (vehicleData != null && vehicleData.vehiclePrefab != null)
        {
            availableVehicles.Add(vehicleData);
            Debug.Log($"Added vehicle: {vehicleData.vehicleName}");
        }
    }
    
    /// <summary>
    /// Entfernt ein Fahrzeug aus der Liste
    /// </summary>
    public void RemoveVehicle(int index)
    {
        if (index >= 0 && index < availableVehicles.Count)
        {
            string vehicleName = availableVehicles[index].vehicleName;
            availableVehicles.RemoveAt(index);
            
            // Falls das aktuell ausgewählte Fahrzeug entfernt wurde
            if (selectedVehicleIndex == index)
            {
                selectedVehicleIndex = Mathf.Clamp(selectedVehicleIndex, 0, availableVehicles.Count - 1);
                if (availableVehicles.Count > 0)
                {
                    SpawnVehicle(selectedVehicleIndex);
                }
            }
            else if (selectedVehicleIndex > index)
            {
                selectedVehicleIndex--;
            }
            
            Debug.Log($"Removed vehicle: {vehicleName}");
        }
    }
}