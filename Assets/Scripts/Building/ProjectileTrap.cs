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

    // protected override void OnEnable()
    // {
    //     base.OnEnable();
    //     if (GameEventManager.instance != null)
    //     {
    //         GameEventManager.instance.OnWaveEnd += StopAllCoroutines;
    //     }
    // }

    // protected override void OnDisable()
    // {
    //     base.OnDisable();
    //     if (GameEventManager.instance != null)
    //     {
    //         GameEventManager.instance.OnWaveEnd -= StopAllCoroutines;
    //     }
    // }


    private void Update()
    {
        if (firing)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0)
            return;

        Enemy target = FindNearestEnemy();

        if (target != null)
            if (target.gameObject.activeInHierarchy)
            {
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

        for (int i = 0; i < burstCount; i++)
        {
            // Target died, got pooled, moved out of range, etc.
            if (!IsTargetValid(target))
                break;

            Vector2 direction = (
                target.transform.position - firePoint.position
            ).normalized;

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
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        float range = GetStat(TrapStat.Range);

        return Vector2.Distance(transform.position, target.transform.position) <= range;
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