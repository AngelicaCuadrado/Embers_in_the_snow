using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepLoop : MonoBehaviour
{
    [SerializeField] private Transform xrOrigin; // XR Origin or camera
    [SerializeField] private float speedThreshold = 0.05f;
    [SerializeField] private float stopDelay = 0.15f;

    private AudioSource audioSource;
    private Vector3 lastPos;
    private float stoppedTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (xrOrigin == null) xrOrigin = Camera.main.transform;
        lastPos = xrOrigin.position;
    }

    private void Update()
    {
        Vector3 delta = xrOrigin.position - lastPos;
        float speed = delta.magnitude / Time.deltaTime;
        lastPos = xrOrigin.position;

        if (speed > speedThreshold)
        {
            stoppedTimer = 0f;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            audioSource.volume = Mathf.Clamp01(speed / 2f); // scale volume by speed
        }
        else
        {
            stoppedTimer += Time.deltaTime;
            if (stoppedTimer >= stopDelay && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}