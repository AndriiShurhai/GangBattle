using UnityEngine;
using System.Collections.Generic;

public class AbilityTargetingVisualizer : MonoBehaviour
{
    public static AbilityTargetingVisualizer Instance { get; private set; }

    [Header("Visual Settings")]
    [SerializeField] private GameObject rangeHighlightPrefab;
    [SerializeField] private GameObject targetHighlightPrefab;
    [SerializeField] private Color validTargetColor;
    [SerializeField] private Color invalidTargetColor;
    [SerializeField] private Color currentTargetColor;
    [SerializeField] private Transform rangeHighlightsContainer;

    private HighlightManager _rangeHighlightManager;
    private GameObject _targetHighlight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _rangeHighlightManager = new HighlightManager(rangeHighlightPrefab, rangeHighlightsContainer);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowAbilityRange(AbilityBaseSO abilityBaseSO, Unit caster)
    {
        HideAbilityRange();

        List<Vector3Int> reachableTiles = abilityBaseSO.GetTilesInRange(caster.GridPosition);

        foreach (var gridPosition in reachableTiles)
        {
            bool isValidTarget = abilityBaseSO.IsValidTarget(caster.GridPosition, gridPosition, caster);
            Color highlightColor = isValidTarget ? validTargetColor : invalidTargetColor;

            _rangeHighlightManager.CreateHighlight(gridPosition, highlightColor);
        }
    }

    public void UpdateTargetPreview(Vector3Int targetPosition, AbilityBaseSO abilityBaseSO, Unit caster)
    {
        if (_targetHighlight != null)
        {
            Destroy(_targetHighlight);
        }

        if (abilityBaseSO.IsValidTarget(caster.GridPosition, targetPosition, caster))
        {
            CreateTargetHighlight(targetPosition, currentTargetColor);

        }
    }

    public void HideAbilityRange()
    {
        _rangeHighlightManager.ClearAllHighlights();

        if (_targetHighlight != null)
        {
            Destroy(_targetHighlight);
            _targetHighlight = null;
        }
    }

    private void CreateTargetHighlight(Vector3Int gridPosition, Color color)
    {
        if (targetHighlightPrefab == null) return;

        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = Instantiate(targetHighlightPrefab, worldPosition, Quaternion.identity, rangeHighlightsContainer);

        var renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
        _targetHighlight = highlight;
    }
}
