using UnityEngine;

public class BrambleTrap : Trap
{
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.currentSpeed *= GetStat(TrapStat.SlowDown);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.currentSpeed /= GetStat(TrapStat.SlowDown);
    }
}