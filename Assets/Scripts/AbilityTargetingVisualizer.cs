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

    private List<GameObject> rangeHighlights = new List<GameObject>();
    private GameObject targetHighlight;
    private AbilityBaseSO currentAbility;
    private Unit currentCaster;

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

    public void ShowAbilityRange(AbilityBaseSO abilityBaseSO, Unit caster)
    {
        HideAbilityRange();

        currentAbility = abilityBaseSO;
        currentCaster = caster;

        List<Vector3Int> reachableTiles = abilityBaseSO.GetTilesInRange(caster.GridPosition);

        foreach (var gridPosition in reachableTiles)
        {
            bool isValidTarget = abilityBaseSO.IsValidTarget(caster.GridPosition, gridPosition, caster);
            Color highlightColor = isValidTarget ? validTargetColor : invalidTargetColor;

            CreateRangeHighlight(gridPosition, highlightColor);
        }
    }

    public void UpdateTargetPreview(Vector3Int targetPosition, AbilityBaseSO abilityBaseSO, Unit caster)
    {
        if (targetHighlight != null)
        {
            Destroy(targetHighlight);
        }

        if (abilityBaseSO.IsValidTarget(caster.GridPosition, targetPosition, caster))
        {
            CreateTargetHighlight(targetPosition, currentTargetColor);

        }
    }

    public void HideAbilityRange()
    {
        foreach (GameObject highlight in rangeHighlights)
        {
            if (highlight != null) Destroy(highlight);
        }

        rangeHighlights.Clear();

        if (targetHighlight != null)
        {
            Destroy(targetHighlight);
            targetHighlight = null;
        }

        currentAbility = null;
        currentCaster = null;
    }

    private void CreateRangeHighlight(Vector3Int gridPosition, Color color)
    {
        if (rangeHighlightPrefab == null) return;

        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = Instantiate(rangeHighlightPrefab, worldPosition, Quaternion.identity);
        
        var renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }

        rangeHighlights.Add(highlight);
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
        targetHighlight = highlight;
    }
}
