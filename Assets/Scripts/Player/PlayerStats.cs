using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//ALL POSSIBLE STATS GO HERE
public enum PlayerStat
{
    MoveSpeed,
    BatFormMaxSpeed,
    BatFormCooldown,
    BiteDamage,
    BiteCooldown,
    BiteRange,
}

public class PlayerStats : MonoBehaviour
{
    //BASE STATS
    private Dictionary<PlayerStat, float> baseStats =
        new Dictionary<PlayerStat, float>()
    {
        { PlayerStat.MoveSpeed, 5 },
        { PlayerStat.BatFormMaxSpeed, 15 },
        { PlayerStat.BatFormCooldown, 2 },
        { PlayerStat.BiteDamage, 10 },
        { PlayerStat.BiteCooldown, 2 },
        { PlayerStat.BiteRange, 5 }
    };

    public List<Upgrade> upgrades = new();


    public float GetStat(PlayerStat stat)
    {
        float value = baseStats[stat];

        foreach (Upgrade upgrade in upgrades)
        {
            value = upgrade.Modify(stat, value);
        }

        return value;
    }

    //check if the player already has this upgrade, if so just level it up, otherwise add it to the list
    public void AddUpgrade(Upgrade upgrade)
    {
        Upgrade existing = upgrades.Find(u => u.GetType() == upgrade.GetType());

        if (existing != null)
        {
            existing.level++;
        }
        else
        {
            upgrades.Add(upgrade);
        }
    }

    //check for/get specific upgrades, used for if we do weird extra upgrades later
    public bool HasUpgrade<T>() where T : Upgrade
    {
        return upgrades.Exists(u => u is T);
    }
    public T GetUpgrade<T>() where T : Upgrade
    {
        return upgrades.Find(u => u is T) as T;
    }
}