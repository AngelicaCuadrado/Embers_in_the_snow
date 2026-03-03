using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class Bag : MonoBehaviour
{
    [Header("Bag Settings")]
    [SerializeField, Tooltip("")] private float maxWeight = 12f;
    private float currentWeight = 0f;

    [Header("References")]
    [SerializeField, Tooltip("")] private Transform itemSpawnPoint;

    private Queue<IBaggable> items = new Queue<IBaggable>();

    private void OnTriggerEnter(Collider other)
    {
        // Get the IBaggable component from the collided object or its parent
        IBaggable baggable = other.GetComponent<IBaggable>() ?? other.GetComponentInParent<IBaggable>();
        if (baggable == null) return;

        // Ignore items currently held
        if (baggable.IsHeld)
            return;

        float weight = baggable.Weight;

        // Ignore if adding this item exceeds max weight
        if (currentWeight + weight > maxWeight)
            return;

        items.Enqueue(baggable);
        currentWeight += weight;

        var poolable = other.GetComponent<IPoolable>();
        if (poolable != null)
        {
            ItemManager.Instance.ReturnToPool(other.gameObject, poolable.PoolKey);
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