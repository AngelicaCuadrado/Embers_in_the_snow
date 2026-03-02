using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [SerializeField] private ObjectPooler objectPooler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject Spawn(string key, Vector3 pos, Quaternion rot)
    {
        return objectPooler.Spawn(key, pos, rot);
    }

    public void ReturnToPool(GameObject obj, string key)
    {
        objectPooler.ReturnToPool(obj, key);
    }
}