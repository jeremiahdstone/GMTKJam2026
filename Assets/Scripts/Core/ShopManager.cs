using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class ShopManager : MonoBehaviour
{
    public UpgradeDatabase upgradeDatabase;
    public TrapDatabase trapDatabase;

    public List<IShoppable> shopDatabase;

    public PlayerManager manager;

    [Header("Shop UI")]
    [SerializeField] private Transform shopRect;
    [SerializeField] private GameObject upgradePanelPrefab;
    [SerializeField] private GameObject trapPanelPrefab;

    [SerializeField] private Sprite trapNineSlice;
    [SerializeField] private Sprite trapBuyButtonNineSlice;

    public Queue<Trap> purchasedTraps = new();

    //adding a SINGLETON :bleh:
    public static ShopManager Instance { get; private set; }
    private void Awake() { Instance = this; }

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
        //GenerateShop();

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

                panel.GetComponent<Image>().sprite = trapNineSlice;

                panel.GetComponentInChildren<Button>().image.sprite = trapBuyButtonNineSlice;
                
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

    public void SpawnPurchasedTraps()
    {
        StartCoroutine(SpawnPurchasedTrapsRoutine());
    }

    private IEnumerator SpawnPurchasedTrapsRoutine()
    {
        while (purchasedTraps.Count > 0)
        {
            Trap trap = purchasedTraps.Dequeue();

            Vector2 spawnPosition = FindSpawnPosition();

            Instantiate(manager.SmokePuffEffect, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(0.5f);

            Instantiate(trap, spawnPosition, Quaternion.identity);
        }
    }

    //probably a better spot for this somewhere but its fineee
    private Vector2 FindSpawnPosition()
    {
        const int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(-5, 6);
            int y = Random.Range(-5, 6);

            Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);

            if (Physics2D.OverlapCircle(pos, 0.35f) == null)
                return pos;
        }

        Debug.LogWarning("Couldn't find an empty spawn location.");
        return new Vector2(0.5f, 0.5f);
    }

}