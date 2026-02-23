using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Causes the AI unit to retreat to the safest reachable tile when its HP drops
/// below a configurable threshold. Score scales with how injured the unit is,
/// so the more desperate the situation, the harder it fights to flee.
/// 
/// Score range: 0 (above threshold) to ~fleeBaseScore (near death)
/// Typical base score of 150 means fleeing beats Move (25) but loses to Attack (100-200)
/// unless the unit is critically low on HP — raise baseScore to override attacks.
/// </summary>

[CreateAssetMenu(fileName = "FleeActionSO", menuName = "AI/Actions/Flee Action")]
public class FleeActionSO : AIActionSO
{
    public override AIActionCategory Category => AIActionCategory.Flee;

    [Tooltip("HP percentage below which this action becomes valid (0 = never flee, 1 = always flee).")]
    [Range(0f, 1f)] public float fleeHealthTreshold = 0.35f;

    [Tooltip("Base score when at 0 HP. Actual score = baseScore * (1 - healthPercent). " +
         "Set above 200 to override even high-value attacks when critically injured.")]
    public float fleeBaseScore = 150f;
    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData() { score = 0f, target = null };


        float healthPercentage = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth;

        if (healthPercentage >= fleeHealthTreshold) return scoreData;

        Vector3Int safeTile = FindSafestReachableTile(aiUnit);

        if (safeTile == aiUnit.GridPosition) return scoreData;

        scoreData.score = fleeBaseScore * (1f - healthPercentage);
        scoreData.target = safeTile;

        return scoreData;
    }
    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Debug.Log($"{aiUnit.name} is fleeing!");
        
        if (target is Vector3Int safeTile)
        {
            Debug.Log($"{aiUnit.name} is fleeing to {safeTile}!");
            aiUnit.MoveTo(safeTile, onComplete);
        }
        else
        {
            Debug.LogError("Invalid target for FleeActionSO. Expected Vector3Int.");
            onComplete?.Invoke();
        }
    }

    private Vector3Int FindSafestReachableTile(Unit aiUnit)
    {
        List<Unit> playerUnits = TurnManager.Instance.GetAlivePlayerUnits();
        if (playerUnits.Count == 0) return aiUnit.GridPosition;
        Unit closestPlayerUnit = playerUnits
            .OrderBy(u => Vector3.Distance(u.GridPosition, aiUnit.GridPosition))
            .First();
        List<Vector3Int> reachableTiles = PathFinder.Instance.GetReachableTiles(
            aiUnit.GridPosition,
            aiUnit.MovementRange,
            GridManager.Instance.IsValidPosition
        );
        if (reachableTiles.Count == 0) return aiUnit.GridPosition;
        Vector3Int bestTile = reachableTiles
            .OrderByDescending(t => Vector3.Distance(t, closestPlayerUnit.GridPosition))
            .First();
        return bestTile;
    }

    public override bool CanExecute(Unit aiUnit)
    {
        return aiUnit.MovedPerTurn < aiUnit.MoveAllowedPerTurn;
    }
}
