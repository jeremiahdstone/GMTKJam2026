using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

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
    [SerializeField] private GameObject loseScreen;

    [Header("Shop Refresh")]
    [SerializeField] private int startingRefreshPrice = 5;
    [SerializeField] private int refreshPriceIncrement = 1;
    [SerializeField] private TMP_Text refreshPriceText;


    private int refreshPrice = 5;



    [Header("Blood Particle Effect")]
    [SerializeField] private float bloodShakeStrength = 0.25f;
    [SerializeField] private ParticleSystem bloodParticlePrefab;
    [SerializeField] private RectTransform bloodParticleSpawnPoint;

    [Header("Cooldown UI Tweening")]
    [SerializeField] private float cooldownSmoothTime = 0.06f;
    [SerializeField] private float cooldownReadyPunchStrength = 0.16f;
    [SerializeField] private float cooldownReadyPunchDuration = 0.3f;

    [Header("Lose Screen Opening")]
    [SerializeField] private float loseScreenFadeInTime = 1f;
    [SerializeField] private float loseScreenFadeOpacity = 0.43f;
    [SerializeField] private GameObject GameOverRestartButton;
    [SerializeField] private GameObject GameOverMainMenuButton;
    [SerializeField] private TMP_Text GameOverStats;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject[] pauseMenuButtons;

    private float attackCooldownDisplayedAmount = 1f;
    private float batFormCooldownDisplayedAmount = 1f;

    private float attackCooldownSmoothVelocity;
    private float batFormCooldownSmoothVelocity;

    private bool attackCooldownWasActive;
    private bool batFormCooldownWasActive;




    private RectTransform attackCooldownRect;
    private RectTransform batFormCooldownRect;

    private float attackCooldownFullHeight;
    private float attackCooldownStartingY;
    private float batFormCooldownFullHeight;
    private float batFormCooldownStartingY;


    //Tweens
    private Tween attackCooldownReadyTween;
    private Tween batFormCooldownReadyTween;
    private Tween bloodSliderWobbleTween;
    private Tween bloodTween;
    private Tween bloodPunchTween;
    private Tween dayPunchTween;
    private Tween enemyCountPunchTween;
    private Tween refreshPricePunchTween;

    private void Start()
    {
        attackCooldownRect = attackCooldownImage.rectTransform;
        batFormCooldownRect = batFormCoolDownImage.rectTransform;

        attackCooldownFullHeight = attackCooldownRect.rect.height;
        attackCooldownStartingY = attackCooldownRect.anchoredPosition.y;
        batFormCooldownFullHeight = batFormCooldownRect.rect.height;
        batFormCooldownStartingY = batFormCooldownRect.anchoredPosition.y;

        attackCooldownDisplayedAmount = 1f;
        batFormCooldownDisplayedAmount = 1f;

        attackCooldownWasActive =
            playerManager.playerAttacks.biteTimer > 0f;

        batFormCooldownWasActive =
            playerManager.playerMovement.batFormCooldownTimer > 0f;
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

        float timer = playerManager.playerAttacks.biteTimer;

        float targetVisibleAmount = 1f;

        if (maxCooldown > 0f)
        {
            float cooldownPercent = Mathf.Clamp01(timer / maxCooldown);
            targetVisibleAmount = 1f - cooldownPercent;
        }

        attackCooldownDisplayedAmount = Mathf.SmoothDamp(
            attackCooldownDisplayedAmount,
            targetVisibleAmount,
            ref attackCooldownSmoothVelocity,
            cooldownSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        ApplyCooldownHeight(
            attackCooldownRect,
            attackCooldownFullHeight,
            attackCooldownStartingY,
            attackCooldownDisplayedAmount
        );

        bool cooldownIsActive = timer > 0f;

        // Trigger once when the cooldown becomes ready.
        if (attackCooldownWasActive && !cooldownIsActive)
        {
            PlayCooldownReadyTween(
                attackCooldownImage.transform,
                ref attackCooldownReadyTween
            );
        }

        attackCooldownWasActive = cooldownIsActive;
    }

    private void SetBatFormCooldown()
    {
        float maxCooldown =
            playerManager.playerStats.GetStat(PlayerStat.BatFormCooldown);

        float timer = playerManager.playerMovement.batFormCooldownTimer;

        float targetVisibleAmount = 1f;

        if (maxCooldown > 0f)
        {
            float cooldownPercent = Mathf.Clamp01(timer / maxCooldown);
            targetVisibleAmount = 1f - cooldownPercent;
        }

        batFormCooldownDisplayedAmount = Mathf.SmoothDamp(
            batFormCooldownDisplayedAmount,
            targetVisibleAmount,
            ref batFormCooldownSmoothVelocity,
            cooldownSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        ApplyCooldownHeight(
            batFormCooldownRect,
            batFormCooldownFullHeight,
            batFormCooldownStartingY,
            batFormCooldownDisplayedAmount
        );

        bool cooldownIsActive = timer > 0f;

        // Trigger once when the cooldown becomes ready.
        if (batFormCooldownWasActive && !cooldownIsActive)
        {
            PlayCooldownReadyTween(
                batFormCoolDownImage.transform,
                ref batFormCooldownReadyTween
            );
        }

        batFormCooldownWasActive = cooldownIsActive;
    }

    private void ApplyCooldownHeight(
    RectTransform cooldownRect,
    float fullHeight,
    float startingY,
    float visibleAmount)
    {
        visibleAmount = Mathf.Clamp01(visibleAmount);

        float newHeight = fullHeight * visibleAmount;
        float removedHeight = fullHeight - newHeight;

        cooldownRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            newHeight
        );

        Vector2 position = cooldownRect.anchoredPosition;
        position.y = startingY - removedHeight * 0.5f;
        cooldownRect.anchoredPosition = position;
    }

    private void PlayCooldownReadyTween(
        Transform cooldownTransform,
        ref Tween cooldownTween)
    {
        cooldownTween?.Kill();

        cooldownTransform.localScale = Vector3.one;
        cooldownTransform.localRotation = Quaternion.identity;

        Sequence sequence = DOTween.Sequence()
            .SetUpdate(true);

        sequence.Append(
            cooldownTransform.DOPunchScale(
                Vector3.one * cooldownReadyPunchStrength,
                cooldownReadyPunchDuration,
                vibrato: 7,
                elasticity: 0.5f
            )
        );

        sequence.Join(
            cooldownTransform.DOShakeRotation(
                duration: cooldownReadyPunchDuration,
                strength: new Vector3(0f, 0f, 4f),
                vibrato: 8,
                randomness: 45f,
                fadeOut: true
            )
        );

        cooldownTween = sequence;
    }

    public void SetDay(int day)
    {
        dayText.text = "Day " + day;

        dayPunchTween?.Kill();
        dayText.transform.localScale = Vector3.one;

        dayPunchTween = dayText.transform
            .DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.5f)
            .SetUpdate(true);
    }


    public void SetEnemyCount(int count)
    {
        enemyCountPunchTween?.Kill();

        if (count == 0)
        {
            enemyCountText.text = "";
            return;
        }

        enemyCountText.text = count.ToString();

        enemyCountText.transform.localScale = Vector3.one;

        enemyCountPunchTween = enemyCountText.transform
            .DOPunchScale(Vector3.one * 0.12f, 0.2f, 5, 0.4f)
            .SetUpdate(true);
    }

    public void SetBloodSlider(int value, int maxValue)
    {
        bloodTween?.Kill();
        bloodPunchTween?.Kill();

        bloodAmountSlider.maxValue = maxValue;

        float startingValue = bloodAmountSlider.value;

        bool bloodIncreased = value > startingValue;

        bloodTween = DOTween.To(
                () => startingValue,
                currentValue =>
                {
                    bloodAmountSlider.value = currentValue;

                    int displayedValue = Mathf.RoundToInt(currentValue);
                    bloodAmountText.text = displayedValue + "/" + maxValue;
                },
                value,
                0.45f
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

        bloodAmountText.transform.localScale = Vector3.one;

        bloodPunchTween = bloodAmountText.transform
            .DOPunchScale(Vector3.one * 0.12f, 0.3f, 5, 0.4f)
            .SetUpdate(true);

        if (bloodIncreased)
            DoBloodSliderPunchPositive();
    }

    public void DoBloodSliderPunchPositive()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(bloodShakeStrength);

        bloodSliderWobbleTween?.Kill();

        bloodAmountSlider.transform.localRotation = Quaternion.identity;

        bloodSliderWobbleTween = bloodAmountSlider.transform
            .DOShakeRotation(
                duration: 0.4f,
                strength: new Vector3(0f, 0f, 3f),
                vibrato: 10,
                randomness: 60f,
                fadeOut: true
            )
            .SetUpdate(true);
    }

    public void DoBloodSliderPunch()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(bloodShakeStrength);

        bloodSliderWobbleTween?.Kill();

        bloodAmountSlider.transform.localRotation = Quaternion.identity;

        bloodSliderWobbleTween = bloodAmountSlider.transform
            .DOShakeRotation(
                duration: 0.4f,
                strength: new Vector3(0f, 0f, 5f),
                vibrato: 10,
                randomness: 60f,
                fadeOut: true
            )
            .SetUpdate(true);

        SpawnBloodSplatter();
    }

    private void SpawnBloodSplatter()
    {
        if (bloodParticlePrefab == null)
            return;

        RectTransform spawnPoint = bloodParticleSpawnPoint != null
            ? bloodParticleSpawnPoint
            : bloodAmountSlider.GetComponent<RectTransform>();

        ParticleSystem particles = Instantiate(
            bloodParticlePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        particles.Play();
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
        AnimateRefreshPrice();
    }

    private void AnimateRefreshPrice()
    {
        refreshPriceText.text = refreshPrice.ToString();

        refreshPricePunchTween?.Kill();
        refreshPriceText.transform.localScale = Vector3.one;

        refreshPricePunchTween = refreshPriceText.transform
            .DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.4f)
            .SetUpdate(true);
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

    public void ShowLoseScreen(RunData run)
    {
        loseScreen.SetActive(true);

        GameOverStats.text = "Protected castle for <color=#d9243c>" + run.day + "</color> days\n\n";
        GameOverStats.text += "Feasted on <color=#d9243c>" + run.enemiesKilled + "</color> humans\n\n";
        GameOverStats.text += "Purchased <color=#d9243c>" + run.upgradesBought + "</color> upgrades ";
        GameOverStats.text += "and placed <color=#d9243c>" + run.upgradesBought + "</color> traps";

        Image loseScreenImage = loseScreen.GetComponent<Image>();

        loseScreenImage.DOKill();

        Color color = loseScreenImage.color;
        color.a = 0f;
        loseScreenImage.color = color;

        loseScreenImage
            .DOFade(loseScreenFadeOpacity, loseScreenFadeInTime)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

        StartCoroutine(DisplayRestartandMainMenu());
    }

    private IEnumerator DisplayRestartandMainMenu()
    {
        yield return new WaitForSecondsRealtime(1f);
        GameOverRestartButton.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        GameOverMainMenuButton.SetActive(true);

    }

    private Tween pauseFadeTween;
    private bool pauseMenuOpen;
    private int pauseTransitionVersion;

    public void OpenPauseMenu()
    {
        pauseMenuOpen = true;
        pauseTransitionVersion++;

        pauseMenuPanel.SetActive(true);

        Image pauseImage = pauseMenuPanel.GetComponent<Image>();

        pauseFadeTween?.Kill();
        pauseFadeTween = null;

        foreach (GameObject button in pauseMenuButtons)
        {
            if (button.TryGetComponent(out UITween buttonTween))
            {
                buttonTween.Show();
            }
        }

        pauseFadeTween = pauseImage
            .DOFade(0.5f, 0.3f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => pauseFadeTween = null);
    }

    public void ClosePauseMenu()
    {
        pauseMenuOpen = false;

        int thisTransition = ++pauseTransitionVersion;

        Image pauseImage = pauseMenuPanel.GetComponent<Image>();

        pauseFadeTween?.Kill();
        pauseFadeTween = null;

        foreach (GameObject button in pauseMenuButtons)
        {
            if (button.TryGetComponent(out UITween buttonTween))
            {
                buttonTween.Hide();
            }
        }

        pauseFadeTween = pauseImage
            .DOFade(0f, 0.3f)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // Ignore this callback if the menu was reopened.
                if (pauseMenuOpen ||
                    thisTransition != pauseTransitionVersion)
                {
                    return;
                }

                pauseMenuPanel.SetActive(false);
                pauseFadeTween = null;
            });
    }
}