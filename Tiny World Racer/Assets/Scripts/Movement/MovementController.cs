using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
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
    
    private Vector3 moveDirection;
    private Vector3 moveForce;
    private float currentSteerInput = 0f;
    private float currentTraction;
    private bool isHandbrakeActive = false;
    
    // Input
    private InputAction moveAction;
    private InputAction handbrakeAction;
    private Vector3 inputVector;
    private bool handbrakePressed;
    
    void Start()
    {
        InitializeInput();
        currentTraction = traction;
    }
    
    void Update()
    {
        ReadInput();
        
        if(isHandbrakeActiveByDefault)
            HandleHandbrake(handbrakePressed);
        
        ApplyForce();
        HandleSteering();
        ApplyMovement();
        VisualizeDebug();
        ApplyPhysics();
    }
    
    private void InitializeInput()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        handbrakeAction = InputSystem.actions.FindAction("Jump");
    }
    
    private void ReadInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();
        handbrakePressed = handbrakeAction.ReadValue<float>() > 0.5f;
    }
    
    private void ApplyForce()
    {
        moveForce += transform.forward * (inputVector.y * moveSpeed * Time.deltaTime);
    }
    
    private void HandleSteering()
    {
        currentSteerInput = Mathf.Lerp(currentSteerInput, inputVector.x, steeringSmoothness);
        
        float speed = moveForce.magnitude;
        float speedFactor = CalculateSpeedFactor(speed);
        float steerFactor = isHandbrakeActive ? handbrakeSteerMultiplier : 1f;
        float steerAmount = CalculateSteerAmount(speedFactor, steerFactor);
        
        transform.Rotate(Vector3.up * steerAmount);
    }
    
    private float CalculateSpeedFactor(float speed)
    {
        return Mathf.Lerp(1f, 0.5f, speed / maxSpeed * speedSteerFactor);
    }
    
    private float CalculateSteerAmount(float speedFactor, float steerFactor)
    {
        float steerAmount = currentSteerInput * steerAngle * moveForce.magnitude * 
                           speedFactor * steerFactor * Time.deltaTime;
        
        return Mathf.Clamp(steerAmount, -maxSteerSpeed * Time.deltaTime, maxSteerSpeed * Time.deltaTime);
    }
    
    private void ApplyMovement()
    {
        moveDirection = Vector3.Lerp(moveDirection.normalized, transform.forward, currentTraction * Time.deltaTime) 
                      * moveForce.magnitude;
        
        transform.position += moveDirection * Time.deltaTime;
    }
    
    private void VisualizeDebug()
    {
        Debug.DrawRay(transform.position, moveDirection, Color.red);
        Debug.DrawRay(transform.position, moveForce, Color.green);
    }
    
    private void ApplyPhysics()
    {
        float currentDrag = CalculateDrag();
        
        moveForce *= currentDrag;
        moveForce = Vector3.ClampMagnitude(moveForce, maxSpeed);
    }
    
    private float CalculateDrag()
    {
        float baseDrag = isHandbrakeActive ? handbrakeDrag : drag;
        
        if (isHandbrakeActive) {
            float speedRatio = Mathf.Clamp01(moveForce.magnitude / maxSpeed);
            return Mathf.Lerp(baseDrag, baseDrag * 0.92f, speedRatio);
        }
        
        return baseDrag;
    }
    
    private void HandleHandbrake(bool active)
    {
        isHandbrakeActive = active;
        currentTraction = active ? handbrakeTraction : Mathf.Lerp(currentTraction, traction, Time.deltaTime * handbrakeRecoverySpeed);
    }
    
    public void SetHandbrake(bool active)
    {
        HandleHandbrake(active);
    }
}