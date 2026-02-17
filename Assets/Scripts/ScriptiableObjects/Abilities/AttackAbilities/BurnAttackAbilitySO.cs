using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName ="Abilities/Burn Attack")]
public class BurnAttackAbilitySO : AbilityBaseSO
{
    public int duration = 3;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            int damage = GetPower(caster);

            targetUnit.TakeDamage(damage, caster);

            targetUnit.ApplyEffect(EffectStatusType.Burned, duration, () =>
            {
                targetUnit.TakeDamage(damage / 2, caster);
            });

        }
    }
}

