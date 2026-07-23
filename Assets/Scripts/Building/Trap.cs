using UnityEngine;

public abstract class Trap : Placeable
{
    [Header("Trap Settings")]
    public bool singleUse = false;

    protected virtual void TriggerTrap(Enemy enemy)
    {
        if (singleUse)
            Destroy(gameObject);
    }
}