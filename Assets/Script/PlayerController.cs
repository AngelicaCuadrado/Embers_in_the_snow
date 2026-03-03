using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Light Tracking")]
    [SerializeField,Tooltip("")] private int lightSources = 0;

    public int LightSources
    {
        get => lightSources;
        private set
        {
            lightSources = value;
        }
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddLightSource()
    {
        LightSources++;
        //Debug.Log("LightSources: " + LightSources);
    }

    public void RemoveLightSource()
    {
        LightSources = Mathf.Max(0, LightSources - 1);
        //Debug.Log("LightSources: " + LightSources);
    }
}
