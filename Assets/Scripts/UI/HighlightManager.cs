using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
public class HighlightManager
{
    [SerializeField] private float highlightAnimDuration = 0.3f;
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

        Vector3 highlightScale = highlight.transform.localScale;

        highlight.transform.localScale = new Vector3(0, 0, 0);
        highlight.transform.DOScale(highlightScale, highlightAnimDuration);

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
