using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPlacementManager : MonoBehaviour
{
    public static GridPlacementManager instance;
    public bool canMovePlaceables = true;
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;

    [Header("Placement")]
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private int placementSearchRadius = 20;

    private readonly Dictionary<Vector3Int, Placeable> occupiedCells = new();

    private Placeable heldObject;
    private Placeable hoveredObject;
    private Vector3Int previousGridPosition;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        if (grid == null)
            grid = GetComponent<Grid>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        RefreshHoveredObject();
    }

    private void RefreshHoveredObject()
    {
        Placeable nextHoveredObject = null;

        if (EventSystem.current == null ||
            !EventSystem.current.IsPointerOverGameObject())
        {
            Collider2D hit = Physics2D.OverlapPoint(
                GetMouseWorldPosition(),
                placeableLayer
            );

            if (hit != null)
                nextHoveredObject = hit.GetComponentInParent<Placeable>();
        }

        if (hoveredObject == nextHoveredObject)
            return;

        hoveredObject?.SetHovered(false);
        hoveredObject = nextHoveredObject;
        hoveredObject?.SetHovered(true);
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

    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    public void TryPickUpObject()
    {
        if (GameSession.instance.phase == Phase.combat) return;
        if (!canMovePlaceables) return;

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

        if (!placeable.canPickUp)
            return;

        heldObject = placeable;
        previousGridPosition = heldObject.GridPosition;
        heldObject.SetBeingDragged(true);

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
        if (heldObject == null)
            return;

        Vector3Int droppedGridPosition = GetOriginCell(heldObject);
        Vector3Int gridPosition = FindNearestValidOrigin(
            heldObject,
            droppedGridPosition
        );

        if (gridPosition != previousGridPosition ||
            CanPlaceObject(heldObject, gridPosition))
        {
            PlaceObject(heldObject, gridPosition);
            heldObject.SetBeingDragged(false);
            heldObject = null;
            return;
        }

        PlaceObject(heldObject, previousGridPosition);
        heldObject.SetBeingDragged(false);
        heldObject = null;
    }

    private Vector3Int FindNearestValidOrigin(
        Placeable placeable,
        Vector3Int droppedOrigin
    )
    {
        if (CanPlaceObject(placeable, droppedOrigin))
            return droppedOrigin;

        Vector3Int nearestOrigin = previousGridPosition;
        float nearestDistance = float.MaxValue;
        int searchRadius = Mathf.Max(0, placementSearchRadius);

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                Vector3Int candidateOrigin = droppedOrigin +
                    new Vector3Int(x, y, 0);

                if (!CanPlaceObject(placeable, candidateOrigin))
                    continue;

                float distance = x * x + y * y;

                if (distance < nearestDistance)
                {
                    nearestOrigin = candidateOrigin;
                    nearestDistance = distance;
                }
            }
        }

        return nearestDistance < float.MaxValue
            ? nearestOrigin
            : previousGridPosition;
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

    public void RegisterPlaceable(Placeable placeable)
    {
        Vector3Int originCell = GetOriginCell(placeable);

        foreach (Vector3Int cell in GetOccupiedCells(
                     originCell,
                     placeable.Size))
        {
            occupiedCells[cell] = placeable;
        }

        placeable.SetPlaced(originCell);
    }

    private Vector3Int GetOriginCell(Placeable placeable)
    {
        Vector3 cellSize = grid.cellSize;

        Vector3 bottomLeft = placeable.transform.position - new Vector3(
            placeable.Size.x * cellSize.x * 0.5f,
            placeable.Size.y * cellSize.y * 0.5f,
            0f
        );

        return grid.WorldToCell(bottomLeft);
    }

    public void ClearOccupiedCells(Placeable placeable)
    {
        if (placeable == null)
            return;

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