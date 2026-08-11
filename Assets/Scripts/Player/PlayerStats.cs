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

    public List<Upgrade> upgrades = new();

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

        foreach (Upgrade upgrade in upgrades)
        {
            value = upgrade.Modify(stat, value);
        }

        return value;
    }

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
}