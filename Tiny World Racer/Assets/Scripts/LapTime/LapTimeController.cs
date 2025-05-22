using System.Collections.Generic;
using UnityEngine;

public class LapTimeController : MonoBehaviour
{
    public static LapTimeController Instance { get; private set; }

    [Header("LapTime Einstellungen")]
    [SerializeField] private int lapHistoryLimit = 3;

    public LapTime currentLap;
    public LapTime BestLap { get; private set; }

    private List<LapTime> lapHistory = new List<LapTime>();
    private int lastCheckpointTimeMs;

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

    public void StartLap()
    {
        LapTimer.Instance.StartTimer();
        currentLap = new LapTime();
        lastCheckpointTimeMs = 0;

        Debug.Log("Neuer Lap gestartet.");
    }

    public void RecordCheckpoint(int checkpointId)
    {
        int currentTime = LapTimer.Instance.GetElapsedTimeMs();

        int splitTime = currentTime - lastCheckpointTimeMs;
        lastCheckpointTimeMs = currentTime;

        switch (checkpointId)
        {
            case 1:
                currentLap.SplitTimeA = splitTime;
                Debug.Log("SplitTimeA (Start -> CP1): " + splitTime + " ms");
                break;
            case 2:
                currentLap.SplitTimeB = splitTime;
                Debug.Log("SplitTimeB (CP1 -> CP2): " + splitTime + " ms");
                break;
            case 3:
                currentLap.SplitTimeC = splitTime;
                Debug.Log("SplitTimeC (CP2 -> CP3): " + splitTime + " ms");
                break;
            case 4:
                currentLap.SplitTimeD = splitTime;
                Debug.Log("SplitTimeD (CP3 -> CP4): " + splitTime + " ms");
                break;
            case 0:
                currentLap.SplitTimeF = splitTime;
                Debug.Log("SplitTimeF (CP4 -> Start): " + splitTime + " ms");
                CompleteLap();
                break;
            default:
                Debug.LogWarning("Ungültige Checkpoint-ID: " + checkpointId);
                break;
        }
    }

    public void CompleteLap()
    {
        currentLap.TotalTimeMs = LapTimer.Instance.GetElapsedTimeMs();

        lapHistory.Add(currentLap);
        if (lapHistory.Count > lapHistoryLimit)
        {
            lapHistory.RemoveAt(0);
        }

        if (BestLap == null || currentLap.TotalTimeMs < BestLap.TotalTimeMs)
        {
            BestLap = currentLap;
        }

        Debug.Log("Lap abgeschlossen: " + currentLap.TotalTimeMs + " ms");

        StartLap();
    }

    public List<LapTime> GetLapHistory()
    {
        return lapHistory;
    }
}