using UnityEngine;

public class ExplosiveShiftUpgrade : Upgrade
{
    [Header("Explosive Shift Settings")]
    [SerializeField] private float explosiveDamage = 8f;
    [SerializeField] private float explosiveRadius = 3f;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private LayerMask damageableLayers;

    private Transform playerTransform;

    private void Awake()
    {
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            playerTransform.position,
            radius,
            damageableLayers
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.Damage(damage, gameObject);
            }
        }

        Debug.Log("Trying to spawn explosion");

        if (ExplosionEffect != null)
        {
            Instantiate(
                ExplosionEffect,
                playerTransform.position,
                Quaternion.identity
            );

            Debug.Log("Spawned explosion at " + playerTransform.position);

        }

        CameraShake.Instance?.Shake(
            0.6f + 0.05f * level
        );
    }
}
