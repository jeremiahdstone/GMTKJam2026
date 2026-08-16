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


    private void Update()
    {
        if (firing)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0)
            return;

        Enemy target = FindNearestEnemy();

        if (target != null)
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

        Vector2 direction = Vector2.right;

        if (target != null)
        {
            direction = (
                target.transform.position - firePoint.position
            ).normalized;
        }

        for (int i = 0; i < burstCount; i++)
        {
            // Update aim if the target still exists.
            if (target != null)
            {
                direction = (
                    target.transform.position - firePoint.position
                ).normalized;
            }

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            //pass in the current direction to fire, and the trap itself so the projectile can grab the stats
            projectile.Initialize(direction, this);

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

        firing = false;
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

            if (enemy == null)
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