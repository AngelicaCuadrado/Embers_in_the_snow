using System;
using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("References")]
    public Light fireLight;
    [SerializeField, Tooltip("")] private Bonfire bonfire;

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

        _noiseOffset = UnityEngine.Random.Range(0f, 100f);

        UpdateVisuals();
    }

    void Update()
    {
        if (!_isBurning) return;

        if (bonfire.FuelAmount <= 0)
        {
            ExtinguishFire();
            return;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float fuelRatio = Mathf.Clamp01(bonfire.FuelAmount / 100f);
        // Visuals
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, fuelRatio);
        float targetRange = Mathf.Lerp(minRange, maxRange, fuelRatio);

        // Flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, _noiseOffset);
        float flickerModifier = Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, noise);

        fireLight.intensity = targetIntensity * flickerModifier;
        fireLight.range = targetRange * Mathf.Lerp(1f, flickerModifier, 0.5f);
    }

    private void ExtinguishFire()
    {
        _isBurning = false;

        fireLight.intensity = 0f;
        fireLight.range = 0f;

        // Broadcast the event to the player (or anyone else listening)
        OnFireExtinguished?.Invoke(this);
    }

    public void AddFuel(float amount)
    {
        if (bonfire.FuelAmount > 0)
        {
            _isBurning = true;
            UpdateVisuals();
        }
    }

    public void RemoveFuel(float amount)
    {
        if (bonfire.FuelAmount <= 0)
        {
            ExtinguishFire();
        }
        else
        {
            UpdateVisuals();
        }
    }

    public void RemoveFuel(float amount) {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, maxFuel);
        if (currentFuel <= 0)
        {
            ExtinguishFire();
        }
    }
}