using UnityEngine;

public abstract class Trap : Placeable, IShoppable
{
    [Header("Trap Settings")]
    public bool singleUse = false;

    //For shop interface
    public string trapName;
    public string description;
    public int cost;
    public Sprite icon;

    public string getName() => trapName;
    public string getDescription() => description;
    public int getCost() => cost;
    public Sprite getIcon() => icon;

    protected virtual void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnTrapPurchased += OnTrapPurchased;
            GameEventManager.instance.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    protected virtual void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnTrapPurchased -= OnTrapPurchased;
            GameEventManager.instance.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }

    protected virtual void OnTrapPurchased(Trap trap)
    {
        
    }

    protected virtual void OnUpgradePurchased(Upgrade upgrade)
    {
        
    }



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