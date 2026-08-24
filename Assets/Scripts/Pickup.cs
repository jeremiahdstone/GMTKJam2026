using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Pickup : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pickupEffect;

    [Header("Value")]
    [SerializeField] private int bloodAmount = 1;

    [SerializeField] private float destroyAfterTime = 5f;

    [Header("Spawn Movement")]
    [SerializeField] private float minSpawnForce = 0.5f;
    [SerializeField] private float maxSpawnForce = 1.5f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnDestroy()
    {
        
    }

    void OnEnable()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomForce = Random.Range(
            minSpawnForce, maxSpawnForce);

        rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
        
        StartCoroutine(DestroyAfterDelay(destroyAfterTime));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            if(collision.gameObject.TryGetComponent<PlayerManager>(out PlayerManager player))
            {
                if (player.canCollectBlood == false)
                {
                    return;
                }
            }
            // add to blood
            GameSession.instance.AddBlood(bloodAmount);
            Instantiate(pickupEffect, transform.position, pickupEffect.transform.rotation);
            
            Destroy(this.gameObject);
            // pickup effect
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
    
}
