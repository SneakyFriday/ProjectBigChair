using UnityEngine;
public class CockpitCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1.2f, 1.5f);
    public bool rotateWithTarget = true;
    public bool smoothFollow = false;
    public float followSpeed = 10f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        if (rotateWithTarget)
        {
            // Kamera rotiert exakt mit dem Auto
            if (smoothFollow)
            {
                // Smooth Follow
                Vector3 targetPosition = target.position + target.TransformDirection(offset);
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * followSpeed);
            }
            else
            {
                // Direkte Synchronisation (empfohlen für Cockpit)
                transform.position = target.position + target.TransformDirection(offset);
                transform.rotation = target.rotation;
            }
        }
        else
        {
            // Nur Position folgen, nicht rotieren
            Vector3 targetPosition = target.position + offset;
            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.DrawLine(target.position, transform.position, Color.cyan);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Zeige Offset-Position in der Scene-View
            Gizmos.color = Color.cyan;
            Vector3 targetPos = target.position + (rotateWithTarget ? target.TransformDirection(offset) : offset);
            Gizmos.DrawWireSphere(targetPos, 0.2f);
            Gizmos.DrawLine(target.position, targetPos);
            
            // Zeige Blickrichtung
            Gizmos.color = Color.blue;
            Vector3 forward = rotateWithTarget ? target.forward : transform.forward;
            Gizmos.DrawRay(targetPos, forward * 2f);
        }
    }
    
    /// <summary>
    /// Setzt das Target zur Laufzeit
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (showDebugInfo)
            Debug.Log($"CockpitCameraController target set to: {newTarget?.name}");
    }
    
    /// <summary>
    /// Setzt den Offset zur Laufzeit
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        if (showDebugInfo)
            Debug.Log($"CockpitCameraController offset set to: {newOffset}");
    }
    
    /// <summary>
    /// Aktiviert/Deaktiviert Rotation mit Target
    /// </summary>
    public void SetRotateWithTarget(bool rotate)
    {
        rotateWithTarget = rotate;
        if (showDebugInfo)
            Debug.Log($"CockpitCameraController rotate with target: {rotate}");
    }
}