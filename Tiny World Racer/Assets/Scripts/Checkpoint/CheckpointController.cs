using System;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public static CheckpointController Instance { get; private set; }

    public event Action OnLapStart;
    public event Action OnLapEnd;
    public event Action<int> OnCheckpointReached;

    [Header("Checkpoints in Reihenfolge (Index 0 = Start-/Ziel)")]
    [SerializeField] private Checkpoint[] checkpoints;

    [SerializeField] private int lastCheckpointReached = 0;
    [SerializeField] private int nextCheckpointIndex = 0;
    [SerializeField] private bool lapStarted = false;

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
        lastCheckpointReached = checkpoint.CheckpointId;

        if (!lapStarted)
        {
            if (checkpoint.CheckpointId == 0)
            {
                lapStarted = true;
                nextCheckpointIndex = 1;

                OnLapStart?.Invoke();

                Debug.Log("Runde gestartet!");
            }
            return;
        }

        if (checkpoint.CheckpointId == checkpoints[nextCheckpointIndex].CheckpointId)
        {
            OnCheckpointReached?.Invoke(checkpoint.CheckpointId);

            if (checkpoint.CheckpointId == 0)
            {
                OnLapEnd?.Invoke();
                nextCheckpointIndex = 1;

                Debug.Log("Runde abgeschlossen!");
            }
            else
            {
                nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Length;
            }
        }
        else
        {
            Debug.Log("Falscher Checkpoint!");
        }
    }

    public Checkpoint GetLastCheckpoint()
    {
        return checkpoints[lastCheckpointReached];
    }
}