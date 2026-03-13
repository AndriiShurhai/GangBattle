using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance {  get; private set; }    

    [Header("Visual Feedback")]
    [SerializeField] private GameObject movementHighlightPrefab;
    [SerializeField] private Color validMoveColor = Color.black;
    [SerializeField] private Color invalidMoveColor = Color.red;
    [SerializeField] private Transform highlightsContainer;

    private HighlightManager _highlightManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _highlightManager = new HighlightManager(movementHighlightPrefab, highlightsContainer);
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
            _highlightManager.CreateHighlight(validPosition, validMoveColor);
        }
    }

    public void ClearHighlights()
    {
        _highlightManager.ClearAllHighlights();
    }

    public void HideHighlights()
    {
        _highlightManager.SetHighlightsActive(false);
    }

    public void ShowHighlights()
    {
        _highlightManager.SetHighlightsActive(true);
    }
}
