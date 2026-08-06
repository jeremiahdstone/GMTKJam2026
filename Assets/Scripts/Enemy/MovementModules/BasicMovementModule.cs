using UnityEngine;
using Pathfinding;
using System.Collections;

public class BasicMovementModule : MonoBehaviour, IMovementModule
{
    [Header("Pathfinding")]
    public Transform target;
    [SerializeField] private float pathUpdateTime = 0.25f;
    [SerializeField] private float nextWaypointDistance = 0.2f;
    [SerializeField] private float stoppingDistance = 0.15f;

    [Header("Enemy Avoidance")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float avoidanceRadius = 0.8f;
    [SerializeField] private float avoidanceStrength = 1.25f;

    private Enemy enemy;
    private Rigidbody2D rb;
    private Seeker seeker;

    private Vector2 avoidanceDirection;

    private Path path;
    private int currentWaypoint;
    private float nextPathUpdateTime;
    private float lastHorizontalDirection = 1f;

    private Coroutine pathUpdateCoroutine;

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        if (target == null && enemy.team == Team.bad)
            target = GameObject.FindGameObjectWithTag("Objective").transform;
    }

    public void OnEnableModule()
    {
        currentWaypoint = 0;
        nextPathUpdateTime = 0f;
        lastHorizontalDirection = 1f;

        if (pathUpdateCoroutine != null)
            StopCoroutine(pathUpdateCoroutine);

        pathUpdateCoroutine = StartCoroutine(UpdatePathOnInterval());
    }

    public void OnDisableModule()
    {
        if (pathUpdateCoroutine != null)
        {
            StopCoroutine(pathUpdateCoroutine);
            pathUpdateCoroutine = null;
        }

        if (seeker != null)
            seeker.CancelCurrentPathRequest();

        if (path != null)
        {
            path.Release(this);
            path = null;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator UpdatePathOnInterval()
    {
        while (enemy != null && target != null)
        {
            UpdatePath();
            ManeuverAroundNearbyEnemies();
            yield return new WaitForSeconds(pathUpdateTime);
        }
    }

    public void Move()
    {
        if (enemy == null)
            return;

        if (target == null || path == null || path.vectorPath == null || path.vectorPath.Count == 0)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        while (currentWaypoint < path.vectorPath.Count && Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]) <= nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (currentWaypoint >= path.vectorPath.Count || Vector2.Distance(rb.position, target.position) <= stoppingDistance)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 waypoint = path.vectorPath[currentWaypoint];
        Vector2 direction = (waypoint - rb.position).normalized;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            lastHorizontalDirection = Mathf.Sign(direction.x);
        }

        if (enemy != null && enemy.GetSpriteRenderer() != null)
        {
            enemy.GetSpriteRenderer().flipX = lastHorizontalDirection < 0f;
        }

        if (rb != null)
            rb.linearVelocity = direction * enemy.currentSpeed;
    }

    private void UpdatePath()
    {
        if (enemy == null || target == null || seeker == null || rb == null)
            return;

        if (Time.time < nextPathUpdateTime)
            return;

        if (!seeker.IsDone())
            return;

        nextPathUpdateTime = Time.time + pathUpdateTime;

        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void ManeuverAroundNearbyEnemies()
    {
        if (enemy == null || rb == null)
            return;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius, enemyLayers);

        Vector2 totalAvoidance = Vector2.zero;
        int nearbyCount = 0;

        foreach (Collider2D nearbyCollider in nearbyEnemies)
        {
            if (nearbyCollider.attachedRigidbody == rb)
                continue;

            Enemy nearbyEnemy = nearbyCollider.GetComponent<Enemy>();
            nearbyEnemy ??= nearbyCollider.GetComponentInParent<Enemy>();

            if (nearbyEnemy == null || nearbyEnemy == enemy)
                continue;

            Vector2 awayDirection = rb.position - (Vector2)nearbyEnemy.transform.position;
            float distance = awayDirection.magnitude;

            if (distance <= 0.001f)
            {
                awayDirection = Random.insideUnitCircle.normalized;
                distance = 0.001f;
            }

            float closeness = 1f - Mathf.Clamp01(distance / avoidanceRadius);

            totalAvoidance += awayDirection.normalized * closeness;
            nearbyCount++;
        }

        if (nearbyCount > 0)
        {
            avoidanceDirection = (totalAvoidance / nearbyCount).normalized;
        }
        else
        {
            avoidanceDirection = Vector2.zero;
        }
    }

    private void OnPathComplete(Path newPath)
    {
        if (!isActiveAndEnabled)
            return;

        if (newPath.error)
        {
            Debug.LogWarning($"{name} failed to calculate a path: {newPath.errorLog}");
            return;
        }

        if (path != null)
        {
            path.Release(this);
        }

        path = newPath;
        path.Claim(this);

        currentWaypoint = 0;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        UpdatePath();
    }

    public Transform GetTarget()
    {
        return target;
    }
}
