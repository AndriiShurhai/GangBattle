using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [SerializeField] private List<AIActionSO> aiActions;

    private Unit aiUnit;

    private void Awake()
    {
        aiUnit = GetComponent<Unit>();   
    }

    public void TakeTurn(Action onComplete)
    {
        if (aiActions == null || aiActions.Count == 0) return;

        List<AIScoreData> allScoreData = new List<AIScoreData>();   
        foreach(AIActionSO action in aiActions)
        {
            AIScoreData scoreData = action.GetScoreAction(aiUnit);
            scoreData.action = action;
            allScoreData.Add(scoreData);
        }

        AIScoreData bestScoreData = allScoreData
            .OrderByDescending(data => data.score)
            .FirstOrDefault();

        if (bestScoreData.action != null && bestScoreData.score >= 0)
        {
            bestScoreData.action.Execute(aiUnit, bestScoreData.target, onComplete);
        }

        else
        {
            onComplete?.Invoke();
            Debug.Log($"{aiUnit.name} has no valid actions to take");
        }
    }
}
