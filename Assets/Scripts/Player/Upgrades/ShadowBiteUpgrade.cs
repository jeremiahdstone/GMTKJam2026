using UnityEngine;
using System.Collections.Generic;

public class ShadowBiteUpgrade : Upgrade
{
    [Header("Shadow Bite")]
    [SerializeField] private GameObject bitingShadowPrefab;
    [SerializeField] private LayerMask damageableLayers;

    private PlayerManager manager;
    private PlayerStats playerStats;
    private PlayerAttacks playerAttacks;

    private void Awake()
    {
        base.Awake();

        manager = GetComponentInParent<PlayerManager>();

        if (manager != null)
        {
            playerStats = manager.GetComponent<PlayerStats>();
            playerAttacks = manager.GetComponent<PlayerAttacks>();
        }
    }

    private void OnEnable()
    {
        GameEventManager.instance.OnStartBite += ShadowBite;
    }

    private void OnDisable()
    {
        if (GameEventManager.instance != null)
            GameEventManager.instance.OnStartBite -= ShadowBite;
    }

    private void ShadowBite(Transform bitingTransform, float chargeAmount)
    {
        if (bitingShadowPrefab == null)
            return;

        if (manager == null || playerStats == null || playerAttacks == null)
            return;

        Enemy currentEnemy = bitingTransform.GetComponent<Enemy>();
        currentEnemy ??= bitingTransform.GetComponentInParent<Enemy>();

        if (currentEnemy == null)
            return;

        // Shadow Bite uses the same charged range as the bite that triggered it.
        float biteRange = GetChargedBiteRange(chargeAmount);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            manager.transform.position,
            biteRange,
            damageableLayers
        );

        // Get each unique enemy in range.
        HashSet<Enemy> nearbyEnemies = new();

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy == null)
                continue;

            // Don't target the enemy the player just bit.
            if (enemy == currentEnemy)
                continue;

            nearbyEnemies.Add(enemy);
        }

        if (nearbyEnemies.Count == 0)
            return;

        // Sort enemies by distance from the player.
        List<Enemy> sortedEnemies = new(nearbyEnemies);

        Vector2 playerPosition = manager.transform.position;

        sortedEnemies.Sort((a, b) =>
        {
            float distanceA =
                ((Vector2)a.transform.position - playerPosition).sqrMagnitude;

            float distanceB =
                ((Vector2)b.transform.position - playerPosition).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        float damageMultiplier = Mathf.Lerp(
            GetMinimumBiteDamageMultiplier(),
            1f,
            chargeAmount
        );

        float biteDamage =
            playerStats.GetStat(PlayerStat.BiteDamage) *
            damageMultiplier;

        float biteSpeed = GetChargedBiteSpeed(chargeAmount);

        bool fullyCharged = chargeAmount >= 0.95f;

        int shadowCount = Mathf.Min(
            level,
            sortedEnemies.Count
        );

        for (int i = 0; i < shadowCount; i++)
        {
            Enemy target = sortedEnemies[i];

            GameObject shadowObject = Instantiate(
                bitingShadowPrefab,
                manager.transform.position,
                manager.transform.rotation
            );

            BitingShadow shadow =
                shadowObject.GetComponent<BitingShadow>();

            if (shadow == null)
            {
                Debug.LogError(
                    "Shadow Bite prefab does not contain a BitingShadow component."
                );

                Destroy(shadowObject);
                continue;
            }

            shadow.Initialize(
                playerStats,
                target,
                biteDamage,
                biteSpeed,
                chargeAmount,
                fullyCharged,
                manager.transform
            );
        }
    }

    private float GetChargedBiteRange(float chargeAmount)
    {
        float minimumMultiplier = GetMinimumBiteRangeMultiplier();

        float rangeMultiplier = Mathf.Lerp(
            minimumMultiplier,
            1f,
            chargeAmount
        );

        return playerStats.GetStat(PlayerStat.BiteRange) *
               rangeMultiplier;
    }

    private float GetChargedBiteSpeed(float chargeAmount)
    {
        float minimumMultiplier = GetMinimumBiteSpeedMultiplier();

        float speedMultiplier = Mathf.Lerp(
            minimumMultiplier,
            1f,
            chargeAmount
        );

        return playerStats.GetStat(PlayerStat.BiteSpeedMultiplier) *
               speedMultiplier;
    }

    private float GetMinimumBiteDamageMultiplier()
    {
        // Keep these matching PlayerAttacks for now.
        return 0.25f;
    }

    private float GetMinimumBiteRangeMultiplier()
    {
        return 0.5f;
    }

    private float GetMinimumBiteSpeedMultiplier()
    {
        return 0.5f;
    }
}