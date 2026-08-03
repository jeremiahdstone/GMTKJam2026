using UnityEngine;

public class BombTrap : Trap
{
    [Header("Explosion")]
    [SerializeField] private float damage = 40f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private GameObject explosionEffect;

    [Header("Camera Shake")]
    [SerializeField] private Transform player;
    [SerializeField] private float maxShakeDistance = 15f;
    [SerializeField] private float maxShakeStrength = 1.5f;
    [SerializeField] private float minShakeStrength = 0.1f;

    private bool hasExploded;

    private void Awake()
    {
        singleUse = true;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded)
            return;

        if (!other.TryGetComponent(out Enemy enemy))
            return;

        hasExploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out Enemy nearbyEnemy))
                nearbyEnemy.Damage(damage);
        }

        Instantiate(
            explosionEffect,
            transform.position,
            explosionEffect.transform.rotation
        );

        ShakeCamera();

        if (GridPlacementManager.instance != null)
            GridPlacementManager.instance.ClearOccupiedCells(this);

        TriggerTrap(enemy);
    }

    private void ShakeCamera()
    {
        player = GameSession.instance.Player.transform;
        if (player == null || CameraShake.Instance == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // 1 when directly beside the explosion, 0 at maxShakeDistance.
        float distancePercent = Mathf.Clamp01(
            1f - distance / maxShakeDistance
        );

        // Squaring makes the shake weaken faster as the player gets farther away.
        distancePercent *= distancePercent;

        if (distancePercent <= 0f)
            return;

        float strength = Mathf.Lerp(
            minShakeStrength,
            maxShakeStrength,
            distancePercent
        );

        CameraShake.Instance.Shake(strength);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxShakeDistance);
    }
#endif
}