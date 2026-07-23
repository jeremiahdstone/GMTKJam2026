using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class PlayerAttacks : MonoBehaviour
{
    //where all the values for player stats are stored
    public PlayerStats playerStats;

    public float biteTimer = 0;

    void Start()
    {
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

        // Teleport to enemy
        // transform.position = hit.transform.position;
        transform.DOMove(hit.transform.position, 0.1f).SetEase(Ease.InOutSine);
        

        // Deal damage
        damageable.Damage(playerStats.GetStat(PlayerStat.BiteDamage));

        Debug.Log("Bite attack executed");  //DEBUG

        // Start cooldown
        biteTimer = playerStats.GetStat(PlayerStat.BiteCooldown);
    }

}