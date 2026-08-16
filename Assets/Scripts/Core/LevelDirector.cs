using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    public static LevelDirector instance;


    [Header("Valid Spawn Area")]
    [SerializeField] private float yPosition = -7f;
    [SerializeField] private Vector2 xMinMaxPosition = new Vector2(-9f, 9f);

    [Header("Wave Settings")]
    [SerializeField] private int budget;
    [SerializeField] private int maxEnemies;
    [SerializeField] private float spawnDelay = 0.5f;

    [Header("Enemies")]
    [SerializeField]
    private List<GameObject> possibleEnemies = new List<GameObject>();

    [SerializeField]
    private List<GameObject> preparedEnemies = new List<GameObject>();
    public int EnemiesLeft;

    [Header("Enemy Unlocking")]
    [Tooltip("Set to 0 to unlock all enemy types immediately.")]
    [SerializeField] private int wavesPerEnemyUnlock = 2;

    [Header("Runtime")]
    [SerializeField] private int currentEnemyCount;
    [SerializeField] private List<GameObject> LivingEnemies = new List<GameObject>();

    private int currentDifficulty;

    [Header("Difficulty Scaling")]
    [SerializeField] private int baseBudget = 8;
    [SerializeField] private float budgetGrowthRate = 1.12f;

    [SerializeField] private int baseMaxEnemies = 8;
    [SerializeField] private float enemiesPerDifficulty = 0.75f;

    [SerializeField] private float minimumSpawnDelay = 0.08f;
    [SerializeField] private float spawnDelayReduction = 0.015f;

    [Header("GameObject References")]
    [SerializeField] private GameObject arrowObject;

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (LevelDirector.instance == null)
            LevelDirector.instance = this;
        else
            Destroy(this.gameObject);

    }

    private void CalculateBudget(int difficulty)
    {
        difficulty = Mathf.Max(1, difficulty);

        budget = Mathf.RoundToInt(
            baseBudget *
            Mathf.Pow(budgetGrowthRate, difficulty - 1)
        );

        // Increase the number allowed on screen more slowly.
        maxEnemies = baseMaxEnemies +
            Mathf.FloorToInt(
                (difficulty - 1) * enemiesPerDifficulty
            );

        // Gradually make enemies enter the arena faster.
        spawnDelay = Mathf.Max(
            minimumSpawnDelay,
            0.5f - ((difficulty - 1) * spawnDelayReduction)
        );
    }

    public void SpawnWave(int difficulty)
    {
        currentDifficulty = Mathf.Max(1, difficulty);

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        List<GameObject> enemiesToClear = new List<GameObject>(LivingEnemies);
        LivingEnemies.Clear();

        currentEnemyCount = 0;
        EnemiesLeft = 0;

        if (GameSession.instance != null && GameSession.instance.uiManager != null)
        {
            GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);
        }

        foreach (GameObject enemy in enemiesToClear)
        {
            if (enemy == null)
                continue;

            Enemy enemyComponent = enemy.GetComponent<Enemy>();

            if (enemyComponent != null)
            {
                enemyComponent.Die(0, false);
            }
            else
            {
                Destroy(enemy);
            }
        }

        CalculateBudget(currentDifficulty);
        PrepareEnemies();

        if (preparedEnemies.Count > 0)
        {
            spawnCoroutine = StartCoroutine(SpawnWaveOverTime());
        }
    }

    private IEnumerator SpawnWaveOverTime()
    {
        while (
            currentEnemyCount < maxEnemies &&
            preparedEnemies.Count > 0
        )
        {
            SpawnEnemy();

            yield return new WaitForSeconds(spawnDelay);
        }

        spawnCoroutine = null;
    }

    private void PrepareEnemies()
    {
        preparedEnemies.Clear();

        int currentBudget = budget;

        while (currentBudget > 0)
        {
            List<GameObject> affordableEnemies =
                GetAffordableEnemies(currentBudget);

            // Nothing can be purchased with the remaining budget.
            if (affordableEnemies.Count == 0)
                break;

            int randomIndex = Random.Range(
                0,
                affordableEnemies.Count
            );

            GameObject chosenEnemy =
                affordableEnemies[randomIndex];

            Enemy enemy = chosenEnemy.GetComponent<Enemy>();

            currentBudget -= enemy.cost;
            preparedEnemies.Add(chosenEnemy);
        }

        EnemiesLeft = preparedEnemies.Count;
        GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);
    }

    private List<GameObject> GetAffordableEnemies(int currentBudget)
    {
        List<GameObject> affordableEnemies = new List<GameObject>();

        int unlockedEnemyCount;

        if (wavesPerEnemyUnlock <= 0)
        {
            unlockedEnemyCount = possibleEnemies.Count;
        }
        else
        {
            unlockedEnemyCount = Mathf.Clamp(
                1 + ((currentDifficulty - 1) / wavesPerEnemyUnlock),
                1,
                possibleEnemies.Count
            );
        }

        for (int i = 0; i < unlockedEnemyCount; i++)
        {
            GameObject enemyObject = possibleEnemies[i];

            if (enemyObject == null)
                continue;

            Enemy enemy = enemyObject.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogWarning(
                    $"{enemyObject.name} has no Enemy component."
                );

                continue;
            }

            if (enemy.cost <= currentBudget)
            {
                affordableEnemies.Add(enemyObject);
            }
        }

        return affordableEnemies;
    }

    private void SpawnEnemy()
    {
        if (preparedEnemies.Count == 0)
            return;

        GameObject enemyPrefab = preparedEnemies[0];

        Vector2 spawnPosition = new Vector2(
            Random.Range(
                xMinMaxPosition.x,
                xMinMaxPosition.y
            ),
            yPosition
        );

        GameObject EnemyGameObject = PoolManager.instance.Spawn(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        preparedEnemies.RemoveAt(0);
        currentEnemyCount++;
        LivingEnemies.Add(EnemyGameObject);
    }

    public void NotifyEnemyRemoved(Enemy enemy)
    {
        currentEnemyCount = Mathf.Max(
            0,
            currentEnemyCount - 1
        );

        // Continue spawning the prepared wave when a slot opens.
        if (
            preparedEnemies.Count > 0 &&
            spawnCoroutine == null
        )
        {
            spawnCoroutine = StartCoroutine(
                SpawnWaveOverTime()
            );
        }

        if (LivingEnemies.Contains(enemy.gameObject))
        {
            LivingEnemies.Remove(enemy.gameObject);

            EnemiesLeft = Mathf.Max(0, EnemiesLeft - 1);

            if (GameSession.instance != null && GameSession.instance.uiManager != null)
            {
                GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);
            }

            if (GameSession.instance != null && GameSession.instance.run != null)
            {
                GameSession.instance.run.enemiesKilled++;
            }
        }

        if (EnemiesLeft <= 0 && GameSession.instance != null)
        {
            GameSession.instance.EndWave();
        }

        if (EnemiesLeft <= 5)
        {
            SpawnArrows();
        }
    }

    public void ReplaceEnemy(Enemy oldEnemy, Enemy newEnemy)
    {
        if (oldEnemy == null || newEnemy == null)
            return;

        if (LivingEnemies.Contains(oldEnemy.gameObject))
        {
            LivingEnemies.Remove(oldEnemy.gameObject);
            LivingEnemies.Add(newEnemy.gameObject);
        }
    }

    private void SpawnArrows()
    {
        foreach (GameObject enemyObj in LivingEnemies)
        {
            if (enemyObj != null)
            {
                EnemyArrow enemyArrow = PoolManager.instance.Spawn(arrowObject, GameSession.instance.Player.transform).GetComponent<EnemyArrow>();
                enemyArrow.Initialize(enemyObj.transform);
            }
        }
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            if (GameSession.instance != null)
            {
                ClearEnemies();
                GameSession.instance.EndWave();
            }
        }
    }

    public void ClearEnemies()
    {
        List<GameObject> enemiesToClear = new List<GameObject>(LivingEnemies);
        LivingEnemies.Clear();

        currentEnemyCount = 0;
        EnemiesLeft = 0;

        if (GameSession.instance != null && GameSession.instance.uiManager != null)
        {
            GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);
        }

        foreach (GameObject enemy in enemiesToClear)
        {
            if (enemy == null)
                continue;

            Enemy enemyComponent = enemy.GetComponent<Enemy>();

            if (enemyComponent != null)
            {
                enemyComponent.Die(0, false);
            }
            else
            {
                PoolManager.instance.Release(enemy);
            }
        }
    }


}