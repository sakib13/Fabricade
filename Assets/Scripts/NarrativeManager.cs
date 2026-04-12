using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;

/// <summary>
/// Core bridge between Ink narrative and Unity.
/// Loads the Ink story, advances text, presents choices,
/// and broadcasts tags to other systems.
/// </summary>
public class NarrativeManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJSON;

    [Header("Condition")]
    [Tooltip("Set to A for narrative-dialogue adaptation, B for atmospheric-aesthetic adaptation")]
    [SerializeField] private string condition = "A";

    private Story story;

    // Events that other scripts listen to
    public System.Action<string> OnNarrativeText;
    public System.Action<List<Choice>> OnChoicesPresented;
    public System.Action<string> OnTagReceived;
    public System.Action<string, string> OnChoiceMade;
    public System.Action OnStoryEnd;

    public Story CurrentStory => story;
    public string CurrentCondition => condition;

    private void Awake()
    {
        if (inkJSON == null)
        {
            Debug.LogError("NarrativeManager: No Ink JSON file assigned.");
            return;
        }

        story = new Story(inkJSON.text);
        story.variablesState["condition"] = condition;
    }

    public void SetCondition(string cond)
    {
        condition = cond;
        if (story != null)
            story.variablesState["condition"] = condition;
    }

    public void StartStory()
    {
        if (story == null)
        {
            Debug.LogError("NarrativeManager: Story not initialized.");
            return;
        }
        ContinueStory();
    }

    /// <summary>
    /// Gathers all text lines until the next choice point or story end.
    /// Sends them as one combined block.
    /// </summary>
    public void ContinueStory()
    {
        if (story == null) return;

        string combinedText = "";

        while (story.canContinue)
        {
            string line = story.Continue().Trim();

            // Process tags
            foreach (string tag in story.currentTags)
            {
                OnTagReceived?.Invoke(tag);
            }

            if (!string.IsNullOrEmpty(line))
            {
                if (!string.IsNullOrEmpty(combinedText))
                    combinedText += "\n\n";
                combinedText += line;
            }

            // If choices appeared after this line, stop gathering
            if (story.currentChoices.Count > 0)
                break;
        }

        // Send the combined text block
        Debug.Log($"NarrativeManager: combinedText length={combinedText.Length}, canContinue={story.canContinue}, choices={story.currentChoices.Count}");
        if (!string.IsNullOrEmpty(combinedText))
        {
            Debug.Log($"NarrativeManager: Sending text: {combinedText.Substring(0, Mathf.Min(100, combinedText.Length))}...");
            OnNarrativeText?.Invoke(combinedText);
        }
        else
        {
            Debug.LogWarning("NarrativeManager: No text to display after continue!");
        }

        // Now handle what comes next
        if (story.currentChoices.Count > 0)
        {
            OnChoicesPresented?.Invoke(story.currentChoices);
        }
        else if (!story.canContinue)
        {
            OnStoryEnd?.Invoke();
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        Debug.Log($"MakeChoice called: index={choiceIndex}, story null={story == null}, choices count={story?.currentChoices?.Count}");
        if (story == null || choiceIndex < 0 || choiceIndex >= story.currentChoices.Count)
        {
            Debug.LogWarning($"MakeChoice: REJECTED. story null={story == null}, index={choiceIndex}, count={story?.currentChoices?.Count}");
            return;
        }

        Choice selected = story.currentChoices[choiceIndex];
        story.ChooseChoiceIndex(choiceIndex);
        OnChoiceMade?.Invoke(selected.text, choiceIndex.ToString());

        ContinueStory();
    }

    public object GetVariable(string varName)
    {
        if (story == null) return null;
        return story.variablesState[varName];
    }

    public string GetCurrentScene()
    {
        object val = GetVariable("current_scene");
        return val?.ToString() ?? "";
    }
}
