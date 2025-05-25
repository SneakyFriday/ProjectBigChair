using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject timeOverviewPanel;
    
    [Header("Main Menu Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitButton;
    
    [Header("Settings")]
    [SerializeField] Slider volumeSlider;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Button backButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; // Falls wir in der Pro Version mit Scenes arbeiten sollten :P
    
    [Header("Game Control")]
    [SerializeField] MovementController movementController;
    [SerializeField] bool autoFindMovementController = true;
    
    private InputAction escapeAction;
    private bool isGameActive = false;
    
    void Start()
    {
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(BackToMainMenu);
        
        LoadSettings();
       
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        
        InitializeEscapeInput();
        
        if (autoFindMovementController && !movementController)
        {
            movementController = FindFirstObjectByType<MovementController>();
            if (!movementController)
            {
                Debug.LogWarning("MovementController konnte nicht gefunden werden! Bitte manuell zuweisen.");
            }
            else
            {
                Debug.Log("MovementController automatisch gefunden und zugewiesen.");
            }
        }
        
        ShowMainMenu();
    }

    private void Update()
    {
        if (escapeAction != null && escapeAction.WasPressedThisFrame())
        {
            HandleEscapePressed();
        }
    }
    
    private void InitializeEscapeInput()
    {
        escapeAction = InputSystem.actions.FindAction("Cancel");
        if (escapeAction == null)
        {
            escapeAction = new InputAction("Escape", binding: "<Keyboard>/escape");
            escapeAction.Enable();
        }
    }
    
    private void HandleEscapePressed()
    {
        if (isGameActive)
        {
            PauseGame();
            Debug.Log("Game paused with Escape key");
        }
        else if (settingsPanel.activeInHierarchy)
        {
            BackToMainMenu();
        }
    }

    void StartGame()
    {
        mainMenuPanel.SetActive(false);
        timeOverviewPanel.SetActive(true);
        isGameActive = true;
        
        if (movementController)
        {
            movementController.StartGame();
            Debug.Log("Game started - Movement controls unlocked!");
        }
        else
        {
            Debug.LogError("MovementController ist nicht zugewiesen! Controls können nicht freigegeben werden.");
        }
    }
    
    void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    void BackToMainMenu()
    {
        SaveSettings();
        ShowMainMenu();
        isGameActive = false;
        
        if (movementController && movementController.IsGameStarted)
        {
            movementController.StopGame();
            Debug.Log("Returned to main menu - Movement controls locked!");
        }
    }
    
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        timeOverviewPanel.SetActive(false);
        isGameActive = false;
    }
    
    void QuitGame()
    {
        Debug.Log("Quitting Game...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
    
    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }
    
    void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel);
    }
    
    void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    // TODO: Dinge, die vllt mal in einem GameManger müssen irgendwann
    public void StartGameProgrammatically()
    {
        StartGame();
    }
    
    public void PauseGame()
    {
        if (movementController)
        {
            movementController.StopGame();
        }
        ShowMainMenu();
    }
    
    public bool IsGameRunning()
    {
        return movementController && movementController.IsGameStarted;
    }
    
    private void OnDestroy()
    {
        if (escapeAction == null) return;
        
        escapeAction.Disable();
        escapeAction.Dispose();
    }
}