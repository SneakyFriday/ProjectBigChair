using System;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public static LapTimer Instance { get; private set; }

    [SerializeField] private bool timerActive = false;
    [SerializeField] private int elapsedTimeMs = 0;
    [SerializeField] private int lastLapTimeMs = 0;
    [SerializeField] private int splitTime = 0;
    [SerializeField] private int lastSplitTime = 0;

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

    private void Start()
    {
        CheckpointController.Instance.OnLapStart += StartTimer;
        CheckpointController.Instance.OnCheckpointReached += CheckpointReached;
        CheckpointController.Instance.OnLapEnd += RestartTimer;
    }

    private void Update()
    {
        if (timerActive)
        {
            elapsedTimeMs += (int)(Time.deltaTime * 1000);
            splitTime += (int)(Time.deltaTime * 1000);
        }
    }

    private void OnDisable()
    {
        CheckpointController.Instance.OnLapStart -= StartTimer;
        CheckpointController.Instance.OnCheckpointReached -= CheckpointReached;
        CheckpointController.Instance.OnLapEnd -= RestartTimer;
    }

    private void StartTimer()
    {
        elapsedTimeMs = 0;
        splitTime = 0;
        timerActive = true;
        Debug.Log("LapTimer.StartTimer()");
    }

    private void RestartTimer()
    {
        lastLapTimeMs = elapsedTimeMs;
        elapsedTimeMs = 0;
        Debug.Log("Lap Timer restart. Verstrichene Zeit: " + lastLapTimeMs + " ms");
        Debug.Log("LapTimer.RestartTimer()");
    }

    private void CheckpointReached(int checkpointId)
    {
        lastSplitTime = splitTime;
        splitTime = 0;
        Debug.Log("LapTimer.CheckpointReached()");
    }

    public int GetElapsedTimeMs()
    {
        return elapsedTimeMs;
    }

    public int GetLastLapTimeMs()
    {
        return lastLapTimeMs;
    }

    public int GetLastSplitTimeMs()
    {
        return lastSplitTime;
    }
}