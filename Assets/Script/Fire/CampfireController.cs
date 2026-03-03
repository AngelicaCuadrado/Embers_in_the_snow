using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CampfireController : MonoBehaviour
{
    [Header("References")]
    public Light fireLight;
    private SphereCollider heatTrigger;

    [Header("Fuel Management")]
    public float maxFuel = 100f;
    public float currentFuel = 50f;
    public float minBurnRate = 0.5f;
    public float maxBurnRate = 3.0f;

    [Header("Light Intensity & Range")]
    public float minIntensity = 1f;
    public float maxIntensity = 10f;
    public float minRange = 2f;
    public float maxRange = 10f;

    [Header("Flicker Settings")]
    public float flickerSpeed = 4f;
    [Range(0f, 1f)] public float flickerAmount = 0.3f;

    // EVENT: Broadcasts to anyone listening when the fire dies
    public event Action<CampfireController> OnFireExtinguished;

    private float _noiseOffset;
    private bool _isBurning = true;

    void Start()
    {
        if (fireLight == null) fireLight = GetComponent<Light>();

        heatTrigger = GetComponent<SphereCollider>();
        heatTrigger.isTrigger = true; // Make sure it's a trigger!

        _noiseOffset = UnityEngine.Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!_isBurning) return;

        if (currentFuel <= 0)
        {
            ExtinguishFire();
            return;
        }

        float fuelRatio = currentFuel / maxFuel;

        // Consume Fuel
        currentFuel -= Mathf.Lerp(minBurnRate, maxBurnRate, fuelRatio) * Time.deltaTime;

        // Visuals
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, fuelRatio);
        float targetRange = Mathf.Lerp(minRange, maxRange, fuelRatio);
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, _noiseOffset);
        float flickerModifier = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, noise);

        fireLight.intensity = targetIntensity * flickerModifier;
        fireLight.range = targetRange * Mathf.Lerp(1f, flickerModifier, 0.5f);

        // OPTIMIZATION: Physically shrink the trigger radius as the fire dies
        heatTrigger.radius = targetRange;
    }

    private void ExtinguishFire()
    {
        _isBurning = false;
        currentFuel = 0f;
        fireLight.intensity = 0f;
        fireLight.range = 0f;
        heatTrigger.enabled = false; // Turn off the heat zone

        // Broadcast the event to the player (or anyone else listening)
        OnFireExtinguished?.Invoke(this);
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
        if (!_isBurning && currentFuel > 0)
        {
            _isBurning = true;
            heatTrigger.enabled = true;
        }
    }
}