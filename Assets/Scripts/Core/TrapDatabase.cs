using System.Collections.Generic;
using UnityEngine;

public class TrapDatabase : MonoBehaviour
{
    public static TrapDatabase Instance { get; private set; }

    [SerializeField]
    private List<Trap> trapPrefabs = new();

    public IReadOnlyList<Trap> TrapPrefabs => trapPrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}