using UnityEngine;
using DG.Tweening;
public enum Team
{
    good,
    bad
}

public class Enemy : MonoBehaviour, IDamageable
{
    public Team team = Team.bad;
    [Header("Stats")]
    [SerializeField] public int cost = 1;
    [SerializeField] public float maxHealth = 50f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float healthIncreasePercentagePerDay = 0.1f;
    [SerializeField] private float speedIncreasePercentagePerDay = 0f;
    [SerializeField] private float damageIncreasePercentagePerDay = 0.15f;
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [Tooltip("Assign a component that implements IMovementModule (e.g. BasicMovementModule)")]
    [SerializeField] private UnityEngine.MonoBehaviour movementModuleBehaviour;
    public IMovementModule movementModule;

    

    [Header("In-Game Stats")]
    public float currentSpeed;
    public float currentHealth;

    [Header("Visuals")]
    [SerializeField] private GameObject bloodSpillEffect;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject DamageNumEffect;

    private Tween biteRangePulse;
    private Color originalSpriteColor;

    [Header("Drops")]
    [SerializeField] private GameObject DeathDrop;
    [SerializeField] private Vector2Int RandomNumDrops = new Vector2Int(1, 3);

    [Header("Hit Bounce")]
    [SerializeField] private float hitSquashAmount = 0.6f;
    [SerializeField] private float hitStretchAmount = 1.45f;
    [SerializeField] private float hitSquashDuration = 0.045f;
    [SerializeField] private float hitRecoverDuration = 0.3f;
    [SerializeField] private Ease hitRecoverEase = Ease.OutElastic;

    private Tween hitBounceTween;
    private Vector3 originalVisualScale;


    

    private AudioSource audioSource;



    

    public virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // bind movement module
        movementModule = movementModuleBehaviour as IMovementModule;
        if (movementModule == null)
            movementModule = GetComponent<IMovementModule>();

        movementModule?.Initialize(this);

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
            originalVisualScale = spriteRenderer.transform.localScale;
        }

        

        CalculateStats(GameSession.instance.run.day);
    }

    public virtual void CalculateStats(int day)
    {
        currentHealth = maxHealth + (maxHealth * day * healthIncreasePercentagePerDay);
        currentSpeed = speed + (speed * day * speedIncreasePercentagePerDay);
        attackDamage = Mathf.RoundToInt(attackDamage + (attackDamage * day * damageIncreasePercentagePerDay));
    }



    public void SetBiteRangeHighlight(bool highlighted)
    {
        if (spriteRenderer == null)
            return;

        // Stop any color tween already affecting this SpriteRenderer.
        spriteRenderer.DOKill();

        if (highlighted)
        {
            ColorUtility.TryParseHtmlString("#FF6157", out Color paletteRed);

            biteRangePulse = spriteRenderer
                .DOColor(paletteRed, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            biteRangePulse = null;

            spriteRenderer
                .DOColor(originalSpriteColor, 0.15f)
                .SetEase(Ease.OutSine);
        }
    }

    // For when object pooling is called.
    public virtual void OnEnable()
    {
        currentSpeed = speed;
        currentHealth = maxHealth;

        movementModule?.OnEnableModule();
    }

    private void FixedUpdate()
    {
        movementModule?.Move();
    }

    
    // Movement is delegated to an IMovementModule implementation.

    private void OnDisable()
    {
        biteRangePulse?.Kill();
        hitBounceTween?.Kill();

        biteRangePulse = null;
        hitBounceTween = null;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
            spriteRenderer.transform.localScale = originalVisualScale;
        }

        movementModule?.OnDisableModule();
    }

    
    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public virtual void Damage(float damage, GameObject attacker = null)
    {
        GameEventManager.instance.EnemyHit(this.gameObject, attacker?.gameObject);
        currentHealth -= damage;

        PlayHitBounce();

        //display damage number
        GameObject effect = Instantiate(DamageNumEffect, transform.position, Quaternion.identity);

        effect.GetComponent<DamageEffect>().DisplayDamage(damage, false);



        if (currentHealth <= 0)
        {
            if (attacker != null && attacker.tag == "Player")
            {
                //bite attack, give 2x blood
                // if you add an extra modifier for blood drops from bites it should go here
                float biteBloodMultiplier = PlayerStats.Instance.GetStat(PlayerStat.BiteBloodMultipler);
                Die(2 * biteBloodMultiplier);
            }
            else
            {
                // any other death
                // if you add an extra modifier for blood drops in general it should go here (and above also ig)
                Die(1, true, attacker?.gameObject);
            }

        }
        else
        {
            Instantiate(bloodSpillEffect, transform.position, bloodSpillEffect.transform.rotation);
        }
    }

    public virtual void Die(float dropMultiplier, bool notifyDirector = true, GameObject attacker = null)
    {
        GameEventManager.instance.EnemyDeath(this.gameObject, attacker?.gameObject);

        Instantiate(deathEffect, transform.position, deathEffect.transform.rotation);

        //Drop items
        int numDrops = Mathf.RoundToInt(Random.Range(RandomNumDrops.x, RandomNumDrops.y) * dropMultiplier);

        for (int i = 0; i < numDrops; i++)
        {
            Instantiate(DeathDrop, transform.position, Quaternion.identity);
        }

        if (notifyDirector && LevelDirector.instance != null)
        {
            LevelDirector.instance.NotifyEnemyRemoved(this);
        }

        Destroy(gameObject);
    }

    private void PlayHitBounce()
    {
        if (spriteRenderer == null)
            return;

        Transform visual = spriteRenderer.transform;

        hitBounceTween?.Kill();

        visual.localScale = originalVisualScale;

        Vector3 squashedScale = new Vector3(
            originalVisualScale.x * hitStretchAmount,
            originalVisualScale.y * hitSquashAmount,
            originalVisualScale.z
        );

        Sequence sequence = DOTween.Sequence();

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

        hitBounceTween = sequence;
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Objective")
        {
            GameSession.instance.DamageCastle(attackDamage);

            // TEMPORARY: for now, when they reach it, they just die
            Die(0, true, collision.gameObject);
        }
    }
}
