using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTween : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.18f;
    [SerializeField] private Ease hoverEase = Ease.OutBack;

    [Header("Click")]
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.2f;
    [SerializeField] private Ease pressEase = Ease.OutCubic;
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    [Header("Click Jolt")]
    [SerializeField] private float joltStrength = 4f;
    [SerializeField] private float joltDuration = 0.18f;
    [SerializeField] private int joltVibrato = 8;

    [Header("Behavior")]
    [SerializeField] private bool useUnscaledTime = true;

    private Button button;
    private RectTransform rectTransform;

    private Vector3 normalScale;
    private Quaternion normalRotation;

    private Tween scaleTween;
    private Tween rotationTween;

    private bool isHovered;
    private bool isSelected;
    private bool isPressed;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();

        normalScale = rectTransform.localScale;
        normalRotation = rectTransform.localRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanAnimate())
            return;

        isHovered = true;
        AnimateHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (!isPressed)
            AnimateNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAnimate() || eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressed = true;
        AnimatePressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed)
            return;

        isPressed = false;

        if (CanAnimate())
            AnimateRelease();
        else
            AnimateNormal();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!CanAnimate())
            return;

        isSelected = true;
        AnimateHover();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;

        if (!isHovered && !isPressed)
            AnimateNormal();
    }

    private void AnimateHover()
    {
        scaleTween?.Kill();

        scaleTween = rectTransform
            .DOScale(normalScale * hoverScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(useUnscaledTime);
    }

    private void AnimatePressed()
    {
        scaleTween?.Kill();
        rotationTween?.Kill();

        rectTransform.localRotation = normalRotation;

        scaleTween = rectTransform
            .DOScale(normalScale * pressedScale, pressDuration)
            .SetEase(pressEase)
            .SetUpdate(useUnscaledTime);

        rotationTween = rectTransform
            .DOShakeRotation(
                joltDuration,
                new Vector3(0f, 0f, joltStrength),
                joltVibrato,
                45f,
                true
            )
            .SetUpdate(useUnscaledTime);
    }

    private void AnimateRelease()
    {
        scaleTween?.Kill();

        Vector3 targetScale =
            isHovered || isSelected
                ? normalScale * hoverScale
                : normalScale;

        scaleTween = rectTransform
            .DOScale(targetScale, releaseDuration)
            .SetEase(releaseEase)
            .SetUpdate(useUnscaledTime);

        rotationTween?.Kill();

        rotationTween = rectTransform
            .DOLocalRotateQuaternion(normalRotation, releaseDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(useUnscaledTime);
    }

    private void AnimateNormal()
    {
        scaleTween?.Kill();
        rotationTween?.Kill();

        scaleTween = rectTransform
            .DOScale(normalScale, hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(useUnscaledTime);

        rotationTween = rectTransform
            .DOLocalRotateQuaternion(normalRotation, hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(useUnscaledTime);
    }

    private bool CanAnimate()
    {
        return button != null &&
               button.interactable &&
               gameObject.activeInHierarchy;
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        rotationTween?.Kill();

        if (rectTransform != null)
        {
            rectTransform.localScale = normalScale;
            rectTransform.localRotation = normalRotation;
        }

        isHovered = false;
        isSelected = false;
        isPressed = false;
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
        rotationTween?.Kill();
    }
}