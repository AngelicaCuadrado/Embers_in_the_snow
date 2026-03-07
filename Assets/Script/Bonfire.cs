using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [Header("Bonfire Settings")]
    [SerializeField, Tooltip("The amount of fuel the bonfire starts with")]
    private float fuelAmount = 100f;
    [SerializeField, Tooltip("The radius of the light emitted by the bonfire")]
    private float lightRadius = 5f;
    [SerializeField, Tooltip("How much (in percentage) of the fuel amount is converted to light radius")]
    private float lightToFuelRatio = 0.1f;
    [SerializeField, Tooltip("The rate at which the bonfire burns fuel")]
    private float burnRate = 1f;

    [Header("Colliders")]
    [SerializeField, Tooltip("The collider used to detect objects within the light radius")]
    private SphereCollider lightTrigger;
    [SerializeField, Tooltip("The physical collider of the bonfire")]
    private Collider physicalCollider;

    [Header("Torch")]
    [SerializeField, Tooltip("The prefab of the torch to spawn")]
    private GameObject torchPrefab;
    [SerializeField, Tooltip("The transform where the torch will be spawned")]
    private Transform torchSpawn;

    [Header("References")]
    [SerializeField, Tooltip("The controller managing the campfire visuals and audio")]
    private CampfireController campfireController;

    // Properties
    public float FuelAmount => fuelAmount;
    public float LightRadius => lightRadius;
    public float BurnRate => burnRate;

    private void Start()
    {
        UpdateLightRadius();
        campfireController.UpdateVisuals(fuelAmount); // initialize visuals
        SpawnTorch();
        Torch.OnTorchPutOut += SpawnTorch;
    }

    private void OnDestroy()
    {
        Torch.OnTorchPutOut -= SpawnTorch;
    }

    private void Update()
    {
        if (fuelAmount > 0f)
        {
            fuelAmount -= burnRate * Time.deltaTime;
            UpdateLightRadius();
            campfireController.UpdateVisuals(fuelAmount);
        }
        else
        {
            ExtinguishFire();
        }
    }

    public void AddFuel(float amount)
    {
        fuelAmount += amount;

        LightFire();

        // Update light radius immediately to reflect the added fuel
        UpdateLightRadius();
        campfireController.UpdateVisuals(fuelAmount);
    }

    public void RemoveFuel(float amount)
    {
        fuelAmount -= amount;
        UpdateLightRadius();
        campfireController.UpdateVisuals(fuelAmount);
    }

    private void UpdateLightRadius()
    {
        // Light radius is a percentage of fuel amount, clamped to a max of 20 units
        lightRadius = Mathf.Clamp(fuelAmount * lightToFuelRatio, 0f, 20f);
        lightTrigger.radius = lightRadius;
    }

    private void LightFire()
    {
        lightTrigger.enabled = true;
        campfireController.EnableFire(true);
    }

    private void ExtinguishFire()
    {
        fuelAmount = 0f;
        lightTrigger.enabled = false;
        campfireController.EnableFire(false);
    }

    public void SpawnTorch()
    {
        GameObject torch = Instantiate(torchPrefab, torchSpawn.position, torchSpawn.rotation);
        if (torch.TryGetComponent<Torch>(out var torchComponent))
        {
            torchComponent.Bonfire = this;
            torchComponent.OnSpawnFromPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerController.Instance?.AddLightSource();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerController.Instance?.RemoveLightSource();
    }

    private void OnDrawGizmos()
    {
        // Visualize light radius in editor
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, lightRadius);

        // Draw wireframe for light
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }
}