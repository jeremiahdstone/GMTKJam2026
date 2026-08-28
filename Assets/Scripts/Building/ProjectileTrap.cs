using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ProjectileTrap : Trap
{
    [Header("Attack")]
    // Range, Cooldown, BurstCount, and BurstDelay are now TrapStats

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    private float cooldownTimer;
    private bool firing;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd += HandleWaveEnd;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd -= HandleWaveEnd;
        }
    }

    private void HandleWaveEnd()
    {
        StopAllCoroutines();
        firing = false;
        cooldownTimer = 0f;
    }


    private void Update()
    {
        if (GameSession.instance.phase != Phase.combat)
            return;

        if (firing)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0)
            return;

        Enemy target = FindNearestEnemy();

        if (target != null)
        {
            Debug.Log("Starting burst at " + target.name);
            StartCoroutine(FireBurst(target));
        }
    }

    private IEnumerator FireBurst(Enemy target)
    {
        firing = true;

        int burstCount = Mathf.RoundToInt(GetStat(TrapStat.BurstCount));
        float burstDelay = GetStat(TrapStat.BurstDelay);

        Vector2 lastDirection = Vector2.zero;
        bool firedSuccessfully = false;

        for (int i = 0; i < burstCount; i++)
        {
            Vector2 direction;

            if (IsTargetValid(target))
            {
                direction = (
                    target.transform.position - firePoint.position
                ).normalized;

                lastDirection = direction;
            }
            else
            {
                // Target died or left the range.
                // Keep firing in the last direction we had.
                direction = lastDirection;
            }

            // We have no valid target and no previous direction.
            if (direction == Vector2.zero)
                break;

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            if (projectile == null)
                break;

            projectile.Initialize(direction, this);

            // Cooldown begins only after a projectile
            // was actually instantiated successfully.
            if (!firedSuccessfully)
            {
                cooldownTimer = GetStat(TrapStat.Cooldown);
                firedSuccessfully = true;
            }

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

        firing = false;
    }

    private bool IsTargetValid(Enemy target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        float range = GetStat(TrapStat.Range);
        float rangeSqr = range * range;

        Collider2D collider = target.GetComponent<Collider2D>();

        if (collider == null || !collider.enabled)
            return false;

        Vector2 closestPoint = collider.ClosestPoint(transform.position);

        float distanceSqr =
            (closestPoint - (Vector2)transform.position).sqrMagnitude;

        return distanceSqr <= rangeSqr;
    }

    private Enemy FindNearestEnemy()
    {
        float range = GetStat(TrapStat.Range);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            range
        );

        Enemy closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 closestPoint = hit.ClosestPoint(transform.position);

            float distance =
                (closestPoint - (Vector2)transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        float range = GetStat(TrapStat.Range);

        Gizmos.DrawWireSphere(
            transform.position,
            range
        );
    }
#endif
}