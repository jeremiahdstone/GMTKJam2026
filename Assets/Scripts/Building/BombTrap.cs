using UnityEngine;

public class BombTrap : Trap
{
    public float damage = 40f;
    public float radius = 3f;

    private void Awake()
    {
        singleUse = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Enemy enemy))
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();

            if (e != null)
                e.Damage(damage);
        }

        // TODO: Spawn explosion effect here

        TriggerTrap(enemy);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}