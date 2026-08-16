using UnityEngine;
using DG.Tweening;

public class AOEBehavior : MonoBehaviour
{
    [Header("AOE Visual")]
    [SerializeField] private float aoeFadeDuration = 0.25f;
    [SerializeField] private float aoeTargetAlpha = 0.5f;
    [SerializeField] private bool showInCombat = false;
    [SerializeField] private bool showInBuildPhase = true;

    private SpriteRenderer spriteRenderer;
    private Material material;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            material = spriteRenderer.material;
    }

    protected virtual void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd += OnWaveEnd;
            GameEventManager.instance.OnWaveStart += OnWaveStart;
        }

        UpdateVisibilityForCurrentPhase(true);
    }

    protected virtual void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd -= OnWaveEnd;
            GameEventManager.instance.OnWaveStart -= OnWaveStart;
        }

        KillVisualTweens();
    }

    public void RefreshVisual(float range)
    {
        Vector3 targetScale = new Vector3(range * 2f, range * 2f, 1f);

        if (transform.localScale == targetScale)
            return;

        transform.DOKill();

        transform.DOScale(targetScale, aoeFadeDuration)
            .SetEase(Ease.Linear);
    }

    public void Show(bool immediate = false)
    {
        if (spriteRenderer == null)
            return;

        KillFadeTween();

        spriteRenderer.enabled = true;

        if (material != null && material.HasProperty("_Alpha"))
        {
            if (immediate)
            {
                material.SetFloat("_Alpha", aoeTargetAlpha);
            }
            else
            {
                material.DOFloat(
                    aoeTargetAlpha,
                    "_Alpha",
                    aoeFadeDuration
                ).SetEase(Ease.Linear);
            }
        }
        else
        {
            if (immediate)
            {
                Color color = spriteRenderer.color;
                color.a = aoeTargetAlpha;
                spriteRenderer.color = color;
            }
            else
            {
                spriteRenderer
                    .DOFade(aoeTargetAlpha, aoeFadeDuration)
                    .SetEase(Ease.Linear);
            }
        }
    }

    public void Hide(bool immediate = false)
    {
        if (spriteRenderer == null)
            return;

        KillFadeTween();

        if (immediate)
        {
            SetAlpha(0f);
            spriteRenderer.enabled = false;
            return;
        }

        if (material != null && material.HasProperty("_Alpha"))
        {
            material.DOFloat(0f, "_Alpha", aoeFadeDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    spriteRenderer.enabled = false;
                });
        }
        else
        {
            spriteRenderer.DOFade(0f, aoeFadeDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    spriteRenderer.enabled = false;
                });
        }
    }

    private void UpdateVisibilityForCurrentPhase(bool immediate = false)
    {
        if (GameSession.instance == null)
            return;

        bool shouldShow =
            (GameSession.instance.phase == Phase.build && showInBuildPhase) ||
            (GameSession.instance.phase == Phase.combat && showInCombat);

        if (shouldShow)
            Show(immediate);
        else
            Hide(immediate);
    }

    private void SetAlpha(float alpha)
    {
        if (material != null && material.HasProperty("_Alpha"))
        {
            material.SetFloat("_Alpha", alpha);
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void KillFadeTween()
    {
        if (material != null && material.HasProperty("_Alpha"))
            DOTween.Kill(material);
        else if (spriteRenderer != null)
            spriteRenderer.DOKill();
    }

    private void KillVisualTweens()
    {
        transform.DOKill();
        KillFadeTween();
    }

    public void OnWaveStart()
    {
        UpdateVisibilityForCurrentPhase();
    }

    public void OnWaveEnd()
    {
        UpdateVisibilityForCurrentPhase();
    }
}