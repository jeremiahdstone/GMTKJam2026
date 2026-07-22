using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerAttacks : MonoBehaviour
{
    public float biteDamage;
    public float biteCooldown;
    public float biteRange;

    public float biteTimer;

    void Start()
    {
        biteDamage = 10;
        biteCooldown = 2;
        biteRange = 5;

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
        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy == null)
            return;

        // Check range
        float distance = Vector2.Distance(transform.position, enemy.transform.position);
        if (distance > biteRange)
            return;

        // GOOD TO ATTACK

        // Teleport to enemy
        transform.position = enemy.transform.position;

        // Deal damage
        enemy.TakeDamage(biteDamage);   //thisll prolly be a IDamagable interface ill need to mess with

        Debug.Log("Bite attack executed");  //DEBUG

        // Start cooldown
        biteTimer = biteCooldown;
    }

}