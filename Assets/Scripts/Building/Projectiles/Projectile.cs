using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Visuals")]
    [SerializeField] private bool spins = false;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private GameObject smokePuff;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 direction;

    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

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
            Destroy(gameObject);
            return;
        }

        // Hit an enemy
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.Damage(damage);
            Destroy(gameObject);
        }
    }
}