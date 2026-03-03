using UnityEngine;

public class TemporaryLightController : MonoBehaviour
{
    [SerializeField, Tooltip("")] private Light temporaryLight;
    [SerializeField, Tooltip("")] private GameObject fireEffect;
    [SerializeField, Tooltip("")] private Collider turnOffTrigger;
    [SerializeField, Tooltip("")] private AudioClip soundEffect;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            temporaryLight.enabled = false;
            fireEffect.SetActive(false);
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        }
    }
}
