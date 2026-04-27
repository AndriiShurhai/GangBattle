using UnityEngine;
using System.Collections.Generic;
using System;


public enum StatType
{
    Strength,
    Intelligence,
    Agility,
    None
}
public abstract class AbilityBaseSO : ScriptableObject
{
    [Header("Basic Information")]
    [UnityEngine.Serialization.FormerlySerializedAs("abilityName")]
    [SerializeField] private string _abilityName;
    public string AbilityName => _abilityName;

    [UnityEngine.Serialization.FormerlySerializedAs("abilityIcon")]
    [SerializeField] private Sprite _abilityIcon;
    public Sprite AbilityIcon => _abilityIcon;

    [TextArea]
    [UnityEngine.Serialization.FormerlySerializedAs("abilityDescription")]
    [SerializeField] private string _abilityDescription;
    public string AbilityDescription => _abilityDescription;

    [UnityEngine.Serialization.FormerlySerializedAs("howMuchCanBeUsed")]
    [SerializeField] private int _maxUses = 1;
    public int MaxUses => _maxUses;

    [Header("Range Settings")]
    [UnityEngine.Serialization.FormerlySerializedAs("range")]
    [SerializeField] private int _range = 3;
    [SerializeField] private bool _hasLineOfSightRequirement = false;    
    public int Range => _range;

    [UnityEngine.Serialization.FormerlySerializedAs("rangeType")]
    [SerializeField] private RangeType _typeOfRange = RangeType.Square;
    public RangeType TypeOfRange => _typeOfRange;

    [UnityEngine.Serialization.FormerlySerializedAs("targetType")]
    [SerializeField] private TargetType _typeOfTarget = TargetType.Enemy;
    public TargetType TypeOfTarget => _typeOfTarget;

    [Header("Visual")]
    [UnityEngine.Serialization.FormerlySerializedAs("rangePreviewColor")]
    [SerializeField] private Color _rangePreviewColor = new Color(1f, 0f, 0f, 0.3f);
    public Color RangePreviewColor => _rangePreviewColor;

    [SerializeField] private string _sfxOnCast;
    [SerializeField] private string _sfxOnUse;
    public string SfxOnUse => _sfxOnUse;
    public string SfxOnCast => _sfxOnCast;

    [UnityEngine.Serialization.FormerlySerializedAs("abilityEffectPrefab")]
    [SerializeField] private GameObject _abilityEffectPrefab;
    public GameObject AbilityEffectPrefab => _abilityEffectPrefab;

    [Header("Scaling")]
    [UnityEngine.Serialization.FormerlySerializedAs("coefficient")]
    [SerializeField] private float _coefficient = 1f;
    public float Coefficient => _coefficient;

    [UnityEngine.Serialization.FormerlySerializedAs("scalingType")]
    [SerializeField] private StatType _typeOfScaling;
    public StatType TypeOfScaling => _typeOfScaling;
    public enum RangeType
    {
        Square, // all tiles within range
        Diamond, // manhattan distance
        Cross, // plus shape
        Line, // straight line in cardinal direction
        Circle // Circular AOE
    }

    public enum TargetType
    {
        Enemy, 
        Ally,
        Self,
        EmptyTile,
        Any,
    }

    public int GetPower(Unit unit)
    {
        switch (TypeOfScaling)
        {
            case StatType.Strength:
                return Mathf.RoundToInt(unit.Strength * Coefficient);

            case StatType.Intelligence:
                return Mathf.RoundToInt(unit.Intelligence * Coefficient);

            case StatType.Agility:
                return Mathf.RoundToInt(unit.Agility * Coefficient);
            default:
                return 0;
        }
    }
    public virtual List<Vector3Int> GetTilesInRange(Vector3Int casterPosition)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();

        switch (TypeOfRange)
        {
            case RangeType.Square:
                tiles = RangeFinder.GetSquareRange(casterPosition, Range);
                break;

            case RangeType.Diamond:
                tiles = RangeFinder.GetDiamondRange(casterPosition, Range);
                break;

            case RangeType.Cross:
                tiles = RangeFinder.GetCrossRange(casterPosition, Range);
                break;

            case RangeType.Line:
                tiles = RangeFinder.GetLineRange(casterPosition, Range);
                break;

            case RangeType.Circle:
                tiles = RangeFinder.GetCircleRange(casterPosition, Range);
                break;
        }

        return tiles;
    }

    public virtual bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!GetTilesInRange(casterPosition).Contains(targetPosition))
        {
            return false;
        }
       
        if (_hasLineOfSightRequirement && !GridUtility.HasLineOfSight(casterPosition, targetPosition))
        {
            return false;
        }

        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        switch (TypeOfTarget)
        {
            case TargetType.Enemy:
                if (caster.ForcedUnitGridPosition != null && targetPosition != caster.ForcedUnitGridPosition) return false;
                return targetObject is Unit enemy && enemy.UnitFaction != caster.UnitFaction;

            case TargetType.Ally:
                return targetObject is Unit ally && ally.UnitFaction == caster.UnitFaction;

            case TargetType.Self:
                return targetPosition == casterPosition;

            case TargetType.EmptyTile:
                return targetObject == null && GridManager.Instance.IsWalkable(targetPosition);

            case TargetType.Any:
                return GridManager.Instance.IsWalkable(targetPosition);
        }

        return false;
    }

    public abstract void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null);

    public virtual bool CanUse(Unit caster)
    {
        // at the future might be cooldown checks or resources checks etc.
        return true;
    }

    public virtual List<Vector3Int> GetAbilityRadiusTiles(Vector3Int targetPosition)
    {
        return new List<Vector3Int>() { targetPosition };
    }

}
