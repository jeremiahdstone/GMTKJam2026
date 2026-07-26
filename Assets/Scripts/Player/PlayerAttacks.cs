using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttacks : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("Targeting")]
    [SerializeField] private float clickSelectionRadius = 1f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Bat Explosion")]
    [SerializeField] private float batExplosionDamage = 8f;
    [SerializeField] private float batExplosionRadius = 3f;
    [SerializeField] private GameObject batExplosionEffect;

    private readonly HashSet<Enemy> highlightedEnemies = new();
    private readonly HashSet<Enemy> enemiesCurrentlyInRange = new();

    private PlayerManager manager;

    public float biteTimer;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
        biteTimer = 0;
    }

    private void Update()
    {
        if (biteTimer > 0)
            biteTimer -= Time.deltaTime;

        UpdateBiteRangeHighlights();
    }

    public void BiteAttack(Vector2 mousePosition)
    {
        if (biteTimer > 0)
            return;

        if(manager.playerMovement.batForm)
         return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            mousePosition,
            clickSelectionRadius,
            damageableLayers
        );

        Collider2D closestCollider = null;
        IDamageable closestDamageable = null;

        float closestMouseDistanceSqr = Mathf.Infinity;
        float biteRange = playerStats.GetStat(PlayerStat.BiteRange);
        float biteRangeSqr = biteRange * biteRange;

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable ??= hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            Vector2 targetPosition = hit.transform.position;

            // Target must be within bite range of the player.
            float playerDistanceSqr =
                ((Vector2)transform.position - targetPosition).sqrMagnitude;

            if (playerDistanceSqr > biteRangeSqr)
                continue;

            // Choose the valid target closest to the mouse click.
            float mouseDistanceSqr =
                (mousePosition - targetPosition).sqrMagnitude;

            if (mouseDistanceSqr < closestMouseDistanceSqr)
            {
                closestMouseDistanceSqr = mouseDistanceSqr;
                closestCollider = hit;
                closestDamageable = damageable;
            }
        }

        if (closestCollider == null || closestDamageable == null)
            return;

        StartCoroutine(DoBite(closestCollider, closestDamageable));

        Debug.Log($"Bite attack executed on {closestCollider.name}");

        biteTimer = playerStats.GetStat(PlayerStat.BiteCooldown);

        if (playerStats.HasUpgrade<DoubleBiteUpgrade>())
        {
            int level = playerStats.GetUpgrade<DoubleBiteUpgrade>().level;

            if (Random.value < 0.1f * level)
            {
                biteTimer = 0;
                Debug.Log("Double Bite triggered! Cooldown reset.");
            }
        }
    }

    private IEnumerator DoBite(
        Collider2D targetCollider,
        IDamageable damageable)
    {
        float initialAnimSpeed = manager.anim.speed;
        float speedMult =
            playerStats.GetStat(PlayerStat.BiteSpeedMultiplier);

        manager.anim.speed = speedMult;
        manager.anim.SetTrigger("Bite");

        if (manager.sr != null)
        {
            manager.sr.flipX =
                targetCollider.transform.position.x < transform.position.x;
        }

        Vector3 targetPosition = targetCollider.transform.position;

        transform
            .DOMove(targetPosition, 0.5f / speedMult)
            .SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.4f / speedMult);

        if (targetCollider != null)
        {
            damageable.Damage(
                playerStats.GetStat(PlayerStat.BiteDamage),
                this.transform
            );
        }

        CameraShake.Instance.Shake(0.5f);

        manager.anim.speed = initialAnimSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            playerStats != null
                ? playerStats.GetStat(PlayerStat.BiteRange)
                : 0f
        );
    }

    private void UpdateBiteRangeHighlights()
    {
        float biteRange = playerStats.GetStat(PlayerStat.BiteRange);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            biteRange,
            damageableLayers
        );

        enemiesCurrentlyInRange.Clear();

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy == null)
                continue;

            enemiesCurrentlyInRange.Add(enemy);

            if (highlightedEnemies.Add(enemy))
            {
                enemy.SetBiteRangeHighlight(true);
            }
        }

        highlightedEnemies.RemoveWhere(enemy =>
        {
            if (enemy == null)
                return true;

            if (enemiesCurrentlyInRange.Contains(enemy))
                return false;

            enemy.SetBiteRangeHighlight(false);
            return true;
        });
    }

    public void BatExplosion()
    {
        if (!playerStats.HasUpgrade<BatExplosionUpgrade>())
            return;

        BatExplosionUpgrade upgrade =
            playerStats.GetUpgrade<BatExplosionUpgrade>();

        float damage = batExplosionDamage + ((upgrade.level-1) * 4f);
        float radius = batExplosionRadius + ((upgrade.level-1) * 0.25f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            damageableLayers
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.Damage(damage, transform);
            }
        }

        if (batExplosionEffect != null)
        {
            Instantiate(
                batExplosionEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // Slightly stronger shake as it levels up.
        CameraShake.Instance?.Shake(
            0.75f + 0.1f * upgrade.level
        );
    }
}