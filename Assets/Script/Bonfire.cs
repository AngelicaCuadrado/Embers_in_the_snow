using UnityEngine;
public class Bonfire : MonoBehaviour
{
    [Header("Bonfire Settings")]
    [SerializeField, Tooltip("")] private float fuelAmount = 100f;
    [SerializeField, Tooltip("")] private float lightRadius = 5f;
    [SerializeField, Tooltip("")] private float burnRate = 1f;

    [Header("References")]
    [SerializeField, Tooltip("")] private PlayerController player;
    [SerializeField, Tooltip("")] private Light bonfireLight;
    [SerializeField, Tooltip("")] private SphereCollider lightTrigger;
    [SerializeField, Tooltip("")] private GameObject torchPrefab;

    private void Start()
    {
        UpdateLightRadius();
        //Torch.OnTorchPutOut += SpawnTorch;
    }

    private void OnDestroy()
    {
        //Torch.OnTorchPutOut -= SpawnTorch;
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
            bonfireLight.enabled = false;
            lightTrigger.enabled = false;
        }
    }

    public void AddFuel(float amount)
    {
        fuelAmount += amount;
        bonfireLight.enabled = true;
        lightTrigger.enabled = true;
        UpdateLightRadius();
    }

    private void UpdateLightRadius()
    {
        // Light radius scales with fuel amount
        lightRadius = Mathf.Clamp(fuelAmount * 0.05f, 2f, 10f);
        bonfireLight.range = lightRadius;
        lightTrigger.radius = lightRadius;
    }

    public void SpawnTorch()
    {
        Instantiate(torchPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (player != null) player.AddLightSource();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (player != null) player.RemoveLightSource();
        }
    }
}
