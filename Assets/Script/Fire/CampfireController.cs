using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The visual effect of the fire")]
    private GameObject fireEffect;
    [SerializeField, Tooltip("The light component of the fire")]
    private Light fireLight;
    [SerializeField, Tooltip("The audio source for the fire sound")]
    private AudioSource fireAudio;
    [SerializeField, Tooltip("The VFX controller for the fire")]
    private VFX_FireController fireVFXController;

    [Header("Light Settings")]
    [SerializeField, Tooltip("The minimum intensity of the fire light")]
    private float minIntensity = 1f;
    [SerializeField, Tooltip("The maximum intensity of the fire light")]
    private float maxIntensity = 10f;

    [Header("Flicker Settings")]
    [SerializeField, Tooltip("The speed at which the fire light flickers")]
    private float flickerSpeed = 4f;
    [SerializeField, Range(0f, 1f), Tooltip("The amount of flicker applied to the fire light")]
    private float flickerAmount = 0.3f;
    [SerializeField, Tooltip("The offset for the noise used in flickering")]
    private float noiseOffset;

    [Header("Color Settings")]
    [SerializeField, Tooltip("The color of the fire when fuel is low")]
    private Color lowFuelColor = new Color(1f, 0.9f, 0.6f);
    [SerializeField, Tooltip("The color of the fire when fuel is high")]
    private Color highFuelColor = new Color(1f, 0.3f, 0.1f);

    [Header("Wind Settings")]
    [SerializeField, Tooltip("The current direction of the wind affecting the fire")]
    private Vector3 currentWindDirection;
    [SerializeField, Tooltip("The speed at which the wind direction changes")]
    private float windChangeSpeed = 0.2f;
    [SerializeField, Tooltip("The strength of the wind affecting the fire")]
    private float windStrength = 0.5f;

    [Header("Fire Curves")]
    [SerializeField, Tooltip("The curve controlling the size of the fire")]
    private AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Tooltip("The curve controlling the intensity of the fire")]
    private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Tooltip("The curve controlling the color of the fire")]
    private AnimationCurve colorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio Settings")]
    [SerializeField, Tooltip("The maximum volume of the fire audio")]
    private float maxVolume = 0.8f;
    [SerializeField, Tooltip("The minimum volume of the fire audio")]
    private float minVolume = 0.1f;
    [SerializeField, Tooltip("The minimum pitch of the fire audio")]
    private float minPitch = 0.8f;
    [SerializeField, Tooltip("The maximum pitch of the fire audio")]
    private float maxPitch = 1.2f;

    private void Awake()
    {
        noiseOffset = Random.Range(0f, 100f);
    }

    public void UpdateVisuals(float fuelAmount)
    {
        float fuelRatio = Mathf.Clamp01(fuelAmount / 100f);

        UpdateLight(fuelRatio);
        UpdateVFX(fuelRatio);
        UpdateWind();
        UpdateAudio(fuelRatio);
    }

    private void UpdateLight(float fuelRatio)
    {
        // Light intensity
        float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, intensityCurve.Evaluate(fuelRatio));

        // Flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        float flicker = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, noise);

        fireLight.intensity = baseIntensity * flicker;

        // Light color
        Color fireColor = Color.Lerp(lowFuelColor, highFuelColor, colorCurve.Evaluate(fuelRatio));
        fireLight.color = fireColor;
    }

    private void UpdateVFX(float fuelRatio)
    {
        Color fireColor = Color.Lerp(lowFuelColor, highFuelColor, colorCurve.Evaluate(fuelRatio));
        fireVFXController.SetFireColor(fireColor);

        // Scale particle intensity
        float vfxIntensity = intensityCurve.Evaluate(fuelRatio);
        fireVFXController.SetFireIntensity(vfxIntensity);

        // Scale the whole fire object
        float scale = Mathf.Lerp(0.3f, 2f, sizeCurve.Evaluate(fuelRatio));
        fireEffect.transform.localScale = Vector3.one * scale;
    }

    private void UpdateWind()
    {
        float t = Time.time * windChangeSpeed;

        float x = Mathf.PerlinNoise(t, 0f) - 0.5f;
        float y = Mathf.PerlinNoise(t, 10f) - 0.5f;
        float z = Mathf.PerlinNoise(t, 20f) - 0.5f;

        y = Mathf.Clamp(y, 0f, 0.3f); // never blow downward

        Vector3 target = new Vector3(x, y, z).normalized * windStrength;

        currentWindDirection = Vector3.Lerp(currentWindDirection, target, Time.deltaTime * 0.5f);

        fireVFXController.SetFireWindDirection(currentWindDirection);
    }

    private void UpdateAudio(float fuelRatio)
    {
        if (fireAudio == null) return;

        float intensity = intensityCurve.Evaluate(fuelRatio);

        // Volume scales with intensity curve
        fireAudio.volume = Mathf.Lerp(minVolume, maxVolume, intensity);

        // Pitch rises slightly with intensity
        fireAudio.pitch = Mathf.Lerp(minPitch, maxPitch, intensity);

        // Stop audio when fire is out
        if (fuelRatio <= 0f && fireAudio.isPlaying)
            fireAudio.Stop();
        else if (fuelRatio > 0f && !fireAudio.isPlaying)
            fireAudio.Play();
    }

    public void EnableFire(bool enabled)
    {
        fireEffect.SetActive(enabled);
        fireLight.enabled = enabled;

        if (enabled)
            fireAudio?.Play();
        else
            fireAudio?.Stop();
    }
}