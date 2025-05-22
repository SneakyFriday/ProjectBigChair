using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public static LapTimer Instance { get; private set; }

    private bool timerActive = false;
    private int elapsedTimeMs = 0;

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

    private void Update()
    {
        if (timerActive)
        {
            elapsedTimeMs += (int)(Time.deltaTime * 1000);
        }
    }

    public void StartTimer()
    {
        elapsedTimeMs = 0;
        timerActive = true;
        Debug.Log("Lap Timer gestartet.");
    }

    public void StopTimer()
    {
        timerActive = false;
        Debug.Log("Lap Timer gestoppt. Verstrichene Zeit: " + elapsedTimeMs + " ms");
    }

    public int GetElapsedTimeMs()
    {
        return elapsedTimeMs;
    }
}