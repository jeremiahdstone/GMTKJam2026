using UnityEngine;

public class MultiStageEnemy : Enemy
{
    [Header("Spawn On Death")]
    [SerializeField] private Enemy enemyToSpawn;
    [SerializeField] private bool transferTarget = true;

    private bool isDying;

    public override void OnEnable()
    {
        base.OnEnable();
        isDying = false;
    }

    public override void Die(
        float dropMultiplier,
        bool notifyDirector = true,
        GameObject attacker = null)
    {
        // Prevent multiple damage calls from spawning multiple enemies
        // before Destroy finishes at the end of the frame.
        if (isDying)
            return;

        isDying = true;

        Enemy spawnedEnemy = null;

        if (enemyToSpawn != null)
        {
            spawnedEnemy = Instantiate(
                enemyToSpawn,
                transform.position,
                transform.rotation
            );

            if (transferTarget)
                spawnedEnemy.target = target;

            if (LevelDirector.instance != null)
            {
                LevelDirector.instance.ReplaceEnemy(this, spawnedEnemy);
            }
        }

        // The mounted knight is removed, but the regular knight replaces it,
        // so the director's enemy count should remain unchanged.
        base.Die(
            dropMultiplier,
            notifyDirector: false,
            attacker: attacker
        );
    }
}
