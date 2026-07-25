using UnityEngine;

public enum Phase
{
    combat,
    build
}

public class GameSession : MonoBehaviour
{
    public static GameSession instance;

    public UIManager uiManager;
    public LevelDirector levelDirector;

    public RunData run {get; private set;}
    public Phase phase {get; private set;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(GameSession.instance == null)
            GameSession.instance = this;
        else
            Destroy(this.gameObject);

        StartRun();
    }

    public void StartRun()
    {
        run = new RunData();
        

        StartWave();
        
    }

    public void EndWave()
    {
        phase = Phase.build;
        // open shop
    }

    public void StartWave()
    {
        run.day++;
        phase = Phase.combat;
        levelDirector.SpawnWave(run.day);

        uiManager.SetDay(run.day);
    }

    public void Update()
    {
        //for testing
        if (Input.GetKeyDown("p") && phase == Phase.build)
        {
            StartWave();
        }
    }
}
