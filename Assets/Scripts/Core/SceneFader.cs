using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;

    [SerializeField] private Ease fadeOutEase = Ease.InOutSine;
    [SerializeField] private Ease fadeInEase = Ease.InOutSine;

    [Header("Behavior")]
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private bool persistBetweenScenes = true;

    private Image image;
    private Tween activeTween;
    private bool isTransitioning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        image = GetComponentInChildren<Image>();

        if (image == null)
        {
            Debug.LogError(
                "SceneFader requires an Image component in its children.",
                this
            );

            return;
        }

        // Make sure the fade image is rendered above other UI.
        image.transform.SetAsLastSibling();

        if (fadeInOnStart)
            SetAlpha(1f);
        else
            SetAlpha(0f);
    }

    private void Start()
    {
        if (fadeInOnStart)
            FadeIn();
    }

    public void FadeToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene name cannot be empty.", this);
            return;
        }

        StartCoroutine(FadeToSceneCoroutine(sceneName));
    }

    public void FadeToScene(int sceneBuildIndex)
    {
        if (isTransitioning)
            return;

        StartCoroutine(FadeToSceneCoroutine(sceneBuildIndex));
    }

    private IEnumerator FadeToSceneCoroutine(string sceneName)
    {
        isTransitioning = true;

        yield return FadeOutCoroutine();

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(sceneName);

        while (!loadOperation.isDone)
            yield return null;

        yield return null;

        FadeIn(() => isTransitioning = false);
    }

    private IEnumerator FadeToSceneCoroutine(int sceneBuildIndex)
    {
        isTransitioning = true;

        yield return FadeOutCoroutine();

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(sceneBuildIndex);

        while (!loadOperation.isDone)
            yield return null;

        yield return null;

        FadeIn(() => isTransitioning = false);
    }

    private IEnumerator FadeOutCoroutine()
    {
        activeTween?.Kill();

        image.raycastTarget = true;

        bool completed = false;

        activeTween = image
            .DOFade(1f, fadeOutDuration)
            .SetEase(fadeOutEase)
            .SetUpdate(true)
            .OnComplete(() => completed = true);

        while (!completed)
            yield return null;
    }

    public void FadeIn(System.Action onComplete = null)
    {
        activeTween?.Kill();

        image.raycastTarget = true;

        activeTween = image
            .DOFade(0f, fadeInDuration)
            .SetEase(fadeInEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                image.raycastTarget = false;
                activeTween = null;
                onComplete?.Invoke();
            });
    }

    public void FadeOut(System.Action onComplete = null)
    {
        activeTween?.Kill();

        image.raycastTarget = true;

        activeTween = image
            .DOFade(1f, fadeOutDuration)
            .SetEase(fadeOutEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                activeTween = null;
                onComplete?.Invoke();
            });
    }

    private void SetAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;

        image.raycastTarget = alpha > 0f;
    }

    private void OnDestroy()
    {
        activeTween?.Kill();

        if (instance == this)
            instance = null;
    }
}