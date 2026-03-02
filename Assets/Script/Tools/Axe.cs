using UnityEngine;

public class Axe : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Log>() is Log log)
        {
            log.Split();
        }
    }
}