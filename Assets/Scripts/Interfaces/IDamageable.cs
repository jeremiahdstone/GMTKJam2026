using UnityEngine;

public interface IDamageable
{
    public void Damage(float damage, Transform attacker = null);
}