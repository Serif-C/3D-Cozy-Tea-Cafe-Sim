using System.Collections;
using UnityEngine;

public class CustomerSocialBubble : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer bubbleRenderer;
    [SerializeField] private Sprite[] socialSprites;

    [Header("Timing")]
    [SerializeField] private float minInterval = 1.25f;
    [SerializeField] private float maxInterval = 3.5f;
    [SerializeField] private float showDuration = 0.9f;

    private float nextPulseTime;

    private void Awake()
    {
        if (bubbleRenderer != null)
            bubbleRenderer.enabled = false;
    }

    // Group socializing
    public void TryPulseEmoji() => TryPulse();

    // Solo “pass time” (book/phone)
    public void TryPulseThought() => TryPulse();

    private void TryPulse()
    {
        if (bubbleRenderer == null || socialSprites == null || socialSprites.Length == 0)
            return;

        if (Time.time < nextPulseTime)
            return;

        nextPulseTime = Time.time + Random.Range(minInterval, maxInterval);
        StartCoroutine(PulseOnce());
    }

    private IEnumerator PulseOnce()
    {
        bubbleRenderer.sprite = socialSprites[Random.Range(0, socialSprites.Length)];
        bubbleRenderer.enabled = true;
        yield return new WaitForSeconds(showDuration);
        bubbleRenderer.enabled = false;
        bubbleRenderer.sprite = null;
    }
}
