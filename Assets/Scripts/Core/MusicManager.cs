using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource stingSource;

    [Header("Build Phase Music")]
    [SerializeField] private AudioClip[] buildMusic;

    [Header("Combat Phase Music")]
    [SerializeField] private AudioClip[] combatMusic;

    [Tooltip("Each sting should correspond to the combat track at the same index.")]
    [SerializeField] private AudioClip[] combatEndingStings;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float buildFadeOutDuration = 1f;
    [SerializeField] private bool loopMusic = true;

    [Header("Game Over Music")]
    [SerializeField] private float gameOverPitch = 0.2f;
    [SerializeField] private float gameOverSlowDuration = 1.5f;
    [SerializeField] private Ease gameOverSlowEase = Ease.InCubic;

    private Tween pitchTween;

    private Phase currentMusicPhase;
    private int currentTrackIndex = -1;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Optional if the manager should survive scene changes.
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Starts a random track belonging to the supplied phase.
    /// </summary>
    public void startMusic(Phase phase)
    {
        StopFadeCoroutine();

        pitchTween?.Kill();
        musicSource.pitch = 1f;

        AudioClip[] availableTracks = GetTracksForPhase(phase);

        if (availableTracks == null || availableTracks.Length == 0)
        {
            Debug.LogWarning($"No music tracks assigned for the {phase} phase.");
            return;
        }

        currentMusicPhase = phase;
        currentTrackIndex = GetRandomTrackIndex(availableTracks);

        musicSource.Stop();
        musicSource.clip = availableTracks[currentTrackIndex];
        musicSource.volume = musicVolume;
        musicSource.loop = loopMusic;
        musicSource.Play();
    }

    /// <summary>
    /// Starts music based on GameSession's current phase.
    /// </summary>
    public void startCurrentPhaseMusic()
    {
        startMusic(GameSession.instance.phase);
    }

    /// <summary>
    /// Combat music stops with its corresponding ending sting.
    /// Build music fades out normally.
    /// </summary>
    public void stopMusic()
    {
        StopFadeCoroutine();

        if (!musicSource.isPlaying)
            return;

        switch (currentMusicPhase)
        {
            case Phase.combat:
                StopCombatMusicWithSting();
                break;

            case Phase.build:
                fadeCoroutine = StartCoroutine(FadeOutBuildMusic());
                break;
        }
    }

    private void StopCombatMusicWithSting()
    {
        musicSource.Stop();
        musicSource.clip = null;

        if (currentTrackIndex < 0 ||
            combatEndingStings == null ||
            currentTrackIndex >= combatEndingStings.Length)
        {
            Debug.LogWarning(
                $"No ending sting corresponds to combat track index {currentTrackIndex}."
            );

            currentTrackIndex = -1;
            return;
        }

        AudioClip endingSting = combatEndingStings[currentTrackIndex];

        if (endingSting != null)
        {
            stingSource.Stop();
            stingSource.volume = musicVolume;
            stingSource.PlayOneShot(endingSting);
        }

        currentTrackIndex = -1;
    }

    private IEnumerator FadeOutBuildMusic()
    {
        float startingVolume = musicSource.volume;
        float elapsedTime = 0f;

        if (buildFadeOutDuration <= 0f)
        {
            musicSource.Stop();
            musicSource.clip = null;
            musicSource.volume = musicVolume;
            currentTrackIndex = -1;
            fadeCoroutine = null;
            yield break;
        }

        while (elapsedTime < buildFadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = elapsedTime / buildFadeOutDuration;
            musicSource.volume = Mathf.Lerp(startingVolume, 0f, progress);

            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = musicVolume;

        currentTrackIndex = -1;
        fadeCoroutine = null;
    }

    private AudioClip[] GetTracksForPhase(Phase phase)
    {
        return phase switch
        {
            Phase.combat => combatMusic,
            Phase.build => buildMusic,
            _ => null
        };
    }

    private int GetRandomTrackIndex(AudioClip[] tracks)
    {
        if (tracks.Length == 1)
            return 0;

        int randomIndex;

        // Prevent playing the same numbered track twice consecutively
        // when restarting music for the same phase.
        do
        {
            randomIndex = Random.Range(0, tracks.Length);
        }
        while (randomIndex == currentTrackIndex);

        return randomIndex;
    }

    private void StopFadeCoroutine()
    {
        if (fadeCoroutine == null)
            return;

        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;

        musicSource.volume = musicVolume;
    }

    public void SlowMusicForGameOver()
    {
        if (musicSource == null || !musicSource.isPlaying)
            return;

        pitchTween?.Kill();

        pitchTween = musicSource
            .DOPitch(gameOverPitch, gameOverSlowDuration)
            .SetEase(gameOverSlowEase)
            .SetUpdate(true);
    }
}