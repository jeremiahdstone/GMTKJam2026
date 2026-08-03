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

    public event Action OnWaveStart; // not implemented
    public event Action OnWaveEnd; // not implemented
    public event Action OnBite;
    public event Action OnBatModeEnter;
    public event Action OnBatModeExit;
    public event Action OnEnemyDeath;
    public event Action OnEnemyHit;
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

    public void Bite()
    {
        OnBite?.Invoke();
    }

    public void BatModeEnter()
    {
        OnBatModeEnter?.Invoke();
    }

    public void BatModeExit()
    {
        OnBatModeExit?.Invoke();
    }

    public void EnemyDeath()
    {
        OnEnemyDeath?.Invoke();
    }

    public void EnemyHit()
    {
        OnEnemyHit?.Invoke();
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