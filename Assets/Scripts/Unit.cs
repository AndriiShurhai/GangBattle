using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IMoveable
{
    [Header("Unit stats")]
    [SerializeField] private int movementRange = 3;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3Int gridPosition;
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    private bool selected = false;

    public Vector3Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public int MovementRange => movementRange;
    public float MoveSpeed => moveSpeed;
    public bool IsMoving => isMoving;
    private void Start()
    {
        gridPosition = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.RegisterUnit(this);

        transform.position = GridManager.Instance.GridToWorld(gridPosition);

        Debug.Log(gridPosition);
    }

    private void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterUnit(this, gridPosition);
        }
    }

    public bool CanMoveTo(Vector3Int position)
    {
        float distance = Vector3Int.Distance(position, gridPosition);
        
        return distance <= movementRange && GridManager.Instance.IsValidPosition(position); 
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        if (isMoving || !CanMoveTo(targetPosition))
        {
            onComplete?.Invoke();
            return;
        }

        var path = GridManager.Instance.GetPath(gridPosition, targetPosition, movementRange);
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

        Vector3Int oldPosition = gridPosition;

        foreach(var gridPosition in path)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(gridPosition);
            while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPosition;
        }

        GridManager.Instance.MoveUnit(this, oldPosition, path[path.Count - 1]);
        isMoving = false;
        onComplete?.Invoke();

    }

    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        Debug.Log($"Unit moved to a new position: {newGridPosition}");
    }

    private void OnMouseDown()
    {

        if (!selected && !isMoving)
        {
            GridManager.Instance.ShowMovementRange(gridPosition, movementRange);
            selected = true;
        }
        else if (selected)
        {
            GridManager.Instance.ClearHighlights();
            selected = false;
        }
    }

}
