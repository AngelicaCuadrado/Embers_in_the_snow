using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the spawning and resetting of an axe instance at a designated spawn point in the scene.
/// </summary>
/// <remarks>This component should be attached to a GameObject in the Unity scene. The axe instance, spawn point,
/// and spawn button must be assigned in the Inspector for correct operation. When the spawn button is clicked, the axe
/// is repositioned and its physics are reset. This class is typically used in gameplay scenarios where the player can
/// respawn or reset an axe object.</remarks>
public class AxeSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("The axe instance to be spawned")]
    private Axe axeInstance;
    [SerializeField, Tooltip("The point where the axe will be spawned")]
    private Transform spawnPoint;
    //[SerializeField, Tooltip("The button used to spawn the axe")]
    //private Button spawnButton;

    private void Start()
    {
        ResetAxePosition();
        //spawnButton.onClick.AddListener(ResetAxePosition);
    }
    private void OnDestroy()
    {
        //spawnButton.onClick.RemoveListener(ResetAxePosition);
    }

    public void ResetAxePosition()
    {
        if (axeInstance == null) { return; }
        axeInstance.transform.position = spawnPoint.position;
        axeInstance.transform.rotation = spawnPoint.rotation;
        axeInstance.FreezePhysics();
    }
}