using System;
using UnityEngine;

[Serializable]
public class LapTime
{
    [SerializeField] private int totalTimeMs;
    [SerializeField] private int splitTimeA;
    [SerializeField] private int splitTimeB;
    [SerializeField] private int splitTimeC;
    [SerializeField] private int splitTimeD;
    [SerializeField] private int splitTimeE;

    public LapTime()
    {
        totalTimeMs = 0;
        splitTimeA = 0;
        splitTimeB = 0;
        splitTimeC = 0;
        splitTimeD = 0;
        splitTimeE = 0;
    }

    public LapTime(LapTime source)
    {
        TotalTimeMs = source.TotalTimeMs;
        SplitTimeA = source.SplitTimeA;
        SplitTimeB = source.SplitTimeB;
        SplitTimeC = source.SplitTimeC;
        SplitTimeD = source.SplitTimeD;
        SplitTimeE = source.SplitTimeE;
    }

    public int TotalTimeMs
    { 
        get => totalTimeMs; 
        set => totalTimeMs = value;
    }

    public int SplitTimeA
    {
        get => splitTimeA;
        set => splitTimeA = value;
    }

    public int SplitTimeB
    {
        get => splitTimeB;
        set => splitTimeB = value;
    }

    public int SplitTimeC
    {
        get => splitTimeC;
        set => splitTimeC = value;
    }

    public int SplitTimeD
    {
        get => splitTimeD;
        set => splitTimeD = value;
    }

    public int SplitTimeE
    {
        get => splitTimeE;
        set => splitTimeE = value;
    }

    public void CalculateTotalTime()
    {
        TotalTimeMs = SplitTimeA + SplitTimeB + SplitTimeC + SplitTimeD + SplitTimeE;
    }
}