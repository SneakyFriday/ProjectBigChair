using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int m_CheckpointId = 0;
    [SerializeField] private Transform m_RespawnPoint;
    public int CheckpointId { get => m_CheckpointId; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointController.Instance.CheckCheckpoint(this);
        }
    }

    public void RespawnAtCheckpoint(GameObject player)
    {
        if (m_RespawnPoint != null)
        {
            player.transform.position = m_RespawnPoint.position;
            player.transform.rotation = m_RespawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("RespawnPoint nicht gesetzt!");
        }
    }

}