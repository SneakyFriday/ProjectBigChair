using UnityEngine;
using UnityEngine.Events;

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
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    // Events
    public UnityEvent OnGameStart = new UnityEvent();
    public UnityEvent OnGameStop = new UnityEvent();
    public UnityEvent OnGamePause = new UnityEvent();
    public UnityEvent OnGameResume = new UnityEvent();
    public UnityEvent<GameObject> OnPlayerRespawn = new UnityEvent<GameObject>();
    
    // Properties
    public bool IsGameStarted => gameStarted;
    public bool IsPaused => isPaused;
    public bool CanControl => gameStarted && !isPaused;
    
    private void Awake()
    {
        if (instance != null && instance != this)
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
        
        OnGameStart?.Invoke();
        
        if (showDebugMessages)
            Debug.Log("Game Started!");
    }
    
    public void StopGame()
    {
        gameStarted = false;
        
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
    
    public void RespawnPlayer(GameObject player)
    {
        if (!useCheckpointSystem)
        {
            ResetPlayerToStart(player);
            return;
        }
        
        StartCoroutine(RespawnSequence(player));
    }
    
    private System.Collections.IEnumerator RespawnSequence(GameObject player)
    {
        // Optional: Add fade out effect here
        
        yield return new WaitForSeconds(respawnDelay);
        
        bool respawnedAtCheckpoint = false;
        
        // Try checkpoint respawn
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
        
        // Fallback to start position if no checkpoint available
        if (!respawnedAtCheckpoint)
        {
            ResetPlayerToStart(player);
            
            if (showDebugMessages)
                Debug.Log("No checkpoint available - respawned at start position");
        }
        
        // Reset velocity if needed
        if (resetVelocityOnRespawn)
        {
            MovementController movement = player.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.ResetVelocity();
            }
        }
        
        OnPlayerRespawn?.Invoke(player);
        
        // Optional: Add fade in effect here
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
        MovementController[] allVehicles = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
        foreach (var vehicle in allVehicles)
        {
            vehicle.ResetToStartPosition();
        }
        
        if (CheckpointController.Instance)
        {
            CheckpointController.Instance.ResetCheckpoints();
        }
        
        StartGame();
        
        if (showDebugMessages)
            Debug.Log("Level Restarted!");
    }
    
    // Call this from UI buttons or other systems
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
}