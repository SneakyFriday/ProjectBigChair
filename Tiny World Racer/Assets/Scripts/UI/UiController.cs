using UnityEngine;
using TMPro;

public class UiController : MonoBehaviour
{
    [Header("Haupt-Zeit-Anzeige")]
    [SerializeField] private TMP_Text currentTotalLapTimeText;
    
    [Header("Sektor-Anzeigen (A-E)")]
    [SerializeField] private TMP_Text currentSplitATimeText;
    [SerializeField] private TMP_Text currentSplitBTimeText;
    [SerializeField] private TMP_Text currentSplitCTimeText;
    [SerializeField] private TMP_Text currentSplitDTimeText;
    [SerializeField] private TMP_Text currentSplitETimeText;

    [Header("=== LAP TIMES PANEL (rechts oben) ===")]
    [SerializeField] private TMP_Text bestTotalLapTimeText;
    [SerializeField] private TMP_Text lastTotalLapTimeText;
    [SerializeField] private TMP_Text l1TotalLapTimeText;
    [SerializeField] private TMP_Text l2TotalLapTimeText;

    void Start()
    {
        CheckpointController.Instance.OnLapStart += OnLapStartHandler;
        CheckpointController.Instance.OnCheckpointReached += OnCheckpointReachedHandler;
        CheckpointController.Instance.OnLapEnd += OnLapEndHandler;
        
        ResetCurrentLapDisplay();
        UpdateLapTimesDisplay();
    }

    void Update()
    {
        int currentTime = LapTimer.Instance.GetElapsedTimeMs();
        int bestTime = LapTimeController.Instance.GetFastestLapTime()?.TotalTimeMs ?? 0;
        
        if (bestTime > 0 && currentTime > bestTime)
        {
            currentTotalLapTimeText.text = $"<color=#ff6b6b>{TimeFormatter.FormatRaceTime(currentTime)}</color>";
        }
        else if (bestTime > 0 && currentTime < bestTime)
        {
            currentTotalLapTimeText.text = $"<color=#00ff88>{TimeFormatter.FormatRaceTime(currentTime)}</color>";
        }
        else
        {
            currentTotalLapTimeText.text = $"<color=#00ff88>{TimeFormatter.FormatRaceTime(currentTime)}</color>";
        }
    }

    private void OnDisable()
    {
        if (CheckpointController.Instance)
        {
            CheckpointController.Instance.OnLapStart -= OnLapStartHandler;
            CheckpointController.Instance.OnCheckpointReached -= OnCheckpointReachedHandler;
            CheckpointController.Instance.OnLapEnd -= OnLapEndHandler;
        }
    }

    private void OnLapStartHandler()
    {
        Debug.Log("UiController.OnLapStartHandler()");
        ResetCurrentLapDisplay();
    }

    private void OnCheckpointReachedHandler(int checkPointId)
    {
        Debug.Log($"UiController.OnCheckpointReachedHandler() - Checkpoint {checkPointId}");
        
        int splitTime = LapTimer.Instance.GetLastSplitTimeMs();
        string formattedSplitTime = TimeFormatter.FormatSplitTime(splitTime);
        
        string coloredSplitTime = $"<color=#00ff88>{formattedSplitTime}</color>";
        
        switch (checkPointId)
        {
            case 0:
                currentSplitETimeText.text = $"E: {coloredSplitTime}";
                break;
            case 1:
                currentSplitATimeText.text = $"A: {coloredSplitTime}";
                break;
            case 2:
                currentSplitBTimeText.text = $"B: {coloredSplitTime}";
                break;
            case 3:  
                currentSplitCTimeText.text = $"C: {coloredSplitTime}";
                break;
            case 4:
                currentSplitDTimeText.text = $"D: {coloredSplitTime}";
                break;
        }
    }

    private void OnLapEndHandler()
    {
        Debug.Log("UiController.OnLapEndHandler()");
        UpdateLapTimesDisplay();
        
        Invoke(nameof(ResetCurrentLapDisplay), 0.5f);
    }

    private void UpdateLapTimesDisplay()
    {
        // Beste Zeit
        LapTime fastestLap = LapTimeController.Instance.GetFastestLapTime();
        if (fastestLap != null && fastestLap.TotalTimeMs > 0)
        {
            bestTotalLapTimeText.text = $"<color=#ffd700>{TimeFormatter.FormatRaceTime(fastestLap.TotalTimeMs)}</color>";
        }
        else
        {
            bestTotalLapTimeText.text = "<color=#ffd700>--:--.---</color>";
        }

        // Letzte Zeit (neueste)
        LapTime lastLap = LapTimeController.Instance.GetLapTimeByIndex(2);
        if (lastLap != null && lastLap.TotalTimeMs > 0)
        {
            lastTotalLapTimeText.text = $"<color=#ff6b6b>{TimeFormatter.FormatRaceTime(lastLap.TotalTimeMs)}</color>";
        }
        else
        {
            lastTotalLapTimeText.text = "<color=#ff6b6b>--:--.---</color>";
        }

        // L-1 Zeit
        LapTime l1Lap = LapTimeController.Instance.GetLapTimeByIndex(1);
        if (l1Lap != null && l1Lap.TotalTimeMs > 0)
        {
            l1TotalLapTimeText.text = $"<color=#cccccc>{TimeFormatter.FormatRaceTime(l1Lap.TotalTimeMs)}</color>";
        }
        else
        {
            l1TotalLapTimeText.text = "<color=#cccccc>--:--.---</color>";
        }

        // L-2 Zeit
        LapTime l2Lap = LapTimeController.Instance.GetLapTimeByIndex(0);
        if (l2Lap != null && l2Lap.TotalTimeMs > 0)
        {
            l2TotalLapTimeText.text = $"<color=#cccccc>{TimeFormatter.FormatRaceTime(l2Lap.TotalTimeMs)}</color>";
        }
        else
        {
            l2TotalLapTimeText.text = "<color=#cccccc>--:--.---</color>";
        }
    }

    private void ResetCurrentLapDisplay()
    {
        currentSplitATimeText.text = "A: <color=#ffffff66>---.---</color>";
        currentSplitBTimeText.text = "B: <color=#ffffff66>---.---</color>";
        currentSplitCTimeText.text = "C: <color=#ffffff66>---.---</color>";
        currentSplitDTimeText.text = "D: <color=#ffffff66>---.---</color>";
        currentSplitETimeText.text = "E: <color=#ffffff66>---.---</color>";
    }
}