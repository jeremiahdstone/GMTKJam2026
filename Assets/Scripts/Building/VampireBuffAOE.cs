using UnityEngine;
using System.Collections.Generic;

public class VampireBuffAOE : AOEBehavior
{
    private VampireBuffTrap trap;

    protected override void OnEnable()
    {
        base.OnEnable();
        trap = GetComponentInParent<VampireBuffTrap>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerStats == null)
            return;

        foreach (PlayerBuff buff in trap.GetPlayerBuffs())
        {
            playerStats.AddBuff(buff);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerStats == null)
            return;

        foreach (PlayerBuff buff in trap.GetPlayerBuffs())
        {
            playerStats.RemoveBuff(buff);
        }
    }
}