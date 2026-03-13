using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Wraps TeleportAbilitySO for AI use. Evaluates every valid teleport destination
/// and scores based on how much it improves the AI's tactical position:
///
///   FLANK mode  (default):  favour tiles adjacent to the weakest enemy
///   ESCAPE mode (low HP):   favour tiles far from all enemies, like a ranged flee
///
/// Score: 0 if teleporting doesn't improve position,
///        baseScore + distanceGain * gainMultiplier when it does.
/// </summary>
/// 

[CreateAssetMenu(fileName = "TeleportAbilityActionSO", menuName = "AI/Actions/Teleport Ability Action")]
public class TeleportAbilityActionSO : AbilityActionBase
{
    public override AIActionCategory Category => AIActionCategory.Teleport;
    protected override AbilityBaseSO GetAbility() => teleportAbility;

    [SerializeField] private AbilityBaseSO teleportAbility;

    [Tooltip("Base score when a beneficial teleport destination is found.")]
    [UnityEngine.Serialization.FormerlySerializedAs("baseScore")]
    [SerializeField] private float _baseScore = 60f;
    public float BaseScore => _baseScore;

    [Tooltip("Multiplied by the distance improvement to produce the full score.")]
    [UnityEngine.Serialization.FormerlySerializedAs("gainMultiplier")]
    [SerializeField] private float _gainMultiplier = 12f;
    public float GainMultiplier => _gainMultiplier;

    [Tooltip("HP percentage below which the unit teleports away from enemies (Escape) " +
             "instead of toward them (Flank).")]
    [UnityEngine.Serialization.FormerlySerializedAs("escapeHealthThreshold")]
    [SerializeField] [Range(0f, 1f)] private float _escapeHealthThreshold = 0.3f;
    public float EscapeHealthThreshold => _escapeHealthThreshold;

    public override AIScoreData GetScoreAction(Unit aiUnit)
    {
        AIScoreData scoreData = new AIScoreData();

        List<Unit> enemies = TurnManager.Instance.GetAlivePlayerUnits();
        if (enemies.Count == 0) return scoreData;

        List<Vector3Int> tilesInRange = teleportAbility.GetTilesInRange(aiUnit.GridPosition);

        bool escapeMode = (float)aiUnit.CurrentHealth / aiUnit.MaxHealth < EscapeHealthThreshold;

        Vector3Int bestTile = default;
        float bestGain = 0f;

        foreach (Vector3Int tile in tilesInRange)
        {
            if (!teleportAbility.IsValidTarget(aiUnit.GridPosition, tile, aiUnit)) continue;

            float gain = escapeMode
                ? EvaluateEscapeTile(tile, aiUnit, enemies)
                : EvaluateFlankTile(tile, aiUnit, enemies);

            if (gain > bestGain)
            {
                bestGain = gain;
                bestTile = tile;
            }
        }

        if (bestGain <= 0f) return scoreData; // Teleporting doesn't help

        scoreData.score = BaseScore + bestGain * GainMultiplier;
        scoreData.target = bestTile;
        return scoreData;
    }

    public override void Execute(Unit aiUnit, object target, Action onComplete)
    {
        if (target is Vector3Int tile)
        {
            Debug.Log($"{aiUnit.name} teleports to {tile}");
            aiUnit.UseAbility(teleportAbility, tile);
        }
        onComplete?.Invoke();
    }

    private float EvaluateFlankTile(Vector3Int tile, Unit aiUnit, List<Unit> enemies)
    {
        Unit weakest = enemies.OrderBy(e => e.CurrentHealth).First();
        float currentDist = Vector3.Distance(aiUnit.GridPosition, weakest.GridPosition);
        float newDist = Vector3.Distance(tile, weakest.GridPosition);
        return currentDist - newDist; // positive means we'd be getting closer
    }

    private float EvaluateEscapeTile(Vector3Int tile, Unit aiUnit, List<Unit> enemies)
    {
        float currentTotalDist = enemies.Sum(e => Vector3.Distance(aiUnit.GridPosition, e.GridPosition));
        float newTotalDist = enemies.Sum(e => Vector3.Distance(tile, e.GridPosition));
        return newTotalDist - currentTotalDist; // positive means we'd be further away
    }
}