using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField, Tooltip("The key used to identify this axe in the object pool")]
    private string poolKey = "Axe";
    [SerializeField, Tooltip("The sound effect played when the axe hits a log")]
    private AudioClip soundEffect;

    public string PoolKey { get => poolKey; set => poolKey = value; }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Log>() is Log log)
        {
            log.Split();
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        }
    }
}