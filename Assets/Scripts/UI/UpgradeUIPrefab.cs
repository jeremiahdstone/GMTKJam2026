using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeUIPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Upgrade currentUpgrade;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private Image icon;

    private InfoPopup popup;

    public void SetUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return;

        LevelText.text = "LVL " + upgrade.level;
        icon.sprite = upgrade.sprite;
        currentUpgrade = upgrade;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentUpgrade == null || popup != null || PopupManager.instance == null)
            return;

        popup = PopupManager.instance.SpawnPopup(
            transform,
            Vector2.zero,
            currentUpgrade.name,
            currentUpgrade.description,
            PopupAnchorSpace.Canvas, 30f
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleasePopup();
    }

    private void OnDisable()
    {
        ReleasePopup();
    }

    private void ReleasePopup()
    {
        if (popup == null)
            return;

        if (PopupManager.instance != null)
            PopupManager.instance.ReleasePopup(popup);

        popup = null;
    }

    public void SetLevel(int level)
    {
        LevelText.text = "LVL " + level;
    }

    public void VerifyUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return;

        bool textNeedsRefresh = LevelText == null || LevelText.text != "LVL " + upgrade.level;
        bool iconNeedsRefresh = icon == null || icon.sprite != upgrade.sprite;

        if (currentUpgrade == null || currentUpgrade != upgrade || textNeedsRefresh || iconNeedsRefresh)
        {
            SetUpgrade(upgrade);
        }
    }

    void Awake()
    {
        UITween uiTween = GetComponentInChildren<UITween>();
        if(uiTween != null)
        {
            uiTween.showDuration = 0.5f + (0.2f * (float)transform.GetSiblingIndex());
        }
    }
}
