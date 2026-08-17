using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttacks : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("Targeting")]
    [SerializeField] private float clickSelectionRadius = 1f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Bat Explosion")]
    [SerializeField] private float batExplosionDamage = 8f;
    [SerializeField] private float batExplosionRadius = 3f;
    [SerializeField] private GameObject batExplosionEffect;

    [Header("Explosive Bite")]
    [SerializeField] private float explosiveBiteDamage = 8f;
    [SerializeField] private float explosiveBiteRadius = 2f;

    [Header("Bite Charge")]
    [Range(0f, 1f)]
    [SerializeField] private float minimumBiteDamageMultiplier = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumBiteRangeMultiplier = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumBiteSpeedMultiplier = 0.5f;

    [SerializeField] private GameObject fullyChargedBiteEffect;

    // [SerializeField] private GameObject explosiveBiteEffect;
    //just using the same one as bite for now

    private readonly HashSet<Enemy> highlightedEnemies = new();
    private readonly HashSet<Enemy> enemiesCurrentlyInRange = new();

    private PlayerManager manager;

    public float biteTimer;

    private bool currentlyBiting;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
        biteTimer = 0;
    }

    private void Update()
    {
        if (biteTimer > 0)
            biteTimer -= Time.deltaTime;

        UpdateBiteRangeHighlights();
    }

    public void BiteAttack(Vector2 mousePosition)
    {
        // Remove this:
        // if (biteTimer > 0)
        //     return;

        if (manager.playerMovement.batForm)
            return;

        if (currentlyBiting) return;



        float biteCooldown = playerStats.GetStat(PlayerStat.BiteCooldown);
        float chargeAmount = GetBiteCharge();

        float damageMultiplier = Mathf.Lerp(
            minimumBiteDamageMultiplier,
            1f,
            chargeAmount
        );

        float biteDamage =
            playerStats.GetStat(PlayerStat.BiteDamage) * damageMultiplier;

        float biteRange = GetChargedBiteRange(chargeAmount);
        float biteSpeed = GetChargedBiteSpeed(chargeAmount);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            mousePosition,
            clickSelectionRadius,
            damageableLayers
        );

        Collider2D closestCollider = null;
        IDamageable closestDamageable = null;

        float closestMouseDistanceSqr = Mathf.Infinity;
        float biteRangeSqr = biteRange * biteRange;

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable ??= hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            Vector2 targetPosition = hit.transform.position;

            float playerDistanceSqr =
                ((Vector2)transform.position - targetPosition).sqrMagnitude;

            if (playerDistanceSqr > biteRangeSqr)
                continue;

            float mouseDistanceSqr =
                (mousePosition - targetPosition).sqrMagnitude;

            if (mouseDistanceSqr < closestMouseDistanceSqr)
            {
                closestMouseDistanceSqr = mouseDistanceSqr;
                closestCollider = hit;
                closestDamageable = damageable;
            }
        }

        if (closestCollider == null || closestDamageable == null)
            return;

        bool fullyCharged = biteTimer <= 0.05f;



        StartCoroutine(
            DoBite(
                closestCollider,
                closestDamageable,
                biteDamage,
                biteSpeed,
                chargeAmount,
                chargeAmount >= 0.95f
            )
        );

        Debug.Log(
            $"Bite executed at {chargeAmount:P0} charge for {biteDamage} damage."
        );

        biteTimer = biteCooldown;


    }



    private IEnumerator DoBite(
        Collider2D targetCollider,
        IDamageable damageable,
        float biteDamage,
        float biteSpeed,
        float chargeAmount,
        bool fullyCharged)
    {
        currentlyBiting = true;

        float initialAnimSpeed = manager.anim.speed;

        // Protect against a zero multiplier.
        biteSpeed = Mathf.Max(0.01f, biteSpeed);

        manager.anim.speed = biteSpeed;
        manager.anim.SetTrigger("Bite");

        if (manager.sr != null)
        {
            manager.sr.flipX =
                targetCollider.transform.position.x < transform.position.x;
        }

        Vector3 targetPosition = targetCollider.transform.position;

        transform
            .DOMove(targetPosition, 0.5f / biteSpeed)
            .SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.4f / biteSpeed);

        if (targetCollider != null)
        {
            damageable.Damage(biteDamage, gameObject);

            GameEventManager.instance.Bite(targetCollider.transform, chargeAmount);
        }

        CameraShake.Instance?.Shake(0.5f);

        if (fullyCharged && fullyChargedBiteEffect != null)
        {
            Instantiate(
                fullyChargedBiteEffect,
                transform.position + new Vector3(0.25f, 0f, 0f),
                fullyChargedBiteEffect.transform.rotation
            );
        }

        manager.anim.speed = initialAnimSpeed;
        currentlyBiting = false;
    }

    //BiteCharge

    private float GetBiteCharge()
    {
        float cooldown = playerStats.GetStat(PlayerStat.BiteCooldown);

        // Prevent division by zero if an upgrade reduces cooldown to zero.
        if (cooldown <= 0f)
            return 1f;

        return 1f - Mathf.Clamp01(biteTimer / cooldown);
    }

    private float GetChargedBiteRange(float chargeAmount)
    {
        float rangeMultiplier = Mathf.Lerp(
            minimumBiteRangeMultiplier,
            1f,
            chargeAmount
        );

        return playerStats.GetStat(PlayerStat.BiteRange) * rangeMultiplier;
    }

    private float GetChargedBiteSpeed(float chargeAmount)
    {
        float speedMultiplier = Mathf.Lerp(
            minimumBiteSpeedMultiplier,
            1f,
            chargeAmount
        );

        return playerStats.GetStat(PlayerStat.BiteSpeedMultiplier)
               * speedMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            playerStats != null
                ? playerStats.GetStat(PlayerStat.BiteRange)
                : 0f
        );
    }

    private void UpdateBiteRangeHighlights()
    {
        float chargeAmount = GetBiteCharge();
        float biteRange = GetChargedBiteRange(chargeAmount);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            biteRange,
            damageableLayers
        );

        enemiesCurrentlyInRange.Clear();

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            enemiesCurrentlyInRange.Add(enemy);
            highlightedEnemies.Add(enemy);

            enemy.SetBiteRangeHighlight(true, transform.position);
        }

        highlightedEnemies.RemoveWhere(enemy =>
        {
            if (enemy == null)
                return true;

            if (enemiesCurrentlyInRange.Contains(enemy))
                return false;

            enemy.SetBiteRangeHighlight(false, transform.position);
            return true;
        });
    }

}