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
    private List<GameObject> _targetHighlights = new();
    private Tween pulseTween;

    private Unit _pulsingUnit;
    private Vector3 _originalScale;
    private Dictionary<SpriteRenderer, Color> _originalColors = new();


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
        if (_targetHighlights?.Count != 0)
        {
            foreach (GameObject obj in _targetHighlights)
            {
                Destroy(obj);
            }

            _targetHighlights.Clear();
        }

        if (caster.ForcedUnitGridPosition != null)
        {
            IGridObject obj = GridObjectRegistry.Instance.GetObjectAt((Vector3Int)caster.ForcedUnitGridPosition);

            if (obj is Unit unit)
            {
                if (_pulsingUnit != unit)
                {
                    StopPulse();

                    _pulsingUnit = unit;
                    _originalScale = unit.transform.localScale;
                    _originalColors.Clear();

                    foreach (var sr in unit.GetComponentsInChildren<SpriteRenderer>())
                    {
                        _originalColors[sr] = sr.color;
                    }

                    pulseTween = unit.transform
                        .DOScale(_originalScale * 1.2f, 0.35f)
                        .SetEase(Ease.OutQuad)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetId(_pulsingUnit); // allows safe global kill

                    SpriteRenderer[] spriteRenderers = unit.GetComponentsInChildren<SpriteRenderer>();

                    foreach (var sr in spriteRenderers)
                    {
                        sr.DOColor(Color.red, 0.35f)
                          .SetLoops(-1, LoopType.Yoyo)
                          .SetEase(Ease.InOutSine)
                          .SetId(_pulsingUnit);
                    }
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
                CreateTargetHighlight(position, currentTargetColor);

            }
            else
            {
            }
        }
    }

    private void StopPulse()
    {
        if (_pulsingUnit != null)
        {
            DOTween.Kill(_pulsingUnit);
            _pulsingUnit.transform.localScale = _originalScale;
            foreach (var pair in _originalColors)
            {
                if (pair.Key != null)
                    pair.Key.color = pair.Value;
            }
            _pulsingUnit = null;

        }
    }


    public void HideAbilityRange()
    {
        _rangeHighlightManager.ClearAllHighlights();

        StopPulse();
        if (_targetHighlights?.Count != 0)
        {
            foreach (GameObject obj in _targetHighlights)
            {
                Destroy(obj);
            }

            _targetHighlights.Clear();
        }
    }

    private void CreateTargetHighlight(Vector3Int gridPosition, Color color)
    {
        if (targetHighlightPrefab == null) return;

        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = Instantiate(targetHighlightPrefab, worldPosition, Quaternion.identity, rangeHighlightsContainer);

        var renderer = highlight.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
        _targetHighlights.Add(highlight);
    }
}
