using UnityEngine;

public class Placeable : MonoBehaviour
{
    [Header("Interaction")]
    public bool canPickUp;
    [Header("Grid Footprint")]
    [SerializeField] private Vector2Int size = Vector2Int.one;

    public Vector2Int Size => size;

    public Vector3Int GridPosition { get; private set; }

    public bool IsPlaced { get; private set; }

    public void SetPlaced(Vector3Int gridPosition)
    {
        GridPosition = gridPosition;
        IsPlaced = true;
    }

    public void SetPickedUp()
    {
        if(canPickUp) IsPlaced = false;
    }
}
