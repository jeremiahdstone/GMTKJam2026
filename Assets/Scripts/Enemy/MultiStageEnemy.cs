using UnityEngine;

public class MultiStageEnemy : Enemy
{
    [Header("Spawn On Death")]
    [SerializeField] private Enemy enemyToSpawn;
    [SerializeField] private bool transferTarget = true;
    bool shouldSpawnOnDeath = true;

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
        if(attacker != null)
        {
            if(attacker.CompareTag("Objective"))
            {
                shouldSpawnOnDeath = false;
            }
        }

        // Prevent multiple damage calls from spawning multiple enemies
        // before Destroy finishes at the end of the frame.
        if (isDying)
            return;

        isDying = true;

        Enemy spawnedEnemy = null;

        if (enemyToSpawn != null && shouldSpawnOnDeath)
        {
            spawnedEnemy = PoolManager.instance.Spawn(
                enemyToSpawn,
                transform.position,
                transform.rotation
            );

            if (transferTarget)
                spawnedEnemy.movementModule.SetTarget(movementModule.GetTarget());

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
