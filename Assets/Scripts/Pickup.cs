using Unity.VisualScripting;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Spawn Movement")]
    [SerializeField] private float minSpawnForce = 0.5f;
    [SerializeField] private float maxSpawnForce = 1.5f;
    void OnEnable()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomForce = Random.Range(
            minSpawnForce, maxSpawnForce);

        rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // add to time
            Destroy(this.gameObject);
            // pickup effect
        }
    }
}
