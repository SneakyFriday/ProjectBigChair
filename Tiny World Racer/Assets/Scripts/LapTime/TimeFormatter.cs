using System;
using UnityEngine;

public class TimeFormatter : MonoBehaviour
{
    /// <summary>
    /// Konvertiert Millisekunden in das Racing-Zeit Format MM:SS.mmm
    /// </summary>
    /// <param name="timeMs">Zeit in Millisekunden</param>
    /// <returns>Formatierte Zeit als String</returns>
    public static string FormatRaceTime(int timeMs)
    {
        if (timeMs <= 0)
            return "--:--.---";

        TimeSpan time = TimeSpan.FromMilliseconds(timeMs);
        
        int minutes = (int)time.TotalMinutes;
        int seconds = time.Seconds;
        int milliseconds = time.Milliseconds;
        
        return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
    }
    
    /// <summary>
    /// Konvertiert Millisekunden in das kurze Format SS.mmm (für Split-Zeiten)
    /// </summary>
    /// <param name="timeMs">Zeit in Millisekunden</param>
    /// <returns>Formatierte Zeit als String</returns>
    public static string FormatSplitTime(int timeMs)
    {
        if (timeMs <= 0)
            return "--.---";

        TimeSpan time = TimeSpan.FromMilliseconds(timeMs);
        
        int totalSeconds = (int)time.TotalSeconds;
        int milliseconds = time.Milliseconds;
        
        return $"{totalSeconds:D2}.{milliseconds:D3}";
    }
    
    /// <summary>
    /// Formatiert Zeit mit Farbe für bessere/schlechtere Zeiten
    /// </summary>
    /// <param name="currentTimeMs">Aktuelle Zeit</param>
    /// <param name="bestTimeMs">Beste Zeit zum Vergleich</param>
    /// <returns>Formatierte Zeit mit Rich Text Color Tags</returns>
    public static string FormatTimeWithColor(int currentTimeMs, int bestTimeMs)
    {
        string formattedTime = FormatRaceTime(currentTimeMs);
        
        if (bestTimeMs <= 0 || currentTimeMs <= 0)
            return formattedTime;
            
        if (currentTimeMs < bestTimeMs)
            return $"<color=green>{formattedTime}</color>";
        else if (currentTimeMs > bestTimeMs * 1.05f)
            return $"<color=red>{formattedTime}</color>";
        else
            return $"<color=yellow>{formattedTime}</color>";
    }
}
