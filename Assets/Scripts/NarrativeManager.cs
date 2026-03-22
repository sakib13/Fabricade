using UnityEngine;
using Ink.Runtime;

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
    public System.Action<string> OnNarrativeText;          // Fired when new text line is ready
    public System.Action<System.Collections.Generic.List<Choice>> OnChoicesPresented; // Fired when choices appear
    public System.Action<string> OnTagReceived;            // Fired for each tag on a line
    public System.Action<string, string> OnChoiceMade;     // Fired when player picks a choice (variableName, value)
    public System.Action OnStoryEnd;                       // Fired when story reaches END

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

    /// <summary>
    /// Call this to set the condition before starting.
    /// Must be called before StartStory().
    /// </summary>
    public void SetCondition(string cond)
    {
        condition = cond;
        if (story != null)
            story.variablesState["condition"] = condition;
    }

    /// <summary>
    /// Begins the story. Called by UIManager when the game starts.
    /// </summary>
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
    /// Advances the story and sends the next chunk of text.
    /// Continues until choices appear or the story ends.
    /// </summary>
    public void ContinueStory()
    {
        if (story == null) return;

        if (story.canContinue)
        {
            string text = story.Continue().Trim();

            // Process tags on this line
            foreach (string tag in story.currentTags)
            {
                OnTagReceived?.Invoke(tag);
            }

            // Skip empty lines
            if (string.IsNullOrEmpty(text))
            {
                // If there's more content, keep going
                if (story.canContinue)
                    ContinueStory();
                else if (story.currentChoices.Count > 0)
                    OnChoicesPresented?.Invoke(story.currentChoices);
                else
                    OnStoryEnd?.Invoke();
                return;
            }

            OnNarrativeText?.Invoke(text);
        }
        else if (story.currentChoices.Count > 0)
        {
            OnChoicesPresented?.Invoke(story.currentChoices);
        }
        else
        {
            OnStoryEnd?.Invoke();
        }
    }

    /// <summary>
    /// Called when the player selects a choice.
    /// </summary>
    public void MakeChoice(int choiceIndex)
    {
        if (story == null || choiceIndex < 0 || choiceIndex >= story.currentChoices.Count)
            return;

        Choice selected = story.currentChoices[choiceIndex];
        story.ChooseChoiceIndex(choiceIndex);

        // Log which choice variable was just set
        OnChoiceMade?.Invoke(selected.text, choiceIndex.ToString());

        ContinueStory();
    }

    /// <summary>
    /// Get current value of an Ink variable.
    /// </summary>
    public object GetVariable(string varName)
    {
        if (story == null) return null;
        return story.variablesState[varName];
    }

    /// <summary>
    /// Get the current scene name from Ink.
    /// </summary>
    public string GetCurrentScene()
    {
        object val = GetVariable("current_scene");
        return val?.ToString() ?? "";
    }
}
