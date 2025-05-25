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
    [SerializeField] VehicleSelectionUI vehicleSelectionUI;
    
    [Header("Main Menu Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button vehicleSelectButton;
    
    [Header("Settings")]
    [SerializeField] Slider volumeSlider;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Button backButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; // Falls wir in der Pro Version mit Scenes arbeiten sollten :P
    
    private InputAction escapeAction;
    private bool isGameActive = false;
    
    void Start()
    {
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(BackToMainMenu);
        vehicleSelectButton.onClick.AddListener(OpenVehicleSelection);
        
        LoadSettings();
       
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        
        var btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
        btnTxt.text = "Start";
        
        InitializeEscapeInput();
        ShowMainMenu();
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        vehicleSelectButton.onClick.RemoveAllListeners();
        volumeSlider.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        qualityDropdown.onValueChanged.RemoveAllListeners();
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
    
    void OpenVehicleSelection()
    {
        vehicleSelectionUI.OpenVehicleSelection();
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
        
        if (GameManager.Instance)
        {
            GameManager.Instance.StartGame();
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
        
        if (GameManager.Instance.IsGameStarted)
        {
            GameManager.Instance.StopGame();
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
        if (GameManager.Instance)
        {
            GameManager.Instance.StopGame();
        }
        playButton.onClick.RemoveAllListeners();
        var btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
        btnTxt.text = "Restart";
        playButton.onClick.AddListener(GameManager.Instance.RestartLevel);
        ShowMainMenu();
    }
    
    public bool IsGameRunning()
    {
        return GameManager.Instance.IsGameStarted;
    }
}