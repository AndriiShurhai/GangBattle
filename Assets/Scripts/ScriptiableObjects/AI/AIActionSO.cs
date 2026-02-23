using System;
using UnityEngine;

public enum AIActionCategory
{
    Attack,
    Move,
    Flee,
    Support,
    AoE,
    Teleport,
    GroupUp,
    Utility
}
public struct AIScoreData
{
    public float score;
    public object target;
    public AIActionSO action;
}
public abstract class AIActionSO : ScriptableObject
{
    public abstract AIActionCategory Category { get; }
    public abstract AIScoreData GetScoreAction(Unit aiUnit);

    public virtual bool CanExecute(Unit aiUnit)
    {
        return true;
    }

    public abstract void Execute(Unit aiUnit, object target, Action onComplete);  
}
