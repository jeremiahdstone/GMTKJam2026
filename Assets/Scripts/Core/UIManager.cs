using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] public ShopManager shopManager;

    [Header("Game UI Components")]
    [SerializeField] private Image attackCooldownImage;
    [SerializeField] private Image batFormCoolDownImage;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private Slider bloodAmountSlider;
    [SerializeField] private TMP_Text bloodAmountText;

    [Header("Build UI Components")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopOpenButton;
    [SerializeField] private GameObject nextDayButton;

    [SerializeField] private TMP_Text upgradesText;
    [SerializeField] private GameObject upgradeList;

    [Header("UI Sections")]
    [SerializeField] private GameObject buildUI;
    [SerializeField] private GameObject openingLetter;

    [Header("Shop Refresh")]
    [SerializeField] private int startingRefreshPrice = 5;
    [SerializeField] private int refreshPriceIncrement = 1;
    [SerializeField] private TMP_Text refreshPriceText;

    private int refreshPrice = 5;


    private RectTransform attackCooldownRect;
    private RectTransform batFormCooldownRect;

    private float attackCooldownFullHeight;
    private float attackCooldownStartingY;
    private float batFormCooldownFullHeight;
    private float batFormCooldownStartingY;

    private void Start()
    {
        attackCooldownRect = attackCooldownImage.rectTransform;
        batFormCooldownRect = batFormCoolDownImage.rectTransform;

        attackCooldownFullHeight = attackCooldownRect.rect.height;
        attackCooldownStartingY = attackCooldownRect.anchoredPosition.y;
        batFormCooldownFullHeight = batFormCooldownRect.rect.height;
        batFormCooldownStartingY = batFormCooldownRect.anchoredPosition.y;
    }

    private void Update()
    {
        SetAttackCooldown();
        SetBatFormCooldown();
    }

    private void SetAttackCooldown()
    {
        float maxCooldown =
            playerManager.playerStats.GetStat(PlayerStat.BiteCooldown);

        float visibleAmount = 1f;

        if (maxCooldown > 0f)
        {
            float cooldownPercent = Mathf.Clamp01(
                playerManager.playerAttacks.biteTimer / maxCooldown
            );

            visibleAmount = 1f - cooldownPercent;
        }

        float newHeight = attackCooldownFullHeight * visibleAmount;
        float removedHeight = attackCooldownFullHeight - newHeight;

        attackCooldownRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            newHeight
        );

        Vector2 position = attackCooldownRect.anchoredPosition;

        // Move downward by half the height that was removed.
        position.y = attackCooldownStartingY - removedHeight * 0.5f;

        attackCooldownRect.anchoredPosition = position;
    }

    private void SetBatFormCooldown()
    {
        float maxCooldown =
            playerManager.playerStats.GetStat(PlayerStat.BatFormCooldown);

        float visibleAmount = 1f;

        if (maxCooldown > 0f)
        {
            float cooldownPercent = Mathf.Clamp01(
                playerManager.playerMovement.batFormCooldownTimer / maxCooldown
            );

            visibleAmount = 1f - cooldownPercent;
        }

        float newHeight = batFormCooldownFullHeight * visibleAmount;
        float removedHeight = batFormCooldownFullHeight - newHeight;

        batFormCooldownRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            newHeight
        );

        Vector2 position = batFormCooldownRect.anchoredPosition;

        // Move downward by half the height that was removed.
        position.y = batFormCooldownStartingY - removedHeight * 0.5f;

        batFormCooldownRect.anchoredPosition = position;
    }

    public void SetDay(int day)
    {
        dayText.text = "Day " + day.ToString();
    }

    public void SetEnemyCount(int count)
    {
        if (count == 0)
        {
            enemyCountText.text = "";
            return;
        }

        enemyCountText.text = count.ToString();
    }

    public void SetBloodSlider(int value, int maxValue)
    {
        bloodAmountSlider.maxValue = maxValue;
        bloodAmountSlider.value = value;

        bloodAmountText.text = value.ToString() + "/" + maxValue.ToString();
    }

    public void OpenShopPanel()
    {
        shopPanel.SetActive(true);
        upgradeList.SetActive(true);
        if (shopOpenButton.activeSelf)
            shopOpenButton.GetComponent<UITween>().Hide();
        if (nextDayButton.activeSelf)
            nextDayButton.GetComponent<UITween>().Hide();
        GameSession.instance.DisablePlayerMovement(true);
    }
    public void CloseShopPanel()
    {
        shopPanel.GetComponent<UITween>().Hide();
        upgradeList.GetComponent<UITween>().Hide();

        shopOpenButton.SetActive(true);
        nextDayButton.SetActive(true);
        GameSession.instance.DisablePlayerMovement(false);
    }

    public void CloseBuildUI()
    {
        if (shopOpenButton.activeSelf)
            shopOpenButton.GetComponent<UITween>().Hide();
        if (nextDayButton.activeSelf)
            nextDayButton.GetComponent<UITween>().Hide();
        if (shopPanel.activeSelf)
            shopPanel.GetComponent<UITween>().Hide();

    }

    public void RebuildUpgradeList(List<Upgrade> upgrades)
    {
        upgradesText.text = "";
        foreach (Upgrade upgrade in upgrades)
        {
            upgradesText.text += upgrade.name + ":<color=#890027> LVL " + upgrade.level.ToString() + "</color>\n";
        }
    }

    public void ResetRefreshPrice()
    {
        refreshPrice = startingRefreshPrice;
        refreshPriceText.text = refreshPrice.ToString();
    }

    public void RefreshShop()
    {
        

        if (GameSession.instance.run.bloodCount > refreshPrice)
        {
            GameSession.instance.SubtractBlood(refreshPrice);
            refreshPrice += refreshPriceIncrement;
            refreshPriceText.text = refreshPrice.ToString();

            shopManager.GenerateShop();
        }


    }

    public void showOpeningLetter()
    {
        openingLetter.SetActive(true);
    }
}