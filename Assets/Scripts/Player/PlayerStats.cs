using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//ALL POSSIBLE STATS GO HERE
public enum PlayerStat
{
    WalkSpeed,
    BatFormMaxSpeed,
    BatFormCooldown,
    BatFormAcceleration,
    BiteDamage,
    BiteCooldown,
    BiteRange,
    BiteSpeedMultiplier,
    BiteBloodMultipler,
    MaxBlood,
    UpgradeSlots,
}

//buffs for the stats
public class PlayerBuff : MonoBehaviour
{
    public PlayerStat affectedStat;
    public float flatBonus;
    public float percentBonus;

    public float Modify(PlayerStat targetStat, float value)
    {
        if (targetStat != affectedStat)
            return value;

        value += flatBonus;
        value *= 1 + percentBonus;

        return value;
    }
}

public class PlayerStats : MonoBehaviour
{
    //game object that houses all the instantiated upgrades
    public GameObject upgradesObject { get; private set; }

    //BASE STATS
    private Dictionary<PlayerStat, float> baseStats =
        new Dictionary<PlayerStat, float>()
    {
        { PlayerStat.WalkSpeed, 5 },
        { PlayerStat.BatFormMaxSpeed, 15 },
        { PlayerStat.BatFormAcceleration, 1.1f },
        { PlayerStat.BatFormCooldown, 2 },
        { PlayerStat.BiteDamage, 10 },
        { PlayerStat.BiteCooldown, 2 },
        { PlayerStat.BiteRange, 5 },
        { PlayerStat.BiteSpeedMultiplier, 1},
        { PlayerStat.BiteBloodMultipler, 1},
        { PlayerStat.MaxBlood, 100},
        { PlayerStat.UpgradeSlots, 6},

    };

    //upgrades in the player's upgrade slots
    public List<Upgrade> upgrades = new();

    //temp buffs for the player
    public List<PlayerBuff> buffs = new();

    //singleton :3
    public static PlayerStats Instance { get; private set; }
    private void Awake()
    {
        //setup singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        //get upgrade gameobject
        upgradesObject = GameObject.Find("Upgrades");
    }

    public float GetStat(PlayerStat stat)
    {
        float value = baseStats[stat];

        //upgrades
        foreach (Upgrade upgrade in upgrades)
        {
            value = upgrade.Modify(stat, value);
        }

        //buffs
        foreach (PlayerBuff buff in buffs)
        {
            if (buff != null)
            {
                value = buff.Modify(stat, value);
            }
        }

        //i kinda wanna make something similar here to the way i do it in my town game, 
        //where all the flat increases are addded first, and then all the percent increases are applied 

        return value;
    }

    //UPGRADES

    public int GetUpgradeSlotCount()
    {
        return Mathf.RoundToInt(GetStat(PlayerStat.UpgradeSlots));
    }

    public bool HasOpenUpgradeSlot()
    {
        return upgrades.Count < GetUpgradeSlotCount();
    }

    //check if the player already has this upgrade, if so just level it up, otherwise add it to the list
    public void AddUpgrade(Upgrade upgrade)
    {
        if (string.IsNullOrEmpty(upgrade.id))
        {
            upgrade.id = upgrade.name;
        }

        Upgrade existing = upgrades.Find(u => u.id == upgrade.id || u.name == upgrade.name);

        if (existing != null)
        {
            existing.level++;
            existing.OnLevelUp();
        }
        else if (HasOpenUpgradeSlot())
        {
            Upgrade playerUpgrade = Instantiate(upgrade.gameObject, transform.GetChild(0).transform).GetComponent<Upgrade>();
            playerUpgrade.level = 1;
            upgrades.Add(playerUpgrade);
        }
        else
        {
            Debug.LogWarning("No upgrade slots available for a new unique upgrade.");
            return;
        }

        if (GameSession.instance != null)
        {
            GameSession.instance.uiManager.RebuildUpgradeList(upgrades);
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


    // BUFFS

    public void AddBuff(PlayerBuff buff)
    {
        if (buff == null)
            return;

        if (!buffs.Contains(buff))
        {
            buffs.Add(buff);
        }
    }

    public void RemoveBuff(PlayerBuff buff)
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