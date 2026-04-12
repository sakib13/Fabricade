using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SceneSetup : MonoBehaviour
{
    [MenuItem("Fabricade/Setup Scene")]
    public static void SetupGameScene()
    {
        // Delete everything in the scene
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            DestroyImmediate(obj);
        }

        // CAMERA
        GameObject camObj = new GameObject("Main Camera");
        Camera mainCam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        camObj.AddComponent<AudioListener>();


        // EVENT SYSTEM
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // CANVAS
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // LOAD FONT
        TMP_FontAsset customFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/Share_Tech_Mono/ShareTechMono-Regular SDF.asset");
        if (customFont == null)
            Debug.LogWarning("Fabricade: Custom font not found at Assets/Fonts/Share_Tech_Mono/ShareTechMono-Regular SDF.asset");

        // BACKGROUND — full screen dark panel
        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.14f);
        Stretch(bgObj);

        // TEXT AREA — clips text at its edges using RectMask2D
        GameObject textAreaObj = new GameObject("TextArea", typeof(RectTransform));
        textAreaObj.transform.SetParent(bgObj.transform, false);
        RectTransform textAreaRect = textAreaObj.GetComponent<RectTransform>();
        textAreaRect.anchorMin = new Vector2(0, 0.18f);
        textAreaRect.anchorMax = new Vector2(1, 0.98f);
        textAreaRect.offsetMin = new Vector2(180, 0);
        textAreaRect.offsetMax = new Vector2(-180, 0);
        textAreaObj.AddComponent<RectMask2D>();

        // NARRATIVE TEXT — inside TextArea, stretched to fill
        GameObject textObj = new GameObject("NarrativeText", typeof(RectTransform));
        textObj.transform.SetParent(textAreaObj.transform, false);
        TextMeshProUGUI narText = textObj.AddComponent<TextMeshProUGUI>();
        narText.text = "";
        if (customFont != null) narText.font = customFont;
        narText.fontSize = 18;
        narText.color = new Color(0.82f, 0.84f, 0.88f);
        narText.lineSpacing = 12f;
        narText.textWrappingMode = TextWrappingModes.Normal;
        narText.overflowMode = TextOverflowModes.Overflow;
        narText.richText = true;
        narText.alignment = TextAlignmentOptions.TopLeft;
        Stretch(textObj);

        // FADE OVERLAY — gradient that fades text above the choice area
        GameObject fadeObj = new GameObject("BottomFade", typeof(RectTransform));
        fadeObj.transform.SetParent(bgObj.transform, false);
        RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
        fadeRect.anchorMin = new Vector2(0, 0.17f);
        fadeRect.anchorMax = new Vector2(1, 0.30f);
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        RawImage fadeImg = fadeObj.AddComponent<RawImage>();
        // Create a small gradient texture: transparent at top, background color at bottom
        Texture2D fadeTex = new Texture2D(1, 32);
        Color bgColor = new Color(0.12f, 0.12f, 0.14f);
        for (int i = 0; i < 32; i++)
        {
            float alpha = 1f - (i / 31f); // bottom=1 (opaque), top=0 (transparent)
            fadeTex.SetPixel(0, i, new Color(bgColor.r, bgColor.g, bgColor.b, alpha));
        }
        fadeTex.Apply();
        fadeImg.texture = fadeTex;
        fadeImg.raycastTarget = false;
        // Force fade to render above the TMP text
        Canvas fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.overrideSorting = true;
        fadeCanvas.sortingOrder = 1;

        // VIGNETTE OVERLAY — dark edge layer; texture generated at runtime by AtmosphericController.
        // Condition B only. Placed before ChoiceContainer so choices render above it.
        GameObject vignetteObj = new GameObject("VignetteOverlay", typeof(RectTransform));
        vignetteObj.transform.SetParent(bgObj.transform, false);
        RawImage vignetteImg = vignetteObj.AddComponent<RawImage>();
        vignetteImg.color = new Color(1f, 1f, 1f, 0f);
        vignetteImg.raycastTarget = false;
        Stretch(vignetteObj);

        // GLOW OVERLAY — warm centre layer rendered above the vignette.
        // Creates contrast with the dark edges so both effects are visible on a dark background.
        GameObject glowObj = new GameObject("GlowOverlay", typeof(RectTransform));
        glowObj.transform.SetParent(bgObj.transform, false);
        RawImage glowImg = glowObj.AddComponent<RawImage>();
        glowImg.color = new Color(1f, 1f, 1f, 0f);
        glowImg.raycastTarget = false;
        Stretch(glowObj);

        // GLITCH OVERLAY — full-screen, rendered above everything, invisible at rest
        // Used by GlitchController to flash a colour tint during screen glitch.
        GameObject glitchOverlayObj = new GameObject("GlitchOverlay", typeof(RectTransform));
        glitchOverlayObj.transform.SetParent(canvasObj.transform, false);
        Image glitchOverlayImg = glitchOverlayObj.AddComponent<Image>();
        glitchOverlayImg.color = new Color(0.38f, 0.42f, 0.90f, 0f); // blue-purple, alpha 0
        glitchOverlayImg.raycastTarget = false;
        Stretch(glitchOverlayObj);
        Canvas glitchOverlayCanvas = glitchOverlayObj.AddComponent<Canvas>();
        glitchOverlayCanvas.overrideSorting = true;
        glitchOverlayCanvas.sortingOrder = 100; // always on top

        // FADE OVERLAY — black full-screen panel for scene transitions (topmost)
        GameObject fadeOverlayObj = new GameObject("FadeOverlay", typeof(RectTransform));
        fadeOverlayObj.transform.SetParent(canvasObj.transform, false);
        Image fadeOverlayImg = fadeOverlayObj.AddComponent<Image>();
        fadeOverlayImg.color = new Color(0f, 0f, 0f, 0f); // black, alpha 0 at rest
        fadeOverlayImg.raycastTarget = false;
        Stretch(fadeOverlayObj);
        Canvas fadeOverlayCanvas = fadeOverlayObj.AddComponent<Canvas>();
        fadeOverlayCanvas.overrideSorting = true;
        fadeOverlayCanvas.sortingOrder = 200; // above glitch overlay (100)

        // CHOICE CONTAINER — bottom area
        GameObject choiceObj = new GameObject("ChoiceContainer", typeof(RectTransform));
        choiceObj.transform.SetParent(bgObj.transform, false);
        RectTransform choiceRect = choiceObj.GetComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.15f, 0.02f);
        choiceRect.anchorMax = new Vector2(0.85f, 0.17f);
        choiceRect.offsetMin = Vector2.zero;
        choiceRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup choiceLayout = choiceObj.AddComponent<VerticalLayoutGroup>();
        choiceLayout.spacing = 8;
        choiceLayout.padding = new RectOffset(10, 10, 5, 5);
        choiceLayout.childAlignment = TextAnchor.MiddleCenter;
        choiceLayout.childControlWidth = true;
        choiceLayout.childControlHeight = true;
        choiceLayout.childForceExpandWidth = true;
        choiceLayout.childForceExpandHeight = false;

        // CHOICE BUTTON PREFAB — hidden template
        GameObject btnPrefab = new GameObject("ChoiceButtonPrefab", typeof(RectTransform));
        btnPrefab.transform.SetParent(canvasObj.transform, false);
        Image btnImg = btnPrefab.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.18f, 0.22f);
        Button btnComp = btnPrefab.AddComponent<Button>();
        ColorBlock btnColors = btnComp.colors;
        btnColors.normalColor = new Color(0.18f, 0.18f, 0.22f);
        btnColors.highlightedColor = new Color(0.25f, 0.25f, 0.32f);
        btnColors.pressedColor = new Color(0.15f, 0.15f, 0.20f);
        btnComp.colors = btnColors;
        LayoutElement btnLE = btnPrefab.AddComponent<LayoutElement>();
        btnLE.minHeight = 48;
        btnLE.preferredHeight = 52;

        GameObject btnTextObj = new GameObject("Text", typeof(RectTransform));
        btnTextObj.transform.SetParent(btnPrefab.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Choice";
        if (customFont != null) btnText.font = customFont;
        btnText.fontSize = 20;
        btnText.color = new Color(0.54f, 0.67f, 0.72f);
        btnText.alignment = TextAlignmentOptions.Left;
        btnText.margin = new Vector4(15, 8, 15, 8);
        Stretch(btnTextObj);

        btnPrefab.SetActive(false);

        // START SCREEN
        GameObject startObj = new GameObject("StartScreen", typeof(RectTransform));
        startObj.transform.SetParent(canvasObj.transform, false);
        Image startImg = startObj.AddComponent<Image>();
        startImg.color = new Color(0.08f, 0.08f, 0.10f);
        Stretch(startObj);

        // Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(startObj.transform, false);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "FABRICADE";
        if (customFont != null) titleTxt.font = customFont;
        titleTxt.fontSize = 56;
        titleTxt.color = new Color(0.75f, 0.78f, 0.82f);
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.fontStyle = FontStyles.SmallCaps;
        RectTransform titleR = titleObj.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0.2f, 0.55f);
        titleR.anchorMax = new Vector2(0.8f, 0.75f);
        titleR.offsetMin = Vector2.zero;
        titleR.offsetMax = Vector2.zero;

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
        subObj.transform.SetParent(startObj.transform, false);
        TextMeshProUGUI subTxt = subObj.AddComponent<TextMeshProUGUI>();
        subTxt.text = "Emotional Landscape and Retrieval Architecture";
        if (customFont != null) subTxt.font = customFont;
        subTxt.fontSize = 20;
        subTxt.color = new Color(0.50f, 0.52f, 0.56f);
        subTxt.alignment = TextAlignmentOptions.Center;
        RectTransform subR = subObj.GetComponent<RectTransform>();
        subR.anchorMin = new Vector2(0.2f, 0.48f);
        subR.anchorMax = new Vector2(0.8f, 0.55f);
        subR.offsetMin = Vector2.zero;
        subR.offsetMax = Vector2.zero;

        // Start buttons
        GameObject saBtnObj = MakeButton("StartButtonA", startObj.transform,
            "Begin Session \u2014 Condition A", new Vector2(0.3f, 0.30f), new Vector2(0.7f, 0.38f), customFont);
        GameObject sbBtnObj = MakeButton("StartButtonB", startObj.transform,
            "Begin Session \u2014 Condition B", new Vector2(0.3f, 0.20f), new Vector2(0.7f, 0.28f), customFont);

        // END SCREEN
        GameObject endObj = new GameObject("EndScreen", typeof(RectTransform));
        endObj.transform.SetParent(canvasObj.transform, false);
        Image endImg = endObj.AddComponent<Image>();
        endImg.color = new Color(0.08f, 0.08f, 0.10f, 1f);
        Stretch(endObj);

        GameObject endTxtObj = new GameObject("EndText", typeof(RectTransform));
        endTxtObj.transform.SetParent(endObj.transform, false);
        TextMeshProUGUI endTxt = endTxtObj.AddComponent<TextMeshProUGUI>();
        endTxt.text = "Session Complete";
        if (customFont != null) endTxt.font = customFont;
        endTxt.fontSize = 36;
        endTxt.color = new Color(0.75f, 0.78f, 0.82f);
        endTxt.alignment = TextAlignmentOptions.Center;
        RectTransform endR = endTxtObj.GetComponent<RectTransform>();
        endR.anchorMin = new Vector2(0.2f, 0.4f);
        endR.anchorMax = new Vector2(0.8f, 0.6f);
        endR.offsetMin = Vector2.zero;
        endR.offsetMax = Vector2.zero;

        endObj.SetActive(false);

        // GAME MANAGER
        GameObject gmObj = new GameObject("GameManager");

        // Find Ink JSON
        TextAsset inkJSON = null;
        string[] guids = AssetDatabase.FindAssets("main t:TextAsset", new[] { "Assets/Ink" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".json"))
            {
                inkJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                break;
            }
        }

        // NarrativeManager
        NarrativeManager narMgr = gmObj.AddComponent<NarrativeManager>();
        var narSO = new SerializedObject(narMgr);
        narSO.FindProperty("inkJSON").objectReferenceValue = inkJSON;
        narSO.ApplyModifiedProperties();

        // AtmosphericController
        AtmosphericController atmCtrl = gmObj.AddComponent<AtmosphericController>();
        var atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        atmSO.FindProperty("mainCamera").objectReferenceValue = mainCam;
        atmSO.FindProperty("backgroundPanel").objectReferenceValue = bgImg;
        atmSO.FindProperty("narrativeText").objectReferenceValue = narText;
        atmSO.ApplyModifiedProperties();

        // Audio
        AudioSource ambSrc = gmObj.AddComponent<AudioSource>();
        ambSrc.loop = true;
        ambSrc.playOnAwake = false;
        ambSrc.volume = 0.3f;
        AudioSource sfxSrc = gmObj.AddComponent<AudioSource>();
        sfxSrc.loop = false;
        sfxSrc.playOnAwake = false;

        atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("ambientSource").objectReferenceValue = ambSrc;
        atmSO.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        atmSO.ApplyModifiedProperties();

        // Load and assign audio clips from Assets/Audio/
        AudioClip clipClinical  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientClinical.wav");
        AudioClip clipWarm      = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientWarm.wav");
        AudioClip clipAutumn    = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientAutumn.wav");
        AudioClip clipHospital  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientHospital.wav");
        AudioClip clipStatic    = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientStatic.wav");
        AudioClip clipHeartbeat = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambientHeartBeat.wav");

        if (clipClinical  == null) Debug.LogWarning("Fabricade: ambientClinical.wav not found in Assets/Audio/");
        if (clipWarm      == null) Debug.LogWarning("Fabricade: ambientWarm.wav not found in Assets/Audio/");
        if (clipAutumn    == null) Debug.LogWarning("Fabricade: ambientAutumn.wav not found in Assets/Audio/");
        if (clipHospital  == null) Debug.LogWarning("Fabricade: ambientHospital.wav not found in Assets/Audio/");
        if (clipStatic    == null) Debug.LogWarning("Fabricade: ambientStatic.wav not found in Assets/Audio/");
        if (clipHeartbeat == null) Debug.LogWarning("Fabricade: ambientHeartBeat.wav not found in Assets/Audio/");

        atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("ambientClinical").objectReferenceValue  = clipClinical;
        atmSO.FindProperty("ambientWarm").objectReferenceValue      = clipWarm;
        atmSO.FindProperty("ambientAutumn").objectReferenceValue    = clipAutumn;
        atmSO.FindProperty("ambientHospital").objectReferenceValue  = clipHospital;
        atmSO.FindProperty("ambientStatic").objectReferenceValue    = clipStatic;
        atmSO.FindProperty("ambientHeartbeat").objectReferenceValue = clipHeartbeat;
        atmSO.ApplyModifiedProperties();

        // Typing AudioSource — dedicated source for ELARA keystroke SFX
        AudioSource typingSrc = gmObj.AddComponent<AudioSource>();
        typingSrc.loop = false;
        typingSrc.playOnAwake = false;
        typingSrc.volume = 0.5f;

        // Find elaraType clip regardless of file extension
        AudioClip elaraClip = null;
        string[] elaraGuids = AssetDatabase.FindAssets("elaraType t:AudioClip", new[] { "Assets/Audio" });
        if (elaraGuids.Length > 0)
            elaraClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(elaraGuids[0]));
        if (elaraClip == null)
            Debug.LogWarning("Fabricade: elaraType audio clip not found in Assets/Audio/");

        // Wire vignette + glow to AtmosphericController
        atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("vignetteImage").objectReferenceValue = vignetteImg;
        atmSO.FindProperty("glowImage").objectReferenceValue     = glowImg;
        atmSO.ApplyModifiedProperties();

        // GlitchController
        GlitchController glitchCtrl = gmObj.AddComponent<GlitchController>();
        var glitchSO = new SerializedObject(glitchCtrl);
        glitchSO.FindProperty("mainCanvas").objectReferenceValue = canvas;
        glitchSO.FindProperty("screenFlashOverlay").objectReferenceValue = glitchOverlayImg;
        glitchSO.FindProperty("narrativeText").objectReferenceValue = narText;
        glitchSO.ApplyModifiedProperties();

        // Wire GlitchController into AtmosphericController
        atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("glitchController").objectReferenceValue = glitchCtrl;
        atmSO.ApplyModifiedProperties();

        // BehavioralLogger
        BehavioralLogger logger = gmObj.AddComponent<BehavioralLogger>();
        var logSO = new SerializedObject(logger);
        logSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        logSO.ApplyModifiedProperties();

        // NarrativeScroller
        NarrativeScroller scroller = gmObj.AddComponent<NarrativeScroller>();
        var scrollerSO = new SerializedObject(scroller);
        scrollerSO.FindProperty("narrativeText").objectReferenceValue = narText;
        scrollerSO.ApplyModifiedProperties();

        // UIManager
        UIManager uiMgr = gmObj.AddComponent<UIManager>();
        var uiSO = new SerializedObject(uiMgr);
        uiSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        uiSO.FindProperty("atmosphericController").objectReferenceValue = atmCtrl;
        uiSO.FindProperty("scroller").objectReferenceValue = scroller;
        uiSO.FindProperty("glitchController").objectReferenceValue = glitchCtrl;
        uiSO.FindProperty("fadeOverlay").objectReferenceValue = fadeOverlayImg;
        uiSO.FindProperty("typingAudioSource").objectReferenceValue = typingSrc;
        uiSO.FindProperty("elaraTypeClip").objectReferenceValue = elaraClip;
        uiSO.FindProperty("narrativeText").objectReferenceValue = narText;
        uiSO.FindProperty("choiceContainer").objectReferenceValue = choiceObj.transform;
        uiSO.FindProperty("choiceButtonPrefab").objectReferenceValue = btnComp;
        uiSO.FindProperty("startScreen").objectReferenceValue = startObj;
        uiSO.FindProperty("startButtonA").objectReferenceValue = saBtnObj.GetComponent<Button>();
        uiSO.FindProperty("startButtonB").objectReferenceValue = sbBtnObj.GetComponent<Button>();
        uiSO.FindProperty("endScreen").objectReferenceValue = endObj;
        uiSO.FindProperty("endText").objectReferenceValue = endTxt;
        uiSO.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Fabricade: Scene setup complete. Save with Ctrl+S.");
        if (inkJSON == null)
            Debug.LogWarning("Fabricade: Ink JSON not found!");
    }

    static GameObject MakeButton(string name, Transform parent, string text, Vector2 aMin, Vector2 aMax, TMP_FontAsset font = null)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.19f);
        Button btn = obj.AddComponent<Button>();
        ColorBlock c = btn.colors;
        c.normalColor = new Color(0.15f, 0.15f, 0.19f);
        c.highlightedColor = new Color(0.22f, 0.22f, 0.28f);
        c.pressedColor = new Color(0.12f, 0.12f, 0.16f);
        btn.colors = c;

        GameObject tObj = new GameObject("Text", typeof(RectTransform));
        tObj.transform.SetParent(obj.transform, false);
        TextMeshProUGUI t = tObj.AddComponent<TextMeshProUGUI>();
        t.text = text;
        if (font != null) t.font = font;
        t.fontSize = 22;
        t.color = new Color(0.54f, 0.67f, 0.72f);
        t.alignment = TextAlignmentOptions.Center;
        Stretch(tObj);

        return obj;
    }

    static void Stretch(GameObject obj)
    {
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
