using UnityEngine;

public class ExplosiveBiteUpgrade : Upgrade
{
    [Header("Explosive Bite Settings")]
    [SerializeField] private float explosiveDamage = 8f;
    [SerializeField] private float explosiveRadius = 2f;
    [SerializeField] private float damageIncreasePerLevel = 4f;
    [SerializeField] private float radiusIncreasePerLevel = .2f;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private LayerMask damageableLayers;

    private void OnEnable()
    {
        GameEventManager.instance.OnBite += ExplosiveBite;
    }

    private void OnDisable()
    {
        GameEventManager.instance.OnBite -= ExplosiveBite;
    }

    private void ExplosiveBite(Transform bittenTransform, float chargeAmount)
    {
        if (chargeAmount < 0.95f) return;
        Debug.LogWarning("Explosive Bite Triggered");
        Vector3 biteLocation = bittenTransform.position;


        // Level 1 = base values, additional levels scale up.
        float damage = explosiveDamage + ((level - 1) * damageIncreasePerLevel);
        float radius = explosiveRadius + ((level - 1) * radiusIncreasePerLevel);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            biteLocation,
            radius,
            damageableLayers
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy == null)
                continue;

            // Distance from the center of the explosion.
            float distance = Vector2.Distance(
                biteLocation,
                hit.ClosestPoint(biteLocation)
            );

            // 1 at center, 0 at the edge of the explosion.
            float damageMultiplier = 1f - Mathf.Clamp01(distance / radius);

            float finalDamage = damage * damageMultiplier;

            enemy.Damage(finalDamage, gameObject);
        }

        if (ExplosionEffect != null)
        {
            Instantiate(
                ExplosionEffect,
                biteLocation,
                Quaternion.identity
            );
        }

        CameraShake.Instance?.Shake(
            0.6f + 0.05f * level
        );
    }
}