using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : Trap
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float damageInterval = 1f;

    private readonly Dictionary<Enemy, float> nextDamageTimes = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        DamageEnemy(enemy);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        if (!nextDamageTimes.TryGetValue(enemy, out float nextDamageTime))
        {
            DamageEnemy(enemy);
            return;
        }

        if (Time.time >= nextDamageTime)
            DamageEnemy(enemy);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
            nextDamageTimes.Remove(enemy);
    }

    private void DamageEnemy(Enemy enemy)
    {
        enemy.Damage(GetStat(TrapStat.Damage));
        TriggerTrap(enemy);

        nextDamageTimes[enemy] = Time.time + GetStat(TrapStat.Cooldown);
    }
}