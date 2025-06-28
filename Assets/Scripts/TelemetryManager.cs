using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    private float sessionStartTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += HandleLog;
            sessionStartTime = Time.realtimeSinceStartup;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void OnApplicationQuit()
    {
        // End session
        float sessionLength = Time.realtimeSinceStartup - sessionStartTime;
        Analytics.CustomEvent("session_end", new Dictionary<string, object> {
            { "duration_sec", Mathf.RoundToInt(sessionLength) }
        });
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            // Send crash report event
            Analytics.CustomEvent("crash_report", new Dictionary<string, object> {
                { "message", condition },
                { "stack", stackTrace }
            });
        }
    }

    // Call at level start
    public void SendLevelStart(int level)
    {
        Analytics.CustomEvent("level_start", new Dictionary<string, object> {
            { "level", level }
        });
    }

    // Call on successful win
    public void SendLevelComplete(int level, int movesLeft, int score)
    {
        Analytics.CustomEvent("level_complete", new Dictionary<string, object> {
            { "level", level },
            { "moves_left", movesLeft },
            { "score", score }
        });
    }

    // Call on failure
    public void SendLevelFail(int level, int socksMatched)
    {
        Analytics.CustomEvent("level_fail", new Dictionary<string, object> {
            { "level", level },
            { "matched_count", socksMatched }
        });
    }

    // Call when a power-up is used
    public void SendPowerupUsed(string powerupName)
    {
        Analytics.CustomEvent("powerup_used", new Dictionary<string, object> {
            { "type", powerupName }
        });
    }
}
