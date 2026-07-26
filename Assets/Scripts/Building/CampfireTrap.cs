using System.Collections;
using UnityEngine;

public class CampfireTrap : Trap
{
    [Header("Attack")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float cooldown = 8f;

    [Header("Projectile")]
    [SerializeField] private ExplosiveProjectile projectilePrefab;
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
            StartCoroutine(FireShot(target));
        }
    }

    private IEnumerator FireShot(Enemy target)
    {
        firing = true;
        cooldownTimer = cooldown;

        Vector2 direction = Vector2.right;

        if (target != null)
        {
            direction = (target.transform.position - firePoint.position).normalized;
        }

        ExplosiveProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectile.Initialize(direction);

        yield return null;

        firing = false;
    }

    private Enemy FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}