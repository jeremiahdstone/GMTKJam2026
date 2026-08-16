using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    private Transform target;
    private bool initialized;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeStartDistance = 10f;
    [SerializeField] private float fullyFadedDistance = 5f;
    [SerializeField] private float minimumAlpha = 0f;

    private void Update()
    {
        if (!initialized)
            return;

        if (!target.gameObject.activeInHierarchy)
        {
            PoolManager.instance.Release(gameObject);
            return;
        }

        PointAtTarget();
    }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(Transform newTarget)
    {
        target = newTarget;
        initialized = true;
        PointAtTarget();
    }

    private void PointAtTarget()
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float distance = direction.magnitude;

        // Far away = 1 alpha, close = minimumAlpha.
        float alpha = Mathf.InverseLerp(5f, fadeStartDistance, distance);
        alpha = Mathf.Lerp(minimumAlpha, 1f, alpha);

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}