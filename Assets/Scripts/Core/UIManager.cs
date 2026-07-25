using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;

    [Header("UI Components")]
    [SerializeField] private Image attackCooldownImage;
    [SerializeField] private Image batFormCoolDownImage;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private Slider bloodAmountSlider;
    [SerializeField] private TMP_Text bloodAmountText;

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
        if(count == 0)
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
}