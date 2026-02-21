using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wraps HealAbilitySO (or any ally-targeting ability) and scores the action
/// by finding the most critically injured friendly unit in range.
///
/// Score: 0 if no ally is below the healThreshold, otherwise baseScore + up to 100 bonus
/// based on how injured they are. A near-dead ally scores ~190, making healing
/// competitive with attacking a low-HP enemy.
/// </summary>
/// 

[CreateAssetMenu(fileName = "HealAllyActionSO", menuName = "AI/Actions/Heal Ally Action")]
public class HealAllyActionSO : AIActionSO
{
    public override AIActionCategory Category => AIActionCategory.Support;

    [SerializeField] private AbilityBaseSO healAbility;

    [Tooltip("Only consider healing allies whose HP% is at or below this threshold.")]
    [Range(0f, 1f)] public float healThreshold = 0.7f;

    [Tooltip("Base score when a healable target is found. Scales up with injury severity.")]
    public float baseScore = 90f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        List<Vector3Int> tilesInRange = healAbility.GetTilesInRange(aiUnit.GridPosition);

        Unit bestTarget = null;
        float lowestHealthPercent = healThreshold;

        foreach (Vector3Int tile in tilesInRange)
        {
            if (!healAbility.IsValidTarget(aiUnit.GridPosition, tile, aiUnit)) continue;

            if (GridObjectRegistry.Instance.GetObjectAt(tile) is Unit unit &&
                unit.UnitFaction == aiUnit.UnitFaction)
            {
                float hp = (float)unit.CurrentHealth / unit.MaxHealth;
                if (hp < lowestHealthPercent)
                {
                    lowestHealthPercent = hp;
                    bestTarget = unit;
                }
            }
        }

        if (bestTarget == null) return scoreData;

        // More injured ally = higher urgency
        scoreData.score = baseScore + (1f - lowestHealthPercent) * 100f;
        scoreData.target = bestTarget;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) { onComplete?.Invoke(); return; }

        Debug.Log($"{aiUnit.name} heals {targetUnit.name}");
        aiUnit.UseAbility(healAbility, targetUnit.GridPosition);
        onComplete?.Invoke();
    }
}