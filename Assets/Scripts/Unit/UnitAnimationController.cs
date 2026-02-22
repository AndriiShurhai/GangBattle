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
        Unit.OnAnyUnitGainedStatusEffect += Unit_OnAnyUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect += Unit_OnAnyUnitLostStatusEffect;
    }

    private void OnDestroy()
    {
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
        Unit.OnAnyUnitTookDamage -= Unit_OnAnyUnitTookDamage;
        Unit.OnAnyUnitStartMoving -= Unit_OnAnyUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving -= Unit_OnAnyUnitFinishedMoving;
        Unit.OnAnyUnitUsedAbility -= Unit_OnAnyUnitUsedAbility;
        Unit.OnAnyUnitGainedStatusEffect -= Unit_OnAnyUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect -= Unit_OnAnyUnitLostStatusEffect;
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

    private void Unit_OnAnyUnitLostStatusEffect(Unit unit, EffectStatusType effectType)
    {
        if (!unit.HasStatus(EffectStatusType.Stunned) && !unit.HasStatus(EffectStatusType.Rooted))
        {
            unit.GetUnitVisualBridge().StopDebuffAnimation();
        }
    }

    private void Unit_OnAnyUnitGainedStatusEffect(Unit unit, EffectStatusType effectType)
    {
        switch (effectType)
        {
            case EffectStatusType.Stunned:
                unit.GetUnitVisualBridge().StartDebuffAnimation();
                break;

            case EffectStatusType.Rooted:
                unit.GetUnitVisualBridge().StartDebuffAnimation();
                break;
        }
    }

    private void Unit_OnAnyUnitDied(Unit unit)
    {
        //unit.GetUnitVisualBridge().DeathAnimation();
    }
}
