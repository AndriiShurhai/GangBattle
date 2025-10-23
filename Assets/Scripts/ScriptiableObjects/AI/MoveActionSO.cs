using UnityEngine;

[CreateAssetMenu(fileName = "Move Action SO", menuName = "AI/Actions/Move Action")]
public class MoveActionSO : AIActionSO
{
    public override float GetScoreAction(Unit aiUnit)
    {
        return 25f;
    }
    public override void Execute(Unit aiUnit)
    {

        Debug.Log("Moving action execute is called");
        Unit targetUnit = FindNearesPlayerUnit(aiUnit);

        if (targetUnit != null)
        {
            Debug.Log("moving");
        }
    }


    private Unit FindNearesPlayerUnit(Unit aiUnit)
    {
        return null;
    }

}
