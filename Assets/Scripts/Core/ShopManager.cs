using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    // public UpgradeDatabase upgradeDatabase; 
    // access with 'UpgradeDatabase.AllUpgrades'
    // yeah i shouldve used scriptable objects for this i dont like how these are different...
    public TrapDatabase trapDatabase;

    public List<IShoppable> shopDatabase;

    void Start()
    {
        shopDatabase = new List<IShoppable>();

        shopDatabase.AddRange(UpgradeDatabase.AllUpgrades); 
        shopDatabase.AddRange(trapDatabase.TrapPrefabs);

        //TESTING, BUY 10 SPEED UPGRADES FROM THE SHOP
        for (int i = 0; i < 10; i++)
        {
            shopDatabase[0].OnPurchase();
        }
    }

    public IShoppable[] generateShop() {
        IShoppable[] shopList = new IShoppable[3];



        return shopList; 
    } 
}
