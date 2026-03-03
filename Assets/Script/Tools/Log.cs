using Unity.VRTemplate;
using UnityEngine;

public class Log : MonoBehaviour, IBaggable, IPoolable
{
    [SerializeField, Tooltip("")] private float weight = 12f;
    [SerializeField, Tooltip("")] protected bool isHeld = false;
    [SerializeField,Tooltip("The unique key for this object in the pool")] private string poolKey = "Log";

    [Header("Firewood Spawn Settings")]
    [SerializeField] private string firewoodPoolKey = "Firewood";
    [SerializeField] private float spawnRadius = 0.1f;
    
    // Properties
    public float Weight => weight;
    public bool IsHeld { get => isHeld; set => isHeld = value; }
    public string PoolKey { get => poolKey; set => poolKey = value; }

    public void OnCreatedPool() { }
    public void OnSpawnFromPool() { }
    public void OnReturnToPool() { }

    public void Split()
    {
        Vector3 center = transform.position;
        Quaternion baseRot = transform.rotation;

        // Spawn 4 firewood pieces at 90° intervals
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            Quaternion rot = baseRot * Quaternion.Euler(0, angle, 0);

            Vector3 offset = rot * Vector3.forward * spawnRadius;
            Vector3 spawnPos = center + offset;

            ItemManager.Instance.Spawn(firewoodPoolKey, spawnPos, rot);
        }

        // Return the log to the pool
        ItemManager.Instance.ReturnToPool(gameObject, PoolKey);
    }
}