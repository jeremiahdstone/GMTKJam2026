using UnityEngine;
using DG.Tweening;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float projectileSpeed = 8f;


    [Header("Targeting")]
    [SerializeField] private float range = 8f;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack")]
    [SerializeField] private float cooldown = 1f;
    private float cooldownTimer;
    private bool canAttack = true;

    [Header("Turret Visual")]
    [SerializeField] private Transform turretTransform;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private bool flipY = true;
    [SerializeField] private bool faceTarget = true;

    private Transform target;


    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
    }


    private void Update()
    {

        if(!canAttack)
            return;

        if (target == null)
            return;

        cooldownTimer -= Time.deltaTime;

        Vector2 toTarget =
            target.position - turretTransform.position;

        if (toTarget.sqrMagnitude > range * range)
            return;

        if (faceTarget)
            FaceTarget();

        if (
            cooldownTimer <= 0f &&
            CheckLineOfSight()
        )
        {
            Shoot();
            cooldownTimer = cooldown;
        }
    }


    private bool CheckLineOfSight()
    {
        Vector2 direction =
            target.position - firePoint.position;

        float distance =
            direction.magnitude;

        return !Physics2D.Raycast(
            firePoint.position,
            direction / distance,
            distance,
            obstacleLayer
        );
    }


    private void Shoot()
    {
        Vector2 direction =
            (target.position - firePoint.position).normalized;

        Projectile newProjectile =
            PoolManager.instance.Spawn(
                projectile,
                firePoint.position,
                Quaternion.identity
            );

        newProjectile.InitializeFromFlatStats(direction, projectileSpeed, damage);
    }


    private void FaceTarget()
    {
        Vector2 direction =
            target.position - turretTransform.position;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        turretTransform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        if (
            turretSpriteRenderer != null &&
            flipY
        )
        {
            turretSpriteRenderer.flipY =
                direction.x < 0f;
        }
    }
    public override void Freeze(float cooldown, GameObject attacker = null)
    {
        base.Freeze(cooldown, attacker);

        if (turretSpriteRenderer != null)
        {
            turretSpriteRenderer.material.SetFloat("_EnableColorSwap", 1f);
        }

        canAttack = false;

    }

    public override void Unfreeze()
    {
        base.Unfreeze();

        if (turretSpriteRenderer != null)
        {
            turretSpriteRenderer.material.SetFloat("_EnableColorSwap", 0f);
        }

        canAttack = true;
    }
}
