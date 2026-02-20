using Assets.Scripts.UI.Abilities_Visual_Effects;
using DG.Tweening;
using System;
using System.Collections;
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

            GameObject fireVisual = Instantiate(abilityEffectPrefab, GridManager.Instance.GridToWorld(targetPosition), Quaternion.identity);


            fireVisual.transform.localScale = Vector3.zero;
            fireVisual.transform.DOScale(Vector3.one, 0.5f);
            int damage = GetPower(caster);

            targetUnit.TakeDamage(damage, caster);

            targetUnit.ApplyEffect(EffectStatusType.Burned, duration, () =>
            {
                targetUnit.TakeDamage(damage / 2, caster);
            });

            fireVisual.GetComponent<FireBurnVisualEffect>().ExtinctFire();
        }
    }
}

