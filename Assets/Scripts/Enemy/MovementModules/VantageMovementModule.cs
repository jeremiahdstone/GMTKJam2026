using UnityEngine;
using Pathfinding;

public class VantageMovementModule : MonoBehaviour, IMovementModule
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Seeker seeker;
    [SerializeField] private Enemy enemy;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float nextWaypointDistance = 0.2f;
    [SerializeField] private float arrivalDistance = 0.35f;

    [Header("Vantage Distance")]
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float minimumDistance = 3f;
    [SerializeField] private float maximumDistance = 7f;

    [Header("Vantage Search")]
    [SerializeField, Min(1)] private int searchRings = 3;
    [SerializeField, Min(4)] private int samplesPerRing = 12;
    [SerializeField] private float maximumNodeSnapDistance = 1.5f;

    [Tooltip("How much preferred range matters compared with travel distance.")]
    [SerializeField] private float rangePreferenceWeight = 2f;

    [Tooltip("How much crowded destinations are penalized.")]
    [SerializeField] private float crowdPreferenceWeight = 3f;

    [Header("Intermittent Movement")]
    [SerializeField] private float minimumHoldTime = 2f;
    [SerializeField] private float maximumHoldTime = 4f;
    [SerializeField] private float minimumRepositionDistance = 2f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector2 enemySightOffset;
    [SerializeField] private Vector2 targetSightOffset;

    [Header("Enemy Avoidance")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Enemies start steering around one another inside this radius.")]
    [SerializeField] private float avoidanceRadius = 1.4f;

    [Tooltip("Desired amount of personal space between enemies.")]
    [SerializeField] private float preferredEnemySpacing = 0.9f;

    [Tooltip("Strength of local separation while moving.")]
    [SerializeField] private float avoidanceStrength = 1.75f;

    [Tooltip("Strength of sidestepping when another enemy blocks the path ahead.")]
    [SerializeField] private float sidestepStrength = 0.8f;

    [Tooltip("How often nearby enemies are queried. Lower is more responsive, higher is cheaper.")]
    [SerializeField] private float avoidanceUpdateInterval = 0.08f;

    [Tooltip("How many nearby enemy colliders can be considered.")]
    [SerializeField, Min(1)] private int maxNeighbors = 12;

    [Tooltip("Enemies won't intentionally choose vantage points this close together.")]
    [SerializeField] private float destinationSpacing = 0.8f;


    private Transform target;

    private Path path;
    private int currentWaypoint;

    private Vector2 currentVantagePoint;
    private bool hasVantagePoint;
    private bool holdingPosition;
    private bool moduleActive;

    private float nextRepositionTime;
    private float nextAvoidanceUpdate;

    private Collider2D[] neighborBuffer;
    private int neighborCount;

    private Vector2 cachedSeparation;
    private bool cachedPathBlocked;


    // =========================================================
    // IMovementModule
    // =========================================================

    public void Initialize(Enemy enemy)
    {
        this.enemy = enemy;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (seeker == null)
            seeker = GetComponent<Seeker>();

        if (target == null && enemy.team == Team.bad)
            target = GameObject.FindGameObjectWithTag("Player").transform;

        if (
            target == null &&
            enemy.team == Team.bad
        )
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }


        neighborBuffer =
            new Collider2D[
                Mathf.Max(1, maxNeighbors)
            ];
    }


    public void OnEnableModule()
    {
        moduleActive = true;

        path = null;
        currentWaypoint = 0;

        hasVantagePoint = false;
        holdingPosition = false;

        nextRepositionTime = 0f;
        nextAvoidanceUpdate = 0f;

        neighborCount = 0;
        cachedSeparation = Vector2.zero;
        cachedPathBlocked = false;

        StopMoving();
    }


    public void OnDisableModule()
    {
        moduleActive = false;

        path = null;
        currentWaypoint = 0;

        hasVantagePoint = false;
        holdingPosition = false;

        StopMoving();
    }


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        path = null;
        currentWaypoint = 0;

        hasVantagePoint = false;
        holdingPosition = false;

        nextRepositionTime = 0f;
    }


    public Transform GetTarget()
    {
        return target;
    }


    // =========================================================
    // MOVE
    // =========================================================

    public void Move()
    {
        if (
            !moduleActive ||
            target == null ||
            rb == null ||
            seeker == null
        )
        {
            return;
        }

        Vector2 targetPosition =
            GetTargetPosition();

        Vector2 toTarget =
            targetPosition - rb.position;

        float targetDistance =
            toTarget.magnitude;

        bool inRange =
            targetDistance >= minimumDistance &&
            targetDistance <= maximumDistance;

        bool canSeeTarget =
            HasLineOfSight(
                rb.position + enemySightOffset,
                targetPosition
            );


        // -----------------------------------------------------
        // Holding a vantage point
        // -----------------------------------------------------

        if (holdingPosition)
        {
            bool timerExpired =
                Time.time >= nextRepositionTime;

            bool mustMove =
                timerExpired ||
                !inRange ||
                !canSeeTarget;

            if (!mustMove)
            {
                UpdateNearbyEnemies(Vector2.zero);

                /*
                 * Even while "holding", make room if another
                 * enemy gets too close.
                 */
                if (
                    cachedSeparation.sqrMagnitude >
                    0.01f
                )
                {
                    rb.linearVelocity =
                        cachedSeparation.normalized *
                        enemy.currentSpeed *
                        0.45f;
                }
                else
                {
                    StopMoving();
                }

                return;
            }

            holdingPosition = false;

            /*
             * Force a new destination rather than trying to
             * return to the old vantage point.
             */
            hasVantagePoint = false;
            path = null;
            currentWaypoint = 0;
        }


        // -----------------------------------------------------
        // Current position is already valid
        // -----------------------------------------------------

        if (
            !hasVantagePoint &&
            inRange &&
            canSeeTarget &&
            nextRepositionTime <= 0f
        )
        {
            BeginHolding(rb.position);
            return;
        }


        // -----------------------------------------------------
        // Need a destination
        // -----------------------------------------------------

        if (!hasVantagePoint)
        {
            Vector2 oldVantage =
                currentVantagePoint;

            bool requireNewPosition =
                nextRepositionTime > 0f;

            if (
                TryFindVantagePoint(
                    oldVantage,
                    requireNewPosition,
                    out Vector2 newVantage
                )
            )
            {
                currentVantagePoint =
                    newVantage;

                hasVantagePoint = true;

                RequestPath();
            }
            else
            {
                StopMoving();
            }

            return;
        }


        /*
         * If the target has moved enough that our chosen
         * vantage is no longer useful, choose another.
         */
        if (
            !IsVantagePointValid(
                currentVantagePoint,
                targetPosition
            )
        )
        {
            hasVantagePoint = false;
            path = null;
            currentWaypoint = 0;

            StopMoving();
            return;
        }


        FollowPath();
    }


    // =========================================================
    // VANTAGE SEARCH
    // =========================================================

    private bool TryFindVantagePoint(
        Vector2 previousVantage,
        bool requireDifferentPosition,
        out Vector2 bestPosition
    )
    {
        bestPosition = rb.position;

        if (AstarPath.active == null)
            return false;


        Vector2 targetPosition =
            GetTargetPosition();


        GraphNode currentNode =
            GetNearestWalkableNode(
                rb.position
            );


        float bestScore =
            float.MaxValue;

        bool found =
            false;


        int rings =
            Mathf.Max(1, searchRings);

        int samples =
            Mathf.Max(4, samplesPerRing);


        /*
         * Give different enemies slightly different sample
         * orientations so a crowd doesn't all evaluate the
         * exact same destinations in the exact same order.
         */
        float instanceAngleOffset =
            Mathf.Abs(GetInstanceID() * 37) %
            360f;


        for (int ring = 0; ring < rings; ring++)
        {
            float ringProgress =
                rings == 1
                    ? 0.5f
                    : ring /
                      (float)(rings - 1);


            float radius =
                Mathf.Lerp(
                    minimumDistance,
                    maximumDistance,
                    ringProgress
                );


            float ringOffset =
                ring % 2 == 0
                    ? 0f
                    : 180f / samples;


            for (
                int sample = 0;
                sample < samples;
                sample++
            )
            {
                float angle =
                    sample *
                    (360f / samples) +
                    ringOffset +
                    instanceAngleOffset;


                float radians =
                    angle * Mathf.Deg2Rad;


                Vector2 direction =
                    new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );


                Vector2 rawCandidate =
                    targetPosition +
                    direction * radius;


                GraphNode candidateNode =
                    GetNearestWalkableNode(
                        rawCandidate
                    );


                if (
                    candidateNode == null ||
                    !candidateNode.Walkable
                )
                {
                    continue;
                }


                if (
                    currentNode != null &&
                    candidateNode.Area !=
                    currentNode.Area
                )
                {
                    continue;
                }


                Vector2 candidate =
                    (Vector3)
                    candidateNode.position;


                Vector2 snapOffset =
                    candidate -
                    rawCandidate;

                if (
                    snapOffset.sqrMagnitude >
                    maximumNodeSnapDistance *
                    maximumNodeSnapDistance
                )
                {
                    continue;
                }


                Vector2 candidateToTarget =
                    candidate -
                    targetPosition;

                float candidateTargetDistance =
                    candidateToTarget.magnitude;


                if (
                    candidateTargetDistance <
                    minimumDistance ||
                    candidateTargetDistance >
                    maximumDistance
                )
                {
                    continue;
                }


                if (
                    requireDifferentPosition &&
                    (
                        candidate -
                        previousVantage
                    ).sqrMagnitude <
                    minimumRepositionDistance *
                    minimumRepositionDistance
                )
                {
                    continue;
                }


                if (
                    !HasLineOfSight(
                        candidate +
                        enemySightOffset,
                        targetPosition
                    )
                )
                {
                    continue;
                }


                int nearbyEnemies =
                    CountEnemiesNearPoint(
                        candidate,
                        destinationSpacing
                    );


                /*
                 * If somebody is already basically occupying
                 * the spot, don't intentionally path there.
                 */
                if (nearbyEnemies > 0)
                    continue;


                float travelDistance =
                    (
                        candidate -
                        rb.position
                    ).magnitude;


                float rangeError =
                    Mathf.Abs(
                        candidateTargetDistance -
                        preferredDistance
                    );


                /*
                 * Also look at a slightly larger radius so
                 * otherwise-valid vantage points spread around
                 * the target instead of forming one blob.
                 */
                int crowdCount =
                    CountEnemiesNearPoint(
                        candidate,
                        destinationSpacing * 2f
                    );


                float score =
                    travelDistance +
                    rangeError *
                    rangePreferenceWeight +
                    crowdCount *
                    crowdPreferenceWeight;


                if (score >= bestScore)
                    continue;


                bestScore =
                    score;

                bestPosition =
                    candidate;

                found =
                    true;
            }
        }


        return found;
    }


    // =========================================================
    // PATH FOLLOWING
    // =========================================================

    private void RequestPath()
    {
        if (
            !moduleActive ||
            seeker == null ||
            !seeker.IsDone()
        )
        {
            return;
        }


        seeker.StartPath(
            rb.position,
            currentVantagePoint,
            OnPathComplete
        );
    }


    private void OnPathComplete(Path newPath)
    {
        if (
            !moduleActive ||
            newPath == null ||
            newPath.error
        )
        {
            return;
        }


        path =
            newPath;

        currentWaypoint =
            0;
    }


    private void FollowPath()
    {
        float arrivalDistanceSquared =
            arrivalDistance *
            arrivalDistance;


        if (
            (
                currentVantagePoint -
                rb.position
            ).sqrMagnitude <=
            arrivalDistanceSquared
        )
        {
            if (
                IsCurrentPositionValid()
            )
            {
                BeginHolding(
                    rb.position
                );
            }
            else
            {
                hasVantagePoint = false;
                path = null;
                currentWaypoint = 0;

                StopMoving();
            }

            return;
        }


        if (
            path == null ||
            path.vectorPath == null ||
            path.vectorPath.Count == 0
        )
        {
            StopMoving();
            return;
        }


        float waypointDistanceSquared =
            nextWaypointDistance *
            nextWaypointDistance;


        while (
            currentWaypoint <
            path.vectorPath.Count - 1
        )
        {
            Vector2 waypointOffset =
                (Vector2)
                path.vectorPath[
                    currentWaypoint
                ] -
                rb.position;


            if (
                waypointOffset.sqrMagnitude >
                waypointDistanceSquared
            )
            {
                break;
            }


            currentWaypoint++;
        }


        Vector2 waypoint =
            path.vectorPath[
                currentWaypoint
            ];


        Vector2 pathDirection =
            waypoint -
            rb.position;


        if (
            pathDirection.sqrMagnitude <
            0.0001f
        )
        {
            StopMoving();
            return;
        }


        pathDirection.Normalize();


        UpdateNearbyEnemies(
            pathDirection
        );


        Vector2 steering =
            pathDirection +
            cachedSeparation *
            avoidanceStrength;


        /*
         * If another enemy is directly blocking our intended
         * motion, add a consistent "keep right" sidestep.
         *
         * This breaks the symmetry where two enemies push
         * directly into each other forever.
         */
        if (cachedPathBlocked)
        {
            Vector2 right =
                new Vector2(
                    pathDirection.y,
                    -pathDirection.x
                );

            steering +=
                right *
                sidestepStrength;
        }


        if (
            steering.sqrMagnitude <
            0.0001f
        )
        {
            steering =
                pathDirection;
        }
        else
        {
            steering.Normalize();
        }

        enemy.anim.SetBool("isMoving", true);

        spriteRenderer.flipX = steering.x < 0f;

        enemy.anim.SetBool("isMoving", true);

        rb.linearVelocity =
            steering *
            enemy.currentSpeed;

        rb.linearVelocity =
            steering *
            enemy.currentSpeed;
    }


    // =========================================================
    // LOCAL AVOIDANCE
    // =========================================================

    private void UpdateNearbyEnemies(
        Vector2 pathDirection
    )
    {
        if (
            Time.time <
            nextAvoidanceUpdate
        )
        {
            return;
        }


        nextAvoidanceUpdate =
            Time.time +
            avoidanceUpdateInterval;


        neighborCount =
            Physics2D.OverlapCircleNonAlloc(
                rb.position,
                avoidanceRadius,
                neighborBuffer,
                enemyLayer
            );


        Vector2 separation =
            Vector2.zero;

        bool pathBlocked =
            false;


        float radiusSquared =
            avoidanceRadius *
            avoidanceRadius;

        float preferredSpacingSquared =
            preferredEnemySpacing *
            preferredEnemySpacing;


        for (
            int i = 0;
            i < neighborCount;
            i++
        )
        {
            Collider2D collider =
                neighborBuffer[i];


            if (collider == null)
                continue;


            Rigidbody2D otherRb =
                collider.attachedRigidbody;


            if (
                otherRb == null ||
                otherRb == rb
            )
            {
                continue;
            }


            Vector2 away =
                rb.position -
                otherRb.position;


            float distanceSquared =
                away.sqrMagnitude;


            // Perfect or near-perfect overlap.
            if (
                distanceSquared <
                0.0001f
            )
            {
                /*
                 * Deterministic opposite directions prevent
                 * overlapping enemies from choosing identical
                 * escape vectors.
                 */
                float sign =
                    GetInstanceID() <
                    otherRb.GetInstanceID()
                        ? -1f
                        : 1f;


                away =
                    new Vector2(
                        sign,
                        0.35f
                    ).normalized;


                separation +=
                    away * 2f;

                pathBlocked = true;

                continue;
            }


            if (
                distanceSquared >
                radiusSquared
            )
            {
                continue;
            }


            float distance =
                Mathf.Sqrt(
                    distanceSquared
                );


            Vector2 awayDirection =
                away / distance;


            float proximity =
                1f -
                Mathf.Clamp01(
                    distance /
                    avoidanceRadius
                );


            /*
             * Strong nonlinear push when enemies are actually
             * entering one another's personal space.
             */
            if (
                distanceSquared <
                preferredSpacingSquared
            )
            {
                float closeFactor =
                    1f -
                    Mathf.Clamp01(
                        distance /
                        preferredEnemySpacing
                    );


                proximity +=
                    closeFactor *
                    closeFactor *
                    2f;
            }


            separation +=
                awayDirection *
                proximity;


            if (
                pathDirection.sqrMagnitude >
                0.001f
            )
            {
                Vector2 toNeighbor =
                    -awayDirection;


                float ahead =
                    Vector2.Dot(
                        pathDirection,
                        toNeighbor
                    );


                /*
                 * Neighbor is substantially in front of us.
                 */
                if (
                    ahead > 0.55f &&
                    distance <
                    preferredEnemySpacing *
                    1.5f
                )
                {
                    pathBlocked = true;
                }
            }
        }


        /*
         * Don't let a giant group produce an arbitrarily huge
         * steering value.
         */
        if (
            separation.sqrMagnitude >
            1f
        )
        {
            separation.Normalize();
        }


        cachedSeparation =
            separation;

        cachedPathBlocked =
            pathBlocked;
    }


    // =========================================================
    // CROWD CHECK
    // =========================================================

    private int CountEnemiesNearPoint(
        Vector2 point,
        float radius
    )
    {
        int count =
            Physics2D.OverlapCircleNonAlloc(
                point,
                radius,
                neighborBuffer,
                enemyLayer
            );


        int enemies =
            0;


        for (int i = 0; i < count; i++)
        {
            Collider2D collider =
                neighborBuffer[i];


            if (collider == null)
                continue;


            Rigidbody2D otherRb =
                collider.attachedRigidbody;


            if (
                otherRb != null &&
                otherRb != rb
            )
            {
                enemies++;
            }
        }


        return enemies;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private bool IsCurrentPositionValid()
    {
        Vector2 targetPosition =
            GetTargetPosition();


        float distance =
            Vector2.Distance(
                rb.position,
                targetPosition
            );


        if (
            distance < minimumDistance ||
            distance > maximumDistance
        )
        {
            return false;
        }


        return HasLineOfSight(
            rb.position +
            enemySightOffset,
            targetPosition
        );
    }


    private bool IsVantagePointValid(
        Vector2 vantagePoint,
        Vector2 targetPosition
    )
    {
        float distance =
            Vector2.Distance(
                vantagePoint,
                targetPosition
            );


        if (
            distance < minimumDistance ||
            distance > maximumDistance
        )
        {
            return false;
        }


        return HasLineOfSight(
            vantagePoint +
            enemySightOffset,
            targetPosition
        );
    }


    // =========================================================
    // A*
    // =========================================================

    private GraphNode GetNearestWalkableNode(
        Vector2 position
    )
    {
        if (
            AstarPath.active == null ||
            seeker == null
        )
        {
            return null;
        }


        NNConstraint constraint =
            NNConstraint.Default;


        constraint.graphMask =
            seeker.graphMask;

        constraint.constrainWalkability =
            true;

        constraint.walkable =
            true;


        return AstarPath.active
            .GetNearest(
                position,
                constraint
            )
            .node;
    }


    // =========================================================
    // LOS
    // =========================================================

    private bool HasLineOfSight(
        Vector2 origin,
        Vector2 destination
    )
    {
        Vector2 offset =
            destination -
            origin;


        float distanceSquared =
            offset.sqrMagnitude;


        if (
            distanceSquared <
            0.000001f
        )
        {
            return true;
        }


        float distance =
            Mathf.Sqrt(
                distanceSquared
            );


        return !Physics2D.Raycast(
            origin,
            offset / distance,
            distance,
            obstacleMask
        );
    }


    private Vector2 GetTargetPosition()
    {
        return
            (Vector2)target.position +
            targetSightOffset;
    }


    // =========================================================
    // HOLDING
    // =========================================================

    private void BeginHolding(
        Vector2 position
    )
    {
        currentVantagePoint =
            position;

        hasVantagePoint =
            true;

        holdingPosition =
            true;

        path =
            null;

        currentWaypoint =
            0;


        nextRepositionTime =
            Time.time +
            Random.Range(
                minimumHoldTime,
                Mathf.Max(
                    minimumHoldTime,
                    maximumHoldTime
                )
            );


        StopMoving();
    }


    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        if (target != null)
        {
            float directionToTarget =
                target.position.x - transform.position.x;

            if (Mathf.Abs(directionToTarget) > 0.05f)
                spriteRenderer.flipX = directionToTarget < 0f;
        }

        enemy.anim.SetBool("isMoving", false);
    }
}