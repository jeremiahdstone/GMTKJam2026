using UnityEngine;

public class TrapBuffAOEUpgrade : AOEUpgrade
{
    [Header("Trap Buff")]
    [SerializeField] private float baseBuffPercent = 0.20f;
    [SerializeField] private float buffPercentPerLevel = 0.10f;

    private TrapBuff damageBuff;
    private TrapBuff cooldownBuff;
    private TrapBuff rangeBuff;

    protected override void Awake()
    {
        base.Awake();

        CreateBuffs();
        UpdateBuffValues();
    }

    private void CreateBuffs()
    {
        damageBuff = gameObject.AddComponent<TrapBuff>();
        damageBuff.affectedStat = TrapStat.Damage;

        cooldownBuff = gameObject.AddComponent<TrapBuff>();
        cooldownBuff.affectedStat = TrapStat.Cooldown;

        rangeBuff = gameObject.AddComponent<TrapBuff>();
        rangeBuff.affectedStat = TrapStat.Range;
    }

    private void UpdateBuffValues()
    {
        float buffPercent =
            baseBuffPercent + (buffPercentPerLevel * (level - 1));

        damageBuff.percentBonus = buffPercent;
        rangeBuff.percentBonus = buffPercent;

        // Cooldown works in the opposite direction.
        cooldownBuff.percentBonus = -buffPercent;
    }

    public TrapBuff GetDamageBuff()
    {
        return damageBuff;
    }

    public TrapBuff GetCooldownBuff()
    {
        return cooldownBuff;
    }

    public TrapBuff GetRangeBuff()
    {
        return rangeBuff;
    }

    public override void OnLevelUp()
    {
        base.OnLevelUp();

        UpdateBuffValues();
    }
}