using UnityEngine;

public class LapTimeController : MonoBehaviour
{
    public static LapTimeController Instance { get; private set; }

    [SerializeField] private LapTime[] lastLapTimes;
    [SerializeField] private LapTime currentLapTime;
    [SerializeField] private LapTime fastesLapTime;

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
        lastLapTimes = new LapTime[3];
        for (int i = 0; i < lastLapTimes.Length; i++)
        {
            lastLapTimes[i] = new LapTime();
        }

        CheckpointController.Instance.OnLapStart += OnLapStartHandler;
        CheckpointController.Instance.OnCheckpointReached += OnCheckpointReachedHandler;
        CheckpointController.Instance.OnLapEnd += OnLapEndHandler;
    }

    private void OnDisable()
    {
        CheckpointController.Instance.OnLapStart -= OnLapStartHandler;
        CheckpointController.Instance.OnCheckpointReached -= OnCheckpointReachedHandler;
        CheckpointController.Instance.OnLapStart -= OnLapStartHandler;
    }

    private void OnLapStartHandler()
    {
        currentLapTime = new LapTime();
        Debug.Log("LapTimeController.OnLapStartHandler()");
    }

    private void OnCheckpointReachedHandler(int checkpointId)
    {
        switch(checkpointId)
        {
            case 1:
                currentLapTime.SplitTimeA = LapTimer.Instance.GetLastSplitTimeMs();
                break;
            case 2:
                currentLapTime.SplitTimeB = LapTimer.Instance.GetLastSplitTimeMs();
                break;
            case 3:
                currentLapTime.SplitTimeC = LapTimer.Instance.GetLastSplitTimeMs();
                break;
            case 4:
                currentLapTime.SplitTimeD = LapTimer.Instance.GetLastSplitTimeMs();
                break;
            case 0:
                currentLapTime.SplitTimeE = LapTimer.Instance.GetLastSplitTimeMs();
                currentLapTime.TotalTimeMs = LapTimer.Instance.GetLastLapTimeMs();
                break;
            default:
                break;
        }

        Debug.Log("LapTimeController.OnCheckpointReachedHandler()");
    }

    private void OnLapEndHandler()
    {
        Debug.Log("LapTimeController.OnLapEndHandler()");
        lastLapTimes[0] = new LapTime(lastLapTimes[1]);
        lastLapTimes[1] = new LapTime(lastLapTimes[2]);
        lastLapTimes[2] = new LapTime(currentLapTime);

        if (fastesLapTime == null || fastesLapTime.TotalTimeMs == 0 || currentLapTime.TotalTimeMs < fastesLapTime.TotalTimeMs)
        {
            fastesLapTime = new LapTime(currentLapTime);
        }

        currentLapTime = new LapTime();
    }
}