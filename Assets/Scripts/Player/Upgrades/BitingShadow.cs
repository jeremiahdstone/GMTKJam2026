using UnityEngine;
using DG.Tweening;
using System.Collections;

public class BitingShadow : MonoBehaviour
{
    private PlayerStats playerStats;
    private Enemy target;

    private float biteDamage;
    private float biteSpeed;
    private float chargeAmount;
    private bool fullyCharged;

    private Transform playerTransform;

    private Animator anim;
    private SpriteRenderer sr;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        PlayerStats playerStats,
        Enemy target,
        float biteDamage,
        float biteSpeed,
        float chargeAmount,
        bool fullyCharged,
        Transform playerTransform)
    {
        this.playerStats = playerStats;
        this.target = target;
        this.biteDamage = biteDamage;
        this.biteSpeed = biteSpeed;
        this.chargeAmount = chargeAmount;
        this.fullyCharged = fullyCharged;
        this.playerTransform = playerTransform;

        StartCoroutine(DoBite());
    }

    private IEnumerator DoBite()
    {
        biteSpeed = Mathf.Max(0.01f, biteSpeed);

        float initialAnimSpeed = anim != null
            ? anim.speed
            : 1f;

        if (anim != null)
        {
            anim.speed = biteSpeed;
            anim.SetTrigger("Bite");
        }

        if (sr != null && target != null)
        {
            sr.flipX =
                target.transform.position.x < transform.position.x;
        }

        if (target == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 targetPosition = target.transform.position;

        transform
            .DOMove(
                targetPosition,
                0.5f / biteSpeed
            )
            .SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.4f / biteSpeed);

        // The enemy may have died or been destroyed while the shadow
        // was travelling toward it.
        if (target != null)
        {
            target.Damage(
                biteDamage,
                gameObject
            );

            GameEventManager.instance.Bite(
                target.transform,
                chargeAmount
            );
        }

        CameraShake.Instance?.Shake(0.5f);

        if (anim != null)
            anim.speed = initialAnimSpeed;

        Destroy(gameObject);
    }
}