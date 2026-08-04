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

    public string getName() => name;
    public string getDescription() => description;
    public int getCost() => cost;
    public Sprite getIcon() => sprite;

    public int level = 1;

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

