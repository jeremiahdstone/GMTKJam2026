using UnityEngine;
using DG.Tweening;

public class AOEBehavior : MonoBehaviour
{
    [Header("AOE Visual")]
    [SerializeField] private float aoeFadeDuration = 0.25f;
    [SerializeField] private float aoeTargetAlpha = 1f;
    [SerializeField] private bool showInCombat = false;
    [SerializeField] private bool showInBuildPhase = true;

    protected virtual void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd += OnWaveEnd;
            GameEventManager.instance.OnWaveStart += OnWaveStart;
        }

        
        UpdateVisibilityForCurrentPhase();
    }

    protected void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd -= OnWaveEnd;
            GameEventManager.instance.OnWaveStart -= OnWaveStart;
        }
    }

    public void RefreshVisual(float range)
    {
        Vector3 targetScale = new Vector3(range * 2f, range * 2f, 1f);

        if (transform.localScale == targetScale)
            return;

        transform.DOScale(targetScale, aoeFadeDuration)
            .SetEase(Ease.Linear);
    }

    public void Show()
    {
        SpriteRenderer sr = transform.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            transform.gameObject.SetActive(true);
            return;
        }

        Material mat = sr.material;

        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", 0f);
        else
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        transform.gameObject.SetActive(true);

        if (mat.HasProperty("_Alpha"))
            mat.DOFloat(aoeTargetAlpha, "_Alpha", aoeFadeDuration).SetEase(Ease.Linear);
        else
            sr.DOFade(aoeTargetAlpha, aoeFadeDuration).SetEase(Ease.Linear);
    }

    public void Hide()
    {
        if (transform == null)
            return;
        SpriteRenderer sr = transform.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            transform.gameObject.SetActive(false);
            return;
        }

        Material mat = sr.material;

        if (mat.HasProperty("_Alpha"))
        {
            mat.DOFloat(0f, "_Alpha", aoeFadeDuration).SetEase(Ease.Linear)
                .OnComplete(() => transform.gameObject.SetActive(false));
        }
        else
        {
            sr.DOFade(0f, aoeFadeDuration).SetEase(Ease.Linear)
                .OnComplete(() => transform.gameObject.SetActive(false));
        }
    }

    private void UpdateVisibilityForCurrentPhase()
    {
        if (GameSession.instance == null)
            return;

        bool shouldShow =
            (GameSession.instance.phase == Phase.build && showInBuildPhase) ||
            (GameSession.instance.phase == Phase.combat && showInCombat);

        if (shouldShow)
            Show();
        else
            Hide();
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
