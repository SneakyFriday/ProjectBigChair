using UnityEngine;
using TMPro;
using System;

public class UiController : MonoBehaviour
{
    [SerializeField] private TMP_Text currentTotalLapTimeText;
    [SerializeField] private TMP_Text currentSplitATimeText;
    [SerializeField] private TMP_Text currentSplitBTimeText;
    [SerializeField] private TMP_Text currentSplitCTimeText;
    [SerializeField] private TMP_Text currentSplitDTimeText;
    [SerializeField] private TMP_Text currentSplitETimeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckpointController.Instance.OnLapStart += OnLapStartHandler;
        CheckpointController.Instance.OnCheckpointReached += OnCheckpointReachedHandler;
        CheckpointController.Instance.OnLapEnd += OnLapEndHandler;
    }

    // Update is called once per frame
    void Update()
    {
        currentTotalLapTimeText.text = "Time: " + LapTimer.Instance.GetElapsedTimeMs().ToString();
    }

    private void OnDisable()
    {
        CheckpointController.Instance.OnLapStart -= OnLapStartHandler;
        CheckpointController.Instance.OnCheckpointReached -= OnCheckpointReachedHandler;
        CheckpointController.Instance.OnLapStart -= OnLapStartHandler;
    }

    private void OnLapStartHandler()
    {
        Debug.Log("UiController.OnLapStartHandler()");
    }

    private void OnCheckpointReachedHandler(int checkPointId)
    {
        Debug.Log("UiController.OnCheckpointReachedHandler()");
        switch (checkPointId)
        {
            case 0:
                currentSplitETimeText.text = "Part E: " + LapTimer.Instance.GetLastSplitTimeMs().ToString();
                break;
            case 1:
                currentSplitATimeText.text = "Part A: " + LapTimer.Instance.GetLastSplitTimeMs().ToString();
                break;
            case 2:
                currentSplitBTimeText.text = "Part B: " + LapTimer.Instance.GetLastSplitTimeMs().ToString();
                break;
            case 3:
                currentSplitCTimeText.text = "Part C: " + LapTimer.Instance.GetLastSplitTimeMs().ToString();
                break;
            case 4:
                currentSplitDTimeText.text = "Part D: " + LapTimer.Instance.GetLastSplitTimeMs().ToString();
                break;
            default:
                break;
        }
    }

    private void OnLapEndHandler()
    {
        Debug.Log("UiController.OnLapEndHandler()");
        currentSplitATimeText.text = "Part A: " + 0;
        currentSplitBTimeText.text = "Part B: " + 0;
        currentSplitCTimeText.text = "Part C: " + 0;
        currentSplitDTimeText.text = "Part D: " + 0;
        currentSplitETimeText.text = "Part E: " + 0;
    }
}
