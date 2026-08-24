using UnityEngine;
using DG.Tweening;
using System.Collections;
public enum Team
{
    good,
    bad
}

public class Enemy : MonoBehaviour, IDamageable, IFreezable
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
    [SerializeField] public Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material materialInstance;

    [Header("Movement")]
    [Tooltip("Assign a component that implements IMovementModule (e.g. BasicMovementModule)")]
    [SerializeField] private UnityEngine.MonoBehaviour movementModuleBehaviour;
    public IMovementModule movementModule;



    [Header("In-Game Stats")]
    public float currentSpeed;
    public float currentHealth;
    public int currentAttackDamage;

    [Header("Visuals")]
    [SerializeField] private GameObject bloodSpillEffect;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject DamageNumEffect;
    [SerializeField] private GameObject iceBreakParticlePrefab;

    [Header("Bite Range Outline")]
    [SerializeField] private float fullOutlineDistance = 2f;
    [SerializeField] private float maxOutlineDistance = 5f;
    [SerializeField] private float minimumAlpha = 0.5f;

    private MaterialPropertyBlock materialPropertyBlock;
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");

    [SerializeField] private Color outlineColor = new Color(1f, 0.38f, 0.34f, 1f);

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
    private bool isDead;

    //freeze stuff
    private Coroutine freezeCoroutine;
    private bool isFrozen;

    private AudioSource audioSource;





    public virtual void Awake()
    {
        currentAttackDamage = attackDamage;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();

        materialPropertyBlock = new MaterialPropertyBlock();

        // bind movement module
        movementModule = movementModuleBehaviour as IMovementModule;
        if (movementModule == null)
            movementModule = GetComponent<IMovementModule>();

        movementModule?.Initialize(this);

        if (spriteRenderer != null)
        {
            originalVisualScale = spriteRenderer.transform.localScale;
        }

        if (materialInstance != null)
        {
            spriteRenderer.material = materialInstance;
        }

        CalculateStats(GameSession.instance.run.day);

    }

    public virtual void CalculateStats(int day)
    {
        currentHealth = maxHealth + (maxHealth * day * healthIncreasePercentagePerDay);
        currentSpeed = speed + (speed * day * speedIncreasePercentagePerDay);
        currentAttackDamage = Mathf.RoundToInt(attackDamage + (attackDamage * day * damageIncreasePercentagePerDay));
    }



    public void SetBiteRangeHighlight(bool highlighted, Vector3 playerPosition, float biteRange = 5f)
    {
        if (spriteRenderer == null)
            return;

        float alpha = 0f;

        if (highlighted)
        {
            float distance = Vector3.Distance(
                transform.position,
                playerPosition
            );

            alpha = Mathf.InverseLerp(
                biteRange,
                fullOutlineDistance,
                distance
            );

            // lerp from minimum alpha to 1
            alpha = alpha * (1f - minimumAlpha) + minimumAlpha;
        }

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);

        Color color = outlineColor;
        color.a = alpha;

        materialPropertyBlock.SetColor(OutlineColorID, color);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    // For when object pooling is called.
    public virtual void OnEnable()
    {
        isDead = false;

        // Recalculate stats on enable so day-based scaling is applied
        // (important for object pooling where Awake may have run earlier)
        if (GameSession.instance != null)
            CalculateStats(GameSession.instance.run.day);

        movementModule?.OnEnableModule();
    }

    private void FixedUpdate()
    {
        movementModule?.Move();
    }


    // Movement is delegated to an IMovementModule implementation.

    private void OnDisable()
    {
        hitBounceTween?.Kill();

        hitBounceTween = null;

        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = originalVisualScale;
        }

        movementModule?.OnDisableModule();

        if(isFrozen) Unfreeze();
    }


    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public virtual void Damage(float damage, GameObject attacker = null)
    {
        if (isDead)
            return;

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
        if (isDead)
            return;

        isDead = true;

        GameEventManager.instance.EnemyDeath(this.gameObject, attacker?.gameObject);

        Instantiate(deathEffect, transform.position, deathEffect.transform.rotation);

        //Drop items
        int numDrops = Mathf.RoundToInt(Random.Range(RandomNumDrops.x, RandomNumDrops.y) * dropMultiplier);

        for (int i = 0; i < numDrops; i++)
        {
            PoolManager.instance.Spawn(DeathDrop, transform.position, Quaternion.identity);
        }

        if (notifyDirector && LevelDirector.instance != null)
        {
            LevelDirector.instance.NotifyEnemyRemoved(this);
        }

        PoolManager.instance?.Release(gameObject);
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
            GameSession.instance.DamageCastle(currentAttackDamage);

            // TEMPORARY: for now, when they reach it, they just die
            Die(0, true, collision.gameObject);
        }
    }

    // FREEZE FUNCTIONS

    public virtual void Freeze(float cooldown, GameObject attacker = null)
    {
        // If already frozen, restart the timer
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        isFrozen = true;

        // Stop movement
        currentSpeed = 0;

        // Pause animation
        if (anim != null)
        {
            anim.speed = 0;
            anim.enabled = false;
        }

        // Enable color swap
        if (spriteRenderer != null)
        {
            spriteRenderer.material.SetFloat("_EnableColorSwap", 1f);
        }

        // Start timer to call Unfreeze
        freezeCoroutine = StartCoroutine(FreezeCoroutine(cooldown));
    }

    private IEnumerator FreezeCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        Unfreeze();
    }

    public virtual void Unfreeze()
    {
        isFrozen = false;
        freezeCoroutine = null;

        // Resume movement
        currentSpeed = speed;

        // Restore animation
        if (anim != null)
        {
            anim.enabled = true;
            anim.speed = 1;
        }

        // Disable color swap
        if (spriteRenderer != null)
        {
            spriteRenderer.material.SetFloat("_EnableColorSwap", 0f);
        }

        // Spawn ice break particles
        if (iceBreakParticlePrefab != null)
        {
            Instantiate(
                iceBreakParticlePrefab,
                transform.position,
                iceBreakParticlePrefab.transform.rotation
            );
        }
    }
}
