using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField, Tooltip("")] private string poolKey = "Axe";
    [SerializeField, Tooltip("")] private AudioClip soundEffect;


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