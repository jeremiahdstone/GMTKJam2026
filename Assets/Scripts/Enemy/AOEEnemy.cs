using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AOEEnemy : Enemy
{
    [Header("AOE Enemy Settings")]
    [SerializeField] private float radius;
    [SerializeField] private AOEBehavior aoeBehavior;

    public override void Awake()
    {
        base.Awake();
        aoeBehavior = GetComponentInChildren<AOEBehavior>();
        aoeBehavior.RefreshVisual(radius);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        aoeBehavior.RefreshVisual(radius);
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
