using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NarrativeManager narrativeManager;
    [SerializeField] private AtmosphericController atmosphericController;
    [SerializeField] private NarrativeScroller scroller;
    [SerializeField] private GlitchController glitchController;

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Typewriter")]
    [SerializeField] private float typewriterSpeed = 30f;

    [Header("Audio — ELARA Typing")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip elaraTypeClip;

    [Header("Overlays")]
    [SerializeField] private Image fadeOverlay; // black, alpha 0 at rest

    [Header("Choices")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Start Screen")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Button startButtonA;
    [SerializeField] private Button startButtonB;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI endText;

    // ── Runtime state ──────────────────────────────────────────────────────────
    private List<Button> activeChoiceButtons = new List<Button>();
    private Coroutine typewriterCoroutine;
    private Coroutine fadeOutRoutine;
    private bool isTyping;
    private List<Choice> pendingChoices;
    private int cachedCharCount;
    private bool skipCooldown;

    // Glitch / speed variation
    private int prevOpenness;
    private int prevResistance;
    private int prevMysteryAwareness;
    private bool pendingTextGlitch;

    // True during the ELARA "..." dot animation — blocks click-to-skip
    private bool processingPause;

    // ELARA features
    private string currentStyledText;
    private HashSet<int> elaraCharIndices = new HashSet<int>();
    private float lastTypingSoundTime;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Start()
    {
        narrativeManager.OnNarrativeText   += DisplayText;
        narrativeManager.OnChoicesPresented += DisplayChoices;
        narrativeManager.OnStoryEnd        += HandleStoryEnd;

        startButtonA.onClick.AddListener(() => StartGame("A"));
        startButtonB.onClick.AddListener(() => StartGame("B"));

        narrativeText.text = "";
        endScreen.SetActive(false);
        startScreen.SetActive(true);
        choiceContainer.gameObject.SetActive(false);

        if (fadeOverlay != null)
            SetOverlayAlpha(fadeOverlay, 0f);
    }

    private void Update()
    {
        if (!isTyping || skipCooldown || processingPause) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SkipTypewriter();
    }

    private void LateUpdate() => skipCooldown = false;

    // ── Game flow ──────────────────────────────────────────────────────────────
    private void StartGame(string condition)
    {
        startScreen.SetActive(false);
        narrativeManager.SetCondition(condition);
        if (atmosphericController != null)
            atmosphericController.SetCondition(condition);
        narrativeManager.StartStory();
    }

    // ── Text display ───────────────────────────────────────────────────────────
    private void DisplayText(string text)
    {
        choiceContainer.gameObject.SetActive(false);

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            isTyping = false;
        }

        UpdateTypewriterSpeed();

        // Detect text glitch BEFORE styling (variables are already updated by MakeChoice)
        pendingTextGlitch = narrativeManager != null
            && narrativeManager.CurrentCondition == "B"
            && glitchController != null
            && GetIntVar("mystery_awareness") > prevMysteryAwareness;

        currentStyledText = StyleElaraLines(text);
        narrativeText.text = currentStyledText;
        narrativeText.maxVisibleCharacters = int.MaxValue;
        narrativeText.ForceMeshUpdate();

        cachedCharCount = narrativeText.textInfo.characterCount;
        BuildElaraCharIndices();

        if (scroller != null) scroller.pauseMeshUpdate = true;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private string StyleElaraLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("ELARA:") || trimmed.StartsWith("ELARA "))
                lines[i] = "<color=#00E5CC>" + lines[i] + "</color>";
        }
        return string.Join("\n", lines);
    }

    /// <summary>
    /// After ForceMeshUpdate, marks which visible character indices belong to ELARA lines.
    /// Used to play the typing sound only on ELARA's speech.
    /// TMP's characterInfo gives actual characters (not tag chars), so "ELARA" is detectable.
    /// </summary>
    private void BuildElaraCharIndices()
    {
        elaraCharIndices.Clear();
        var textInfo = narrativeText.textInfo;

        for (int li = 0; li < textInfo.lineCount; li++)
        {
            var lineInfo = textInfo.lineInfo[li];
            if (lineInfo.characterCount <= 0) continue;

            // Sample the first characters of this TMP line to detect "ELARA"
            string lineStart = "";
            int checkLen = Mathf.Min(8, lineInfo.characterCount);
            for (int ci = lineInfo.firstCharacterIndex; ci < lineInfo.firstCharacterIndex + checkLen; ci++)
            {
                if (ci < textInfo.characterInfo.Length)
                    lineStart += textInfo.characterInfo[ci].character;
            }

            if (lineStart.TrimStart().StartsWith("ELARA"))
            {
                for (int ci = lineInfo.firstCharacterIndex;
                     ci < lineInfo.firstCharacterIndex + lineInfo.characterCount; ci++)
                    elaraCharIndices.Add(ci);
            }
        }
    }

    // ── Typewriter ─────────────────────────────────────────────────────────────
    private IEnumerator TypewriterRoutine()
    {
        // Must be true before the first yield so DisplayChoices (fired synchronously
        // from ContinueStory right after DisplayText) stores choices as pendingChoices
        // instead of showing them immediately.
        isTyping = true;

        yield return null; // settle layout

        // Fade overlay back to transparent — runs concurrently with what follows
        fadeOutRoutine = null;
        if (fadeOverlay != null)
            fadeOutRoutine = StartCoroutine(FadeOverlay(1f, 0f, 0.5f));

        // ── Condition A: ELARA processing pause — dots appear one at a time ────
        if (narrativeManager != null
            && narrativeManager.CurrentCondition == "A"
            && currentStyledText != null
            && currentStyledText.Contains("ELARA:"))
        {
            processingPause = true; // block click-to-skip for the duration of the animation

            string[] frames    = { "ELARA: ", "ELARA: .", "ELARA: ..", "ELARA: ..." };
            float[]  durations = {    0.2f,       0.28f,      0.28f,       0.35f    };

            for (int f = 0; f < frames.Length; f++)
            {
                narrativeText.text = "<color=#00E5CC>" + frames[f] + "</color>";
                narrativeText.maxVisibleCharacters = int.MaxValue;
                yield return new WaitForSeconds(durations[f]);
            }

            processingPause = false;

            // Restore actual text and re-cache
            narrativeText.text = currentStyledText;
            narrativeText.ForceMeshUpdate();
            cachedCharCount = narrativeText.textInfo.characterCount;
            BuildElaraCharIndices();
        }
        // ── Condition B: text glitch (waits for full reveal first) ─────────────
        else if (pendingTextGlitch)
        {
            pendingTextGlitch = false;
            if (fadeOutRoutine != null)
                yield return fadeOutRoutine; // wait until screen is fully visible
            yield return StartCoroutine(glitchController.RunTextGlitch());
        }

        skipCooldown = true;

        int totalChars = cachedCharCount;
        narrativeText.maxVisibleCharacters = 0;
        float charsRevealed = 0f;
        bool wasInElara = false;

        while ((int)charsRevealed < totalChars)
        {
            charsRevealed += typewriterSpeed * Time.deltaTime;
            int visible = Mathf.Min((int)charsRevealed, totalChars);
            narrativeText.maxVisibleCharacters = visible;

            // Detect entry into / exit from ELARA speech and play or stop the typing sound
            bool inElara = visible > 0 && elaraCharIndices.Contains(visible - 1);
            if (inElara && !wasInElara && typingAudioSource != null && elaraTypeClip != null)
            {
                typingAudioSource.clip = elaraTypeClip;
                typingAudioSource.loop = true;
                typingAudioSource.Play();
            }
            else if (!inElara && wasInElara && typingAudioSource != null)
            {
                typingAudioSource.Stop();
            }
            wasInElara = inElara;

            yield return null;
        }

        // Always stop typing sound when typewriter finishes
        if (typingAudioSource != null) typingAudioSource.Stop();

        narrativeText.maxVisibleCharacters = totalChars;
        isTyping = false;
        typewriterCoroutine = null;
        fadeOutRoutine = null;

        if (scroller != null)
        {
            scroller.pauseMeshUpdate = false;
            scroller.ScrollToBottom();
        }

        ShowPendingChoices();
    }

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        processingPause = false;

        // If fade-out is still running, complete it immediately
        if (fadeOutRoutine != null)
        {
            StopCoroutine(fadeOutRoutine);
            fadeOutRoutine = null;
            if (fadeOverlay != null) SetOverlayAlpha(fadeOverlay, 0f);
        }

        if (typingAudioSource != null) typingAudioSource.Stop();

        narrativeText.maxVisibleCharacters = cachedCharCount;
        isTyping = false;

        if (scroller != null)
        {
            scroller.pauseMeshUpdate = false;
            scroller.ScrollToBottom();
        }

        ShowPendingChoices();
    }

    // ── Choices ────────────────────────────────────────────────────────────────
    private void DisplayChoices(List<Choice> choices)
    {
        if (isTyping)
            pendingChoices = choices;
        else
        {
            ClearChoices();
            StartCoroutine(ShowChoicesDelayed(choices));
        }
    }

    private void ShowPendingChoices()
    {
        if (pendingChoices == null || pendingChoices.Count == 0) return;
        ClearChoices();
        StartCoroutine(ShowChoicesDelayed(pendingChoices));
        pendingChoices = null;
    }

    private IEnumerator ShowChoicesDelayed(List<Choice> choices)
    {
        yield return new WaitForSeconds(0.3f);
        choiceContainer.gameObject.SetActive(true);

        foreach (Choice choice in choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = choice.text;

            // Each button fades in individually
            CanvasGroup cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            StartCoroutine(FadeInCanvasGroup(cg, 0.4f));

            int index = choice.index;
            button.onClick.AddListener(() => OnChoiceSelected(index));
            activeChoiceButtons.Add(button);

            yield return new WaitForSeconds(0.5f); // stagger delay
        }
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (cg != null) cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
    }

    private void OnChoiceSelected(int index)
    {
        skipCooldown = true;

        // Capture variable state before Ink processes the choice
        prevOpenness         = GetIntVar("openness");
        prevResistance       = GetIntVar("resistance");
        prevMysteryAwareness = GetIntVar("mystery_awareness");

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            isTyping = false;
        }

        if (fadeOutRoutine != null)
        {
            StopCoroutine(fadeOutRoutine);
            fadeOutRoutine = null;
        }

        ClearChoices();
        choiceContainer.gameObject.SetActive(false);
        pendingChoices = null;

        StartCoroutine(FadeAndContinue(index));
    }

    /// <summary>
    /// Fades screen to black, then advances the story.
    /// TypewriterRoutine fades the screen back in once the new text begins.
    /// </summary>
    private IEnumerator FadeAndContinue(int index)
    {
        yield return StartCoroutine(FadeOverlay(0f, 1f, 0.5f));

        if (scroller != null)
        {
            scroller.pauseMeshUpdate = false;
            scroller.ResetScroll();
        }

        narrativeText.text = "";
        narrativeText.maxVisibleCharacters = int.MaxValue;

        // MakeChoice → ContinueStory → DisplayText fires synchronously here.
        // DisplayText starts TypewriterRoutine, which fades the overlay back out.
        narrativeManager.MakeChoice(index);
    }

    // ── Overlay helpers ────────────────────────────────────────────────────────
    private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;
        SetOverlayAlpha(fadeOverlay, fromAlpha);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetOverlayAlpha(fadeOverlay, Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration));
            yield return null;
        }
        SetOverlayAlpha(fadeOverlay, toAlpha);
    }

    private static void SetOverlayAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    // ── Speed + variable helpers ───────────────────────────────────────────────
    private void UpdateTypewriterSpeed()
    {
        if (narrativeManager == null || narrativeManager.CurrentCondition != "B") return;

        int newOpenness   = GetIntVar("openness");
        int newResistance = GetIntVar("resistance");

        if (newOpenness > prevOpenness)         typewriterSpeed = 20f;
        else if (newResistance > prevResistance) typewriterSpeed = 45f;
        else                                     typewriterSpeed = 30f;
    }

    private int GetIntVar(string varName)
    {
        object val = narrativeManager?.GetVariable(varName);
        if (val is int i)  return i;
        if (val is long l) return (int)l;
        return 0;
    }

    // ── Other ──────────────────────────────────────────────────────────────────
    private void ClearChoices()
    {
        foreach (Button b in activeChoiceButtons) Destroy(b.gameObject);
        activeChoiceButtons.Clear();
    }

    private void HandleStoryEnd()
    {
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // OnStoryEnd fires synchronously from ContinueStory, before the final
        // typewriter has had a chance to run. Wait for it to finish naturally
        // so the player can read the last screen in full.
        while (isTyping)
            yield return null;

        // Reading pause — give the player time to absorb the final text
        yield return new WaitForSeconds(3f);

        if (typingAudioSource != null) typingAudioSource.Stop();
        choiceContainer.gameObject.SetActive(false);

        // Fade to black
        yield return StartCoroutine(FadeOverlay(0f, 1f, 0.5f));

        // Clear narrative text before revealing end screen
        narrativeText.text = "";
        if (scroller != null) scroller.pauseMeshUpdate = false;

        endText.text = "Session Complete";
        endScreen.SetActive(true);

        // Fade in to reveal end screen
        yield return StartCoroutine(FadeOverlay(1f, 0f, 0.8f));
    }

    private void OnDestroy()
    {
        if (narrativeManager != null)
        {
            narrativeManager.OnNarrativeText    -= DisplayText;
            narrativeManager.OnChoicesPresented -= DisplayChoices;
            narrativeManager.OnStoryEnd         -= HandleStoryEnd;
        }
    }
}
