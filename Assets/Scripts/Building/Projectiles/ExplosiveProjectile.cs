using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionEffect;

    public override void Initialize(Vector2 fireDirection, Trap sourceTrap)
    {
        base.Initialize(fireDirection, sourceTrap);

        explosionRadius = sourceTrap.GetStat(TrapStat.ExplosionRadius);
    }

    protected override void OnHit(Collider2D other)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                enemy.Damage(damage);
            }
        }

        if (explosionEffect != null)
        {
            Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}