using UnityEngine;

/// <summary>
/// Represents an axe that can interact with logs and be managed within an object pool in the game environment.
/// </summary>
/// <remarks>This class provides methods to control the axe's physics state and handles collision interactions
/// with log objects. It is intended for use within Unity-based gameplay scenarios where axes are pooled and reused.
/// Thread safety is not guaranteed; all interactions should occur on the Unity main thread.</remarks>
public class Axe : MonoBehaviour
{
    [SerializeField, Tooltip("The sound effect played when the axe hits a log")]
    private AudioClip soundEffect;
    [SerializeField, Tooltip("The Rigidbody attached to the axe")]
    private Rigidbody rb;

    public void FreezePhysics()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void UnfreezePhysics()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Log>() is Log log)
        {
            log.Split();
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        }
    }
}