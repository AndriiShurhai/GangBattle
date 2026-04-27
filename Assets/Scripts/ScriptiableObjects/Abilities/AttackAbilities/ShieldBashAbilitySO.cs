using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shield Bash Ability")]
public class ShieldBashSO : AbilityBaseSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("stunDuration")]
    [SerializeField] private int _stunDuration = 2;
    public int StunDuration => _stunDuration;

    [UnityEngine.Serialization.FormerlySerializedAs("stunChance")]
    [SerializeField] private float _stunChance = 0.4f;
    public float StunChance => _stunChance;

    [UnityEngine.Serialization.FormerlySerializedAs("jumpHeight")]
    [SerializeField] private float _jumpHeight = 0.5f;
    public float JumpHeight => _jumpHeight;

    [UnityEngine.Serialization.FormerlySerializedAs("jumpDuration")]
    [SerializeField] private float _jumpDuration = 0.3f;
    public float JumpDuration => _jumpDuration;
    public override void Execute(Unit caster, Vector3Int position, Action onComplete = null)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(position);

        if (targetObject is Unit targetUnit)
        {
            int damage = GetPower(caster);

            caster.transform.DOJump(GridManager.Instance.GridToWorld(position), JumpHeight, 1, JumpDuration).OnComplete(() =>
            {
                targetUnit.TakeDamage(damage, caster);
                onComplete?.Invoke();


                if (IsAttackStunning())
                {

                    targetUnit.ApplyEffect(EffectStatusType.Stunned, StunDuration, null, AbilityEffectPrefab);
                }
                caster.transform.DOJump(GridManager.Instance.GridToWorld(caster.GridPosition), JumpHeight, 1, JumpDuration);
            });
        }
    }

    private bool IsAttackStunning()
    {
        float chance = UnityEngine.Random.value;

        return chance < StunChance;
    }

    public override List<AbilityUIStat> GetDetailedStats(Unit caster)
    {
        return new List<AbilityUIStat>
        {
            new AbilityUIStat { Label = "Damage", Value = GetPower(caster).ToString() },
            new AbilityUIStat { Label = "Stun Chance", Value = $"{Mathf.RoundToInt(StunChance * 100)}%" },
            new AbilityUIStat { Label = "Stun Duration", Value = $"{StunDuration} Turns" }
        };
    }
}
