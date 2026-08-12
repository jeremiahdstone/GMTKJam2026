using System.Collections;
using UnityEngine;

public class ProjectileTrap : Trap
{
    [Header("Attack")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private int burstCount = 1;
    [SerializeField] private float burstDelay = 0.5f;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("AOE Visual")]
    [SerializeField] private Transform aoeVisual;

    private float cooldownTimer;
    private bool firing;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd += ShowAOE;
            GameEventManager.instance.OnWaveStart += HideAOE;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd -= ShowAOE;
            GameEventManager.instance.OnWaveStart -= HideAOE;
        }
    }

    protected override void OnTrapPurchased(Trap trap)
    {
        base.OnTrapPurchased(trap);
        RefreshAOEVisual(trap);
    }

    protected override void OnUpgradePurchased(Upgrade upgrade)
    {
        base.OnUpgradePurchased(upgrade);
        RefreshAOEVisual(null);
    }

    void RefreshAOEVisual(Trap trap)
    {
        if (aoeVisual != null)
        {
            aoeVisual.localScale = new Vector3(detectionRange * 2, detectionRange * 2, 1);
        }
    }

    void ShowAOE()
    {
        if (aoeVisual == null)
            return;
        aoeVisual.localScale = new Vector3(detectionRange * 2, detectionRange * 2, 1);
        aoeVisual.gameObject.SetActive(true);
    }

    void HideAOE()
    {
        if (aoeVisual == null)
            return;
        aoeVisual.gameObject.SetActive(false);
    }

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

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(direction);

            if (i < burstCount - 1)
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