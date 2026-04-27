using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName ="Abilities/Burn Attack")]
public class BurnAttackAbilitySO : AbilityBaseSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("duration")]
    [SerializeField] private int _duration = 3;
    public int Duration => _duration;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {

            GameObject fireVisual = Instantiate(AbilityEffectPrefab, GridManager.Instance.GridToWorld(targetPosition), Quaternion.identity);


            fireVisual.transform.localScale = Vector3.zero;
            fireVisual.transform.DOScale(Vector3.one, 0.5f);
            int damage = GetPower(caster);

            targetUnit.TakeDamage(damage, caster);

            targetUnit.ApplyEffect(EffectStatusType.Burned, Duration, () =>
            {
                targetUnit.TakeDamage(damage / 2, caster);
            });

            onAbilityInvoke?.Invoke();
            fireVisual.GetComponent<FireBurnVisualEffect>().ExtinctFire();
        }
    }

    public override List<AbilityUIStat> GetDetailedStats(Unit caster)
    {
        return new List<AbilityUIStat>
        {
            new AbilityUIStat { Label = "Initial Damage", Value = GetPower(caster).ToString() },
            new AbilityUIStat { Label = "Burn Damage", Value = $"{GetPower(caster) / 2} / Turn" },
            new AbilityUIStat { Label = "Burn Duration", Value = $"{Duration} Turns" }
        };
    }
}

