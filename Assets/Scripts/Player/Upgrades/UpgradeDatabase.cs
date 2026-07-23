using System.Collections.Generic;

// all the upgrades the shop can pull from
public static class UpgradeDatabase
{
    public static readonly List<Upgrade> AllUpgrades = new()
    {
        new StatUpgrade
        {
            name = "Swift Feet",
            description = "Move faster while walking.",
            affectedStat = PlayerStat.WalkSpeed,
            flatBonus = 1f,
        },

        new StatUpgrade
        {
            name = "Stronger Wings",
            description = "Increase your maximum bat flight speed.",
            affectedStat = PlayerStat.BatFormMaxSpeed,
            flatBonus = 2.5f,
        },

        new StatUpgrade
        {
            name = "Rapid Shift",
            description = "Reduce the cooldown before entering bat form again.",
            affectedStat = PlayerStat.BatFormCooldown,
            flatBonus = -0.25f,
        },

        new StatUpgrade
        {
            name = "Quick Fangs",
            description = "Reduce the cooldown of Bite.",
            affectedStat = PlayerStat.BiteCooldown,
            flatBonus = -0.2f,
        },

        new StatUpgrade
        {
            name = "Sharpened Fangs",
            description = "Increase the damage dealt by Bite.",
            affectedStat = PlayerStat.BiteDamage,
            flatBonus = 2f,
        },

        new StatUpgrade
        {
            name = "Lunging Bite",
            description = "Increase the range of Bite.",
            affectedStat = PlayerStat.BiteRange,
            flatBonus = 0.5f,
        },
    };
}