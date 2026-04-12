using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Two glitch effects for Condition B atmospheric adaptation.
///
/// Screen glitch  — full-screen overlay flash + canvas position jitter.
///                  Fires once at the corridor → room transition (Scene 3 → 4).
///
/// Text glitch    — character scramble on the narrative text, run BEFORE
///                  the typewriter begins so it is always visible even if
///                  the player clicks to skip the typewriter.
///                  Fires whenever mystery_awareness increases.
/// </summary>
public class GlitchController : MonoBehaviour
{
    [Header("Screen Glitch")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Image screenFlashOverlay;  // full-screen, alpha 0 at rest

    [Header("Text Glitch")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    private static readonly char[] glitchChars = { '░', '▓', '█', '╳', '◆', '▒', '■', '□' };

    private Coroutine screenGlitchCoroutine;

    // ── Public API ────────────────────────────────────────────────────────────

    public void TriggerScreenGlitch()
    {
        if (screenGlitchCoroutine != null)
            StopCoroutine(screenGlitchCoroutine);
        screenGlitchCoroutine = StartCoroutine(ScreenGlitchRoutine());
    }

    /// <summary>
    /// Called from UIManager via yield return StartCoroutine(glitchController.RunTextGlitch())
    /// so the typewriter waits for the glitch to finish before it begins.
    /// </summary>
    public IEnumerator RunTextGlitch()
    {
        if (narrativeText == null) yield break;

        string original = narrativeText.text;
        const int flashes = 3;
        const float interval = 0.1f;

        for (int i = 0; i < flashes; i++)
        {
            narrativeText.text = ScrambleText(original, 0.30f);
            yield return new WaitForSeconds(interval);
            narrativeText.text = original;
            yield return new WaitForSeconds(interval * 0.5f);
        }

        narrativeText.text = original;
    }

    // ── Screen glitch implementation ─────────────────────────────────────────

    private IEnumerator ScreenGlitchRoutine()
    {
        if (mainCanvas == null) yield break;

        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Vector2 originalPos = canvasRect.anchoredPosition;

        float duration = 0.55f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float intensity = Mathf.Sin(t * Mathf.PI); // bell curve: 0 → peak → 0

            // Canvas position jitter — shifts the entire screen by a few pixels
            float jitter = intensity * 5f;
            canvasRect.anchoredPosition = originalPos + new Vector2(
                Random.Range(-jitter, jitter),
                Random.Range(-jitter, jitter)
            );

            // Overlay flash — semi-transparent blue-purple tint
            if (screenFlashOverlay != null)
            {
                Color c = screenFlashOverlay.color;
                // Flicker the alpha at a fast rate on top of the bell curve
                float flicker = Mathf.Sin(elapsed * 80f) * 0.5f + 0.5f;
                c.a = intensity * 0.22f * flicker;
                screenFlashOverlay.color = c;
            }

            yield return null;
        }

        // Reset to resting state
        canvasRect.anchoredPosition = originalPos;
        if (screenFlashOverlay != null)
        {
            Color c = screenFlashOverlay.color;
            c.a = 0f;
            screenFlashOverlay.color = c;
        }

        screenGlitchCoroutine = null;
    }

    // ── Text scramble ─────────────────────────────────────────────────────────

    private string ScrambleText(string text, float ratio)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(text);
        bool inTag = false;

        for (int i = 0; i < sb.Length; i++)
        {
            char c = sb[i];
            if (c == '<') { inTag = true; continue; }
            if (c == '>') { inTag = false; continue; }
            if (inTag) continue;
            if (c == ' ' || c == '\n' || c == '\r') continue;

            if (Random.value < ratio)
                sb[i] = glitchChars[Random.Range(0, glitchChars.Length)];
        }

        return sb.ToString();
    }
}
