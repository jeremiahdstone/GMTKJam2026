using System.Collections.Generic;
using UnityEngine;

public class TrapBuffAOEUpgrade : AOEUpgrade
{
    [Header("Trap Buff")]
    [SerializeField] private float baseBuffPercent = 0.20f;
    [SerializeField] private float buffPercentPerLevel = 0.10f;

    private List<TrapBuff> buffs = new List<TrapBuff>();

    protected override void Awake()
    {
        base.Awake();

        CreateBuffs();
        UpdateBuffValues();
    }

    private void CreateBuffs()
    {
        TrapBuff damageBuff = gameObject.AddComponent<TrapBuff>();
        damageBuff.affectedStat = TrapStat.Damage;
        buffs.Add(damageBuff);

        TrapBuff cooldownBuff = gameObject.AddComponent<TrapBuff>();
        cooldownBuff.affectedStat = TrapStat.Cooldown;
        buffs.Add(cooldownBuff);

        TrapBuff rangeBuff = gameObject.AddComponent<TrapBuff>();
        rangeBuff.affectedStat = TrapStat.Range;
        buffs.Add(rangeBuff);
    }

    private void UpdateBuffValues()
    {
        float buffPercent =
            baseBuffPercent + (buffPercentPerLevel * (level - 1));

        // this cooldown thing is a little odd
        // maybe cooldown should be replaced with like "attack speed" so i can add to it like the others
        // or, each buff has its own base % and per level %, not all the same, and it set them in the inspector.
        foreach (TrapBuff buff in buffs)
        {
            buff.percentBonus = buff.affectedStat == TrapStat.Cooldown
                ? -buffPercent
                : buffPercent;
        }
    }

    public List<TrapBuff> GetBuffs()
    {
        return buffs;
    }

    public override void OnLevelUp()
    {
        base.OnLevelUp();

        UpdateBuffValues();
    }
}