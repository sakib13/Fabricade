using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;

/// <summary>
/// Handles all UI: text display, choice buttons, scrolling,
/// and the start/end screens.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NarrativeManager narrativeManager;
    [SerializeField] private AtmosphericController atmosphericController;

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;

    [Header("Choices")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Start Screen")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Button startButtonA;
    [SerializeField] private Button startButtonB;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI endText;

    [Header("Text Settings")]
    [SerializeField] private float textFadeSpeed = 30f;
    [SerializeField] private float scrollDelay = 0.1f;

    private List<Button> activeChoiceButtons = new List<Button>();
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        // Subscribe to narrative events
        narrativeManager.OnNarrativeText += DisplayText;
        narrativeManager.OnChoicesPresented += DisplayChoices;
        narrativeManager.OnStoryEnd += HandleStoryEnd;

        // Setup continue button
        continueButton.onClick.AddListener(OnContinueClicked);
        continueButton.gameObject.SetActive(false);

        // Setup start buttons
        startButtonA.onClick.AddListener(() => StartGame("A"));
        startButtonB.onClick.AddListener(() => StartGame("B"));

        // Hide game UI until start
        narrativeText.text = "";
        endScreen.SetActive(false);
        startScreen.SetActive(true);
        choiceContainer.gameObject.SetActive(false);
    }

    private void StartGame(string condition)
    {
        startScreen.SetActive(false);
        narrativeManager.SetCondition(condition);

        if (atmosphericController != null)
            atmosphericController.SetCondition(condition);

        narrativeManager.StartStory();
    }

    private void DisplayText(string text)
    {
        // Hide choices and continue button while text is appearing
        choiceContainer.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        // Append new paragraph with spacing
        if (!string.IsNullOrEmpty(narrativeText.text))
            narrativeText.text += "\n\n";

        int startIndex = narrativeText.text.Length;
        narrativeText.text += text;

        // Force mesh update and scroll to bottom
        narrativeText.ForceMeshUpdate();
        yield return null;
        ScrollToBottom();

        isTyping = false;

        // Check if there are choices waiting or more text
        if (narrativeManager.CurrentStory.currentChoices.Count > 0)
        {
            DisplayChoices(narrativeManager.CurrentStory.currentChoices);
        }
        else if (narrativeManager.CurrentStory.canContinue)
        {
            continueButton.gameObject.SetActive(true);
        }
        else
        {
            // Story ended
            HandleStoryEnd();
        }
    }

    private void DisplayChoices(List<Choice> choices)
    {
        // Clear old buttons
        ClearChoices();

        choiceContainer.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);

        foreach (Choice choice in choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = choice.text;

            int index = choice.index;
            button.onClick.AddListener(() => OnChoiceSelected(index));

            activeChoiceButtons.Add(button);
        }

        StartCoroutine(ScrollToBottomDelayed());
    }

    private void OnChoiceSelected(int index)
    {
        // Add the selected choice text to the narrative display
        if (index < narrativeManager.CurrentStory.currentChoices.Count)
        {
            string choiceText = narrativeManager.CurrentStory.currentChoices[index].text;
            narrativeText.text += "\n\n<color=#8AACB8>> " + choiceText + "</color>";
        }

        ClearChoices();
        choiceContainer.gameObject.SetActive(false);

        narrativeManager.MakeChoice(index);
    }

    private void OnContinueClicked()
    {
        if (isTyping) return;

        continueButton.gameObject.SetActive(false);
        narrativeManager.ContinueStory();
    }

    private void ClearChoices()
    {
        foreach (Button button in activeChoiceButtons)
        {
            Destroy(button.gameObject);
        }
        activeChoiceButtons.Clear();
    }

    private void HandleStoryEnd()
    {
        continueButton.gameObject.SetActive(false);
        choiceContainer.gameObject.SetActive(false);

        endScreen.SetActive(true);
        endText.text = "Session Complete";
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private IEnumerator ScrollToBottomDelayed()
    {
        yield return new WaitForEndOfFrame();
        ScrollToBottom();
    }

    private void OnDestroy()
    {
        if (narrativeManager != null)
        {
            narrativeManager.OnNarrativeText -= DisplayText;
            narrativeManager.OnChoicesPresented -= DisplayChoices;
            narrativeManager.OnStoryEnd -= HandleStoryEnd;
        }
    }
}
