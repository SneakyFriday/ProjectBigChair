using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    [Header("Cameras - Flexibel für alle Cinemachine-Typen")]
    [SerializeField] private GameObject followCameraObject;
    [SerializeField] private GameObject cockpitCameraObject;
    
    [Header("Camera Settings")]
    [SerializeField] private bool smoothTransitions = true;
    [SerializeField] private float transitionTime = 1f;
    
    [Header("Input")]
    [SerializeField] private string cameraInputAction = "SwitchCamera";
    [SerializeField] private KeyCode fallbackKey = KeyCode.C;
    
    [Header("Cockpit Settings")]
    [SerializeField] private Vector3 cockpitOffset = new Vector3(0, 1.2f, 0.3f);
    [SerializeField] private Vector3 cockpitRotation = new Vector3(0, 0, 0);
    [SerializeField] private bool cockpitRotatesWithVehicle = true;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindExistingCamera = true;
    [SerializeField] private VehicleSelector vehicleSelector;
    
    private InputAction cameraAction;
    private int currentCameraIndex = 0;
    private GameObject[] cameraObjects;
    private Transform followTarget;
    
    public enum CameraType
    {
        Follow = 0,
        Cockpit = 1
    }
    
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeCameras();
        SetupInput();
        
        currentCameraIndex = 0;
        SetActiveCamera(CameraType.Follow);
        
        Debug.Log("CameraManager started - Follow Camera is default");
    }
    
    void Update()
    {
        if (cameraAction != null && cameraAction.WasPressedThisFrame())
        {
            SwitchCamera();
        }
    }
    
    void InitializeCameras()
    {
        Debug.Log("=== INITIALIZING CAMERAS ===");
        
        if (autoFindExistingCamera && !followCameraObject)
        {
            FindExistingCinemachineCamera();
        }
        
        FindFollowTarget();
        
        if (!cockpitCameraObject)
        {
            CreateCockpitCamera();
        }
        
        if (!followCameraObject)
        {
            Debug.LogError("Follow Camera Object is null! Please assign manually.");
            return;
        }
        
        if (!cockpitCameraObject)
        {
            Debug.LogError("Cockpit Camera Object is null! Creation failed.");
            return;
        }
        
        cameraObjects = new GameObject[] { followCameraObject, cockpitCameraObject };
        
        Debug.Log($"Follow Camera: {followCameraObject.name}");
        Debug.Log($"Cockpit Camera: {cockpitCameraObject.name}");
        
        var mainCamera = Camera.main;
        if (mainCamera)
        {
            var brain = mainCamera.GetComponent<CinemachineBrain>();
            if (!brain)
            {
                Debug.LogWarning("Main Camera has no CinemachineBrain! Adding one...");
                brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
            }
            Debug.Log($"CinemachineBrain found on: {mainCamera.name}");
        }
        else
        {
            Debug.LogError("No Main Camera found!");
        }
        
        SetupCameras();
        
        Debug.Log("=== CAMERA INITIALIZATION COMPLETE ===");
    }
    
    void FindExistingCinemachineCamera()
    {
        foreach (var cam in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (cam.GetType().Name == "CinemachineCamera")
            {
                followCameraObject = cam.gameObject;
                Debug.Log($"Found new CinemachineCamera: {cam.name}");
                return;
            }
        }
        
        foreach (var cam in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (cam.GetType().Name == "CinemachineVirtualCamera")
            {
                followCameraObject = cam.gameObject;
                Debug.Log($"Found old CinemachineVirtualCamera: {cam.name}");
                return;
            }
        }
        
        foreach (var cam in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (cam.GetType().Name.Contains("Cinemachine") && 
                cam.name.ToLower().Contains("camera"))
            {
                followCameraObject = cam.gameObject;
                Debug.Log($"Found Cinemachine component: {cam.name} ({cam.GetType().Name})");
                return;
            }
        }
        
        Debug.LogWarning("No Cinemachine Camera found. Please assign manually.");
    }
    
    void FindFollowTarget()
    {
        if (!followTarget)
        {
            if (vehicleSelector)
            {
                followTarget = vehicleSelector.GetCurrentVehicle()?.transform;
            }
            
            if (!followTarget)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player)
                {
                    followTarget = player.transform;
                }
            }
        }
    }
    
    void SetupInput()
    {
        if (!string.IsNullOrEmpty(cameraInputAction))
        {
            cameraAction = InputSystem.actions.FindAction(cameraInputAction);
        }
        
        if (cameraAction == null)
        {
            cameraAction = new InputAction("SwitchCamera", binding: $"<Keyboard>/{fallbackKey.ToString().ToLower()}");
            cameraAction.Enable();
            Debug.Log($"Created fallback input action for {fallbackKey} key");
        }
        else
        {
            Debug.Log($"Found existing input action: {cameraInputAction}");
        }
    }
    
    void SetupCameras()
    {
        if (!followTarget) return;
        
        Debug.Log("Setting up cameras...");
        
        if (followCameraObject)
        {
            SetupFollowCamera(followCameraObject, followTarget);
        }
        
        if (cockpitCameraObject)
        {
            SetupCockpitCamera(cockpitCameraObject, false);
        }
        
        Debug.Log("Camera setup complete - Follow camera should be DEFINITELY active");
    }
    
    private void SetupFollowCamera(GameObject cameraObj, Transform target)
    {
        var camera = cameraObj.GetComponent<MonoBehaviour>();
        if (!camera) return;
        
        try
        {
            var cameraType = camera.GetType();
            
            var followProperty = cameraType.GetProperty("Follow");
            var lookAtProperty = cameraType.GetProperty("LookAt");
            var priorityProperty = cameraType.GetProperty("Priority");
            
            if (followProperty != null && followProperty.CanWrite)
            {
                followProperty.SetValue(camera, target);
                Debug.Log($"Set Follow target for {cameraType.Name}");
            }
            
            if (lookAtProperty != null && lookAtProperty.CanWrite)
            {
                lookAtProperty.SetValue(camera, target);
                Debug.Log($"Set LookAt target for {cameraType.Name}");
            }
            
            if (priorityProperty != null && priorityProperty.CanWrite)
            {
                priorityProperty.SetValue(camera, 20);
                Debug.Log($"Set Priority to 20 for {cameraType.Name}");
            }
            
            cameraObj.SetActive(true);
            Debug.Log($"Follow camera {cameraObj.name} ACTIVATED ({cameraType.Name})");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not configure follow camera: {e.Message}");
        }
    }
    
    private void SetupCockpitCamera(GameObject cameraObj, bool activate)
    {
        var camera = cameraObj.GetComponent<MonoBehaviour>();
        if (!camera) return;
        
        try
        {
            var cameraType = camera.GetType();
            var priorityProperty = cameraType.GetProperty("Priority");
            
            if (priorityProperty != null && priorityProperty.CanWrite)
            {
                priorityProperty.SetValue(camera, activate ? 20 : -10);
            }
            
            cameraObj.SetActive(activate);
            
            var cockpitController = cameraObj.GetComponent<CockpitCameraController>();
            if (cockpitController)
            {
                cockpitController.target = followTarget;
                cockpitController.offset = cockpitOffset;
                cockpitController.rotateWithTarget = cockpitRotatesWithVehicle;
                cockpitController.enabled = activate;
            }
            
            Debug.Log($"Cockpit camera {(activate ? "ACTIVATED" : "DEACTIVATED")} ({cameraType.Name})");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not configure cockpit camera: {e.Message}");
        }
    }
    
    void CreateCockpitCamera()
    {
        GameObject cockpitObj = new GameObject("CockpitCamera");
        cockpitObj.transform.SetParent(transform);
        MonoBehaviour cameraComponent = null;
        
        try
        {
            var newCameraType = System.Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
            if (newCameraType != null)
            {
                cameraComponent = cockpitObj.AddComponent(newCameraType) as MonoBehaviour;
                Debug.Log("Created new CinemachineCamera component");
            }
        }
        catch
        {
            try
            {
                var oldCameraType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Unity.Cinemachine");
                if (oldCameraType != null)
                {
                    cameraComponent = cockpitObj.AddComponent(oldCameraType) as MonoBehaviour;
                    Debug.Log("Created old CinemachineVirtualCamera component");
                }
            }
            catch
            {
                Debug.LogError("Could not create any Cinemachine camera component!");
            }
        }
        
        if (cameraComponent)
        {
            try
            {
                var priorityProperty = cameraComponent.GetType().GetProperty("Priority");
                if (priorityProperty != null && priorityProperty.CanWrite)
                {
                    priorityProperty.SetValue(cameraComponent, -10);
                }
            }
            catch { }
        }
        
        var cockpitController = cockpitObj.AddComponent<CockpitCameraController>();
        cockpitController.enabled = false;
        cockpitObj.SetActive(false);
        cockpitCameraObject = cockpitObj;
        Debug.Log("Cockpit Camera automatisch erstellt (komplett deaktiviert)");
    }
    
    public void SwitchCamera()
    {
        currentCameraIndex = (currentCameraIndex + 1) % cameraObjects.Length;
        SetActiveCamera((CameraType)currentCameraIndex);
    }
    
    public void SetActiveCamera(CameraType cameraType)
    {
        currentCameraIndex = (int)cameraType;
        
        Debug.Log($"=== SWITCHING TO {cameraType} CAMERA ===");
        
        if (cameraType == CameraType.Follow)
        {
            if (followCameraObject)
            {
                SetupFollowCamera(followCameraObject, followTarget);
            }
            
            if (cockpitCameraObject)
            {
                SetupCockpitCamera(cockpitCameraObject, false);
            }
            
            Debug.Log("Follow camera ACTIVATED, Cockpit camera DEACTIVATED");
        }
        else if (cameraType == CameraType.Cockpit)
        {
            if (followCameraObject)
            {
                SetCameraPriority(followCameraObject, -10);
            }
            
            if (cockpitCameraObject)
            {
                SetupCockpitCamera(cockpitCameraObject, true);
            }
            
            Debug.Log("Cockpit camera ACTIVATED, Follow camera DEACTIVATED");
        }
        
        var brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain)
        {
            try
            {
                var activeVirtualCamera = brain.ActiveVirtualCamera;
                if (activeVirtualCamera != null)
                {
                    Debug.Log($"CinemachineBrain active camera: {activeVirtualCamera.Name}");
                }
            }
            catch
            {
                Debug.Log("CinemachineBrain status could not be read (different API version)");
            }
        }
    }
    
    private void SetCameraPriority(GameObject cameraObj, int priority)
    {
        if (!cameraObj) return;
        
        var cinemachineCamera = cameraObj.GetComponent<MonoBehaviour>();
        if (cinemachineCamera && cinemachineCamera.GetType().Name.Contains("Cinemachine"))
        {
            try
            {
                var priorityProperty = cinemachineCamera.GetType().GetProperty("Priority");
                if (priorityProperty != null && priorityProperty.CanWrite)
                {
                    priorityProperty.SetValue(cinemachineCamera, priority);
                    Debug.Log($"Set {cinemachineCamera.GetType().Name} {cameraObj.name} priority to {priority}");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set priority: {e.Message}");
            }
        }
        
        Debug.LogWarning($"Could not set priority for {cameraObj.name} - no compatible Cinemachine component found!");
    }
    
    public void SetFollowTarget(Transform newTarget)
    {
        followTarget = newTarget;
        SetupCameras();
        
        if (cockpitCameraObject)
        {
            var cockpitController = cockpitCameraObject.GetComponent<CockpitCameraController>();
            if (cockpitController)
            {
                cockpitController.target = followTarget;
            }
        }
    }
    
    // PUBLIC METHODS
    
    public void SetFollowCamera()
    {
        SetActiveCamera(CameraType.Follow);
    }
    
    public void SetCockpitCamera()
    {
        SetActiveCamera(CameraType.Cockpit);
    }
    
    public CameraType GetCurrentCameraType()
    {
        return (CameraType)currentCameraIndex;
    }
    
    public void SetCockpitOffset(Vector3 newOffset, bool rotateWithVehicle = true)
    {
        cockpitOffset = newOffset;
        cockpitRotatesWithVehicle = rotateWithVehicle;
        
        if (cockpitCameraObject)
        {
            var cockpitController = cockpitCameraObject.GetComponent<CockpitCameraController>();
            if (cockpitController)
            {
                cockpitController.offset = cockpitOffset;
                cockpitController.rotateWithTarget = cockpitRotatesWithVehicle;
            }
        }
    }
    
    /// <summary>
    /// Schnelle Presets für verschiedene Fahrzeugtypen
    /// </summary>
    public void SetCockpitPreset(VehicleType vehicleType)
    {
        switch (vehicleType)
        {
            case VehicleType.SportsCar:
                SetCockpitOffset(new Vector3(0, 0.8f, 0.2f), true);
                break;
            case VehicleType.SUV:
                SetCockpitOffset(new Vector3(0, 1.5f, 0.4f), true);
                break;
            case VehicleType.Truck:
                SetCockpitOffset(new Vector3(0, 2.0f, 0.5f), true);
                break;
            case VehicleType.Standard:
            default:
                SetCockpitOffset(new Vector3(0, 1.2f, 0.3f), true);
                break;
        }
        
        Debug.Log($"Applied cockpit preset for {vehicleType}");
    }
    
    public enum VehicleType
    {
        Standard,
        SportsCar,
        SUV,
        Truck
    }
    
    void OnDestroy()
    {
        if (cameraAction != null)
        {
            cameraAction.Disable();
            cameraAction.Dispose();
        }
    }
}