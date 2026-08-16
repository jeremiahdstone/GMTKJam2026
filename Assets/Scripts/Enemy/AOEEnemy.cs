using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AOEEnemy : Enemy
{

    [SerializeField] private GameObject aoeObject;
    public override void Freeze(float cooldown, GameObject attacker = null)
    {
        base.Freeze(cooldown, attacker);
        
    }

    public override void Unfreeze()
    {
        base.Unfreeze();
    }
}
