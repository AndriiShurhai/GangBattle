using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Context-aware scoring for Burn Attack. The ability deals immediate damage AND
/// applies a Burned DoT — so its value is highest when the DoT will actually tick
/// multiple times on a surviving target.
///
/// Scoring logic:
///
///   Base:               flat reward for having any valid target
///   High HP target:     burn DoT pays off most against targets that will survive
///                       long enough to take all the ticks
///   Already burning:    heavy penalty — reapplying burn only refreshes duration,
///                       the immediate damage alone doesn't justify choosing this
///                       over a plain attack
///   Low target HP:      penalty — target likely dies before DoT does anything;
///                       a plain attack that secures the kill is better
///   AI outnumbered:     small bonus — sustained DoT pressure is more valuable
///                       when the AI team needs help thinning enemies over time
///
/// Score range: ~0 to ~260, competitive with AttackActionSO but only pulling ahead
/// when the burn DoT will meaningfully contribute.
/// </summary>
/// 

[CreateAssetMenu(fileName = "BurnAttackActionSO", menuName = "AI/Actions/Burn Attack Action")]
public class BurnAttackActionSO : AbilityActionBase
{
    public override AIActionCategory Category => AIActionCategory.Attack;
    protected override AbilityBaseSO GetAbility() => burnAttackAbility;

    [SerializeField] private AbilityBaseSO burnAttackAbility;

    [Header("Base Scoring")]
    [Tooltip("Starting score when a valid target exists. Slightly below AttackActionSO's 100 " +
             "baseline so Burn is a conditional upgrade, not always the default choice.")]
    public float baseScore = 75f;

    [Header("Situational Bonuses")]
    [Tooltip("HP% threshold above which the target is considered 'high HP' — likely to survive " +
             "long enough for the DoT to tick multiple times.")]
    [Range(0f, 1f)] public float highHpThreshold = 0.55f;

    [Tooltip("Bonus score when the target clears the highHpThreshold. This is the core value " +
             "of burn — scale it up if burn DoT is high in your design.")]
    public float highHpBonus = 80f;

    [Tooltip("Additional bonus per DoT tick the target is likely to survive. Calculated as " +
             "estimated surviving turns × this value. Rewards burning very tanky targets.")]
    public float perSurvivingTickBonus = 15f;

    [Tooltip("Bonus per unit the AI team is outnumbered by. Sustained DoT is more valuable " +
             "when there are many enemies to wear down over time.")]
    public float outnumberedBonusPerUnit = 20f;

    [Header("Penalties")]
    [Tooltip("Score multiplier when the target is already burning. Near zero — re-burning " +
             "only refreshes duration and wastes the action over a normal attack.")]
    [Range(0f, 1f)] public float alreadyBurningMultiplier = 0.15f;

    [Tooltip("HP% below which the target is unlikely to survive long enough for the DoT " +
             "to be worthwhile. A plain finisher is better.")]
    [Range(0f, 1f)] public float lowHpThreshold = 0.3f;

    [Tooltip("Score subtracted when the target is below lowHpThreshold.")]
    public float lowHpPenalty = 60f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        Unit bestTarget = FindBestTarget(aiUnit);
        if (bestTarget == null) return scoreData;

        float score = baseScore;

        float targetHpPercent = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;

        // PENALTY: Target is already burning — DoT adds nothing new
        if (bestTarget.HasStatus(EffectStatusType.Burned))
        {
            score *= alreadyBurningMultiplier;


            Unit alternativeTarget = FindBestNonBurningTarget(aiUnit);
            if (alternativeTarget != null)
            {
                // Re-score using the non-burning target
                bestTarget = alternativeTarget;
                targetHpPercent = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;
                score = baseScore; // reset
            }
            else if (score <= 0f)
            {
                return scoreData;
            }
        }

        // PENALTY: Target is low HP — likely to die before DoT ticks meaningfully
        if (targetHpPercent < lowHpThreshold)
            score -= lowHpPenalty;

        // BONUS: Target is high HP — will survive to eat all burn ticks
        if (targetHpPercent >= highHpThreshold)
        {
            score += highHpBonus;

            // Additional bonus based on how many ticks the target will likely survive
            // Rough estimate: each 10% HP above the low threshold = 1 surviving tick
            int estimatedSurvivingTicks = Mathf.FloorToInt((targetHpPercent - lowHpThreshold) / 0.1f);
            score += estimatedSurvivingTicks * perSurvivingTickBonus;
        }

        // BONUS: AI team is outnumbered — DoT helps chip down a crowd over time
        int aliveEnemies = TurnManager.Instance.GetAlivePlayerUnits().Count;
        int aliveAllies = TurnManager.Instance.GetAliveEnemyUnits().Count;
        int deficit = aliveEnemies - aliveAllies;
        if (deficit > 0)
            score += deficit * outnumberedBonusPerUnit;

        if (score <= 0f) return scoreData;

        scoreData.score = score;
        scoreData.target = bestTarget;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) { onComplete?.Invoke(); return; }

        Debug.Log($"{aiUnit.name} uses Burn Attack on {targetUnit.name}");
        aiUnit.UseAbility(burnAttackAbility, targetUnit.GridPosition);
        onComplete?.Invoke();
    }


    private Unit FindBestNonBurningTarget(Unit aiUnit)
    {
        List<Vector3Int> tilesInRange = burnAttackAbility.GetTilesInRange(aiUnit.GridPosition);

        return tilesInRange
            .Select(t => GridObjectRegistry.Instance.GetObjectAt(t) as Unit)
            .Where(u => u != null &&
                        u.UnitFaction == Faction.Player &&
                        !u.HasStatus(EffectStatusType.Burned) &&
                        burnAttackAbility.IsValidTarget(aiUnit.GridPosition, u.GridPosition, aiUnit))
            .OrderByDescending(u => u.CurrentHealth)
            .FirstOrDefault();
    }

    private Unit FindBestTarget(Unit aiUnit)
    {
        List<Vector3Int> tilesInRange = burnAttackAbility.GetTilesInRange(aiUnit.GridPosition);

        return tilesInRange
            .Select(t => GridObjectRegistry.Instance.GetObjectAt(t) as Unit)
            .Where(u => u != null &&
                        u.UnitFaction == Faction.Player &&
                        burnAttackAbility.IsValidTarget(aiUnit.GridPosition, u.GridPosition, aiUnit))
            .OrderByDescending(u => u.CurrentHealth)
            .FirstOrDefault();
    }
}