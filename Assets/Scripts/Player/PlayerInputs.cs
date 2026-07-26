using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;

public class PlayerInputs : MonoBehaviour
{
    private PlayerMovement playerMovement;
    public PlayerAttacks playerAttacks;
    public GridPlacementManager gridPlacementManager;
    private PlayerManager manager;

    // public Animator animator;

    private Vector2 movementVector;

    private bool mouseDown;
    private Vector2 mousePosition;

    private Vector2 lastMovedDirection;


    [SerializeField] private Vector2 facingDirection;

    //TODO eventually this should be in like a round manager or smth,
    //this determines whether you attack or can move objects 

    void Start()
    {
        manager = GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
        //makes the player face toward the middle of the screen when they spawn in
        facingDirection = Vector3.zero - playerMovement.rb.transform.position;
        // animator.SetFloat("Horizontal", facingDirection.x);
        // animator.SetFloat("Vertical", facingDirection.y);
    }

    // Update is called once per frame
    void Update()
    {

        movementVector.x = Input.GetAxisRaw("Horizontal");
        movementVector.y = Input.GetAxisRaw("Vertical");
        if (movementVector != Vector2.zero)
        {
            manager.anim.SetBool("isMoving", true);
            facingDirection = movementVector;
            manager.sr.flipX = movementVector.x < 0;
            //new Vector2(Mathf.Round(movementVector.x), Mathf.Round(movementVector.y)
        }
        else
        {
            manager.anim.SetBool("isMoving", false);
        }
        movementVector.Normalize();

        // animator.SetBool("isMoving", movementVector != Vector2.zero);
        // animator.SetFloat("Horizontal", facingDirection.x);
        // animator.SetFloat("Vertical", facingDirection.y);


        Debug.DrawLine((Vector2)transform.position, (Vector2)transform.position + facingDirection / 1.5f);

        //OLD KICKING LOGIC 

        // if (Input.GetKeyDown("space")) {

        //     RaycastHit2D[] hits = Physics2D.LinecastAll((Vector2)transform.position, (Vector2)transform.position + facingDirection/1.5f, LayerMask.NameToLayer("pushables"));
        //     foreach (var hit in hits)
        //     {
        //         if (hit && hit.collider != null && hit.transform.tag == "pushables" && hit.collider.GetComponent<Pushable>() != null)
        //         {
        //             float xOffset = transform.position.x - hit.collider.transform.position.x;
        //             float yOffset = transform.position.y - hit.collider.transform.position.y;


        //             StartCoroutine("ToggleAnimBool", "isPushing");
        //             if (Mathf.Abs(xOffset) > Mathf.Abs(yOffset))
        //                 hit.collider.GetComponent<Pushable>().tryPush(new Vector2(-Mathf.Sign(xOffset), 0));
        //             else
        //                 hit.collider.GetComponent<Pushable>().tryPush(new Vector2(0, -Mathf.Sign(yOffset)));

        //         }
        //     }
        // }

        // freezeSelector.moveSelector(mousePosition);

        //OLD FREEZING LOGIC

        // Collider2D hitFreeze = Physics2D.OverlapPoint(mousePosition);
        // if (hitFreeze && hitFreeze.GetComponent<IFreezable>() != null)
        // {
        //     freezeSelector.showSelector();

        //     if (Input.GetButtonDown("Fire1"))
        //     {

        //         freezeSelector.ToggleFreeze(hitFreeze, mousePosition);
        //         StartCoroutine("ToggleAnimBool", "isCasting");
        //         // hitFreeze.GetComponent<IFreezable>().ToggleFreeze(mousePosition);    //moved to inside FreezeSelector.freeze()
        //     }
        // }
        // else
        // {
        //     freezeSelector.hideSelector();
        // }

        //BAT FORM TOGGLE
        if (Input.GetKeyDown("space"))
        {
            playerMovement.ToggleBatForm();
        }

        // converts mouse position from screen coordinates to game coordinates  
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //BITE ATTACK
        if (GameSession.instance != null && GameSession.instance.phase == Phase.combat && Input.GetButtonDown("Fire1"))
        {
            playerAttacks.BiteAttack(mousePosition);
        }
        else if (GameSession.instance.phase == Phase.build)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gridPlacementManager.TryPickUpObject();
            }

            if (gridPlacementManager.IsHoldingObject())
            {
                // Runs every frame, so the object follows the mouse.
                gridPlacementManager.MoveHeldObject();

                if (Input.GetMouseButtonUp(0))
                {
                    gridPlacementManager.TryPlaceHeldObject();
                }
            }
        }

        //resets the level
        if (Input.GetKeyDown("r"))
        {
            //loads the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //pauses the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameSession.instance.TogglePauseGame();
        }

    }

    private void FixedUpdate()
    {
        playerMovement.MovePlayer(movementVector);
    }

    // IEnumerator ToggleAnimBool(string animBool)
    // {
    //     animator.SetBool(animBool, true);
    //     yield return new WaitForSeconds(0.25f);
    //     animator.SetBool(animBool, false);
    // }
}
