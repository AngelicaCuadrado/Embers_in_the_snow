using System.Collections.Generic;
using UnityEngine;

public class PlayerTemperature : MonoBehaviour
{
    [Header("References")]
    public Material frostMaterial;

    [Header("Temperature Settings")]
    public float freezeSpeed = 0.15f;
    public float thawSpeed = 0.4f;
    [SerializeField, Tooltip("")] private float maxFrostThreshold = 5f;

    private float _currentFrostLevel = 0f;


    void Update()
    {
        if (frostMaterial == null) return;

        // NO FindObjectsOfType! Just check if our list has anything in it.
        if (PlayerController.Instance.LightSources > 0)
        {
            _currentFrostLevel -= thawSpeed * Time.deltaTime; // Melt
        }
        else
        {
            _currentFrostLevel += freezeSpeed * Time.deltaTime; // Freeze
            if (_currentFrostLevel >= maxFrostThreshold)
            {
                GameManager.Instance.Losegame();
            }
        }

        _currentFrostLevel = Mathf.Clamp01(_currentFrostLevel);
        frostMaterial.SetFloat("_FrostIntensity", _currentFrostLevel);
    }

    void OnApplicationQuit()
    {
        if (frostMaterial != null) frostMaterial.SetFloat("_FrostIntensity", 0f);
    }
}