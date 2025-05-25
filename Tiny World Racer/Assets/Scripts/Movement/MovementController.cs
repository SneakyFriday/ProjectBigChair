using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float drag = 0.98f;
    [SerializeField] float traction = 0.5f;
    
    [Header("Steering")]
    [SerializeField] float steerAngle = 10f;
    [SerializeField] float steeringSmoothness = 0.1f;
    [SerializeField] float speedSteerFactor = 0.7f;
    [SerializeField] float maxSteerSpeed = 100f;
    
    [Header("Handbrake")]
    [SerializeField] bool isHandbrakeActiveByDefault = false;
    [SerializeField] float handbrakeTraction = 0.05f;
    [SerializeField] float handbrakeDrag = 0.93f;
    [SerializeField] float handbrakeSteerMultiplier = 1.5f;
    [SerializeField] float handbrakeRecoverySpeed = 2f;
    
    [Header("Ground Detection")]
    [SerializeField] bool showGroundDebug = true;
    [SerializeField] string groundTag = "Ground";
    [SerializeField] float airControlDelay = 0.2f;
    [SerializeField] float airDrag = 0.995f;
    
    [Header("Vehicle Reset")]
    [SerializeField] float resetHeight = 1f;
    [SerializeField] bool resetClearsVelocity = true;
    [SerializeField] bool useCheckpointRespawn = true;
    
    [Header("Game Control")]
    [SerializeField] bool startControlsLocked = true;
    
    private Vector3 moveDirection;
    private float currentSpeed = 0f;
    private float currentSteerInput = 0f;
    private float currentTraction;
    private bool isHandbrakeActive = false;
    private bool isGrounded = false;
    private bool canControl = true;
    private bool gameStarted = false;
    private bool justStartedInput = false;
    private int groundContactCount = 0;
    private float timeSinceGrounded = 0f;
    private Quaternion initialRotation;
    
    private InputAction moveAction;
    private InputAction handbrakeAction;
    private InputAction interactAction;
    private Vector3 inputVector;
    private Vector3 lastInputVector;
    private bool handbrakePressed;
    private bool interactPressed;
    
    public bool IsGameStarted => gameStarted;
    public bool IsGrounded => isGrounded;
    public bool CanControl => canControl;
    public float TimeSinceGrounded => timeSinceGrounded;
    
    
    void Start()
    {
        InitializeInput();
        currentTraction = traction;
        initialRotation = transform.rotation;
        SetupGroundDetection();
        
        if (startControlsLocked)
        {
            gameStarted = false;
            if (showGroundDebug)
                Debug.Log("Controls are locked until game is started!");
        }
        else
        {
            gameStarted = true;
        }
    }
    
    void Update()
    {
        ReadInput();
        UpdateGroundStatus();
        
        if (interactPressed)
        {
            ResetVehicle();
        }
        
        if(isHandbrakeActiveByDefault)
            HandleHandbrake(handbrakePressed);
        
        if (canControl && gameStarted)
        {
            ApplyForce();
            HandleSteering();
        }
        
        ApplyMovement();
        VisualizeDebug();
        ApplyPhysics();
    }
    
    private void SetupGroundDetection()
    {
        Transform groundDetector = transform.Find("GroundDetector");
        if (groundDetector == null)
        {
            GameObject detectorObj = new GameObject("GroundDetector");
            detectorObj.transform.SetParent(transform);
            detectorObj.transform.localPosition = Vector3.down * 0.5f;
            
            SphereCollider detector = detectorObj.AddComponent<SphereCollider>();
            detector.isTrigger = true;
            detector.radius = 0.6f;
            
            GroundDetector detectorScript = detectorObj.AddComponent<GroundDetector>();
            detectorScript.SetParent(this);
            
            if (showGroundDebug)
                Debug.Log("Ground Detector automatisch erstellt!");
        }
    }
    
    private void InitializeInput()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        handbrakeAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");
    }
    
    private void ReadInput()
    {
        lastInputVector = inputVector;
        inputVector = moveAction.ReadValue<Vector2>();
        handbrakePressed = handbrakeAction.ReadValue<float>() > 0.5f;
        interactPressed = interactAction != null && interactAction.WasPressedThisFrame();
    }
    
    private void UpdateGroundStatus()
    {
        if (isGrounded)
        {
            timeSinceGrounded = 0f;
            canControl = true;
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
            canControl = timeSinceGrounded < airControlDelay;
        }
        
        if (showGroundDebug && !canControl && timeSinceGrounded > airControlDelay)
        {
            Debug.Log($"Air control lost after {airControlDelay}s - maintaining momentum only");
        }
    }
    
    private void ApplyForce()
    {
        bool startedForward = inputVector.y > 0.5f && lastInputVector.y <= 0.5f;
        bool startedBackward = inputVector.y < -0.5f && lastInputVector.y >= -0.5f;
        
        if (startedForward && currentSpeed < 0 || startedBackward && currentSpeed > 0)
        {
            currentSpeed = 0f;
        }

        if (startedForward)
        {
            currentSpeed = 3f;
        }
        else if (startedBackward)
        {
            currentSpeed = -3f;
        }
        
        currentSpeed += inputVector.y * moveSpeed * Time.deltaTime;
    }
    
    private void HandleSteering()
    {
        currentSteerInput = Mathf.Lerp(currentSteerInput, inputVector.x, steeringSmoothness);
        
        float speed = Mathf.Abs(currentSpeed);
        float speedFactor = CalculateSpeedFactor(speed);
        float steerFactor = isHandbrakeActive ? handbrakeSteerMultiplier : 1f;
        float steerAmount = CalculateSteerAmount(speed, speedFactor, steerFactor);
        
        transform.Rotate(Vector3.up * steerAmount);
    }
    
    private float CalculateSpeedFactor(float speed)
    {
        return Mathf.Lerp(1f, 0.5f, speed / maxSpeed * speedSteerFactor);
    }
    
    private float CalculateSteerAmount(float speed, float speedFactor, float steerFactor)
    {
        float steerAmount = currentSteerInput * steerAngle * speed * 
                           speedFactor * steerFactor * Time.deltaTime;
        
        return Mathf.Clamp(steerAmount, -maxSteerSpeed * Time.deltaTime, maxSteerSpeed * Time.deltaTime);
    }
    
    private void ApplyMovement()
    {
        Vector3 movement = transform.forward * (currentSpeed * Time.deltaTime);
        transform.position += movement;
        
        moveDirection = movement / Time.deltaTime;
    }
    
    private void VisualizeDebug()
    {
        Debug.DrawRay(transform.position, moveDirection, Color.red);
        
        Vector3 speedVector = (currentSpeed >= 0 ? transform.forward : -transform.forward) * Mathf.Abs(currentSpeed);
        Debug.DrawRay(transform.position, speedVector, Color.green);
        
        if (showGroundDebug)
        {
            Color statusColor;
            if (isGrounded) 
                statusColor = Color.green;
            else if (canControl) 
                statusColor = Color.yellow;
            else 
                statusColor = Color.red;
                
            Debug.DrawRay(transform.position + Vector3.down * 0.5f, Vector3.forward * 0.5f, statusColor);
            Debug.DrawRay(transform.position + Vector3.down * 0.5f, Vector3.right * 0.5f, statusColor);
            Debug.DrawRay(transform.position + Vector3.down * 0.5f, Vector3.back * 0.5f, statusColor);
            Debug.DrawRay(transform.position + Vector3.down * 0.5f, Vector3.left * 0.5f, statusColor);
        }
    }
    
    private void ApplyPhysics()
    {
        if (justStartedInput)
        {
            justStartedInput = false;
            return;
        }
        
        float currentDrag = CalculateDrag();
        
        currentSpeed *= currentDrag;
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        
        if (Mathf.Abs(currentSpeed) < 0.01f)
        {
            currentSpeed = 0f;
        }
    }
    
    private float CalculateDrag()
    {
        if (!isGrounded)
        {
            return airDrag;
        }
        
        float baseDrag = isHandbrakeActive ? handbrakeDrag : drag;
        
        if (isHandbrakeActive) 
        {
            float speedRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            return Mathf.Lerp(baseDrag, baseDrag * 0.92f, speedRatio);
        }
        
        return baseDrag;
    }
    
    private void HandleHandbrake(bool active)
    {
        isHandbrakeActive = active;
        currentTraction = active ? handbrakeTraction : Mathf.Lerp(currentTraction, traction, Time.deltaTime * handbrakeRecoverySpeed);
    }
    
    private void ResetVehicle()
    {
        RespawnAtLastCheckpoint();
        
        if (resetClearsVelocity)
        {
            currentSpeed = 0f;
            moveDirection = Vector3.zero;
            currentSteerInput = 0f;
        }
        
        timeSinceGrounded = 0f;
        canControl = true;
        
        if (showGroundDebug)
            Debug.Log("Vehicle reset to upright position!");
    }
    
    private void RespawnVehicle()
    {
        if (useCheckpointRespawn && CheckpointController.Instance)
        {
            try
            {
                Checkpoint lastCheckpoint = CheckpointController.Instance.GetLastCheckpoint();
                if (lastCheckpoint)
                {
                    lastCheckpoint.RespawnAtCheckpoint(gameObject);
                    
                    if (resetClearsVelocity)
                    {
                        currentSpeed = 0f;
                        moveDirection = Vector3.zero;
                        currentSteerInput = 0f;
                    }
                    
                    timeSinceGrounded = 0f;
                    canControl = true;
                    
                    if (showGroundDebug)
                        Debug.Log($"Vehicle respawned at checkpoint {lastCheckpoint.CheckpointId}!");
                    
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Checkpoint respawn failed: {e.Message}. Using standard reset.");
            }
        }
        
        ResetVehicle();
        
        if (showGroundDebug && useCheckpointRespawn)
            Debug.Log("No checkpoint available - used standard reset instead.");
    }
    
    public void RespawnAtLastCheckpoint()
    {
        RespawnVehicle();
    }
    
    public void OnGroundEnter(Collider other)
    {
        if (IsValidGround(other))
        {
            groundContactCount++;
            isGrounded = groundContactCount > 0;
            
            if (showGroundDebug)
                Debug.Log($"Ground entered: {other.name} (Total contacts: {groundContactCount})");
        }
    }
    
    public void OnGroundExit(Collider other)
    {
        if (IsValidGround(other))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
            isGrounded = groundContactCount > 0;
            
            if (showGroundDebug)
                Debug.Log($"Ground left: {other.name} (Total contacts: {groundContactCount})");
        }
    }
    
    private bool IsValidGround(Collider other)
    {
        bool hasGroundTag = string.IsNullOrEmpty(groundTag) || other.CompareTag(groundTag);
        bool hasGroundLayer = other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                             other.gameObject.layer == 0;
        
        return hasGroundTag || hasGroundLayer;
    }
    
    // Public Methods
    public void ResetVehiclePosition()
    {
        ResetVehicle();
    }
    
    public void SetHandbrake(bool active)
    {
        HandleHandbrake(active);
    }
    
    public void StartGame()
    {
        gameStarted = true;
        ResetVehicleToStart();
        if (showGroundDebug)
            Debug.Log("Game started - Controls unlocked!");
    }
    
    public void ResetVehicleToStart()
    {
        transform.position = transform.position;
        transform.rotation = initialRotation;
        
        currentSpeed = 0f;
        moveDirection = Vector3.zero;
        currentSteerInput = 0f;
        
        isHandbrakeActive = false;
        currentTraction = traction;
        
        timeSinceGrounded = 0f;
        canControl = true;
        justStartedInput = false;
        
        inputVector = Vector3.zero;
        lastInputVector = Vector3.zero;
        
        if (showGroundDebug)
            Debug.Log("Vehicle reset to starting state!");
    }
    
    public void StopGame()
    {
        gameStarted = false;
        if (showGroundDebug)
            Debug.Log("Game stopped - Controls locked!");
    }
}

public class GroundDetector : MonoBehaviour
{
    private MovementController parentController;
    
    public void SetParent(MovementController controller)
    {
        parentController = controller;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (parentController)
            parentController.OnGroundEnter(other);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (parentController)
            parentController.OnGroundExit(other);
    }
}