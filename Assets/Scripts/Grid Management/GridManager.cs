using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Setup")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap walkableTilemap;
    [SerializeField] private Tilemap blockedTilemap;

    public Tilemap WalkableTilemap => walkableTilemap;
    public Tilemap BlockedTilemap => blockedTilemap;

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
        return grid.GetCellCenterWorld(gridPosition);
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
    public bool IsValidPosition(Vector3Int gridPosition)
    {
        if (!IsWalkable(gridPosition))
        {
            return false;
        }

        IGridObject obj = GridObjectRegistry.Instance.GetObjectAt(gridPosition);
        return obj == null || !obj.BlocksMovement; 
    }

    public bool HasWalkableTilemap(Vector3Int gridPosition)
    {
        return walkableTilemap.HasTile(gridPosition);
    }

    public bool HasBlockedTilemap(Vector3Int gridPosition)
    {
        return blockedTilemap.HasTile(gridPosition);
    }

    #endregion
}
