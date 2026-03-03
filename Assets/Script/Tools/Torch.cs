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
    [SerializeField] private Bonfire bonfire;
    [SerializeField] private Rigidbody rb;


    [SerializeField, Tooltip("The unique key for this object in the pool")] private string poolKey = "Torch";

    [SerializeField] private float fuelAmount;
    private bool isActive = false;
    private bool isHeld = false;

    // Events
    public static event Action OnTorchPutOut;

    // Properties
    public float FuelValue => fuelAmount;
    public string PoolKey { get => poolKey; set => poolKey = value; }
    public Bonfire Bonfire { get => bonfire; set => bonfire = value; }

    public void OnCreatedPool() { }

    public void OnSpawnFromPool()
    {
        fuelAmount = maxFuel;
        isActive = false;
        isHeld = false;

        torchLight.enabled = false;
        lightTrigger.enabled = false;

        // Freeze physics so it stays suspended
        rb.isKinematic = true;
        rb.useGravity = false;
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

        UpdateLightRadius();

        if (fuelAmount <= 0f)
        {
            print("Torch ran out of fuel");
            fuelAmount = 0f;
            PutOut();
        }
    }

    private void UpdateLightRadius()
    {
        float fuelPercent = Mathf.Clamp01(fuelAmount / maxFuel);
        float currentRadius = lightRadius * fuelPercent;
        torchLight.range = currentRadius;
        lightTrigger.radius = currentRadius;
    }

    public void PickUp()
    {
        if (isHeld || isActive) return;

        isHeld = true;
        isActive = true;

        torchLight.enabled = true;
        lightTrigger.enabled = true;

        UpdateLightRadius();

        // Re-enable physics so it behaves normally after pickup
        rb.isKinematic = false;
        rb.useGravity = true;

        // Remove fuel from bonfire when picked up
        bonfire.RemoveFuel(maxFuel);
    }


    public void Drop()
    {
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void PutOut()
    {
        print("Torch put out");
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
        print("Torch collided with: " + collision.collider.name);
        if (!isActive || isHeld) return;
        print("Torch is active and not held, processing collision");

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

}