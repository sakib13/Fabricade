using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NarrativeScroller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private float scrollSpeed = 300f;

    private RectTransform textRect;
    private RectTransform viewRect; // the visible area (parent of text)
    private float scrollOffset = 0f;
    private float viewHeight;

    /// <summary>
    /// When true, LateUpdate skips ForceMeshUpdate to avoid interfering with typewriter.
    /// </summary>
    [HideInInspector] public bool pauseMeshUpdate = false;

    private void Start()
    {
        if (narrativeText == null) return;

        textRect = narrativeText.GetComponent<RectTransform>();
        viewRect = textRect.parent.GetComponent<RectTransform>();

        viewHeight = viewRect.rect.height;

        // Switch to top-anchored
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        // Width stretches with parent, height set manually
        textRect.offsetMin = new Vector2(textRect.offsetMin.x, 0);
        textRect.offsetMax = new Vector2(textRect.offsetMax.x, 0);
        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, viewHeight);

        // Overflow so TMP doesn't hide text beyond the rect
        narrativeText.overflowMode = TextOverflowModes.Overflow;
    }

    private void LateUpdate()
    {
        if (textRect == null || viewRect == null) return;

        // Keep text rect height matched to content
        if (!pauseMeshUpdate)
            narrativeText.ForceMeshUpdate();
        float textHeight = narrativeText.preferredHeight;
        viewHeight = viewRect.rect.height;

        // Only resize if content is taller than view
        if (textHeight > viewHeight)
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, textHeight);
        else
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, viewHeight);

        // Read scroll input
        if (Mouse.current == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.1f) return;

        float maxScroll = Mathf.Max(0, textHeight - viewHeight);
        scrollOffset -= scroll * scrollSpeed;
        scrollOffset = Mathf.Clamp(scrollOffset, 0, maxScroll);

        Vector2 pos = textRect.anchoredPosition;
        pos.y = scrollOffset;
        textRect.anchoredPosition = pos;
    }

    public void ScrollToBottom()
    {
        if (textRect == null || viewRect == null) return;

        narrativeText.ForceMeshUpdate();
        float textHeight = narrativeText.preferredHeight;
        viewHeight = viewRect.rect.height;

        scrollOffset = Mathf.Max(0, textHeight - viewHeight);
        Vector2 pos = textRect.anchoredPosition;
        pos.y = scrollOffset;
        textRect.anchoredPosition = pos;
    }

    public void ResetScroll()
    {
        scrollOffset = 0f;
        if (textRect != null)
            textRect.anchoredPosition = Vector2.zero;
    }
}
