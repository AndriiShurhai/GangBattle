using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Move Action SO", menuName = "AI/Actions/Move Action")]
public class MoveActionSO : AIActionSO
{
    public override AIActionCategory Category => AIActionCategory.Move;
    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData() { score = 0f, target = null };

        Unit closestUnit = GetMoveTarget(aiUnit);

        if (closestUnit == null) return scoreData;

        scoreData.score = 25f; 
        scoreData.target = closestUnit;
        return scoreData;
    }
    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) { onComplete?.Invoke(); return; }

        Debug.Log($"{aiUnit.name} is moving towards {targetUnit.name}");

        List<Vector3Int> reachableTiles = PathFinder.Instance.GetReachableTiles(
            aiUnit.GridPosition,
            aiUnit.MovementRange,
            GridManager.Instance.IsValidPosition
        );

        if (reachableTiles.Count == 0) { onComplete?.Invoke(); return; }

        Vector3Int bestTile;
        
        bestTile = reachableTiles
            .OrderBy(t => Vector3.Distance(t, targetUnit.GridPosition))
            .First();
       
        if (!aiUnit.CanMoveTo(bestTile)) { onComplete?.Invoke(); return;  }

        aiUnit.MoveTo(bestTile, onComplete);
    }

    private Unit GetMoveTarget(Unit aiUnit)
    {
        if(aiUnit.HasStatus(EffectStatusType.Provoked) && aiUnit.ForcedUnitGridPosition != null)
        {
            if (GridObjectRegistry.Instance.GetObjectAt((Vector3Int)aiUnit.ForcedUnitGridPosition) is Unit forcedUnit)
                return forcedUnit;
        }

        List<Unit> playerUnits = TurnManager.Instance.GetAlivePlayerUnits();
        if (playerUnits.Count == 0) return null;

        return playerUnits
            .OrderBy(u => Vector3.Distance(aiUnit.transform.position, u.transform.position))
            .FirstOrDefault();
    }

    public override bool CanExecute(Unit aiUnit)
    {
        return aiUnit.MovedPerTurn < aiUnit.MoveAllowedPerTurn;
    }

}
