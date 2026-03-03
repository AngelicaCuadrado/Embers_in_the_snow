using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("References")]
    public Light fireLight;

    [Header("Fuel Management")]
    public float maxFuel = 100f;
    public float currentFuel = 50f;

    [Tooltip("Fuel consumed per second when the fire is dying/low.")]
    public float minBurnRate = 0.5f;
    [Tooltip("Fuel consumed per second when fully stoked.")]
    public float maxBurnRate = 3.0f;

    [Header("Light Intensity & Range")]
    [Tooltip("Base intensity when fuel is almost gone.")]
    public float minIntensity = 1f;
    [Tooltip("Base intensity when fuel is completely full.")]
    public float maxIntensity = 10f;

    [Tooltip("Light radius when fuel is almost gone.")]
    public float minRange = 2f;
    [Tooltip("Light radius when fuel is completely full.")]
    public float maxRange = 10f;

    [Header("Flicker Settings")]
    public float flickerSpeed = 4f;
    [Tooltip("How much the light flickers (0.0 = none, 1.0 = heavy flicker).")]
    [Range(0f, 1f)] public float flickerAmount = 0.3f;

    private float _noiseOffset;

    void Start()
    {
        if (fireLight == null) fireLight = GetComponent<Light>();

        // Offset the noise seed so if you have multiple fires, they don't flicker in perfect sync
        _noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (currentFuel <= 0)
        {
            fireLight.intensity = 0f;
            fireLight.range = 0f; // Fire completely goes out
            currentFuel = 0f;
            return;
        }

        // 1. Determine how full the campfire is (0.0 to 1.0)
        float fuelRatio = currentFuel / maxFuel;

        // 2. Dynamic Burn Rate
        float currentBurnRate = Mathf.Lerp(minBurnRate, maxBurnRate, fuelRatio);
        currentFuel -= currentBurnRate * Time.deltaTime;

        // 3. Dynamic Base Brightness & Range
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, fuelRatio);
        float targetRange = Mathf.Lerp(minRange, maxRange, fuelRatio);

        // 4. Smooth Flicker Effect using Perlin Noise
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, _noiseOffset);
        float flickerModifier = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, noise);

        // 5. Apply to the light
        fireLight.intensity = targetIntensity * flickerModifier;

        // Apply 50% of the flicker effect to the range so the light bounds "dance" slightly
        fireLight.range = targetRange * Mathf.Lerp(1f, flickerModifier, 0.5f);
    }

    // Call this from your interaction script when throwing a log on the fire
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
        Debug.Log("Fuel added! Current fuel: " + currentFuel);
    }
}