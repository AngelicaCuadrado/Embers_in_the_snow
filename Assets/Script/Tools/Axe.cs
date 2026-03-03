using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField, Tooltip("")] private string poolKey = "Axe";

    public string PoolKey { get => poolKey; set => poolKey = value; }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Log>() is Log log)
        {
            log.Split();
        }
    }
}