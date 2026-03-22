using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles Condition B atmospheric adaptation.
/// Reads mood tags from Ink and adjusts visual/audio presentation.
/// In Condition A, this controller is inactive (text changes handle adaptation).
/// </summary>
public class AtmosphericController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NarrativeManager narrativeManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Audio")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientClinical;
    [SerializeField] private AudioClip ambientWarm;
    [SerializeField] private AudioClip ambientAutumn;
    [SerializeField] private AudioClip ambientHospital;
    [SerializeField] private AudioClip ambientStatic;
    [SerializeField] private AudioClip ambientHeartbeat;

    [Header("Transition Settings")]
    [SerializeField] private float colorTransitionSpeed = 2f;
    [SerializeField] private float audioTransitionSpeed = 1.5f;

    private string currentCondition = "A";
    private string currentMood = "";

    // Mood-to-visual mappings
    private Dictionary<string, MoodProfile> moodProfiles;

    private Color targetBgColor;
    private Color targetTextColor;
    private float targetFontSize;
    private float targetAudioVolume;
    private AudioClip targetAmbientClip;

    private struct MoodProfile
    {
        public Color backgroundColor;
        public Color textColor;
        public float fontSize;
        public float audioVolume;
        public string ambientKey;

        public MoodProfile(Color bg, Color text, float size, float vol, string audio)
        {
            backgroundColor = bg;
            textColor = text;
            fontSize = size;
            audioVolume = vol;
            ambientKey = audio;
        }
    }

    private void Awake()
    {
        InitializeMoodProfiles();
    }

    private void Start()
    {
        if (narrativeManager != null)
            narrativeManager.OnTagReceived += ProcessTag;

        // Set initial state
        ApplyMoodInstant("clinical");
    }

    public void SetCondition(string condition)
    {
        currentCondition = condition;
    }

    private void InitializeMoodProfiles()
    {
        moodProfiles = new Dictionary<string, MoodProfile>();

        // === CLINICAL / INTAKE ===
        Color clinicalBg = new Color(0.12f, 0.12f, 0.14f);         // Dark charcoal
        Color clinicalText = new Color(0.75f, 0.78f, 0.82f);       // Cool silver
        moodProfiles["clinical"] = new MoodProfile(clinicalBg, clinicalText, 24f, 0.3f, "clinical");
        moodProfiles["clinical_soft"] = new MoodProfile(
            new Color(0.13f, 0.13f, 0.16f), clinicalText, 24f, 0.25f, "clinical");
        moodProfiles["clinical_quiet"] = new MoodProfile(clinicalBg, clinicalText, 24f, 0.2f, "clinical");
        moodProfiles["clinical_summary"] = new MoodProfile(clinicalBg, clinicalText, 23f, 0.2f, "clinical");
        moodProfiles["clinical_decision"] = new MoodProfile(
            new Color(0.11f, 0.12f, 0.15f), new Color(0.8f, 0.82f, 0.85f), 24f, 0.15f, "clinical");

        // === WARM / DINNER ===
        Color warmBg = new Color(0.16f, 0.12f, 0.08f);             // Deep warm brown
        Color warmText = new Color(0.92f, 0.85f, 0.72f);           // Warm cream
        moodProfiles["warm_golden"] = new MoodProfile(warmBg, warmText, 24f, 0.5f, "warm");
        moodProfiles["warm_unsettled"] = new MoodProfile(
            new Color(0.17f, 0.11f, 0.08f), warmText, 24f, 0.45f, "warm");
        moodProfiles["warm_settled"] = new MoodProfile(warmBg, warmText, 24f, 0.5f, "warm");
        moodProfiles["warm_tense"] = new MoodProfile(
            new Color(0.18f, 0.10f, 0.07f), new Color(0.95f, 0.82f, 0.68f), 24f, 0.4f, "warm");
        moodProfiles["warm_muted"] = new MoodProfile(
            new Color(0.14f, 0.12f, 0.10f), new Color(0.78f, 0.75f, 0.70f), 24f, 0.35f, "warm");
        moodProfiles["warm_steady"] = new MoodProfile(warmBg, warmText, 24f, 0.5f, "warm");
        moodProfiles["warm_fading"] = new MoodProfile(
            new Color(0.13f, 0.11f, 0.09f), new Color(0.80f, 0.75f, 0.65f), 24f, 0.3f, "warm");
        moodProfiles["warm_with_dissonance"] = new MoodProfile(
            new Color(0.17f, 0.11f, 0.09f), new Color(0.90f, 0.82f, 0.70f), 24f, 0.4f, "warm");
        moodProfiles["warm_wrong"] = new MoodProfile(
            new Color(0.18f, 0.10f, 0.06f), new Color(0.95f, 0.80f, 0.60f), 25f, 0.45f, "warm");

        // === AUTUMN / PARK BENCH ===
        Color autumnBg = new Color(0.12f, 0.11f, 0.10f);           // Muted earth
        Color autumnText = new Color(0.82f, 0.72f, 0.58f);         // Copper-cream
        moodProfiles["autumn_cool"] = new MoodProfile(autumnBg, autumnText, 24f, 0.4f, "autumn");
        moodProfiles["autumn_intimate"] = new MoodProfile(
            new Color(0.14f, 0.11f, 0.09f), new Color(0.85f, 0.74f, 0.58f), 24f, 0.45f, "autumn");
        moodProfiles["autumn_vulnerable"] = new MoodProfile(
            new Color(0.13f, 0.10f, 0.09f), new Color(0.85f, 0.72f, 0.55f), 24f, 0.4f, "autumn");
        moodProfiles["autumn_heavy"] = new MoodProfile(
            new Color(0.11f, 0.10f, 0.09f), new Color(0.78f, 0.68f, 0.55f), 25f, 0.5f, "autumn");
        moodProfiles["autumn_dissonant"] = new MoodProfile(
            new Color(0.12f, 0.09f, 0.08f), new Color(0.82f, 0.65f, 0.50f), 24f, 0.45f, "autumn");
        moodProfiles["autumn_warm_ache"] = new MoodProfile(
            new Color(0.15f, 0.11f, 0.08f), new Color(0.88f, 0.75f, 0.58f), 24f, 0.5f, "autumn");
        moodProfiles["autumn_bright_forced"] = new MoodProfile(
            new Color(0.16f, 0.13f, 0.09f), new Color(0.92f, 0.80f, 0.62f), 24f, 0.45f, "autumn");
        moodProfiles["autumn_sparse"] = new MoodProfile(
            new Color(0.10f, 0.10f, 0.10f), new Color(0.70f, 0.65f, 0.58f), 24f, 0.3f, "autumn");
        moodProfiles["autumn_fading"] = new MoodProfile(
            new Color(0.09f, 0.09f, 0.09f), new Color(0.65f, 0.60f, 0.52f), 24f, 0.25f, "autumn");

        // === CLINICAL / HOSPITAL CORRIDOR ===
        Color corridorBg = new Color(0.08f, 0.10f, 0.08f);         // Sickly dark green-grey
        Color corridorText = new Color(0.78f, 0.82f, 0.78f);       // Pale green-white
        moodProfiles["clinical_cold"] = new MoodProfile(corridorBg, corridorText, 24f, 0.35f, "hospital");
        moodProfiles["clinical_dread"] = new MoodProfile(
            new Color(0.07f, 0.09f, 0.07f), corridorText, 24f, 0.4f, "hospital");
        moodProfiles["clinical_bleeding"] = new MoodProfile(
            new Color(0.09f, 0.08f, 0.07f), new Color(0.80f, 0.75f, 0.72f), 24f, 0.45f, "hospital");
        moodProfiles["clinical_fracture"] = new MoodProfile(
            new Color(0.06f, 0.08f, 0.06f), new Color(0.85f, 0.85f, 0.80f), 25f, 0.5f, "hospital");
        moodProfiles["clinical_impossible"] = new MoodProfile(
            new Color(0.05f, 0.07f, 0.06f), new Color(0.90f, 0.88f, 0.82f), 25f, 0.55f, "hospital");
        moodProfiles["clinical_pressure"] = new MoodProfile(
            new Color(0.06f, 0.07f, 0.06f), corridorText, 24f, 0.5f, "hospital");
        moodProfiles["clinical_warm_wrong"] = new MoodProfile(
            new Color(0.12f, 0.09f, 0.07f), warmText, 24f, 0.45f, "warm");
        moodProfiles["clinical_harsh"] = new MoodProfile(
            new Color(0.03f, 0.03f, 0.03f), new Color(0.95f, 0.95f, 0.95f), 24f, 0.3f, "hospital");
        moodProfiles["clinical_glitch"] = new MoodProfile(
            new Color(0.06f, 0.08f, 0.05f), new Color(0.88f, 0.82f, 0.78f), 25f, 0.55f, "static");
        moodProfiles["clinical_dissolve"] = new MoodProfile(
            new Color(0.08f, 0.08f, 0.08f), new Color(0.75f, 0.75f, 0.75f), 24f, 0.35f, "hospital");
        moodProfiles["clinical_threshold"] = new MoodProfile(
            new Color(0.07f, 0.07f, 0.07f), new Color(0.80f, 0.80f, 0.80f), 24f, 0.4f, "hospital");

        // === FRACTURED / THE ROOM ===
        Color fracturedBg = new Color(0.08f, 0.07f, 0.09f);        // Dark violet-grey
        Color fracturedText = new Color(0.82f, 0.78f, 0.85f);      // Pale lavender
        moodProfiles["fractured"] = new MoodProfile(fracturedBg, fracturedText, 24f, 0.45f, "static");
        moodProfiles["fractured_reveal"] = new MoodProfile(
            new Color(0.09f, 0.07f, 0.10f), fracturedText, 24f, 0.5f, "static");
        moodProfiles["fractured_clinical"] = new MoodProfile(
            new Color(0.10f, 0.09f, 0.11f), new Color(0.78f, 0.78f, 0.82f), 24f, 0.45f, "static");
        moodProfiles["fractured_truth"] = new MoodProfile(
            new Color(0.07f, 0.06f, 0.09f), new Color(0.88f, 0.82f, 0.90f), 25f, 0.55f, "static");
        moodProfiles["fractured_exposed"] = new MoodProfile(
            new Color(0.08f, 0.06f, 0.10f), fracturedText, 25f, 0.5f, "static");
        moodProfiles["fractured_core"] = new MoodProfile(
            new Color(0.06f, 0.05f, 0.08f), new Color(0.90f, 0.85f, 0.92f), 25f, 0.55f, "heartbeat");
        moodProfiles["fractured_warm"] = new MoodProfile(
            new Color(0.12f, 0.08f, 0.08f), new Color(0.90f, 0.82f, 0.78f), 24f, 0.5f, "warm");
        moodProfiles["fractured_cold"] = new MoodProfile(
            new Color(0.04f, 0.04f, 0.06f), new Color(0.70f, 0.72f, 0.78f), 24f, 0.3f, "static");
        moodProfiles["fractured_unstable"] = new MoodProfile(
            new Color(0.08f, 0.06f, 0.10f), fracturedText, 24f, 0.5f, "static");
        moodProfiles["fractured_fading"] = new MoodProfile(
            new Color(0.07f, 0.07f, 0.08f), new Color(0.72f, 0.70f, 0.75f), 24f, 0.35f, "static");
        moodProfiles["fractured_dissolve"] = new MoodProfile(
            new Color(0.09f, 0.09f, 0.10f), new Color(0.68f, 0.68f, 0.72f), 24f, 0.25f, "clinical");

        // === TRANSITIONS ===
        moodProfiles["transition"] = new MoodProfile(clinicalBg, clinicalText, 24f, 0.2f, "clinical");
        moodProfiles["transition_to_warm"] = new MoodProfile(
            new Color(0.14f, 0.11f, 0.08f), new Color(0.88f, 0.80f, 0.68f), 24f, 0.3f, "warm");
        moodProfiles["transition_to_cool"] = new MoodProfile(
            new Color(0.10f, 0.10f, 0.11f), new Color(0.75f, 0.72f, 0.68f), 24f, 0.2f, "autumn");
        moodProfiles["transition_to_cold"] = new MoodProfile(
            new Color(0.08f, 0.09f, 0.08f), new Color(0.72f, 0.75f, 0.72f), 24f, 0.15f, "hospital");
        moodProfiles["transition_to_fractured"] = new MoodProfile(
            new Color(0.07f, 0.06f, 0.09f), new Color(0.78f, 0.75f, 0.82f), 24f, 0.3f, "static");
        moodProfiles["transition_to_clinical"] = new MoodProfile(clinicalBg, clinicalText, 24f, 0.2f, "clinical");

        // === ENDINGS ===
        moodProfiles["ending_return"] = new MoodProfile(
            new Color(0.13f, 0.12f, 0.11f), new Color(0.85f, 0.80f, 0.72f), 24f, 0.35f, "warm");
        moodProfiles["ending_return_atmosphere"] = moodProfiles["ending_return"];
        moodProfiles["ending_sever"] = new MoodProfile(
            new Color(0.06f, 0.06f, 0.07f), new Color(0.72f, 0.72f, 0.75f), 24f, 0.1f, "clinical");
        moodProfiles["ending_sever_atmosphere"] = moodProfiles["ending_sever"];
        moodProfiles["ending_ambiguous"] = new MoodProfile(
            new Color(0.10f, 0.10f, 0.10f), new Color(0.75f, 0.75f, 0.75f), 24f, 0.2f, "clinical");
        moodProfiles["ending_ambiguous_atmosphere"] = moodProfiles["ending_ambiguous"];

        // === CONDITION B SPECIFIC (same key but stronger shifts) ===
        moodProfiles["cool_blue"] = new MoodProfile(
            new Color(0.08f, 0.10f, 0.15f), new Color(0.72f, 0.78f, 0.88f), 24f, 0.3f, "clinical");
        moodProfiles["warm_amber"] = new MoodProfile(
            new Color(0.16f, 0.12f, 0.06f), new Color(0.92f, 0.82f, 0.65f), 24f, 0.35f, "warm");
        moodProfiles["flat_grey"] = new MoodProfile(
            new Color(0.10f, 0.10f, 0.10f), new Color(0.62f, 0.62f, 0.62f), 24f, 0.2f, "clinical");
        moodProfiles["clinical_cold_sever"] = new MoodProfile(
            new Color(0.05f, 0.05f, 0.06f), new Color(0.68f, 0.70f, 0.72f), 24f, 0.1f, "clinical");
        moodProfiles["clinical_warm_return"] = new MoodProfile(
            new Color(0.14f, 0.12f, 0.10f), new Color(0.88f, 0.82f, 0.75f), 24f, 0.4f, "warm");
        moodProfiles["clinical_neutral_take"] = new MoodProfile(
            new Color(0.10f, 0.10f, 0.11f), new Color(0.75f, 0.75f, 0.78f), 24f, 0.25f, "clinical");
    }

    private void ProcessTag(string tag)
    {
        // Only process mood tags
        if (!tag.StartsWith("mood:") && !tag.StartsWith("scene:"))
            return;

        if (tag.StartsWith("scene:"))
        {
            // Scene tags are informational, no visual change
            return;
        }

        string mood = tag.Substring(5); // Remove "mood:" prefix

        // In Condition A, only apply scene-level transitions (not choice-dependent moods)
        if (currentCondition == "A")
        {
            if (mood.StartsWith("transition") || mood == "clinical")
                ApplyMoodSmooth(mood);
            return;
        }

        // Condition B: apply all mood changes
        ApplyMoodSmooth(mood);
    }

    private void ApplyMoodSmooth(string mood)
    {
        if (!moodProfiles.ContainsKey(mood))
        {
            Debug.LogWarning($"AtmosphericController: Unknown mood '{mood}'");
            return;
        }

        currentMood = mood;
        MoodProfile profile = moodProfiles[mood];

        StopAllCoroutines();
        StartCoroutine(TransitionToMood(profile));
    }

    private void ApplyMoodInstant(string mood)
    {
        if (!moodProfiles.ContainsKey(mood)) return;

        currentMood = mood;
        MoodProfile profile = moodProfiles[mood];

        if (backgroundPanel != null)
            backgroundPanel.color = profile.backgroundColor;
        if (mainCamera != null)
            mainCamera.backgroundColor = profile.backgroundColor;
        if (narrativeText != null)
        {
            narrativeText.color = profile.textColor;
            narrativeText.fontSize = profile.fontSize;
        }
    }

    private IEnumerator TransitionToMood(MoodProfile target)
    {
        Color startBg = backgroundPanel != null ? backgroundPanel.color : Color.black;
        Color startText = narrativeText != null ? narrativeText.color : Color.white;
        float startSize = narrativeText != null ? narrativeText.fontSize : 24f;

        // Audio transition
        AudioClip targetClip = GetAmbientClip(target.ambientKey);
        if (ambientSource != null && targetClip != null && ambientSource.clip != targetClip)
        {
            StartCoroutine(CrossfadeAudio(targetClip, target.audioVolume));
        }
        else if (ambientSource != null)
        {
            ambientSource.volume = target.audioVolume;
        }

        float elapsed = 0f;
        float duration = 1f / colorTransitionSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (backgroundPanel != null)
                backgroundPanel.color = Color.Lerp(startBg, target.backgroundColor, t);
            if (mainCamera != null)
                mainCamera.backgroundColor = Color.Lerp(startBg, target.backgroundColor, t);
            if (narrativeText != null)
            {
                narrativeText.color = Color.Lerp(startText, target.textColor, t);
                narrativeText.fontSize = Mathf.Lerp(startSize, target.fontSize, t);
            }

            yield return null;
        }

        // Ensure final values
        if (backgroundPanel != null)
            backgroundPanel.color = target.backgroundColor;
        if (mainCamera != null)
            mainCamera.backgroundColor = target.backgroundColor;
        if (narrativeText != null)
        {
            narrativeText.color = target.textColor;
            narrativeText.fontSize = target.fontSize;
        }
    }

    private IEnumerator CrossfadeAudio(AudioClip newClip, float targetVolume)
    {
        float startVolume = ambientSource.volume;
        float elapsed = 0f;
        float duration = 1f / audioTransitionSpeed;

        // Fade out
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration * 0.5f));
            yield return null;
        }

        // Switch clip
        ambientSource.clip = newClip;
        ambientSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration * 0.5f));
            yield return null;
        }

        ambientSource.volume = targetVolume;
    }

    private AudioClip GetAmbientClip(string key)
    {
        return key switch
        {
            "clinical" => ambientClinical,
            "warm" => ambientWarm,
            "autumn" => ambientAutumn,
            "hospital" => ambientHospital,
            "static" => ambientStatic,
            "heartbeat" => ambientHeartbeat,
            _ => null,
        };
    }

    private void OnDestroy()
    {
        if (narrativeManager != null)
            narrativeManager.OnTagReceived -= ProcessTag;
    }
}
