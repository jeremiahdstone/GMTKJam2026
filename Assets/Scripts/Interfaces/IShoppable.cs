using UnityEngine;

public interface IShoppable
{
    //all UI elements
    public string getName();
    public string getDescription();
    public float getCost();
    public Sprite getIcon();
    
    //spawn the trap or add the upgrade
    public void OnPurchase();
}