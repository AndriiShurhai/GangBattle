using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Wraps ProvokeAbilitySO for use by AI units. Scores by counting how many
/// player units are within the provoke range — more enemies forced to attack
/// the caster = higher value.
///
/// Score: 0 if no enemies are in range, otherwise baseScore + perEnemyBonus per enemy.
/// This is most powerful on tanky enemy units where drawing fire is intentional.
/// Pair with high Support multiplier in a "Taunt Tank" personality.
/// </summary>
/// 
[CreateAssetMenu(fileName = "ProvokeAbilityActionSO", menuName = "AI/Actions/Provoke Ability Action")]
public class ProvokeAbilityActionSO : AIActionSO
{
    public override AIActionCategory Category => AIActionCategory.Utility;

    [SerializeField] private AbilityBaseSO provokeAbility;

    [Tooltip("Base score when at least one enemy is in range.")]
    public float baseScore = 70f;

    [Tooltip("Additional score per enemy that will be provoked.")]
    public float perEnemyBonus = 30f;

    [Tooltip("Only consider provoking when HP is above this threshold. " +
             "A low-HP unit shouldn't be drawing attention to itself.")]
    [Range(0f, 1f)] public float minimumHealthPercent = 0.5f;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        float healthPercent = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth;
        if (healthPercent < minimumHealthPercent) return scoreData;

        List<Vector3Int> tilesInRange = provokeAbility.GetTilesInRange(aiUnit.GridPosition);

        int unprovokedEnemiesInRange = tilesInRange.Count(t =>
            GridObjectRegistry.Instance.GetObjectAt(t) is Unit u &&
            u.UnitFaction != aiUnit.UnitFaction && u.ForcedUnitGridPosition == null);

        if (unprovokedEnemiesInRange == 0) return scoreData;

        scoreData.score = baseScore + unprovokedEnemiesInRange * perEnemyBonus;
        // Provoke radiates from the caster's position, so the "target" is the caster itself
        scoreData.target = aiUnit.GridPosition;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        Debug.Log($"{aiUnit.name} uses Provoke!");
        aiUnit.UseAbility(provokeAbility, aiUnit.GridPosition);
        onComplete?.Invoke();
    }
}