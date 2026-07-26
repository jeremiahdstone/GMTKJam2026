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

    [SerializeField] private LayerMask trapBlockingLayers;

    public int shopItemCount = 3; 

    //adding a SINGLETON :bleh:
    public static ShopManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        Physics2D.queriesHitTriggers = true;
    }


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

    public void GenerateShop()
    {
        IShoppable[] shopList = new IShoppable[shopItemCount];

        // ensure first item is always an upgrade
        shopList[0] = upgradeDatabase.AllUpgrades[Random.Range(0, upgradeDatabase.AllUpgrades.Count)];

        // ensure last item is always a trap
        shopList[shopItemCount-1] = trapDatabase.TrapPrefabs[Random.Range(0, trapDatabase.TrapPrefabs.Count)];

        //pick random shop items for everything else (rn thats just the 1 middle item)
        for (int i = 1; i < shopItemCount-1; i++)
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
                if (GameSession.instance.run.bloodCount > purchasedItem.getCost())
                {
                    GameSession.instance.SubtractBlood(purchasedItem.getCost());
                    purchasedItem.OnPurchase();
                    Destroy(panel);
                }

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
            CameraShake.Instance.Shake(0.5f);

            yield return new WaitForSeconds(0.5f);

            Trap spawnedTrap = Instantiate(
                trap,
                spawnPosition,
                Quaternion.identity
            );

            Placeable placeable = spawnedTrap.GetComponent<Placeable>();

            if (placeable != null)
            {
                GridPlacementManager.instance.RegisterPlaceable(placeable);
            }
        }
    }

    //probably a better spot for this somewhere but its fineee
    private Vector2 FindSpawnPosition()
    {
        const int maxAttempts = 100;
        const int minDistance = 2;
        const int maxDistance = 4;
        const float overlapRadius = 0.35f;

        Vector2 playerPosition = manager.transform.position;

        Vector2Int playerTilePosition = new Vector2Int(
            Mathf.FloorToInt(playerPosition.x),
            Mathf.FloorToInt(playerPosition.y)
        );

        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(-maxDistance, maxDistance + 1);
            int y = Random.Range(-maxDistance, maxDistance + 1);

            Vector2Int offset = new Vector2Int(x, y);

            // Prevent spawning directly on or immediately beside the player.
            if (offset.sqrMagnitude < minDistance * minDistance)
                continue;

            Vector2 spawnPosition = new Vector2(
                playerTilePosition.x + x + 0.5f,
                playerTilePosition.y + y + 0.5f
            );

            Collider2D overlap = Physics2D.OverlapCircle(
    spawnPosition,
    overlapRadius,
    trapBlockingLayers
);

            if (overlap == null)
                return spawnPosition;
        }

        Debug.LogWarning(
            "Couldn't find an empty spawn location near the player."
        );

        return playerPosition + Vector2.right * minDistance;
    }

}