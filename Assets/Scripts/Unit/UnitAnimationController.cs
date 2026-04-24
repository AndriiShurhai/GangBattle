using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    private void Awake()
    {
        Unit.OnAnyUnitDied += OnUnitDied;
        Unit.OnAnyUnitTookDamage += OnUnitTookDamage;
        Unit.OnAnyUnitStartMoving += OnUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving += OnUnitFinishedMoving;
        Unit.OnAnyUnitUsedAbility += OnUnitUsedAbility;
        Unit.OnAnyUnitGainedStatusEffect += OnUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect += OnUnitLostStatusEffect;
    }

    private void OnDestroy()
    {
        Unit.OnAnyUnitDied -= OnUnitDied;
        Unit.OnAnyUnitTookDamage -= OnUnitTookDamage;
        Unit.OnAnyUnitStartMoving -= OnUnitStartMoving;
        Unit.OnAnyUnitFinishedMoving -= OnUnitFinishedMoving;
        Unit.OnAnyUnitUsedAbility -= OnUnitUsedAbility;
        Unit.OnAnyUnitGainedStatusEffect -= OnUnitGainedStatusEffect;
        Unit.OnAnyUnitLostStatusEffect -= OnUnitLostStatusEffect;
    }

    private void OnUnitUsedAbility(Unit unit, AbilityBaseSO ability)
    {
        if (ability is AttackAbilitySO)
            unit.GetUnitVisualBridge().AttackAnimation();
    }

    private void OnUnitStartMoving(Unit unit, Vector3 destination)
        => unit.GetUnitVisualBridge().StartRunningAnimation(destination);

    private void OnUnitFinishedMoving(Unit unit)
        => unit.GetUnitVisualBridge().StopRunningAnimation();

    private void OnUnitTookDamage(Unit unit, int amount, int currentHealth)
        => unit.GetUnitVisualBridge().TakeDamageAnimation();

    private void OnUnitDied(Unit unit)
        => unit.GetUnitVisualBridge().DeathAnimation();

    private void OnUnitGainedStatusEffect(Unit unit, EffectStatusType effectType)
    {
        if (effectType == EffectStatusType.Stunned || effectType == EffectStatusType.Rooted)
            unit.GetUnitVisualBridge().StartDebuffAnimation();
    }

    private void OnUnitLostStatusEffect(Unit unit, EffectStatusType effectType)
    {
        // Only stop debuff animation if no other debuffing effects remain
        if (!unit.HasStatus(EffectStatusType.Stunned) && !unit.HasStatus(EffectStatusType.Rooted))
            unit.GetUnitVisualBridge().StopDebuffAnimation();
    }
}