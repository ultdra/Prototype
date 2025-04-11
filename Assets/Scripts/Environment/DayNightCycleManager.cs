using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the day-night cycle, updates lighting, and exposes UnityEvents for transitions.
/// Attach to a GameObject in your scene and assign the main Directional Light.
/// </summary>
public class DayNightCycleManager : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Length of a full day in seconds.")]
    public float dayLengthInSeconds = 600f; // 10 minutes for a full cycle
    [Tooltip("Fraction of the day that is daylight (0-1).")]
    [Range(0.01f, 0.99f)]
    public float dayFraction = 0.6667f; // 40s day, 20s night for 60s day
    [Tooltip("Start time of day (0 = midnight, 6 = 6am, 12 = noon, 18 = 6pm)")]
    [Range(0f, 24f)]
    public float startTimeOfDay = 8f;

    [Header("Lighting References")]
    public Light directionalLight;

    [Header("Lighting Settings")]
    public Color dayLightColor = new Color(1f, 0.956f, 0.839f);
    public Color nightLightColor = new Color(0.2f, 0.22f, 0.35f);
    public float dayLightIntensity = 1.2f;
    public float nightLightIntensity = 0.2f;

    [Header("Color Temperature")]
    [Tooltip("Daytime color temperature in Kelvin (e.g., 6500 = neutral daylight)")]
    public float dayColorTemperature = 6500f;
    [Tooltip("Nighttime color temperature in Kelvin (e.g., 2000 = warm night)")]
    public float nightColorTemperature = 2000f;
    [Tooltip("Enable blending of color temperature during day-night cycle")]
    public bool enableColorTemperatureBlending = false;

    [Header("Ambient Settings")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);
    public Color nightAmbientColor = new Color(0.05f, 0.07f, 0.15f);

    // Sun movement fields removed: day-night cycle is now controlled by color/intensity only.

    [Header("Events")]
    public UnityEvent OnDayStart;
    public UnityEvent OnNightStart;

    [Header("Debug")]
    private float timeOfDay; // 0-24
    private bool isDaytime;

    [Header("Day Counter")]
    [Tooltip("Current day number, increments after each full day.")]
    public int currentDay = 1;

    private float dayStart = 6f;   // 6am
    private float nightStart = 18f; // 6pm
    private bool lastIsDaytime;

    void Start()
    {
        timeOfDay = startTimeOfDay;
        lastIsDaytime = IsDaytime();
        isDaytime = lastIsDaytime;
        if (directionalLight == null)
        {
            // Try to find a Directional Light in the scene
            directionalLight = FindFirstObjectByType<Light>();
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                Debug.LogWarning("No Directional Light assigned or found in the scene.");
            }
        }
        UpdateLighting();
    }

    void Update()
    {
        UpdateTime();
        UpdateLighting();
        HandleDayNightTransition();
    }

    void UpdateTime()
    {
        float delta = (24f / dayLengthInSeconds) * Time.deltaTime;
        timeOfDay += delta;
        if (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;
            currentDay++;
        }
    }

    void UpdateLighting()
    {
        // Lighting is now based on explicit time-of-day: 08:00–18:00 is day, rest is night.
        // A short blend (0.5 hour) is used at the transitions for smoothness.

        float blendDuration = 0.5f; // hours (30 minutes)
        float dayStart = 8f;
        float nightStart = 18f;

        float dayBlend = 0f;

        if (timeOfDay >= dayStart - blendDuration && timeOfDay < dayStart)
        {
            // Blend from night to day
            dayBlend = Mathf.InverseLerp(dayStart - blendDuration, dayStart, timeOfDay);
        }
        else if (timeOfDay >= dayStart && timeOfDay < nightStart)
        {
            // Full day
            dayBlend = 1f;
        }
        else if (timeOfDay >= nightStart && timeOfDay < nightStart + blendDuration)
        {
            // Blend from day to night
            dayBlend = 1f - Mathf.InverseLerp(nightStart, nightStart + blendDuration, timeOfDay);
        }
        else
        {
            // Full night
            dayBlend = 0f;
        }

        // Interpolate between night and day settings
        Color targetColor = Color.Lerp(nightLightColor, dayLightColor, dayBlend);
        float targetIntensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, dayBlend);
        Color ambientColor = Color.Lerp(nightAmbientColor, dayAmbientColor, dayBlend);

        if (directionalLight != null)
        {
            directionalLight.color = targetColor;
            directionalLight.intensity = targetIntensity;
        }

        RenderSettings.ambientLight = ambientColor;
    }

    void HandleDayNightTransition()
    {
        bool currentIsDay = IsDaytime();
        if (currentIsDay != lastIsDaytime)
        {
            if (currentIsDay)
                OnDayStart?.Invoke();
            else
                OnNightStart?.Invoke();
            lastIsDaytime = currentIsDay;
            isDaytime = currentIsDay;
        }
    }

    bool IsDaytime()
    {
        return timeOfDay >= dayStart && timeOfDay < nightStart;
    }
    /// <summary>
    /// Gets the current in-game hour (0-24).
    /// </summary>
    public float CurrentHour => timeOfDay;
}