using UnityEngine;

//abstract upgrade to allow for different weird upgrades in the future
public abstract class Upgrade
{
    public string name = "Upgrade";
    public string description = "An upgrade for the player";
    public Sprite sprite;

    public int level = 1;

    public virtual float Modify(PlayerStat stat, float value)
    {
        return value;
    }
}

public class StatUpgrade : Upgrade
{
    public PlayerStat affectedStat;
    public float flatBonus;
    public float percentBonus;

    public override float Modify(PlayerStat targetStat, float value)
    {
        if (targetStat != affectedStat)
            return value;

        value += flatBonus * level;
        value *= 1 + percentBonus * level;

        return value;
    }

}

// non standard upgrades are basically just 'flags' that can be checked for in the various other player files
// this isnt great practice, but works for the jam timeline, ideally thered be some sort of event system in place
public class DoubleBiteUpgrade : Upgrade
{
}
