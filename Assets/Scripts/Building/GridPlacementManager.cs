using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPlacementManager : MonoBehaviour
{
    public bool canMovePlaceables = true;
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;

    [Header("Placement")]
    [SerializeField] private LayerMask placeableLayer;

    private readonly Dictionary<Vector3Int, Placeable> occupiedCells = new();

    private Placeable heldObject;
    private Vector3Int previousGridPosition;

    private void Awake()
    {
        if (grid == null)
            grid = GetComponent<Grid>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         TryPickUpObject();
    //     }

    //     if (heldObject != null)
    //     {
    //         MoveHeldObject();

    //         if (Input.GetMouseButtonUp(0))
    //         {
    //             TryPlaceHeldObject();
    //         }
    //     }
    // }

    public bool IsHoldingObject() {
        return heldObject != null;
    }

    public void TryPickUpObject()
    {
        if(!canMovePlaceables) return;
        
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 mouseWorldPosition = GetMouseWorldPosition();

        Collider2D hit = Physics2D.OverlapPoint(
            mouseWorldPosition,
            placeableLayer
        );

        if (hit == null)
            return;

        Placeable placeable = hit.GetComponentInParent<Placeable>();

        if (placeable == null)
            return;

        if(!placeable.canPickUp)
            return;

        heldObject = placeable;
        previousGridPosition = heldObject.GridPosition;

        if (heldObject.IsPlaced)
        {
            ClearOccupiedCells(heldObject);
            heldObject.SetPickedUp();
        }
    }

    public void MoveHeldObject()
    {
        Vector2 mouseWorldPosition = GetMouseWorldPosition();

        Vector3Int gridPosition =
            grid.WorldToCell(mouseWorldPosition);

        heldObject.transform.position =
            GetObjectWorldPosition(gridPosition, heldObject.Size);
    }

    public void TryPlaceHeldObject()
    {
        Vector3Int gridPosition =
            grid.WorldToCell(heldObject.transform.position);

        if (CanPlaceObject(heldObject, gridPosition))
        {
            PlaceObject(heldObject, gridPosition);
            heldObject = null;
            return;
        }

        // Return the object to its previous valid position.
        PlaceObject(heldObject, previousGridPosition);
        heldObject = null;
    }

    private bool CanPlaceObject(
        Placeable placeable,
        Vector3Int originCell
    )
    {
        foreach (Vector3Int cell in GetOccupiedCells(
                     originCell,
                     placeable.Size))
        {
            if (occupiedCells.ContainsKey(cell))
                return false;
        }

        return true;
    }

    private void PlaceObject(
        Placeable placeable,
        Vector3Int originCell
    )
    {
        placeable.transform.position =
            GetObjectWorldPosition(originCell, placeable.Size);

        foreach (Vector3Int cell in GetOccupiedCells(
                     originCell,
                     placeable.Size))
        {
            occupiedCells[cell] = placeable;
        }

        placeable.SetPlaced(originCell);
    }

    private void ClearOccupiedCells(Placeable placeable)
    {
        List<Vector3Int> cellsToRemove = new();

        foreach (KeyValuePair<Vector3Int, Placeable> entry
                 in occupiedCells)
        {
            if (entry.Value == placeable)
            {
                cellsToRemove.Add(entry.Key);
            }
        }

        foreach (Vector3Int cell in cellsToRemove)
        {
            occupiedCells.Remove(cell);
        }
    }

    private IEnumerable<Vector3Int> GetOccupiedCells(
        Vector3Int originCell,
        Vector2Int size
    )
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                yield return originCell + new Vector3Int(x, y, 0);
            }
        }
    }

    private Vector3 GetObjectWorldPosition(
        Vector3Int originCell,
        Vector2Int size
    )
    {
        Vector3 bottomLeft = grid.CellToWorld(originCell);

        Vector3 cellSize = grid.cellSize;

        return bottomLeft + new Vector3(
            size.x * cellSize.x * 0.5f,
            size.y * cellSize.y * 0.5f,
            0f
        );
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        return new Vector2(mousePosition.x, mousePosition.y);
    }
}