using System.Collections.Generic;
using UnityEngine;

public class GridObjectRegistry : MonoBehaviour
{
    public static GridObjectRegistry Instance { get; private set; }

    private Dictionary<Vector3Int, IGridObject> occupiedTiles = new Dictionary<Vector3Int, IGridObject>();


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
    public void RegisterUnit(IGridObject unit)
    {
        if (IsOccupied(unit.GridPosition))
        {
            Debug.LogWarning($"Trying to register unit at occupied position: {unit.GridPosition}");
            return;
        }

        occupiedTiles[unit.GridPosition] = unit;
    }

    public void UnregisterUnit(IGridObject unit, Vector3Int position)
    {
        if (occupiedTiles.ContainsKey(position) && occupiedTiles[position] == unit)
        {
            occupiedTiles.Remove(position);
        }
    }

    public void MoveUnit(IGridObject unit, Vector3Int fromPosition, Vector3Int toPosition)
    {
        UnregisterUnit(unit, fromPosition);
        unit.GridPosition = toPosition;
        RegisterUnit(unit);
        unit.OnGridPositionChanged(toPosition);
    }

    public IGridObject GetObjectAt(Vector3Int gridPosition)
    {
        if (!occupiedTiles.ContainsKey(gridPosition)) { return null; }

        return occupiedTiles[gridPosition];
    }

    public bool IsOccupied(Vector3Int gridPosition)
    {
        return occupiedTiles.ContainsKey(gridPosition);
    }

}
