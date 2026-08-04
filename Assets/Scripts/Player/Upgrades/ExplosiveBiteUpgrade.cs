using UnityEngine;

public class ExplosiveBiteUpgrade : Upgrade
{
    [Header("Explosive Bite Settings")]
    [SerializeField] private float explosiveDamage = 8f;
    [SerializeField] private float explosiveRadius = 2f;
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

    private void ExplosiveBite(Transform bittenTransform, bool fullyCharged = false)
    {
        if (!fullyCharged) return;
        Debug.LogWarning("Explosive Bite Triggered");
        Vector3 biteLocation = bittenTransform.position;


        // Level 1 = base values, additional levels scale up.
        float damage = explosiveDamage + ((level - 1) * 5f);
        float radius = explosiveRadius + ((level - 1) * 0.2f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            biteLocation,
            radius,
            damageableLayers
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.Damage(damage, transform);
            }
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
