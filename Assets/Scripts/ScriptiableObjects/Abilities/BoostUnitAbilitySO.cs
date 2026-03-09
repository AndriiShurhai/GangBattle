using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Abilities/Boost Unit Ability")]
public class BoostUnitAbilitySO : AbilityBaseSO
{
    public int duration = 2;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        IGridObject gridObj = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (gridObj is Unit unit)
        {
            int strengthBosst = GetPower(unit);
            int intelligenceBoost = GetPower(unit);
            int agilityBoost = GetPower(unit);

            unit.BoostUnit(strengthBosst, intelligenceBoost, agilityBoost); 
            unit.ApplyEffect(EffectStatusType.Boosted, duration);
            onAbilityInvoke?.Invoke();
        }
    }
}
