using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Bag : MonoBehaviour
{
    [Header("Bag Settings")]
    [SerializeField, Tooltip("")] private float maxWeight = 12f;
    private float currentWeight = 0f;

    [Header("References")]
    [SerializeField, Tooltip("")] private ObjectPooler objectPooler;
    [SerializeField, Tooltip("")] private Transform itemSpawnPoint;

    private Queue<IBaggable> items = new Queue<IBaggable>();

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is baggable
        if (other.TryGetComponent<IBaggable>(out var baggable))
        {
            float weight = baggable.GetWeight();

            // If bag is full, ignore
            if (currentWeight + weight > maxWeight)
                return;

            // Add to bag
            items.Enqueue(baggable);
            currentWeight += weight;

            // Return object to pool
            var poolable = other.GetComponent<IPoolable>();
            if (poolable != null)
            {
                objectPooler.ReturnToPool(other.gameObject, poolable.PoolKey);
            }
        }
    }
    public void Interact(XRBaseInteractor interactor)
    {
        if (items.Count == 0)
            return;

        IBaggable baggable = items.Dequeue();
        currentWeight -= baggable.GetWeight();

        string key = baggable.GetPoolKey();
        GameObject obj = ItemManager.Instance.Spawn(key, itemSpawnPoint.position, itemSpawnPoint.rotation);

        if (obj.TryGetComponent<XRGrabInteractable>(out var grab))
        {
            // Cast to the new interface types
            var selectInteractor = interactor as IXRSelectInteractor;
            var selectInteractable = grab as IXRSelectInteractable;

            if (selectInteractor != null && selectInteractable != null)
            {
                interactor.interactionManager.SelectEnter(selectInteractor, selectInteractable);
            }
        }
    }
}