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
    public void RegisterObject(IGridObject obj)
    {
        if (IsOccupied(obj.GridPosition))
        {
            Debug.LogWarning($"Trying to register unit at occupied position: {obj.GridPosition}");
            return;
        }

        occupiedTiles[obj.GridPosition] = obj;
    }

    public void UnregisterObject(IGridObject obj, Vector3Int position)
    {
        if (occupiedTiles.ContainsKey(position) && occupiedTiles[position] == obj)
        {
            occupiedTiles.Remove(position);
        }
    }

    public void MoveObject(IGridObject obj, Vector3Int fromPosition, Vector3Int toPosition)
    {
        UnregisterObject(obj, fromPosition);
        obj.GridPosition = toPosition;
        RegisterObject(obj);
        obj.OnGridPositionChanged(toPosition);
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
