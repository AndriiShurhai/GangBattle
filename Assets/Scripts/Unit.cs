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
        GridObjectRegistry.Instance.RegisterUnit(this);

        transform.position = GridManager.Instance.GridToWorld(gridPosition);

        Debug.Log(gridPosition);
    }

    private void OnDestroy()
    {
        if (GridObjectRegistry.Instance != null)
        {
            GridObjectRegistry.Instance.UnregisterUnit(this, gridPosition);
        }
    }

    public bool CanMoveTo(Vector3Int position)
    {
        if (GridManager.Instance == null || PathFinder.Instance == null)
        {
            Debug.LogWarning("Required managers not available for movement validation.");
            return false;
        }

        if (!GridManager.Instance.IsValidPosition(position))
        {
            return false;
        }

        List<Vector3Int> reachableTiles = PathFinder.Instance.GetReachableTiles(gridPosition, movementRange, GridManager.Instance.IsValidPosition);

        return reachableTiles.Contains(position);
    }

    public void MoveTo(Vector3Int targetPosition, Action onComplete = null)
    {
        if (isMoving || !CanMoveTo(targetPosition))
        {
            onComplete?.Invoke();
            return;
        }

        var path = PathFinder.Instance.GetPath(gridPosition, targetPosition, GridManager.Instance.IsValidPosition);
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

        Debug.Log("Moving unit");
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(path[i]);
            while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPosition;
        }

        GridObjectRegistry.Instance.MoveUnit(this, oldPosition, path[path.Count - 1]);
        isMoving = false;
        onComplete?.Invoke();

    }

    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        Debug.Log($"Unit moved to a new position: {newGridPosition}");
    }

    private void OnMouseDown()
    {
        Vector3Int randomPosition = new Vector3Int(UnityEngine.Random.Range(0, 7), UnityEngine.Random.Range(0, 7), 0);
        List<Vector3Int> path = PathFinder.Instance.GetPath(gridPosition, randomPosition, GridManager.Instance.IsValidPosition);
        Debug.Log($"Destiantion: {randomPosition}");
        foreach (var node in path)
        {
            Debug.Log($"Node: {node}");
            GridVisualizer.Instance.CreateHighlight(node, Color.green);
        }
        StartCoroutine(MoveAlongPath(path));
        if (!selected && !isMoving)
        {
            //GridVisualizer.Instance.ShowMovementRange(gridPosition, movementRange, GridManager.Instance.IsValidPosition);
            selected = true;
        }
        else if (selected)
        {
            GridVisualizer.Instance.ClearHighlights();
            selected = false;
        }
    }

}
