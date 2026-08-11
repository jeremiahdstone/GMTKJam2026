using UnityEngine;
using UnityEditor;

//abstract upgrade to allow for different weird upgrades in the future
public abstract class Upgrade : MonoBehaviour, IShoppable
{
    public string id = "";
    public string name = "Upgrade";
    [TextArea(2, 5)] public string description = "An upgrade for the player";
    public Sprite sprite;
    public int cost;
    [Range(0f, 2f)] public float costPercentIncrease = 0.2f;

    public string getName() => name;
    public string getDescription() => description;
    public int getCost() => GetCurrentCost();
    public Sprite getIcon() => sprite;

    public int level = 1;

    public int GetCurrentLevel()
    {
        PlayerStats playerStats = GameSession.instance != null
            ? GameSession.instance.Player?.GetComponent<PlayerStats>()
            : PlayerStats.Instance;

        if (playerStats == null)
        {
            return level;
        }

        Upgrade existing = playerStats.upgrades.Find(u => u.id == id || u.name == name);

        return existing != null ? existing.level : 0;
    }

    public int GetCurrentCost()
    {
        int currentLevel = GetCurrentLevel();
        return Mathf.RoundToInt(cost + (cost * currentLevel * costPercentIncrease));
    }

    public virtual Upgrade Clone()
    {
        return (Upgrade)MemberwiseClone();
    }

    public virtual float Modify(PlayerStat stat, float value)
    {
        return value;
    }

    public virtual void OnPurchase()
    {
        // Add to player upgrade list

        PlayerManager player = GameSession.instance.Player;


        player.GetComponent<PlayerStats>().AddUpgrade(this);
        
        

    }

    //disable the sprite renderer for the upgrade prefab so its not in the scene
    protected virtual void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.enabled = false;
    }

    //make the prefab look like the upgrade in the editor
    protected virtual void OnValidate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null) {
            sr.sprite = sprite;
        }
    }

}

// non standard upgrades are basically just 'flags' that can be checked for in the various other player files
// this isnt great practice, but works for the jam timeline, ideally thered be some sort of event system in place
public class DoubleBiteUpgrade : Upgrade
{
}

public class BatExplosionUpgrade : Upgrade
{
}

