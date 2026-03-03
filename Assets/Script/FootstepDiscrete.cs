using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepDiscrete : MonoBehaviour
{
    [SerializeField] private Transform xrOrigin; // XR Origin or camera
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float baseStride = 0.7f; // meters per step at normal speed
    [SerializeField] private float minSpeedThreshold = 0.05f;
    [SerializeField] private float pitchRange = 0.1f;

    private AudioSource audioSource;
    private Vector3 lastPos;
    private float distanceAccumulator;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (xrOrigin == null) xrOrigin = Camera.main.transform;
        lastPos = xrOrigin.position;
    }

    private void Update()
    {
        Vector3 delta = xrOrigin.position - lastPos;
        float moved = delta.magnitude;
        lastPos = xrOrigin.position;

        // accumulate distance walked
        distanceAccumulator += moved;

        // compute dynamic stride based on speed (shorter stride when slow)
        float speed = moved / Time.deltaTime;
        if (speed < minSpeedThreshold) return;

        float stride = Mathf.Lerp(baseStride * 0.5f, baseStride * 1.2f, Mathf.Clamp01(speed / 2f));

        if (distanceAccumulator >= stride)
        {
            PlayFootstep();
            distanceAccumulator = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = 1f + Random.Range(-pitchRange, pitchRange);
        audioSource.PlayOneShot(clip, 1f);
    }
}