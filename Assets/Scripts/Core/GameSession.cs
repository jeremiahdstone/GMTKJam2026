using System.Collections;
using UnityEngine;

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
    public Phase phase { get; private set; }

    [Header("Blood Loss")]
    public float bloodLossInterval = 1f;
    public int bloodLossIntervalAmt = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (GameSession.instance == null)
            GameSession.instance = this;
        else
            Destroy(this.gameObject);

        StartRun();
    }

    public void StartRun()
    {
        run = new RunData();
        runInProgress = true;

        StartWave();

    }

    public void EndWave()
    {
        StopAllCoroutines();
        phase = Phase.build;
        // open shop
    }

    public void StartWave()
    {
        run.day++;
        phase = Phase.combat;
        levelDirector.SpawnWave(run.day);

        uiManager.SetDay(run.day);

        StartCoroutine(SubtractBloodOnInterval());
    }

    public void Update()
    {
        //for testing
        if (Input.GetKeyDown("p") && phase == Phase.build)
        {
            StartWave();
        }
    }

    public void DamageCastle(int damage)
    {

        SubtractBlood(damage);
    }

    private void LoseGame()
    {
        Debug.Log("Game Loss Triggered");
        runInProgress = false;
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
        if(run.bloodCount > run.maxBloodCount) run.bloodCount = run.maxBloodCount;

        uiManager.SetBloodSlider(run.bloodCount, run.maxBloodCount);
    }
}
