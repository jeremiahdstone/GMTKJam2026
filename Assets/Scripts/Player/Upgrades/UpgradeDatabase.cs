using UnityEngine;
using System.Collections.Generic;

public class UpgradeDatabase : MonoBehaviour
{
    public Sprite[] upgradeIcons;

    public List<Upgrade> AllUpgrades = new();

    private void Awake()
    {
        AllUpgrades = new List<Upgrade>()
        {
            new StatUpgrade
            {
                id = "swift_feet",
                name = "Swift Feet",
                description = "Move faster while walking.",
                sprite = upgradeIcons[0],
                affectedStat = PlayerStat.WalkSpeed,
                flatBonus = 1f,
            },

            new StatUpgrade
            {
                id = "stronger_wings",
                name = "Stronger Wings",
                description = "Increase your maximum bat flight speed.",
                sprite = upgradeIcons[2],
                affectedStat = PlayerStat.BatFormMaxSpeed,
                flatBonus = 2.5f,
            },

            new StatUpgrade
            {
                id = "rapid_shift",
                name = "Rapid Shift",
                description = "Reduce the cooldown before entering bat form again.",
                sprite = upgradeIcons[4],
                affectedStat = PlayerStat.BatFormCooldown,
                flatBonus = -0.25f,
            },

            new StatUpgrade
            {
                id = "quick_fangs",
                name = "Quick Fangs",
                description = "Reduce the cooldown of Bite.",
                sprite = upgradeIcons[3],
                affectedStat = PlayerStat.BiteCooldown,
                flatBonus = -0.2f,
            },

            new StatUpgrade
            {
                id = "sharpened_fangs",
                name = "Sharpened Fangs",
                description = "Increase the damage dealt by Bite.",
                sprite = upgradeIcons[1],
                affectedStat = PlayerStat.BiteDamage,
                flatBonus = 2f,
            },

            new StatUpgrade
            {
                id = "lunging_bite",
                name = "Lunging Bite",
                description = "Increase the range of Bite.",
                sprite = upgradeIcons[5],
                affectedStat = PlayerStat.BiteRange,
                flatBonus = 0.5f,
            },

            new StatUpgrade
            {
                id = "quick_bite",
                name = "Quick Bite",
                description = "Increase the speed of your Bite Lunge.",
                sprite = upgradeIcons[6],
                affectedStat = PlayerStat.BiteSpeedMultiplier,
                flatBonus = 0.2f,
            },

            new DoubleBiteUpgrade
            {
                id = "bite_chain",
                name = "Bite Chain",
                description = "Chance for Bite cooldown to reset immediatley after Bite.",
                sprite = upgradeIcons[7],
            },
        };
    }
}
