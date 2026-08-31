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

        trap.AddBuff(upgrade.GetDamageBuff());
        trap.AddBuff(upgrade.GetCooldownBuff());
        trap.AddBuff(upgrade.GetRangeBuff());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (upgrade == null)
            return;

        Trap trap = other.GetComponent<Trap>();

        if (trap == null)
            return;

        trap.RemoveBuff(upgrade.GetDamageBuff());
        trap.RemoveBuff(upgrade.GetCooldownBuff());
        trap.RemoveBuff(upgrade.GetRangeBuff());
    }
}