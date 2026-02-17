using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "AttackActionSO", menuName = "AI/Actions/Attack Action")]
public class AttackActionSO : AIActionSO
{
    [SerializeField] private AbilityBaseSO attackAbility;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData() { score = 0f, target = null }; 

        List<Unit> targetsInRange = FindPlayerUnitsInRange(aiUnit);
        if (targetsInRange.Count == 0) return scoreData;

        Unit bestTarget = null;
        int bestHealth = int.MaxValue;
        foreach (Unit targetUnit in targetsInRange)
        {
            if (targetUnit == null)
            {
                Debug.Log("YOU ARE FUCKING NOBODY");
                continue;
            }
            if (attackAbility.IsValidTarget(aiUnit.GridPosition, targetUnit.GridPosition, aiUnit) && targetUnit.CurrentHealth < bestHealth)
            {
                bestTarget = targetUnit;
                bestHealth = targetUnit.CurrentHealth;
                bestTarget = targetUnit;    
            }
        }

        if (bestTarget == null)
        {
            scoreData.score = 0f;
            scoreData.target = null;
            return scoreData;
        };
        if (attackAbility.IsValidTarget(aiUnit.GridPosition, bestTarget.GridPosition, aiUnit))
        {
            float healthPercentage = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;
            scoreData.score = 100f + (1f - healthPercentage) * 100f;
            scoreData.target = bestTarget;
        }

        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;

        if (targetUnit == null)
        {
            onComplete?.Invoke();
            return;
        }

        aiUnit.UseAbility(attackAbility, targetUnit.GridPosition);
        //attackAbility.Execute(aiUnit, targetUnit.GridPosition);

        onComplete?.Invoke();
        
    }

    private List<Unit> FindPlayerUnitsInRange(Unit aiUnit)
    {
        List<Unit> units = new List<Unit>();

        List<Vector3Int> tilesInRange = attackAbility.GetTilesInRange(aiUnit.GridPosition);

        foreach (var tile in tilesInRange)
        {
            if (GridObjectRegistry.Instance.GetObjectAt(tile) is Unit unit && unit.UnitFaction == Faction.Player)
            {
                units.Add(unit);
            }
        }

        return units;
    }
}
