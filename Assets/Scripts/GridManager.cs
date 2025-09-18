using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Setup")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap walkableTilemap;
    [SerializeField] private Tilemap blockedTilemap;


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

    private void Start()
    {
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
    public bool IsValidPosition(Vector3Int gridPosition)
    {
        return IsWalkable(gridPosition) && !GridObjectRegistry.Instance.IsOccupied(gridPosition);
    }

    #endregion

    #region Unit Registration
    #endregion

    #region Visual Feedback
    #endregion
}
