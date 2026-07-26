using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum Phase
{
    combat,
    build
}

public class GameSession : MonoBehaviour
{
    private bool runInProgress = false;
    public static GameSession instance;

    public UIManager uiManager;
    public LevelDirector levelDirector;

    public RunData run { get; private set; }
    public Phase phase;// { get; private set; }
    public PlayerManager Player { get; private set; }

    [Header("Blood Loss During Combat")]
    public float bloodLossInterval = 1f;
    public int bloodLossIntervalAmt = 1;

    private float deltaTimeValue = 0.02f;

    public void ResetGameSpeed()
    {
        gameSpeedTween?.Kill();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = deltaTimeValue;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        deltaTimeValue = Time.fixedDeltaTime;
        if (GameSession.instance == null)
            GameSession.instance = this;
        else
            Destroy(this.gameObject);

        ResetGameSpeed();

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();

        DisablePlayerMovement(true);

        uiManager.showOpeningLetter();

    }

    public void StartRun()
    {
        run = new RunData();
        runInProgress = true;

        StartWave();

    }

    public void EndWave()
    {
        uiManager.shopManager.GenerateShop();
        StopAllCoroutines();
        phase = Phase.build;
        MusicManager.instance.stopMusic();
        MusicManager.instance.startMusic(phase);
        // open shop
        uiManager.ResetRefreshPrice();
        uiManager.OpenShopPanel();
    }

    public void StartWave()
    {
        run.day++;
        phase = Phase.combat;
        MusicManager.instance.stopMusic();
        MusicManager.instance.startMusic(phase);
        levelDirector.SpawnWave(run.day);

        uiManager.SetDay(run.day);
        uiManager.CloseBuildUI();

        StartCoroutine(SubtractBloodOnInterval());
    }

    public void Update()
    {
        //for testing
        if (Input.GetKeyDown("p") && phase == Phase.build)
        {
            StartWave();
        }

        if (Input.GetKeyDown("o") && phase == Phase.combat)
        {
            EndWave();
        }
    }

    public void DamageCastle(int damage)
    {

        SubtractBlood(damage);
    }

    private Tween gameSpeedTween;

    private void LoseGame()
    {
        if (!runInProgress)
            return;

        Debug.Log("Game Loss Triggered");

        runInProgress = false;
        run.playerStats = Player.playerStats;

        uiManager.ShowLoseScreen(run);

        MusicManager.instance?.SlowMusicForGameOver();

        gameSpeedTween?.Kill();

        float startingFixedDeltaTime = Time.fixedDeltaTime;

        gameSpeedTween = DOTween.To(
                () => Time.timeScale,
                value =>
                {
                    Time.timeScale = value;
                    Time.fixedDeltaTime = startingFixedDeltaTime * value;
                },
                0f,
                1f
            )
            .SetEase(Ease.InCubic)
            .SetUpdate(true);
    }

    private IEnumerator SubtractBloodOnInterval()
    {
        while (phase == Phase.combat && runInProgress)
        {

            yield return new WaitForSeconds(bloodLossInterval);
            SubtractBlood(bloodLossIntervalAmt);




        }
    }

    public void SubtractBlood(int amt)
    {
        run.bloodCount -= amt;

        if (amt > bloodLossIntervalAmt)
        {
            uiManager.DoBloodSliderPunch();
        }

        uiManager.SetBloodSlider(run.bloodCount, run.maxBloodCount);

        if (run.bloodCount <= 0)
        {
            run.bloodCount = 0;
            LoseGame();
        }
    }

    public void AddBlood(int amt)
    {
        run.bloodCount += amt;
        if (run.bloodCount > run.maxBloodCount) run.bloodCount = run.maxBloodCount;

        uiManager.SetBloodSlider(run.bloodCount, run.maxBloodCount);
    }

    public void DisablePlayerMovement(bool val)
    {
        Player.playerMovement.ToggleFrozen(val);
    }

    public void RestartGame()
    {
        ResetGameSpeed();
        SceneFader.instance.FadeToScene("GameScene");
    }

    public void ReturnToMenu()
    {
        ResetGameSpeed();
        SceneFader.instance.FadeToScene("MainMenu");
    }
}
