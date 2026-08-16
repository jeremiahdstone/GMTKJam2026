using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private Team team = Team.good;
    [SerializeField] private bool friendlyFire = false;
    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] protected float damage = 5f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Visuals")]
    [SerializeField] private bool spins = false;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] protected GameObject smokePuff;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 direction;

    public virtual void Initialize(Vector2 fireDirection, Trap sourceTrap)
    {
        direction = fireDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //grab relevant trap stats from trap
        speed = sourceTrap.GetStat(TrapStat.ProjectileSpeed);
        damage = sourceTrap.GetStat(TrapStat.Damage);

        Destroy(gameObject, lifeTime);
    }

    public virtual void InitializeFromFlatStats(Vector2 fireDirection, float givenSpeed, float givenDamage)
    {
        direction = fireDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //grab relevant projectile stats from method call
        speed = givenSpeed;
        damage = givenDamage;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (spins)
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit a wall/obstacle
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Instantiate(smokePuff, transform.position, smokePuff.transform.rotation);
            OnHit(other);
            return;
        }

        // Hit an enemy
        if (other.TryGetComponent(out Enemy enemy))
        {
            if (enemy.team == team && !friendlyFire)
                return;
            
            if (damage != 0) { 
                enemy.Damage(damage);
            }

            OnHit(other);
        }

        if (other.TryGetComponent(out PlayerManager player))
        {
            if(team == Team.good && !friendlyFire)
                return;
            player.Damage(damage);
            OnHit(other);
        }
    }

    protected virtual void OnHit(Collider2D other)
    {
        Destroy(gameObject);
    }
}