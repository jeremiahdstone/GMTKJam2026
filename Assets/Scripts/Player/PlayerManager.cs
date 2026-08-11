using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [Header("Visuals")]
    public SpriteRenderer sr;
    public Animator anim;
    public GameObject SmokePuffEffect;

    [Header("Hit Bounce")]
    [SerializeField] private float hitSquashAmount = 0.6f;
    [SerializeField] private float hitStretchAmount = 1.45f;
    [SerializeField] private float hitSquashDuration = 0.045f;
    [SerializeField] private float hitRecoverDuration = 0.3f;
    [SerializeField] private Ease hitRecoverEase = Ease.OutElastic;

    [Header("Collision")]
    public Collider2D col;

    public PlayerAttacks playerAttacks { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public PlayerMovement playerMovement { get; private set; }
    public PlayerStats playerStats { get; private set; }

    [Header("Blood Collection")]
    PointEffector2D pointEffector;
    bool canCollectBlood = true;

    private Tween hitBounceTween;
    private Vector3 originalVisualScale;





    void Awake()
    {
        playerAttacks = GetComponent<PlayerAttacks>();
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
        pointEffector = GetComponentInChildren<PointEffector2D>();

        if (sr != null)
            originalVisualScale = sr.transform.localScale;
    }

    public void Update()
    {
        if (GameSession.instance.run.bloodCount < GameSession.instance.run.maxBloodCount)
        {
            canCollectBlood = true;
        }
        else
        {
            canCollectBlood = false;
        }
        pointEffector.enabled = canCollectBlood;
    }


    public void Damage(float damage, GameObject attacker = null)
    {
        GameSession.instance.SubtractBlood(
            Mathf.RoundToInt(damage)
        );

        PlayHitBounce();
    }


    private void PlayHitBounce()
    {
        if (sr == null)
            return;

        Transform visual = sr.transform;

        hitBounceTween?.Kill();

        visual.localScale =
            originalVisualScale;

        Vector3 squashedScale =
            new Vector3(
                originalVisualScale.x *
                hitStretchAmount,

                originalVisualScale.y *
                hitSquashAmount,

                originalVisualScale.z
            );

        Sequence sequence =
            DOTween.Sequence();

        sequence.Append(
            visual.DOScale(
                squashedScale,
                hitSquashDuration
            )
            .SetEase(Ease.OutCubic)
        );

        sequence.Append(
            visual.DOScale(
                originalVisualScale,
                hitRecoverDuration
            )
            .SetEase(hitRecoverEase)
        );

        hitBounceTween =
            sequence;
    }


    private void OnDisable()
    {
        hitBounceTween?.Kill();
        hitBounceTween = null;

        if (sr != null)
            sr.transform.localScale = originalVisualScale;
    }
}