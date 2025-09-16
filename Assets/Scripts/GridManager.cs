using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Setupt")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap walkableTilemap;
    [SerializeField] private Tilemap blockedTilemap;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject movementHighlightPrefab;
    [SerializeField] private Color validMoveColor = Color.green;
    [SerializeField] private Color invalidMoveColor = Color.red;

    private Dictionary<Vector3Int, IGridObject> occupiedTiles = new Dictionary<Vector3Int, IGridObject>();
    private List<GameObject> highlightObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Coordinate Conversion
    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        return grid.CellToWorld(gridPosition);
    }

    public Vector3Int WorldToGrid(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }

    #endregion

    #region Tile Validation

    public bool IsWalkable(Vector3Int gridPosition)
    {
        if (walkableTilemap.GetTile(gridPosition) == null) return false;
        if (blockedTilemap.GetTile(gridPosition) != null) return false;
        return true;
    }

    public bool IsOccupied(Vector3Int gridPosition)
    {
        return occupiedTiles.ContainsKey(gridPosition);
    }

    public bool IsValidPosition(Vector3Int gridPosition)
    {
        return IsWalkable(gridPosition) && !IsOccupied(gridPosition);
    }

    #endregion

    #region Unit Registration

    public void RegisterUnit(IGridObject unit)
    {
        if (IsOccupied(unit.GridPosition))
        {
            Debug.LogWarning($"Trying to register unit at occupied position: {unit.GridPosition}");
            return;
        }

        occupiedTiles[unit.GridPosition] = unit;
    }

    public void UnregisterUnit(IGridObject unit)
    {
        if (occupiedTiles.ContainsKey(unit.GridPosition) && occupiedTiles[unit.GridPosition] == unit)
        {
            occupiedTiles.Remove(unit.GridPosition);
        }
    }

    public void MoveUnit(IGridObject unit, Vector3Int fromPosition, Vector3Int toPosition)
    {
        UnregisterUnit(unit);
        unit.GridPosition = toPosition;
        RegisterUnit(unit);
        unit.OnGridPositionChanged(toPosition);
    }

    #endregion

    #region Pathfinding & Movement

    public List<Vector3Int> GetValidMovementPositions(Vector3Int startPosition, int movementRange)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();

        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= movementRange)
                {
                    Vector3Int checkPosition = startPosition + new Vector3Int(x, y, 0); 
                    if (IsValidPosition(checkPosition) && checkPosition != startPosition)
                    {
                        validPositions.Add(checkPosition);
                    }
                }
            }
        }

        return validPositions;
    }

    public List<Vector3Int> GetPath(Vector3Int start, Vector3Int target, int maxRange)
    {
        int distance = Mathf.Abs(target.x - start.x) + Mathf.Abs(target.y - start.y);

        if (distance > maxRange || !IsValidPosition(target))
        {
            return new List<Vector3Int>();
        }

        List<Vector3Int> openSet = new List<Vector3Int>();
        List<Vector3Int> closedSet = new List<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        Dictionary<Vector3Int, int> gCost = new Dictionary<Vector3Int, int>() { [start] = 0 };
        Dictionary<Vector3Int, int> fCost = new Dictionary<Vector3Int, int> { [start] = distance };

        while (openSet.Count > 0)
        {
            Vector3Int current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (fCost.GetValueOrDefault(openSet[i], int.MaxValue) < fCost.GetValueOrDefault(current, int.MaxValue))
                {
                    current = openSet[i];
                }
            }

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector3Int neighbour in GetNeighbours(current))
            {
                if (closedSet.Contains(neighbour) || !IsValidPosition(neighbour))
                {
                    continue;
                }

                int fromStartToNeighbour = gCost[current] + 1;

                if (fromStartToNeighbour > maxRange)
                {
                    continue;
                }

                if (!openSet.Contains(neighbour) || fromStartToNeighbour < gCost.GetValueOrDefault(neighbour, int.MaxValue))
                {
                    cameFrom[neighbour] = current;
                    gCost[neighbour] = fromStartToNeighbour;
                    fCost[neighbour] = gCost[neighbour] + Mathf.Abs(target.x - neighbour.x) + Mathf.Abs(target.y - neighbour.y);
                    
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return new List<Vector3Int>();

    }

    private IEnumerable<Vector3Int> GetNeighbours(Vector3Int node)
    {
        yield return node + Vector3Int.right;
        yield return node + Vector3Int.left;
        yield return node + Vector3Int.up;
        yield return node + Vector3Int.down;

        yield return node + new Vector3Int(1, 1, 0);
        yield return node + new Vector3Int(-1, -1, 0);
        yield return node + new Vector3Int(-1, 1, 0);
        yield return node + new Vector3Int(1, -1, 0);
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> totalPath = new List<Vector3Int>() { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    #endregion

    #region Visual Feedback

    public void ShowMovementRange(Vector3Int centerPosition, int movementRange)
    {
        ClearHighlights();

        var validPositions = GetValidMovementPositions(centerPosition, movementRange);

        foreach (var validPosition in validPositions)
        {
            CreateHighlight(validPosition, validMoveColor);
        }
    }

    public void ClearHighlights()
    {
        foreach (var highlight in highlightObjects)
        {
            if (highlight != null) Destroy(highlight);

            highlightObjects.Clear();   
        }
    }

    public void CreateHighlight(Vector3Int gridPosition, Color color)
    {
        if (movementHighlightPrefab == null) return;

        Vector3 worldPosition = GridToWorld(gridPosition);
        GameObject highlight = Instantiate(movementHighlightPrefab, worldPosition, Quaternion.identity);
        
        var renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = color;

        highlightObjects.Add(highlight);
    }

    #endregion
}
