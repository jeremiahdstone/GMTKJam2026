using System.Collections.Generic;
using UnityEngine;

public class UpgradeDatabase : MonoBehaviour
{
    [Header("Assign Upgrade Prefabs Here")]
    public List<Upgrade> AllUpgrades = new();


    //PREVIOUS UPGRADES
    
    // public Sprite[] upgradeIcons;

    // public List<Upgrade> AllUpgrades = new();

    // private void Awake()
    // {
    //     AllUpgrades = new List<Upgrade>()
    //     {
    //         new StatUpgrade
    //         {
    //             id = "swift_feet",
    //             name = "Swift Feet",
    //             description = "Move faster while walking.",
    //             sprite = upgradeIcons[0],
    //             affectedStat = PlayerStat.WalkSpeed,
    //             flatBonus = 1f,
    //             cost = 8,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "stronger_wings",
    //             name = "Stronger Wings",
    //             description = "Increase your maximum bat flight speed.",
    //             sprite = upgradeIcons[2],
    //             affectedStat = PlayerStat.BatFormMaxSpeed,
    //             flatBonus = 2.5f,
    //             cost = 8,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "rapid_shift",
    //             name = "Rapid Shift",
    //             description = "Reduce the cooldown before entering bat form again.",
    //             sprite = upgradeIcons[4],
    //             affectedStat = PlayerStat.BatFormCooldown,
    //             flatBonus = -0.25f,
    //             cost = 10,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "quick_fangs",
    //             name = "Quick Fangs",
    //             description = "Reduce the cooldown of Bite.",
    //             sprite = upgradeIcons[3],
    //             affectedStat = PlayerStat.BiteCooldown,
    //             flatBonus = -0.2f,
    //             cost = 14,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "sharpened_fangs",
    //             name = "Sharpened Fangs",
    //             description = "Increase the damage dealt by Bite.",
    //             sprite = upgradeIcons[1],
    //             affectedStat = PlayerStat.BiteDamage,
    //             flatBonus = 2f,
    //             cost = 10,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "lunging_bite",
    //             name = "Lunging Bite",
    //             description = "Increase the range of Bite.",
    //             sprite = upgradeIcons[5],
    //             affectedStat = PlayerStat.BiteRange,
    //             flatBonus = 0.5f,
    //             cost = 10,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "quick_bite",
    //             name = "Quick Bite",
    //             description = "Increase the speed of your Bite Lunge.",
    //             sprite = upgradeIcons[6],
    //             affectedStat = PlayerStat.BiteSpeedMultiplier,
    //             flatBonus = 0.2f,
    //             cost = 12,
    //         },

    //         new DoubleBiteUpgrade
    //         {
    //             id = "bite_chain",
    //             name = "Bite Chain",
    //             description = "Chance for Bite cooldown to reset immediatley after Bite.",
    //             sprite = upgradeIcons[7],
    //             cost = 16,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "sucking_bite",
    //             name = "Sucking Bite",
    //             description = "Gain more blood when you kill an enemy with Bite.",
    //             sprite = upgradeIcons[9],
    //             affectedStat = PlayerStat.BiteBloodMultipler,
    //             flatBonus = 0.1f,
    //             cost = 12,
    //         },

    //         new StatUpgrade
    //         {
    //             id = "max_blood",
    //             name = "Max Blood",
    //             description = "Increase your total amount of blood.",
    //             sprite = upgradeIcons[8],
    //             affectedStat = PlayerStat.MaxBlood,
    //             flatBonus = 10f,
    //             cost = 10,
    //         },

    //         new BatExplosionUpgrade
    //         {
    //             id = "explosive_shift",
    //             name = "Explosive Shift",
    //             description = "Leaving bat form creates an explosion.",
    //             sprite = upgradeIcons[10],
    //             cost = 16,
    //         },

    //         new ExplosiveBiteUpgrade
    //         {
    //             id = "explosive_bite",
    //             name = "Explosive Bite",
    //             description = "Create an explosion when you bite an enemy",
    //             sprite = upgradeIcons[11],
    //             cost = 14,
    //         },
    //     };
    // }
}
