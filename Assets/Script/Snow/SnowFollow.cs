using UnityEngine;

public class SnowFollow : MonoBehaviour
{
    public Transform playerTransform;
    public float heightOffset = 5f;

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            // follow the players horizontal position,  but stay at a fixed height
            transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + heightOffset, playerTransform.position.z);
        }
    }
}
