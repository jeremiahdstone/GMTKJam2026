using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Mixer Parameters")]
    [SerializeField] private string soundVolumeParameter = "soundVolume";
    [SerializeField] private string musicVolumeParameter = "musicVolume";

    [Header("Blood Effect")]
    [SerializeField] private float bloodShakeStrength = 0.1f;
    [SerializeField] private ParticleSystem bloodParticlePrefab;
    [SerializeField] private RectTransform soundParticleSpawnPoint;
    [SerializeField] private RectTransform musicParticleSpawnPoint;
    [SerializeField] private float particleSpawnCooldown = 0.08f;

    private float lastSoundParticleTime;
    private float lastMusicParticleTime;

    [Header("Slider Tween")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 3f;
    [SerializeField] private int shakeVibrato = 8;

    private Quaternion soundSliderStartingRotation;
    private Quaternion musicSliderStartingRotation;

    private Tween soundSliderTween;
    private Tween musicSliderTween;

    private bool isInitializing;

    private const string SoundVolumeKey = "GameSound";
    private const string MusicVolumeKey = "Music";

    private void Awake()
    {
        soundSliderStartingRotation = soundSlider.transform.localRotation;
        musicSliderStartingRotation = musicSlider.transform.localRotation;

        isInitializing = true;

        ConfigureSlider(soundSlider);
        ConfigureSlider(musicSlider);

        float savedSoundVolume =
            PlayerPrefs.GetFloat(SoundVolumeKey, 1f);

        float savedMusicVolume =
            PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

        soundSlider.SetValueWithoutNotify(savedSoundVolume);
        musicSlider.SetValueWithoutNotify(savedMusicVolume);

        ApplyMixerVolume(soundVolumeParameter, savedSoundVolume);
        ApplyMixerVolume(musicVolumeParameter, savedMusicVolume);

        soundSlider.onValueChanged.AddListener(SetSoundVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        isInitializing = false;
    }

    private void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    public void SetSoundVolume(float volume)
    {
        ApplyMixerVolume(soundVolumeParameter, volume);
        PlayerPrefs.SetFloat(SoundVolumeKey, volume);

        if (!isInitializing)
        {
            bool canSpawnParticle =
                Time.unscaledTime >= lastSoundParticleTime + particleSpawnCooldown;

            PlaySliderBloodEffect(
                soundSlider,
                soundParticleSpawnPoint,
                soundSliderStartingRotation,
                ref soundSliderTween,
                canSpawnParticle
            );

            if (canSpawnParticle)
                lastSoundParticleTime = Time.unscaledTime;
        }
    }

    public void SetMusicVolume(float volume)
    {
        ApplyMixerVolume(musicVolumeParameter, volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);

        if (!isInitializing)
        {
            bool canSpawnParticle =
                Time.unscaledTime >= lastMusicParticleTime + particleSpawnCooldown;

            PlaySliderBloodEffect(
                musicSlider,
                musicParticleSpawnPoint,
                musicSliderStartingRotation,
                ref musicSliderTween,
                canSpawnParticle
            );

            if (canSpawnParticle)
                lastMusicParticleTime = Time.unscaledTime;
        }
    }

    private void ApplyMixerVolume(
        string mixerParameter,
        float linearVolume)
    {
        float decibels = LinearToDecibel(linearVolume);

        audioMixer.SetFloat(mixerParameter, decibels);
    }

    private float LinearToDecibel(float linearVolume)
    {
        // Prevent Log10(0).
        if (linearVolume <= 0.0001f)
            return -80f;

        return Mathf.Log10(linearVolume) * 20f;
    }

    private void PlaySliderBloodEffect(
    Slider slider,
    RectTransform particleSpawnPoint,
    Quaternion startingRotation,
    ref Tween sliderTween,
    bool spawnParticles)
    {
        sliderTween?.Kill();

        slider.transform.localRotation = startingRotation;

        sliderTween = slider.transform
            .DOShakeRotation(
                duration: shakeDuration,
                strength: new Vector3(0f, 0f, shakeStrength),
                vibrato: shakeVibrato,
                randomness: 60f,
                fadeOut: true
            )
            .SetUpdate(true)
            .OnComplete(() =>
            {
                slider.transform.localRotation = startingRotation;
            });

        if (spawnParticles)
            SpawnBloodSplatter(slider, particleSpawnPoint);
    }

    private void SpawnBloodSplatter(
    Slider slider,
    RectTransform particleSpawnPoint)
    {
        if (isInitializing)
            return;



        if (bloodParticlePrefab == null)
            return;

        RectTransform spawnPoint =
            particleSpawnPoint != null
                ? particleSpawnPoint
                : slider.GetComponent<RectTransform>();

        ParticleSystem particles = Instantiate(
            bloodParticlePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        particles.Play();
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();

        soundSliderTween?.Kill();
        musicSliderTween?.Kill();

        if (soundSlider != null)
        {
            soundSlider.transform.localRotation =
                soundSliderStartingRotation;
        }

        if (musicSlider != null)
        {
            musicSlider.transform.localRotation =
                musicSliderStartingRotation;
        }
    }

    private void OnDestroy()
    {
        if (soundSlider != null)
        {
            soundSlider.onValueChanged.RemoveListener(
                SetSoundVolume
            );
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(
                SetMusicVolume
            );
        }

        soundSliderTween?.Kill();
        musicSliderTween?.Kill();
    }
}