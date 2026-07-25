using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageEffect : MonoBehaviour
{
    [SerializeField]private TMP_Text text;
    private Sequence sequence;

    [Header("Visuals")]
    [SerializeField] private TMP_FontAsset regularFont;
    [SerializeField] private TMP_FontAsset critFont;
    [SerializeField] private Color regularColor = Color.white;
    [SerializeField] private Color critColor = Color.yellow;

    [Header("Normal Hit")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float normalRiseHeight = 0.8f;
    [SerializeField] private float normalFallDistance = 0.3f;

    [Header("Critical Hit")]
    [SerializeField] private float critScale = 1.5f;
    [SerializeField] private float critRiseHeight = 1.2f;
    [SerializeField] private float critFallDistance = 0.45f;

    [Header("Timing")]
    [SerializeField] private float appearDuration = 0.12f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.25f;

    private void Awake()
    {
    }

    public void DisplayDamage(float damage, bool isCrit)
    {
        sequence?.Kill();

        text.text = Mathf.RoundToInt(damage).ToString();
        text.font = isCrit ? critFont : regularFont;
        text.color = isCrit ? critColor : regularColor;

        float targetScale = isCrit ? critScale : normalScale;
        float riseHeight = isCrit ? critRiseHeight : normalRiseHeight;
        float fallDistance = isCrit ? critFallDistance : normalFallDistance;

        Vector3 startPosition = transform.position;
        Vector3 peakPosition = startPosition + Vector3.up * riseHeight;
        Vector3 endPosition = peakPosition - Vector3.up * fallDistance;

        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;

        Color startingColor = text.color;
        startingColor.a = 1f;
        text.color = startingColor;

        sequence = DOTween.Sequence();

        // Pop into existence.
        sequence.Append(
            transform
                .DOScale(targetScale, appearDuration)
                .SetEase(Ease.OutBack)
        );

        // Fly upward while growing slightly.
        sequence.Join(
            transform
                .DOMove(peakPosition, riseDuration)
                .SetEase(Ease.OutCubic)
        );

        sequence.Join(
            transform
                .DOScale(targetScale * 1.15f, riseDuration)
                .SetEase(Ease.OutSine)
        );

        // Fall back down.
        sequence.Append(
            transform
                .DOMove(endPosition, fallDuration)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            transform
                .DOScale(targetScale * 0.85f, fallDuration)
                .SetEase(Ease.InSine)
        );

        // Fade near the end of the fall.
        sequence.Join(
            text
                .DOFade(0f, fadeDuration)
                .SetDelay(Mathf.Max(0f, fallDuration - fadeDuration))
        );

        if (isCrit)
        {
            // Quick punch and slight rotation for critical hits.
            sequence.Insert(
                0f,
                transform.DOPunchScale(
                    Vector3.one * 0.45f,
                    0.25f,
                    6,
                    0.7f
                )
            );

            float rotationDirection = Random.value < 0.5f ? -1f : 1f;

            sequence.Insert(
                appearDuration,
                transform.DORotate(
                    new Vector3(0f, 0f, rotationDirection * 12f),
                    riseDuration
                ).SetEase(Ease.OutSine)
            );
        }

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}