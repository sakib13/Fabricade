using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Editor script that builds the entire Fabricade game scene.
/// Run via the menu: Fabricade > Setup Scene
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [MenuItem("Fabricade/Setup Scene")]
    public static void SetupGameScene()
    {
        // Clean up existing objects (except camera)
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<Camera>() != null) continue;
            if (obj.transform.parent != null) continue;
            if (obj.name == "EventSystem") continue;
            DestroyImmediate(obj);
        }

        // ==========================================
        // CAMERA
        // ==========================================
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;

        // ==========================================
        // EVENT SYSTEM
        // ==========================================
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ==========================================
        // CANVAS
        // ==========================================
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // ==========================================
        // BACKGROUND PANEL (full screen)
        // ==========================================
        GameObject bgPanel = CreateUIObject("BackgroundPanel", canvasObj.transform);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.12f, 0.14f);
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        StretchFull(bgRect);

        // ==========================================
        // NARRATIVE AREA (scrollable text)
        // ==========================================
        GameObject scrollArea = CreateUIObject("NarrativeScrollArea", bgPanel.transform);
        RectTransform scrollRect = scrollArea.GetComponent<RectTransform>();
        StretchFull(scrollRect);
        scrollRect.offsetMin = new Vector2(200, 180);   // left, bottom padding
        scrollRect.offsetMax = new Vector2(-200, -60);  // right, top padding

        ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        // Scroll mask
        Image scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0); // transparent
        scrollArea.AddComponent<Mask>().showMaskGraphic = false;

        // Content container
        GameObject contentObj = CreateUIObject("Content", scrollArea.transform);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(0, 0);
        contentRect.offsetMax = new Vector2(0, 0);

        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        scroll.content = contentRect;
        scroll.viewport = scrollRect;

        // Narrative text
        GameObject textObj = CreateUIObject("NarrativeText", contentObj.transform);
        TextMeshProUGUI narrativeText = textObj.AddComponent<TextMeshProUGUI>();
        narrativeText.text = "";
        narrativeText.fontSize = 24;
        narrativeText.color = new Color(0.75f, 0.78f, 0.82f);
        narrativeText.lineSpacing = 12f;
        narrativeText.paragraphSpacing = 16f;
        narrativeText.enableWordWrapping = true;
        narrativeText.overflowMode = TextOverflowModes.Overflow;
        narrativeText.richText = true;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // ==========================================
        // CHOICE CONTAINER (bottom area)
        // ==========================================
        GameObject choiceContainer = CreateUIObject("ChoiceContainer", bgPanel.transform);
        RectTransform choiceRect = choiceContainer.GetComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.15f, 0.02f);
        choiceRect.anchorMax = new Vector2(0.85f, 0.16f);
        choiceRect.offsetMin = Vector2.zero;
        choiceRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup choiceLayout = choiceContainer.AddComponent<VerticalLayoutGroup>();
        choiceLayout.spacing = 10;
        choiceLayout.padding = new RectOffset(10, 10, 5, 5);
        choiceLayout.childAlignment = TextAnchor.MiddleCenter;
        choiceLayout.childControlWidth = true;
        choiceLayout.childControlHeight = true;
        choiceLayout.childForceExpandWidth = true;
        choiceLayout.childForceExpandHeight = false;

        // ==========================================
        // CHOICE BUTTON PREFAB (template)
        // ==========================================
        GameObject choiceBtnObj = CreateUIObject("ChoiceButtonPrefab", canvasObj.transform);
        Image btnImage = choiceBtnObj.AddComponent<Image>();
        btnImage.color = new Color(0.18f, 0.18f, 0.22f);
        Button choiceBtn = choiceBtnObj.AddComponent<Button>();

        // Button hover colors
        ColorBlock colors = choiceBtn.colors;
        colors.normalColor = new Color(0.18f, 0.18f, 0.22f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.32f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.20f);
        colors.selectedColor = new Color(0.22f, 0.22f, 0.28f);
        choiceBtn.colors = colors;

        // Button text
        GameObject btnTextObj = CreateUIObject("Text", choiceBtnObj.transform);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Choice";
        btnText.fontSize = 20;
        btnText.color = new Color(0.54f, 0.67f, 0.72f);
        btnText.alignment = TextAlignmentOptions.Left;
        btnText.margin = new Vector4(15, 8, 15, 8);
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        StretchFull(btnTextRect);

        LayoutElement btnLayoutElem = choiceBtnObj.AddComponent<LayoutElement>();
        btnLayoutElem.minHeight = 50;
        btnLayoutElem.preferredHeight = 55;

        choiceBtnObj.SetActive(false); // Hidden by default, used as prefab

        // ==========================================
        // CONTINUE BUTTON
        // ==========================================
        GameObject continueObj = CreateUIObject("ContinueButton", bgPanel.transform);
        RectTransform continueRect = continueObj.GetComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(0.4f, 0.04f);
        continueRect.anchorMax = new Vector2(0.6f, 0.09f);
        continueRect.offsetMin = Vector2.zero;
        continueRect.offsetMax = Vector2.zero;

        Image continueImg = continueObj.AddComponent<Image>();
        continueImg.color = new Color(0.15f, 0.15f, 0.19f);
        Button continueBtn = continueObj.AddComponent<Button>();

        ColorBlock contColors = continueBtn.colors;
        contColors.normalColor = new Color(0.15f, 0.15f, 0.19f);
        contColors.highlightedColor = new Color(0.22f, 0.22f, 0.28f);
        contColors.pressedColor = new Color(0.12f, 0.12f, 0.16f);
        continueBtn.colors = contColors;

        GameObject contTextObj = CreateUIObject("Text", continueObj.transform);
        TextMeshProUGUI contText = contTextObj.AddComponent<TextMeshProUGUI>();
        contText.text = "Continue";
        contText.fontSize = 18;
        contText.color = new Color(0.54f, 0.67f, 0.72f);
        contText.alignment = TextAlignmentOptions.Center;
        RectTransform contTextRect = contTextObj.GetComponent<RectTransform>();
        StretchFull(contTextRect);

        // ==========================================
        // START SCREEN
        // ==========================================
        GameObject startScreen = CreateUIObject("StartScreen", canvasObj.transform);
        Image startBg = startScreen.AddComponent<Image>();
        startBg.color = new Color(0.08f, 0.08f, 0.10f);
        StretchFull(startScreen.GetComponent<RectTransform>());

        // Title
        GameObject titleObj = CreateUIObject("Title", startScreen.transform);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "FABRICADE";
        titleText.fontSize = 56;
        titleText.color = new Color(0.75f, 0.78f, 0.82f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.SmallCaps;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.2f, 0.55f);
        titleRect.anchorMax = new Vector2(0.8f, 0.75f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Subtitle
        GameObject subtitleObj = CreateUIObject("Subtitle", startScreen.transform);
        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "Emotional Landscape and Retrieval Architecture";
        subtitleText.fontSize = 20;
        subtitleText.color = new Color(0.50f, 0.52f, 0.56f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.2f, 0.48f);
        subtitleRect.anchorMax = new Vector2(0.8f, 0.55f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;

        // Start Button A
        GameObject startAObj = CreateButton("StartButtonA", startScreen.transform,
            "Begin Session — Condition A", new Vector2(0.3f, 0.30f), new Vector2(0.7f, 0.38f));
        Button startABtn = startAObj.GetComponent<Button>();

        // Start Button B
        GameObject startBObj = CreateButton("StartButtonB", startScreen.transform,
            "Begin Session — Condition B", new Vector2(0.3f, 0.20f), new Vector2(0.7f, 0.28f));
        Button startBBtn = startBObj.GetComponent<Button>();

        // ==========================================
        // END SCREEN
        // ==========================================
        GameObject endScreen = CreateUIObject("EndScreen", canvasObj.transform);
        Image endBg = endScreen.AddComponent<Image>();
        endBg.color = new Color(0.08f, 0.08f, 0.10f, 0.95f);
        StretchFull(endScreen.GetComponent<RectTransform>());

        GameObject endTextObj = CreateUIObject("EndText", endScreen.transform);
        TextMeshProUGUI endTextComp = endTextObj.AddComponent<TextMeshProUGUI>();
        endTextComp.text = "Session Complete";
        endTextComp.fontSize = 36;
        endTextComp.color = new Color(0.75f, 0.78f, 0.82f);
        endTextComp.alignment = TextAlignmentOptions.Center;
        RectTransform endTextRect = endTextObj.GetComponent<RectTransform>();
        endTextRect.anchorMin = new Vector2(0.2f, 0.4f);
        endTextRect.anchorMax = new Vector2(0.8f, 0.6f);
        endTextRect.offsetMin = Vector2.zero;
        endTextRect.offsetMax = Vector2.zero;

        endScreen.SetActive(false);

        // ==========================================
        // GAME MANAGER (empty object holding scripts)
        // ==========================================
        GameObject gameManager = new GameObject("GameManager");

        // Find the compiled Ink JSON
        TextAsset inkJSON = FindInkJSON();

        // NarrativeManager
        NarrativeManager narMgr = gameManager.AddComponent<NarrativeManager>();
        SerializedObject narSO = new SerializedObject(narMgr);
        narSO.FindProperty("inkJSON").objectReferenceValue = inkJSON;
        narSO.ApplyModifiedProperties();

        // AtmosphericController
        AtmosphericController atmCtrl = gameManager.AddComponent<AtmosphericController>();
        SerializedObject atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        atmSO.FindProperty("mainCamera").objectReferenceValue = mainCam;
        atmSO.FindProperty("backgroundPanel").objectReferenceValue = bgImage;
        atmSO.FindProperty("narrativeText").objectReferenceValue = narrativeText;
        atmSO.ApplyModifiedProperties();

        // BehavioralLogger
        BehavioralLogger logger = gameManager.AddComponent<BehavioralLogger>();
        SerializedObject logSO = new SerializedObject(logger);
        logSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        logSO.ApplyModifiedProperties();

        // UIManager
        UIManager uiMgr = gameManager.AddComponent<UIManager>();
        SerializedObject uiSO = new SerializedObject(uiMgr);
        uiSO.FindProperty("narrativeManager").objectReferenceValue = narMgr;
        uiSO.FindProperty("atmosphericController").objectReferenceValue = atmCtrl;
        uiSO.FindProperty("narrativeText").objectReferenceValue = narrativeText;
        uiSO.FindProperty("scrollRect").objectReferenceValue = scroll;
        uiSO.FindProperty("contentPanel").objectReferenceValue = contentRect;
        uiSO.FindProperty("choiceContainer").objectReferenceValue = choiceContainer.transform;
        uiSO.FindProperty("choiceButtonPrefab").objectReferenceValue = choiceBtn;
        uiSO.FindProperty("continueButton").objectReferenceValue = continueBtn;
        uiSO.FindProperty("startScreen").objectReferenceValue = startScreen;
        uiSO.FindProperty("startButtonA").objectReferenceValue = startABtn;
        uiSO.FindProperty("startButtonB").objectReferenceValue = startBBtn;
        uiSO.FindProperty("endScreen").objectReferenceValue = endScreen;
        uiSO.FindProperty("endText").objectReferenceValue = endTextComp;
        uiSO.ApplyModifiedProperties();

        // ==========================================
        // AUDIO SOURCES
        // ==========================================
        AudioSource ambientSrc = gameManager.AddComponent<AudioSource>();
        ambientSrc.loop = true;
        ambientSrc.playOnAwake = false;
        ambientSrc.volume = 0.3f;

        AudioSource sfxSrc = gameManager.AddComponent<AudioSource>();
        sfxSrc.loop = false;
        sfxSrc.playOnAwake = false;

        // Wire audio to atmospheric controller
        atmSO = new SerializedObject(atmCtrl);
        atmSO.FindProperty("ambientSource").objectReferenceValue = ambientSrc;
        atmSO.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        atmSO.ApplyModifiedProperties();

        // Mark scene as dirty so changes can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Fabricade: Scene setup complete. Save the scene with Ctrl+S.");

        if (inkJSON == null)
            Debug.LogWarning("Fabricade: Ink JSON not found. Make sure main.ink is compiled. Go to Ink > Recompile Story.");
    }

    private static TextAsset FindInkJSON()
    {
        // Look for compiled ink JSON file
        string[] guids = AssetDatabase.FindAssets("main t:TextAsset", new[] { "Assets/Ink" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".json"))
                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        // Try broader search
        guids = AssetDatabase.FindAssets("main t:TextAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Ink") && path.EndsWith(".json"))
                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        return null;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static GameObject CreateButton(string name, Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.19f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock btnColors = btn.colors;
        btnColors.normalColor = new Color(0.15f, 0.15f, 0.19f);
        btnColors.highlightedColor = new Color(0.22f, 0.22f, 0.28f);
        btnColors.pressedColor = new Color(0.12f, 0.12f, 0.16f);
        btn.colors = btnColors;

        GameObject txtObj = CreateUIObject("Text", btnObj.transform);
        TextMeshProUGUI tmpText = txtObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 22;
        tmpText.color = new Color(0.54f, 0.67f, 0.72f);
        tmpText.alignment = TextAlignmentOptions.Center;
        StretchFull(txtObj.GetComponent<RectTransform>());

        return btnObj;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
