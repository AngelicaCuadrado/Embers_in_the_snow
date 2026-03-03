using UnityEngine;
public class Bonfire : MonoBehaviour
{
    [Header("Bonfire Settings")]
    [SerializeField, Tooltip("")] private float fuelAmount = 100f;
    [SerializeField, Tooltip("")] private float lightRadius = 5f;
    [SerializeField, Tooltip("")] private float burnRate = 1f;

    [Header("References")]
    [SerializeField, Tooltip("")] private PlayerController player;
    [SerializeField, Tooltip("")] private SphereCollider lightTrigger;
    [SerializeField, Tooltip("")] private Collider physicalCollider;
    [SerializeField, Tooltip("")] private GameObject torchPrefab;
    [SerializeField, Tooltip("")] private Transform torchSpawn;
    [SerializeField, Tooltip("")] private CampfireController campfireController;
    [SerializeField, Tooltip("")] private GameObject fireEffect;




    private void Start()
    {
        UpdateLightRadius();
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
        }
        else
        {
            fuelAmount = 0f;
            lightTrigger.enabled = false;
            if (fireEffect != null) fireEffect.SetActive(false);
        }
    }

    public void AddFuel(float amount)
    {
        fuelAmount += amount;
        campfireController.AddFuel(amount);
        lightTrigger.enabled = true;
        if (fireEffect != null) fireEffect.SetActive(true);
        UpdateLightRadius();
    }

    public void RemoveFuel(float amount)
    {
        fuelAmount -= amount;
        UpdateLightRadius();
    }

    private void UpdateLightRadius()
    {
        // Light radius scales with fuel amount
        lightRadius = Mathf.Clamp(fuelAmount * 0.05f, 0f, 10f);
        lightTrigger.radius = lightRadius;
    }

    public void SpawnTorch()
    {
        print("Spawning torch from bonfire");
        GameObject torch = Instantiate(torchPrefab, torchSpawn.position, Quaternion.identity);
        if (torch.TryGetComponent<Torch>(out var torchComponent))
        {
            torchComponent.Bonfire = this;
            torchComponent.OnSpawnFromPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player != null) player.AddLightSource();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player != null) player.RemoveLightSource();
        }
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
