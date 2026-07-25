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

        if (LivingEnemies.Count > 0)
        {
            foreach (GameObject enemy in LivingEnemies)
            {
                if (enemy != null)
                    enemy.GetComponent<Enemy>().Die(0);
            }
        }

        LivingEnemies = new List<GameObject>();

        CalculateBudget(currentDifficulty);
        PrepareEnemies();

        if (spawnCoroutine == null && preparedEnemies.Count > 0)
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

        // Wave 1 only allows index 0.
        // Every few waves, another enemy is added.
        int unlockedEnemyCount = Mathf.Clamp(
            1 + ((currentDifficulty - 1) / wavesPerEnemyUnlock),
            1,
            possibleEnemies.Count
        );

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

        GameObject EnemyGameObject = Instantiate(
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

        LivingEnemies.Remove(enemy.gameObject);

        EnemiesLeft--;
        GameSession.instance.uiManager.SetEnemyCount(EnemiesLeft);

        if (EnemiesLeft <= 0)
        {
            GameSession.instance.EndWave();
        }

        if (EnemiesLeft <= 5)
        {
            SpawnArrows();
        }
    }

    private void SpawnArrows()
    {
        foreach (GameObject enemyObj in LivingEnemies)
        {
            if (enemyObj != null)
            {
                EnemyArrow enemyArrow = Instantiate(arrowObject, GameSession.instance.Player.transform).GetComponent<EnemyArrow>();
                enemyArrow.Initialize(enemyObj.transform);
            }
        }
    }
}