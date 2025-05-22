using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public static CheckpointController Instance { get; private set; }

    [SerializeField] private Checkpoint[] m_checkpoints;

    private int m_nextCheckpointIndex = 0;
    private bool lapStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckCheckpoint(Checkpoint checkpoint)
    {
        if (!lapStarted)
        {
            if (checkpoint.CheckpointId == m_checkpoints[0].CheckpointId)
            {
                lapStarted = true;
                m_nextCheckpointIndex = 1;
                LapTimeController.Instance.StartLap();
                Debug.Log("Runde gestartet! Fahre nun alle Checkpoints in der richtigen Reihenfolge ab.");
            }
            else
            {
                Debug.Log("Runde noch nicht gestartet – fahre zuerst über die Startlinie!");
            }
            return;
        }

        if (checkpoint.CheckpointId == m_checkpoints[m_nextCheckpointIndex].CheckpointId)
        {
            Debug.Log("Richtiger Checkpoint erreicht: " + checkpoint.CheckpointId);
            LapTimeController.Instance.RecordCheckpoint(checkpoint.CheckpointId);
            m_nextCheckpointIndex = (m_nextCheckpointIndex + 1) % m_checkpoints.Length;

            if (m_nextCheckpointIndex == 0)
            {
                Debug.Log("Runde beendet!");
                m_nextCheckpointIndex = 1;
            }
            else
            {
                Debug.Log("Weiter zum nächsten Checkpoint (" + m_checkpoints[m_nextCheckpointIndex].CheckpointId + ").");
            }
        }
        else
        {
            Debug.Log("Falscher Checkpoint! Erwartet wird Checkpoint " + m_checkpoints[m_nextCheckpointIndex].CheckpointId + ".");
        }
    }
}