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
    public event Action<Transform, float> OnBite;
    public event Action OnBatModeEnter;
    public event Action OnBatModeExit;
    public event Action<GameObject, GameObject> OnEnemyDeath;
    public event Action<GameObject, GameObject> OnEnemyHit;
    public event Action OnCastleHit;
    public event Action OnGameLose;
    public event Action <GameObject, GameObject> OnPlayerHit;
    public event Action <Upgrade> OnUpgradePurchased; // not implemented
    public event Action <Trap> OnTrapPurchased; // not implemented

    public void WaveStart()
    {
        OnWaveStart?.Invoke();
    }
    public void WaveEnd()
    {
        OnWaveEnd?.Invoke();
    }

    public void Bite(Transform bittenTransform, float chargeAmount = 0f)
    {
        OnBite?.Invoke(bittenTransform, chargeAmount);
    }

    public void BatModeEnter()
    {
        OnBatModeEnter?.Invoke();
    }

    public void BatModeExit()
    {
        OnBatModeExit?.Invoke();
    }

    public void EnemyDeath(GameObject enemy, GameObject attacker)
    {
        OnEnemyDeath?.Invoke(enemy, attacker);
    }

    public void EnemyHit(GameObject enemy, GameObject attacker)
    {
        OnEnemyHit?.Invoke(enemy, attacker);
    }

    public void CastleHit()
    {
        OnCastleHit?.Invoke();
    }

    public void GameLose()
    {
        OnGameLose?.Invoke();
    }

    public void PlayerHit(GameObject player, GameObject attacker)
    {
        OnPlayerHit?.Invoke(player, attacker);
    }

    public void UpgradePurchased(Upgrade upgrade)
    {
        OnUpgradePurchased?.Invoke(upgrade);
    }

    public void TrapPurchased(Trap trap)
    {
        OnTrapPurchased?.Invoke(trap);
    }
}