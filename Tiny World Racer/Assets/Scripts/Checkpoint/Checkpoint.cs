using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int m_CheckpointId = 0;
    public int CheckpointId { get => m_CheckpointId; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointController.Instance.CheckCheckpoint(this);
        }
    }
}