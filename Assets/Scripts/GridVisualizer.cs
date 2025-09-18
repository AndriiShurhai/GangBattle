using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance {  get; private set; }    

    [Header("Visual Feedback")]
    [SerializeField] private GameObject movementHighlightPrefab;
    [SerializeField] private Color validMoveColor = Color.green;
    [SerializeField] private Color invalidMoveColor = Color.red;

    private List<GameObject> highlightObjects = new List<GameObject>();

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

    public void ShowMovementRange(Vector3Int centerPosition, int movementRange, System.Func<Vector3Int, bool> IsValidPosition)
    {
        ClearHighlights();

        var validPositions = PathFinder.Instance.GetReachableTiles(centerPosition, movementRange, IsValidPosition);

        foreach (var validPosition in validPositions)
        {
            CreateHighlight(validPosition, validMoveColor);
        }
    }

    public void ClearHighlights()
    {
        foreach (GameObject highlight in highlightObjects)
        {
            if (highlight != null) Destroy(highlight);

        }
        highlightObjects.Clear();
    }

    public void CreateHighlight(Vector3Int gridPosition, Color color)
    {
        if (movementHighlightPrefab == null) return;

        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = Instantiate(movementHighlightPrefab, worldPosition, Quaternion.identity);

        var renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = color;

        highlightObjects.Add(highlight);
    }
}
