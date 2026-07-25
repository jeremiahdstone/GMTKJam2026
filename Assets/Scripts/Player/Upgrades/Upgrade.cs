using UnityEngine;

//abstract upgrade to allow for different weird upgrades in the future
public abstract class Upgrade : IShoppable
{
    public string name = "Upgrade";
    public string description = "An upgrade for the player";
    public Sprite sprite;
    public float cost;

    public string getName() => name;
    public string getDescription() => description;
    public float getCost() => cost;
    public Sprite getIcon() => sprite;

    public int level = 1;

    public virtual float Modify(PlayerStat stat, float value)
    {
        return value;
    }

    public virtual void OnPurchase()
    {
        // Add to player upgrade list
        //uggh i hate this im def doing something wrong here this feels nasty in terms of coupling
        Object.FindFirstObjectByType<PlayerStats>().AddUpgrade(this);

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
