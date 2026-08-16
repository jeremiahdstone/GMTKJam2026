using UnityEngine;
using System.Collections;

public class PlayerDamageAOE : AOEBehavior
{
    [SerializeField] private int damageAmount = 3;
    [SerializeField] private float damageInterval = 2f;

    [Header("Radius")]
    private Coroutine damageCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (damageCoroutine == null)
            damageCoroutine = StartCoroutine(DamagePlayer(GameSession.instance.Player));
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

    private IEnumerator DamagePlayer(PlayerManager player)
    {
        while (true)
        {
            player.Damage(damageAmount);

            yield return new WaitForSeconds(damageInterval);
        }
    }

    
}