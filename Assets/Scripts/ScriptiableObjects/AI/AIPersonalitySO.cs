using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Defines an enemy archetype by applying score multipliers to each action category.
/// Assign different personalities to different enemy units in the Inspector to create
/// varied behavior without writing any new code.
///
/// Example archetypes to configure:
///   Aggressive  — Attack x2.0,  Flee x0.1,  Support x0.5
///   Cowardly    — Flee  x2.0,   Attack x0.7, GroupUp x1.5
///   Support     — Support x2.5, AoE x1.5,    Attack x0.6
///   Tactical    — AoE x2.0,     Teleport x1.8, Attack x0.8
/// </summary>
/// 


[CreateAssetMenu(fileName = "AIPersonalitySO", menuName = "AIPersonalitySO")]
public class AIPersonalitySO: ScriptableObject
{
    [Serializable]
    public struct CategoryWeight
    {
        public AIActionCategory category;
        [Range(0f, 5f)] public float multiplier;
    }

    public List<CategoryWeight> categoryWeights = new List<CategoryWeight>();

    [Range(0f, 1f)] public float fleeHealthTreshold = 0.35f;

    [TextArea(2, 4)] public string archetypeDescription;

    public float GetCategoryWeight(AIActionCategory category)
    {
        foreach (var categoryWeight in categoryWeights)
        {
            if (categoryWeight.category == category)
            {
                return categoryWeight.multiplier;
            }
        }
        return 1f; // Default multiplier if category not found
    }

}
