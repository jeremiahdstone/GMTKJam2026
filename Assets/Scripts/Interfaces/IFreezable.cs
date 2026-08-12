using UnityEngine;

public interface IFreezable
{
    public void Freeze(float duration, GameObject attacker = null);
    public void Unfreeze();
}