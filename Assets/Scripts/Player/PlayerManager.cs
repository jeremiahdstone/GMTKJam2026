using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer sr;
    public Animator anim;
    public GameObject SmokePuffEffect;

    [Header("Collision")]
    public Collider2D col;


    //General refs
    public PlayerAttacks playerAttacks { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public PlayerMovement playerMovement { get; private set; }

    void Awake()
    {
        playerAttacks = GetComponent<PlayerAttacks>();
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
    }

}
