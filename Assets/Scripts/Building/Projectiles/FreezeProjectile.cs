using UnityEngine;

public class FreezeProjectile : Projectile
{
    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 2f;

    public override void Initialize(Vector2 fireDirection, Trap sourceTrap)
    {
        base.Initialize(fireDirection, sourceTrap);

        freezeDuration = sourceTrap.GetStat(TrapStat.Duration);
    }

    protected override void OnHit(Collider2D other)
    {
        if (other.TryGetComponent(out IFreezable freezable))
        {
            freezable.Freeze(freezeDuration);
        }

        Instantiate(smokePuff, transform.position, smokePuff.transform.rotation);

        base.OnHit(other);
    }
}