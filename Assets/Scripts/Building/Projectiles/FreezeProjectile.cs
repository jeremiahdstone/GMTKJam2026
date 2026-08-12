using UnityEngine;

public class FreezeProjectile : Projectile
{
    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 2f;

    protected override void OnHit(Collider2D other)
    {
        if (other.TryGetComponent(out IFreezable freezable))
        {
            freezable.Freeze(freezeDuration);
        }

        base.OnHit(other);
    }
}