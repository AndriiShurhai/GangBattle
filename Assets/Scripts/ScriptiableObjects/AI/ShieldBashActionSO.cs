using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Context-aware scoring for Shield Bash — an attack whose real value is the
/// stun chance, not the raw damage. Scoring asks "is stunning THIS target right NOW
/// actually worth it?" rather than just "is there someone in range?"
///
/// Scoring logic (cumulative bonuses on top of baseScore):
///
///   Base:             flat reward for having any valid target
///   High HP target:   stun is most valuable against targets that would survive a
///                     normal attack, since it prevents them acting next turn
///   Outnumbered:      shutting down an attacker is worth more when the AI team is
///                     taking more hits per round
///   Already stunned:  penalise heavily — wasting a stun on a stunned target is pure loss
///   Low own HP:       penalise — better to flee or attack for kill rather than stun
///
/// Total score range: ~0 to ~260, keeping it competitive with AttackActionSO (100-200)
/// but only pulling ahead when stunning is actually the smart play.
/// </summary>
/// 

[CreateAssetMenu(fileName = "ShieldBashActionSO", menuName = "AI/Actions/Shield Bash Action")]
public class ShieldBashActionSO : AbilityActionBase
{
    public override AIActionCategory Category => AIActionCategory.Attack;
    protected override AbilityBaseSO GetAbility() => shieldBashAbility;

    [SerializeField] private AbilityBaseSO shieldBashAbility;

    [Header("Base Scoring")]
    [Tooltip("Starting score when a valid target exists. Should be lower than AttackActionSO's " +
             "100 baseline so stun is a conditional upgrade, not a default preference.")]
    public float baseScore = 80f;

    [Header("Situational Bonuses")]
    [Tooltip("Bonus when the target is above this HP% — stun is wasted on someone about to die.")]
    [Range(0f, 1f)] public float highHpThreshold = 0.6f;
    public float highHpBonus = 60f;

    [Tooltip("Bonus per enemy unit that outnumbers the AI's team. " +
             "More enemies alive = each stun is more valuable.")]
    public float outnumberedBonusPerUnit = 25f;

    [Header("Penalties")]
    [Tooltip("Score multiplier applied when the target is already stunned. " +
             "Near zero to make it essentially never chosen.")]
    [Range(0f, 1f)] public float alreadyStunnedMultiplier = 0.05f;

    [Tooltip("HP% below which the unit penalises using bash (should flee or kill instead).")]
    [Range(0f, 1f)] public float lowOwnHpThreshold = 0.25f;
    public float lowOwnHpPenalty = 50f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        Unit bestTarget = FindBestTarget(aiUnit);
        if (bestTarget == null) return scoreData;

        float score = baseScore;

        // BONUS: Target is high HP — still a threat next turn, stun is meaningful
        float targetHpPercent = (float)bestTarget.CurrentHealth / bestTarget.MaxHealth;
        if (targetHpPercent >= highHpThreshold)
            score += highHpBonus;

        // BONUS: AI team is outnumbered — each disabled enemy counts more
        int aliveEnemies = TurnManager.Instance.GetAlivePlayerUnits().Count;
        int aliveAllies = TurnManager.Instance.GetAliveEnemyUnits().Count;
        int deficit = aliveEnemies - aliveAllies;
        if (deficit > 0)
            score += deficit * outnumberedBonusPerUnit;

        // PENALTY: Target is already stunned — pick something else
        if (bestTarget.HasStatus(EffectStatusType.Stunned))
            score *= alreadyStunnedMultiplier;

        // PENALTY: AI unit is critically low — stun doesn't save it, aggression or flee does
        float ownHpPercent = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth;
        if (ownHpPercent < lowOwnHpThreshold)
            score -= lowOwnHpPenalty;

        if (score <= 0f) return scoreData;

        scoreData.score = score;
        scoreData.target = bestTarget;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Unit targetUnit = target as Unit;
        if (targetUnit == null) { onComplete?.Invoke(); return; }

        Debug.Log($"{aiUnit.name} uses Shield Bash on {targetUnit.name}");
        aiUnit.UseAbility(shieldBashAbility, targetUnit.GridPosition);
        onComplete?.Invoke();
    }

    private Unit FindBestTarget(Unit aiUnit)
    {
        List<Vector3Int> tilesInRange = shieldBashAbility.GetTilesInRange(aiUnit.GridPosition);

        return tilesInRange
            .Select(t => GridObjectRegistry.Instance.GetObjectAt(t) as Unit)
            .Where(u => u != null &&
                        u.UnitFaction == Faction.Player &&
                        shieldBashAbility.IsValidTarget(aiUnit.GridPosition, u.GridPosition, aiUnit))
            .OrderByDescending(u => u.CurrentHealth)
            .FirstOrDefault();
    }
}