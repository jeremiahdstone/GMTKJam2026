using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public bool singleUse = false;

    protected virtual void TriggerTrap(Enemy enemy)
    {
        if (singleUse)
            Destroy(gameObject);
    }
}