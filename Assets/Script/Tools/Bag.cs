using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class Bag : MonoBehaviour
{
    [Header("Bag Settings")]
    [SerializeField, Tooltip("The maximum weight the bag can hold")]
    private float maxWeight = 12f;
    [SerializeField, Tooltip("The current weight of items in the bag")]
    private float currentWeight = 0f;

    [Header("References")]
    [SerializeField, Tooltip("The point where items will be spawned when taken from the bag")]
    private Transform itemSpawnPoint;
    private Queue<IBaggable> items = new Queue<IBaggable>();

    private void OnTriggerStay(Collider other)
    {
        // Find the IBaggable on the collider or any parent
        IBaggable baggable = other.GetComponent<IBaggable>() ?? other.GetComponentInParent<IBaggable>();
        if (baggable == null)
        {
            return;
        }

        // We need the MonoBehaviour root so we can return the whole GameObject to the pool
        var baggableMB = baggable as MonoBehaviour;
        if (baggableMB == null)
        {
            return;
        }


        // Ignore items currently held
        if (baggable.IsHeld)
        {
            return;
        }

        float weight = baggable.Weight;

        // Ignore if adding this item exceeds max weight
        if (currentWeight + weight > maxWeight)
        {
            return;
        }

        // Enqueue the interface reference (you can switch to a lightweight struct if preferred)
        items.Enqueue(baggable);
        currentWeight += weight;

        // Return the root item to the pool (use the MonoBehaviour's GameObject)
        var poolable = baggableMB.GetComponent<IPoolable>() ?? baggableMB.GetComponentInParent<IPoolable>();
        if (poolable != null)
        {
            ItemManager.Instance.ReturnToPool(baggableMB.gameObject, poolable.PoolKey);
        }
    }


    public void Interact(XRBaseInteractor interactor)
    {
        if (items.Count == 0)
            return;

        IBaggable baggable = items.Dequeue();
        currentWeight -= baggable.Weight;

        string key = baggable.PoolKey;
        GameObject obj = ItemManager.Instance.Spawn(key, itemSpawnPoint.position, itemSpawnPoint.rotation);

        if (obj.TryGetComponent<IBaggable>(out var newItem))
        {
            newItem.IsHeld = true;
        }

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

    private void OnDrawGizmos()
    {
        // Draw a wire sphere to visualize the spawning area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(itemSpawnPoint.position, 0.2f);
    }
}