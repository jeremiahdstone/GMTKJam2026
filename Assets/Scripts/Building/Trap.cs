using UnityEngine;
using System;
using System.Collections.Generic;

// ALL POSSIBLE TRAP STATS GO HERE
public enum TrapStat
{
    // General, pretty much every trap has these
    Damage,
    Range,
    Cooldown,

    // Trap Specifc, keeping them in the same enum allows upgrades that buff all traps that have a stat (for example 'Duration')
    Duration,
    SlowDown,
    ExplosionRadius,
}

// Made this its own serializable thing so you can use it in editor 
[Serializable]
public class TrapStatValue
{
    public TrapStat stat;
    public float value;
}

// trap buffs are held by the trap
// if we ever wanna have buffs that do something other than affect stats, well need a different system for that
public class TrapBuff : MonoBehaviour
{
    public TrapStat affectedStat;
    public float flatBonus;
    public float percentBonus;

    public float Modify(TrapStat targetStat, float value)
    {
        if (targetStat != affectedStat)
            return value;

        value += flatBonus;
        value *= 1 + percentBonus;

        return value;
    }

}

public abstract class Trap : Placeable, IShoppable
{
    [Header("Trap Settings")]
    public bool singleUse = false;

    [Header("Trap Stats")]
    [SerializeField] private List<TrapStatValue> baseStats = new();

    // Runtime dictionary for fast stat lookups.
    // ^ this makes it way faster since we're calling these all the time
    // baseStats is just used for setting them up in the editor
    private Dictionary<TrapStat, float> baseStatsDictionary = new();

    // Buffs currently affecting this trap.
    // Buffs modify stats when GetStat() is called.
    public List<TrapBuff> buffs = new();    

    //For shop interface
    [Header("Shop Information")]
    public string trapName;
    public string description;
    public int cost;
    public Sprite icon;

    public string getName() => trapName;
    public string getDescription() => description;
    public int getCost() => cost;
    public Sprite getIcon() => icon;

    protected virtual void Awake()
    {
        // Transfer serialized stats into a dictionary for fast runtime lookups.
        foreach (TrapStatValue stat in baseStats)
        {
            //just in case we flub somewhere
            if (baseStatsDictionary.ContainsKey(stat.stat))
            {
                Debug.LogWarning(
                    $"{name} has multiple values for TrapStat.{stat.stat}. " +
                    $"Only the last value will be used."
                );
            }

            baseStatsDictionary[stat.stat] = stat.value;
        }
    }

    protected virtual void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnTrapPurchased += OnTrapPurchased;
            GameEventManager.instance.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    protected virtual void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnTrapPurchased -= OnTrapPurchased;
            GameEventManager.instance.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }

    protected virtual void OnTrapPurchased(Trap trap)
    {
        
    }

    protected virtual void OnUpgradePurchased(Upgrade upgrade)
    {
        
    }



    protected virtual void TriggerTrap(Enemy enemy)
    {
        if (singleUse)
            Destroy(gameObject);
    }

    

    public virtual void OnPurchase()
    {
        ShopManager.Instance.purchasedTraps.Enqueue(this);
    }


    // STATS
    // this might should be in its own class and have a "TrapStats" inside the trap

    public float GetStat(TrapStat stat)
    {
        if (!baseStatsDictionary.TryGetValue(stat, out float value))
        {
            Debug.LogWarning(
                $"{name} does not have TrapStat.{stat}"
            );

            return 0f;
        }

        foreach (TrapBuff buff in buffs)
        {
            if (buff != null)
            {
                value = buff.Modify(stat, value);
            }
        }

        return value;
    }

    //could be useful for displaying in UI or smth, or maybe not increasing the stat if its not in the base stats
    public bool HasStat(TrapStat stat)
    {
        return baseStatsDictionary.ContainsKey(stat);
    }


    // BUFFS

    public void AddBuff(TrapBuff buff)
    {
        if (buff == null)
            return;

        if (!buffs.Contains(buff))
        {
            buffs.Add(buff);
        }
    }

    public void RemoveBuff(TrapBuff buff)
    {
        if (buff == null)
            return;

        buffs.Remove(buff);
    }

    public void ClearBuffs()
    {
        buffs.Clear();
    }
}