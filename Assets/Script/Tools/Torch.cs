using UnityEngine;
using System;

public class Torch : MonoBehaviour, IBurnable
{
    [Header("Torch Settings")]
    [SerializeField, Tooltip("")] private float fuelAmount = 50f;
    [SerializeField, Tooltip("")] private float lightRadius = 3f;
    [SerializeField, Tooltip("")] private float burnRate = 1f;

    [Header("References")]
    [SerializeField, Tooltip("")] private Light torchLight;
    [SerializeField, Tooltip("")] private SphereCollider lightTrigger;
    [SerializeField, Tooltip("")] private Collider physicalCollider;

    public static event Action OnTorchPutOut;

    private bool isActive = false;
    private bool isHeld = false;

    private void Start()
    {
        UpdateLightRadius();
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
        if (isHeld || !isActive) return;

        isHeld = true;
        isActive = true;
        bonfire.RemoveFuel(fuelAmount);
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
    }

    public float GetFuelValue()
    {
        return fuelAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.AddLightSource();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.RemoveLightSource();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        if (collision.collider.CompareTag("Bonfire"))
        {
            Bonfire bonfire = collision.collider.GetComponent<Bonfire>();
            if (bonfire != null)
            {
                bonfire.AddFuel(fuelAmount);
                PutOut();
                Destroy(gameObject);
            }
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            PutOut();
            Destroy(gameObject);
        }
    }
}
