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

        float cooldown = GetStat(TrapStat.Cooldown);
        int burstCount = Mathf.RoundToInt(GetStat(TrapStat.BurstCount));
        float burstDelay = GetStat(TrapStat.BurstDelay);

        cooldownTimer = cooldown;

        Vector2 lastDirection = Vector2.zero;

        for (int i = 0; i < burstCount; i++)
        {
            Vector2 direction;

            if (IsTargetValid(target))
            {
                // Target is still alive/active, so update our direction.
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

            // Safety check in case the target was invalid before
            // we ever got a valid direction.
            if (direction == Vector2.zero)
                break;

            Debug.Log(
                $"[ProjectileTrap] TARGET SEARCH\n" +
                $"Trap Position: {transform.position}\n" +
                $"Range: {GetStat(TrapStat.Range):F2}\n" +
                $"Found Target: {(target != null ? target.name : "NONE")}\n" +
                $"Target Position: {(target != null ? target.transform.position.ToString() : "N/A")}\n" +
                $"Target Distance: {(target != null ? Vector2.Distance(transform.position, target.transform.position).ToString("F2") : "N/A")}"
            );

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(direction, this);

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

        firing = false;
    }

    private bool IsTargetValid(Enemy target)
    {
        return (target != null && target.gameObject.activeInHierarchy);
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

            float distance =
                (enemy.transform.position - transform.position).sqrMagnitude;

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