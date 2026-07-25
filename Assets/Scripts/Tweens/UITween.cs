using UnityEngine;
using DG.Tweening;

public enum UITweenDirection
{
    Top,
    Bottom,
    Left,
    Right
}

[RequireComponent(typeof(RectTransform))]
public class UITween : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private UITweenDirection direction = UITweenDirection.Top;

    [Tooltip("Distance from the visible position to the hidden position.")]
    [SerializeField] private float offscreenDistance = 800f;

    [Header("Open Tween")]
    [SerializeField] private float showDuration = 0.4f;
    [SerializeField] private Ease showEase = Ease.OutCubic;

    [Header("Hide Tween")]
    [SerializeField] private float hideDuration = 0.25f;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    [Header("Behavior")]
    [SerializeField] private bool showOnEnable = true;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool disableAfterHiding = true;

    private RectTransform rectTransform;

    private Vector2 visiblePosition;
    private Vector2 hiddenPosition;

    private Tween activeTween;

    private bool initialized;
    private bool isShowing;
    private bool isHiding;

    public bool IsHiding()
    {
        return isHiding;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();

        if (showOnEnable)
        {
            Show();
        }
    }

    private void Initialize()
    {
        if (initialized)
            return;

        rectTransform = GetComponent<RectTransform>();

        visiblePosition = rectTransform.anchoredPosition;
        hiddenPosition = GetHiddenPosition();

        rectTransform.anchoredPosition = hiddenPosition;

        initialized = true;
    }

    private Vector2 GetHiddenPosition()
    {
        Vector2 offset = direction switch
        {
            UITweenDirection.Top => Vector2.up * offscreenDistance,
            UITweenDirection.Bottom => Vector2.down * offscreenDistance,
            UITweenDirection.Left => Vector2.left * offscreenDistance,
            UITweenDirection.Right => Vector2.right * offscreenDistance,
            _ => Vector2.zero
        };

        return visiblePosition + offset;
    }

    public void Show()
    {
        Initialize();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            return;
        }

        activeTween?.Kill();

        isShowing = true;
        isHiding = false;

        rectTransform.anchoredPosition = hiddenPosition;

        activeTween = rectTransform
            .DOAnchorPos(visiblePosition, showDuration)
            .SetEase(showEase)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                isShowing = false;
                activeTween = null;
            });
    }

    public void Hide()
    {
        Initialize();

        if (!gameObject.activeInHierarchy || isHiding)
            return;

        activeTween?.Kill();

        isShowing = false;
        isHiding = true;

        activeTween = rectTransform
            .DOAnchorPos(hiddenPosition, hideDuration)
            .SetEase(hideEase)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                isHiding = false;
                activeTween = null;

                if (disableAfterHiding)
                {
                    gameObject.SetActive(false);
                }
            });
    }

    public void Toggle()
    {
        if (!gameObject.activeSelf)
        {
            Show();
            return;
        }

        if (isHiding)
            Show();
        else
            Hide();
    }

    public void ShowImmediately()
    {
        Initialize();

        activeTween?.Kill();
        activeTween = null;

        isShowing = false;
        isHiding = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        rectTransform.anchoredPosition = visiblePosition;
    }

    public void HideImmediately()
    {
        Initialize();

        activeTween?.Kill();
        activeTween = null;

        isShowing = false;
        isHiding = false;

        rectTransform.anchoredPosition = hiddenPosition;

        if (disableAfterHiding)
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        activeTween?.Kill();
        activeTween = null;

        isShowing = false;
        isHiding = false;

        if (initialized)
            rectTransform.anchoredPosition = hiddenPosition;
    }

    private void OnDestroy()
    {
        activeTween?.Kill();
    }
}