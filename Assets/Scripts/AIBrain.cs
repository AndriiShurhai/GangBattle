using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [SerializeField] private List<AIActionSO> aiActions;

    [Tooltip("Optional. Defines this enemy's archetype via per-category score multipliers.")]
    [SerializeField] private AIPersonalitySO personality;

    [SerializeField] private float minimumScoreToAct = 10f;

    [SerializeField] private int maxActionsPerTurn = 5;

    [SerializeField] private float actionDelay = 1f;

    public AIPersonalitySO Personality => personality;

    private Unit aiUnit;

    private void Awake()
    {
        aiUnit = GetComponent<Unit>();   
    }


    public void TakeTurn(Action onComplete)
    {
        if (aiActions == null || aiActions.Count == 0) 
        {
            Debug.LogWarning($"{aiUnit.name} has no AI actions assigned.");
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(aiUnit.HighlightUnit(true));
        StartCoroutine(ExecuteNextAction(onComplete, 0));
    }
    private IEnumerator ExecuteNextAction(Action onComplete, int actionsTaken)
    {
        yield return new WaitForSeconds(actionDelay);

        if (actionsTaken >= maxActionsPerTurn)
        {
            Debug.Log($"{aiUnit.name} reached max actions per turn ({maxActionsPerTurn})");
            onComplete?.Invoke();
            yield break;
        }

        AIScoreData bestScoreData = ScoreBestAction();

        if (bestScoreData.action != null && bestScoreData.score >= minimumScoreToAct && gameObject.activeSelf)
        {
            Debug.Log($"{aiUnit.name} executes {bestScoreData.action.name} with score {bestScoreData.score}");
            try
            {
                bestScoreData.action.Execute(aiUnit, bestScoreData.target, () =>
                {
                    if (gameObject != null && gameObject.activeSelf)
                        StartCoroutine(ExecuteNextAction(onComplete, actionsTaken + 1));
                    else
                        onComplete?.Invoke(); // Let the turn system clean up normally
                });
            }
            catch (Exception e)
            {
                StartCoroutine(aiUnit.HighlightUnit(false));
                onComplete?.Invoke();
            }
        }
        else if (bestScoreData.action == null && bestScoreData.score >= minimumScoreToAct)
        {
            Debug.Log($"{aiUnit.name} has no validActions to take");
            StartCoroutine(aiUnit.HighlightUnit(false));
            onComplete?.Invoke();
        }
        else if (bestScoreData.action != null && bestScoreData.score < minimumScoreToAct)
        {
            Debug.Log($"{aiUnit.name} has all actions score below the threshold");
            StartCoroutine(aiUnit.HighlightUnit(false));
            onComplete?.Invoke();
        }
        else
        {
            // Covers cases where no actions are scoreable or all are below the threshold,
            // and ScoreBestAction returned a default/empty AIScoreData.
            Debug.Log($"{aiUnit.name} cannot act this turn (no executable actions or scores below threshold).");
            StartCoroutine(aiUnit.HighlightUnit(false));
            onComplete?.Invoke();
        }
    }

    private AIScoreData ScoreBestAction()
    {
        List<AIScoreData> allScoreData = new List<AIScoreData>();
        foreach (AIActionSO action in aiActions)
        {
            if (!action.CanExecute(aiUnit)) continue;

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
        return allScoreData
            .OrderByDescending(data => data.score)
            .FirstOrDefault();
    }

    private AIScoreData ApplyProvokeModifier(AIScoreData scoreData)
    {
        if (aiUnit.ForcedUnitGridPosition == null) return scoreData;


        Unit target = null;

        if (scoreData.target is Unit)
        {
            target = (Unit)scoreData.target;
        }

        switch (scoreData.action.Category)
        {
            case AIActionCategory.Attack:
                if (target != null && target.GridPosition.Equals(aiUnit.ForcedUnitGridPosition))
                {
                    scoreData.score *= 2f; // Double the score for attacks against the provoking unit
                }
                else
                {
                    scoreData.score *= 0.1f; // Halve the score for attacks against other unit
                }
                break;

            case AIActionCategory.Flee:
                scoreData.score *= 1.2f;
                break;

            case AIActionCategory.AoE:
                if (target != null && target.GridPosition.Equals(aiUnit.ForcedUnitGridPosition))
                {
                    scoreData.score *= 2f; // Double the score for attacks against the provoking unit
                }
                else
                {
                    scoreData.score *= 0.1f; // Halve the score for attacks against other unit
                }
                break;

            case AIActionCategory.Move:
                scoreData.score *= 1.5f;
                break;

            default:
                scoreData.score *= 0.1f;
                break;
        }
        return scoreData;
    }
}
