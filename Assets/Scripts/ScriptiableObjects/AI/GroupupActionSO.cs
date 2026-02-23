using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Makes the AI unit reposition toward its nearest ally when it finds itself isolated.
/// Useful for giving Support/Cowardly archetypes a fallback behavior that keeps
/// the enemy team clustered together.
///
/// Score: 0 when allies are already nearby, baseScore (~40) when isolated.
/// This is intentionally lower than Attack/Flee so grouped fighting is preferred,
/// but the unit still retreats to allies when it has nothing else to do.
///
/// </summary>
/// 

[CreateAssetMenu(fileName = "GroupUpActionSO", menuName = "AI/Actions/Group Up Action")]
public class GroupUpActionSO : AIActionSO
{
    public override AIActionCategory Category => AIActionCategory.GroupUp;

    [Tooltip("Distance in grid tiles below which the unit considers itself 'grouped'. " +
             "If any ally is within this range, the action scores 0.")]
    public float allyProximityThreshold = 3f;

    [Tooltip("Base score when isolated. Should be higher than Move (25) so this takes " +
             "priority over chasing enemies when the unit is alone.")]
    public float baseScore = 45f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        if (HasNearbyAlly(aiUnit)) return scoreData;

        Unit nearestAlly = FindNearestAlly(aiUnit);
        if (nearestAlly == null) return scoreData;

        scoreData.score = baseScore;
        scoreData.target = nearestAlly;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit allyUnit = target as Unit;
        if (allyUnit == null) { onComplete?.Invoke(); return; }

        List<Vector3Int> reachable = PathFinder.Instance.GetReachableTiles(
            aiUnit.GridPosition,
            aiUnit.MovementRange,
            GridManager.Instance.IsValidPosition
        );

        if (reachable.Count == 0) { onComplete?.Invoke(); return; }

        Vector3Int bestTile = reachable
            .Where(t => t == aiUnit.GridPosition || GridObjectRegistry.Instance.GetObjectAt(t) == null)
            .OrderBy(t => Vector3.Distance(t, allyUnit.transform.position))
            .FirstOrDefault();

        Debug.Log($"{aiUnit.name} is grouping up with {allyUnit.name}");
        aiUnit.MoveTo(bestTile, onComplete);
    }

    private bool HasNearbyAlly(Unit aiUnit)
    {
        foreach (Unit ally in TurnManager.Instance.GetAliveEnemyUnits())
        {
            if (ally == aiUnit) continue;
            if (Vector3.Distance(ally.GridPosition, aiUnit.GridPosition) <= allyProximityThreshold)
                return true;
        }
        return false;
    }

    private Unit FindNearestAlly(Unit aiUnit)
    {
        List<Unit> allies = TurnManager.Instance.GetAliveEnemyUnits()
            .Where(u => u != aiUnit)
            .ToList();

        if (allies.Count == 0) return null;

        return allies
            .OrderBy(u => Vector3.Distance(u.transform.position, aiUnit.transform.position))
            .FirstOrDefault();
    }

    public override bool CanExecute(Unit aiUnit)
    {
        return aiUnit.MovedPerTurn < aiUnit.MoveAllowedPerTurn;
    }
}