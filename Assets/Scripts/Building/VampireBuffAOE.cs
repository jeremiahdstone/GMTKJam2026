using UnityEngine;
using System.Collections.Generic;

public class VampireBuffAOE : MonoBehaviour
{
    private VampireBuffTrap trap;

    private void Awake()
    {
        trap = GetComponentInParent<VampireBuffTrap>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.LogWarning("Player entered Vampire Buff AOE");
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            return;

        foreach (PlayerBuff buff in trap.GetPlayerBuffs())
        {
            playerStats.AddBuff(buff);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            return;

        foreach (PlayerBuff buff in trap.GetPlayerBuffs())
        {
            playerStats.RemoveBuff(buff);
        }
    }
}