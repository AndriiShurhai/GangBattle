using UnityEngine;

[CreateAssetMenu(fileName = "AttackActionSO", menuName = "AI/Actions/Attack Action")]
public class AttackActionSO : AIActionSO
{
    [SerializeField] private AbilityBaseSO attackAbility;

    public override float GetScoreAction(Unit aiUnit)
    {
        Unit target = FindNearestPlayerUnit(aiUnit);

        if (target == null) return 0f;

        if (attackAbility.IsValidTarget(aiUnit.GridPosition, target.GridPosition, aiUnit))
        {
            return 100 - (target.CurrentHealth / target.MaxHealth) * 100f;
        }

        return 0f;
    }

    public override void Execute(Unit aiUnit)
    {
        Unit targetUnit = FindNearestPlayerUnit(aiUnit);

        if (targetUnit != null)
        {
            attackAbility.Execute(aiUnit, targetUnit.GridPosition);
        }
    }

    private Unit FindNearestPlayerUnit(Unit aiUnit)
    {
        return null;
    }
}
