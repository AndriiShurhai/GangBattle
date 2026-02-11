using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HighlightManager
{
    private readonly GameObject _highlightPrefab;
    private readonly Transform _container;
    private readonly List<GameObject> _activeHighlights = new List<GameObject>();
    public HighlightManager(GameObject highlightPrefab, Transform container)
    {
        _highlightPrefab = highlightPrefab;
        _container = container;
    }

    public void CreateHighlight(Vector3Int gridPosition, Color color)
    {
        if (_highlightPrefab == null) return;

        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = Object.Instantiate(_highlightPrefab, worldPosition, Quaternion.identity, _container);

        var renderer = highlight.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
        _activeHighlights.Add(highlight);
    }

    public void ClearAllHighlights()
    {
        foreach (GameObject highlight in  _activeHighlights)
        {
            if (highlight != null) Object.Destroy(highlight);
        }

        _activeHighlights.Clear();
    }

    public void SetHighlightsActive(bool isActive)
    {
        foreach (GameObject highlight in _activeHighlights)
        {
            if (highlight != null) highlight.SetActive(isActive);   
        }
    }
}
