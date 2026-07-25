using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;



public class ShopManager : MonoBehaviour
{
    public UpgradeDatabase upgradeDatabase;
    public TrapDatabase trapDatabase;

    public List<IShoppable> shopDatabase;

    [Header("Shop UI")]
    [SerializeField] private Transform shopRect;
    [SerializeField] private GameObject upgradePanelPrefab;
    [SerializeField] private GameObject trapPanelPrefab;

    void Start()
    {
        shopDatabase = new List<IShoppable>();

        shopDatabase.AddRange(upgradeDatabase.AllUpgrades); 
        shopDatabase.AddRange(trapDatabase.TrapPrefabs);

        //TESTING, BUY 10 SPEED UPGRADES FROM THE SHOP
        // for (int i = 0; i < 10; i++)
        // {
        //     shopDatabase[0].OnPurchase();
        // }

        //CALL THIS WHENEVER YOU WANNA GENERATE AND DISPLAY A NEW SHOP
        GenerateShop();

    }

    public void GenerateShop() {
        IShoppable[] shopList = new IShoppable[3];

        //TODO eventually guarantee theres at least 1 trap and 1 upgrade
        
        //pick shop items
        for (int i = 0; i < 3; i++)
        {
            shopList[i] = shopDatabase[Random.Range(0, shopDatabase.Count)];
        }

        DisplayShop(shopList);
    }

    private void DisplayShop(IShoppable[] shopList)
    {
        // Remove old shop items
        foreach (Transform child in shopRect)
        {
            Destroy(child.gameObject);
        }

        foreach (IShoppable item in shopList)
        {
            GameObject panel;

            // if (item is Trap)
            //     panel = Instantiate(trapPanelPrefab, shopRect);
            // else
            //     panel = Instantiate(upgradePanelPrefab, shopRect);

            panel = Instantiate(upgradePanelPrefab, shopRect);

            panel.transform.Find("Title")
                .GetComponent<TextMeshProUGUI>().text = item.getName();

            panel.transform.Find("Description")
                .GetComponent<TextMeshProUGUI>().text = item.getDescription();

            panel.transform.Find("upgrade/icon")
                .GetComponent<Image>().sprite = item.getIcon();
            if (item is Trap)
            {
                panel.transform.Find("upgrade/icon")
                    .GetComponent<RectTransform>().sizeDelta = new Vector2(8, 8);
            }

            panel.transform.Find("Button/PriceText")
                .GetComponent<TextMeshProUGUI>().text = item.getCost().ToString();

            Button button = panel.transform.Find("Button").GetComponent<Button>();

            IShoppable purchasedItem = item;
            button.onClick.AddListener(() =>
            {
                // SUBTRACT MONEY
                purchasedItem.OnPurchase();
                Destroy(panel);
            });

        }
    }

}