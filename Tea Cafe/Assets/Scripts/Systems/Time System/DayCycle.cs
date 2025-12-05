using UnityEngine;

public class DayCycle : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;

    [Header("Lights")]
    [SerializeField] private Light sunLight;

    [Header("Angles")]
    [Tooltip("Angle when the day starts (in degrees). E.g. -90 = horizon at sunrise")]
    [SerializeField] private float sunriseAngle = -90f;
    [Tooltip("Angle when the day ends. E.g. 270 = full circle over a day.")]
    [SerializeField] private float sunsetAngle = 270f;
    [SerializeField] private float yRotation;
    [SerializeField] private float zRotation;

    private void Reset()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void Update()
    {
        if (timeManager == null || sunLight == null) return;

        float t = timeManager.DayProgress01;
        float sunAngle = Mathf.Lerp(sunriseAngle, sunsetAngle, t);

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, yRotation, zRotation);

        // turn sun intensity up/down based on angle
        float sunDot = Mathf.Clamp01(Vector3.Dot(sunLight.transform.forward, Vector3.down));
        sunLight.intensity = Mathf.Lerp(0.1f, 1.2f, sunDot);
    }
}
