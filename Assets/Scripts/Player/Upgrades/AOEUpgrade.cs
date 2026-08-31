using UnityEngine;

public class AOEUpgrade : Upgrade
{
    [Header("AOE")]
    [SerializeField] private Transform aoe;

    [SerializeField] private float baseRange = 3f;
    [SerializeField] private float rangePerLevel = 0.5f;

    protected virtual void Awake()
    {
        base.Awake();
        UpdateAOESize();
    }

    private void UpdateAOESize()
    {
        if (aoe == null)
        {
            Debug.LogWarning($"{name} has no AOE assigned.");
            return;
        }

        float range = baseRange + (rangePerLevel * (level - 1));

        aoe.localScale = new Vector3(
            range * 2f,
            range * 2f,
            1f
        );
    }

    public override void OnLevelUp()
    {
        UpdateAOESize();
    }
}