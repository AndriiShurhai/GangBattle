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

    public void TakeTurn()
    {
        if (aiActions == null || aiActions.Count == 0) return;

        AIActionSO bestAction = aiActions
            .OrderByDescending(action => action.GetScoreAction(aiUnit))
            .FirstOrDefault();

        if (bestAction != null)
        {
            bestAction.Execute(aiUnit);
        }

        else
        {
            Debug.Log("Best action is equal to null");
        }
    }
}
