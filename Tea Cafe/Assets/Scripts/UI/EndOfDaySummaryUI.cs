using TMPro;
using UnityEngine;

public class EndOfDaySummaryUI : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private GameObject shopUI;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text servedText;
    [SerializeField] private TMP_Text lostText;
    [SerializeField] private TMP_Text moodText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text goalsText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text reputationText;

    private void Awake()
    {
        root.SetActive(false);
    }

    public void Show(EndOfDayReport report)
    {
        root.SetActive(true);

        servedText.text = $"Customers Served: {report.customersServed}";
        lostText.text = $"Customers Lost: {report.customersLost}";
        moodText.text = $"Avg Mood: {report.averageMood:0.0}";
        moneyText.text = $"Money Earned: ${report.moneyEarned}";
        goalsText.text = $"Daily Goals: {report.goalCompletion01:P0}";
        xpText.text = $"Cafe XP Gained: +{report.xpGained}";
        reputationText.text = $"Reputation Change: {report.reputationDelta:+#;-#;0}";
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    // --- Buttons ---

    public void OnNextDayPressed()
    {
        Time.timeScale = 1f;

        Hide();

        // Day start logic goes here later
        FindFirstObjectByType<TimeManager>()?.StartNextDay();
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        gameObject.SetActive(false);
    }
}
