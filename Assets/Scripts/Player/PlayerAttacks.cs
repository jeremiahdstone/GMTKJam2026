using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;


public class PlayerAttacks : MonoBehaviour
{
    //where all the values for player stats are stored
    public PlayerStats playerStats;

    private PlayerManager manager;

    public float biteTimer = 0;

    void Start()
    {
        manager = GetComponent<PlayerManager>();
        biteTimer = 0;
    }

    void Update()
    {
        //handle cooldowns
        if (biteTimer > 0)
            biteTimer -= Time.deltaTime;
    }

    public void BiteAttack(Vector2 mousePosition)
    {
        // Cooldown check
        if (biteTimer > 0)
            return;

        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            damageable ??= hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                hit.transform.position
            );

            if (distance > playerStats.GetStat(PlayerStat.BiteRange))
                continue;

            StartCoroutine(DoBite(hit));

            Debug.Log($"Bite attack executed on {hit.name}");

            biteTimer = playerStats.GetStat(PlayerStat.BiteCooldown);

            if (playerStats.HasUpgrade<DoubleBiteUpgrade>())
            {
                int level = playerStats.GetUpgrade<DoubleBiteUpgrade>().level;

                if (Random.value < 0.1f * level)
                {
                    biteTimer = 0;
                    Debug.Log("Double Bite triggered! Cooldown reset.");
                }
            }

            // Only attack the first valid target found.
            return;
        }

    }

    IEnumerator DoBite(Collider2D hit)
    {
        float initialAnimSpeed = manager.anim.speed;
        float speedMult = playerStats.GetStat(PlayerStat.BiteSpeedMultiplier);
        manager.anim.speed = speedMult;
        manager.anim.SetTrigger("Bite");

        if (manager.sr != null)
        {
            manager.sr.flipX = hit.transform.position.x < transform.position.x;
        }

        // Teleport to enemy
        // transform.position = hit.transform.position;
        transform.DOMove(hit.transform.position, 0.5f / speedMult).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(0.4f / speedMult);


        // Deal damage
        hit.GetComponent<IDamageable>().Damage(playerStats.GetStat(PlayerStat.BiteDamage));

        manager.anim.speed = initialAnimSpeed;
    }

}