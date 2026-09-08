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
        Vector3 biteLocation = bittenTransform.position;


        // Level 1 = base values, additional levels scale up.
        float damage = explosiveDamage + ((level - 1) * damageIncreasePerLevel);
        float radius = explosiveRadius + ((level - 1) * radiusIncreasePerLevel);

        
        if (ExplosionEffect != null)
        {
            GameObject explosion = PoolManager.instance.Spawn(ExplosionEffect, biteLocation, Quaternion.identity);
            explosion.GetComponent<Explosion>().Explode(radius, damage, damageableLayers);
        }

        
    }
}