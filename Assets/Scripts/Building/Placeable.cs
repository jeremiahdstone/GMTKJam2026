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

    public bool IsHovered { get; private set; }

    public bool IsBeingDragged { get; private set; }

    internal void SetHovered(bool hovered)
    {
        if (IsHovered == hovered)
            return;

        IsHovered = hovered;

        if (hovered)
            OnMouseHover();
        else
            OnMouseUnhover();
    }

    protected virtual void OnMouseHover() { }

    protected virtual void OnMouseUnhover() { }

    internal void SetBeingDragged(bool beingDragged)
    {
        if (IsBeingDragged == beingDragged)
            return;

        IsBeingDragged = beingDragged;

        if (beingDragged)
            OnDragStart();
        else
            OnDragEnd();
    }

    protected virtual void OnDragStart() { }

    protected virtual void OnDragEnd() { }

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
