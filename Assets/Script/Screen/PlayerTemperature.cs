using System.Collections.Generic;
using UnityEngine;

public class PlayerTemperature : MonoBehaviour
{
    [Header("References")]
    public Material frostMaterial;

    [Header("Temperature Settings")]
    public float freezeSpeed = 0.15f;
    public float thawSpeed = 0.4f;

    private float _currentFrostLevel = 0f;

    // Uses a HashSet for lightning-fast lookups and to prevent duplicates
    private HashSet<CampfireController> activeHeatSources = new HashSet<CampfireController>();

    void Update()
    {
        if (frostMaterial == null) return;

        // NO FindObjectsOfType! Just check if our list has anything in it.
        if (activeHeatSources.Count > 0)
        {
            _currentFrostLevel -= thawSpeed * Time.deltaTime; // Melt
        }
        else
        {
            _currentFrostLevel += freezeSpeed * Time.deltaTime; // Freeze
        }

        _currentFrostLevel = Mathf.Clamp01(_currentFrostLevel);
        frostMaterial.SetFloat("_FrostIntensity", _currentFrostLevel);
    }

    // --- EVENT LISTENERS & TRIGGERS ---

    private void OnTriggerEnter(Collider other)
    {
        // When we walk into a heat zone, add it to our list and listen for its death
        if (other.TryGetComponent(out CampfireController fire))
        {
            activeHeatSources.Add(fire);
            fire.OnFireExtinguished += HandleFireExtinguished;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When we walk out of a heat zone, remove it
        if (other.TryGetComponent(out CampfireController fire))
        {
            RemoveHeatSource(fire);
        }
    }

    // Fired automatically by the Action in CampfireController
    private void HandleFireExtinguished(CampfireController fire)
    {
        RemoveHeatSource(fire);
    }

    private void RemoveHeatSource(CampfireController fire)
    {
        if (activeHeatSources.Contains(fire))
        {
            activeHeatSources.Remove(fire);
            fire.OnFireExtinguished -= HandleFireExtinguished; // Unsubscribe to prevent memory leaks
        }
    }

    void OnApplicationQuit()
    {
        if (frostMaterial != null) frostMaterial.SetFloat("_FrostIntensity", 0f);
    }
}