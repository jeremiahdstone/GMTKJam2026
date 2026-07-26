using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomizeAudioPitch : MonoBehaviour
{
    [Header("Pitch Multiplier")]
    [Min(0.01f)]
    [SerializeField] private float minimumPitchMultiplier = 0.9f;

    [Min(0.01f)]
    [SerializeField] private float maximumPitchMultiplier = 1.1f;

    private AudioSource audioSource;
    private float basePitch;
    private bool wasPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        basePitch = audioSource.pitch;
    }

    private void Update()
    {
        // Detect when the AudioSource starts playing.
        if (audioSource.isPlaying && !wasPlaying)
        {
            RandomizePitch();
        }

        wasPlaying = audioSource.isPlaying;
    }

    public void RandomizePitch()
    {
        float minimum = Mathf.Min(
            minimumPitchMultiplier,
            maximumPitchMultiplier
        );

        float maximum = Mathf.Max(
            minimumPitchMultiplier,
            maximumPitchMultiplier
        );

        float randomMultiplier = Random.Range(minimum, maximum);

        audioSource.pitch = basePitch * randomMultiplier;
    }

    public void Play()
    {
        RandomizePitch();
        audioSource.Play();
        wasPlaying = true;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        RandomizePitch();
        audioSource.PlayOneShot(clip);
        wasPlaying = true;
    }

    public void ResetPitch()
    {
        audioSource.pitch = basePitch;
    }
}