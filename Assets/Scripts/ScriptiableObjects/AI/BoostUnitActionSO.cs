using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Context-aware scoring for Boost Unit. The ability raises all stats, so its
/// value is entirely time-dependent — the unit needs turns remaining to actually
/// USE those stronger stats. Boosting at the end of a fight is wasteful.
///
/// Scoring logic:
///
///   Base:                flat reward for having any valid target in range
///   Target is self:      prefer boosting the highest-damage unit; self-boost
///                        is valid but less reliable without knowing damage output
///   Enemies remaining:   more enemies alive = more turns of boosted stats ahead.
///                        Core multiplier on the base score.
///   Target HP is low:    penalise — a boosted unit that dies next turn contributed nothing
///   Already boosted:     heavy penalty — stacking the same buff does nothing since
///                        BoostUnitAbilitySO just adds on top of existing stats and
///                        the Boosted status effect doesn't stack duration
///   Caster is low HP:    penalise — better to flee or attack for a kill than buff
///
/// Score range: ~0 (wrong situation) to ~250 (early fight, many enemies, healthy target)
/// This competes well with Attack (100-200) early in a fight but loses late.
/// </summary>
/// 

[CreateAssetMenu(fileName = "BoostUnitActionSO", menuName = "AI/Actions/Boost Unit Action")]
public class BoostUnitActionSO : AbilityActionBase
{
    public override AIActionCategory Category => AIActionCategory.Support;
    protected override AbilityBaseSO GetAbility() => boostAbility;

    [SerializeField] private AbilityBaseSO boostAbility;

    [Header("Base Scoring")]
    [Tooltip("Starting score before situational modifiers. Set below Attack's 100 baseline " +
             "so buffing is a conditional priority, not an automatic opener.")]
    public float baseScore = 60f;

    [Tooltip("Multiplied by the number of enemies alive. Core value driver — more enemies " +
             "means more turns where the boosted stats will matter.")]
    public float perEnemyMultiplier = 20f;

    [Header("Penalties")]
    [Tooltip("HP% below which the target is likely to die before benefitting from the boost.")]
    [Range(0f, 1f)] public float targetLowHpThreshold = 0.4f;
    public float targetLowHpPenalty = 70f;

    [Tooltip("Score multiplier when the target is already boosted. Near zero — re-buffing " +
             "wastes the action since the effect is already active.")]
    [Range(0f, 1f)] public float alreadyBoostedMultiplier = 0.05f;

    [Tooltip("HP% below which the caster is too endangered to spend an action buffing.")]
    [Range(0f, 1f)] public float casterLowHpThreshold = 0.3f;
    public float casterLowHpPenalty = 80f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        // Caster is too low HP — this action isn't worth the tempo loss
        float casterHpPercent = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth;
        if (casterHpPercent < casterLowHpThreshold) return scoreData;

        Unit bestTarget = FindBestTarget(aiUnit);
        if (bestTarget == null) return scoreData;

        int aliveEnemies = TurnManager.Instance.GetAlivePlayerUnits().Count;
        if (aliveEnemies == 0) return scoreData;

        float score = baseScore + aliveEnemies * perEnemyMultiplier;

        if (bestTarget.HasStatus(EffectStatusType.Boosted))
        {
            score *= alreadyBoostedMultiplier;

            Unit unboostedTarget = FindBestTarget(aiUnit, excludeBoosted: true);
            if (unboostedTarget != null)
            {
                bestTarget = unboostedTarget;
                score = baseScore + aliveEnemies * perEnemyMultiplier; // reset
            }
        }

        // PENALTY: Target is low HP — will likely die before using the boost
        float targetHpPercent = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;
        if (targetHpPercent < targetLowHpThreshold) score -= targetLowHpPenalty;

        if (score <= 0f) return scoreData;

        scoreData.score = score;
        scoreData.target = bestTarget;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) { onComplete?.Invoke(); return; }

        Debug.Log($"{aiUnit.name} boosts {targetUnit.name}");
        aiUnit.UseAbility(boostAbility, targetUnit.GridPosition);
        onComplete?.Invoke();
    }

    private Unit FindBestTarget(Unit aiUnit, bool excludeBoosted = false)
    {
        return boostAbility.GetTilesInRange(aiUnit.GridPosition)
            .Select(t => GridObjectRegistry.Instance.GetObjectAt(t) as Unit)
            .Where(u => u != null &&
                        u.UnitFaction == aiUnit.UnitFaction &&
                        (!excludeBoosted || !u.HasStatus(EffectStatusType.Boosted)) &&
                        boostAbility.IsValidTarget(aiUnit.GridPosition, u.GridPosition, aiUnit))
            .OrderByDescending(u => u.CurrentHealth)
            .FirstOrDefault();
    }
}