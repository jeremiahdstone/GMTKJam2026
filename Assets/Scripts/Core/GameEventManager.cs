using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event Action OnWaveStart;
    public event Action OnWaveEnd;
    public event Action<Transform, bool> OnBite;
    public event Action OnBatModeEnter;
    public event Action OnBatModeExit;
    public event Action<GameObject> OnEnemyDeath;
    public event Action<GameObject> OnEnemyHit;
    public event Action OnCastleHit;
    public event Action OnGameLose;

    public void WaveStart()
    {
        OnWaveStart?.Invoke();
    }
    public void WaveEnd()
    {
        OnWaveEnd?.Invoke();
    }

    public void Bite(Transform bittenTransform, bool fullyCharged = false)
    {
        OnBite?.Invoke(bittenTransform, fullyCharged);
    }

    public void BatModeEnter()
    {
        OnBatModeEnter?.Invoke();
    }

    public void BatModeExit()
    {
        OnBatModeExit?.Invoke();
    }

    public void EnemyDeath(GameObject enemy)
    {
        OnEnemyDeath?.Invoke(enemy);
    }

    public void EnemyHit(GameObject enemy)
    {
        OnEnemyHit?.Invoke(enemy);
    }

    public void CastleHit()
    {
        OnCastleHit?.Invoke();
    }

    public void GameLose()
    {
        OnGameLose?.Invoke();
    }
}