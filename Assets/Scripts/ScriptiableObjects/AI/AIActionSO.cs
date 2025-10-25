using UnityEngine;


public struct AIScoreData
{
    public float score;
    public object target;
}
public abstract class AIActionSO : ScriptableObject
{
    public abstract AIScoreData GetScoreAction(Unit aiUnit);

    public abstract void Execute(Unit aiUnit, object target);  
}
