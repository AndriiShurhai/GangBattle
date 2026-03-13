using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class AbilityTargetingVisualizer : MonoBehaviour
{
    public static AbilityTargetingVisualizer Instance { get; private set; }

    [Header("Visual Settings")]
    [SerializeField] private GameObject rangeHighlightPrefab;
    [SerializeField] private GameObject targetHighlightPrefab;
    [SerializeField] private Color validTargetColor;
    [SerializeField] private Color invalidTargetColor;
    [SerializeField] private Color currentTargetColor;
    [SerializeField] private Color blockedLOSColor;
    [SerializeField] private Transform rangeHighlightsContainer;

    private HighlightManager _rangeHighlightManager;
    private HighlightManager _targetHighlightManager;

    private Unit _pulsingUnit;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _rangeHighlightManager = new HighlightManager(rangeHighlightPrefab, rangeHighlightsContainer);
            _targetHighlightManager = new HighlightManager(targetHighlightPrefab, rangeHighlightsContainer);
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
            bool hasLineOfSight = GridUtility.HasLineOfSight(caster.GridPosition, gridPosition);

            bool isValidTarget = abilityBaseSO.IsValidTarget(caster.GridPosition, gridPosition, caster);
            
            Color highlightColor;

            if (isValidTarget)
            {
                highlightColor = validTargetColor;
            }
            else if (!hasLineOfSight)
            {
                highlightColor = blockedLOSColor;
            }
            else
            {
                highlightColor = invalidTargetColor;
            }

            if (!GridManager.Instance.HasWalkableTilemap(gridPosition)) continue;

            _rangeHighlightManager.CreateHighlight(gridPosition, highlightColor);
        }
    }

    public void UpdateTargetPreview(Vector3Int targetPosition, AbilityBaseSO abilityBaseSO, Unit caster)
    {
        _targetHighlightManager.ClearAllHighlights();

        if (caster.ForcedUnitGridPosition != null)
        {
            IGridObject obj = GridObjectRegistry.Instance.GetObjectAt((Vector3Int)caster.ForcedUnitGridPosition);

            if (obj is Unit unit)
            {
                if (_pulsingUnit != unit)
                {
                    StopPulse();

                    _pulsingUnit = unit;
                    _pulsingUnit.GetUnitVisualBridge().StartPulse();
                }
            }
        }
        else
        {
            StopPulse();
        }

        List<Vector3Int> positions = abilityBaseSO.GetAbilityRadiusTiles(targetPosition);

        foreach (var position in positions)
        {
            if (abilityBaseSO.IsValidTarget(caster.GridPosition, position, caster))
            {
                _targetHighlightManager.CreateHighlight(position, currentTargetColor, false);
            }
        }
    }

    private void StopPulse()
    {
        if (_pulsingUnit != null)
        {
            _pulsingUnit.GetUnitVisualBridge().StopPulse();
            _pulsingUnit = null;

        }
    }


    public void HideAbilityRange()
    {
        _rangeHighlightManager.ClearAllHighlights();
        StopPulse();
        _targetHighlightManager.ClearAllHighlights();
    }
}
