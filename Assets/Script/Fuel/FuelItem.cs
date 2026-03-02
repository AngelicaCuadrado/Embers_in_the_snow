using UnityEngine;

public abstract class FuelItem : MonoBehaviour, IBaggable, IBurnable, IPoolable
{
    [Header("Fuel Settings")]
    [SerializeField] protected float weight = 1f;
    [SerializeField] protected float fuelValue = 1f;

    public string PoolKey { get; set; }

    public float GetWeight() => weight;
    public float GetFuelValue() => fuelValue;
    public string GetPoolKey() => PoolKey;

    public virtual void OnCreatedPool() { }
    public virtual void OnSpawnFromPool() { }
    public virtual void OnReturnToPool() { }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Bonfire")) return;

        if (collision.collider.TryGetComponent<Bonfire>(out var bonfire))
        {
            bonfire.AddFuel(fuelValue);
            ReturnToPool();
        }
    }

    protected void ReturnToPool()
    {
        var pooler = FindAnyObjectByType<ObjectPooler>();
        pooler.ReturnToPool(gameObject, PoolKey);
    }
}