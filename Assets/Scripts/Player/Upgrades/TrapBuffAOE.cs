using UnityEngine;

public class TrapBuffAOE : MonoBehaviour
{
    private TrapBuffAOEUpgrade upgrade;

    private void Awake()
    {
        upgrade = GetComponentInParent<TrapBuffAOEUpgrade>();

        if (upgrade == null)
        {
            Debug.LogError(
                $"{name} could not find a TrapBuffAOEUpgrade in its parent."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (upgrade == null)
            return;

        Trap trap = other.GetComponent<Trap>();

        if (trap == null)
            return;

        foreach (TrapBuff buff in upgrade.GetBuffs())
        {
            trap.AddBuff(buff);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (upgrade == null)
            return;

        Trap trap = other.GetComponent<Trap>();

        if (trap == null)
            return;

        foreach (TrapBuff buff in upgrade.GetBuffs())
        {
            trap.RemoveBuff(buff);
        }
    }
}