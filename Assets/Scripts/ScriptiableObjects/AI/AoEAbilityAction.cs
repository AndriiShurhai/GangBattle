using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Wraps any AoE ability (Fireball, etc.) and scores the action
/// by scanning every valid target tile and finding the position that hits the
/// most enemies simultaneously.
///
/// Score: 0 if fewer than minTargetsToUse are hittable, otherwise baseScore + perTargetBonus per enemy.
/// Example: hits 3 enemies → 80 + 3×50 = 230, which beats a single-target attack (100-200).
/// 
/// Works with any ability that implements GetAbilityRadiusTiles() — the default
/// implementation returns only the target tile, so splash abilities must override it
/// (FireballAttackAbilitySO already does this correctly).
/// </summary>
/// 
[CreateAssetMenu(fileName = "AoEAbilityActionSO", menuName = "AI/Actions/AoE Ability Action")]
public class AoEAbilityActionSO : AbilityActionBase
{
    public override AIActionCategory Category => AIActionCategory.AoE;
    protected override AbilityBaseSO GetAbility() => aoeAbility;

    [SerializeField] private AbilityBaseSO aoeAbility;

    [Tooltip("Base score when the minimum number of targets are hit.")]
    public float baseScore = 80f;

    [Tooltip("Additional score per enemy caught in the blast.")]
    public float perTargetBonus = 50f;

    [Tooltip("Minimum enemies that must be hittable for the ability to be considered at all.")]
    public int minTargetsToUse = 2;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        List<Vector3Int> tilesInRange = aoeAbility.GetTilesInRange(aiUnit.GridPosition);

        Vector3Int bestTile = default;
        int bestEnemyCount = 0;

        foreach (Vector3Int tile in tilesInRange)
        {
            if (!aoeAbility.IsValidTarget(aiUnit.GridPosition, tile, aiUnit)) continue;

            // Count how many player units fall inside the blast radius at this tile
            List<Vector3Int> blastArea = aoeAbility.GetAbilityRadiusTiles(tile);
            int enemyCount = blastArea.Count(t =>
                GridObjectRegistry.Instance.GetObjectAt(t) is Unit u &&
                u.UnitFaction == Faction.Player);

            if (enemyCount > bestEnemyCount)
            {
                bestEnemyCount = enemyCount;
                bestTile = tile;
            }
        }

        if (bestEnemyCount < minTargetsToUse) return scoreData;

        scoreData.score = baseScore + bestEnemyCount * perTargetBonus;
        scoreData.target = bestTile;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        if (target is Vector3Int tile)
        {
            Debug.Log($"{aiUnit.name} fires AoE ability at {tile}");
            aiUnit.UseAbility(aoeAbility, tile);
        }
        onComplete?.Invoke();
    }
}