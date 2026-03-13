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
    [UnityEngine.Serialization.FormerlySerializedAs("baseScore")]
    [SerializeField] private float _baseScore = 60f;
    public float BaseScore => _baseScore;

    [Tooltip("Multiplied by the number of enemies alive. Core value driver — more enemies " +
             "means more turns where the boosted stats will matter.")]
    [UnityEngine.Serialization.FormerlySerializedAs("perEnemyMultiplier")]
    [SerializeField] private float _perEnemyMultiplier = 20f;
    public float PerEnemyMultiplier => _perEnemyMultiplier;

    [Header("Penalties")]
    [Tooltip("HP% below which the target is likely to die before benefitting from the boost.")]
    [UnityEngine.Serialization.FormerlySerializedAs("targetLowHpThreshold")]
    [SerializeField] [Range(0f, 1f)] private float _targetLowHpThreshold = 0.4f;
    public float TargetLowHpThreshold => _targetLowHpThreshold;

    [UnityEngine.Serialization.FormerlySerializedAs("targetLowHpPenalty")]
    [SerializeField] private float _targetLowHpPenalty = 70f;
    public float TargetLowHpPenalty => _targetLowHpPenalty;

    [Tooltip("Score multiplier when the target is already boosted. Near zero — re-buffing " +
             "wastes the action since the effect is already active.")]
    [UnityEngine.Serialization.FormerlySerializedAs("alreadyBoostedMultiplier")]
    [SerializeField] [Range(0f, 1f)] private float _alreadyBoostedMultiplier = 0.05f;
    public float AlreadyBoostedMultiplier => _alreadyBoostedMultiplier;

    [Tooltip("HP% below which the caster is too endangered to spend an action buffing.")]
    [UnityEngine.Serialization.FormerlySerializedAs("casterLowHpThreshold")]
    [SerializeField] [Range(0f, 1f)] private float _casterLowHpThreshold = 0.3f;
    public float CasterLowHpThreshold => _casterLowHpThreshold;

    [UnityEngine.Serialization.FormerlySerializedAs("casterLowHpPenalty")]
    [SerializeField] private float _casterLowHpPenalty = 80f;
    public float CasterLowHpPenalty => _casterLowHpPenalty;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        // Caster is too low HP — this action isn't worth the tempo loss
        float casterHpPercent = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth;
        if (casterHpPercent < CasterLowHpThreshold) return scoreData;

        Unit bestTarget = FindBestTarget(aiUnit);
        if (bestTarget == null) return scoreData;

        int aliveEnemies = TurnManager.Instance.GetAlivePlayerUnits().Count;
        if (aliveEnemies == 0) return scoreData;

        float score = BaseScore + aliveEnemies * PerEnemyMultiplier;

        if (bestTarget.HasStatus(EffectStatusType.Boosted))
        {
            score *= AlreadyBoostedMultiplier;

            Unit unboostedTarget = FindBestTarget(aiUnit, excludeBoosted: true);
            if (unboostedTarget != null)
            {
                bestTarget = unboostedTarget;
                score = BaseScore + aliveEnemies * PerEnemyMultiplier; // reset
            }
        }

        // PENALTY: Target is low HP — will likely die before using the boost
        float targetHpPercent = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;
        if (targetHpPercent < TargetLowHpThreshold) score -= TargetLowHpPenalty;

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