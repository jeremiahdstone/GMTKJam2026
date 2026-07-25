using UnityEngine;
using Pathfinding;
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

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Seeker seeker;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] public Transform target;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateTime = 0.25f;
    [SerializeField] private float nextWaypointDistance = 0.2f;
    [SerializeField] private float stoppingDistance = 0.15f;

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


    private Path path;
    private int currentWaypoint;
    private float nextPathUpdateTime;
    private float lastHorizontalDirection = 1f;



    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalSpriteColor = spriteRenderer.color;

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Objective").transform;
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
        currentWaypoint = 0;
        nextPathUpdateTime = 0f;
        lastHorizontalDirection = 1f;

        currentSpeed = speed;
        currentHealth = maxHealth;
        UpdatePath();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public virtual void Move()
    {
        if (
            target == null ||
            path == null ||
            path.vectorPath == null ||
            path.vectorPath.Count == 0
        )
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        while (
            currentWaypoint < path.vectorPath.Count &&
            Vector2.Distance(
                rb.position,
                path.vectorPath[currentWaypoint]
            ) <= nextWaypointDistance
        )
        {
            currentWaypoint++;
        }

        if (
            currentWaypoint >= path.vectorPath.Count ||
            Vector2.Distance(rb.position, target.position)
                <= stoppingDistance
        )
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 waypoint = path.vectorPath[currentWaypoint];

        Vector2 direction =
            (waypoint - rb.position).normalized;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            lastHorizontalDirection = Mathf.Sign(direction.x);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = lastHorizontalDirection < 0f;
        }

        rb.linearVelocity = direction * currentSpeed;
    }

    private void OnDisable()
    {
        biteRangePulse?.Kill();
        biteRangePulse = null;

        if (spriteRenderer != null)
            spriteRenderer.color = originalSpriteColor;

        if (seeker != null)
        {
            seeker.CancelCurrentPathRequest();
        }

        if (path != null)
        {
            path.Release(this);
            path = null;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdatePath()
    {
        if (target == null || seeker == null)
            return;

        if (Time.time < nextPathUpdateTime)
            return;

        // Do not start another request while one is still processing.
        if (!seeker.IsDone())
            return;

        nextPathUpdateTime = Time.time + pathUpdateTime;

        seeker.StartPath(
            rb.position,
            target.position,
            OnPathComplete
        );
    }

    private void OnPathComplete(Path newPath)
    {
        if (!isActiveAndEnabled)
            return;

        if (newPath.error)
        {
            Debug.LogWarning(
                $"{name} failed to calculate a path: {newPath.errorLog}"
            );

            return;
        }

        // Release the previous path back into the path pool.
        if (path != null)
        {
            path.Release(this);
        }

        path = newPath;
        path.Claim(this);

        currentWaypoint = 0;
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;

        //display damage number
        GameObject effect = Instantiate(DamageNumEffect, transform.position, Quaternion.identity);

        effect.GetComponent<DamageEffect>().DisplayDamage(damage, false);



        if (currentHealth <= 0)
        {
            Die(1);
        }
        else
        {
            Instantiate(bloodSpillEffect, transform.position, bloodSpillEffect.transform.rotation);
        }
    }

    public void Die(float dropMultiplier)
    {
        Instantiate(deathEffect, transform.position, deathEffect.transform.rotation);

        //Drop items
        int numDrops = Mathf.RoundToInt(Random.Range(RandomNumDrops.x, RandomNumDrops.y) * dropMultiplier);

            for (int i = 0; i < numDrops; i++)
            {
                Instantiate(DeathDrop, transform.position, Quaternion.identity);
            }


        LevelDirector.instance.NotifyEnemyRemoved(this);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Objective")
        {
            GameSession.instance.DamageCastle(attackDamage);

            // TEMPORARY: for now, when they reach it, they just die
            Die(0);
        }
    }
}
