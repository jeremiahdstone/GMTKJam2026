using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int cost = 1;
    [SerializeField] private float health = 50f;
    [SerializeField] private float speed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Seeker seeker;

    [SerializeField] public Transform target;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateTime = 0.25f;
    [SerializeField] private float nextWaypointDistance = 0.2f;
    [SerializeField] private float stoppingDistance = 0.15f;

    private Path path;
    private int currentWaypoint;
    private float nextPathUpdateTime;

    public virtual void Awake(){
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        if(target == null) target = GameObject.FindGameObjectWithTag("Objective").transform;

    }

    // For when object pooling is called.
    public virtual void OnEnable(){
        currentWaypoint = 0;
        nextPathUpdateTime = 0f;
        UpdatePath();
    }

    private void FixedUpdate(){
        Move();
    }

    public virtual void Move()
    {
        if (
            target == null ||
            path == null ||
            path.vectorPath == null ||
            path.vectorPath.Count == 0
        )
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        while (
            currentWaypoint < path.vectorPath.Count &&
            Vector2.Distance(
                rb.position,
                path.vectorPath[currentWaypoint]
            ) <= nextWaypointDistance
        )
        {
            currentWaypoint++;
        }

        if (
            currentWaypoint >= path.vectorPath.Count ||
            Vector2.Distance(rb.position, target.position)
                <= stoppingDistance
        )
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 waypoint = path.vectorPath[currentWaypoint];

        Vector2 direction =
            (waypoint - rb.position).normalized;

        rb.linearVelocity = direction * speed;
    }

    private void OnDisable()
    {
        if (seeker != null)
        {
            seeker.CancelCurrentPathRequest();
        }

        if (path != null)
        {
            path.Release(this);
            path = null;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdatePath()
    {
        if (target == null || seeker == null)
            return;

        if (Time.time < nextPathUpdateTime)
            return;

        // Do not start another request while one is still processing.
        if (!seeker.IsDone())
            return;

        nextPathUpdateTime = Time.time + pathUpdateTime;

        seeker.StartPath(
            rb.position,
            target.position,
            OnPathComplete
        );
    }

    private void OnPathComplete(Path newPath)
    {
        if (!isActiveAndEnabled)
            return;

        if (newPath.error)
        {
            Debug.LogWarning(
                $"{name} failed to calculate a path: {newPath.errorLog}"
            );

            return;
        }

        // Release the previous path back into the path pool.
        if (path != null)
        {
            path.Release(this);
        }

        path = newPath;
        path.Claim(this);

        currentWaypoint = 0;
    }
}
