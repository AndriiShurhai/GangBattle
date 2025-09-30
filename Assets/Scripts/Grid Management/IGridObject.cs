using UnityEngine;

public interface IGridObject 
{ 
    Vector3Int GridPosition { get; set; }
    void OnGridPositionChanged(Vector3Int newGridPosition);
}
