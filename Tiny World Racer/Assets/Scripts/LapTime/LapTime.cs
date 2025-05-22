using System;

[Serializable]
public class LapTime
{
    public int TotalTimeMs { get; set; }

    public int SplitTimeA { get; set; }
    public int SplitTimeB { get; set; }
    public int SplitTimeC { get; set; }
    public int SplitTimeD { get; set; }
    public int SplitTimeF { get; set; }
    public void CalculateTotalTime()
    {
        TotalTimeMs = SplitTimeA + SplitTimeB + SplitTimeC + SplitTimeD + SplitTimeF;
    }
}