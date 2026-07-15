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
    [SerializeField] private BehavioralLogger behavioralLogger;

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Typewriter")]
    [SerializeField] private float typewriterSpeed = 30f;

    [Header("Audio — ELARA Typing")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip elaraTypeClip;

    [Header("Overlays")]
    [SerializeField] private Image fadeOverlay; // black, alpha 0 at rest

    [Header("Scene Images (Condition B)")]
    [SerializeField] private RawImage sceneImageDisplay; // fullscreen behind text, assign in editor

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

    private Button restartButton;

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

    // ELARA pause — false start tracking
    private bool elaraFalseStartPending;
    private bool elaraFalseStartUsed; // ensures false start only fires once per playthrough

    // True during the ELARA "..." dot animation — blocks click-to-skip
    private bool processingPause;

    // Inner conflict line extracted from text block, shown separately before choices
    private string pendingInnerConflict;

    // Typewriter stall + screen dim — triggered once at the room reveal (Condition A only)
    private bool pendingPulse;
    private bool screenDimmed; // stays true once dimmed — permanent for rest of session

    // Reflection — maps (scene, choiceIndex) to a short mirror line
    private static readonly Dictionary<string, string[]> reflectionMap = new Dictionary<string, string[]>
    {
        { "dinner",    new[] { "You let it pass.", "You pulled at the thread." } },
        { "bench",     new[] { "You opened yourself to it.", "You held him together instead.", "You gave him silence.", "You looked away." } },
        { "corridor",  new[] { "You stopped.", "You asked the question." } },
        { "room",      new[] { "You let it count.", "You refused it.", "You didn't pretend to know.", "You turned the mirror." } },
        { "discharge", new[] { "You chose to return.", "You let him go.", "You kept it." } },
    };

    // Scene background images (Condition B)
    private Dictionary<string, Texture2D> sceneImages = new Dictionary<string, Texture2D>();
    private Coroutine sceneImageFadeRoutine;


    // ELARA / Liam features
    private string currentStyledText;
    private HashSet<int> elaraCharIndices = new HashSet<int>();
    private HashSet<int> liamCharIndices = new HashSet<int>();
    private float lastTypingSoundTime;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Start()
    {
        narrativeManager.OnNarrativeText   += DisplayText;
        narrativeManager.OnChoicesPresented += DisplayChoices;
        narrativeManager.OnStoryEnd        += HandleStoryEnd;
        narrativeManager.OnTagReceived     += OnTag;

        startButtonA.onClick.AddListener(() => StartGame("A"));
        startButtonB.onClick.AddListener(() => StartGame("B"));

        narrativeText.text = "";
        endScreen.SetActive(false);
        startScreen.SetActive(true);
        choiceContainer.gameObject.SetActive(false);

        if (fadeOverlay != null)
            SetOverlayAlpha(fadeOverlay, 0f);

        // Load scene images from Resources/SceneImages
        string[] sceneNames = { "dinner", "bench", "corridor", "room", "discharge" };
        foreach (string s in sceneNames)
        {
            Texture2D tex = Resources.Load<Texture2D>("SceneImages/" + s);
            if (tex != null) sceneImages[s] = tex;
        }
        if (sceneImageDisplay != null)
        {
            sceneImageDisplay.raycastTarget = false;
            sceneImageDisplay.color = new Color(1f, 1f, 1f, 0f);
            sceneImageDisplay.gameObject.SetActive(false);
        }
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

        // Detect ELARA false-start condition: player has questioned enough inconsistencies
        // (mystery_awareness >= 2) AND just confronted (resistance rose on this choice).
        // Fires once — elaraFalseStartUsed prevents repeats.
        elaraFalseStartPending = narrativeManager != null
            && narrativeManager.CurrentCondition == "A"
            && !elaraFalseStartUsed
            && GetIntVar("mystery_awareness") >= 2
            && GetIntVar("resistance") > prevResistance;

        currentStyledText = StyleSpecialLines(text);
        narrativeText.text = currentStyledText;
        narrativeText.maxVisibleCharacters = int.MaxValue;
        narrativeText.ForceMeshUpdate();

        cachedCharCount = narrativeText.textInfo.characterCount;
        BuildElaraCharIndices();

        if (scroller != null) scroller.pauseMeshUpdate = true;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private string StyleSpecialLines(string text)
    {
        pendingInnerConflict = null;
        string[] lines = text.Split('\n');
        List<string> outputLines = new List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("ELARA:") || trimmed.StartsWith("ELARA "))
                outputLines.Add("<color=#00E5CC>" + lines[i] + "</color>");
            else if (trimmed.StartsWith("Liam:") || trimmed.StartsWith("Liam "))
                outputLines.Add("<i>" + lines[i] + "</i>");
            else if (trimmed.StartsWith("[inner]"))
            {
                if (narrativeManager != null && narrativeManager.CurrentCondition == "A")
                    pendingInnerConflict = trimmed.Replace("[inner]", "").Trim();
                // Condition B: silently discard the line
            }
            else
                outputLines.Add(lines[i]);
        }
        return string.Join("\n", outputLines);
    }

    /// <summary>
    /// After ForceMeshUpdate, marks which visible character indices belong to ELARA lines.
    /// Used to play the typing sound only on ELARA's speech.
    /// TMP's characterInfo gives actual characters (not tag chars), so "ELARA" is detectable.
    /// </summary>
    private void BuildElaraCharIndices()
    {
        elaraCharIndices.Clear();
        liamCharIndices.Clear();
        var textInfo = narrativeText.textInfo;

        for (int li = 0; li < textInfo.lineCount; li++)
        {
            var lineInfo = textInfo.lineInfo[li];
            if (lineInfo.characterCount <= 0) continue;

            // Sample the first characters of this TMP line to detect speaker
            string lineStart = "";
            int checkLen = Mathf.Min(8, lineInfo.characterCount);
            for (int ci = lineInfo.firstCharacterIndex; ci < lineInfo.firstCharacterIndex + checkLen; ci++)
            {
                if (ci < textInfo.characterInfo.Length)
                    lineStart += textInfo.characterInfo[ci].character;
            }

            string trimmed = lineStart.TrimStart();
            if (trimmed.StartsWith("ELARA"))
            {
                for (int ci = lineInfo.firstCharacterIndex;
                     ci < lineInfo.firstCharacterIndex + lineInfo.characterCount; ci++)
                    elaraCharIndices.Add(ci);
            }
            else if (trimmed.StartsWith("Liam"))
            {
                for (int ci = lineInfo.firstCharacterIndex;
                     ci < lineInfo.firstCharacterIndex + lineInfo.characterCount; ci++)
                    liamCharIndices.Add(ci);
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

        // Fade overlay back to transparent (or to dim level if screen has been dimmed)
        fadeOutRoutine = null;
        if (fadeOverlay != null)
        {
            float targetAlpha = screenDimmed ? 0.22f : 0f;
            fadeOutRoutine = StartCoroutine(FadeOverlay(1f, targetAlpha, 0.5f));
        }

        // ── Condition A: ELARA processing pause — dots appear one at a time ────
        if (narrativeManager != null
            && narrativeManager.CurrentCondition == "A"
            && currentStyledText != null
            && currentStyledText.Contains("ELARA:"))
        {
            processingPause = true; // block click-to-skip for the duration of the animation

            // Progressive pause: ELARA's hesitation grows as the session deepens
            string scene = narrativeManager.GetCurrentScene();
            float sceneScale;
            switch (scene)
            {
                case "intake":
                case "dinner":   sceneScale = 0.45f; break;  // early — ELARA is confident (~0.5s)
                case "bench":
                case "corridor": sceneScale = 1.2f;  break;  // mid — session deepens (~1.33s)
                case "room":
                case "discharge":sceneScale = 2.5f;  break;  // late — emotional weight (~2.8s)
                default:         sceneScale = 1.0f;  break;
            }

            string[] frames    = { "ELARA: ", "ELARA: .", "ELARA: ..", "ELARA: ..." };
            float[] baseDur    = { 0.2f, 0.28f, 0.28f, 0.35f };
            float[] durations  = new float[4];
            for (int i = 0; i < 4; i++)
                durations[i] = baseDur[i] * sceneScale;

            for (int f = 0; f < frames.Length; f++)
            {
                narrativeText.text = "<color=#00E5CC>" + frames[f] + "</color>";
                narrativeText.maxVisibleCharacters = int.MaxValue;
                yield return new WaitForSeconds(durations[f]);
            }

            // False start: ELARA begins typing then reconsiders.
            // Triggers once per playthrough when mystery_awareness >= 2 and player just confronted.
            if (elaraFalseStartPending)
            {
                elaraFalseStartPending = false;
                elaraFalseStartUsed = true;

                string elaraSnippet = ExtractElaraSnippet(currentStyledText, 4);
                if (!string.IsNullOrEmpty(elaraSnippet))
                {
                    // Show first few characters as if ELARA started responding
                    narrativeText.text = "<color=#00E5CC>ELARA: " + elaraSnippet + "</color>";
                    narrativeText.maxVisibleCharacters = int.MaxValue;
                    yield return new WaitForSeconds(0.35f);

                    // Clear — ELARA reconsiders
                    narrativeText.text = "<color=#00E5CC>ELARA: </color>";
                    narrativeText.maxVisibleCharacters = int.MaxValue;
                    yield return new WaitForSeconds(0.4f);

                    // Replay dots (shorter second pass)
                    for (int f = 1; f < frames.Length; f++)
                    {
                        narrativeText.text = "<color=#00E5CC>" + frames[f] + "</color>";
                        narrativeText.maxVisibleCharacters = int.MaxValue;
                        yield return new WaitForSeconds(durations[f] * 0.7f);
                    }
                }
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

        // Find stall point: the period after "memory" in "This isn't a memory."
        int stallCharIndex = -1;
        if (pendingPulse)
        {
            pendingPulse = false;
            var charInfo = narrativeText.textInfo.characterInfo;
            for (int ci = 6; ci < totalChars; ci++)
            {
                if (charInfo[ci].character == '.'
                    && charInfo[ci - 1].character == 'y'
                    && charInfo[ci - 2].character == 'r'
                    && charInfo[ci - 3].character == 'o'
                    && charInfo[ci - 4].character == 'm'
                    && charInfo[ci - 5].character == 'e'
                    && charInfo[ci - 6].character == 'm')
                {
                    stallCharIndex = ci;
                    break;
                }
            }
        }
        bool stallFired = false;

        while ((int)charsRevealed < totalChars)
        {
            // Condition A speaker-based speed (before the reveal dims the screen)
            if (!screenDimmed && narrativeManager != null && narrativeManager.CurrentCondition == "A")
            {
                int nextChar = Mathf.Min((int)charsRevealed, totalChars - 1);
                if (elaraCharIndices.Contains(nextChar))
                    typewriterSpeed = 42f;       // ELARA: fast, clinical, machine-like
                else if (liamCharIndices.Contains(nextChar))
                    typewriterSpeed = 22f;        // Liam: slow, weighted, memory
                else
                    typewriterSpeed = 30f;         // narrator: default
            }

            charsRevealed += typewriterSpeed * Time.deltaTime;
            int visible = Mathf.Min((int)charsRevealed, totalChars);
            narrativeText.maxVisibleCharacters = visible;

            // ── THE BREAK: text erases, blackout, then slow resume ────────────
            if (!stallFired && stallCharIndex >= 0 && visible >= stallCharIndex)
            {
                stallFired = true;
                processingPause = true; // block click-to-skip entirely

                if (typingAudioSource != null) typingAudioSource.Stop();

                // 1) Pause on "This isn't a memory." for 1 second — let the player read it
                yield return new WaitForSeconds(1.0f);

                // 2) Text fades out — dissolve what's on screen over 2 seconds
                //    Done by fading the narrative text color alpha from 1 → 0
                Color textColor = narrativeText.color;
                float fadeElapsed = 0f;
                while (fadeElapsed < 2f)
                {
                    fadeElapsed += Time.deltaTime;
                    float a = Mathf.Lerp(1f, 0f, fadeElapsed / 2f);
                    narrativeText.color = new Color(textColor.r, textColor.g, textColor.b, a);
                    yield return null;
                }
                narrativeText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);

                // 3) Full blackout — overlay to 100% black
                if (fadeOverlay != null)
                    fadeOverlay.color = new Color(0f, 0f, 0f, 1f);

                // 4) 3 seconds of total darkness — black screen, no text, no sound
                yield return new WaitForSeconds(3f);

                // 5) Clear old text, prepare the full text block for re-typing
                narrativeText.text = currentStyledText;
                narrativeText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
                narrativeText.maxVisibleCharacters = 0;
                narrativeText.ForceMeshUpdate();
                cachedCharCount = narrativeText.textInfo.characterCount;
                totalChars = cachedCharCount;
                BuildElaraCharIndices();
                charsRevealed = 0f;
                // Disable stall so it doesn't re-trigger
                stallCharIndex = -1;

                // 6) Fade screen back in to dimmed state (0.22 alpha) over 1 second
                if (fadeOverlay != null)
                {
                    float dimElapsed = 0f;
                    while (dimElapsed < 1f)
                    {
                        dimElapsed += Time.deltaTime;
                        float a = Mathf.Lerp(1f, 0.22f, dimElapsed / 1f);
                        fadeOverlay.color = new Color(0f, 0f, 0f, a);
                        yield return null;
                    }
                    fadeOverlay.color = new Color(0f, 0f, 0f, 0.22f);
                }
                screenDimmed = true;

                // 7) Permanently slow the typewriter speed for rest of session
                typewriterSpeed = 18f;

                if (scroller != null) scroller.ResetScroll();

                processingPause = false;

                // Typewriter loop continues — now re-typing the entire block at half speed
                // in a darker room. The game changed.
            }

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
            StartCoroutine(ShowInnerThenChoices(choices));
        }
    }

    private void ShowPendingChoices()
    {
        if (pendingChoices == null || pendingChoices.Count == 0) return;
        ClearChoices();
        StartCoroutine(ShowInnerThenChoices(pendingChoices));
        pendingChoices = null;
    }

    private IEnumerator ShowInnerThenChoices(List<Choice> choices)
    {
        // Show inner conflict line with a fade-in before choices
        if (!string.IsNullOrEmpty(pendingInnerConflict))
        {
            yield return new WaitForSeconds(0.6f);

            string baseText = narrativeText.text;
            string innerLine = "*" + pendingInnerConflict + "*";
            pendingInnerConflict = null;

            // Pause scroller mesh updates during fade to prevent tag bleed
            if (scroller != null) scroller.pauseMeshUpdate = true;

            // Fade in over 0.8s by progressively increasing color alpha
            float fadeDuration = 0.8f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float alpha = t * t;
                byte a = (byte)(255 * alpha);
                string hex = a.ToString("X2");
                narrativeText.text = baseText + "\n\n<align=\"center\"><color=#7B8C9A" + hex + "><i>" + innerLine + "</i></color></align>";
                narrativeText.maxVisibleCharacters = int.MaxValue;
                narrativeText.ForceMeshUpdate();
                yield return null;
            }

            // Ensure fully visible at the end
            narrativeText.text = baseText + "\n\n<align=\"center\"><color=#7B8C9AFF><i>" + innerLine + "</i></color></align>";
            narrativeText.maxVisibleCharacters = int.MaxValue;
            narrativeText.ForceMeshUpdate();

            // Resume scroller and scroll to bottom
            if (scroller != null)
            {
                scroller.pauseMeshUpdate = false;
                scroller.ScrollToBottom();
            }

            yield return new WaitForSeconds(1.0f);
        }

        // Then show the choices
        yield return StartCoroutine(ShowChoicesDelayed(choices));
    }

    private IEnumerator ShowChoicesDelayed(List<Choice> choices)
    {
        yield return new WaitForSeconds(0.3f);
        choiceContainer.gameObject.SetActive(true);

        if (choices.Count >= 4)
        {
            // Create row containers
            GameObject row1 = CreateChoiceRow("Row1");
            GameObject row2 = CreateChoiceRow("Row2");

            for (int c = 0; c < choices.Count; c++)
            {
                Transform parent = (c < 2) ? row1.transform : row2.transform;
                Button button = Instantiate(choiceButtonPrefab, parent);
                button.gameObject.SetActive(true);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choices[c].text;
                    buttonText.enableWordWrapping = false;
                    buttonText.ForceMeshUpdate();

                    // Set button width to fit text + padding
                    float textWidth = buttonText.preferredWidth;
                    float padding = 30f;
                    RectTransform btnRT = button.GetComponent<RectTransform>();
                    if (btnRT != null)
                        btnRT.sizeDelta = new Vector2(textWidth + padding, btnRT.sizeDelta.y);
                }

                // Tell the HLG not to resize this button
                var le = button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                le.preferredWidth = button.GetComponent<RectTransform>().sizeDelta.x;
                le.preferredHeight = 35f;

                CanvasGroup cg = button.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                StartCoroutine(FadeInCanvasGroup(cg, 0.4f));

                int index = choices[c].index;
                button.onClick.AddListener(() => OnChoiceSelected(index));
                activeChoiceButtons.Add(button);

                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            // Standard vertical layout for 2-3 choices
            foreach (Choice choice in choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceContainer);
                button.gameObject.SetActive(true);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = choice.text;

                CanvasGroup cg = button.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                StartCoroutine(FadeInCanvasGroup(cg, 0.4f));

                int index = choice.index;
                button.onClick.AddListener(() => OnChoiceSelected(index));
                activeChoiceButtons.Add(button);

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private GameObject CreateChoiceRow(string name)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(choiceContainer, false);

        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(0f, 35f);

        var hlg = row.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hlg.spacing = 15f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.padding = new RectOffset(5, 5, 0, 0);

        var rowFitter = row.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        rowFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
        rowFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        return row;
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

        pendingChoices = null;

        StartCoroutine(ReflectThenContinue(index));
    }

    private IEnumerator ReflectThenContinue(int index)
    {
        // Look up reflection by scene and choice index (Condition A only)
        string reflection = null;
        if (narrativeManager != null && narrativeManager.CurrentCondition == "A")
        {
            string scene = narrativeManager.GetCurrentScene();
            string[] sceneReflections;
            if (reflectionMap.TryGetValue(scene, out sceneReflections)
                && index >= 0 && index < sceneReflections.Length)
            {
                reflection = sceneReflections[index];
            }
        }

        Button clickedButton = (index >= 0 && index < activeChoiceButtons.Count)
            ? activeChoiceButtons[index] : null;

        if (reflection != null && clickedButton != null)
        {
            clickedButton.interactable = false;

            // Fade out the OTHER buttons over 0.3s
            List<CanvasGroup> otherCGs = new List<CanvasGroup>();
            for (int i = 0; i < activeChoiceButtons.Count; i++)
            {
                if (i != index)
                {
                    CanvasGroup cg = activeChoiceButtons[i].GetComponent<CanvasGroup>();
                    if (cg != null) otherCGs.Add(cg);
                }
            }
            float fadeElapsed = 0f;
            while (fadeElapsed < 0.3f)
            {
                fadeElapsed += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, fadeElapsed / 0.3f);
                foreach (var cg in otherCGs)
                    if (cg != null) cg.alpha = a;
                yield return null;
            }
            // Destroy the faded-out buttons
            for (int i = activeChoiceButtons.Count - 1; i >= 0; i--)
            {
                if (i != index)
                {
                    Destroy(activeChoiceButtons[i].gameObject);
                    activeChoiceButtons.RemoveAt(i);
                }
            }

            // Brief pause — the player sees their choice sitting alone
            yield return new WaitForSeconds(0.5f);

            // Fade out the clicked button text
            CanvasGroup clickedCG = clickedButton.GetComponent<CanvasGroup>();
            if (clickedCG != null)
            {
                fadeElapsed = 0f;
                while (fadeElapsed < 0.25f)
                {
                    fadeElapsed += Time.deltaTime;
                    clickedCG.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / 0.25f);
                    yield return null;
                }
                clickedCG.alpha = 0f;
            }

            // Swap the text while invisible
            TextMeshProUGUI reflectText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (reflectText != null)
                reflectText.text = reflection;

            // Fade back in with the reflection text
            if (clickedCG != null)
            {
                fadeElapsed = 0f;
                while (fadeElapsed < 0.35f)
                {
                    fadeElapsed += Time.deltaTime;
                    clickedCG.alpha = Mathf.Lerp(0f, 1f, fadeElapsed / 0.35f);
                    yield return null;
                }
                clickedCG.alpha = 1f;
            }

            // Hold for 2 seconds — the player reads what they did
            yield return new WaitForSeconds(2.0f);
        }

        // Clean up remaining buttons
        ClearChoices();
        choiceContainer.gameObject.SetActive(false);

        yield return StartCoroutine(FadeAndContinue(index));
    }

    /// <summary>
    /// Fades screen to black, then advances the story.
    /// TypewriterRoutine fades the screen back in once the new text begins.
    /// </summary>
    private IEnumerator FadeAndContinue(int index)
    {
        float fromAlpha = screenDimmed ? 0.22f : 0f;
        yield return StartCoroutine(FadeOverlay(fromAlpha, 1f, 0.5f));

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
    /// <summary>
    /// Extracts the first N characters of ELARA's actual dialogue from the styled text
    /// (after "ELARA: "), used for the false-start animation.
    /// </summary>
    private string ExtractElaraSnippet(string styledText, int charCount)
    {
        // Strip color tags to find the raw ELARA line
        string raw = styledText.Replace("<color=#00E5CC>", "").Replace("</color>", "");
        int idx = raw.IndexOf("ELARA:");
        if (idx < 0) return null;

        // Skip "ELARA: " prefix
        int start = idx + "ELARA:".Length;
        while (start < raw.Length && raw[start] == ' ') start++;

        if (start >= raw.Length) return null;

        int len = Mathf.Min(charCount, raw.Length - start);
        return raw.Substring(start, len);
    }

    // ── Tag handling ──────────────────────────────────────────────────────────
    private void OnTag(string tag)
    {
        string trimmed = tag.Trim();

        if (trimmed == "pulse"
            && narrativeManager != null
            && narrativeManager.CurrentCondition == "A")
            pendingPulse = true;

        // Scene images — Condition B only
        if (trimmed.StartsWith("scene:")
            && narrativeManager != null
            && narrativeManager.CurrentCondition == "B"
            && sceneImageDisplay != null)
        {
            string sceneName = trimmed.Substring(6); // e.g. "dinner"
            Texture2D tex;
            if (sceneImages.TryGetValue(sceneName, out tex))
            {
                if (sceneImageFadeRoutine != null)
                    StopCoroutine(sceneImageFadeRoutine);
                sceneImageFadeRoutine = StartCoroutine(CrossfadeSceneImage(tex));
            }
        }
    }


    private IEnumerator CrossfadeSceneImage(Texture2D newTex)
    {
        // Per-scene opacity — brighter images get lower alpha
        string scene = narrativeManager != null ? narrativeManager.GetCurrentScene() : "";
        float maxAlpha;
        switch (scene)
        {
            case "dinner":    maxAlpha = 0.12f; break;
            case "bench":     maxAlpha = 0.10f; break;
            case "corridor":  maxAlpha = 0.10f; break;
            case "room":      maxAlpha = 0.05f; break;
            case "discharge": maxAlpha = 0.08f; break;
            default:          maxAlpha = 0.10f; break;
        }
        float duration = 1.2f;

        // If already showing an image, fade it out first
        if (sceneImageDisplay.gameObject.activeSelf && sceneImageDisplay.color.a > 0.01f)
        {
            float startAlpha = sceneImageDisplay.color.a;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(startAlpha, 0f, elapsed / 0.4f);
                sceneImageDisplay.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
        }

        // Swap texture and fade in
        sceneImageDisplay.texture = newTex;
        sceneImageDisplay.gameObject.SetActive(true);
        sceneImageDisplay.color = new Color(1f, 1f, 1f, 0f);

        float fadeElapsed = 0f;
        while (fadeElapsed < duration)
        {
            fadeElapsed += Time.deltaTime;
            float a = Mathf.Lerp(0f, maxAlpha, fadeElapsed / duration);
            sceneImageDisplay.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        sceneImageDisplay.color = new Color(1f, 1f, 1f, maxAlpha);
    }

    private void ClearChoices()
    {
        foreach (Button b in activeChoiceButtons) Destroy(b.gameObject);
        activeChoiceButtons.Clear();

        // Destroy any row containers created for 2x2 layout
        for (int i = choiceContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = choiceContainer.GetChild(i);
            if (child.name == "Row1" || child.name == "Row2")
                Destroy(child.gameObject);
        }
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

        // Fade to black (from dimmed state if screen was dimmed)
        float endFromAlpha = screenDimmed ? 0.22f : 0f;
        yield return StartCoroutine(FadeOverlay(endFromAlpha, 1f, 0.5f));

        // Clear narrative text and scene image before revealing end screen
        narrativeText.text = "";
        if (scroller != null) scroller.pauseMeshUpdate = false;
        if (sceneImageDisplay != null)
        {
            sceneImageDisplay.color = new Color(1f, 1f, 1f, 0f);
            sceneImageDisplay.gameObject.SetActive(false);
        }

        endText.text = "Session Complete";
        endScreen.SetActive(true);

        // Create restart button below the end text
        if (restartButton != null)
            Destroy(restartButton.gameObject);
        restartButton = CreateRestartButton();

        // Fade in to reveal end screen
        yield return StartCoroutine(FadeOverlay(1f, 0f, 0.8f));
    }

    private Button CreateRestartButton()
    {
        GameObject btnObj = new GameObject("RestartButton", typeof(RectTransform));
        btnObj.transform.SetParent(endScreen.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.3f);
        rt.anchorMax = new Vector2(0.65f, 0.38f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.19f, 1f);

        btnObj.AddComponent<CanvasRenderer>();

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.19f, 1f);
        colors.highlightedColor = new Color(0.22f, 0.22f, 0.28f, 1f);
        colors.pressedColor = new Color(0.12f, 0.12f, 0.16f, 1f);
        btn.colors = colors;
        btn.targetGraphic = img;
        btn.onClick.AddListener(ReturnToStart);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.anchoredPosition = Vector2.zero;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Play Again";
        tmp.color = new Color(0.54f, 0.67f, 0.72f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24f;

        if (narrativeText != null && narrativeText.font != null)
            tmp.font = narrativeText.font;

        return btn;
    }

    private void ReturnToStart()
    {
        // Stop any running coroutines
        StopAllCoroutines();

        // Reset UI
        narrativeText.text = "";
        if (scroller != null) scroller.pauseMeshUpdate = false;
        ClearChoices();
        choiceContainer.gameObject.SetActive(false);
        endScreen.SetActive(false);
        if (restartButton != null)
        {
            Destroy(restartButton.gameObject);
            restartButton = null;
        }

        // Reset scene image
        if (sceneImageDisplay != null)
        {
            sceneImageDisplay.color = new Color(1f, 1f, 1f, 0f);
            sceneImageDisplay.gameObject.SetActive(false);
        }

        // Reset overlay
        if (fadeOverlay != null)
            SetOverlayAlpha(fadeOverlay, 0f);

        // Reset state flags
        isTyping = false;
        skipCooldown = false;
        pendingPulse = false;
        screenDimmed = false;
        pendingTextGlitch = false;
        elaraFalseStartPending = false;
        elaraFalseStartUsed = false;
        processingPause = false;
        pendingInnerConflict = null;
        currentStyledText = null;

        // Reset tracked variable state
        prevOpenness = 0;
        prevResistance = 0;
        prevMysteryAwareness = 0;

        // Reset the Ink story and logger
        narrativeManager.ResetStory();
        if (behavioralLogger != null)
            behavioralLogger.ResetSession();

        // Show start screen
        startScreen.SetActive(true);
    }

    private void OnDestroy()
    {
        if (narrativeManager != null)
        {
            narrativeManager.OnNarrativeText    -= DisplayText;
            narrativeManager.OnChoicesPresented -= DisplayChoices;
            narrativeManager.OnStoryEnd         -= HandleStoryEnd;
            narrativeManager.OnTagReceived      -= OnTag;
        }
    }
}
