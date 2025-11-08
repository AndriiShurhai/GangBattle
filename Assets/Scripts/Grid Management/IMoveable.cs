using System;
using UnityEngine;

public interface IMoveable : IGridObject
{
    int MovementRange { get; }
    bool IsMoving { get; }
    bool CanMoveTo(Vector3Int position);
    void MoveTo(Vector3Int position, Action onComplete = null);
}