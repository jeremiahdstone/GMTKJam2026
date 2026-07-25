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

    [Header("Runtime")]
    [SerializeField] private int currentEnemyCount;

    [Header("Difficulty Scaling")]
    [SerializeField] private int baseBudget = 8;
    [SerializeField] private float budgetGrowthRate = 1.12f;

    [SerializeField] private int baseMaxEnemies = 8;
    [SerializeField] private float enemiesPerDifficulty = 0.75f;

    [SerializeField] private float minimumSpawnDelay = 0.08f;
    [SerializeField] private float spawnDelayReduction = 0.015f;

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
        CalculateBudget(difficulty);
        PrepareEnemies();

        if (
            spawnCoroutine == null &&
            preparedEnemies.Count > 0
        )
        {
            spawnCoroutine = StartCoroutine(
                SpawnWaveOverTime()
            );
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

    private List<GameObject> GetAffordableEnemies(
        int currentBudget
    )
    {
        List<GameObject> affordableEnemies =
            new List<GameObject>();

        foreach (GameObject enemyObject in possibleEnemies)
        {
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

        Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        preparedEnemies.RemoveAt(0);
        currentEnemyCount++;
    }

    public void NotifyEnemyRemoved()
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

        EnemiesLeft--;
        GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);

        if(EnemiesLeft <= 0)
        {
            GameSession.instance.EndWave();
        }
    }
}