using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(IMoveable))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private int movementRange = 3;
    [SerializeField] private float moveSpeed = 2f;

    private IMoveable moveableObject;
    private bool isMoving;
    private Coroutine moveCoroutine;

    public bool IsMoving => isMoving;
    public int MovementRange => movementRange;

    private void Awake()
    {
        moveableObject = GetComponent<IMoveable>();
    }

    public void Initialize(CharacterClassSO characterClassSO)
    {
        movementRange = characterClassSO.movementRange; 
    }

    public bool CanMoveTo(Vector3Int gridPosition)
    {
        if (GridManager.Instance == null || PathFinder.Instance == null)
        {
            Debug.LogWarning("Required managers not available for movement validation");
            return false;
        }

        if (!GridManager.Instance.IsValidPosition(gridPosition))
        {
            return false;
        }

        return PathFinder.Instance.GetReachableTiles(moveableObject.GridPosition, movementRange, GridManager.Instance.IsValidPosition).Contains(gridPosition);
    }

    public void MoveTo(Vector3Int gridPosition, Action onComplete = null)
    {
        if (isMoving || !CanMoveTo(gridPosition))
        {
            onComplete?.Invoke();
            return;
        }

        List<Vector3Int> path = PathFinder.Instance.GetPath(moveableObject.GridPosition, gridPosition, GridManager.Instance.IsValidPosition);

        if (path.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        moveCoroutine = StartCoroutine(MoveAlongPath(path, onComplete));
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path, Action onComplete = null)
    {
        isMoving = true;

        Vector3Int oldPosition = moveableObject.GridPosition;
        GridObjectRegistry.Instance.MoveObject(moveableObject, oldPosition, path[path.Count - 1]);

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(path[i]);

            while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPosition;

            if (moveableObject is Unit unit)
            {
                Unit.InvokeUnitEnteredTile(unit, path[i]);
            }
        }

        isMoving = false;
        onComplete?.Invoke();
    }
}
