using UnityEngine;

public interface IGridObject 
{ 
    Vector3Int GridPosition { get; set; }
    bool BlocksMovement { get; }
    void OnGridPositionChanged(Vector3Int newGridPosition);
}
