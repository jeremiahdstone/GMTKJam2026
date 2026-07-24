using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;


public class PlayerAttacks : MonoBehaviour
{
    //where all the values for player stats are stored
    public PlayerStats playerStats;

    public float biteTimer = 0;

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Find an enemy at the mouse position
        Collider2D hit = Physics2D.OverlapPoint(mousePosition);
        if (hit == null)
            return;

        // Make sure the hit object is an enemy
        IDamageable damageable = hit.GetComponent<IDamageable>();
        if (damageable == null)
            return;

        // Check range
        float distance = Vector2.Distance(transform.position, hit.transform.position);
        if (distance > playerStats.GetStat(PlayerStat.BiteRange))
            return;

        // GOOD TO ATTACK
        StartCoroutine(DoBite(hit));
        // Call animation trigger
        

        Debug.Log("Bite attack executed");  //DEBUG

        // Start cooldown
        biteTimer = playerStats.GetStat(PlayerStat.BiteCooldown);

        // Check for DoubleBiteUpgrade
        // Again ideally this would be an event system
        if (playerStats.HasUpgrade<DoubleBiteUpgrade>())
        {
            // 10% chance to reset cooldown per level of DoubleBiteUpgrade
            if (Random.value < 0.1f * playerStats.GetUpgrade<DoubleBiteUpgrade>().level)
            {
                biteTimer = 0;
                Debug.Log("Double Bite triggered! Cooldown reset.");  //DEBUG
            }
        }

    }

    IEnumerator DoBite(Collider2D hit)
    {
        float initialAnimSpeed = anim.speed;
        float speedMult = playerStats.GetStat(PlayerStat.BiteSpeedMultiplier);
        anim.speed = speedMult;
        anim.SetTrigger("Bite");

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = hit.transform.position.x < transform.position.x;
        }

        // Teleport to enemy
        // transform.position = hit.transform.position;
        transform.DOMove(hit.transform.position, 0.5f / speedMult).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(0.4f / speedMult);
        

        // Deal damage
        hit.GetComponent<IDamageable>().Damage(playerStats.GetStat(PlayerStat.BiteDamage));

        anim.speed = initialAnimSpeed;
    }

}