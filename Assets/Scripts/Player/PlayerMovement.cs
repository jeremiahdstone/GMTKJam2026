
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
    private bool isFrozen = false;
    private PlayerManager manager;
    //where all the values for player stats are stored
    public PlayerStats playerStats;

    public float speed;
    public bool batForm;
    public float batFormCooldownTimer;

    public Rigidbody2D rb;

    public static string previousLevel = "NONE";

    [Header("Human Form Shift Obstacle Check")]
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private float humanCheckRadius = 0.4f;
    [SerializeField] private float humanSearchRadius = 3f;
    [SerializeField] private float humanSearchStep = 0.2f;
    [SerializeField] private int checksPerRing = 16;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GetComponent<PlayerManager>();
        //INITIAL VALUES
        speed = playerStats.GetStat(PlayerStat.WalkSpeed);
        batForm = false;
        batFormCooldownTimer = 0f;
        gameObject.layer = LayerMask.NameToLayer("Player");

        //OLD LEVEL SPAWNING LOGIC 

        // //sets the player to spawn near the door the left at when they enter the main room
        // if (SceneManager.GetActiveScene().name == "Main" && previousLevel != "NONE")
        // {
        //     //we just came from a level, set the position accordingly
        //     GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Finish");

        //     //find which door you came from
        //     foreach (GameObject spawnPoint in spawnPoints)
        //     {
        //         if (spawnPoint.GetComponent<LevelTransitioner>().LevelName == previousLevel)
        //         {
        //             //moves the position 1 closer toward x 0, so its not overlapping the LevelTransitioner 
        //             Vector2 spawnPosition = new Vector2(spawnPoint.transform.position.x-(spawnPoint.transform.position.x/Mathf.Abs(spawnPoint.transform.position.x)), spawnPoint.transform.position.y);
        //             rb.transform.position = spawnPosition;
        //         }
        //     }
        // }

        // previousLevel = SceneManager.GetActiveScene().name;
    }

    public void ToggleFrozen(bool val)
    {
        isFrozen = val;
        if (isFrozen) rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (!batForm && batFormCooldownTimer > 0f)
        {
            batFormCooldownTimer = Mathf.Max(0f, batFormCooldownTimer - Time.deltaTime);
        }
    }

    //moves the player a small increment based on the inputted direction
    public void MovePlayer(Vector2 direction)
    {
        if (isFrozen)
        {

            return;
        }
        //moves based on the player speed, the time, and the movement direction
        //Time.fixedDeltaTime is to ensure altering frame rates do not affect speed
        //rb.MovePosition(rb.position+(speed * Time.fixedDeltaTime * direction));
        // rb.linearVelocity = direction * speed;

        // walking is velocity based
        if (!batForm)
        {
            rb.linearVelocity = direction * playerStats.GetStat(PlayerStat.WalkSpeed);
        }
        else // bat form is acceleration based
        {
            rb.AddForce(direction * playerStats.GetStat(PlayerStat.BatFormAcceleration), ForceMode2D.Impulse);
            // max speed for bat form
            if (rb.linearVelocity.magnitude > playerStats.GetStat(PlayerStat.BatFormMaxSpeed))
            {
                rb.linearVelocity = rb.linearVelocity.normalized * playerStats.GetStat(PlayerStat.BatFormMaxSpeed);
            }
            if (direction == Vector2.zero)  //decelerate when no input is given
            {
                rb.linearVelocity *= 1 / playerStats.GetStat(PlayerStat.BatFormAcceleration);
            }
        }
    }

    public void ToggleBatForm()
    {
        if (!batForm && batFormCooldownTimer > 0f)
        {
            return;
        }

        Instantiate(manager.SmokePuffEffect, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.1f);

        if (batForm) // bat form, entering human form
        {
            Vector2 walkablePosition = FindNearestWalkablePosition(transform.position);

            transform.position = walkablePosition;

            batForm = false;
            batFormCooldownTimer = playerStats.GetStat(PlayerStat.BatFormCooldown);

            GameEventManager.instance.BatModeExit();

            manager.anim.SetBool("isBat", false);

            gameObject.layer = LayerMask.NameToLayer("Player");
            manager.sr.sortingOrder = 0;
        }
        else // human form, entering bat form
        {
            batForm = true;
            batFormCooldownTimer = 0f;

            GameEventManager.instance.BatModeEnter();

            manager.anim.SetBool("isBat", true);

            gameObject.layer = LayerMask.NameToLayer("Bat");
            manager.sr.sortingOrder = 3;
        }
    }

    private Vector2 FindNearestWalkablePosition(Vector2 startingPosition)
    {
        // Current position is already valid
        if (!Physics2D.OverlapCircle(startingPosition, humanCheckRadius, obstacleLayer))
            return startingPosition;

        // Search outward in rings
        for (float radius = humanSearchStep;
             radius <= humanSearchRadius;
             radius += humanSearchStep)
        {
            for (int i = 0; i < checksPerRing; i++)
            {
                float angle = i * Mathf.PI * 2f / checksPerRing;

                Vector2 direction = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                );

                Vector2 checkPosition = startingPosition + direction * radius;

                if (!Physics2D.OverlapCircle(
                        checkPosition,
                        humanCheckRadius,
                        obstacleLayer))
                {
                    return checkPosition;
                }
            }
        }

        // No valid location found
        return startingPosition;
    }

}
