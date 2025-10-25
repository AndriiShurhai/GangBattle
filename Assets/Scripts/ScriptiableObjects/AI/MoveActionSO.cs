using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Move Action SO", menuName = "AI/Actions/Move Action")]
public class MoveActionSO : AIActionSO
{
    [SerializeField] private AbilityBaseSO attackAbility;
    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData() { score = 0f, target = null };

        Unit closestUnit = FindClosestPlayerUnit(aiUnit);

        if (closestUnit == null) return scoreData;

        if (attackAbility != null && attackAbility.IsValidTarget(aiUnit.GridPosition, closestUnit.GridPosition, aiUnit))
        {
            return scoreData; 
        }

        scoreData.score = 25f; 
        scoreData.target = closestUnit;
        return scoreData;
    }
    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) return;

        Debug.Log($"{aiUnit.name} is moving towards {targetUnit.name}");

        List<Vector3Int> reachableTiles = PathFinder.Instance.GetReachableTiles(
            aiUnit.GridPosition,
            aiUnit.MovementRange,
            GridManager.Instance.IsValidPosition
        );

        if (reachableTiles.Count == 0) return; 

        Vector3Int bestTile = reachableTiles[0];
        float closestDist = Vector3.Distance(bestTile, targetUnit.GridPosition);

        foreach (Vector3Int tile in reachableTiles)
        {
            float dist = Vector3.Distance(tile, targetUnit.GridPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTile = tile;
            }
        }

        aiUnit.MoveTo(bestTile, onComplete);
    }

    private List<Unit> FindPlayerUnits(Unit aiUnit)
    {
        List<Unit> units = TurnManager.Instance.GetPlayerUnits();

        return units;
    }

    private Unit FindClosestPlayerUnit(Unit aiUnit)
    {
        List<Unit> playerUnits = TurnManager.Instance.GetPlayerUnits();
        if (playerUnits.Count == 0) return null;

        Unit closest = null;
        float minDistance = float.MaxValue;

        foreach (Unit playerUnit in playerUnits)
        {
            float distance = Vector3.Distance(aiUnit.transform.position, playerUnit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = playerUnit;
            }
        }
        return closest;
    }

}
