using UnityEngine;

public class SpikeTrap : Trap
{
    public float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.TakeDamage(damage);

        TriggerTrap(enemy);
    }
}