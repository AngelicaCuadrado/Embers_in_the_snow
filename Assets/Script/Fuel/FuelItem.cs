using UnityEngine;

public abstract class FuelItem : MonoBehaviour, IBaggable, IBurnable, IPoolable
{
    [Header("Fuel Settings")]
    [SerializeField, Tooltip("")] protected float weight = 1f;
    [SerializeField, Tooltip("")] protected float fuelValue = 1f;
    [SerializeField, Tooltip("")] protected string poolKey;
    [SerializeField, Tooltip("")] protected bool isHeld = false;

    // Properties
    public float Weight => weight;
    public float FuelValue => fuelValue;
    public string PoolKey { get => poolKey; set => poolKey = value; }
    public bool IsHeld { get => isHeld; set => isHeld = value; }

    public virtual void OnCreatedPool() { }
    public virtual void OnSpawnFromPool() { }
    public virtual void OnReturnToPool() { }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Bonfire")) return;

        Bonfire bonfire = collision.collider.GetComponentInParent<Bonfire>();
        if (bonfire != null)
        {
            // Add fuel to bonfire
            bonfire.AddFuel(fuelValue);

            // Return to pool
            ItemManager.Instance.ReturnToPool(gameObject, PoolKey);
        }
    }
}