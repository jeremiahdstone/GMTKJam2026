using UnityEngine;
using System.Collections;

public class PlayerDamageTrigger : MonoBehaviour
{
    [SerializeField] private int damageAmount = 3;
    [SerializeField] private float damageInterval = 2f;

    [Header("Radius")]
    [SerializeField] private float radius = 0.5f;
    private Coroutine damageCoroutine;

    void Awake()
    {
        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
    }
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (damageCoroutine == null)
            damageCoroutine = StartCoroutine(DamagePlayer());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamagePlayer()
    {
        while (true)
        {
            GameSession.instance.SubtractBlood(damageAmount);

            yield return new WaitForSeconds(damageInterval);
        }
    }
}