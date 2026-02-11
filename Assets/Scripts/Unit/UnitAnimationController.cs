using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    private void Start()
    {
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        Unit.OnAnyUnitTookDamage += Unit_OnAnyUnitTookDamage;
        Unit.OnAnyUnitStartMoving += Unit_OnAnyUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving += Unit_OnAnyUnitFinishedMoving;
        Unit.OnAnyUnitUsedAbility += Unit_OnAnyUnitUsedAbility;
    }

    private void OnDestroy()
    {
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
        Unit.OnAnyUnitTookDamage -= Unit_OnAnyUnitTookDamage;
        Unit.OnAnyUnitStartMoving -= Unit_OnAnyUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving -= Unit_OnAnyUnitFinishedMoving;
        Unit.OnAnyUnitUsedAbility -= Unit_OnAnyUnitUsedAbility;
    }
    private void Unit_OnAnyUnitUsedAbility(Unit unit, AbilityBaseSO ability)
    {
        Debug.Log("UNIT USAGE ABILITY HAS BEEN CALLED");
        if (ability is AttackAbilitySO attackSO)
        {
            Debug.Log("UNIT ATTACK ABILITY HAS BEEN CALLED");
            unit.GetUnitVisualBridge().AttackAnimation();
        }
    }

    private void Unit_OnAnyUnitFinishedMoving(Unit unit)
    {
        unit.GetUnitVisualBridge().StopRunningAnimation();
    }

    private void Unit_OnAnyUnitStartMoving(Unit unit, Vector3 destination)
    {
        unit.GetUnitVisualBridge().StartRunningAnimation(destination);
    }

    private void Unit_OnAnyUnitTookDamage(Unit unit, int arg2, int arg3)
    {
        unit.GetUnitVisualBridge().TakeDamageAnimation();
    }

    private void Unit_OnAnyUnitDied(Unit unit)
    {
        //unit.GetUnitVisualBridge().DeathAnimation();
    }
}
