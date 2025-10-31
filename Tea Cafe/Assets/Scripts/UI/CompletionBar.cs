using UnityEngine;
using UnityEngine.UI;

public class CompletionBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider slider;

    [Header("Source")]
    [Tooltip("If left empty, search in parents for IHasProgress.")]
    [SerializeField] private MonoBehaviour source;  // any component that implements IHasProgress

    private IHasProgress hasProgress;

    private void Awake()
    {
        // 'true' in GetComponent parameter means to also search for active.false gameObjects
        if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
        if (slider == null) slider = GetComponentInChildren<Slider>(true);

        // Bind source
        hasProgress = source as IHasProgress;
        if (hasProgress == null)
        {
            // Try to find in parent hierarchy
            hasProgress = GetComponentInParent<IHasProgress>();
        }

        if (hasProgress == null)
        {
            Debug.LogError($"{name}: No IHasProgress found for CompletionBar.");
            enabled = false;
            return;
        }

        // Initialize UI
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = hasProgress.Progress01;
        }
        if (canvas != null)
        {
            canvas.enabled = hasProgress.ShowProgress;
        }
    }

    private void OnEnable()
    {
        if (hasProgress != null)
        {
            hasProgress.OnProgressChanged += HandleProgressChanged;
            // Sync once in case we enabled after an update
            HandleProgressChanged(hasProgress.Progress01, hasProgress.ShowProgress);
        }
    }

    private void OnDisable()
    {
        if (hasProgress != null)
        {
            hasProgress.OnProgressChanged -= HandleProgressChanged;
        }
    }

    private void HandleProgressChanged(float progress01, bool show)
    {
        if (slider != null) slider.value = progress01;
        if (canvas != null) canvas.enabled = show;
    }
}
