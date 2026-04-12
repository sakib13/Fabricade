using UnityEngine;
using System.Collections.Generic;
using Ink.Runtime;
using IOPath = System.IO.Path;
using Directory = System.IO.Directory;
using File = System.IO.File;

/// <summary>
/// Logs all player behavior to a timestamped JSON file.
/// Records: choices, timing, condition, scene transitions.
/// Output is used for stimulated recall interviews and research data.
/// </summary>
public class BehavioralLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NarrativeManager narrativeManager;

    [Header("Settings")]
    [SerializeField] private string outputFolder = "SessionLogs";

    private SessionLog sessionLog;
    private float choicePresentedTime;
    private string currentScene = "";
    private string sessionFilePath;

    [System.Serializable]
    private class SessionLog
    {
        public string sessionId;
        public string condition;
        public string startTime;
        public string endTime;
        public float totalDurationSeconds;
        public List<LogEntry> entries = new List<LogEntry>();
    }

    [System.Serializable]
    private class LogEntry
    {
        public string timestamp;
        public float sessionTimeSeconds;
        public string eventType;    // "scene_enter", "choice_presented", "choice_made", "story_end"
        public string scene;
        public string details;
        public float hesitationSeconds;  // Time between choice presented and selection
    }

    private float sessionStartTime;

    private void Start()
    {
        if (narrativeManager == null)
        {
            Debug.LogError("BehavioralLogger: NarrativeManager not assigned.");
            return;
        }

        // Subscribe to events
        narrativeManager.OnTagReceived += OnTag;
        narrativeManager.OnChoicesPresented += OnChoicesPresented;
        narrativeManager.OnChoiceMade += OnChoiceMade;
        narrativeManager.OnStoryEnd += OnStoryEnd;

        // Initialize session
        sessionLog = new SessionLog
        {
            sessionId = System.Guid.NewGuid().ToString().Substring(0, 8),
            startTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            entries = new List<LogEntry>()
        };

        sessionStartTime = Time.time;

        // Create output directory
        string fullPath = IOPath.Combine(Application.dataPath, "..", outputFolder);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        sessionFilePath = IOPath.Combine(fullPath,
            $"session_{sessionLog.sessionId}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    private void OnTag(string tag)
    {
        if (tag.StartsWith("scene:"))
        {
            string scene = tag.Substring(6);
            currentScene = scene;

            // Log condition on first scene
            if (sessionLog.condition == null || sessionLog.condition == "")
            {
                object cond = narrativeManager.GetVariable("condition");
                sessionLog.condition = cond?.ToString() ?? "unknown";
            }

            AddEntry("scene_enter", scene, $"Entered scene: {scene}");
        }
    }

    private void OnChoicesPresented(List<Choice> choices)
    {
        choicePresentedTime = Time.time;

        string choiceTexts = "";
        for (int i = 0; i < choices.Count; i++)
        {
            if (i > 0) choiceTexts += " | ";
            choiceTexts += $"[{i}] {choices[i].text}";
        }

        AddEntry("choice_presented", currentScene, choiceTexts);
    }

    private void OnChoiceMade(string choiceText, string choiceIndex)
    {
        float hesitation = Time.time - choicePresentedTime;

        LogEntry entry = new LogEntry
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            sessionTimeSeconds = Time.time - sessionStartTime,
            eventType = "choice_made",
            scene = currentScene,
            details = $"Selected [{choiceIndex}]: {choiceText}",
            hesitationSeconds = hesitation
        };

        sessionLog.entries.Add(entry);
        SaveLog();
    }

    private void OnStoryEnd()
    {
        sessionLog.endTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        sessionLog.totalDurationSeconds = Time.time - sessionStartTime;

        // Log final variable states
        string finalState = $"miss_most={narrativeManager.GetVariable("miss_most")}, " +
                           $"openness={narrativeManager.GetVariable("openness")}, " +
                           $"deflection={narrativeManager.GetVariable("deflection")}, " +
                           $"resistance={narrativeManager.GetVariable("resistance")}, " +
                           $"emotional_posture={narrativeManager.GetVariable("emotional_posture")}, " +
                           $"trust_in_system={narrativeManager.GetVariable("trust_in_system")}, " +
                           $"mystery_awareness={narrativeManager.GetVariable("mystery_awareness")}, " +
                           $"choice_0={narrativeManager.GetVariable("choice_0_intake")}, " +
                           $"choice_1={narrativeManager.GetVariable("choice_1_dinner")}, " +
                           $"choice_2={narrativeManager.GetVariable("choice_2_bench")}, " +
                           $"choice_3={narrativeManager.GetVariable("choice_3_corridor")}, " +
                           $"choice_4={narrativeManager.GetVariable("choice_4_room")}, " +
                           $"choice_5={narrativeManager.GetVariable("choice_5_discharge")}";

        AddEntry("story_end", currentScene, finalState);

        Debug.Log($"BehavioralLogger: Session saved to {sessionFilePath}");
    }

    private void AddEntry(string eventType, string scene, string details)
    {
        LogEntry entry = new LogEntry
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            sessionTimeSeconds = Time.time - sessionStartTime,
            eventType = eventType,
            scene = scene,
            details = details,
            hesitationSeconds = 0f
        };

        sessionLog.entries.Add(entry);
        SaveLog();
    }

    private void SaveLog()
    {
        string json = JsonUtility.ToJson(sessionLog, true);
        File.WriteAllText(sessionFilePath, json);
    }

    private void OnDestroy()
    {
        if (narrativeManager != null)
        {
            narrativeManager.OnTagReceived -= OnTag;
            narrativeManager.OnChoicesPresented -= OnChoicesPresented;
            narrativeManager.OnChoiceMade -= OnChoiceMade;
            narrativeManager.OnStoryEnd -= OnStoryEnd;
        }
    }
}
