using UnityEngine;

public class Pickup : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pickupEffect;

    [Header("Value")]
    [SerializeField] private int bloodAmount = 1;

    [Header("Spawn Movement")]
    [SerializeField] private float minSpawnForce = 0.5f;
    [SerializeField] private float maxSpawnForce = 1.5f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void OnEnable()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomForce = Random.Range(
            minSpawnForce, maxSpawnForce);

        rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            // add to blood
            GameSession.instance.AddBlood(bloodAmount);
            Instantiate(pickupEffect, transform.position, pickupEffect.transform.rotation);
            
            Destroy(this.gameObject);
            // pickup effect
        }
    }
}
