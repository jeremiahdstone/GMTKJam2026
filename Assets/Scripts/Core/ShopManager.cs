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
        upgradeDatabase = GameSession.instance.GetComponentInChildren<UpgradeDatabase>();
        trapDatabase = GameSession.instance.GetComponentInChildren<TrapDatabase>();

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
        List<IShoppable> shopList = new List<IShoppable>();

        PlayerStats playerStats = GetPlayerStats();
        List<IShoppable> upgradeOptions = GetAvailableUpgradeOptions(playerStats);
        List<IShoppable> shopPool = GetShopPool(playerStats);

        // Ensure 1st option is alwyas an upgrade
        if (upgradeOptions.Count > 0)
        {
            shopList.Add(upgradeOptions[Random.Range(0, upgradeOptions.Count)]);
        }

        //pick random shop items for everything else (rn thats just the 1 middle item)
        for (int i = shopList.Count; i < shopItemCount-1; i++)
        {
            shopList.Add(shopPool[Random.Range(0, shopPool.Count)]);
        }

        // ensure last item is always a trap
        shopList.Add(trapDatabase.TrapPrefabs[Random.Range(0, trapDatabase.TrapPrefabs.Count)]);

        DisplayShop(shopList);
    }

    private void DisplayShop(List<IShoppable> shopList)
    {
        // Remove old shop items
        foreach (Transform child in shopRect)
        {
            Destroy(child.gameObject);
        }

        foreach (IShoppable item in shopList)
        {
            if (item == null)
            {
                Debug.LogWarning("Null item in shopList, skipping");
                continue;
            }

            GameObject panel;

            // if (item is Trap)
            //     panel = Instantiate(trapPanelPrefab, shopRect);
            // else
            //     panel = Instantiate(upgradePanelPrefab, shopRect);

            panel = Instantiate(upgradePanelPrefab, shopRect);

            if (panel == null)
            {
                Debug.LogError("Failed to instantiate upgrade panel prefab");
                continue;
            }

            // Safely find and update Title
            Transform titleTransform = panel.transform.Find("Title");
            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                    titleText.text = item.getName();
            }
            else
                Debug.LogError("Panel missing 'Title' child element");

            // Safely find and update Description
            Transform descTransform = panel.transform.Find("Description");
            if (descTransform != null)
            {
                TextMeshProUGUI descText = descTransform.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                    descText.text = item.getDescription();
            }
            else
                Debug.LogError("Panel missing 'Description' child element");

            // Safely find and update Icon
            Transform iconTransform = panel.transform.Find("upgrade/icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                    iconImage.sprite = item.getIcon();
            }
            else
                Debug.LogError("Panel missing 'upgrade/icon' child element");

            if (item is Trap)
            {
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();

                    if (iconImage != null)
                    {
                        iconImage.SetNativeSize();
                    }
                }

                Image panelImage = panel.GetComponent<Image>();
                if (panelImage != null)
                    panelImage.sprite = trapNineSlice;

                Button buttonComponent = panel.GetComponentInChildren<Button>();
                if (buttonComponent != null && buttonComponent.image != null)
                    buttonComponent.image.sprite = trapBuyButtonNineSlice;
            }

            // Safely find and update Price
            Transform priceTransform = panel.transform.Find("Button/PriceText");
            if (priceTransform != null)
            {
                TextMeshProUGUI priceText = priceTransform.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                    priceText.text = item.getCost().ToString();
            }
            else
                Debug.LogError("Panel missing 'Button/PriceText' child element");

            // Safely find Button
            Transform buttonTransform = panel.transform.Find("Button");
            if (buttonTransform == null)
            {
                Debug.LogError("Panel missing 'Button' child element");
                continue;
            }

            Button button = buttonTransform.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Button child missing Button component");
                continue;
            }

            IShoppable purchasedItem = item;
            button.onClick.AddListener(() =>
            {
                PlayerStats playerStats = GetPlayerStats();
                bool canPurchaseAsNewUpgrade = true;

                if (purchasedItem is Upgrade upgradeItem)
                {
                    bool alreadyOwned = playerStats != null && playerStats.upgrades.Exists(u => u.id == upgradeItem.id || u.name == upgradeItem.name);
                    bool hasOpenSlot = playerStats == null || playerStats.HasOpenUpgradeSlot();
                    canPurchaseAsNewUpgrade = alreadyOwned || hasOpenSlot;
                }

                // SUBTRACT MONEY
                if (canPurchaseAsNewUpgrade && GameSession.instance.run.bloodCount >= purchasedItem.getCost())
                {
                    GameSession.instance.SubtractBlood(purchasedItem.getCost());
                    purchasedItem.OnPurchase();

                    if(purchasedItem is Trap)
                    {
                        GameSession.instance.run.trapsBought++;
                        GameEventManager.instance.TrapPurchased(purchasedItem as Trap);
                    }
                    else if(purchasedItem is Upgrade)
                    {
                        GameSession.instance.run.upgradesBought++;
                        GameEventManager.instance.UpgradePurchased(purchasedItem as Upgrade);
                    }

                    // Destroy(panel);

                    // Take item out of current shop list
                    shopList.Remove(purchasedItem);

                    // Re display shop after purchase (for duplicate upgrades)
                    DisplayShop(shopList);

                    // Redisplay max blood count (in case the purchased item had anything to do with max blood count)
                    GameSession.instance.updateMaxBlood();
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

    private PlayerStats GetPlayerStats()
    {
        return GameSession.instance != null
            ? GameSession.instance.Player?.GetComponent<PlayerStats>()
            : PlayerStats.Instance;
    }

    private List<IShoppable> GetAvailableUpgradeOptions(PlayerStats playerStats)
    {
        List<IShoppable> upgradeOptions = new List<IShoppable>();

        if (playerStats != null && !playerStats.HasOpenUpgradeSlot())
        {
            foreach (Upgrade upgrade in playerStats.upgrades)
            {
                upgradeOptions.Add(upgrade);
            }
            return upgradeOptions;
        }

        foreach (Upgrade upgrade in upgradeDatabase.AllUpgrades)
        {
            upgradeOptions.Add(upgrade);
        }

        return upgradeOptions;
    }

    private List<IShoppable> GetShopPool(PlayerStats playerStats)
    {
        if (playerStats != null && !playerStats.HasOpenUpgradeSlot())
        {
            List<IShoppable> ownedUpgradePool = new List<IShoppable>();
            ownedUpgradePool.AddRange(playerStats.upgrades);
            ownedUpgradePool.AddRange(trapDatabase.TrapPrefabs);
            return ownedUpgradePool;
        }

        return shopDatabase;
    }

    //probably a better spot for this somewhere but its fineee
    private Vector2 FindSpawnPosition()
    {
        const int maxAttempts = 100;
        const int minDistance = 2;
        const int maxDistance = 4;
        const float overlapRadius = 0.35f;

        // World bounds for trap spawning
        const int worldMinX = -31;
        const int worldMaxX = 31;
        const int worldMinY = -31;
        const int worldMaxY = 28;

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

            // Ensure the spawn position is within the allowed world bounds
            if (spawnPosition.x < worldMinX || spawnPosition.x > worldMaxX || spawnPosition.y < worldMinY || spawnPosition.y > worldMaxY)
                continue;

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

        float clampedX = Mathf.Clamp(playerPosition.x + minDistance, worldMinX, worldMaxX);
        float clampedY = Mathf.Clamp(playerPosition.y, worldMinY, worldMaxY);

        return new Vector2(clampedX, clampedY);
    }

}