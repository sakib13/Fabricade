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

    private List<Button> activeChoiceButtons = new List<Button>();
    private bool waitingForChoices = false;

    private void Start()
    {
        narrativeManager.OnNarrativeText += DisplayText;
        narrativeManager.OnChoicesPresented += DisplayChoices;
        narrativeManager.OnStoryEnd += HandleStoryEnd;

        continueButton.onClick.AddListener(OnContinueClicked);
        continueButton.gameObject.SetActive(false);

        startButtonA.onClick.AddListener(() => StartGame("A"));
        startButtonB.onClick.AddListener(() => StartGame("B"));

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
        choiceContainer.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        // Append text with paragraph spacing
        if (!string.IsNullOrEmpty(narrativeText.text))
            narrativeText.text += "\n\n";

        narrativeText.text += text;

        // Force layout update and scroll
        narrativeText.ForceMeshUpdate();
        StartCoroutine(ScrollToBottomDelayed());
    }

    private void DisplayChoices(List<Choice> choices)
    {
        ClearChoices();

        // Small delay so text is visible before choices appear
        StartCoroutine(ShowChoicesDelayed(choices));
    }

    private IEnumerator ShowChoicesDelayed(List<Choice> choices)
    {
        yield return new WaitForSeconds(0.3f);

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
        yield return null;
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
