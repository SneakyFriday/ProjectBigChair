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

    [SerializeField] private TMP_Text fastestTotalLapTimeText;
    [SerializeField] private TMP_Text fastestSplitATimeText;
    [SerializeField] private TMP_Text fastestSplitBTimeText;
    [SerializeField] private TMP_Text fastestSplitCTimeText;
    [SerializeField] private TMP_Text fastestSplitDTimeText;
    [SerializeField] private TMP_Text fastestSplitETimeText;

    [SerializeField] private TMP_Text last1TotalLapTimeText;
    [SerializeField] private TMP_Text last1fastestSplitATimeText;
    [SerializeField] private TMP_Text last1fastestSplitBTimeText;
    [SerializeField] private TMP_Text last1fastestSplitCTimeText;
    [SerializeField] private TMP_Text last1fastestSplitDTimeText;
    [SerializeField] private TMP_Text last1fastestSplitETimeText;

    [SerializeField] private TMP_Text last2TotalLapTimeText;
    [SerializeField] private TMP_Text last2fastestSplitATimeText;
    [SerializeField] private TMP_Text last2fastestSplitBTimeText;
    [SerializeField] private TMP_Text last2fastestSplitCTimeText;
    [SerializeField] private TMP_Text last2fastestSplitDTimeText;
    [SerializeField] private TMP_Text last2fastestSplitETimeText;

    [SerializeField] private TMP_Text last3TotalLapTimeText;
    [SerializeField] private TMP_Text last3fastestSplitATimeText;
    [SerializeField] private TMP_Text last3fastestSplitBTimeText;
    [SerializeField] private TMP_Text last3fastestSplitCTimeText;
    [SerializeField] private TMP_Text last3fastestSplitDTimeText;
    [SerializeField] private TMP_Text last3fastestSplitETimeText;

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

    private void UpdateFastestLapText()
    {
        LapTime fastestLapTime = LapTimeController.Instance.GetFastestLapTime();

        fastestTotalLapTimeText.text = "Time: " + fastestLapTime.TotalTimeMs.ToString();
        fastestSplitATimeText.text = "Part A: " + fastestLapTime.SplitTimeA.ToString();
        fastestSplitBTimeText.text = "Part B: " + fastestLapTime.SplitTimeB.ToString();
        fastestSplitCTimeText.text = "Part C: " + fastestLapTime.SplitTimeC.ToString();
        fastestSplitDTimeText.text = "Part D: " + fastestLapTime.SplitTimeD.ToString();
        fastestSplitETimeText.text = "Part E: " + fastestLapTime.SplitTimeE.ToString();
    }

    private void UpdateLastLapsText()
    {
        LapTime fastestLapTime = LapTimeController.Instance.GetLapTimeByIndex(0);

        last1TotalLapTimeText.text = fastestLapTime.TotalTimeMs.ToString();
        last1fastestSplitATimeText.text = fastestLapTime.SplitTimeA.ToString();
        last1fastestSplitBTimeText.text = fastestLapTime.SplitTimeB.ToString();
        last1fastestSplitCTimeText.text = fastestLapTime.SplitTimeC.ToString();
        last1fastestSplitDTimeText.text = fastestLapTime.SplitTimeD.ToString();
        last1fastestSplitETimeText.text = fastestLapTime.SplitTimeE.ToString();

        fastestLapTime = LapTimeController.Instance.GetLapTimeByIndex(1);

        last2TotalLapTimeText.text = fastestLapTime.TotalTimeMs.ToString();
        last2fastestSplitATimeText.text = fastestLapTime.SplitTimeA.ToString();
        last2fastestSplitBTimeText.text = fastestLapTime.SplitTimeB.ToString();
        last2fastestSplitCTimeText.text = fastestLapTime.SplitTimeC.ToString();
        last2fastestSplitDTimeText.text = fastestLapTime.SplitTimeD.ToString();
        last2fastestSplitETimeText.text = fastestLapTime.SplitTimeE.ToString();

        fastestLapTime = LapTimeController.Instance.GetLapTimeByIndex(2);

        last3TotalLapTimeText.text = fastestLapTime.TotalTimeMs.ToString();
        last3fastestSplitATimeText.text = fastestLapTime.SplitTimeA.ToString();
        last3fastestSplitBTimeText.text = fastestLapTime.SplitTimeB.ToString();
        last3fastestSplitCTimeText.text = fastestLapTime.SplitTimeC.ToString();
        last3fastestSplitDTimeText.text = fastestLapTime.SplitTimeD.ToString();
        last3fastestSplitETimeText.text = fastestLapTime.SplitTimeE.ToString();
    }

    private void OnLapEndHandler()
    {
        Debug.Log("UiController.OnLapEndHandler()");
        UpdateFastestLapText();
        UpdateLastLapsText();
        currentSplitATimeText.text = "Part A: " + 0;
        currentSplitBTimeText.text = "Part B: " + 0;
        currentSplitCTimeText.text = "Part C: " + 0;
        currentSplitDTimeText.text = "Part D: " + 0;
        currentSplitETimeText.text = "Part E: " + 0;
    }
}
