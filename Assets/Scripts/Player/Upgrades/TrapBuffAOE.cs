using UnityEngine;

public class TrapBuffAOE : MonoBehaviour
{
    private TrapBuff damageBuff;
    private TrapBuff cooldownBuff;

    private void Awake()
    {
        // Damage +50%
        damageBuff = gameObject.AddComponent<TrapBuff>();
        damageBuff.affectedStat = TrapStat.Damage;
        damageBuff.percentBonus = 0.50f;

        // Cooldown -50%
        cooldownBuff = gameObject.AddComponent<TrapBuff>();
        cooldownBuff.affectedStat = TrapStat.Cooldown;
        cooldownBuff.percentBonus = -0.50f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Trap trap = other.GetComponentInParent<Trap>();

        if (trap == null)
            return;

        trap.AddBuff(damageBuff);
        trap.AddBuff(cooldownBuff);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Trap trap = other.GetComponentInParent<Trap>();

        if (trap == null)
            return;

        trap.RemoveBuff(damageBuff);
        trap.RemoveBuff(cooldownBuff);
    }
}