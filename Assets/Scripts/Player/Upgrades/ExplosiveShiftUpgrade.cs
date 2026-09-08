using UnityEngine;

public class ExplosiveShiftUpgrade : Upgrade
{
    [Header("Explosive Shift Settings")]
    [SerializeField] private float explosiveDamage = 8f;
    [SerializeField] private float explosiveRadius = 3f;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private LayerMask damageableLayers;

    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = transform.parent.parent;
    }

    private void OnEnable()
    {
        GameEventManager.instance.OnBatModeExit += BatExplosion;
    }

    private void OnDisable()
    {
        GameEventManager.instance.OnBatModeExit -= BatExplosion;
    }

    public void BatExplosion()
    {
        float damage = explosiveDamage + ((level - 1) * 4f);
        float radius = explosiveRadius + ((level - 1) * 0.25f);

        
        if (ExplosionEffect != null)
        {
            Explosion explosion = Instantiate(
                ExplosionEffect,
                playerTransform.position,
                Quaternion.identity
            ).GetComponent<Explosion>();

            explosion.Explode(radius, damage, damageableLayers);

            

        }
    }
}
