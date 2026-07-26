using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject gameLogo;
    [SerializeField] private AudioSource audioSource;

    [Header("Logo Bob")]
    [SerializeField] private float bobDistance = 8f;
    [SerializeField] private float bobDuration = 2.5f;

    [Header("Music Fade")]
    [SerializeField] private float musicFadeDuration = 1f;

    private Tween logoTween;
    private Tween musicFadeTween;
    private RectTransform logoRect;

    private void Start()
    {
        logoRect = gameLogo.GetComponent<RectTransform>();

        logoTween = logoRect
            .DOAnchorPosY(
                logoRect.anchoredPosition.y + bobDistance,
                bobDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    public void StartGame()
    {
        musicFadeTween?.Kill();

        if (audioSource != null)
        {
            musicFadeTween = audioSource
                .DOFade(0f, musicFadeDuration)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }

        SceneFader.instance.FadeToScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        logoTween?.Kill();
        musicFadeTween?.Kill();
    }
}