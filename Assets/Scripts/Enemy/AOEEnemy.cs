using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AOEEnemy : Enemy
{
    [Header("AOE Enemy Settings")]
    [SerializeField] private AOEBehavior aoeBehavior;
    [SerializeField] private float radius;
    [SerializeField] private float scaledRadius;
    [SerializeField] private float aoeDamage = 3;
    [SerializeField] private float scaledAoeDamage = 3f;
    [SerializeField] private float aoeDamageIncreasePercentagePerDay = 0.05f;
    [SerializeField] private float radiusIncreasePercentagePerDay = 0;

    public override void Awake()
    {
        base.Awake();
        aoeBehavior = GetComponentInChildren<AOEBehavior>();
        aoeBehavior.RefreshVisual(radius);
    }

    public override void CalculateStats(int day)
    {
        base.CalculateStats(day);

        if(aoeBehavior is PlayerDamageAOE)
        {
            scaledAoeDamage = aoeDamage + (aoeDamage * day * aoeDamageIncreasePercentagePerDay);

            PlayerDamageAOE damageAOE = aoeBehavior as PlayerDamageAOE;
            
            damageAOE.SetDamageAmount(Mathf.RoundToInt(scaledAoeDamage));

        }

        scaledRadius = radius + (radius * day * radiusIncreasePercentagePerDay);

        aoeBehavior.RefreshVisual(scaledRadius);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        
    }
    public override void Freeze(float cooldown, GameObject attacker = null)
    {
        base.Freeze(cooldown, attacker);

        aoeBehavior.Hide();
        
    }

    public override void Unfreeze()
    {
        base.Unfreeze();

        aoeBehavior.Show();
    }
}
