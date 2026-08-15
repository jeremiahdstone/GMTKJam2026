using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ProjectileTrap : Trap
{
    [Header("Attack")]
    // Range, Cooldown, BurstCount, and BurstDelay are now TrapStats

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("AOE Visual")]
    [SerializeField] private Transform aoeVisual;
    [SerializeField] private float aoeFadeDuration = 0.25f;
    [SerializeField] private float aoeTargetAlpha = 1f;

    private float cooldownTimer;
    private bool firing;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd += ShowAOE;
            GameEventManager.instance.OnWaveStart += HideAOE;
        }

        if(GameSession.instance.phase == Phase.build)
            ShowAOE();
        else
            HideAOE();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (GameEventManager.instance != null)
        {
            GameEventManager.instance.OnWaveEnd -= ShowAOE;
            GameEventManager.instance.OnWaveStart -= HideAOE;
        }
    }

    protected override void OnTrapPurchased(Trap trap)
    {
        base.OnTrapPurchased(trap);
        RefreshAOEVisual();
    }

    protected override void OnUpgradePurchased(Upgrade upgrade)
    {
        base.OnUpgradePurchased(upgrade);
        RefreshAOEVisual();
    }

    void RefreshAOEVisual()
    {
        if (aoeVisual != null)
        {
            float range = GetStat(TrapStat.Range);

            aoeVisual.localScale = new Vector3(
                range * 2,
                range * 2,
                1
            );
        }
    }

    void ShowAOE()
    {
        if (aoeVisual == null)
            return;
        RefreshAOEVisual();

        SpriteRenderer sr = aoeVisual.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            aoeVisual.gameObject.SetActive(true);
            return;
        }

        Material mat = sr.material;

        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", 0f);
        else
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        aoeVisual.gameObject.SetActive(true);

        if (mat.HasProperty("_Alpha"))
            mat.DOFloat(aoeTargetAlpha, "_Alpha", aoeFadeDuration).SetEase(Ease.Linear);
        else
            sr.DOFade(aoeTargetAlpha, aoeFadeDuration).SetEase(Ease.Linear);
    }

    void HideAOE()
    {
        if (aoeVisual == null)
            return;
        SpriteRenderer sr = aoeVisual.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            aoeVisual.gameObject.SetActive(false);
            return;
        }

        Material mat = sr.material;

        if (mat.HasProperty("_Alpha"))
        {
            mat.DOFloat(0f, "_Alpha", aoeFadeDuration).SetEase(Ease.Linear)
                .OnComplete(() => aoeVisual.gameObject.SetActive(false));
        }
        else
        {
            sr.DOFade(0f, aoeFadeDuration).SetEase(Ease.Linear)
                .OnComplete(() => aoeVisual.gameObject.SetActive(false));
        }
    }

    private void Update()
    {
        if (firing)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0)
            return;

        Enemy target = FindNearestEnemy();

        if (target != null)
        {
            StartCoroutine(FireBurst(target));
        }
    }

    private IEnumerator FireBurst(Enemy target)
    {
        firing = true;

        float cooldown = GetStat(TrapStat.Cooldown);
        int burstCount = Mathf.RoundToInt(GetStat(TrapStat.BurstCount));
        float burstDelay = GetStat(TrapStat.BurstDelay);

        cooldownTimer = cooldown;

        Vector2 direction = Vector2.right;

        if (target != null)
        {
            direction = (
                target.transform.position - firePoint.position
            ).normalized;
        }

        for (int i = 0; i < burstCount; i++)
        {
            // Update aim if the target still exists.
            if (target != null)
            {
                direction = (
                    target.transform.position - firePoint.position
                ).normalized;
            }

            Projectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            //pass in the current direction to fire, and the trap itself so the projectile can grab the stats
            projectile.Initialize(direction, this);

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

        firing = false;
    }

    private Enemy FindNearestEnemy()
    {
        float range = GetStat(TrapStat.Range);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            range
        );

        Enemy closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            float distance =
                (enemy.transform.position - transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        float range = GetStat(TrapStat.Range);

        Gizmos.DrawWireSphere(
            transform.position,
            range
        );
    }
#endif
}