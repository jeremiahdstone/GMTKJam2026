using System.Collections;
using UnityEngine;

public class BoneTrap : Trap
{
    [Header("Attack")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.5f;

    [Header("Projectile")]
    [SerializeField] private Projectile bonePrefab;
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
        cooldownTimer = cooldown;

        Vector2 direction = Vector2.right;

        if (target != null)
        {
            direction = (target.transform.position - firePoint.position).normalized;
        }

        for (int i = 0; i < burstCount; i++)
        {
            // Update aim if the target still exists.
            if (target != null)
            {
                direction = (target.transform.position - firePoint.position).normalized;
            }

            Projectile bone = Instantiate(
                bonePrefab,
                firePoint.position,
                Quaternion.identity
            );

            bone.Initialize(direction);

            yield return new WaitForSeconds(burstDelay);
        }

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}