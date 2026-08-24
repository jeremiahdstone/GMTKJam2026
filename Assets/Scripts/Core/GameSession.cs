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
    private bool waveEnding = false;
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
        if (waveEnding)
            return;
        
        waveEnding = true;

        phase = Phase.build;
        GameEventManager.instance.WaveEnd();
        uiManager.shopManager.GenerateShop();
        StopAllCoroutines();
        MusicManager.instance.stopMusic();
        MusicManager.instance.startMusic(phase);
        // open shop
        waveEnding = false;
        uiManager.ResetRefreshPrice();
        uiManager.OpenShopPanel();
    }

    public void StartWave()
    {
        run.day++;

        phase = Phase.combat;
        GameEventManager.instance.WaveStart();

        MusicManager.instance.stopMusic();
        MusicManager.instance.startMusic(phase);
        levelDirector.SpawnWave(run.day);

        uiManager.SetDay(run.day);
        uiManager.CloseBuildUI();

        StartCoroutine(SubtractBloodOnInterval());
    }

    public void Update()
    {
        
    }

    public void DamageCastle(int damage)
    {
        if(!runInProgress) return;
        GameEventManager.instance.CastleHit();
        SubtractBlood(damage);
    }

    private Tween gameSpeedTween;

    public void LoseGame()
    {
        if (!runInProgress)
            return;

        GameEventManager.instance.GameLose();

        Debug.Log("Game Loss Triggered");

        runInProgress = false;
        run.playerStats = Player.playerStats;

        uiManager.ShowLoseScreen(run);

        DisablePlayerMovement(true);

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

    private bool isPaused;

    public void TogglePauseGame()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            uiManager.OpenPauseMenu();
            MusicManager.instance.TweenMusicPitch(0.8f, 0.2f);
        }
        else
        {
            uiManager.ClosePauseMenu();
            MusicManager.instance.TweenMusicPitch(1f, 0.2f);
            
        }
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

        if(!runInProgress) return;
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
        if(!runInProgress) return;
        run.bloodCount += amt;
        if (run.bloodCount > run.maxBloodCount) run.bloodCount = run.maxBloodCount;

        uiManager.SetBloodSlider(run.bloodCount, run.maxBloodCount);
    }

    public void updateMaxBlood()
    {
        run.maxBloodCount = Mathf.RoundToInt(Player.playerStats.GetStat(PlayerStat.MaxBlood));
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
