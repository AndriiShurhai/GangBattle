using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [SerializeField] private List<AIActionSO> aiActions;
    [SerializeField] private List<AIActionSO> attackAIActions;

    [SerializeField] private AIPersonalitySO personality;

    private Unit aiUnit;

    private void Awake()
    {
        aiUnit = GetComponent<Unit>();   
    }

    public void TakeTurn(Action onComplete)
    {
        if (aiActions == null || aiActions.Count == 0) return;

        List<AIScoreData> allScoreData = new List<AIScoreData>();   

        foreach (AIActionSO action in aiActions)
        {
            AIScoreData scoreData = action.GetScoreAction(aiUnit);
            scoreData.action = action;

            if (personality != null)
            {
                scoreData.score *= personality.GetCategoryWeight(action.Category);
            }

            if (aiUnit.HasStatus(EffectStatusType.Provoked))
            {
                scoreData = ApplyProvokeModifier(scoreData);    
            }

            allScoreData.Add(scoreData);
        }

        AIScoreData bestScoreData = allScoreData
            .OrderByDescending(data => data.score)
            .FirstOrDefault();

        if (bestScoreData.action != null && bestScoreData.score >= 0)
        {
            bestScoreData.action.Execute(aiUnit, bestScoreData.target, onComplete);
            Debug.Log($"Action is executed yeaaaah, {bestScoreData.action.name}");
        }

        else
        {
            onComplete?.Invoke();
            Debug.Log($"{aiUnit.name} has no valid actions to take");
        }
    }

    private AIScoreData ApplyProvokeModifier(AIScoreData scoreData)
    {
        if (aiUnit.ForcedUnitGridPosition == null) return scoreData;

        switch(scoreData.action.Category)
        {
            case AIActionCategory.Attack:
                if (scoreData.target is Unit targetUnit &&
                    targetUnit.GridPosition == aiUnit.ForcedUnitGridPosition)
                {
                    scoreData.score *= 2f; // Example: Double the score for attack actions against the provoking unit  
                }
                else
                {
                    scoreData.score *= 0.1f; // Example: Halve the score for actions that do not target the provoking unit
                }
                break;

            case AIActionCategory.Move:
                break;

            default:
                scoreData.score *= 0.5f; // Example: Reduce the score for all other actions
                break;
        }
       
        return scoreData;
    }
}
