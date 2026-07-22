using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
    public float batSpeed;
    public float walkSpeed;
    
    public float speed;
    public bool batForm;
    
    public Rigidbody2D rb;

    public static string previousLevel = "NONE";
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //INITIAL VALUES
        batSpeed = 15;
        walkSpeed = 5;
        speed = walkSpeed;
        batForm = false;

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
    
    //moves the player a small increment based on the inputted direction
    public void MovePlayer(Vector2 direction)
    {
        //moves based on the player speed, the time, and the movement direction
        //Time.fixedDeltaTime is to ensure altering frame rates do not affect speed
        //rb.MovePosition(rb.position+(speed * Time.fixedDeltaTime * direction));
        rb.linearVelocity = direction * speed;
    }

    public void ToggleBatForm()
    {
        if (batForm) //bat form, entering human form
        {
            speed = walkSpeed;
            batForm = false;
            //set sprite to bat
            //make pixel shadow poof
        } else  //human form, entering bat form
        {
            speed = batSpeed;
            batForm = true;
            //set sprite to human
            //make pixel shadow poof
            //start velocity in direction of mouse?
        }
    }

}
