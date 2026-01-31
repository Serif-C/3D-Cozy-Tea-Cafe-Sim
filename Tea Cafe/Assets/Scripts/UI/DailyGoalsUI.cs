using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DailyGoalsUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text hudText;

    [Header("Summary")]
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private Text summaryText;
    [SerializeField] private float summaryAutoHideSeconds = 0f;

    private Action onSummaryClosed;
    private Coroutine autoHideRoutine;

    private void Awake()
    {
        if (hudText == null || summaryPanel == null || summaryText == null)
        {
            BuildDefaultUI();
        }

        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (summaryPanel != null && summaryPanel.activeSelf && Input.anyKeyDown)
        {
            HideSummary();
        }
    }

    public static DailyGoalsUI CreateDefaultUI()
    {
        GameObject root = new GameObject("DailyGoalsCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        DailyGoalsUI ui = root.AddComponent<DailyGoalsUI>();
        ui.BuildDefaultUI();
        return ui;
    }

    public void SetHudText(string text)
    {
        if (hudText == null)
        {
            return;
        }

        hudText.text = text;
    }

    public void ShowSummary(string text, Action onClosed = null)
    {
        if (summaryPanel == null || summaryText == null)
        {
            return;
        }

        summaryText.text = text;
        summaryPanel.SetActive(true);
        onSummaryClosed = onClosed;

        if (autoHideRoutine != null)
        {
            StopCoroutine(autoHideRoutine);
            autoHideRoutine = null;
        }

        if (summaryAutoHideSeconds > 0f)
        {
            autoHideRoutine = StartCoroutine(AutoHideSummary(summaryAutoHideSeconds));
        }
    }

    private IEnumerator AutoHideSummary(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        HideSummary();
    }

    private void HideSummary()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }

        onSummaryClosed?.Invoke();
        onSummaryClosed = null;
    }

    private void BuildDefaultUI()
    {
        Font builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject hudPanel = new GameObject("DailyGoalsHUD", typeof(RectTransform), typeof(Image));
        hudPanel.transform.SetParent(transform, false);

        Image hudImage = hudPanel.GetComponent<Image>();
        hudImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 1f);
        hudRect.anchorMax = new Vector2(0.5f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = new Vector2(0f, -20f);
        hudRect.sizeDelta = new Vector2(420f, 210f);

        GameObject hudTextObject = new GameObject("HUDText", typeof(RectTransform), typeof(Text));
        hudTextObject.transform.SetParent(hudPanel.transform, false);
        hudText = hudTextObject.GetComponent<Text>();
        hudText.font = builtinFont;
        hudText.color = Color.white;
        hudText.alignment = TextAnchor.UpperLeft;
        hudText.horizontalOverflow = HorizontalWrapMode.Wrap;
        hudText.verticalOverflow = VerticalWrapMode.Overflow;
        hudText.fontSize = 16;

        RectTransform hudTextRect = hudTextObject.GetComponent<RectTransform>();
        hudTextRect.anchorMin = new Vector2(0f, 0f);
        hudTextRect.anchorMax = new Vector2(1f, 1f);
        hudTextRect.offsetMin = new Vector2(16f, 16f);
        hudTextRect.offsetMax = new Vector2(-16f, -16f);

        summaryPanel = new GameObject("DailyGoalsSummary", typeof(RectTransform), typeof(Image));
        summaryPanel.transform.SetParent(transform, false);

        Image summaryImage = summaryPanel.GetComponent<Image>();
        summaryImage.color = new Color(0f, 0f, 0f, 0.85f);

        RectTransform summaryRect = summaryPanel.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0.5f, 0.5f);
        summaryRect.anchorMax = new Vector2(0.5f, 0.5f);
        summaryRect.pivot = new Vector2(0.5f, 0.5f);
        summaryRect.anchoredPosition = Vector2.zero;
        summaryRect.sizeDelta = new Vector2(520f, 360f);

        GameObject summaryTextObject = new GameObject("SummaryText", typeof(RectTransform), typeof(Text));
        summaryTextObject.transform.SetParent(summaryPanel.transform, false);
        summaryText = summaryTextObject.GetComponent<Text>();
        summaryText.font = builtinFont;
        summaryText.color = Color.white;
        summaryText.alignment = TextAnchor.UpperLeft;
        summaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
        summaryText.verticalOverflow = VerticalWrapMode.Overflow;
        summaryText.fontSize = 18;

        RectTransform summaryTextRect = summaryTextObject.GetComponent<RectTransform>();
        summaryTextRect.anchorMin = new Vector2(0f, 0f);
        summaryTextRect.anchorMax = new Vector2(1f, 1f);
        summaryTextRect.offsetMin = new Vector2(20f, 20f);
        summaryTextRect.offsetMax = new Vector2(-20f, -20f);
    }
}
