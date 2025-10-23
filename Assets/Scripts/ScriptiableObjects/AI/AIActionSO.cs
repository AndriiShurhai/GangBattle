using UnityEngine;

public abstract class AIActionSO : ScriptableObject
{
    public abstract float GetScoreAction(Unit aiUnit);

    public abstract void Execute(Unit aiUnit);  
}
