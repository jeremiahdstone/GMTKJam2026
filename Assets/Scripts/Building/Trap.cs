using UnityEngine;

public abstract class Trap : Placeable, IShoppable
{
    [Header("Trap Settings")]
    public bool singleUse = false;

    //For shop interface
    public string trapName;
    public string description;
    public float cost;
    public Sprite icon;

    public string getName() => trapName;
    public string getDescription() => description;
    public float getCost() => cost;
    public Sprite getIcon() => icon;

    protected virtual void TriggerTrap(Enemy enemy)
    {
        if (singleUse)
            Destroy(gameObject);
    }

    

    public virtual void OnPurchase()
    {
        ShopManager.Instance.purchasedTraps.Enqueue(this);
    }
}