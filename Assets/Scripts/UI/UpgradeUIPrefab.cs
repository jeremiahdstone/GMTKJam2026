using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeUIPrefab : MonoBehaviour
{
    public Upgrade currentUpgrade;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private Image icon;

    public void SetUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return;

        LevelText.text = "LVL " + upgrade.level;
        icon.sprite = upgrade.sprite;
        currentUpgrade = upgrade;
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
