using UnityEngine;
using System.Collections.Generic;

public class VampireBuffTrap : Trap
{
    [Header("Vampire Buff Trap Settings")]
    [SerializeField]
    private List<PlayerBuff> playerBuffs = new();
    private PlayerBuff biteDamageBuff;
    private PlayerBuff biteCooldownBuff;
    private PlayerBuff biteSpeedBuff;
    private PlayerBuff walkSpeedBuff;

    

    protected override void Awake()
    {
        base.Awake();

        // Bite Damage +50%
        biteDamageBuff = gameObject.AddComponent<PlayerBuff>();
        biteDamageBuff.id = "vampire-buff-trap-bite-damage";
        biteDamageBuff.affectedStat = PlayerStat.BiteDamage;
        biteDamageBuff.percentBonus = 0.50f;
        playerBuffs.Add(biteDamageBuff);

        // Bite Cooldown -50%
        biteCooldownBuff = gameObject.AddComponent<PlayerBuff>();
        biteCooldownBuff.id = "vampire-buff-trap-bite-cooldown";
        biteCooldownBuff.affectedStat = PlayerStat.BiteCooldown;
        biteCooldownBuff.percentBonus = -0.50f;
        playerBuffs.Add(biteCooldownBuff);

        // Bite Speed +50%
        biteSpeedBuff = gameObject.AddComponent<PlayerBuff>();
        biteSpeedBuff.id = "vampire-buff-trap-bite-speed";
        biteSpeedBuff.affectedStat = PlayerStat.BiteSpeedMultiplier;
        biteSpeedBuff.percentBonus = 0.50f;
        playerBuffs.Add(biteSpeedBuff);

        // Walk Speed +50%
        walkSpeedBuff = gameObject.AddComponent<PlayerBuff>();
        walkSpeedBuff.id = "vampire-buff-trap-walk-speed";
        walkSpeedBuff.affectedStat = PlayerStat.WalkSpeed;
        walkSpeedBuff.percentBonus = 0.50f;
        playerBuffs.Add(walkSpeedBuff);
    }

    public List<PlayerBuff> GetPlayerBuffs()
    {
        return playerBuffs;
    }
}