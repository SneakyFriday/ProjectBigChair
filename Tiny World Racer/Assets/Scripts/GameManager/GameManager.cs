using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (!instance)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Game State")]
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool startWithLockedControls = true;
    [SerializeField] private bool isPaused = false;
    
    [Header("Respawn Settings")]
    [SerializeField] private bool useCheckpointSystem = true;
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private bool resetVelocityOnRespawn = true;
    
    [Header("Fall Detection")]
    [SerializeField] private bool enableFallDetection = true;
    [SerializeField] private float fallThreshold = -10f;
    [SerializeField] private bool restartOnFall = true;
    [SerializeField] private float fallCheckInterval = 0.2f;
    [SerializeField] private float restartDelay = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    // Events
    public UnityEvent OnGameStart = new UnityEvent();
    public UnityEvent OnGameStop = new UnityEvent();
    public UnityEvent OnGamePause = new UnityEvent();
    public UnityEvent OnGameResume = new UnityEvent();
    public UnityEvent<GameObject> OnPlayerRespawn = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> OnPlayerFell = new UnityEvent<GameObject>();
    
    // Properties
    public bool IsGameStarted => gameStarted;
    public bool IsPaused => isPaused;
    public bool CanControl => gameStarted && !isPaused;
    
    // Fall Detection
    private Coroutine fallDetectionCoroutine;
    private bool isRestarting = false;
    
    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        if (startWithLockedControls)
        {
            gameStarted = false;
            if (showDebugMessages)
                Debug.Log("Game initialized with locked controls. Call StartGame() to begin.");
        }
        else
        {
            StartGame();
        }
    }
    
    public void StartGame()
    {
        gameStarted = true;
        isPaused = false;
        isRestarting = false;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
        
        if (enableFallDetection)
        {
            StartFallDetection();
        }
        
        if (showDebugMessages)
            Debug.Log("Game Started!");
    }
    
    public void StopGame()
    {
        gameStarted = false;
        Time.timeScale = 1f;
        
        StopFallDetection();
        
        OnGameStop?.Invoke();
        if (showDebugMessages)
            Debug.Log("Game Stopped!");
    }
    
    public void PauseGame()
    {
        if (!gameStarted) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        OnGamePause?.Invoke();
        
        if (showDebugMessages)
            Debug.Log("Game Paused!");
    }
    
    public void ResumeGame()
    {
        if (!gameStarted) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        OnGameResume?.Invoke();
        
        if (showDebugMessages)
            Debug.Log("Game Resumed!");
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    #region Fall Detection System
    
    private void StartFallDetection()
    {
        StopFallDetection(); // Sicherstellen dass keine doppelte Coroutine läuft
        fallDetectionCoroutine = StartCoroutine(FallDetectionLoop());
        
        if (showDebugMessages)
            Debug.Log($"Fall Detection started - Threshold: {fallThreshold}");
    }
    
    private void StopFallDetection()
    {
        if (fallDetectionCoroutine != null)
        {
            StopCoroutine(fallDetectionCoroutine);
            fallDetectionCoroutine = null;
        }
    }
    
    private IEnumerator FallDetectionLoop()
    {
        while (gameStarted && enableFallDetection)
        {
            if (!isPaused && !isRestarting)
            {
                CheckForFallenVehicles();
            }
            
            yield return new WaitForSeconds(fallCheckInterval);
        }
    }
    
    private void CheckForFallenVehicles()
    {
        MovementController[] vehicles = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
        
        foreach (var vehicle in vehicles)
        {
            if (vehicle != null && vehicle.transform.position.y < fallThreshold)
            {
                OnVehicleFell(vehicle.gameObject);
                break;
            }
        }
    }
    
    private void OnVehicleFell(GameObject vehicle)
    {
        if (isRestarting) return;
        
        OnPlayerFell?.Invoke(vehicle);
        
        if (showDebugMessages)
            Debug.Log($"Vehicle {vehicle.name} fell below threshold ({fallThreshold})!");
        
        if (restartOnFall)
        {
            StartCoroutine(RestartAfterFall(vehicle));
        }
        else
        {
            RespawnPlayer(vehicle);
        }
    }
    
    private IEnumerator RestartAfterFall(GameObject vehicle)
    {
        isRestarting = true;
        
        if (showDebugMessages)
            Debug.Log($"Restarting level in {restartDelay} seconds...");
        
        yield return new WaitForSeconds(restartDelay);
        
        RestartLevel();
    }
    
    #endregion
    
    public void RespawnPlayer(GameObject player)
    {
        if (!useCheckpointSystem)
        {
            ResetPlayerToStart(player);
            return;
        }
        
        StartCoroutine(RespawnSequence(player));
    }
    
    private IEnumerator RespawnSequence(GameObject player)
    {
       yield return new WaitForSeconds(respawnDelay);
        
        bool respawnedAtCheckpoint = false;
        
        if (CheckpointController.Instance != null)
        {
            Checkpoint lastCheckpoint = CheckpointController.Instance.GetLastCheckpoint();
            if (lastCheckpoint != null)
            {
                lastCheckpoint.RespawnAtCheckpoint(player);
                respawnedAtCheckpoint = true;
                
                if (showDebugMessages)
                    Debug.Log($"Player respawned at checkpoint {lastCheckpoint.CheckpointId}");
            }
        }
        
        if (!respawnedAtCheckpoint)
        {
            ResetPlayerToStart(player);
            
            if (showDebugMessages)
                Debug.Log("No checkpoint available - respawned at start position");
        }
        
        if (resetVelocityOnRespawn)
        {
            MovementController movement = player.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.ResetVelocity();
            }
        }
        
        OnPlayerRespawn?.Invoke(player);
    }
    
    private void ResetPlayerToStart(GameObject player)
    {
        MovementController movement = player.GetComponent<MovementController>();
        if (movement != null)
        {
            movement.ResetToStartPosition();
        }
    }
    
    public void RestartLevel()
    {
        bool wasPaused = isPaused;
        if (wasPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
        
        MovementController[] allVehicles = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
        foreach (var vehicle in allVehicles)
        {
            if (vehicle != null)
            {
                vehicle.ResetToStartPosition();
                
                if (resetVelocityOnRespawn)
                {
                    vehicle.ResetVelocity();
                }
            }
        }
        
        if (CheckpointController.Instance)
        {
            CheckpointController.Instance.ResetCheckpoints();
        }
        
        if (LapTimer.Instance)
        {
            
        }
        
        StartGame();
        
        if (showDebugMessages)
            Debug.Log("Level Restarted!");
    }
    
    // Public Methods für UI/Debug
    public void OnStartButtonPressed()
    {
        StartGame();
    }
    
    public void OnRestartButtonPressed()
    {
        RestartLevel();
    }
    
    public void OnPauseButtonPressed()
    {
        TogglePause();
    }
    
    // Debug/Testing Methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestFallDetection()
    {
        MovementController[] vehicles = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
        if (vehicles.Length > 0)
        {
            Vector3 fallPosition = vehicles[0].transform.position;
            fallPosition.y = fallThreshold - 1f;
            vehicles[0].transform.position = fallPosition;
            
            Debug.Log("Simulated vehicle fall for testing!");
        }
    }
    
    // Getter für Fall-Detection Settings
    public float GetFallThreshold() => fallThreshold;
    public bool IsFallDetectionEnabled() => enableFallDetection;
    
    // Setter um Fall-Detection zur Laufzeit zu ändern
    public void SetFallThreshold(float newThreshold)
    {
        fallThreshold = newThreshold;
        if (showDebugMessages)
            Debug.Log($"Fall threshold updated to: {fallThreshold}");
    }
    
    public void SetFallDetectionEnabled(bool enabled)
    {
        enableFallDetection = enabled;
        
        if (gameStarted)
        {
            if (enabled)
                StartFallDetection();
            else
                StopFallDetection();
        }
        
        if (showDebugMessages)
            Debug.Log($"Fall detection {(enabled ? "enabled" : "disabled")}");
    }
}