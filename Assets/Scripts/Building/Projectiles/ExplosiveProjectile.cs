using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    [Header("Explosion")]
    [SerializeField] private float explosionDamage = 10f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionEffect;

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
                enemy.Damage(explosionDamage);
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