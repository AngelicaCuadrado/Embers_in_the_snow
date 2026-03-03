using UnityEngine;
using System;

public class Torch : MonoBehaviour, IBurnable, IPoolable
{
    [Header("Torch Settings")]
    [SerializeField] private float maxFuel = 50f;
    [SerializeField] private float burnRate = 1f;
    [SerializeField] private float lightRadius = 3f;

    [Header("References")]
    [SerializeField] private Light torchLight;
    [SerializeField] private SphereCollider lightTrigger;
    [SerializeField] private Collider physicalCollider;

    [SerializeField, Tooltip("The unique key for this object in the pool")] private string poolKey = "Torch";

    private float fuelAmount;
    private bool isActive = false;
    private bool isHeld = false;

    // Events
    public static event Action OnTorchPutOut;

    // Properties
    public float FuelValue => fuelAmount;
    public string PoolKey { get => poolKey; set => poolKey = value; }

    public void OnCreatedPool() { }

    public void OnSpawnFromPool()
    {
        fuelAmount = maxFuel;
        isActive = false;
        isHeld = false;

        torchLight.enabled = false;
        lightTrigger.enabled = false;
    }

    public void OnReturnToPool()
    {
        // Reset state when returned
        isActive = false;
        isHeld = false;

        torchLight.enabled = false;
        lightTrigger.enabled = false;
    }

    private void Update()
    {
        if (!isActive) return;

        fuelAmount -= burnRate * Time.deltaTime;

        if (fuelAmount <= 0f)
        {
            fuelAmount = 0f;
            PutOut();
        }
    }

    private void UpdateLightRadius()
    {
        torchLight.range = lightRadius;
        lightTrigger.radius = lightRadius;
    }

    public void PickUp(Bonfire bonfire)
    {
        if (isHeld || isActive) return;

        isHeld = true;
        isActive = true;

        torchLight.enabled = true;
        lightTrigger.enabled = true;

        UpdateLightRadius();

        // Taking a torch removes fuel from the bonfire
        bonfire.RemoveFuel(maxFuel);
    }

    public void Drop()
    {
        isHeld = false;
    }

    private void PutOut()
    {
        isActive = false;
        torchLight.enabled = false;
        lightTrigger.enabled = false;

        OnTorchPutOut?.Invoke();

        ItemManager.Instance.ReturnToPool(gameObject, PoolKey);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.AddLightSource();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.RemoveLightSource();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        // Returned to bonfire
        if (collision.collider.CompareTag("Bonfire"))
        {
            if (collision.collider.TryGetComponent<Bonfire>(out var bonfire))
            {
                bonfire.AddFuel(fuelAmount);
            }

            PutOut();
        }

        // Dropped in snow
        else if (collision.collider.CompareTag("Ground"))
        {
            PutOut();
        }
    }

    public float GetFuelValue() => fuelAmount;
}