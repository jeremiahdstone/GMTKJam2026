using UnityEngine;
using System.Collections;
public class Explosion : MonoBehaviour
{

    private ParticleSystem ps;
    private
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    public void OnEnable()
    {

    }

    public IEnumerator PlayAndRelease(float duration)
    {
        yield return new WaitForSeconds(duration);
        PoolManager.instance.Release(gameObject);
    }

    public void Explode(float radius, float damage, LayerMask damageableLayers)
    {
        StartCoroutine(PlayAndRelease(ps.main.duration));
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.radius = radius;

        ps.Play();

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            damageableLayers
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            enemy ??= hit.GetComponentInParent<Enemy>();

            if (enemy == null)
                continue;

            // Distance from the center of the explosion.
            float distance = Vector2.Distance(
                transform.position,
                hit.ClosestPoint(transform.position)
            );

            // 1 at center, 0 at the edge of the explosion.
            float damageMultiplier = 1f - Mathf.Clamp01(distance / radius);

            float finalDamage = damage * damageMultiplier;

            enemy.Damage(finalDamage, gameObject);
        }

        CameraShake.Instance?.Shake(
            0.6f + 0.05f * radius
        );

    }


    // Update is called once per frame
    void Update()
    {

    }
}
