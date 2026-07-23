using UnityEngine;

public class BrambleTrap : Trap
{
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.speedMultiplier *= slowMultiplier;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.speedMultiplier /= slowMultiplier;
    }
}