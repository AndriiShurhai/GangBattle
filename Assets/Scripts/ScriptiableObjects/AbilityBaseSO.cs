using UnityEngine;
using System.Collections.Generic;

public abstract class AbilityBaseSO : ScriptableObject
{
    [Header("Basic Information")]
    public string abilityName;
    public Sprite abilityIcon;
    [TextArea] public string abilityDescription;

    [Header("Range Settings")]
    public int range = 3;
    public RangeType rangeType = RangeType.Square;
    public TargetType targetType = TargetType.Enemy;

    [Header("Visual")]
    public Color rangePreviewColor = new Color(1f, 0f, 0f, 0.3f);
    public GameObject abilityEffectPrefab;

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
        Any
    }

    public virtual List<Vector3Int> GetTilesInRange(Vector3Int casterPosition)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();

        switch (rangeType)
        {
            case RangeType.Square:
                tiles = RangeFinder.GetSquareRange(casterPosition, range);
                break;

            case RangeType.Diamond:
                tiles = RangeFinder.GetDiamondRange(casterPosition, range);
                break;

            case RangeType.Cross:
                tiles = RangeFinder.GetCrossRange(casterPosition, range);
                break;

            case RangeType.Line:
                tiles = RangeFinder.GetLineRange(casterPosition, range);
                break;

            case RangeType.Circle:
                tiles = RangeFinder.GetCircleRange(casterPosition, range);
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

        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        switch (targetType)
        {
            case TargetType.Enemy:
                return targetObject is Unit unit && unit != caster;

            case TargetType.Ally:
                return targetObject is Unit;

            case TargetType.Self:
                return targetPosition == casterPosition;

            case TargetType.EmptyTile:
                return targetObject == null && GridManager.Instance.IsWalkable(targetPosition);

            case TargetType.Any:
                return GridManager.Instance.IsWalkable(targetPosition);
        }

        return false;
    }

    public abstract void Execute(Unit caster, Vector3Int targetPosition);

    public virtual bool CanUse(Unit caster)
    {
        // at the future might be cooldown checks or resources checks etc.
        return true;
    }
}
